using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;

namespace SysadminsLV.Asn1Editor.API.Generic {
	public interface IObservableList<T> : IList<T>, INotifyCollectionChanged, INotifyPropertyChanged { }
}