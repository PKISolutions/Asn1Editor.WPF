using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using SysadminsLV.Asn1Editor.API.ViewModel;
using SysadminsLV.Asn1Editor.Properties;
using SysadminsLV.Asn1Parser;
using SysadminsLV.Asn1Parser.Universal;

namespace SysadminsLV.Asn1Editor.API.Utils.ASN {
    class AsnDecoder {
        public static String GetEditValue(Asn1Reader asn) {
            switch (asn.Tag) {
                case (Byte)Asn1Type.INTEGER:
                    return new Asn1Integer(asn.GetTagRawData()).Value.ToString();
                case (Byte)Asn1Type.BIT_STRING:
                    return HexUtility.GetHexEditString(new Asn1BitString(asn).Value);
                case (Byte)Asn1Type.OBJECT_IDENTIFIER:
                    Oid oid = new Asn1ObjectIdentifier(asn).Value;
                    return oid.Value;
                case (Byte)Asn1Type.BOOLEAN:
                case (Byte)Asn1Type.UTCTime:
                case (Byte)Asn1Type.GeneralizedTime:
                case (Byte)Asn1Type.UTF8String:
                case (Byte)Asn1Type.NumericString:
                case (Byte)Asn1Type.PrintableString:
                case (Byte)Asn1Type.TeletexString:
                case (Byte)Asn1Type.VideotexString:
                case (Byte)Asn1Type.IA5String:
                case (Byte)Asn1Type.VisibleString:
                case (Byte)Asn1Type.UniversalString:
                case (Byte)Asn1Type.BMPString:
                    return GetViewValue(asn);
                default:
                    return (asn.Tag & (Byte)Asn1Type.TAG_MASK) == 6
                        ? Encoding.UTF8.GetString(asn.GetPayload())
                        : HexUtility.GetHexEditString(asn.GetPayload());
            }
        }
        public static String GetViewValue(Asn1Reader asn) {
            if (asn.PayloadLength == 0 && asn.Tag != (Byte)Asn1Type.NULL) { return "NULL"; }
            switch (asn.Tag) {
                case (Byte)Asn1Type.BOOLEAN:
                    return new Asn1Boolean(asn).Value.ToString();
                case (Byte)Asn1Type.INTEGER:
                    return DecodeInteger(asn);
                case (Byte)Asn1Type.BIT_STRING:
                    return DecodeBitString(asn);
                case (Byte)Asn1Type.OCTET_STRING:
                    return DecodeOctetString(asn);
                case (Byte)Asn1Type.NULL:
                    return null;
                case (Byte)Asn1Type.OBJECT_IDENTIFIER:
                    return DecodeObjectIdentifier(asn);
                case (Byte)Asn1Type.UTF8String:
                case (Byte)Asn1Type.VisibleString:
                    return Encoding.UTF8.GetString(asn.GetPayload());
                // we do not care on encoding enforcement when viewing data
                case (Byte)Asn1Type.NumericString:
                case (Byte)Asn1Type.PrintableString:
                case (Byte)Asn1Type.TeletexString:
                case (Byte)Asn1Type.VideotexString:
                case (Byte)Asn1Type.IA5String:
                    return Encoding.ASCII.GetString(asn.GetPayload());
                case (Byte)Asn1Type.UTCTime:
                    return DecodeUtcTime(asn);
                case (Byte)Asn1Type.BMPString:
                    return new Asn1BMPString(asn).Value;
                case (Byte)Asn1Type.GeneralizedTime:
                    return DecodeGeneralizedTime(asn);
                default:
                    return (asn.Tag & (Byte)Asn1Type.TAG_MASK) == 6
                        ? DecodeUTF8String(asn)
                        : DecodeOctetString(asn);
            }
        }

