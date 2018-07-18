using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using SysadminsLV.Asn1Editor.API.Interfaces;
using SysadminsLV.Asn1Editor.API.ModelObjects;
using SysadminsLV.Asn1Editor.API.ViewModel;
using SysadminsLV.Asn1Editor.Properties;
using SysadminsLV.Asn1Parser;
using SysadminsLV.Asn1Parser.Universal;
using Unity;

namespace SysadminsLV.Asn1Editor.API.Utils.ASN {
    class AsnDecoder {
        static List<Byte> generatePrintableStringTable() {
            List<Byte> allowed = new List<Byte> { 32 };
            for (Byte index = 0x30; index <= 0x39; index++) { allowed.Add(index); }
            for (Byte index = 0x41; index <= 0x5a; index++) { allowed.Add(index); }
            for (Byte index = 0x61; index <= 0x7a; index++) { allowed.Add(index); }
            for (Byte index = 0x27; index <= 0x29; index++) { allowed.Add(index); }
            for (Byte index = 0x2b; index <= 0x2f; index++) { allowed.Add(index); }
            allowed.AddRange(new Byte[] { 0x3a, 0x3d, 0x3f });
            return allowed;
        }
        public static String GetEditValue(Asn1Lite asn) {
            var raw = App.Container.Resolve<IDataSource>().RawData;
            switch (asn.Tag) {
                case (Byte)Asn1Type.BOOLEAN:
                case (Byte)Asn1Type.UTCTime:
                case (Byte)Asn1Type.Generalizedtime:
                    return asn.ExplicitValue;
                case (Byte)Asn1Type.INTEGER:
                    return new Asn1Integer(raw.Skip(asn.Offset).Take(asn.TagLength).ToArray()).Value.ToString();
                case (Byte)Asn1Type.BIT_STRING:
                    return HexUtility.GetHexEditString(raw.Skip(asn.PayloadStartOffset + 1).Take(asn.PayloadLength - 1).ToArray());
                case (Byte)Asn1Type.OBJECT_IDENTIFIER:
                    Oid oid = DecodeObjectIdentifier(raw.Skip(asn.Offset).Take(asn.TagLength).ToArray());
                    return oid.Value;
                case (Byte)Asn1Type.UTF8String:
                    return DecodeUTF8String(raw.Skip(asn.Offset).Take(asn.TagLength).ToArray());
                case (Byte)Asn1Type.NumericString:
                    //TODO create appropriate decoder
                    return Encoding.ASCII.GetString(raw.Skip(asn.PayloadStartOffset).ToArray());
                case (Byte)Asn1Type.PrintableString:
                    return DecodePrintableString(raw.Skip(asn.Offset).Take(asn.TagLength).ToArray());
                // TODO 
                case (Byte)Asn1Type.TeletexString:
                    return Encoding.ASCII.GetString(raw.Skip(asn.PayloadStartOffset).ToArray());
                case (Byte)Asn1Type.VideotexString:
                    return Encoding.ASCII.GetString(raw.Skip(asn.PayloadStartOffset).ToArray());
                case (Byte)Asn1Type.IA5String:
                    return DecodeIA5String(raw.Skip(asn.Offset).Take(asn.TagLength).ToArray());
                case (Byte)Asn1Type.VisibleString:
                    return DecodeVisibleString(raw.Skip(asn.Offset).Take(asn.TagLength).ToArray());
                case (Byte)Asn1Type.UniversalString:
                    return DecodeUniversalString(raw.Skip(asn.Offset).Take(asn.TagLength).ToArray());
                case (Byte)Asn1Type.BMPString:
                    return DecodeBMPString(raw.Skip(asn.Offset).Take(asn.TagLength).ToArray());
                default:
                    return (asn.Tag & (Byte)Asn1Type.TAG_MASK) == 6
                        ? Encoding.UTF8.GetString(raw.Skip(asn.PayloadStartOffset).Take(asn.PayloadLength).ToArray())
                        : HexUtility.GetHexEditString(raw.Skip(asn.PayloadStartOffset).Take(asn.PayloadLength).ToArray());
            }
        }
        public static String GetViewValue(Asn1Reader asn) {
            if (asn.PayloadLength == 0 && asn.Tag != (Byte)Asn1Type.NULL) { return "NULL"; }
            switch (asn.Tag) {
                case (Byte)Asn1Type.BOOLEAN: return DecodeBoolean(asn);
                case (Byte)Asn1Type.INTEGER: return DecodeInteger(asn);
                case (Byte)Asn1Type.BIT_STRING: return DecodeBitString(asn);
                case (Byte)Asn1Type.OCTET_STRING: return DecodeOctetString(asn);
                case (Byte)Asn1Type.NULL: return null;
                case (Byte)Asn1Type.OBJECT_IDENTIFIER: return DecodeObjectIdentifier(asn);
                case (Byte)Asn1Type.UTF8String: return DecodeUTF8String(asn);
                case (Byte)Asn1Type.NumericString:
                case (Byte)Asn1Type.PrintableString:
                case (Byte)Asn1Type.TeletexString:
                case (Byte)Asn1Type.VideotexString:
                case (Byte)Asn1Type.IA5String:
                    return DecodeAsciiString(asn);
                case (Byte)Asn1Type.UTCTime:
                    return DecodeUtcTime(asn);
                case (Byte)Asn1Type.BMPString: return DecodeBMPString(asn);
                case (Byte)Asn1Type.Generalizedtime:
                    return DecodeGeneralizedTime(asn);
                default:
                    return (asn.Tag & (Byte)Asn1Type.TAG_MASK) == 6
                        ? DecodeUTF8String(asn)
                        : DecodeOctetString(asn);
            }
        }

