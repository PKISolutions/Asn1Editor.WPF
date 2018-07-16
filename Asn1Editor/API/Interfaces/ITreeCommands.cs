using System.Windows.Input;

namespace SysadminsLV.Asn1Editor.API.Interfaces {
    public interface ITreeCommands {
        ICommand ShowNodeTextViewer { get; }
        ICommand EditNodeCommand { get; }
        ICommand NewNodeCommand { get; }
        ICommand DeleteNodeCommand { get; }
        ICommand CutNodeCommand { get; }
        ICommand CopyNodeCommand { get; }
        ICommand PasteBeforeCommand { get; }
        ICommand PasteAfterCommand { get; }
        ICommand PasteLastCommand { get; }
    }
}