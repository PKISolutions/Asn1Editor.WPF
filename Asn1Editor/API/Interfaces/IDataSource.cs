using System.Collections.ObjectModel;
using SysadminsLV.Asn1Editor.API.ModelObjects;

namespace SysadminsLV.Asn1Editor.API.Interfaces;

public interface IDataSource : IBinarySource {
    Asn1TreeNode SelectedNode { get; set; }
    NodeViewOptions NodeViewOptions { get; }
    ObservableCollection<Asn1TreeNode> Tree { get; }

    void FinishBinaryUpdate();
}