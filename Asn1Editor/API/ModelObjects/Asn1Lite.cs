using System;
using SysadminsLV.Asn1Editor.API.Interfaces;
using SysadminsLV.Asn1Editor.API.Utils;
using SysadminsLV.Asn1Editor.API.Utils.ASN;
using SysadminsLV.Asn1Editor.API.ViewModel;
using SysadminsLV.Asn1Editor.Properties;
using SysadminsLV.Asn1Editor.Views.UserControls.HexViewer;
using SysadminsLV.Asn1Parser;
using Unity;

namespace SysadminsLV.Asn1Editor.API.ModelObjects {
    public class Asn1Lite : ViewModelBase, IHexAsnNode {
        Byte tag, unusedBits;
        Boolean invalidData;
        Int32 offset, offsetChange;
        Int32 payloadLength, deepness;
        String tagName, explicitValue, treePath;

        public Asn1Lite(Asn1Reader asn) {
            Offset = asn.Offset;
            Tag = asn.Tag;
            TagName = asn.TagName;
            PayloadLength = asn.PayloadLength;
            PayloadStartOffset = asn.PayloadStartOffset;
            IsContainer = asn.IsConstructed;
            Deepness = 0;
            Path = String.Empty;
            try {
                if (!asn.IsConstructed) {
                    ExplicitValue = AsnDecoder.GetViewValue(asn);
                }
            } catch {
                InvalidData = true;
            }
        }
        public Asn1Lite(Asn1Reader root, Asn1TreeNode tree, Int32 index) {
            Offset = root.Offset;
            Tag = root.Tag;
            TagName = root.TagName;
            PayloadLength = root.PayloadLength;
            PayloadStartOffset = root.PayloadStartOffset;
            IsContainer = root.IsConstructed;
            Deepness = tree.Value.Deepness + 1;
            Path = tree.Value.Path + "/" + index;
            if (Tag == (Byte)Asn1Type.BIT_STRING) {
                if (root.PayloadLength > 0)
                    UnusedBits = root.RawData[root.PayloadStartOffset];
            }
            if (!root.IsConstructed) {
                try {
                    ExplicitValue = AsnDecoder.GetViewValue(root);
                } catch {
                    InvalidData = true;
                }
            }
        }

        public String Caption {
            get {
                String value = String.IsNullOrEmpty(explicitValue) || !Settings.Default.DecodePayload
                    ? String.Empty
                    : " : " + explicitValue;
                String value2 = IsContainer && Tag == (Byte)Asn1Type.BIT_STRING
                    ? " Unused bits: " + UnusedBits
                    : String.Empty;
                return IsContainer
                    ? $"({Offset}, {PayloadLength}) {TagName}{value2.TrimEnd()}"
                    : $"({Offset}, {PayloadLength}) {TagName}{value.TrimEnd()}";
            }
        }
        public Byte Tag {
            get => tag;
            set {
                tag = value;
                OnPropertyChanged(nameof(Tag));
            }
        }
        public Byte UnusedBits {
            get => unusedBits;
            set {
                unusedBits = value;
                OnPropertyChanged(nameof(UnusedBits));
                OnPropertyChanged(nameof(Caption));
            }
        }
        public String TagName {
            get => tagName;
            set {
                tagName = value;
                if (value.StartsWith("CONTEXT SPECIFIC")) { IsContextSpecific = true; }
                OnPropertyChanged(nameof(Caption));
            }
        }
        public Int32 Offset {
            get => offset;
            set {
                var diff = value - offset;
                offset = value;
                PayloadStartOffset += diff;
                OnPropertyChanged(nameof(Caption));
            }
        }
        public Int32 OffsetChange {
            get => offsetChange;
            set {
                if (offsetChange == value) { return; }
                offsetChange = value;
                OnPropertyChanged(nameof(OffsetChange));
            }
        }

        public Int32 PayloadStartOffset { get; set; }
        public Int32 HeaderLength => PayloadStartOffset - Offset;
        public Int32 PayloadLength {
            get => payloadLength;
            set {
                payloadLength = value;
                OnPropertyChanged(nameof(Caption));
            }
        }
        public Int32 TagLength => HeaderLength + PayloadLength;
        public Boolean IsContainer { get; set; }

        public Boolean IsContextSpecific { get; set; }
        public Boolean IsRoot { get; set; }
        public Boolean InvalidData {
            get => invalidData;
            set {
                invalidData = value;
                OnPropertyChanged(nameof(InvalidData));
            }
        } //TODO
        public Int32 Deepness {
            get => deepness;
            set {
                deepness = value;
                OnPropertyChanged(nameof(Caption));
            }
        }
        public String Path {
            get => treePath;
            set {
                treePath = value;
                OnPropertyChanged(nameof(Caption));
            }
        }
        public String ExplicitValue {
            get => explicitValue;
            set {
                explicitValue = value;
                OnPropertyChanged(nameof(Caption));
            }
        }

        public void UpdateView() {
            OnPropertyChanged(nameof(Caption));
        }
        public Boolean ValidateValue(String newValue, Byte unused) {
            Byte[] binValue;
            try {
                binValue = AsnDecoder.EncodeGeneric(Tag, newValue, unused);
            } catch (Exception e) {
                Tools.MsgBox("Error", e.Message);
                return false;
            }

            var data = App.Container.Resolve<IDataSource>();
            updateBinaryCopy(binValue, data);
            if (Tag == (Byte)Asn1Type.BIT_STRING) { UnusedBits = unused; }

            Asn1Reader asn = new Asn1Reader(binValue);
            PayloadStartOffset = Offset + asn.TagLength - asn.PayloadLength;
            ExplicitValue = AsnDecoder.GetViewValue(asn);
            OffsetChange = asn.PayloadLength - PayloadLength;
            PayloadLength = asn.PayloadLength;
            data.FinishBinaryUpdate();
            return true;
        }
        public override Boolean Equals(Object obj) {
            if (ReferenceEquals(null, obj)) { return false; }
            if (ReferenceEquals(this, obj)) { return true; }
            return obj.GetType() == typeof(Asn1Lite) && Equals((Asn1Lite)obj);
        }
        protected Boolean Equals(Asn1Lite other) {
            return offset == other.offset && tag == other.tag;
        }
        public override Int32 GetHashCode() {
            unchecked {
                return (offset * 397) ^ tag.GetHashCode();
            }
        }

        void updateBinaryCopy(Byte[] newBytes, IDataSource data) {
            data.RawData.RemoveRange(Offset, TagLength);
            data.RawData.InsertRange(Offset, newBytes);
        }
    }
}