        #region Data type encoders
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
        public static Byte[] EncodeGeneralizedTime(DateTime time) {
            Char[] chars = (time.ToUniversalTime().ToString("yyyyMMddHHmmss") + "Z").ToCharArray();
            Byte[] rawData = new Byte[chars.Length];
            Int32 index = 0;
            foreach (Char element in chars) {
                rawData[index] = Convert.ToByte(element);
                index++;
            }
            return Asn1Utils.Encode(rawData, (Byte)Asn1Type.GeneralizedTime);
        }
        public static String DecodeUTF8String(Byte[] rawData) {
            if (rawData == null) { throw new ArgumentNullException(); }
            Asn1Reader asn = new Asn1Reader(rawData);
            if (asn.Tag != (Byte)Asn1Type.UTF8String) { throw new InvalidDataException(); }
            return Encoding.UTF8.GetString(asn.GetPayload());
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

        public static Byte[] EncodeHex(Byte tag, String hexString, Byte unusedBits) {
            Byte[] hexBytes = AsnFormatter.StringToBinary(hexString, EncodingType.Hex);
            return Asn1Utils.Encode(hexBytes, tag);
        }
        public static Byte[] EncodeGeneric(Byte tag, String value, Byte unusedBits) {
            switch (tag) {
                case (Byte)Asn1Type.BOOLEAN:
                    return new Asn1Boolean(Boolean.Parse(value)).GetRawData();
                case (Byte)Asn1Type.INTEGER:
                    return new Asn1Integer(BigInteger.Parse(value)).GetRawData();
                case (Byte)Asn1Type.BIT_STRING:
                    return new Asn1BitString(HexUtility.HexToBinary(value), unusedBits).GetRawData();
                case (Byte)Asn1Type.OCTET_STRING:
                    return new Asn1OctetString(HexUtility.HexToBinary(value), false).GetRawData();
                case (Byte)Asn1Type.NULL:
                    return new Asn1Null().GetRawData();
                case (Byte)Asn1Type.OBJECT_IDENTIFIER:
                    return new Asn1ObjectIdentifier(value).GetRawData();
                case (Byte)Asn1Type.ENUMERATED:
                    return new Asn1Enumerated(UInt64.Parse(value)).GetRawData();
                case (Byte)Asn1Type.UTF8String:
                    return new Asn1UTF8String(value).GetRawData();
                case (Byte)Asn1Type.NumericString:
                    return new Asn1NumericString(value).GetRawData();
                case (Byte)Asn1Type.TeletexString:
                    return new Asn1TeletexString(value).GetRawData();
                case (Byte)Asn1Type.VideotexString:
                    return Asn1Utils.Encode(Encoding.ASCII.GetBytes(value), tag);
                case (Byte)Asn1Type.PrintableString:
                    return new Asn1PrintableString(value).GetRawData();
                case (Byte)Asn1Type.IA5String:
                    return new Asn1IA5String(value).GetRawData();
                case (Byte)Asn1Type.UTCTime:
                    return EncodeUTCTime(DateTime.ParseExact(value, "MM/dd/yyyy HH:mm:ss", CultureInfo.InvariantCulture));
                case (Byte)Asn1Type.GeneralizedTime:
                    return EncodeGeneralizedTime(DateTime.ParseExact(value, "MM/dd/yyyy HH:mm:ss", CultureInfo.InvariantCulture));
                //case (Byte)ASN1Tags.GraphicString
                case (Byte)Asn1Type.VisibleString:
                    return new Asn1VisibleString(value).GetRawData();
                case (Byte)Asn1Type.GeneralString:
                    return Asn1Utils.Encode(Encoding.UTF8.GetBytes(value), tag);
                case (Byte)Asn1Type.UniversalString:
                    return new Asn1UniversalString(value).GetRawData();
                case (Byte)Asn1Type.BMPString:
                    return new Asn1BMPString(value).GetRawData();
                default:
                    if ((tag & (Byte)Asn1Type.TAG_MASK) == 6) {
                        return Asn1Utils.Encode(Encoding.UTF8.GetBytes(value), tag);
                    }
                    return Asn1Utils.Encode(HexUtility.HexToBinary(value), tag);
            }
        }
        #endregion
        #region Data type robust decoders
        static String DecodeInteger(Asn1Reader asn) {
            return Settings.Default.IntAsInt
                ? new BigInteger(asn.GetPayload().Reverse().ToArray()).ToString()
                : AsnFormatter.BinaryToString(
                    asn.GetRawData(),
                    EncodingType.HexRaw,
                    EncodingFormat.NOCRLF, asn.PayloadStartOffset, asn.PayloadLength);
        }
        static String DecodeBitString(Asn1Reader asn) {
            if (asn.PayloadLength == 1) {
                return $"Unused bits: {asn[asn.PayloadStartOffset]} : NULL";
            }
            return String.Format(
                "Unused bits: {0} : {1}",
                asn[asn.PayloadStartOffset],
                AsnFormatter.BinaryToString(
                    asn.GetRawData(),
                    EncodingType.HexRaw,
                    EncodingFormat.NOCRLF,
                    asn.PayloadStartOffset + 1,
                    asn.PayloadLength - 1)
            );
        }
        static String DecodeOctetString(Asn1Reader asn) {
            return AsnFormatter.BinaryToString(
                asn.GetRawData(),
                EncodingType.HexRaw,
                EncodingFormat.NOCRLF, asn.PayloadStartOffset, asn.PayloadLength);
        }
        static String DecodeObjectIdentifier(Asn1Reader asn) {
            Oid oid = new Asn1ObjectIdentifier(asn).Value;
            if (!String.IsNullOrWhiteSpace(oid.FriendlyName)) {
                return $"{oid.FriendlyName} ({oid.Value})";
            }
            if (MainWindowVM.OIDs.ContainsKey(oid.Value))
            {
                return $"{MainWindowVM.OIDs[oid.Value]} ({oid.Value})"; //todo: This seems not to be thread safe
            }
            return oid.Value;
        }
        static String DecodeUTF8String(Asn1Reader asn) {
            return Encoding.UTF8.GetString(asn.GetRawData(), asn.PayloadStartOffset, asn.PayloadLength);
        }
        static String DecodeUtcTime(Asn1Reader asn) {
            DateTime dt = new Asn1UtcTime(asn).Value;
            return dt.ToString("dd.MM.yyyy hh:mm:ss");
        }
        static String DecodeGeneralizedTime(Asn1Reader asn) {
            DateTime dt = new Asn1GeneralizedTime(asn).Value;
            return dt.ToString("dd.MM.yyyy hh:mm:ss");
        }
        #endregion
    }
}
