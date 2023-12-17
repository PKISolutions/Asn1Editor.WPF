using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Input;
using Asn1Editor.Wpf.Controls.Helpers;
using SysadminsLV.Asn1Editor.API.Abstractions;
using SysadminsLV.Asn1Editor.API.Interfaces;
using SysadminsLV.Asn1Editor.API.ModelObjects;
using SysadminsLV.Asn1Editor.API.Utils;
using SysadminsLV.Asn1Editor.API.Utils.ASN;
using SysadminsLV.Asn1Editor.Properties;
using SysadminsLV.Asn1Parser;
using SysadminsLV.WPF.OfficeTheme.Toolkit.Commands;

namespace SysadminsLV.Asn1Editor.API.ViewModel;

class TagDataEditorVM : ViewModelBase, ITagDataEditorVM {
    readonly IDataSource _data;
    const String masterTag = "123";
    const String masterUnused = "1";
    NodeEditMode mode;
    Boolean? dialogResult;
    String tagDetails;
    AsnViewValue tagValue, oldValue;
    Boolean rbText = true, rbHex, isReadonly, tagIsReadOnly = true;
    Byte unusedBits, tag;
    Double tagTextBoxWidth, unusedTextBoxWidth;

    public TagDataEditorVM(IHasAsnDocumentTabs appTabs) {
        _data = appTabs.SelectedTab.DataSource;
        NodeViewOptions = appTabs.NodeViewOptions;
        OkCommand = new RelayCommand(submitValues);
        CloseCommand = new RelayCommand(close);
        initialize();
    }

    public ICommand OkCommand { get; set; }
    public ICommand CloseCommand { get; set; }
    public Asn1Lite Node { get; private set; }
    public NodeViewOptions NodeViewOptions { get; }

    public String TagDetails {
        get => tagDetails;
        set {
            tagDetails = value;
            OnPropertyChanged(nameof(TagDetails));
        }
    }
    public AsnViewValue TagValue {
        get => tagValue;
        set {
            tagValue = value;
            OnPropertyChanged(nameof(TagValue));
        }
    }
    public Boolean TagIsReadOnly {
        get => tagIsReadOnly;
        set {
            tagIsReadOnly = value;
            OnPropertyChanged(nameof(TagIsReadOnly));
        }
    }
    public Byte Tag {
        get => tag;
        set {
            if (Byte.TryParse(value.ToString(CultureInfo.InvariantCulture), out Byte t)) {
                tag = t;
            }
            if (t != 0) {
                OnPropertyChanged(nameof(TagName));
            }
            OnPropertyChanged(nameof(UnusedBitsVisible));
            OnPropertyChanged(nameof(Tag));
        }
    }
    public String TagName => Asn1Reader.GetTagName(Tag);
    public Byte UnusedBits {
        get => unusedBits;
        set {
            unusedBits = value;
            OnPropertyChanged(nameof(UnusedBits));
        }
    }
    public Boolean UnusedBitsVisible => Tag == (Byte)Asn1Type.BIT_STRING;
    public Boolean IsReadOnly {
        get => isReadonly;
        set {
            isReadonly = value;
            OnPropertyChanged(nameof(IsReadOnly));
            OnPropertyChanged(nameof(IsEditable));
        }
    }
    public Boolean IsEditable => !IsReadOnly;
    public Double TagTextBoxWidth {
        get => tagTextBoxWidth;
        set {
            tagTextBoxWidth = value;
            OnPropertyChanged(nameof(TagTextBoxWidth));
        }
    }
    public Double UnusedTextBoxWidth {
        get => unusedTextBoxWidth;
        set {
            unusedTextBoxWidth = value;
            OnPropertyChanged(nameof(UnusedTextBoxWidth));
        }
    }
    public Boolean RbText {
        get => rbText;
        set {
            if (rbText == value) {
                return;
            }
            rbText = value;
            if (rbText) {
                editText();
            }
            OnPropertyChanged(nameof(RbText));
        }
    }
    public Boolean RbHex {
        get => rbHex;
        set {
            if (rbHex == value) {
                return;
            }
            rbHex = value;
            if (rbHex) {
                editHex();
            }
            OnPropertyChanged(nameof(RbHex));
        }
    }
    public Boolean IsRbTextEnabled => (TagValue.Options & AsnViewValueOptions.SupportsPrintableText) != 0;
    public Boolean? DialogResult {
        get => dialogResult;
        set {
            dialogResult = value;
            OnPropertyChanged(nameof(DialogResult));
        }
    }

