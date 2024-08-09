using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SysadminsLV.Asn1Editor.API.Interfaces;
using SysadminsLV.Asn1Editor.API.ModelObjects;
using SysadminsLV.Asn1Editor.API.Utils.ASN;

namespace SysadminsLV.Asn1Editor.API.Utils;

/// <summary>
/// Represents in-app custom clipboard manager to transfer ASN.1 tree nodes
/// </summary>
static class ClipboardManager {
    static readonly List<Byte> _rawData = [];

    /// <summary>
    /// Writes ASN.1 tree node into clipboard.
    /// </summary>
    /// <param name="content">ASN node's entire TLV in binary form.</param>
    public static void SetClipboardData(IEnumerable<Byte> content) {
        _rawData.Clear();
        _rawData.AddRange(content);
    }
    /// <summary>
    /// Clears clipboard data.
    /// </summary>
    public static void ClearClipboard() {
        _rawData.Clear();
    }
    /// <summary>
    /// Gets ASN.1 tree node stored in clipboard.
    /// </summary>
    /// <param name="dataSource"></param>
    /// <returns>ASN tree node.</returns>
    public static Task<Asn1TreeNode> GetClipboardDataAsync(IDataSource dataSource) {
        return AsnTreeBuilder.BuildTreeAsync(_rawData.ToArray(), dataSource);
    }
    /// <summary>
    /// Gets ASN.1 tree node bytes.
    /// </summary>
    /// <returns>ASN.1 tree node bytes.</returns>
    public static IEnumerable<Byte> GetClipboardBytes() {
        return _rawData;
    }
}