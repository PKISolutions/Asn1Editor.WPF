using System;
using System.Xml.Serialization;
using SysadminsLV.Asn1Editor.API.ViewModel;

namespace SysadminsLV.Asn1Editor.API.ModelObjects; 

[XmlRoot("appSettings")]
public class NodeViewOptions : ViewModelBase {
    Boolean showTagNumber, showNodeOffset = true, showNodeLength = true, showInHex, showContent = true, showNodePath, intAsInt,
        showHexViewer, showHexAddrPanel = true, showHexAsciiPanel = true;
    Int32 fontSize = 12;
    Int32 maxStringLength = 150;

    [XmlElement("showTagNum")]
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
    [XmlElement("showTagOffset")]
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
    [XmlElement("showNodeLength")]
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
    [XmlElement("showHexHeader")]
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
    [XmlElement("showNodeContent")]
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
    [XmlElement("showNodePath")]
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
    [XmlElement("forceInteger")]
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
    [XmlElement("showHexViewer")]
    public Boolean ShowHexViewer {
        get => showHexViewer;
        set {
            if (value == showHexViewer) {
                return;
            }
            showHexViewer = value;
            OnPropertyChanged(nameof(ShowHexViewer));
        }
    }
    [XmlElement("showAddrPanel")]
    public Boolean ShowHexAddrPanel {
        get => showHexAddrPanel;
        set {
            if (value == showHexAddrPanel) {
                return;
            }
            showHexAddrPanel = value;
            OnPropertyChanged(nameof(ShowHexAddrPanel));
        }
    }
    [XmlElement(ElementName = "showAsciiPanel")]
    public Boolean ShowHexAsciiPanel {
        get => showHexAsciiPanel;
        set {
            if (value == showHexAsciiPanel) {
                return;
            }
            showHexAsciiPanel = value;
            OnPropertyChanged(nameof(ShowHexAsciiPanel));
        }
    }
    [XmlElement("fontSize")]
    public Int32 FontSize {
        get => fontSize;
        set {
            if (value == fontSize) {
                return;
            }
            fontSize = value;
            OnPropertyChanged(nameof(FontSize));
        }
    }
    [XmlElement("maxTreeTextLength")]
    public Int32 MaxTreeTextLength {
        get => maxStringLength;
        set {
            if (value == maxStringLength) {
                return;
            }
            maxStringLength = value;
            OnPropertyChanged(nameof(MaxTreeTextLength));
        }
    }
}