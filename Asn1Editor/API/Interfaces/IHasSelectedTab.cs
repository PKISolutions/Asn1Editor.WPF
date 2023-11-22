using SysadminsLV.Asn1Editor.API.ModelObjects;
using SysadminsLV.Asn1Editor.API.ViewModel;

namespace SysadminsLV.Asn1Editor.API.Interfaces;

public interface IHasSelectedTab {
    NodeViewOptions NodeViewOptions { get; }
    Asn1DocumentVM SelectedTab { get; }
}