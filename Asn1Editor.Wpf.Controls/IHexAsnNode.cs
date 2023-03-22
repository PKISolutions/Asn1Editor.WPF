using System;

namespace Asn1Editor.Wpf.Controls;

public interface IHexAsnNode {
    Int32 Offset { get; }
    Int32 PayloadStartOffset { get; }
    Int32 PayloadLength { get; }
}