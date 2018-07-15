using System;
using System.Collections.ObjectModel;
using SysadminsLV.Asn1Editor.API.Generic;
using SysadminsLV.Asn1Editor.API.ModelObjects;

namespace SysadminsLV.Asn1Editor.API.Interfaces {
    public interface IDataSource {
        Asn1TreeNode SelectedNode { get; set; }
        ObservableCollection<Asn1TreeNode> Tree { get; }
        ObservableList<Byte> RawData { get; }
    }
}