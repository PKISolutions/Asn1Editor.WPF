using SysadminsLV.Asn1Editor.API.ModelObjects;

namespace SysadminsLV.Asn1Editor.API.Interfaces {
    interface IWindowFactory {
        void ShowSettingsDialog();
        void ShowAboutdialog();
        void ShowTagcontentEditor(Asn1Lite node);
        void ShowNodeTextViewer();
    }
}
