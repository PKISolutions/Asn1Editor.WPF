using System;
using SysadminsLV.Asn1Editor.API.ViewModel;
using SysadminsLV.Asn1Editor.Properties;

namespace SysadminsLV.Asn1Editor.API.ModelObjects {
    public class NodeViewOptions : ViewModelBase {
        Boolean showTagNumber, showNodeOffset = true, showNodeLength = true, showInHex, showContent = true, showNodePath, intAsInt;
        Int32 fontSize = 12;

        public Boolean ShowTagNumber {
            get => showTagNumber;
            set {
                if (value == showTagNumber) {
                    return;
                }
                showTagNumber = value;
                OnPropertyChanged(nameof(ShowTagNumber));
            }
        }
        public Boolean ShowNodeOffset {
            get => showNodeOffset;
            set {
                if (value == showNodeOffset) {
                    return;
                }
                showNodeOffset = value;
                OnPropertyChanged(nameof(ShowNodeOffset));
            }
        }
        public Boolean ShowNodeLength {
            get => showNodeLength;
            set {
                if (value == showNodeLength) {
                    return;
                }
                showNodeLength = value;
                OnPropertyChanged(nameof(ShowNodeLength));
            }
        }
        public Boolean ShowInHex {
            get => showInHex;
            set {
                if (value == showInHex) {
                    return;
                }
                showInHex = value;
                OnPropertyChanged(nameof(ShowInHex));
            }
        }
        public Boolean ShowContent {
            get => showContent;
            set {
                if (value == showContent) {
                    return;
                }
                showContent = value;
                OnPropertyChanged(nameof(ShowContent));
            }
        }
        public Boolean ShowNodePath {
            get => showNodePath;
            set {
                if (value == showNodePath) {
                    return;
                }
                showNodePath = value;
                OnPropertyChanged(nameof(ShowNodePath));
            }
        }
        public Boolean IntegerAsInteger {
            get => intAsInt;
            set {
                if (value == intAsInt) {
                    return;
                }
                intAsInt = value;
                OnPropertyChanged(nameof(IntegerAsInteger));
            }
        }
        public Int32 FontSize {
            get => fontSize;
            set {
                if (value == fontSize) {
                    return;
                }
                fontSize = value;
                Settings.Default.FontSize = fontSize;
                OnPropertyChanged(nameof(FontSize));
            }
        }
    }
}
