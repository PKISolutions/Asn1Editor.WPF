using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Documents;
using SysadminsLV.Asn1Editor.API.Interfaces;
using SysadminsLV.Asn1Editor.API.Utils;
using SysadminsLV.Asn1Editor.Properties;
using SysadminsLV.Asn1Parser;
using Unity;

namespace SysadminsLV.Asn1Editor.API.ViewModel {
    class HexViewerVM : ViewModelBase, IHexViewerVM {
        const String masterAddr = "12345678";
        const String masterHex = "123456789012345678901234567890123456789012345678";
        const String masterAscii = "1234567890123456";
        FlowDocument addressDocument;
        FlowDocument hexDocument;
        FlowDocument asciiDocument;
        Double addrWidth, hexWidth, asciiWidth;

        public HexViewerVM() {
            init();
        }

        public IHexView View { get; set; }

        public FlowDocument AddressDocument {
            get => addressDocument;
            set {
                addressDocument = value;
                OnPropertyChanged(nameof(AddressDocument));
            }
        }
        public FlowDocument HexDocument {
            get => hexDocument;
            set {
                hexDocument = value;
                OnPropertyChanged(nameof(HexDocument));
            }
        }
        public FlowDocument AsciiDocument {
            get => asciiDocument;
            set {
                asciiDocument = value;
                OnPropertyChanged(nameof(AsciiDocument));
            }
        }
        public Double AddrWidth {
            get => addrWidth;
            set {
                addrWidth = value;
                OnPropertyChanged(nameof(AddrWidth));
            }
        }
        public Double HexWidth {
            get => hexWidth;
            set {
                hexWidth = value;
                OnPropertyChanged(nameof(HexWidth));
            }
        }
        public Double AsciiWidth {
            get => asciiWidth;
            set {
                asciiWidth = value;
                OnPropertyChanged(nameof(AsciiWidth));
            }
        }

        void init() {
            calculateWidths();
            Settings.Default.PropertyChanged += (Sender, Args) => {
                if (Args.PropertyName == "FontSize") {
                    calculateWidths();
                }
            };
        }
        void calculateWidths() {
            AddrWidth = Tools.MeasureString(masterAddr, Settings.Default.FontSize, false);
            HexWidth = Tools.MeasureString(masterHex, Settings.Default.FontSize, false);
            AsciiWidth = Tools.MeasureString(masterAscii, Settings.Default.FontSize, false);
        }
        void buildAddress(ICollection<Byte> rawData) {
            AddressDocument = new FlowDocument();
            Paragraph addressParagraph = new Paragraph();
            foreach (Int32 row in Enumerable.Range(0, (Int32)Math.Ceiling((Double)rawData.Count / 16))) {
                addressParagraph.Inlines.Add(new Run($"{row * 16:X8}" + Environment.NewLine));
            }
            AddressDocument.Blocks.Add(addressParagraph);
        }
        void buildHex(Byte[] rawData) {
            HexDocument = new FlowDocument();
            Paragraph hexParagraph = new Paragraph();
            hexParagraph.Inlines.Add(new Run(AsnFormatter.BinaryToString(rawData, EncodingType.Hex).ToUpper()));
            HexDocument.Blocks.Add(hexParagraph);
        }
        void buildAscii(IList<Byte> rawData) {
            AsciiDocument = new FlowDocument();
            Paragraph asciiParagraph = new Paragraph();
            StringBuilder SB = new StringBuilder();
            for (Int32 index = 0; index < rawData.Count; index++) {
                Char c = rawData[index] < 32 || rawData[index] > 126
                        ? '.'
                        : (Char)rawData[index];
                if (index != 0 && index % 16 == 0) {
                    SB.Append(Environment.NewLine);
                }
                SB.Append(c);
            }
            asciiParagraph.Inlines.Add(new Run(SB + Environment.NewLine));
            AsciiDocument.Blocks.Add(asciiParagraph);
        }
        
        public void BuildHexView(Byte[] rawData) {
            var data = App.Container.Resolve<IDataSource>();
            if (rawData == null) {
                if (data.RawData.Count == 0) { return; }
                rawData = data.RawData.ToArray();
            }
            buildAddress(rawData);
            buildHex(rawData);
            buildAscii(rawData);
        }
    }
}
