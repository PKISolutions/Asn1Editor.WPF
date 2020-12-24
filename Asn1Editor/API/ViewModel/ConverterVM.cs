using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Input;
using Asn1Editor.Wpf.Controls;
using Microsoft.Win32;
using SysadminsLV.Asn1Editor.API.Utils;
using SysadminsLV.Asn1Editor.Properties;
using SysadminsLV.Asn1Parser;
using SysadminsLV.WPF.OfficeTheme.Toolkit.CLR;
using SysadminsLV.WPF.OfficeTheme.Toolkit.Commands;

namespace SysadminsLV.Asn1Editor.API.ViewModel {
    class ConverterVM : ViewModelBase {
        readonly String master = new String('0', 78);
        Double width;
        String text, path;
        Boolean? dialogResult;
        Boolean isLocked, canCheck,
            base64, base64header, base64req, base64crl, hex, hexaddr, hexascii, hexaddrascii, hexraw;
        readonly Action<Byte[]> _action;

        public ConverterVM(Action<Byte[]> action) {
            _action = action;
            base64 = true;
            OnPropertyChanged(nameof(Base64));
            OpenCommand = new RelayCommand(openFile);
            SaveCommand = new RelayCommand(saveFile, canPrintSave);
            PrintCommand = new RelayCommand(print, canPrintSave);
            ClearCommand = new RelayCommand(clearText);
            ValidateCommand = new RelayCommand(validateInput);
            CloseCommand = new RelayCommand(close);
            TextBoxWidth = TextUtility.MeasureStringWidth(master, Settings.Default.FontSize, true);
        }
        
        public ICommand OpenCommand { get; }
        public ICommand SaveCommand { get; }
        public ICommand ClearCommand { get; }
        public ICommand ValidateCommand { get; }
        public ICommand PrintCommand { get; }
        public ICommand CloseCommand { get; }

        public String Text {
            get => text;
            set {
                text = value?.Trim();
                OnPropertyChanged(nameof(Text));
            }
        }
        public String Path {
            get => path;
            set {
                path = value;
                OnPropertyChanged(nameof(Path));
            }
        }
        public Double TextBoxWidth {
            get => width;
            set {
                width = value;
                OnPropertyChanged(nameof(TextBoxWidth));
            }
        }
        public Boolean CanCheck {
            get => canCheck;
            set {
                canCheck = value;
                OnPropertyChanged(nameof(CanCheck));
            }
        }
        public Boolean? DialogResult {
            get => dialogResult;
            set {
                dialogResult = value;
                OnPropertyChanged(nameof(DialogResult));
            }
        }
        public ObservableList<Byte> RawData { get; } = new ObservableList<Byte>(true, false);

        #region Radiobuttons
        public Boolean Base64 {
            get => base64;
            set {
                base64 = value;
                if (base64) { Text = AsnFormatter.BinaryToString(RawData.ToArray(), EncodingType.Base64); }
                OnPropertyChanged(nameof(Base64));
            }
        }
        public Boolean Base64Header {
            get => base64header;
            set {
                base64header = value;
                if (base64header) { Text = AsnFormatter.BinaryToString(RawData.ToArray(), EncodingType.Base64Header); }
                OnPropertyChanged(nameof(Base64Header));
            }
        }
        public Boolean Base64Req {
            get => base64req;
            set {
                base64req = value;
                if (base64req) { Text = AsnFormatter.BinaryToString(RawData.ToArray(), EncodingType.Base64RequestHeader); }
                OnPropertyChanged(nameof(Base64Req));
            }
        }
        public Boolean Base64Crl {
            get => base64crl;
            set {
                base64crl = value;
                if (base64crl) { Text = AsnFormatter.BinaryToString(RawData.ToArray(), EncodingType.Base64CrlHeader); }
                OnPropertyChanged(nameof(Base64Crl));
            }
        }
        public Boolean Hex {
            get => hex;
            set {
                hex = value;
                if (hex) { Text = AsnFormatter.BinaryToString(RawData.ToArray(), EncodingType.Hex); }
                OnPropertyChanged(nameof(Hex));
            }
        }
        public Boolean HexAddr {
            get => hexaddr;
            set {
                hexaddr = value;
                if (hexaddr) { Text = AsnFormatter.BinaryToString(RawData.ToArray(), EncodingType.HexAddress); }
                OnPropertyChanged(nameof(HexAddr));
            }
        }
        public Boolean HexAscii {
            get => hexascii;
            set {
                hexascii = value;
                if (hexascii) { Text = AsnFormatter.BinaryToString(RawData.ToArray(), EncodingType.HexAscii); }
                OnPropertyChanged(nameof(HexAscii));
            }
        }
        public Boolean HexAddrAscii {
            get => hexaddrascii;
            set {
                hexaddrascii = value;
                if (hexaddrascii) { Text = AsnFormatter.BinaryToString(RawData.ToArray(), EncodingType.HexAsciiAddress); }
                OnPropertyChanged(nameof(HexAddrAscii));
            }
        }
        public Boolean HexRaw {
            get => hexraw;
            set {
                hexraw = value;
                if (hexraw) { Text = AsnFormatter.BinaryToString(RawData.ToArray()); }
                OnPropertyChanged(nameof(HexRaw));
            }
        }
        #endregion

