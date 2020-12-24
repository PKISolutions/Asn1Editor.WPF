using System;
using System.Collections.Specialized;
using SysadminsLV.WPF.OfficeTheme.Toolkit.CLR;

namespace Asn1Editor.Wpf.Controls {
    public interface IBinarySource : INotifyCollectionChanged {
        ObservableList<Byte> RawData { get; }
    }
}