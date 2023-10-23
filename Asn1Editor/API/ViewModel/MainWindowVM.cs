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
        CloseAllTabsCommand = new RelayCommand(closeAllTabs);
        CloseAllButThisTabCommand = new RelayCommand(closeAllButThisTab, canCloseAllButThisTab);
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

    public ICommand NewCommand { get; }
    public ICommand CloseTabCommand { get; }
    public ICommand CloseAllTabsCommand { get; }
    public ICommand CloseAllButThisTabCommand { get; }
    public IAsyncCommand OpenCommand { get; }
    public ICommand SaveCommand { get; }
    public ICommand PrintCommand { get; }
    public ICommand SettingsCommand { get; }
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

    void showConverter(Object o) {
        if (SelectedTab == null) {
            _windowFactory.ShowConverterWindow(Array.Empty<Byte>(), openRawAsync);
        } else {
            _windowFactory.ShowConverterWindow(SelectedTab.DataSource.RawData, openRawAsync);
        }
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

    #region Close Tab(s)

    void closeTab(Object o) {
        if (o == null) {
            closeTab(SelectedTab);
        } else if (o is ClosableTabItem tabItem) { // TODO: need to eliminate explicit reference to UI elements
            var vm = (Asn1DocumentVM)tabItem.Content;
            closeTab(vm);
        }
    }
    Boolean canCloseTab(Object o) {
        // TODO: need to eliminate explicit reference to UI elements
        return o is null or ClosableTabItem;
    }
    void closeAllTabs(Object o) {
        CloseAllTabs();
    }
    void closeAllButThisTab(Object o) {
        if (o == null) {
            closeTabsWithPreservation(SelectedTab);
        } else if (o is ClosableTabItem tabItem) { // TODO: need to eliminate explicit reference to UI elements
            var vm = (Asn1DocumentVM)tabItem.Content;
            closeTabsWithPreservation(vm);
        }
    }
    Boolean canCloseAllButThisTab(Object o) {
        if (Tabs.Count == 0) {
            return false;
        }
        if (o == null) {
            return SelectedTab != null;
        }

        return true;
    }

    void closeTab(Asn1DocumentVM tab) {
        if (!tab.IsModified) {
            Tabs.Remove(tab);
        }
        if (tab.IsModified && RequestFileSave(tab)) {
            Tabs.Remove(tab);
        }
    }
    Boolean closeTabsWithPreservation(Asn1DocumentVM preservedTab = null) {
        // loop over a copy of tabs since we are going to update source collection in a loop
        var tabs = Tabs.ToList();
        foreach (Asn1DocumentVM tab in tabs) {
            if (preservedTab != null && Equals(tab, preservedTab)) {
                continue;
            }
            if (!tab.IsModified) {
                Tabs.Remove(tab);

                continue;
            }
            SelectedTab = tab;
            if (!RequestFileSave(tab)) {
                return false;
            }
            Tabs.Remove(tab);
        }

        return true;
    }
    public Boolean CloseAllTabs() {
        return closeTabsWithPreservation();
    }

    #endregion

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
    Task openRawAsync(Byte[] rawBytes) {
        var asn = new Asn1Reader(rawBytes);
        asn.BuildOffsetMap();
        var tab = new Asn1DocumentVM(NodeViewOptions, TreeCommands);
        Tabs.Add(tab);
        SelectedTab = tab;
        return tab.Decode(rawBytes, false);
    }
}