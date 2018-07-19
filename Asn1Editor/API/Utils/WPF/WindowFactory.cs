using SysadminsLV.Asn1Editor.API.Interfaces;
using SysadminsLV.Asn1Editor.API.ModelObjects;
using SysadminsLV.Asn1Editor.Views.Windows;
using Unity;

namespace SysadminsLV.Asn1Editor.API.Utils.WPF {
    class WindowFactory : WindowFactoryBase, IWindowFactory {
        public void ShowSettingsDialog() {
            hwnd = App.Container.Resolve<SettingsWnd>();
            ShowAsDialog();
        }
        public void ShowAboutdialog() {
            hwnd = App.Container.Resolve<AboutBox>();
            ShowAsDialog();
        }
        public Asn1Lite ShowNodeContentEditor(NodeEditMode editMode) {
            hwnd = App.Container.Resolve<TagDataEditor>();
            ((ITagDataEditorVM)hwnd.DataContext).SetBinding(editMode);
            ShowAsDialog();
            return ((ITagDataEditorVM) hwnd.DataContext).Node;
        }
        public void ShowNodeTextViewer() {
            hwnd = App.Container.Resolve<TextViewer>();
            ShowAsWindow(true);
        }
    }
}
