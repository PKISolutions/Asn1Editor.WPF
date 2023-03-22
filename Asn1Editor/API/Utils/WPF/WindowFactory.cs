using System;
using System.Collections.Generic;
using SysadminsLV.Asn1Editor.API.Interfaces;
using SysadminsLV.Asn1Editor.API.ModelObjects;
using SysadminsLV.Asn1Editor.API.ViewModel;
using SysadminsLV.Asn1Editor.Views.Windows;
using Unity;

namespace SysadminsLV.Asn1Editor.API.Utils.WPF; 

class WindowFactory : WindowFactoryBase, IWindowFactory {
    static Converter converter;
    static Boolean converterClosed = true;

    public void ShowLicenseDialog() {
        hwnd = App.Container.Resolve<LicenseWindow>();
        ShowAsDialog();
    }
    public void ShowSettingsDialog() {
        hwnd = App.Container.Resolve<SettingsWnd>();
        ShowAsDialog();
    }
    public void ShowAboutDialog() {
        hwnd = App.Container.Resolve<AboutBox>();
        ShowAsDialog();
    }
    public Asn1Lite ShowNodeContentEditor(NodeEditMode editMode) {
        hwnd = App.Container.Resolve<TagDataEditor>();
        ((ITagDataEditorVM)hwnd.DataContext).SetBinding(editMode);
        ShowAsDialog();
        return ((ITagDataEditorVM)hwnd.DataContext).Node;
    }
    public void ShowNodeTextViewer() {
        hwnd = App.Container.Resolve<TextViewer>();
        ShowAsWindow(true);
    }
    public void ShowConverterWindow(IEnumerable<Byte> data, Action<Byte[]> action) {
        if (!converterClosed) {
            converter.Focus();
            return;
        }
        converterClosed = false;
        hwnd = converter = new Converter();
        var hwndvm = new ConverterVM(action);
        hwnd.DataContext = hwndvm;
        hwndvm.SetBytes(data);
        hwnd.Closed += (o, e) => { converterClosed = true; };
        ShowAsWindow(true);
    }
}