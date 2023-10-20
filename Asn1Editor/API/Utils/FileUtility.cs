using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using SysadminsLV.Asn1Parser;

namespace SysadminsLV.Asn1Editor.API.Utils; 

static class FileUtility {
    static String GetRawText(String path) {
        using (StreamReader sr = new StreamReader(path)) {
            return sr.ReadToEnd();
        }
    }
    static Boolean ValidateData(Byte tag) {
        //List<Byte> excludedTags = new List<Byte>(
        //	new Byte[] { 0, 1, 2, 5, 6, 9, 10, 13 }
        //);
        //if (excludedTags.Contains(tag)) { return false; }
        //Byte masked = (Byte)(tag & (Byte)ASN1Tags.TAG_MASK);
        //if ((masked & (Byte)ASN1Classes.CONSTRUCTED) != 0) {
        //	if (excludedTags.Contains(masked)) {
        //		return false;
        //	}
        //}
        return true;
    }
    public static Task<IEnumerable<Byte>> FileToBinary(String path) {
        return Task.Factory.StartNew(() => {
                                         FileInfo fileInfo = new FileInfo(path);
                                         if (!fileInfo.Exists) {
                                             throw new Win32Exception(2);
                                         }
                                         Byte[] buffer = new Byte[4];
                                         using (FileStream fs = new FileStream(path, FileMode.Open)) {
                                             fs.Read(buffer, 0, 4);
                                         }
                                         if (
                                             buffer[0] == 0xfe && buffer[1] == 0xff || // BigEndian unicode
                                             buffer[0] == 0xff && buffer[1] == 0xfe || // LittleEndian unicode
                                             buffer[0] == 0xff && buffer[1] == 0xfe && buffer[2] == 0 && buffer[3] == 0 || // UTF32
                                             buffer[0] == 0xef && buffer[1] == 0xbb && buffer[2] == 0xbf || // UTF8
                                             buffer[0] == 0x2b && buffer[1] == 0x2f && buffer[2] == 0x76 // UTF7
                                         ) {
                                             return HexUtility.AnyToBinary(GetRawText(path));
                                         }
                                         Asn1Reader asn = new Asn1Reader(File.ReadAllBytes(path));
                                         if (asn.TagLength == fileInfo.Length) {
                                             if (ValidateData(asn.Tag)) {
                                                 return asn.GetTagRawData();
                                             }
                                             throw new Exception("The data is invalid");
                                         }
                                         asn = new Asn1Reader(HexUtility.AnyToBinary(GetRawText(path)).ToArray());
                                         if (ValidateData(asn.Tag)) {
                                             return asn.GetTagRawData();
                                         }
                                         throw new Exception("The data is invalid");
                                     });
    }
}