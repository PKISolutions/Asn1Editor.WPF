using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using SysadminsLV.Asn1Editor.API.Interfaces;
using SysadminsLV.Asn1Editor.API.Utils;
using SysadminsLV.Asn1Editor.API.ViewModel;
using SysadminsLV.Asn1Parser;
using Unity;

namespace SysadminsLV.Asn1Editor.API.ModelObjects {
    public class Asn1TreeNode {
        readonly ObservableCollection<Asn1TreeNode> _children = new ObservableCollection<Asn1TreeNode>();

        public Asn1TreeNode(Asn1Lite value) {
            Value = value;
            Path = value.Path;
            String[] tokens = Path.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
            MyIndex = String.IsNullOrEmpty(Path)
                ? 0
                : Convert.ToInt32(tokens[tokens.Length - 1]);
            Value.PropertyChanged += valuePropertyChanged;
        }

        public Asn1TreeNode this[Int32 index] => _children[index];
        public String Path { get; set; }
        public Int32 MyIndex { get; set; }
        public Asn1TreeNode Parent { get; private set; }
        public Asn1Lite Value { get; }
        public ObservableCollection<Asn1TreeNode> Children => _children;
        public void InsertChildNode(Asn1TreeNode nodeToInsert, Asn1TreeNode caller, NodeAddOption option) {
            Int32 indexToInsert, newOffset;
            nodeToInsert.Parent = this;
            nodeToInsert.Value.IsRoot = false;
            Int32 headerLength = nodeToInsert.Value.HeaderLength;
            switch (option) {
                case NodeAddOption.Before:
                    indexToInsert = _children.IndexOf(caller);
                    newOffset = caller.Value.Offset;
                    break;
                case NodeAddOption.After:
                    indexToInsert = _children.IndexOf(caller) + 1;
                    newOffset = caller.Value.Offset + caller.Value.TagLength;
                    break;
                case NodeAddOption.Last:
                    indexToInsert = _children.Count;
                    newOffset = Value.Offset + Value.TagLength;
                    break;
                default:
                    return;
            }
            //TODO verify code
            Value.IsContainer = true;
            if (indexToInsert < 0) { return; }
            App.Container.Resolve<IDataSource>().RawData.InsertRange(newOffset, ClipboardManager.GetClipboardBytes());
            _children.Insert(indexToInsert, nodeToInsert);
            NotifySizeChanged(nodeToInsert, nodeToInsert.Value.TagLength);
            _children[indexToInsert].Value.Offset = newOffset;
            _children[indexToInsert].Value.PayloadStartOffset = newOffset + headerLength;
            for (Int32 index = indexToInsert; index < Children.Count; index++) {
                UpdatePath(Children[index], Path, index);
            }
            foreach (Asn1TreeNode child in _children[indexToInsert].Children) {
                child.UpdateOffset(newOffset);
            }
            ((MainWindowVM)Application.Current.MainWindow.DataContext).HexViewerContext.BuildHexView(null);
        }
        public void AddChild(Asn1Lite value, Boolean forcePathUpdate = false) {
            var node = new Asn1TreeNode(value) { Parent = this };
            _children.Add(node);
            if (forcePathUpdate) {
                NotifySizeChanged(node, node.Value.TagLength);
                UpdatePath(node, Path, _children.Count - 1);
            }
            Value.IsContainer = true;
        }
        public void AddNewNode(Asn1Lite value) {
            value.Offset = Value.Offset + Value.TagLength;
            var node = new Asn1TreeNode(value) { Parent = this };
            _children.Add(node);
            NotifySizeChanged(node, Value.OffsetChange);
            Value.IsContainer = true;
            ((MainWindowVM)Application.Current.MainWindow.DataContext).HexViewerContext.BuildHexView(null);
        }
        public void RemoveChild(Asn1TreeNode node) {
            //Int32 indexToRemove = Children.IndexOf(node);
            //if (indexToRemove < 0) { return; } // TODO: is it necessary?
            Int32 difference = Children[node.MyIndex].Value.TagLength;
            App.Container.Resolve<IDataSource>().RawData.RemoveRange(node.Value.Offset, difference);
            NotifySizeChanged(node, -difference);
            _children.RemoveAt(node.MyIndex);
            ((MainWindowVM)Application.Current.MainWindow.DataContext).HexViewerContext.BuildHexView(null);
            // update path only below removed node
            for (Int32 childIndex = node.MyIndex; childIndex < Children.Count; childIndex++) {
                UpdatePath(this[childIndex], Path, childIndex);
            }
            if (_children.Count == 0 && !Value.IsRoot) {
                Value.IsContainer = false;
            }
        }
        public IEnumerable<Asn1Lite> Flatten() {
            return new[] { Value }.Union(_children.SelectMany(x => x.Flatten()));
        }

        void NotifySizeChanged(Asn1TreeNode source, Int32 difference) {
            Asn1TreeNode t = this;
            do {
                if (t.Children.Count > 0) {
                    Int32 callerIndex = t.Children.IndexOf(source);
                    if (callerIndex < 0) { return; }
                    for (Int32 index = callerIndex + 1; index < t.Children.Count; index++) {
                        t.Children[index].UpdateOffset(difference);
                    }
                }
                Byte[] newLenBytes = Asn1Utils.GetLengthBytes(t.Value.PayloadLength + difference);
                App.Container.Resolve<IDataSource>().RawData.RemoveRange(t.Value.Offset + 1, t.Value.HeaderLength - 1);
                App.Container.Resolve<IDataSource>().RawData.InsertRange(t.Value.Offset + 1, newLenBytes);
                Int32 diff = newLenBytes.Length - (t.Value.HeaderLength - 1);
                difference += diff;
                if (diff != 0) {
                    t.UpdateOffset(diff);
                }
                source = t;
                t = t.Parent;
                source.Value.PayloadLength += difference;
            } while (t != null);
        }
        void UpdateOffset(Int32 difference) {
            Value.Offset += difference;
            Value.PayloadStartOffset += difference;
            foreach (Asn1TreeNode children in Children) {
                children.UpdateOffset(difference);
            }
        }
        void valuePropertyChanged(Object sender, PropertyChangedEventArgs e) {
            if (e.PropertyName == "OffsetChange") {
                if (Parent != null) {
                    if (Value.OffsetChange != 0) {
                        Parent.NotifySizeChanged(this, Value.OffsetChange);
                    }
                    ((MainWindowVM)Application.Current.MainWindow.DataContext).HexViewerContext.BuildHexView(null);
                }
            }
        }

        static void UpdatePath(Asn1TreeNode source, String path, Int32 index) {
            source.Value.Path = source.Path = path + "/" + index;
            source.MyIndex = index;
            Int32 deepness = source.Value.Path.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries).Length;
            source.Value.Deepness = deepness;
            for (Int32 Index = 0; Index < source.Children.Count; Index++) {
                UpdatePath(source.Children[Index], source.Path, Index);
            }
        }

        protected Boolean Equals(Asn1TreeNode other) {
            return String.Equals(Path, other.Path);
        }
        public override Int32 GetHashCode() {
            return Path != null ? Path.GetHashCode() : 0;
        }
        public override Boolean Equals(Object obj) {
            if (ReferenceEquals(null, obj)) { return false; }
            if (ReferenceEquals(this, obj)) { return true; }
            return obj.GetType() == GetType() && Equals((Asn1TreeNode) obj);
        }
    }
}
