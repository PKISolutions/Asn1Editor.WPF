using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Asn1Editor.API.ModelObjects;
using SysadminsLV.Asn1Parser;

namespace Asn1Editor.API.Utils.ASN {
    public static class AsnTreeBuilder {
        public static Task<Asn1TreeNode> BuildTree(Byte[] rawData) {
            return Task.Factory.StartNew(() => {
                Asn1Reader asn = new Asn1Reader(rawData);
                asn.BuildOffsetMap();
                Asn1Lite root = new Asn1Lite(asn);
                Asn1TreeNode parent = new Asn1TreeNode(root);
                if (asn.NextOffset == 0) {
                    return parent;
                }
                List<Asn1TreeNode> list = new List<Asn1TreeNode> { parent };
                buildTree(asn, parent);
                return list[0];
            });
        }
        static void buildTree(Asn1Reader root, Asn1TreeNode tree) {
            root.MoveNext();
            Int32 index = 0;
            do {
                tree.AddChild(new Asn1Lite(root, tree, index));
                index++;
            } while (root.MoveNextCurrentLevel());
            root.Reset();
            foreach (Asn1TreeNode node in tree.Children.Where(node => node.Value.IsContainer)) {
                root.MoveToPoisition(node.Value.Offset);
                buildTree(root, node);
            }
        }
    }
}
