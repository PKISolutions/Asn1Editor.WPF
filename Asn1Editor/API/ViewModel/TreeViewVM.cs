using SysadminsLV.Asn1Editor.API.Interfaces;

namespace SysadminsLV.Asn1Editor.API.ViewModel {
    class TreeViewVM : ViewModelBase, ITreeViewVM {
        public TreeViewVM(IDataSource data, ITreeCommands treeCommands) {
            DataSource = data;
            TreeCommands = treeCommands;
        }

        public IDataSource DataSource { get; }
        public ITreeCommands TreeCommands { get; }
    }
}
