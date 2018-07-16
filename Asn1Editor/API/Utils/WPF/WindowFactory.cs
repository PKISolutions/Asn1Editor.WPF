using System;
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
        public void ShowTagcontentEditor(Asn1Lite node) {
            throw new NotImplementedException();
        }
        public void ShowNodeTextViewer() {
            hwnd = App.Container.Resolve<TextViewer>();
            ShowAsWindow();
        }
    }
}
