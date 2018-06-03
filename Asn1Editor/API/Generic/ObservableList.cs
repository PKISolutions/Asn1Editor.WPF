using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;

namespace SysadminsLV.Asn1Editor.API.Generic {
    public class ObservableList<T> : List<T>, IObservableList<T> {
        public ObservableList() {
            IsNotifying = true;
            CollectionChanged += delegate { OnPropertyChanged(nameof(Count)); };
        }

        public Boolean IsNotifying { get; set; }

        public new void Add(T item) {
            base.Add(item);
            var e = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item);
            OnCollectionChanged(e);
        }
        public new void AddRange(IEnumerable<T> collection) {
            IEnumerable<T> array = collection as T[] ?? collection.ToArray();
            base.AddRange(array);
            var e = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, new List<T>(array));
            OnCollectionChanged(e);
        }
        public new void Clear() {
            base.Clear();
            var e = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset);
            OnCollectionChanged(e);
        }
        public new void Insert(Int32 i, T item) {
            base.Insert(i, item);
            var e = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item);
            OnCollectionChanged(e);
        }
        public new void InsertRange(Int32 i, IEnumerable<T> collection) {
            base.InsertRange(i, collection);
            var e = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, collection);
            OnCollectionChanged(e);
        }
        public new void Remove(T item) {
            base.Remove(item);
            var e = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item);
            OnCollectionChanged(e);
        }
        public new void RemoveAll(Predicate<T> match) {
            List<T> backup = FindAll(match);
            base.RemoveAll(match);
            var e = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, backup);
            OnCollectionChanged(e);
        }
        public new void RemoveAt(Int32 i) {
            T backup = this[i];
            base.RemoveAt(i);
            var e = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, backup);
            OnCollectionChanged(e);
        }
        public new void RemoveRange(Int32 index, Int32 count) {
            List<T> backup = GetRange(index, count);
            base.RemoveRange(index, count);
            var e = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, backup);
            OnCollectionChanged(e);
        }
        public new T this[Int32 index] {
            get => base[index];
            set {
                T oldValue = base[index];
                NotifyCollectionChangedEventArgs e =
                    new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, value, oldValue);
                OnCollectionChanged(e);
            }
        }

        protected void OnCollectionChanged(NotifyCollectionChangedEventArgs e) {
            if (IsNotifying && CollectionChanged != null) {
                try {
                    CollectionChanged(this, e);
                } catch (NotSupportedException) {
                    NotifyCollectionChangedEventArgs alternativeEventArgs =
                        new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset);
                    OnCollectionChanged(alternativeEventArgs);
                }
            }
        }
        protected void OnPropertyChanged(String propertyName) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        public event PropertyChangedEventHandler PropertyChanged;
        public event NotifyCollectionChangedEventHandler CollectionChanged;
    }

}