        #region Data type encoders
        public static Byte[] EncodeInteger(UInt64 value) {
            return Asn1Utils.Encode(HexUtility.HexToBinary($"{value:x2}"), (Byte)Asn1Type.INTEGER);
        }
        public static Byte[] EncodeUTCTime(DateTime time) {
            Char[] chars = (time.ToUniversalTime().ToString("yyMMddHHmmss") + "Z").ToCharArray();
            Byte[] rawData = new Byte[chars.Length];
            Int32 index = 0;
            foreach (Char element in chars) {
                rawData[index] = Convert.ToByte(element);
                index++;
            }
            return Asn1Utils.Encode(rawData, (Byte)Asn1Type.UTCTime);
        }
        public static DateTime DecodeUTCTime(Byte[] rawData) {
            if (rawData == null) { throw new ArgumentNullException(); }
            Asn1Reader asn = new Asn1Reader(rawData);
            if (asn.Tag != (Byte)Asn1Type.UTCTime) { throw new InvalidDataException("Input data is not valid ASN-encoded UTC Time."); }
            StringBuilder SB = new StringBuilder();
            foreach (Byte element in asn.GetPayload()) { SB.Append(Convert.ToChar(element)); }
            try {
                return DateTime.ParseExact(SB.ToString().Replace("Z", null), "yyMMddHHmmss", null).ToLocalTime();
            } catch { throw new InvalidDataException("Input data is not valid ASN-encoded UTC Time."); }
        }
        public static Byte[] EncodeGeneralizedTime(DateTime time) {
            Char[] chars = (time.ToUniversalTime().ToString("yyyyMMddHHmmss") + "Z").ToCharArray();
            Byte[] rawData = new Byte[chars.Length];
            Int32 index = 0;
            foreach (Char element in chars) {
                rawData[index] = Convert.ToByte(element);
                index++;
            }
            return Asn1Utils.Encode(rawData, (Byte)Asn1Type.Generalizedtime);
        }
        public static DateTime DecodeGeneralizedTime(Byte[] rawData) {
            if (rawData == null) { throw new ArgumentNullException(); }
            Asn1Reader asn = new Asn1Reader(rawData);
            if (asn.Tag != (Byte)Asn1Type.Generalizedtime) { throw new InvalidDataException("Input data is not valid ASN-encoded GENERALIZED TIME."); }
            StringBuilder SB = new StringBuilder();
            foreach (Byte element in asn.GetPayload()) {
                SB.Append(Convert.ToChar(element));
            }
            try {
                return DateTime.ParseExact(SB.ToString().Replace("Z", null), "yyyyMMddHHmmss", null).ToLocalTime();
            } catch { throw new InvalidDataException("Input data is not valid ASN-encoded GENERALIZED TIME."); }
        }
        public static Byte[] EncodeObjectIdentifier(Oid oid) {
            if (oid == null) { throw new ArgumentNullException(); }
            if (String.IsNullOrEmpty(oid.Value)) { throw new ArgumentException("oid"); }
            return CryptoConfig.EncodeOID(oid.Value);
        }
        public static Oid DecodeObjectIdentifier(Byte[] rawData) {
            if (rawData == null) { throw new ArgumentNullException(); }
            try {
                Byte[] raw = Asn1Utils.Encode(rawData, 48);
                AsnEncodedData asnencoded = new AsnEncodedData(raw);
                X509EnhancedKeyUsageExtension eku = new X509EnhancedKeyUsageExtension(asnencoded, false);
                return eku.EnhancedKeyUsages[0];
            } catch { throw new InvalidDataException("Input data is not valid ASN-encoded Oid."); }
        }
        public static Byte[] EncodeBoolean(Boolean str) {
            Byte[] rawData = { 1, 1, 0 };
            if (str) { rawData[2] = 255; }
            return rawData;
        }
        public static String DecodeUTF8String(Byte[] rawData) {
            if (rawData == null) { throw new ArgumentNullException(); }
            Asn1Reader asn = new Asn1Reader(rawData);
            if (asn.Tag != (Byte)Asn1Type.UTF8String) { throw new InvalidDataException(); }
            return Encoding.UTF8.GetString(asn.GetPayload());
        }
        public static Byte[] EncodeUTF8String(String inputString) {
            return Asn1Utils.Encode(Encoding.UTF8.GetBytes(inputString), (Byte)Asn1Type.UTF8String);
        }
        public static String DecodeIA5String(Byte[] rawData) {
            if (rawData == null) { throw new ArgumentNullException(); }
            Asn1Reader asn = new Asn1Reader(rawData);
            if (asn.GetPayload().Any(@by => @by > 127)) {
                throw new ArgumentException("The data is invalid.");
            }
            if (asn.Tag == (Byte)Asn1Type.IA5String) {
                return Encoding.ASCII.GetString(asn.GetPayload());
            }
            throw new InvalidDataException();
        }
        public static Byte[] EncodeIA5String(String inputString) {
            Char[] chars = inputString.ToCharArray();
            if (chars.Any(ch => ch > 127)) {
                throw new InvalidDataException();
            }
            return Asn1Utils.Encode(Encoding.ASCII.GetBytes(inputString), (Byte)Asn1Type.IA5String);
        }
        public static String DecodePrintableString(Byte[] rawData) {
            if (rawData == null) { throw new ArgumentNullException(); }
            Asn1Reader asn = new Asn1Reader(rawData);
            if (asn.Tag != (Byte)Asn1Type.PrintableString) { throw new InvalidDataException(); }
            List<Byte> allowed = generatePrintableStringTable();
            if (asn.GetPayload().Any(@by => !allowed.Contains(@by))) {
                throw new ArgumentException("The data is invalid.");
            }
            return Encoding.ASCII.GetString(asn.GetPayload());
        }
        public static Byte[] EncodePrintableString(String inputString) {
            List<Byte> allowed = generatePrintableStringTable();
            if (inputString.Any(c => !allowed.Contains((Byte)c))) {
                throw new InvalidDataException();
            }
            return Asn1Utils.Encode(Encoding.ASCII.GetBytes(inputString), (Byte)Asn1Type.PrintableString);
        }
        public static String DecodeVisibleString(Byte[] rawData) {
            if (rawData == null) { throw new ArgumentNullException(); }
            Asn1Reader asn = new Asn1Reader(rawData);
            if (asn.GetPayload().Any(x => x < 32 || x > 126)) {
                throw new InvalidDataException();
            }
            if (asn.Tag == (Byte)Asn1Type.VisibleString) {
                return Encoding.ASCII.GetString(asn.GetPayload());
            }
            throw new InvalidDataException();
        }
        public static Byte[] EncodeVisibleString(String inputString) {
            Char[] chars = inputString.ToCharArray();
            if (chars.Any(ch => ch < 32 || ch > 126)) {
                throw new InvalidDataException();
            }
            return Asn1Utils.Encode(Encoding.ASCII.GetBytes(inputString), (Byte)Asn1Type.VisibleString);
        }
        public static String DecodeBMPString(Byte[] rawData) {
            if (rawData == null) { throw new ArgumentNullException(); }
            Asn1Reader asn = new Asn1Reader(rawData);
            if (asn.Tag == (Byte)Asn1Type.BMPString) {
                return Encoding.BigEndianUnicode.GetString(asn.GetPayload());
            }
            throw new InvalidDataException();
        }
        public static Byte[] EncodeBMPString(String inputString) {
            return Asn1Utils.Encode(Encoding.BigEndianUnicode.GetBytes(inputString), (Byte)Asn1Type.BMPString);
        }
        public static String DecodeUniversalString(Byte[] rawData) {
            if (rawData == null) { throw new ArgumentNullException(); }
            Asn1Reader asn = new Asn1Reader(rawData);
            List<Byte> orderedBytes = new List<Byte>();
            if (asn.Tag == (Byte)Asn1Type.UniversalString) {
                for (Int32 index = 0; index < rawData.Length; index += 4) {
                    orderedBytes.AddRange(new[] { rawData[index + 3], rawData[index + 2], rawData[index + 1], rawData[index] });
                }
                return Encoding.UTF32.GetString(orderedBytes.ToArray());
            }
            throw new InvalidDataException();
        }
        public static Byte[] EncodeUniversalString(String inputString) {
            List<Byte> orderedBytes = new List<Byte>();
            Byte[] unordered = Encoding.UTF32.GetBytes(inputString);
            for (Int32 index = 0; index < unordered.Length; index += 4) {
                orderedBytes.AddRange(new[] { unordered[index + 3], unordered[index + 2], unordered[index + 1], unordered[index] });
            }
            return Asn1Utils.Encode(orderedBytes.ToArray(), (Byte)Asn1Type.UniversalString);
        }
        public static Byte[] EncodeNull() {
            return new Byte[] { 5, 0 };
        }
        public static Byte[] EncodeDateTime(DateTime time) {
            return time.Year <= 2049 ? EncodeUTCTime(time) : EncodeGeneralizedTime(time);
        }

