using System;
using System.Collections.Generic;
using System.Linq;
using SysadminsLV.Asn1Editor.API.ModelObjects;
using SysadminsLV.Asn1Parser;

namespace SysadminsLV.Asn1Editor.API.Utils; 

static class HexUtility {
    public static String GetHexString(IEnumerable<Byte> rawData, Asn1Lite node) {
        Int32 startOffset = node.Offset + node.TagLength - node.PayloadLength;
        Int32 endOffset = node.Offset + node.TagLength;
        return AsnFormatter.BinaryToString(rawData.ToArray(), EncodingType.Hex, 0, startOffset, endOffset - startOffset);
    }
    public static String GetHexUString(Byte[] rawData) {
        return AsnFormatter.BinaryToString(rawData);
    }
    public static String GetHexEditString(Byte[] rawData) {
        return AsnFormatter.BinaryToString(rawData, EncodingType.Hex);
    }
    public static String BinaryToHex(Byte[] rawData) {
        return AsnFormatter.BinaryToString(rawData, EncodingType.Hex, EncodingFormat.NOCR);
    }
    public static Byte[] HexToBinary(String hexString) {
        return AsnFormatter.StringToBinary(hexString, EncodingType.Hex);
    }
    public static IEnumerable<Byte> AnyToBinary(String anyString) {
        try {
            return AsnFormatter.StringToBinary(anyString, EncodingType.HexAny);
        } catch {
            try {
                return AsnFormatter.StringToBinary(anyString, EncodingType.StringAny);
            } catch {
                try {
                    return AsnFormatter.StringToBinary(anyString, EncodingType.Binary);
                } catch {
                    Tools.MsgBox("Error", "The data is invalid");
                    return null;
                }
            }
        }
    }
}