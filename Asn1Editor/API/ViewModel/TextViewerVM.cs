using System;
using System.Globalization;
using System.IO;
using System.Windows.Input;
using Asn1Editor.Wpf.Controls.Helpers;
using SysadminsLV.Asn1Editor.API.Abstractions;
using SysadminsLV.Asn1Editor.API.Interfaces;
using SysadminsLV.Asn1Editor.API.ModelObjects;
using SysadminsLV.Asn1Editor.API.Utils;
using SysadminsLV.Asn1Editor.Properties;
using SysadminsLV.WPF.OfficeTheme.Toolkit.Commands;

namespace SysadminsLV.Asn1Editor.API.ViewModel;

class TextViewerVM : ViewModelBase, ITextViewerVM {
    readonly IUIMessenger _uiMessenger;
    readonly Asn1TreeNode rootNode;
    const String master = "123";
    const Int32 minLength = 60;
    const Int32 defaultLength = 80;
    const Int32 maxLength = 400;
    ITextRenderer renderer;
    Boolean certutilChecked, openSSLChecked;

    String text;
    Int32 currentLength = 80;
    String currentLengthStr = "80";
    Double width;

    public TextViewerVM(IHasAsnDocumentTabs appTabs, IUIMessenger uiMessenger) {
        rootNode = appTabs.SelectedTab.DataSource.SelectedNode;
        CurrentLength = defaultLength.ToString(CultureInfo.InvariantCulture);
        SaveCommand = new RelayCommand(saveFile);
        PrintCommand = new RelayCommand(print);
        ApplyCommand = new RelayCommand(applyNewLength);
        TextBoxWidth = TextUtility.MeasureStringWidth(master, Settings.Default.FontSize, false);
        CertutilViewChecked = true;
        renderer.RenderText(currentLength);
        _uiMessenger = uiMessenger;
    }

    public ICommand SaveCommand { get; set; }
    public ICommand PrintCommand { get; set; }
    public ICommand ApplyCommand { get; set; }

    public String Text {
        get => text;
        set {
            text = value;
            OnPropertyChanged(nameof(Text));
        }
    }
    public String CurrentLength {
        get => currentLengthStr;
        set {
            currentLengthStr = value;
            OnPropertyChanged(nameof(CurrentLength));
        }
    }
    public Double TextBoxWidth {
        get => width;
        set {
            width = value;
            OnPropertyChanged(nameof(TextBoxWidth));
        }
    }
    public Boolean CertutilViewChecked {
        get => certutilChecked;
        set {
            if (certutilChecked == value) {
                return;
            }
            certutilChecked = value;
            if (certutilChecked) {
                renderer = new CertutilRenderer(rootNode);
                Text = renderer.RenderText(currentLength);
            }
            OnPropertyChanged(nameof(CertutilViewChecked));
        }
    }
    public Boolean OpenSSLViewChecked {
        get => openSSLChecked;
        set {
            if (openSSLChecked == value) {
                return;
            }
            openSSLChecked = value;
            if (openSSLChecked) {
                renderer = new OpenSSLRenderer(rootNode);
                Text = renderer.RenderText(currentLength);
            }
            OnPropertyChanged(nameof(OpenSSLViewChecked));
        }
    }

    void print(Object obj) {
        StaticCommands.Print(Text);
    }
    void applyNewLength(Object obj) {
        if (!Int32.TryParse(CurrentLength, NumberStyles.Integer, null, out Int32 value)) {
            CurrentLength = currentLength.ToString(CultureInfo.InvariantCulture);
            return;
        }
        if (value == currentLength) { return; }
        currentLength = value < minLength || value > maxLength
            ? minLength
            : value;
        CurrentLength = currentLength.ToString(CultureInfo.InvariantCulture);
        renderer.RenderText(currentLength);
    }

    void saveFile(Object obj) {
        if (!_uiMessenger.TryGetSaveFileName(out String filePath)) {
            return;
        }
        try {
            File.WriteAllText(filePath, Text);
        } catch (Exception e) {
            _uiMessenger.ShowError(e.Message, "Save Error");
        }
    }
}