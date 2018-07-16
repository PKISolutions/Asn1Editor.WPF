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
        Boolean? dialogResult;
        String tagDetails, tagValue, oldValue;
        Boolean isReadonly, tagIsReadOnly = true;
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
        public Boolean Accepted { get; private set; }
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
                OnPropertyChanged(nameof(Tag));
            }
        }
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
        public Boolean IsReadonly {
            get => isReadonly;
            set {
                isReadonly = value;
                OnPropertyChanged(nameof(IsReadonly));
            }
        }
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
            Accepted = true;
            if (Tag == 0) {
                Tools.MsgBox("Error", "Invalid tag number");
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
            DialogResult = true;
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
            Node = _data.SelectedNode.Value;
            Tag = Node.Tag;
            UnusedBits = Node.UnusedBits;
            IsReadonly = Node.IsContainer || Node.Tag == (Byte)Asn1Type.NULL;
            TagDetails = String.Format(Resources.TagEditorHeaderTemp, Tag, Node.TagName, Node.Offset, Node.PayloadLength, Node.Deepness, Node.Path);
            if (IsReadonly || Node.Tag == (Byte)Asn1Type.NULL) {
                TagValue = "Containers and NULL (0x05) tags are not editable";
            } else {
                switch (editMode) {
                    case NodeEditMode.Text:
                        editText();
                        break;
                    case NodeEditMode.Hex:
                        editHex();
                        break;
                }
            }
            oldValue = TagValue;
            UnusedBitsVisible = Node.Tag == (Byte)Asn1Type.BIT_STRING
                ? Visibility.Visible
                : Visibility.Hidden;
        }
    }
}
