using System;
using System.Windows.Input;
using SysadminsLV.Asn1Editor.API.Interfaces;
using SysadminsLV.Asn1Editor.API.ModelObjects;
using SysadminsLV.WPF.OfficeTheme.Toolkit.Commands;

namespace SysadminsLV.Asn1Editor.API.ViewModel {
    class TreeNodeCommands : ITreeCommands {
        readonly IWindowFactory _windowFactory;

        public TreeNodeCommands(IWindowFactory windowFactory) {
            _windowFactory = windowFactory;
            ShowNodeTextViewer = new RelayCommand(showNodeTextViewer);
        }

        public ICommand ShowNodeTextViewer { get; }

        void showNodeTextViewer(Object o) {
            if (!(o is Asn1TreeNode)) { return; }
            _windowFactory.ShowNodeTextViewer((Asn1TreeNode)o);
        }
    }
}
