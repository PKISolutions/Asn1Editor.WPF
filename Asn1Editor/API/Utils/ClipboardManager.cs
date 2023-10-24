using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SysadminsLV.Asn1Editor.API.Interfaces;
using SysadminsLV.Asn1Editor.API.ModelObjects;
using SysadminsLV.Asn1Editor.API.Utils.ASN;

namespace SysadminsLV.Asn1Editor.API.Utils;

static class ClipboardManager {
    static readonly List<Byte> _rawData = new List<Byte>();

    public static void SetClipboardData(IEnumerable<Byte> content) {
        _rawData.Clear();
        _rawData.AddRange(content);
    }
    public static void ClearClipboard() {
        _rawData.Clear();
    }
    public static async Task<Asn1TreeNode> GetClipboardDataAsync(IDataSource dataSource) {
        return await AsnTreeBuilder.BuildTree(_rawData.ToArray(), dataSource);
    }
    public static IEnumerable<Byte> GetClipboardBytes() {
        return _rawData;
    }
}