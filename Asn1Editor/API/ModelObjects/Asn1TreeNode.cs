using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using SysadminsLV.Asn1Editor.API.Interfaces;
using SysadminsLV.Asn1Editor.API.Utils;
using SysadminsLV.Asn1Parser;
using Unity;

namespace SysadminsLV.Asn1Editor.API.ModelObjects {
    public class Asn1TreeNode {
        public Asn1TreeNode(Asn1Lite value) {
            Value = value;
            Path = value.Path;
            String[] tokens = Path.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
            MyIndex = String.IsNullOrEmpty(Path)
                ? 0
                : Convert.ToInt32(tokens[tokens.Length - 1]);
            Value.PropertyChanged += valuePropertyChanged;
        }

        public Asn1TreeNode this[Int32 index] => Children[index];
        public String Path { get; set; }
        public Int32 MyIndex { get; set; }
        public Asn1TreeNode Parent { get; private set; }
        public Asn1Lite Value { get; }
        public ObservableCollection<Asn1TreeNode> Children { get; } = new ObservableCollection<Asn1TreeNode>();

        public void InsertChildNode(Asn1TreeNode nodeToInsert, Asn1TreeNode caller, NodeAddOption option) {
            Int32 indexToInsert, newOffset;
            nodeToInsert.Parent = this;
            nodeToInsert.Value.IsRoot = false;
            //Int32 headerLength = nodeToInsert.Value.HeaderLength;
            switch (option) {
                case NodeAddOption.Before:
                    indexToInsert = Children.IndexOf(caller);
                    newOffset = caller.Value.Offset;
                    break;
                case NodeAddOption.After:
                    indexToInsert = Children.IndexOf(caller) + 1;
                    newOffset = caller.Value.Offset + caller.Value.TagLength;
                    break;
                case NodeAddOption.Last:
                    indexToInsert = Children.Count;
                    newOffset = Value.Offset + Value.TagLength;
                    break;
                default:
                    return;
            }
            //TODO verify code
            Value.IsContainer = true;
            if (indexToInsert < 0) { return; }
            var data = App.Container.Resolve<IDataSource>();
            data.RawData.InsertRange(newOffset, ClipboardManager.GetClipboardBytes());
            Children.Insert(indexToInsert, nodeToInsert);
            notifySizeChanged(nodeToInsert, nodeToInsert.Value.TagLength);
            Children[indexToInsert].Value.Offset = newOffset;
            for (Int32 index = indexToInsert; index < Children.Count; index++) {
                updatePath(Children[index], Path, index);
            }
            foreach (Asn1TreeNode child in Children[indexToInsert].Children) {
                child.updateOffset(newOffset);
            }
        }
        public void AddChild(Asn1Lite value, Boolean forcePathUpdate = false) {
            var node = new Asn1TreeNode(value) { Parent = this };
            Children.Add(node);
            if (forcePathUpdate) {
                notifySizeChanged(node, node.Value.TagLength);
                updatePath(node, Path, Children.Count - 1);
            }
            Value.IsContainer = true;
        }
        public void RemoveChild(Asn1TreeNode node) {
            notifySizeChanged(node, -node.Value.TagLength);
            Children.RemoveAt(node.MyIndex);
            // update path only below removed node
            for (Int32 childIndex = node.MyIndex; childIndex < Children.Count; childIndex++) {
                updatePath(this[childIndex], Path, childIndex);
            }
            if (Children.Count == 0 && !Value.IsRoot) {
                Value.IsContainer = false;
            }
        }
        public IEnumerable<Asn1Lite> Flatten() {
            return new[] { Value }.Union(Children.SelectMany(x => x.Flatten()));
        }

        void notifySizeChanged(Asn1TreeNode source, Int32 difference) {
            Asn1TreeNode t = this;
            do {
                if (t.Children.Count > 0) {
                    Int32 callerIndex = t.Children.IndexOf(source);
                    if (callerIndex < 0) { return; }
                    for (Int32 index = callerIndex + 1; index < t.Children.Count; index++) {
                        t.Children[index].updateOffset(difference);
                    }
                }
                Byte[] newLenBytes = Asn1Utils.GetLengthBytes(t.Value.PayloadLength + difference);
                App.Container.Resolve<IDataSource>().RawData.RemoveRange(t.Value.Offset + 1, t.Value.HeaderLength - 1);
                App.Container.Resolve<IDataSource>().RawData.InsertRange(t.Value.Offset + 1, newLenBytes);
                // check if extra length byte is added. If so, add this byte to difference variable
                Int32 diff = newLenBytes.Length - (t.Value.HeaderLength - 1);
                // update offset of every node below modified (source) node
                if (diff != 0) {
                    // shift payload start offset to extra length bytes
                    t.Value.PayloadStartOffset += diff;
                    t.updateOffset(diff);
                    t.Value.Offset -= diff; // TODO this: updateOffset method updates current node as well which is not neccessary
                }
                source = t;
                t = t.Parent;
                source.Value.PayloadLength += difference;
                difference += diff;
            } while (t != null);
        }
        void updateOffset(Int32 difference) {
            Value.Offset += difference;
            foreach (Asn1TreeNode child in Children) {
                //child.Value.Offset += difference;
                child.updateOffset(difference);
            }
        }
        void valuePropertyChanged(Object sender, PropertyChangedEventArgs e) {
            if (e.PropertyName == nameof(Asn1Lite.OffsetChange)) {
                if (Parent != null) {
                    if (Value.OffsetChange != 0) {
                        Parent.notifySizeChanged(this, Value.OffsetChange);
                    }
                }
            }
        }

        static void updatePath(Asn1TreeNode source, String path, Int32 index) {
            source.Value.Path = source.Path = path + "/" + index;
            source.MyIndex = index;
            Int32 deepness = source.Value.Path.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries).Length;
            source.Value.Deepness = deepness;
            for (Int32 Index = 0; Index < source.Children.Count; Index++) {
                updatePath(source.Children[Index], source.Path, Index);
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
            return obj.GetType() == GetType() && Equals((Asn1TreeNode)obj);
        }
    }
}
