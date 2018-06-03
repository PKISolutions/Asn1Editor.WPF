using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Microsoft.Win32;
using SysadminsLV.Asn1Editor.API.Generic;
using SysadminsLV.Asn1Editor.API.ModelObjects;
using SysadminsLV.Asn1Editor.API.Utils;
using SysadminsLV.Asn1Editor.API.Utils.ASN;
using SysadminsLV.Asn1Editor.Properties;
using SysadminsLV.WPF.OfficeTheme.Toolkit.Commands;

namespace SysadminsLV.Asn1Editor.API.ViewModel {
    class MainWindowVM : ViewModelBase {
        String path;
        Asn1TreeNode selectedNode;
        Visibility hexVisible;
        Boolean hexChecked, hasClipboard, isBusy;

        public MainWindowVM() {
            RawData = new ObservableList<Byte> { IsNotifying = true };
            Tree = new ObservableCollection<Asn1TreeNode>();
            HexViewerContext = new HexViewerVM { ParentVM = this };
            TreeCommands = new TreeViewCommands(this);
            OpenCommand = new RelayCommand(OpenFile);
            SaveCommand = new RelayCommand(SaveFile, CanPrintSave);
            HexViewerChanged = new RelayCommand(ChangeHexViewer);
            ShowConverterCommand = new RelayCommand(StaticCommands.ShowConverter);
            SettingsCommand = new RelayCommand(StaticCommands.ShowSettings);
            AboutCommand = new RelayCommand(StaticCommands.ShowAbout);
            Settings.Default.PropertyChanged += onSettingChange;
            initialize();
        }

        public ICommand OpenCommand { get; set; }
        public ICommand SaveCommand { get; set; }
        public ICommand PrintCommand { get; set; }
        public ICommand HexViewerChanged { get; set; }
        public ICommand ShowConverterCommand { get; set; }
        public ICommand SettingsCommand { get; set; }
        public ICommand AboutCommand { get; set; }

        public HexViewerVM HexViewerContext { get; set; }
        public TreeViewCommands TreeCommands { get; set; }

        public static ObservableList<Byte> RawData { get; set; }
        public static Dictionary<String, String> OIDs { get; set; }
        public ObservableCollection<Asn1TreeNode> Tree { get; set; }

        public String Path {
            get => path;
            set {
                path = value;
                OnPropertyChanged("Path");
                OnPropertyChanged("Title");
            }
        }
        public String Title => String.IsNullOrEmpty(Path)
            ? "ASN.1 Editor"
            : "ASN.1 Editor " + new FileInfo(Path).Name;
        public Boolean HasClipboardData {
            get => hasClipboard;
            set {
                hasClipboard = value;
                OnPropertyChanged("HasClipboardData");
            }
        }
        public Asn1TreeNode SelectedTreeNode {
            get => selectedNode;
            set {
                selectedNode = value;
                OnPropertyChanged("SelectedTreeNode");
                if (selectedNode != null) {
                    HexViewerContext.View.SelectHexView(selectedNode.Value);
                }
            }
        }
        public Visibility HexViewerVisible {
            get => hexVisible;
            set {
                hexVisible = value;
                OnPropertyChanged("HexViewerVisible");
            }
        }
        public Boolean HexViewerChecked {
            get => hexChecked;
            set {
                hexChecked = value;
                OnPropertyChanged("HexViewerChecked");
            }
        }
        public Boolean IsBusy {
            get => isBusy;
            set {
                isBusy = value;
                OnPropertyChanged("IsBusy");
            }
        }

        void initialize() {
            HexViewerVisible = Visibility.Collapsed;
            HexViewerChecked = false;
        }
        void OpenFile(Object obj) {
            OpenFileDialog dlg = new OpenFileDialog {
                FileName = "",
                DefaultExt = ".*",
                Filter = "All files (*.*)|*.*"
            };
            Boolean? result = dlg.ShowDialog();
            if (result != true) { return; }
            OpenExisting(dlg.FileName);
        }
        void SaveFile(Object obj) {
            if (obj != null || String.IsNullOrEmpty(Path)) {
                if (!GetFilePath()) { return; }
            }
            try {
                File.WriteAllBytes(Path, RawData.ToArray());
            } catch (Exception e) {
                Tools.MsgBox("Save Error", e.Message);
            }
        }
        async void Decode() {
            IsBusy = true;
            if (await DecodeFile()) {
                Tree.Clear();
                try {
                    Asn1TreeNode rootNode = await AsnTreeBuilder.BuildTree(RawData.ToArray());
                    Tree.Add(rootNode);
                    HexViewerContext.BuildHexView(RawData.ToArray());
                } catch (Exception e) {
                    Tools.MsgBox("Error", e.Message);
                }
            }
            IsBusy = false;	
        }

        
        static Boolean CanPrintSave(Object obj) {
            return RawData.Count > 0;
        }
        Boolean GetFilePath() {
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
        void ChangeHexViewer(Object obj) {
            HexViewerVisible = HexViewerChecked
                ? Visibility.Visible
                : Visibility.Collapsed;
        }
        async Task<Boolean> DecodeFile() {
            if (RawData.Count > 0) {
                return true;
            }
            try {
                RawData.AddRange(await FileUtility.FileToBinary(Path));
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
            RawData.Clear();
            Decode();
        }
        public void OpenExisting(String filePath) {
            Path = filePath;
            RawData.Clear();
            Decode();
        }
        public void OpenRaw(String base64String) {
            OpenRaw(Convert.FromBase64String(base64String));
        }
        public void OpenRaw(IEnumerable<Byte> rawBytes) {
            Path = null;
            RawData.Clear();
            RawData.AddRange(rawBytes);
            Decode();
        }

        void onSettingChange(Object sender, PropertyChangedEventArgs e) {
            if (Tree.Count == 0) { return; }
            switch (e.PropertyName) {
                case "IntAsInt":
                    StaticCommands.UpdateSettingsInteger(Tree[0], RawData);
                    break;
                case "DecodePayload":
                    StaticCommands.UpdateSettingsDecode(Tree[0]);
                    break;
            }
        }
    }
}
