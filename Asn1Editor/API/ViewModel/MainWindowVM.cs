using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using SysadminsLV.Asn1Editor.API.Interfaces;
using SysadminsLV.Asn1Editor.API.ModelObjects;
using SysadminsLV.Asn1Editor.API.Utils;
using SysadminsLV.Asn1Editor.API.Utils.ASN;
using SysadminsLV.WPF.OfficeTheme.Toolkit.Commands;

namespace SysadminsLV.Asn1Editor.API.ViewModel {
    class MainWindowVM : ViewModelBase, IMainWindowVM {
        readonly IWindowFactory _windowFactory;
        String path;
        Asn1TreeNode selectedNode;
        Boolean hasClipboard, isBusy;

        public MainWindowVM(
            IWindowFactory windowFactory,
            IAppCommands appCommands,
            ITreeCommands treeCommands,
            IDataSource data) {

            _windowFactory = windowFactory;
            AppCommands = appCommands;
            TreeCommands = treeCommands;
            DataSource = data;
            OpenCommand = new RelayCommand(openFile);
            SaveCommand = new RelayCommand(saveFile, canPrintSave);
            appCommands.ShowConverterWindow = new RelayCommand(showConverter);
            NodeViewOptions.PropertyChanged += onNodeViewOptionsChanged;
        }
        void onNodeViewOptionsChanged(Object sender, PropertyChangedEventArgs e) {
            if (Tree.Any()) {
                Tree[0].UpdateNodeView(NodeViewOptions);
            }
        }

        public ICommand OpenCommand { get; set; }
        public ICommand SaveCommand { get; set; }
        public ICommand PrintCommand { get; set; }
        public ICommand SettingsCommand { get; set; }

        public IAppCommands AppCommands { get; }
        public ITreeCommands TreeCommands { get; }

        public IDataSource DataSource { get; }
        public static Dictionary<String, String> OIDs { get; } = new Dictionary<String, String>();
        public ObservableCollection<Asn1TreeNode> Tree => DataSource.Tree;
        public NodeViewOptions NodeViewOptions => DataSource.NodeViewOptions;

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
            _windowFactory.ShowConverterWindow(DataSource.RawData, openRaw);
        }
        void openFile(Object obj) {
            String file = Tools.GetOpenFileName();
            if (String.IsNullOrWhiteSpace(file)) { return; }
            OpenExisting(file);
        }
        void saveFile(Object obj) {
            if ((obj != null || String.IsNullOrEmpty(Path)) && !getFilePath()) {
                return;
            }
            try {
                File.WriteAllBytes(Path, DataSource.RawData.ToArray());
            } catch (Exception e) {
                Tools.MsgBox("Save Error", e.Message);
            }
        }
        async void decode() {
            IsBusy = true;
            if (await decodeFile()) {
                Tree.Clear();
                try {
                    Asn1TreeNode rootNode = await AsnTreeBuilder.BuildTree(DataSource.RawData.ToArray());
                    Tree.Add(rootNode);
                    DataSource.FinishBinaryUpdate();
                } catch (Exception e) {
                    Tools.MsgBox("Error", e.Message);
                }
            }
            IsBusy = false;
        }


        Boolean canPrintSave(Object obj) {
            return DataSource.RawData.Count > 0;
        }
        Boolean getFilePath() {
            String p = Tools.GetSaveFileName();
            if (String.IsNullOrWhiteSpace(p.Trim())) { return false; }
            Path = p;
            return true;
        }
        async Task<Boolean> decodeFile() {
            if (DataSource.RawData.Count > 0) {
                return true;
            }
            try {
                DataSource.RawData.AddRange(await FileUtility.FileToBinary(Path));
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
            DataSource.RawData.Clear();
            decode();
        }
        public void OpenExisting(String filePath) {
            Path = filePath;
            DataSource.RawData.Clear();
            decode();
        }
        public void OpenRaw(String base64String) {
            openRaw(Convert.FromBase64String(base64String));
        }
        void openRaw(IEnumerable<Byte> rawBytes) {
            Path = null;
            DataSource.RawData.Clear();
            DataSource.RawData.AddRange(rawBytes);
            decode();
        }
    }
}