    void initialize() {
        tagValue = new AsnViewValue();
        calculateLengths();
        Settings.Default.PropertyChanged += (Sender, Args) => {
                                                if (Args.PropertyName == nameof(Settings.FontSize)) {
                                                    calculateLengths();
                                                }
                                            };
    }
    void calculateLengths() {
        TagTextBoxWidth = TextUtility.MeasureStringWidth(masterTag, Settings.Default.FontSize, false);
        UnusedTextBoxWidth = TextUtility.MeasureStringWidth(masterUnused, Settings.Default.FontSize, false);
    }
    void submitValues(Object obj) {
        if (Tag == 0) {
            Tools.MsgBox("Error", "Invalid tag number");
            return;
        }
        if (mode == NodeEditMode.NewNode) {
            saveNewNode();
            return;
        }
        if (Equals(TagValue, oldValue)) {
            DialogResult = true;
            return;
        }
        saveEditChanges();
    }
    void close(Object o) {
        Node = null;
        DialogResult = true;
    }
    void saveNewNode() {
        var asn = new Asn1Reader(new Byte[] { Tag, 0 });
        Asn1Lite node;
        // if it is constructed or value is not entered, use it as is and ignore possible entered tag value
        if (asn.IsConstructed || String.IsNullOrWhiteSpace(TagValue?.TextValue?.Trim())) {
            node = new Asn1Lite(asn);
        } else {
            // some tag value is entered, attempt to validate
            Byte[] binValue = validateValue();
            if (binValue == null) {
                return;
            }
            asn = new Asn1Reader(binValue);
            node = new Asn1Lite(asn);
        }
        // if it is not root node, update offset, depth and mark it non-root
        if (_data.SelectedNode != null) {
            node.Offset = _data.SelectedNode.Value.Offset + _data.SelectedNode.Value.TagLength;
            node.Depth += _data.SelectedNode.Value.Depth;
        }

        Node = node;
        updateNewNodeBinarySource(asn.GetRawData());
        DialogResult = true;
    }
    void saveEditChanges() {
        Byte[] binValue = validateValue();
        if (binValue == null) {
            return;
        }
        updateBinaryCopy(binValue);
        Node.UnusedBits = UnusedBits;
        var asn = new Asn1Reader(binValue);
        Node.PayloadStartOffset = Node.Offset + asn.TagLength - asn.PayloadLength;
        Node.ExplicitValue = AsnDecoder.GetViewValue(asn);
        Node.OffsetChange = asn.PayloadLength - Node.PayloadLength;
        Node.PayloadLength = asn.PayloadLength;
        _data.FinishBinaryUpdate();
        DialogResult = true;
    }
    Byte[] validateValue() {
        Byte[] binValue = null;
        try {
            // if we are in hex mode, or we are in text mode, but text is hex, go to hex
            if (rbHex || (!rbHex && (TagValue.Options & AsnViewValueOptions.SupportsPrintableText) == 0)) {
                binValue = AsnDecoder.EncodeHex(Tag, TagValue.TextValue, UnusedBits);
            } else {
                binValue = AsnDecoder.EncodeGeneric(Tag, TagValue.TextValue, UnusedBits);
            }
        } catch (Exception e) {
            Tools.MsgBox("Error", e.Message);
        }
        return binValue;
    }
    void updateNewNodeBinarySource(IEnumerable<Byte> binData) {
        _data.RawData.InsertRange(Node.Offset, binData);
    }
    void updateBinaryCopy(IEnumerable<Byte> newBytes) {
        _data.RawData.RemoveRange(Node.Offset, Node.TagLength);
        _data.RawData.InsertRange(Node.Offset, newBytes);
    }
    void setRbHexSilent() {
        rbHex = true;
        OnPropertyChanged(nameof(RbHex));
    }
    void editText() {
        if (Node == null) {
            return;
        }

        TagValue = AsnDecoder.GetEditValue(new Asn1Reader(_data.RawData.Skip(Node.Offset).Take(Node.TagLength).ToArray()));
        if ((TagValue.Options & AsnViewValueOptions.SupportsPrintableText) == 0) {
            setRbHexSilent();
        }
        OnPropertyChanged(nameof(IsRbTextEnabled));
    }
    void editHex() {
        if (Node == null) { return; }
        TagValue.TextValue = HexUtility.BinaryToHex(_data.RawData
            .Skip(Node.PayloadStartOffset)
            .Take(Node.PayloadLength)
            .ToArray());
    }
    void copyValues() {
        Node = _data.SelectedNode.Value;
        Tag = Node.Tag;
        UnusedBits = Node.UnusedBits;
        IsReadOnly = Node.IsContainer || Node.Tag == (Byte)Asn1Type.NULL;
        TagDetails = String.Format(Resources.TagEditorHeaderTemp, Tag, Node.TagName, Node.Offset, Node.PayloadLength, Node.Depth, Node.Path);
    }

    public void SetBinding(NodeEditMode editMode) {
        mode = editMode;
        if (editMode == NodeEditMode.NewNode) {
            Tag = 48;
            TagIsReadOnly = false;
            return;
        }
        copyValues();
        if (IsReadOnly || Node.Tag == (Byte)Asn1Type.NULL) {
            TagValue.TextValue = "Containers and NULL (0x05) tags are not editable";
        } else {
            editText();
        }
        oldValue = TagValue.Clone();
    }
}