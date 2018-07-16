using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Threading.Tasks;
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
        String path;
        Asn1TreeNode selectedNode;
        Boolean hasClipboard, isBusy;

        public MainWindowVM(IAppCommands appCommands, ITreeCommands treeCommands, IDataSource data) {
            AppCommands = appCommands;
            TreeCommands = treeCommands;
            DataSource = data;
            TreeCommands2 = new TreeViewCommands(this);
            OpenCommand = new RelayCommand(openFile);
            SaveCommand = new RelayCommand(saveFile, canPrintSave);
            ShowConverterCommand = new RelayCommand(StaticCommands.ShowConverter);
            Settings.Default.PropertyChanged += onSettingChange;
        }

        public ICommand OpenCommand { get; set; }
        public ICommand SaveCommand { get; set; }
        public ICommand PrintCommand { get; set; }
        public ICommand ShowConverterCommand { get; set; }
        public ICommand SettingsCommand { get; set; }

        public IAppCommands AppCommands { get; }
        public ITreeCommands TreeCommands { get; }
        public TreeViewCommands TreeCommands2 { get; set; }

        public IDataSource DataSource { get; }
        public static Dictionary<String, String> OIDs { get; } = new Dictionary<String, String>();
        public ObservableCollection<Asn1TreeNode> Tree => DataSource.Tree;

        public Int32 FileLength => DataSource.RawData.Count;
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
            OpenRaw(Convert.FromBase64String(base64String));
        }
        public void OpenRaw(IEnumerable<Byte> rawBytes) {
            Path = null;
            DataSource.RawData.Clear();
            DataSource.RawData.AddRange(rawBytes);
            decode();
        }

        void onSettingChange(Object sender, PropertyChangedEventArgs e) {
            if (Tree.Count == 0) { return; }
            switch (e.PropertyName) {
                case nameof(Settings.IntAsInt):
                    StaticCommands.UpdateSettingsInteger(Tree[0], DataSource.RawData);
                    break;
                case nameof(Settings.DecodePayload):
                    StaticCommands.UpdateSettingsDecode(Tree[0]);
                    break;
            }
        }
    }
}
