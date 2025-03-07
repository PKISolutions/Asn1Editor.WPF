using System;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows.Input;
using SysadminsLV.Asn1Editor.API.Abstractions;
using SysadminsLV.Asn1Editor.API.Interfaces;
using SysadminsLV.Asn1Editor.API.ModelObjects;
using SysadminsLV.Asn1Parser;
using SysadminsLV.WPF.OfficeTheme.Toolkit.Commands;

namespace SysadminsLV.Asn1Editor.API.ViewModel;
public class NewAsnNodeEditorVM : ClosableWindowVM, INewAsnNodeEditorVM {
    Boolean shouldGenerateNode;
    Boolean formTagChecked, decimalTagChecked, hexTagChecked,
        clsConstructedChecked, clsSpecificChecked, clsApplicationChecked, clsPrivateChecked;
    String decimalTagText, hexTagText, resultTagDecimal, resultTagHex, resultTagName;
    Asn1Type selectedType;

    public NewAsnNodeEditorVM(NodeViewOptions options) {
        NodeViewOptions = options;
        formTagChecked = true;
        OkCommand = new RelayCommand(save, canSave);
        SelectedType = Asn1Type.SEQUENCE;
    }

    public ICommand OkCommand { get; }

    public NodeViewOptions NodeViewOptions { get; }

    public Boolean FormTagChecked {
        get => formTagChecked;
        set {
            formTagChecked = value;
            OnPropertyChanged(nameof(FormTagChecked));
            if (formTagChecked) {
                DecimalTagText = null;
                HexTagText = null;
                updateResultTagInfo();
            }
        }
    }
    public Boolean DecimalTagChecked {
        get => decimalTagChecked;
        set {
            decimalTagChecked = value;
            OnPropertyChanged(nameof(DecimalTagChecked));
            if (decimalTagChecked) {
                HexTagText = null;
                updateResultTagInfo();
            }
        }
    }
    public Boolean HexTagChecked {
        get => hexTagChecked;
        set {
            hexTagChecked = value;
            OnPropertyChanged(nameof(HexTagChecked));
            if (hexTagChecked) {
                DecimalTagText = null;
                updateResultTagInfo();
            }
        }
    }

    public Boolean ConstructedChecked {
        get => clsConstructedChecked;
        set {
            clsConstructedChecked = value;
            OnPropertyChanged(nameof(ConstructedChecked));
            updateResultTagInfo();
        }
    }
    public Boolean SpecificChecked {
        get => clsSpecificChecked;
        set {
            clsSpecificChecked = value;
            OnPropertyChanged(nameof(SpecificChecked));
            updateResultTagInfo();
        }
    }
    public Boolean ApplicationChecked {
        get => clsApplicationChecked;
        set {
            clsApplicationChecked = value;
            OnPropertyChanged(nameof(ApplicationChecked));
            updateResultTagInfo();
        }
    }
    public Boolean PrivateChecked {
        get => clsPrivateChecked;
        set {
            clsPrivateChecked = value;
            OnPropertyChanged(nameof(PrivateChecked));
            updateResultTagInfo();
        }
    }

    public Asn1Type SelectedType {
        get => selectedType;
        set {
            selectedType = value;
            OnPropertyChanged(nameof(SelectedType));
            ConstructedChecked = SelectedType is Asn1Type.SEQUENCE or Asn1Type.SET;
            // no need to call updateResultTagInfo(), it is already called.
        }
    }
    public String DecimalTagText {
        get => decimalTagText;
        set {
            decimalTagText = value;
            OnPropertyChanged(nameof(DecimalTagText));
            updateResultTagInfo();
        }
    }
    public String HexTagText {
        get => hexTagText;
        set {
            hexTagText = value;
            OnPropertyChanged(nameof(HexTagText));
            updateResultTagInfo();
        }
    }

    public String ResultTagDecimal {
        get => resultTagDecimal;
        set {
            resultTagDecimal = value;
            OnPropertyChanged(nameof(ResultTagDecimal));
        }
    }
    public String ResultTagHex {
        get => resultTagHex;
        set {
            resultTagHex = value;
            OnPropertyChanged(nameof(ResultTagHex));
        }
    }
    public String ResultTagName {
        get => resultTagName;
        set {
            resultTagName = value;
            OnPropertyChanged(nameof(ResultTagName));
        }
    }

    Byte getResultingTag() {
        Byte retValue;
        if (FormTagChecked) {
            retValue = (Byte)SelectedType;
            if (ConstructedChecked) {
                retValue |= (Byte)Asn1Class.CONSTRUCTED;
            }
            if (SpecificChecked) {
                retValue |= (Byte)Asn1Class.CONTEXT_SPECIFIC;
            }
            if (ApplicationChecked) {
                retValue |= (Byte)Asn1Class.APPLICATION;
            }
            if (PrivateChecked) {
                retValue |= (Byte)Asn1Class.PRIVATE;
            }
        } else if (DecimalTagChecked) {
            retValue = Byte.Parse(DecimalTagText);
        } else {
            retValue = Byte.Parse(HexTagText, NumberStyles.HexNumber);
        }

        return retValue;
    }
    void updateResultTagInfo() {
        Byte tag = getResultingTag();
        ResultTagDecimal = tag.ToString();
        ResultTagHex = "0x" + tag.ToString("X2");
        ResultTagName = Asn1Reader.GetTagName(tag);
    }
    void save(Object o) {
        shouldGenerateNode = true;
        CloseCommand.Execute(null);
    }
    Boolean canSave(Object o) {
        if (FormTagChecked) {
            if (SelectedType == Asn1Type.RESERVED) {
                return ConstructedChecked || SpecificChecked || ApplicationChecked || PrivateChecked;
            }

            return true;
        }
        if (DecimalTagChecked && Byte.TryParse(DecimalTagText, out _)) {
            return true;
        }
        if (HexTagChecked) {
            String value = HexTagText ?? String.Empty;
            return Regex.IsMatch(value, "^[0-9A-F]{1,2}$", RegexOptions.IgnoreCase);
        }

        return false;
    }
    public Asn1Lite GetAsnNode(IDataSource dataSource) {
        if (!shouldGenerateNode) {
            return null;
        }
        Byte[] binData = [getResultingTag(), 0];
        var node = new Asn1Lite(new Asn1Reader(binData));
        if (dataSource.SelectedNode != null) {
            node.Offset = dataSource.SelectedNode.Value.Offset + dataSource.SelectedNode.Value.TagLength;
            node.Depth += dataSource.SelectedNode.Value.Depth;
        }
        dataSource.RawData.InsertRange(node.Offset, binData);

        return node;
    }
}
