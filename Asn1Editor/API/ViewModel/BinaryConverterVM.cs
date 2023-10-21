﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using SysadminsLV.Asn1Editor.API.ModelObjects;
using SysadminsLV.Asn1Editor.API.Utils;
using SysadminsLV.Asn1Parser;
using SysadminsLV.WPF.OfficeTheme.Toolkit.CLR;
using SysadminsLV.WPF.OfficeTheme.Toolkit.Commands;

namespace SysadminsLV.Asn1Editor.API.ViewModel;

class BinaryConverterVM : AsyncViewModel {
    readonly String master = new String('0', 78);
    Double width;
    String text, path;
    EncodingTypeEntry? selectedEncoding;
    Boolean canCheck;
    readonly Func<Byte[], Task> _action;

    public BinaryConverterVM(Func<Byte[], Task> action) {
        _action = action;
        OpenCommand = new RelayCommand(openFile);
        SaveCommand = new RelayCommand(saveFile, canPrintSave);
        PrintCommand = new RelayCommand(print, canPrintSave);
        ClearCommand = new RelayCommand(clearText);
        ValidateCommand = new RelayCommand(validateInput);
        //TextBoxWidth = TextUtility.MeasureStringWidth(master, Settings.Default.FontSize, true);
        initialize();
        EncodingTypesView = CollectionViewSource.GetDefaultView(EncodingTypes);
        EncodingTypesView.GroupDescriptions.Add(new PropertyGroupDescription(nameof(EncodingTypeEntry.EncodingGroup)));
    }

    public ICommand OpenCommand { get; }
    public ICommand SaveCommand { get; }
    public ICommand ClearCommand { get; }
    public ICommand ValidateCommand { get; }
    public ICommand PrintCommand { get; }

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
    public EncodingTypeEntry? SelectedEncoding {
        get => selectedEncoding;
        set {
            if (selectedEncoding == value) {
                return;
            }
            selectedEncoding = value;
            if (selectedEncoding != null) {
                Text = AsnFormatter.BinaryToString(RawData.ToArray(), selectedEncoding.EncodingType);
            }
            OnPropertyChanged(nameof(SelectedEncoding));
        }
    }
    public ObservableList<Byte> RawData { get; } = new(true, false);
    // we use this for combobox ItemsSource binding
    public ICollectionView EncodingTypesView { get; }
    // This is observable and public just in case if a list of radiobuttons is necessary,
    // so ItemsControl.ItemsSource can bind to a list of checkboxes
    public ObservableCollection<EncodingTypeEntry> EncodingTypes { get; } = new();

    void initialize() {
        addEncodingToTheList(EncodingType.Base64, "Base64", EncodingGroup.PEM);
        addEncodingToTheList(EncodingType.PemCert, "PEM - Certificate", EncodingGroup.PEM);
        addEncodingToTheList(EncodingType.PemTrustedCert, "PEM - Trusted Certificate", EncodingGroup.PEM);
        addEncodingToTheList(EncodingType.PemNewReq, "PEM - New Request", EncodingGroup.PEM);
        addEncodingToTheList(EncodingType.PemReq, "PEM - Request", EncodingGroup.PEM);
        addEncodingToTheList(EncodingType.Base64CrlHeader, "PEM - X.509 CRL", EncodingGroup.PEM);
        addEncodingToTheList(EncodingType.PemEvpPrivateKey, "PEM - EVP Private Key", EncodingGroup.PEM);
        addEncodingToTheList(EncodingType.PemPublicKey, "PEM - Public Key", EncodingGroup.PEM);
        addEncodingToTheList(EncodingType.PemRsaPrivateKey, "PEM - RSA Private Key", EncodingGroup.PEM);
        addEncodingToTheList(EncodingType.PemRsaPublicKey, "PEM - RSA Public Key", EncodingGroup.PEM);
        addEncodingToTheList(EncodingType.PemDsaPrivateKey, "PEM - DSA Private Key", EncodingGroup.PEM);
        addEncodingToTheList(EncodingType.PemDsaPublicKey, "PEM - DSA Public Key", EncodingGroup.PEM);
        addEncodingToTheList(EncodingType.PemDsaParams, "PEM - DSA Parameters", EncodingGroup.PEM);
        addEncodingToTheList(EncodingType.PemECPrivateKey, "PEM - ECDSA Private Key", EncodingGroup.PEM);
        addEncodingToTheList(EncodingType.PemECDsaPublicKey, "PEM - ECDSA Public Key", EncodingGroup.PEM);
        addEncodingToTheList(EncodingType.PemECParams, "PEM - EC Parameters", EncodingGroup.PEM);
        addEncodingToTheList(EncodingType.PemDHParams, "PEM - DH Parameters", EncodingGroup.PEM);
        addEncodingToTheList(EncodingType.PemDHXParams, "PEM - DHX Parameters", EncodingGroup.PEM);
        addEncodingToTheList(EncodingType.PemParams, "PEM - Parameters", EncodingGroup.PEM);
        addEncodingToTheList(EncodingType.PemPkcs7, "PEM - PKCS#7", EncodingGroup.PEM);
        addEncodingToTheList(EncodingType.PemPkcs7Signed, "PEM - PKCS#7 Signed", EncodingGroup.PEM);
        addEncodingToTheList(EncodingType.PemCms, "PEM - CMS", EncodingGroup.PEM);
        addEncodingToTheList(EncodingType.PemPkcs8Encrypted, "PEM - PKCS#8 Encrypted", EncodingGroup.PEM);
        addEncodingToTheList(EncodingType.PemPkcs8Inf, "PEM - PKCS#8 Unencrypted", EncodingGroup.PEM);
        addEncodingToTheList(EncodingType.PemSSLSessionParams, "PEM - SSL Session Parameters", EncodingGroup.PEM);
        addEncodingToTheList(EncodingType.Hex, "Hex", EncodingGroup.Hexadecimal);
        addEncodingToTheList(EncodingType.HexAddress, "Hex - with address", EncodingGroup.Hexadecimal);
        addEncodingToTheList(EncodingType.HexAscii, "Hex - with ASCII", EncodingGroup.Hexadecimal);
        addEncodingToTheList(EncodingType.HexAsciiAddress, "Hex - with address and ASCII", EncodingGroup.Hexadecimal);
        addEncodingToTheList(EncodingType.HexRaw, "Hex - raw", EncodingGroup.Hexadecimal);
    }
    void addEncodingToTheList(EncodingType encoding, String displayName, EncodingGroup encodingGroup) {
        EncodingTypes.Add(new EncodingTypeEntry(encoding, displayName, encodingGroup));
    }

