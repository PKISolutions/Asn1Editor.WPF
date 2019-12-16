using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using SysadminsLV.Asn1Editor.API.Interfaces;
using SysadminsLV.Asn1Editor.Properties;
using SysadminsLV.Asn1Parser;
using Unity;

namespace SysadminsLV.Asn1Editor.API.ModelObjects {
    public class Asn1TreeNode {
        readonly IDataSource _dataSource;

        public Asn1TreeNode(Asn1Lite value) {
            _dataSource = App.Container.Resolve<IDataSource>();

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
        /// <summary>
        /// Gets the index of current node in parent's children collection.
        /// </summary>
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
            Value.IsContainer = true;
            if (indexToInsert < 0) { return; }
            nodeToInsert.Value.Offset = newOffset;
            Children.Insert(indexToInsert, nodeToInsert);
            notifySizeChanged(nodeToInsert, nodeToInsert.Value.TagLength);
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
        public IEnumerable<Asn1TreeNode> Flatten() {
            return new[] { this }.Union(Children.SelectMany(x => x.Flatten()));
        }

        public void UpdateNodeView(NodeViewOptions options) {
           updateNodeHeader(this, options); 
        }

        void updateNodeHeader(Asn1TreeNode node, NodeViewOptions options) {
            var outerList = new List<String>();
            var innerList = new List<String>();
            if (options.ShowNodePath) {
                outerList.Add($"({node.Path})");
            }
            if (options.ShowTagNumber) {
                innerList.Add(options.ShowInHex ? $"T:{node.Value.Tag:x2}" : $"T:{node.Value.Tag}");
            }
            if (options.ShowNodeOffset) {
                innerList.Add(options.ShowInHex ? $"O:{node.Value.Offset:x4}" : $"O:{node.Value.Offset}");
            }
            if (options.ShowNodeLength) {
                innerList.Add(options.ShowInHex ? $"L:{node.Value.PayloadLength:x4}" : $"L:{node.Value.PayloadLength}");
            }
            if (innerList.Count > 0) {
                outerList.Add("(" + String.Join(", ", innerList) + ")");
            }
            outerList.Add(node.Value.TagName);
            if (options.ShowContent) {
                if (!String.IsNullOrEmpty(node.Value.ExplicitValue)) {
                    outerList.Add(":");
                    outerList.Add(node.Value.ExplicitValue);
                }

            }
            node.Value.Header = String.Join(" ", outerList);
            node.Value.ToolTip = writeToolTip(node);
            foreach (Asn1TreeNode child in node.Children) {
                updateNodeHeader(child, options);
            }
        }
        String writeToolTip(Asn1TreeNode node) {
            var sb = new StringBuilder();
            sb.AppendFormat(
                Resources.TagEditorHeaderTemp,
                node.Value.Tag,
                node.Value.TagName,
                node.Value.Offset,
                node.Value.TagLength,
                node.Value.Deepness,
                node.Value.Path);
            sb.AppendLine();
            if (!node.Value.IsContainer) {
                sb.Append("Value:");
                if (node.Value.PayloadLength == 0) {
                    sb.AppendLine(" NULL");
                } else {
                    sb.AppendLine();
                    Int32 skip = node.Value.PayloadStartOffset;
                    Int32 take = node.Value.PayloadLength;
                    Boolean writeUnusedBits = false;
                    if (node.Value.Tag == (Byte)Asn1Type.BIT_STRING) {
                        skip++;
                        take--;
                        writeUnusedBits = true;
                    }
                    if (writeUnusedBits) {
                        sb.AppendLine($"Unused Bits: {node.Value.UnusedBits}");
                    }
                    Byte[] binData = _dataSource.RawData.Skip(skip).Take(take).ToArray();
                    sb.Append(binData.Length == 0
                        ? "EMPTY"
                        : AsnFormatter.BinaryToString(binData, EncodingType.Hex).TrimEnd());
                }
            }
            return sb.ToString();
        }

        void notifySizeChanged(Asn1TreeNode source, Int32 difference) {
            Asn1TreeNode t = this;
            do {
                if (t.Children.Count > 0) {
                    // update offset of every node below modified (source) node
                    Int32 callerIndex = t.Children.IndexOf(source);
                    if (callerIndex < 0) { return; }
                    for (Int32 index = callerIndex + 1; index < t.Children.Count; index++) {
                        t.Children[index].updateOffset(difference);
                    }
                }
                Byte[] newLenBytes = Asn1Utils.GetLengthBytes(t.Value.PayloadLength + difference);
                _dataSource.RawData.RemoveRange(t.Value.Offset + 1, t.Value.HeaderLength - 1);
                _dataSource.RawData.InsertRange(t.Value.Offset + 1, newLenBytes);
                // check if extra length byte is added. If so, add this byte to difference variable
                Int32 diff = newLenBytes.Length - (t.Value.HeaderLength - 1);
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
                child.updateOffset(difference);
            }
        }
        void valuePropertyChanged(Object sender, PropertyChangedEventArgs e) {
            if (e.PropertyName == nameof(Asn1Lite.OffsetChange) && Parent != null && Value.OffsetChange != 0)
                Parent.notifySizeChanged(this, Value.OffsetChange);
        }

        static void updatePath(Asn1TreeNode source, String path, Int32 index) {
            source.Value.Path = source.Path = path + "/" + index;
            source.MyIndex = index;
            Int32 deepness = source.Value.Path.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries).Length;
            source.Value.Deepness = deepness;
            for (Int32 i = 0; i < source.Children.Count; i++) {
                updatePath(source.Children[i], source.Path, i);
            }
        }

        #region Equals
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
        #endregion
    }
}
