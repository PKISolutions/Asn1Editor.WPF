using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SysadminsLV.Asn1Editor.API.Interfaces;
using SysadminsLV.Asn1Editor.API.ModelObjects;
using SysadminsLV.Asn1Parser;
using Unity;

namespace SysadminsLV.Asn1Editor.API.Utils; 

class CertutilRenderer : ITextRenderer {
    readonly IDataSource _dataSource;
    readonly StringBuilder _sb = new StringBuilder(), line = new StringBuilder();
    readonly String nl = Environment.NewLine;
    readonly Asn1TreeNode _rootNode;
    readonly List<Byte> _headList = new List<Byte>(4);
    readonly HashSet<Byte> _noAsciiTags = new HashSet<Byte>(
        new[] {
                  (Byte)Asn1Type.BOOLEAN,
                  (Byte)Asn1Type.INTEGER,
                  (Byte)Asn1Type.OBJECT_IDENTIFIER,
              });
    readonly HashSet<Byte> _explicitTextTags = new HashSet<Byte>(
        new [] {
                   (Byte)Asn1Type.OBJECT_IDENTIFIER,
                   (Byte)Asn1Type.UTCTime,
                   (Byte)Asn1Type.GeneralizedTime,
                   (Byte)Asn1Type.PrintableString,
                   (Byte)Asn1Type.IA5String,
                   (Byte)Asn1Type.NumericString,
                   (Byte)Asn1Type.TeletexString,
                   (Byte)Asn1Type.VisibleString,
                   (Byte)Asn1Type.VideotexString,
                   (Byte)Asn1Type.UTF8String,
                   (Byte)Asn1Type.UniversalString,
                   (Byte)Asn1Type.BMPString,
               }
    );

    public CertutilRenderer(Asn1TreeNode node) {
        _rootNode = node;
        _dataSource = App.Container.Resolve<IDataSource>();
    }

    public String RenderText(Int32 textWidth) {
        _sb.Clear();
        if (_rootNode == null) {
            return _sb.ToString();
        }
        foreach (Asn1TreeNode node in _rootNode.Flatten()) {
            String leftPad = getLeftPad(node);
            writeTagHeader(node, leftPad);
            if (!node.Value.IsContainer && node.Value.PayloadLength > 0) {
                writeContent(node, leftPad);
            }
        }
        return _sb.ToString();
    }

    void writeTagHeader(Asn1TreeNode node, String leftPadString) {
        Asn1Lite value = node.Value;
        line.Clear();
        _headList.Clear();
        _headList.Add(value.Tag);
        _headList.AddRange(Asn1Utils.GetLengthBytes(value.PayloadLength));
        // write tag address
        line.AppendFormat("{0:x4}: ", value.Offset);
        // pad from
        line.Append(leftPadString);
        line.Append(AsnFormatter.BinaryToString(_headList.ToArray(), EncodingType.Hex, EncodingFormat.NOCRLF));
        if (line.Length < 48) {
            Int32 padLeft = 48 - line.Length;
            line.Append(new String(' ', padLeft));
        }
        line.AppendFormat("; {0} ({1:x} Bytes)", value.TagName, value.PayloadLength);
        line.Append(nl);
        _sb.Append(line);
    }
    void writeContent(Asn1TreeNode node, String leftPadString) {
        Asn1Lite value = node.Value;
        line.Clear();

        var lines = getHexTable(node);
        String padLeftContent = String.Empty;
        if (node.Parent != null) {
            padLeftContent = node.MyIndex < node.Parent.Children.Count - 1 ? "|  " : "   ";
        }

        for (Int32 i = 0; i < lines.Count; i++) {
            line.AppendFormat("{0:x4}: ", value.PayloadStartOffset + i * 16);
            // shift right nested content
            line.Append(leftPadString).Append(padLeftContent);
            if (!_noAsciiTags.Contains(value.Tag)) {
                Int32 index = lines[i].LastIndexOf("  ", StringComparison.InvariantCulture);
                if (index >= 0) {
                    lines[i] = lines[i].Insert(index + 1, ";");
                }
            }
            line.AppendLine(lines[i]);
        }
        // write decoded content
        if (_explicitTextTags.Contains(value.Tag)) {
            line.Append(new String(' ', 6)).Append(leftPadString).Append(padLeftContent);
            line.AppendLine($"   ; \"{value.ExplicitValue}\"");
        }
        _sb.Append(line);
    }
    List<String> getHexTable(Asn1TreeNode node) {
        Asn1Lite value = node.Value;

        Byte[] binValue = getTagBinaryValue(node.Value);
        Byte? highByte = null;
        if (node.Value.Tag == (Byte)Asn1Type.INTEGER && binValue[0] == 0) {
            highByte = 0;
            binValue = binValue.Skip(1).ToArray();
        }
        String strValue = AsnFormatter.BinaryToString(binValue, _noAsciiTags.Contains(value.Tag)
            ? EncodingType.Hex
            : EncodingType.HexAscii).TrimEnd();
        List<String> lines = strValue.Split(new[] { nl }, StringSplitOptions.RemoveEmptyEntries).ToList();
        if (node.Value.Tag == (Byte)Asn1Type.BIT_STRING) {
            lines.Insert(0, $"{node.Value.UnusedBits:x2} ; UNUSED BITS");
        } else if (highByte.HasValue) {
            lines.Insert(0, $"{highByte.Value:x2}");
        }

        return lines;
    }
    Byte[] getTagBinaryValue(Asn1Lite node) {
        Int32 skip = node.Tag == (Byte)Asn1Type.BIT_STRING ? node.PayloadStartOffset + 1 : node.PayloadStartOffset;
        Int32 take = node.Tag == (Byte)Asn1Type.BIT_STRING ? node.PayloadLength - 1 : node.PayloadLength;
        return _dataSource.RawData.Skip(skip).Take(take).ToArray();
    }
    String getLeftPad(Asn1TreeNode node) {
        if (node.Parent == null) {
            return String.Empty;
        }
        var sb = new StringBuilder();
        List<Int32> l = getParents(node);
        for (Int32 i = _rootNode.Value.Depth; i < node.Value.Depth; i++) {
            sb.Append(l.Contains(i) ? "|  " : "   ");
        }
        return sb.ToString();
    }

    static List<Int32> getParents(Asn1TreeNode node) {
        var depths = new List<Int32>();
        Asn1TreeNode n = node;

        while (n.Parent != null) {
            if (n.MyIndex < n.Parent.Children.Count - 1) {
                depths.Add(n.Value.Depth);
            }
            n = n.Parent;
        }
        return depths;
    }
}