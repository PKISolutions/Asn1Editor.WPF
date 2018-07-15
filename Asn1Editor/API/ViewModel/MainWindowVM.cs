using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Microsoft.Win32;
using SysadminsLV.Asn1Editor.API.Interfaces;
using SysadminsLV.Asn1Editor.API.ModelObjects;
using SysadminsLV.Asn1Editor.API.Utils;
using SysadminsLV.Asn1Editor.API.Utils.ASN;
using SysadminsLV.Asn1Editor.Properties;
using SysadminsLV.WPF.OfficeTheme.Toolkit.Commands;

namespace SysadminsLV.Asn1Editor.API.ViewModel {
    class MainWindowVM : ViewModelBase, IMainWindowVM {
        readonly IDataSource _data;
        String path;
        Asn1TreeNode selectedNode;
        Visibility hexVisible;
        Boolean hexChecked, hasClipboard, isBusy;

        public MainWindowVM(IAppCommands appCommands, ITreeCommands treeCommands, IDataSource data) {
            AppCommands = appCommands;
            TreeCommands = treeCommands;
            _data = data;
            HexViewerContext = new HexViewerVM();
            TreeCommands2 = new TreeViewCommands(this);
            OpenCommand = new RelayCommand(openFile);
            SaveCommand = new RelayCommand(saveFile, canPrintSave);
            HexViewerChanged = new RelayCommand(changeHexViewer);
            ShowConverterCommand = new RelayCommand(StaticCommands.ShowConverter);
            Settings.Default.PropertyChanged += onSettingChange;
            initialize();
        }

        public ICommand OpenCommand { get; set; }
        public ICommand SaveCommand { get; set; }
        public ICommand PrintCommand { get; set; }
        public ICommand HexViewerChanged { get; set; }
        public ICommand ShowConverterCommand { get; set; }
        public ICommand SettingsCommand { get; set; }

        public IAppCommands AppCommands { get; }
        public ITreeCommands TreeCommands { get; }

        public HexViewerVM HexViewerContext { get; set; }
        public TreeViewCommands TreeCommands2 { get; set; }

        //public static ObservableList<Byte> RawData { get; } = new ObservableList<Byte> { IsNotifying = true };
        public static Dictionary<String, String> OIDs { get; } = new Dictionary<String, String>();
        public ObservableCollection<Asn1TreeNode> Tree => _data.Tree;

        public Int32 FileLength => _data.RawData.Count;
        public String Path {
            get => path;
            set {
                path = value;
                OnPropertyChanged(nameof(Path));
                OnPropertyChanged(nameof(Title));
            }
        }
        public String Title => String.IsNullOrEmpty(Path)
            ? "ASN.1 Editor"
            : "ASN.1 Editor - " + new FileInfo(Path).Name;
        public Boolean HasClipboardData {
            get => hasClipboard;
            set {
                hasClipboard = value;
                OnPropertyChanged(nameof(HasClipboardData));
            }
        }
        public Asn1TreeNode SelectedTreeNode {
            get => selectedNode;
            set {
                selectedNode = value;
                OnPropertyChanged(nameof(SelectedTreeNode));
                if (selectedNode != null) {
                    HexViewerContext.View.SelectHexView(selectedNode.Value);
                }
            }
        }
        public Visibility HexViewerVisible {
            get => hexVisible;
            set {
                hexVisible = value;
                OnPropertyChanged(nameof(HexViewerVisible));
            }
        }
        public Boolean HexViewerChecked {
            get => hexChecked;
            set {
                hexChecked = value;
                OnPropertyChanged(nameof(HexViewerChecked));
            }
        }
        public Boolean IsBusy {
            get => isBusy;
            set {
                isBusy = value;
                OnPropertyChanged(nameof(IsBusy));
            }
        }

        void initialize() {
            HexViewerVisible = Visibility.Collapsed;
            HexViewerChecked = false;
        }
        void openFile(Object obj) {
            OpenFileDialog dlg = new OpenFileDialog {
                FileName = "",
                DefaultExt = ".*",
                Filter = "All files (*.*)|*.*"
            };
            Boolean? result = dlg.ShowDialog();
            if (result != true) { return; }
            OpenExisting(dlg.FileName);
        }
        void saveFile(Object obj) {
            if (obj != null || String.IsNullOrEmpty(Path)) {
                if (!getFilePath()) { return; }
            }
            try {
                File.WriteAllBytes(Path, _data.RawData.ToArray());
            } catch (Exception e) {
                Tools.MsgBox("Save Error", e.Message);
            }
        }
        async void decode() {
            IsBusy = true;
            if (await decodeFile()) {
                Tree.Clear();
                try {
                    Asn1TreeNode rootNode = await AsnTreeBuilder.BuildTree(_data.RawData.ToArray());
                    Tree.Add(rootNode);
                    HexViewerContext.BuildHexView(_data.RawData.ToArray());
                } catch (Exception e) {
                    Tools.MsgBox("Error", e.Message);
                }
            }
            IsBusy = false;	
        }

        
        Boolean canPrintSave(Object obj) {
            return _data.RawData.Count > 0;
        }
        Boolean getFilePath() {
            SaveFileDialog dlg = new SaveFileDialog {
                FileName = "",
                DefaultExt = ".*",
                Filter = "All files (*.*)|*.*"
            };
            Boolean? result = dlg.ShowDialog();
            if (result != true) { return false; }
            Path = dlg.FileName;
            return true;
        }
        void changeHexViewer(Object obj) {
            HexViewerVisible = HexViewerChecked
                ? Visibility.Visible
                : Visibility.Collapsed;
        }
        async Task<Boolean> decodeFile() {
            if (_data.RawData.Count > 0) {
                return true;
            }
            try {
                _data.RawData.AddRange(await FileUtility.FileToBinary(Path));
                return true;
            } catch (Exception e) {
                Tools.MsgBox("Read Error", e.Message);
                Path = null;
                return false;
            }
        }
        public void DropFile(String filePath) {
            if (!File.Exists(filePath)) { return; }
            Path = filePath;
            _data.RawData.Clear();
            decode();
        }
        public void OpenExisting(String filePath) {
            Path = filePath;
            _data.RawData.Clear();
            decode();
        }
        public void OpenRaw(String base64String) {
            OpenRaw(Convert.FromBase64String(base64String));
        }
        public void OpenRaw(IEnumerable<Byte> rawBytes) {
            Path = null;
            _data.RawData.Clear();
            _data.RawData.AddRange(rawBytes);
            decode();
        }

        void onSettingChange(Object sender, PropertyChangedEventArgs e) {
            if (Tree.Count == 0) { return; }
            switch (e.PropertyName) {
                case nameof(Settings.IntAsInt):
                    StaticCommands.UpdateSettingsInteger(Tree[0], _data.RawData);
                    break;
                case nameof(Settings.DecodePayload):
                    StaticCommands.UpdateSettingsDecode(Tree[0]);
                    break;
            }
        }
    }
}
