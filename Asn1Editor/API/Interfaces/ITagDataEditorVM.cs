using SysadminsLV.Asn1Editor.API.ModelObjects;
using SysadminsLV.Asn1Editor.API.Utils;

namespace SysadminsLV.Asn1Editor.API.Interfaces {
    public interface ITagDataEditorVM {
        Asn1Lite Node { get; }
        void SetBinding(NodeEditMode editMode);
    }
}