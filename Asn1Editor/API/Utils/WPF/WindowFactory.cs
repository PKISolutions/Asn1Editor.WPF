﻿using System;
using System.Collections.Generic;
using SysadminsLV.Asn1Editor.API.Interfaces;
using SysadminsLV.Asn1Editor.API.ModelObjects;
using SysadminsLV.Asn1Editor.API.ViewModel;
using SysadminsLV.Asn1Editor.Views.Windows;
using Unity;

namespace SysadminsLV.Asn1Editor.API.Utils.WPF;

class WindowFactory : WindowFactoryBase, IWindowFactory {
    static BinaryConverterWindow binConverterWindow;
    static Boolean binConverterWindowClosed = true;

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
        if (!binConverterWindowClosed) {
            binConverterWindow.Focus();
            return;
        }
        binConverterWindowClosed = false;
        hwnd = binConverterWindow = new BinaryConverterWindow();
        var binConverterVM = new BinaryConverterVM(action);
        hwnd.DataContext = binConverterVM;
        binConverterVM.SetBytes(data);
        hwnd.Closed += (o, e) => { binConverterWindowClosed = true; };
        ShowAsWindow(true);
    }
}