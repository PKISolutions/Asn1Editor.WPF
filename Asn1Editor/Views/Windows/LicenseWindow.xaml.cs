using System;
using System.IO;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using SysadminsLV.WPF.OfficeTheme.Toolkit;
using SysadminsLV.WPF.OfficeTheme.Toolkit.Commands;

namespace SysadminsLV.Asn1Editor.Views.Windows;

/// <summary>
/// Interaction logic for LicenseWindow.xaml
/// </summary>
public partial class LicenseWindow {
    public LicenseWindow() {
        CloseCommand = new RelayCommand(o => Close());
        InitializeComponent();
        loadEula();
        DataContext = this;
    }
    public ICommand CloseCommand { get; }

    void loadEula() {
        if (File.Exists("EULA.rtf")) {
            try {
                using var fileStream = new FileStream("EULA.rtf", FileMode.Open, FileAccess.Read, FileShare.Read);
                var textRange = new TextRange(rtb.Document.ContentStart, rtb.Document.ContentEnd);
                textRange.Load(fileStream, DataFormats.Rtf);
            } catch (Exception ex) {
                MsgBox.Show("Error", $"Failed to open license file:\n{ex.Message}");
            }
        } else {
            MsgBox.Show("Error", "License file not found.");
        }
    }
}