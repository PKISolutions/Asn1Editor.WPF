using System;
using System.Collections.ObjectModel;
using SysadminsLV.Asn1Editor.API.ModelObjects;
using SysadminsLV.Asn1Editor.Views.UserControls.HexViewer;

namespace SysadminsLV.Asn1Editor.API.Interfaces {
    public interface IDataSource : IBinarySource {
        Boolean HasClipboardData { get; set; }
        Asn1TreeNode SelectedNode { get; set; }
        NodeViewOptions NodeViewOptions { get; }
        ObservableCollection<Asn1TreeNode> Tree { get; }

        void FinishBinaryUpdate();
    }
}