using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Threading.Tasks;

namespace SysadminsLV.Asn1Editor.API.Interfaces;

public interface IBinarySource : INotifyCollectionChanged {
    IReadOnlyList<Byte> RawData { get; }

    Task InitializeFromRawData(IEnumerable<Byte> rawData);
}