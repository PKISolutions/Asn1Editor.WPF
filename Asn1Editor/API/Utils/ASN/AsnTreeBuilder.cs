using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SysadminsLV.Asn1Editor.API.Interfaces;
using SysadminsLV.Asn1Editor.API.ModelObjects;
using SysadminsLV.Asn1Parser;

namespace SysadminsLV.Asn1Editor.API.Utils.ASN;

public static class AsnTreeBuilder {
    public static Task<Asn1TreeNode> BuildTree(Byte[] rawData, IDataSource dataSource) {
        return Task.Factory.StartNew(() => {
                                         var asn = new Asn1Reader(rawData);
                                         asn.BuildOffsetMap();
                                         var root = new Asn1Lite(asn);
                                         var parent = new Asn1TreeNode(root, dataSource);
                                         if (asn.NextOffset == 0) {
                                             return parent;
                                         }
                                         var list = new List<Asn1TreeNode> { parent };
                                         buildTree(asn, parent);
                                         return list[0];
                                     });
    }

    public static Task<Asn1TreeNode> BuildTree(IDataSource dataSource) {
        return BuildTree(dataSource.RawData.ToArray(), dataSource);
    }
    static void buildTree(Asn1Reader root, Asn1TreeNode tree) {
        root.MoveNext();
        Int32 index = 0;
        do {
            tree.AddChild(new Asn1Lite(root, tree, index));
            index++;
        } while (root.MoveNextSibling());
        root.Reset();
        foreach (Asn1TreeNode node in tree.Children.Where(node => node.Value.IsContainer && node.Value.PayloadLength > 0)) {
            root.Seek(node.Value.Offset);
            buildTree(root, node);
        }
    }
}