using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using SysadminsLV.Asn1Editor.API.Interfaces;
using SysadminsLV.Asn1Editor.API.ModelObjects;
using SysadminsLV.Asn1Editor.API.Utils;
using SysadminsLV.Asn1Parser;
using SysadminsLV.WPF.OfficeTheme.Controls;
using SysadminsLV.WPF.OfficeTheme.Toolkit.Commands;

namespace SysadminsLV.Asn1Editor.API.ViewModel;

class MainWindowVM : ViewModelBase, IMainWindowVM, IHasSelectedTab {
    readonly IWindowFactory _windowFactory;
    String path;
    Asn1DocumentVM selectedTab;
    Boolean hasClipboard, isBusy, isModified;

    public MainWindowVM(
        IWindowFactory windowFactory,
        IAppCommands appCommands,
        NodeViewOptions nodeViewOptions) {
        _windowFactory = windowFactory;
        GlobalData = new GlobalData();
        AppCommands = appCommands;
        TreeCommands = new TreeViewCommands(windowFactory, this);
        NodeViewOptions = nodeViewOptions;
        NodeViewOptions.PropertyChanged += onNodeViewOptionsChanged;
        NewCommand = new RelayCommand(newTab);
        CloseTabCommand = new RelayCommand(closeTab, canCloseTab);
        OpenCommand = new AsyncCommand(openFileAsync);
        SaveCommand = new RelayCommand(saveFile, canPrintSave);
        DropFileCommand = new AsyncCommand(dropFileAsync);
        appCommands.ShowConverterWindow = new RelayCommand(showConverter);
        //DataSource.CollectionChanged += (sender, args) => IsModified = true;
        IsModified = false;
        Tabs.Add(new Asn1DocumentVM(NodeViewOptions, TreeCommands));
        SelectedTab = Tabs[0];
    }

    void onNodeViewOptionsChanged(Object sender, PropertyChangedEventArgs e) {
        foreach (Asn1DocumentVM tab in Tabs) {
            if (tab.Tree.Any()) {
                tab.Tree[0].UpdateNodeView(NodeViewOptions);
            }
        }
    }

    public ICommand NewCommand { get; set; }
    public ICommand CloseTabCommand { get; set; }
    public IAsyncCommand OpenCommand { get; set; }
    public ICommand SaveCommand { get; set; }
    public ICommand PrintCommand { get; set; }
    public ICommand SettingsCommand { get; set; }
    public IAsyncCommand DropFileCommand { get; }

    public IAppCommands AppCommands { get; }
    public ITreeCommands TreeCommands { get; }

    public GlobalData GlobalData { get; }
    //public IDataSource DataSource { get; }
    public static Dictionary<String, String> OIDs { get; } = new();
    public NodeViewOptions NodeViewOptions { get; }
    public ObservableCollection<Asn1DocumentVM> Tabs { get; } = new();
    public Asn1DocumentVM SelectedTab {
        get => selectedTab;
        set {
            selectedTab = value;
            OnPropertyChanged(nameof(SelectedTab));
        }
    }

    public String Path {
        get => path;
        set {
            path = value;
            OnPropertyChanged(nameof(Path));
        }
    }
    public Boolean IsModified {
        get => isModified;
        set {
            isModified = value;
            OnPropertyChanged(nameof(IsModified));
        }
    }
    public Boolean HasClipboardData {
        get => hasClipboard;
        set {
            hasClipboard = value;
            OnPropertyChanged(nameof(HasClipboardData));
        }
    }
    public Boolean IsBusy {
        get => isBusy;
        set {
            isBusy = value;
            OnPropertyChanged(nameof(IsBusy));
        }
    }
    void showConverter(Object o) {
        _windowFactory.ShowConverterWindow(SelectedTab.DataSource.RawData, openRawAsync);
    }
    void newTab(Object o) {
        var tab = new Asn1DocumentVM(NodeViewOptions, TreeCommands);
        addTabToList(tab);
    }
    void addTabToList(Asn1DocumentVM tab, Boolean focus = true) {
        Tabs.Add(tab);
        if (focus) {
            SelectedTab = tab;
        }
    }
    void closeTab(Object o) {
        // TODO: need to eliminate explicit reference to UI elements
        if (o is ClosableTabItem tabItem) {
            Tabs.Remove((Asn1DocumentVM)tabItem.Content);
        }
    }
    Boolean canCloseTab(Object o) {
        // TODO: need to eliminate explicit reference to UI elements
        return o is ClosableTabItem;
    }

