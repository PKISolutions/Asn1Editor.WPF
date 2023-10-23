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
    Asn1DocumentVM selectedTab;
    Boolean hasClipboard;

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
    public NodeViewOptions NodeViewOptions { get; }
    public ObservableCollection<Asn1DocumentVM> Tabs { get; } = new();
    public Asn1DocumentVM SelectedTab {
        get => selectedTab;
        set {
            selectedTab = value;
            OnPropertyChanged(nameof(SelectedTab));
        }
    }
    public Boolean HasClipboardData {
        get => hasClipboard;
        set {
            hasClipboard = value;
            OnPropertyChanged(nameof(HasClipboardData));
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
            var vm = (Asn1DocumentVM)tabItem.Content;
            Close(vm);
        }
    }
    Boolean canCloseTab(Object o) {
        // TODO: need to eliminate explicit reference to UI elements
        return o is ClosableTabItem;
    }

    #region Read content to tab
    Task openFileAsync(Object obj, CancellationToken token = default) {
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
    #endregion

    #region Write tab to file
    // 'o' parameter can receive:
    // null - use current tab with default name
    // 1    - use current tab with custom name
    // 2    - save all tabs with default name.
    void saveFile(Object obj) {
        if (obj == null) {
            writeFile(SelectedTab);
        } else {
            switch (obj.ToString()) {
                case "1": {
                    if (getSaveFilePath(out String filePath)) {
                        writeFile(SelectedTab, filePath);
                    }

                    break;
                }
                case "2":
                    // do something with save all tabs
                    break;
            }
        }
    }
    Boolean canPrintSave(Object obj) {
        return SelectedTab?.DataSource.RawData.Count > 0;
    }

    // general method to write arbitrary tab to a file.
    static Boolean writeFile(Asn1DocumentVM tab, String filePath = null) {
        // use default path if no custom file path specified
        filePath ??= tab.Path;
        // if file path is still null, then it came from "untitled" tab with default file path
        // so prompt for file to save and abort if cancelled.
        if (String.IsNullOrEmpty(filePath) && !getSaveFilePath(out filePath)) {
            return false;
        }
        try {
            File.WriteAllBytes(filePath, tab.DataSource.RawData.ToArray());
            tab.Path = filePath;
            tab.IsModified = false;

            return true;
        } catch (Exception e) {
            Tools.MsgBox("Save Error", e.Message);
        }
        return false;
    }
    static Boolean getSaveFilePath(out String saveFilePath) {
        return Tools.TryGetSaveFileName(out saveFilePath);
    }

    public Boolean RequestFileSave(Asn1DocumentVM tab) {
        MessageBoxResult result = Tools.MsgBox(
            "Unsaved Data",
            "Current file was modified. Save changes?",
            MessageBoxImage.Warning,
            MessageBoxButton.YesNoCancel);
        switch (result) {
            case MessageBoxResult.No:
                return true;
            case MessageBoxResult.Yes:
                return writeFile(tab);
            default:
                return false;
        }
    }
    #endregion
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
            await tab.Decode(bytes, true);
        } catch (Exception ex) {
            Tools.MsgBox("Read Error", ex.Message);
            return;
        }
        addTabToList(tab);
    }

    public void Close(Asn1DocumentVM tab) {
        if (!tab.IsModified) {
            Tabs.Remove(tab);
        }
        if (tab.IsModified && RequestFileSave(tab)) {
            Tabs.Remove(tab);
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
        await Tabs[0].Decode(rawBytes, false);
    }
}