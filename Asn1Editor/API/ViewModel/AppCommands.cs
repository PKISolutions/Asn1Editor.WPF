using System;
using System.Windows.Input;
using SysadminsLV.Asn1Editor.API.Interfaces;
using SysadminsLV.WPF.OfficeTheme.Toolkit.Commands;

namespace SysadminsLV.Asn1Editor.API.ViewModel; 

class AppCommands : IAppCommands {
    readonly IWindowFactory _windowFactory;

    public AppCommands(IWindowFactory windowFactory) {
        _windowFactory = windowFactory;
        ShowLicenseCommand = new RelayCommand(showLicense);
        ShowAboutCommand = new RelayCommand(showAbout);
        ShowSettingsCommand = new RelayCommand(showSettings);
    }

    public ICommand ShowLicenseCommand { get; }
    public ICommand ShowAboutCommand { get; }
    public ICommand ShowSettingsCommand { get; }
    public ICommand ShowNodeTextViewer { get; set; }
    public ICommand ShowConverterWindow { get; set; }

    void showLicense(Object o) {
        _windowFactory.ShowLicenseDialog();
    }
    void showAbout(Object o) {
        _windowFactory.ShowAboutDialog();
    }
    void showSettings(Object o) {
        _windowFactory.ShowSettingsDialog();
    }
}