    Task openFileAsync(Object obj, CancellationToken token = default) {
        if (IsModified && !RequestFileSave()) {
            return Task.CompletedTask;
        }
        String filePath;
        Boolean useDefaultTab = false;
        if (obj == null) {
            Tools.TryGetOpenFileName(out filePath);
        } else {
            filePath = obj.ToString();
            useDefaultTab = true;
        }
        if (String.IsNullOrWhiteSpace(filePath)) {
            return Task.CompletedTask;
        }
        return createTabFromFile(filePath, useDefaultTab);
    }
    void saveFile(Object obj) {
        if ((obj != null || String.IsNullOrEmpty(Path)) && !getFilePath()) {
            return;
        }
        writeFile();
    }
    Boolean writeFile() {
        if (String.IsNullOrEmpty(Path) && !getFilePath()) {
            return false;
        }
        try {
            File.WriteAllBytes(Path, SelectedTab.DataSource.RawData.ToArray());
            IsModified = false;
            return true;
        } catch (Exception e) {
            Tools.MsgBox("Save Error", e.Message);
        }
        return false;
    }
    async Task createTabFromFile(String file, Boolean useDefaultTab = false) {
        Asn1DocumentVM tab;
        if (useDefaultTab && Tabs.Any()) {
            tab = Tabs[0];
        } else {
            tab = new Asn1DocumentVM(NodeViewOptions, TreeCommands) {
                Path = file
            };
        }
        try {
            IEnumerable<Byte> bytes = await FileUtility.FileToBinary(file);
            await tab.Decode(bytes);
        } catch (Exception ex) {
            Tools.MsgBox("Read Error", ex.Message);
            return;
        }
        addTabToList(tab);
    }


    Boolean canPrintSave(Object obj) {
        return SelectedTab?.DataSource.RawData.Count > 0;
    }
    Boolean getFilePath() {
        if (!Tools.TryGetSaveFileName(out String filePath)) {
            return false;
        }
        Path = filePath;
        return true;
    }

    public Boolean RequestFileSave() {
        MessageBoxResult result = Tools.MsgBox(
            "Unsaved Data",
            "Current file was modified. Save changes?",
            MessageBoxImage.Warning,
            MessageBoxButton.YesNoCancel);
        switch (result) {
            case MessageBoxResult.No:
                return true;
            case MessageBoxResult.Yes:
                return writeFile();
            default:
                return false;
        }
    }

    Task dropFileAsync(Object o, CancellationToken token = default) {
        if (o is not String filePath) {
            return Task.CompletedTask;
        }

        if (!File.Exists(filePath)) {
            return Task.CompletedTask;
        }

        return createTabFromFile(filePath);
    }
    public Task DropFileAsync(String filePath) {
        if (!File.Exists(filePath)) {
            return Task.CompletedTask;
        }
        
        return createTabFromFile(filePath);
    }
    public Task OpenExistingAsync(String filePath) {
        return openFileAsync(filePath);
    }
    public Task OpenRawAsync(String base64String) {
        return openRawAsync(Convert.FromBase64String(base64String));
    }
    async Task openRawAsync(Byte[] rawBytes) {
        var asn = new Asn1Reader(rawBytes);
        asn.BuildOffsetMap();
        Tabs[0].Reset();
        await Tabs[0].Decode(rawBytes);
    }
}