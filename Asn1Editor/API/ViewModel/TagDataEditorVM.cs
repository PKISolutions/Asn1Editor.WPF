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
using Unity;

namespace SysadminsLV.Asn1Editor.API.ViewModel {
    class TagDataEditorVM : ViewModelBase, ITagDataEditorVM {
        readonly IDataSource _data;
        const String masterTag = "123";
        const String masterUnused = "1";
        Boolean? dialogResult;
        Asn1Lite asn;
        String tagDetails, tagValue, oldValue;
        Boolean isReadonly, tagIsReadOnly = true;
        Visibility unusedBitsVisible = Visibility.Collapsed;
        Byte unusedBits, tag;
        Double tagTextBoxWidth, unusedTextBoxWidth;

        public TagDataEditorVM() {
            OkCommand = new RelayCommand(submitValues);
            CloseCommand = new RelayCommand(close);
            init();
        }

        public ICommand OkCommand { get; set; }
        public ICommand CloseCommand { get; set; }
        public Boolean Accepted { get; private set; }

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

        void init() {
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
            asn.Tag = Tag;
            if (TagValue == oldValue) {
                DialogResult = true;
                return;
            }
            if (asn.ValidateValue(TagValue, UnusedBits)) {
                DialogResult = true;
            }
        }
        void close(Object o) {
            DialogResult = true;
        }

        public void SetBinding(Asn1Lite asnNode, Boolean hex) {
            asn = asnNode;
            Tag = asnNode.Tag;
            UnusedBits = asnNode.UnusedBits;
            IsReadonly = asnNode.IsContainer || asnNode.Tag == (Byte)Asn1Type.NULL;
            TagDetails = String.Format(Resources.TagEditorHeaderTemp, Tag, asnNode.TagName, asnNode.Offset, asnNode.PayloadLength, asnNode.Deepness, asnNode.Path);
            TagValue = IsReadonly || asnNode.Tag == (Byte)Asn1Type.NULL
                ? "Containers and NULL (0x05) tags are not editable"
                : hex
                    ? HexUtility.BinaryToHex(App.Container.Resolve<IDataSource>().RawData.Skip(asnNode.PayloadStartOffset).Take(asnNode.PayloadLength).ToArray())
                    : AsnDecoder.GetEditValue(asnNode);
            oldValue = TagValue;
            UnusedBitsVisible = asnNode.Tag == (Byte)Asn1Type.BIT_STRING
                ? Visibility.Visible
                : Visibility.Hidden;
        }
    }
}
