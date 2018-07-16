using System;
using System.Collections.Specialized;
using SysadminsLV.Asn1Editor.API.Generic;

namespace SysadminsLV.Asn1Editor.Views.UserControls.HexViewer {
    public interface IBinarySource : INotifyCollectionChanged {
        ObservableList<Byte> RawData { get; }
    }
}