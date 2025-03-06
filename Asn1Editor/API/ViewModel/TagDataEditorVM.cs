using System;
using System.Collections.Generic;
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
    readonly IUIMessenger _uiMessenger;
    readonly IDataSource _data;
    const String masterUnused = "1";
    NodeEditMode mode;
    Boolean? dialogResult;
    String tagDetails;
    AsnViewValue tagValue, oldValue;
    Boolean rbText = true, rbHex, isReadonly;
    Byte unusedBits;
    Double unusedTextBoxWidth;


    public TagDataEditorVM(IHasAsnDocumentTabs appTabs, IUIMessenger uiMessenger) {
        _data = appTabs.SelectedTab.DataSource;
        _uiMessenger = uiMessenger;
        NodeViewOptions = appTabs.NodeViewOptions;
        OkCommand = new RelayCommand(submitValues);
        CloseCommand = new RelayCommand(close);
        initialize();
    }

    public ICommand OkCommand { get; set; }
    public ICommand CloseCommand { get; set; }
    public Asn1Lite Node { get; private set; }
    public NodeViewOptions NodeViewOptions { get; }

    /// <summary>
    /// Gets or sets ASN node formatted text information.
    /// </summary>
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
    public Byte UnusedBits {
        get => unusedBits;
        set {
            unusedBits = value;
            OnPropertyChanged(nameof(UnusedBits));
        }
    }
    public Boolean UnusedBitsVisible => Node.Tag == (Byte)Asn1Type.BIT_STRING;
    public Boolean IsReadOnly {
        get => isReadonly;
        set {
            isReadonly = value;
            OnPropertyChanged(nameof(IsReadOnly));
            OnPropertyChanged(nameof(IsEditable));
        }
    }
    public Boolean IsEditable => !IsReadOnly;
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
        UnusedTextBoxWidth = TextUtility.MeasureStringWidth(masterUnused, Settings.Default.FontSize, false);
    }
    void submitValues(Object obj) {
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
                binValue = AsnDecoder.EncodeHex(Node.Tag, TagValue.TextValue, UnusedBits);
            } else {
                binValue = AsnDecoder.EncodeGeneric(Node.Tag, TagValue.TextValue, UnusedBits);
            }
        } catch (Exception e) {
            _uiMessenger.ShowError(e.Message);
        }
        return binValue;
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
        UnusedBits = Node.UnusedBits;
        IsReadOnly = Node.IsContainer || Node.Tag == (Byte)Asn1Type.NULL;
        TagDetails = String.Format(Resources.TagEditorHeaderTemplate, Node.Tag, Node.TagName, Node.Offset, Node.PayloadLength, Node.Depth, Node.Path);
        OnPropertyChanged(nameof(UnusedBitsVisible));
    }

    public void SetBinding(NodeEditMode editMode) {
        mode = editMode;
        copyValues();
        if (IsReadOnly || Node.Tag == (Byte)Asn1Type.NULL) {
            TagValue.TextValue = "Containers and NULL (0x05) tags are not editable";
        } else {
            editText();
        }
        oldValue = TagValue.Clone();
    }
}