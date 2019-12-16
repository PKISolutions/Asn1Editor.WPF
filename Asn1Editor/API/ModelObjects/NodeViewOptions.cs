using System;
using SysadminsLV.Asn1Editor.API.ViewModel;

namespace SysadminsLV.Asn1Editor.API.ModelObjects {
    public class NodeViewOptions : ViewModelBase {
        Boolean showTagNumber, showNodeOffset = true, showNodeLength = true, showInHex, showContent = true, showNodePath;

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
    }
}
