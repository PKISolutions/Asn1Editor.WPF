using SysadminsLV.Asn1Editor.API.ModelObjects;
using SysadminsLV.Asn1Editor.API.Utils;

namespace SysadminsLV.Asn1Editor.API.Interfaces {
    interface IWindowFactory {
        void ShowSettingsDialog();
        void ShowAboutdialog();
        Asn1Lite ShowNodeContentEditor(NodeEditMode editMode);
        void ShowNodeTextViewer();
    }
}
