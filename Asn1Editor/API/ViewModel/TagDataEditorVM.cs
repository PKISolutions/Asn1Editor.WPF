using System;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Asn1Editor.API.ModelObjects;
using Asn1Editor.API.Utils;
using Asn1Editor.API.Utils.ASN;
using Asn1Editor.Properties;
using Asn1Editor.Views.Windows;
using SysadminsLV.Asn1Parser;
using SysadminsLV.WPF.OfficeTheme.Toolkit.Commands;

namespace Asn1Editor.API.ViewModel {
    class TagDataEditorVM : INotifyPropertyChanged {
        const String masterTag = "123";
        const String masterUnused = "1";
        Asn1Lite asn;
        String tagDetails, tagValue, tagHex, oldValue;
        Boolean isReadonly, tagIsReadOnly = true;
        Visibility unusedBitsVisible = Visibility.Collapsed;
        Byte unusedBits, tag;
        Double tagTextBoxWidth, unusedTextBoxWidth;

        public TagDataEditorVM() {
            OkCommand = new RelayCommand(SubmitValues);
            Init();
        }

        public ICommand OkCommand { get; set; }
        public Boolean Accepted { get; private set; }

        public String TagDetails {
            get => tagDetails;
            set {
                tagDetails = value;
                OnPropertyChanged("TagDetails");
            }
        }
        public String TagValue {
            get => tagValue;
            set {
                tagValue = value;
                OnPropertyChanged("TagValue");
            }
        }
        public Boolean TagIsReadOnly {
            get => tagIsReadOnly;
            set {
                tagIsReadOnly = value;
                OnPropertyChanged("TagIsReadOnly");
            }
        }
        public Byte Tag {
            get => tag;
            set {
                if (Byte.TryParse(value.ToString(CultureInfo.InvariantCulture), out Byte t)) {
                    tag = t;
                }
                OnPropertyChanged("Tag");
            }
        }
        public Byte UnusedBits {
            get => unusedBits;
            set {
                unusedBits = value;
                OnPropertyChanged("UnusedBits");
            }
        }
        public Visibility UnusedBitsVisible {
            get => unusedBitsVisible;
            set {
                unusedBitsVisible = value;
                OnPropertyChanged("UnusedBitsVisible");
            }
        }
        public Boolean IsReadonly {
            get => isReadonly;
            set {
                isReadonly = value;
                OnPropertyChanged("IsReadonly");
            }
        }
        public Double TagTextBoxWidth {
            get => tagTextBoxWidth;
            set {
                tagTextBoxWidth = value;
                OnPropertyChanged("TagTextBoxWidth");
            }
        }
        public Double UnusedTextBoxWidth {
            get => unusedTextBoxWidth;
            set {
                unusedTextBoxWidth = value;
                OnPropertyChanged("UnusedTextBoxWidth");
            }
        }

        void Init() {
            calculateLengths();
            Settings.Default.PropertyChanged += (Sender, Args) => {
                if (Args.PropertyName == "FontSize") {
                    calculateLengths();
                }
            };
        }
        void calculateLengths() {
            TagTextBoxWidth = Tools.MeasureString(masterTag, Settings.Default.FontSize, false);
            UnusedTextBoxWidth = Tools.MeasureString(masterUnused, Settings.Default.FontSize, false);
        }
        void SubmitValues(Object obj) {
            Accepted = true;
            if (Tag == 0) {
                Tools.MsgBox("Error", "Invalid tag number");
                return;
            }
            asn.Tag = Tag;
            if (TagValue == oldValue) {
                ((TagDataEditor)obj).Close();
                return;
            }
            if (asn.ValidateValue(TagValue, UnusedBits)) {
                ((TagDataEditor)obj).Close();
            }
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
                    ? HexUtility.BinaryToHex(MainWindowVM.RawData.Skip(asnNode.PayloadStartOffset).Take(asnNode.PayloadLength).ToArray())
                    : AsnDecoder.GetEditValue(asnNode);
            oldValue = TagValue;
            UnusedBitsVisible = asnNode.Tag == (Byte)Asn1Type.BIT_STRING
                ? Visibility.Visible
                : Visibility.Hidden;
        }

        void OnPropertyChanged(String PropertyName) {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) {
                handler(this, new PropertyChangedEventArgs(PropertyName));
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;
    }
}
