using SysadminsLV.Asn1Editor.API.Interfaces;
using SysadminsLV.Asn1Editor.API.ModelObjects;

namespace SysadminsLV.Asn1Editor.API.Abstractions;

public interface INewAsnNodeEditorVM {
    Asn1Lite GetAsnNode(IDataSource dataSource);
}