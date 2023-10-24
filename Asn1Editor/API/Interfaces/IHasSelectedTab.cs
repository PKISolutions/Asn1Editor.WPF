using SysadminsLV.Asn1Editor.API.ViewModel;

namespace SysadminsLV.Asn1Editor.API.Interfaces;

public interface IHasSelectedTab {
    Asn1DocumentVM SelectedTab { get; }
}