        void openFile(Object obj) {
            isLocked = true;
            OpenFileDialog dlg = new OpenFileDialog {
                FileName = "",
                DefaultExt = ".*",
                Filter = "All files (*.*)|*.*"
            };
            Boolean? result = dlg.ShowDialog();
            if (result != true) { return; }
            Path = dlg.FileName;
            RawData.Clear();
            try {
                RawData.AddRange(FileUtility.FileToBinary(path).Result);
                RadioChanged("base64");
            } catch (Exception e) {
                Tools.MsgBox("Read Error", e.Message);
                Path = null;
            }
            isLocked = false;
        }
        void saveFile(Object obj) {
            switch ((String)obj) {
                case "text":
                    if (String.IsNullOrEmpty(Path)) {
                        if (!getFilePath()) { return; }
                    }
                    saveText();
                    break;
                case "textas":
                    if (!getFilePath()) { return; }
                    saveText();
                    break;
                case "bin":
                    if (String.IsNullOrEmpty(Path)) {
                        if (!getFilePath()) { return; }
                    }
                    saveBinary();
                    break;
                case "binas":
                    if (!getFilePath()) { return; }
                    saveBinary();
                    break;
            }
        }
        void validateInput(Object obj) {
            if (String.IsNullOrEmpty(Text)) {
                Tools.MsgBox("Warning", "There is nothing to validate.", MessageBoxImage.Warning);
                return;
            }
            EncodingType encoding = AsnFormatter.TestInputString(Text);
            RawData.Clear();
            switch (encoding) {
                case EncodingType.Base64:
                    base64 = true;
                    OnPropertyChanged(nameof(Base64));
                    break;
                case EncodingType.Base64Header:
                    base64header = true;
                    OnPropertyChanged(nameof(Base64Header));
                    break;
                case EncodingType.Base64RequestHeader:
                    base64req = true;
                    OnPropertyChanged(nameof(Base64Req));
                    break;
                case EncodingType.Base64CrlHeader:
                    base64crl = true;
                    OnPropertyChanged(nameof(Base64Crl));
                    break;
                case EncodingType.Hex:
                    hex = true;
                    OnPropertyChanged(nameof(Hex));
                    break;
                case EncodingType.HexAddress:
                    hexaddr = true;
                    OnPropertyChanged(nameof(HexAddr));
                    break;
                case EncodingType.HexAscii:
                    hexascii = true;
                    OnPropertyChanged(nameof(HexAscii));
                    break;
                case EncodingType.HexAsciiAddress:
                    hexaddrascii = true;
                    OnPropertyChanged(nameof(HexAddrAscii));
                    break;
                case EncodingType.HexRaw:
                    hexraw = true;
                    OnPropertyChanged(nameof(HexRaw));
                    break;
                default:
                    Tools.MsgBox("Error", "Input text cannot be validated.");
                    return;
            }
            RawData.AddRange(HexUtility.AnyToBinary(Text));
            if (obj != null && obj.ToString() == "Decode") {
                _action.Invoke(RawData.ToArray());
            }
        }
        void clearText(Object obj) {
            Text = String.Empty;
            RawData.Clear();
            Path = null;
        }
        void close(Object Obj) {
            throw new NotImplementedException();
        }
        void print(Object obj) {
            StaticCommands.Print(Text);
        }
        Boolean canPrintSave(Object obj) {
            return CanCheck = RawData.Count > 0;
        }
        Boolean getFilePath() {
            String path = Tools.GetSaveFileName();
            if (String.IsNullOrWhiteSpace(path.Trim())) { return false; }
            Path = path;
            return true;
        }
        void saveText() {
            try {
                File.WriteAllText(Path, Text);
            } catch (Exception e) {
                Tools.MsgBox("Save Error", e.Message);
            }
        }
        void saveBinary() {
            try {
                File.WriteAllBytes(Path, RawData.ToArray());
            } catch (Exception e) {
                Tools.MsgBox("Save Error", e.Message);
            }
        }

        public void RadioChanged(String tag) {
            if (RawData.Count == 0) { return; }
            switch (tag.ToLower()) {
                case "base64":
                    Text = AsnFormatter.BinaryToString(RawData.ToArray(), EncodingType.Base64);
                    Base64 = true;
                    break;
                case "base64header":
                    Text = AsnFormatter.BinaryToString(RawData.ToArray(), EncodingType.Base64Header);
                    break;
                case "base64reqheader":
                    Text = AsnFormatter.BinaryToString(RawData.ToArray(), EncodingType.Base64RequestHeader);
                    break;
                case "base64crlheader":
                    Text = AsnFormatter.BinaryToString(RawData.ToArray(), EncodingType.Base64CrlHeader);
                    break;
                case "hex":
                    Text = AsnFormatter.BinaryToString(RawData.ToArray(), EncodingType.Hex);
                    break;
                case "hexaddr":
                    Text = AsnFormatter.BinaryToString(RawData.ToArray(), EncodingType.HexAddress);
                    break;
                case "hexascii":
                    Text = AsnFormatter.BinaryToString(RawData.ToArray(), EncodingType.HexAscii);
                    break;
                case "hexaddrascii":
                    Text = AsnFormatter.BinaryToString(RawData.ToArray(), EncodingType.HexAsciiAddress);
                    break;
                case "hexraw":
                    Text = AsnFormatter.BinaryToString(RawData.ToArray());
                    break;
            }
        }
        public void SetBytes(IEnumerable<Byte> rawData) {
            if (rawData == null) { return; }
            RawData.Clear();
            RawData.AddRange(rawData);
            RadioChanged("base64");
        }
    }
}
