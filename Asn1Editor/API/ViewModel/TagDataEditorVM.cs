using System;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using SysadminsLV.Asn1Editor.API.Interfaces;
using SysadminsLV.Asn1Editor.API.ModelObjects;
using SysadminsLV.Asn1Editor.API.Utils;
using SysadminsLV.Asn1Editor.API.Utils.ASN;
using SysadminsLV.Asn1Editor.Properties;
using SysadminsLV.Asn1Parser;
using SysadminsLV.WPF.OfficeTheme.Toolkit.Commands;

namespace SysadminsLV.Asn1Editor.API.ViewModel {
    class TagDataEditorVM : ViewModelBase, ITagDataEditorVM {
        readonly IDataSource _data;
        const String masterTag = "123";
        const String masterUnused = "1";
        NodeEditMode mode;
        Boolean? dialogResult;
        String tagDetails, tagValue, oldValue;
        Boolean rbText = true, rbHex, isReadonly, tagIsReadOnly = true;
        Visibility unusedBitsVisible = Visibility.Collapsed;
        Byte unusedBits, tag;
        Double tagTextBoxWidth, unusedTextBoxWidth;

        public TagDataEditorVM(IDataSource data) {
            _data = data;
            OkCommand = new RelayCommand(submitValues);
            CloseCommand = new RelayCommand(close);
            initialize();
        }

        public ICommand OkCommand { get; set; }
        public ICommand CloseCommand { get; set; }
        public Asn1Lite Node { get; private set; }

        public String TagDetails {
            get => tagDetails;
            set {
                tagDetails = value;
                OnPropertyChanged(nameof(TagDetails));
            }
        }
        public String TagValue {
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
                OnPropertyChanged(nameof(Tag));
            }
        }
        public String TagName => Asn1Utils.GetTagName(Tag);
        public Byte UnusedBits {
            get => unusedBits;
            set {
                unusedBits = value;
                OnPropertyChanged(nameof(UnusedBits));
            }
        }
        public Visibility UnusedBitsVisible {
            get => unusedBitsVisible;
            set {
                unusedBitsVisible = value;
                OnPropertyChanged(nameof(UnusedBitsVisible));
            }
        }
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
        public Boolean? DialogResult {
            get => dialogResult;
            set {
                dialogResult = value;
                OnPropertyChanged(nameof(DialogResult));
            }
        }

        void initialize() {
            calculateLengths();
            Settings.Default.PropertyChanged += (Sender, Args) => {
                if (Args.PropertyName == nameof(Settings.FontSize)) {
                    calculateLengths();
                }
            };
        }
        void calculateLengths() {
            TagTextBoxWidth = Tools.MeasureString(masterTag, Settings.Default.FontSize, false);
            UnusedTextBoxWidth = Tools.MeasureString(masterUnused, Settings.Default.FontSize, false);
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
            Node.Tag = Tag;
            if (TagValue == oldValue) {
                DialogResult = true;
                return;
            }
            if (Node.ValidateValue(TagValue, UnusedBits)) {
                DialogResult = true;
            }
        }
        void close(Object o) {
            Node = null;
            DialogResult = true;
        }
        void saveNewNode() {
            var asn = new Asn1Reader(new Byte[] { Tag, 0 });
            Asn1Lite node;
            // if it is constructed or value is not entered, use it as is and ignore possible entered tag value
            if (asn.IsConstructed || String.IsNullOrWhiteSpace(TagValue?.Trim())) {
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
            if (_data.SelectedNode == null) {
                node.IsRoot = true;
            } else {
                node.Offset = _data.SelectedNode.Value.Offset + _data.SelectedNode.Value.TagLength;
                node.Deepness += _data.SelectedNode.Value.Deepness;
            }
            Node = node;
            updateNewNodeBinarySource(asn.RawData);
            DialogResult = true;
        }
        Byte[] validateValue() {
            Byte[] binValue = null;
            try {
                binValue = AsnDecoder.EncodeGeneric(Tag, TagValue, UnusedBits);
            } catch (Exception e) {
                Tools.MsgBox("Error", e.Message);
            }
            return binValue;
        }
        void updateNewNodeBinarySource(Byte[] binData) {
            _data.RawData.InsertRange(Node.Offset, binData);
        }
        void editText() {
            TagValue = AsnDecoder.GetEditValue(Node);
        }
        void editHex() {
            TagValue = HexUtility
                .BinaryToHex(_data.RawData
                    .Skip(Node.PayloadStartOffset)
                    .Take(Node.PayloadLength)
                .ToArray());
        }

        public void SetBinding(NodeEditMode editMode) {
            mode = editMode;
            if (editMode == NodeEditMode.NewNode) {
                Tag = 48;
                TagIsReadOnly = false;
                return;
            }
            Node = _data.SelectedNode.Value;
            Tag = Node.Tag;
            UnusedBits = Node.UnusedBits;
            IsReadOnly = Node.IsContainer || Node.Tag == (Byte)Asn1Type.NULL;
            TagDetails = String.Format(Resources.TagEditorHeaderTemp, Tag, Node.TagName, Node.Offset, Node.PayloadLength, Node.Deepness, Node.Path);
            if (IsReadOnly || Node.Tag == (Byte)Asn1Type.NULL) {
                TagValue = "Containers and NULL (0x05) tags are not editable";
            } else {
                editText();
            }
            oldValue = TagValue;
            UnusedBitsVisible = Node.Tag == (Byte)Asn1Type.BIT_STRING
                ? Visibility.Visible
                : Visibility.Collapsed;
        }
    }
}
