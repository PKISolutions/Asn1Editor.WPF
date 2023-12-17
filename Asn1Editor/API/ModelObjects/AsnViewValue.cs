using System;
using SysadminsLV.WPF.OfficeTheme.Toolkit.ViewModels;

namespace SysadminsLV.Asn1Editor.API.ModelObjects;

/// <summary>
/// Represents ASN.1 node text value object. Is used by TreeView and TagDataEditor controls.
/// </summary>
class AsnViewValue : ViewModelBase {
    String textValue;
    AsnViewValueOptions options;

    /// <summary>
    /// Gets or sets the ASN.1 node text value. Can be either printable string or hex dump.
    /// </summary>
    public String TextValue {
        get => textValue;
        set {
            if (textValue != value) {
                textValue = value;
                OnPropertyChanged(nameof(TextValue));
            }
        }
    }
    /// <summary>
    /// Gets or sets the ASN.1 node text value options, such as whether it can be viewed as printable text.
    /// </summary>
    public AsnViewValueOptions Options {
        get => options;
        set {
            if (options != value) {
                options = value;
                OnPropertyChanged(nameof(Options));
            }
        }
    }

    public AsnViewValue Clone() {
        return new AsnViewValue {
            TextValue = TextValue,
            Options = Options
        };
    }

    #region Equals

    public override Boolean Equals(Object obj) {
        return !ReferenceEquals(null, obj) &&
               (ReferenceEquals(this, obj) || obj.GetType() == GetType() && Equals((AsnViewValue)obj));
    }
    protected Boolean Equals(AsnViewValue other) {
        return textValue == other.textValue;
    }
    public override Int32 GetHashCode() {
        return textValue != null
            ? textValue.GetHashCode()
            : 0;
    }

    #endregion
}