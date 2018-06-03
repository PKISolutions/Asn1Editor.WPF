using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using Asn1Editor.API.Utils;
using Asn1Editor.Properties;
using SysadminsLV.Asn1Parser;
using SysadminsLV.WPF.OfficeTheme.Toolkit.Commands;

namespace Asn1Editor.API.ViewModel {
    class HexViewerVM : INotifyPropertyChanged {
        const String masterAddr = "12345678";
        const String masterHex = "123456789012345678901234567890123456789012345678";
        const String masterAscii = "1234567890123456";
        Visibility hexAddressVisible;
        Visibility asciiValueVisible;
        Boolean hexAddressChecked;
        Boolean asciiValueChecked;
        FlowDocument addressDocument;
        FlowDocument hexDocument;
        FlowDocument asciiDocument;
        Double addrWidth, hexWidth, asciiWidth;

        public HexViewerVM() {
            ChangeHexAddressCommand = new RelayCommand(ChangeHexAddress);
            ChangeAsciiValueCommand = new RelayCommand(ChangeAsciiValue);
            Init();
        }

        public MainWindowVM ParentVM { get; set; }
        public IHexView View { get; set; }

        public ICommand ChangeHexAddressCommand { get; set; }
        public ICommand ChangeAsciiValueCommand { get; set; }

        public Visibility HexAddressVisible {
            get => hexAddressVisible;
            set {
                hexAddressVisible = value;
                OnPropertyChanged("HexAddressVisible");
            }
        }
        public Visibility AsciiValueVisible {
            get => asciiValueVisible;
            set {
                asciiValueVisible = value;
                OnPropertyChanged("AsciiValueVisible");
            }
        }
        public Boolean HexAddressChecked {
            get => hexAddressChecked;
            set {
                hexAddressChecked = value;
                OnPropertyChanged("HexAddressChecked");
            }
        }
        public Boolean AsciiValueChecked {
            get => asciiValueChecked;
            set {
                asciiValueChecked = value;
                OnPropertyChanged("AsciiValueChecked");
            }
        }
        public FlowDocument AddressDocument {
            get => addressDocument;
            set {
                addressDocument = value;
                OnPropertyChanged("AddressDocument");
            }
        }
        public FlowDocument HexDocument {
            get => hexDocument;
            set {
                hexDocument = value;
                OnPropertyChanged("HexDocument");
            }
        }
        public FlowDocument AsciiDocument {
            get => asciiDocument;
            set {
                asciiDocument = value;
                OnPropertyChanged("AsciiDocument");
            }
        }
        public Double AddrWidth {
            get => addrWidth;
            set {
                addrWidth = value;
                OnPropertyChanged("AddrWidth");
            }
        }
        public Double HexWidth {
            get => hexWidth;
            set {
                hexWidth = value;
                OnPropertyChanged("HexWidth");
            }
        }
        public Double AsciiWidth {
            get => asciiWidth;
            set {
                asciiWidth = value;
                OnPropertyChanged("AsciiWidth");
            }
        }

        void Init() {
            calculateWidths();
            Settings.Default.PropertyChanged += (Sender, Args) => {
                if (Args.PropertyName == "FontSize") {
                    calculateWidths();
                }
            };
            HexAddressVisible = Visibility.Visible;
            AsciiValueVisible = Visibility.Visible;
            HexAddressChecked = true;
            AsciiValueChecked = true;
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
                addressParagraph.Inlines.Add(new Run(String.Format("{0:X8}", row * 16) + Environment.NewLine));
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
        void ChangeHexAddress(Object obj) {
            HexAddressVisible = HexAddressChecked
                ? Visibility.Visible
                : Visibility.Collapsed;
        }
        void ChangeAsciiValue(Object obj) {
            AsciiValueVisible = AsciiValueChecked
                ? Visibility.Visible
                : Visibility.Collapsed;
        }

        public void BuildHexView(Byte[] rawData) {
            if (rawData == null) {
                if (MainWindowVM.RawData.Count == 0) { return; }
                rawData = MainWindowVM.RawData.ToArray();
            }
            buildAddress(rawData);
            buildHex(rawData);
            buildAscii(rawData);
        }

        void OnPropertyChanged(String PropertyName) {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) {
                handler(this, new PropertyChangedEventArgs(PropertyName));
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;
    }
}
