using System;

namespace SysadminsLV.Asn1Editor.Views.UserControls.HexViewer {
    public interface IHexAsnNode {
        Int32 Offset { get; }
        Int32 PayloadStartOffset { get; }
        Int32 PayloadLength { get; }
    }
}