    void openFile(Object obj) {
        if (!getOpenFilePath()) {
            return;
        }
        RawData.Clear();
        IsBusy = true;
        try {
            RawData.AddRange(FileUtility.FileToBinary(path).GetAwaiter().GetResult());
            formatToSelectedEncoding();
        } catch (Exception e) {
            Tools.MsgBox("Read Error", e.Message);
            Path = null;
        }
        IsBusy = false;
    }
    void saveFile(Object obj) {
        switch ((String)obj) {
            case "text":
                if (String.IsNullOrEmpty(Path)) {
                    if (!getSaveFilePath()) { return; }
                }
                saveText();
                break;
            case "textas":
                if (!getSaveFilePath()) { return; }
                saveText();
                break;
            case "bin":
                if (String.IsNullOrEmpty(Path)) {
                    if (!getSaveFilePath()) { return; }
                }
                saveBinary();
                break;
            case "binas":
                if (!getSaveFilePath()) { return; }
                saveBinary();
                break;
        }
    }
    async void validateInput(Object obj) {
        if (String.IsNullOrEmpty(Text)) {
            Tools.MsgBox("Warning", "There is nothing to validate.", MessageBoxImage.Warning);
            return;
        }
        EncodingType encoding = AsnFormatter.TestInputString(Text);
        RawData.Clear();
        selectedEncoding = EncodingTypes.FirstOrDefault(x => x.EncodingType == encoding);
        OnPropertyChanged(nameof(SelectedEncoding));
        if (SelectedEncoding == null) {
            Tools.MsgBox("Error", "Input text cannot be validated.");
        }
        RawData.AddRange(HexUtility.AnyToBinary(Text));
        if (obj != null && obj.ToString() == "Decode") {
            await _action(RawData.ToArray());
        }
    }
    void clearText(Object obj) {
        Text = String.Empty;
        RawData.Clear();
        Path = null;
    }
    void print(Object obj) {
        StaticCommands.Print(Text);
    }
    Boolean canPrintSave(Object obj) {
        return CanCheck = RawData.Count > 0;
    }
    Boolean getSaveFilePath() {
        String saveFilePath = Tools.GetSaveFileName();
        if (String.IsNullOrWhiteSpace(saveFilePath.Trim())) {
            return false;
        }
        Path = saveFilePath;

        return true;
    }
    Boolean getOpenFilePath() {
        String openFilePath = Tools.GetOpenFileName();
        if (String.IsNullOrWhiteSpace(openFilePath.Trim())) {
            return false;
        }
        Path = openFilePath;

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

    void formatToSelectedEncoding() {
        if (RawData.Count == 0) {
            return;
        }
        
        SelectedEncoding = EncodingTypes[0];
    }
    public void SetBytes(IEnumerable<Byte> rawData) {
        if (rawData == null) { return; }
        RawData.Clear();
        RawData.AddRange(rawData);
        formatToSelectedEncoding();
    }
}