        public static Byte[] EncodeGeneric(Byte tag, String value, Byte unusedBits) {
            switch (tag) {
                case (Byte)Asn1Type.BOOLEAN:
                    return EncodeBoolean(Boolean.Parse(value));
                case (Byte)Asn1Type.INTEGER:
                    return Asn1Utils.Encode(HexUtility.HexToBinary(value), tag);
                case (Byte)Asn1Type.BIT_STRING:
                    List<Byte> bitBlob = new List<Byte> { unusedBits };
                    bitBlob.AddRange(HexUtility.HexToBinary(value));
                    return Asn1Utils.Encode(bitBlob.ToArray(), (Byte)Asn1Type.BIT_STRING);
                case (Byte)Asn1Type.OCTET_STRING:
                    return Asn1Utils.Encode(HexUtility.HexToBinary(value), tag);
                case (Byte)Asn1Type.NULL:
                    return EncodeNull();
                case (Byte)Asn1Type.OBJECT_IDENTIFIER:
                    return EncodeObjectIdentifier(new Oid(value));
                case (Byte)Asn1Type.ENUMERATED:
                    return Asn1Utils.Encode(new[] { Byte.Parse(value) }, tag);
                case (Byte)Asn1Type.UTF8String:
                    return EncodeUTF8String(value);
                case (Byte)Asn1Type.NumericString:
                case (Byte)Asn1Type.TeletexString:
                case (Byte)Asn1Type.VideotexString:
                    return Asn1Utils.Encode(Encoding.ASCII.GetBytes(value), tag);
                case (Byte)Asn1Type.PrintableString:
                    return EncodePrintableString(value);
                case (Byte)Asn1Type.IA5String:
                    return EncodeIA5String(value);
                case (Byte)Asn1Type.UTCTime:
                    return EncodeUTCTime(DateTime.ParseExact(value, "MM/dd/yyyy HH:mm:ss", CultureInfo.InvariantCulture));
                case (Byte)Asn1Type.Generalizedtime:
                    return EncodeGeneralizedTime(DateTime.ParseExact(value, "MM/dd/yyyy HH:mm:ss", CultureInfo.InvariantCulture));
                //case (Byte)ASN1Tags.GraphicString
                case (Byte)Asn1Type.VisibleString:
                    return EncodeVisibleString(value);
                case (Byte)Asn1Type.GeneralString:
                    return Asn1Utils.Encode(Encoding.UTF8.GetBytes(value), tag);
                case (Byte)Asn1Type.UniversalString:
                    return EncodeUniversalString(value);
                case (Byte)Asn1Type.BMPString:
                    return EncodeBMPString(value);
                default:
                    if ((tag & (Byte)Asn1Type.TAG_MASK) == 6) {
                        return Asn1Utils.Encode(Encoding.UTF8.GetBytes(value), tag);
                    }
                    return Asn1Utils.Encode(HexUtility.HexToBinary(value), tag);
            }
        }
        #endregion
        #region Data type robust decoders
        static String DecodeBoolean(Asn1Reader asn) {
            if (asn.PayloadLength != 1) {
                throw new InvalidDataException("Invalid Boolean.");
            }
            // non-zero value is True
            return asn.RawData[asn.PayloadStartOffset] == 0 ? false.ToString() : true.ToString();
        }
        static String DecodeInteger(Asn1Reader asn) {
            return Settings.Default.IntAsInt
                ? new BigInteger(asn.GetPayload().Reverse().ToArray()).ToString()
                : AsnFormatter.BinaryToString(
                    asn.RawData,
                    EncodingType.HexRaw,
                    EncodingFormat.NOCRLF, asn.PayloadStartOffset, asn.PayloadLength);
        }
        static String DecodeBitString(Asn1Reader asn) {
            return String.Format(
                "Unused bits: {0} : {1}",
                asn.RawData[asn.PayloadStartOffset],
                AsnFormatter.BinaryToString(
                    asn.RawData,
                    EncodingType.HexRaw,
                    EncodingFormat.NOCRLF,
                    asn.PayloadStartOffset + 1,
                    asn.PayloadLength - 1)
            );
        }
        static String DecodeOctetString(Asn1Reader asn) {
            return AsnFormatter.BinaryToString(
                asn.RawData,
                EncodingType.HexRaw,
                EncodingFormat.NOCRLF, asn.PayloadStartOffset, asn.PayloadLength);
        }
        static String DecodeObjectIdentifier(Asn1Reader asn) {
            Oid oid = Asn1ObjectIdentifier.Decode(asn);
            if (String.IsNullOrEmpty(oid.FriendlyName) && MainWindowVM.OIDs.ContainsKey(oid.Value)) {
                oid.FriendlyName = MainWindowVM.OIDs[oid.Value];
            }
            return String.IsNullOrEmpty(oid.FriendlyName)
                ? oid.Value
                : String.Format("{0} ({1})", oid.FriendlyName, oid.Value);
        }
        static String DecodeUTF8String(Asn1Reader asn) {
            return Encoding.UTF8.GetString(asn.RawData, asn.PayloadStartOffset, asn.PayloadLength);
        }
        static String DecodeAsciiString(Asn1Reader asn) {
            return Encoding.ASCII.GetString(asn.RawData, asn.PayloadStartOffset, asn.PayloadLength);
        }
        static String DecodeUtcTime(Asn1Reader asn) {
            DateTime dt = Asn1UtcTime.Decode(asn);
            return dt.ToString("dd.MM.yyyy hh:mm:ss");
        }
        static String DecodeGeneralizedTime(Asn1Reader asn) {
            DateTime dt = Asn1GeneralizedTime.Decode(asn);
            return dt.ToString("dd.MM.yyyy hh:mm:ss");
        }
        static String DecodeBMPString(Asn1Reader asn) {
            return Encoding.BigEndianUnicode.GetString(asn.RawData, asn.PayloadStartOffset, asn.PayloadLength);
        }
        #endregion
    }
}
