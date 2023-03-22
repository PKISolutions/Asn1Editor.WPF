using System;
using System.Collections.Specialized;
using SysadminsLV.WPF.OfficeTheme.Toolkit.CLR;

namespace SysadminsLV.Asn1Editor.API.Interfaces; 

public interface IBinarySource : INotifyCollectionChanged {
    ObservableList<Byte> RawData { get; }
}