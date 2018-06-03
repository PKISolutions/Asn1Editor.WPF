using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Asn1Editor.API.ModelObjects;
using Asn1Editor.API.Utils.ASN;

namespace Asn1Editor.API.Utils {
    static class ClipboardManager {
        static readonly List<Byte> _rawData = new List<Byte>();

        public static void SetClipboardData(IEnumerable<Byte> content) {
            _rawData.Clear();
            _rawData.AddRange(content);
        }
        public static void ClearClipboard() {
            _rawData.Clear();
        }
        public static async Task<Asn1TreeNode> GetClipboardData() {
            return await AsnTreeBuilder.BuildTree(_rawData.ToArray());
        }
        public static IEnumerable<Byte> GetClipboardBytes() {
            return _rawData;
        }
    }
}
