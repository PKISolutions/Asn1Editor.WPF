using System;
using System.Windows.Input;
using SysadminsLV.Asn1Editor.API.Interfaces;
using SysadminsLV.WPF.OfficeTheme.Toolkit.Commands;

namespace SysadminsLV.Asn1Editor.API.ViewModel {
    class TreeNodeCommands : ITreeCommands {
        readonly IWindowFactory _windowFactory;
        readonly IDataSource _data;

        public TreeNodeCommands(IWindowFactory windowFactory, IDataSource data) {
            _windowFactory = windowFactory;
            _data = data;
            ShowNodeTextViewer = new RelayCommand(showNodeTextViewer, canExecuteTreeCommands);
        }

        public ICommand ShowNodeTextViewer { get; }
        public ICommand EditNodeCommand { get; set; }
        public ICommand SaveNodeCommand { get; set; }
        public ICommand NewNodeCommand { get; set; }
        public ICommand DeleteNodeCommand { get; set; }
        public ICommand CutNodeCommand { get; set; }
        public ICommand CopyNodeCommand { get; set; }
        public ICommand PasteBeforeCommand { get; set; }
        public ICommand PasteAfterCommand { get; set; }
        public ICommand PasteLastCommand { get; set; }

        void showNodeTextViewer(Object o) {
            _windowFactory.ShowNodeTextViewer();
        }
        Boolean canExecuteTreeCommands(Object o) {
            return _data.SelectedNode != null;
        }
    }
}
