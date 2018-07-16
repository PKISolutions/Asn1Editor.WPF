using System;
using System.Windows.Input;
using SysadminsLV.Asn1Editor.API.Interfaces;
using SysadminsLV.Asn1Editor.API.Utils;
using SysadminsLV.WPF.OfficeTheme.Toolkit.Commands;

namespace SysadminsLV.Asn1Editor.API.ViewModel {
    class TreeNodeCommands : ITreeCommands {
        readonly IWindowFactory _windowFactory;
        readonly IDataSource _data;

        public TreeNodeCommands(IWindowFactory windowFactory, IDataSource data) {
            _windowFactory = windowFactory;
            _data = data;
            ShowNodeTextViewer = new RelayCommand(showNodeTextViewer, canExecuteTreeCommands);
            EditNodeCommand = new RelayCommand(editNodeContent, canExecuteTreeCommands);
            NewNodeCommand = new RelayCommand(newNode);
        }

        public ICommand ShowNodeTextViewer { get; }
        public ICommand EditNodeCommand { get; }
        public ICommand SaveNodeCommand { get; }
        public ICommand NewNodeCommand { get; }
        public ICommand DeleteNodeCommand { get; }
        public ICommand CutNodeCommand { get; }
        public ICommand CopyNodeCommand { get; }
        public ICommand PasteBeforeCommand { get; }
        public ICommand PasteAfterCommand { get; }
        public ICommand PasteLastCommand { get; }

        void showNodeTextViewer(Object o) {
            _windowFactory.ShowNodeTextViewer();
        }
        void editNodeContent(Object o) {
            _windowFactory.ShowNodeContentEditor((NodeEditMode)o);
        }
        void newNode(Object o) {

        }
        Boolean canExecuteTreeCommands(Object o) {
            return _data.SelectedNode != null;
        }
    }
}
