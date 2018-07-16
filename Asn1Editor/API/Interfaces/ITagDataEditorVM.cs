using System;
using SysadminsLV.Asn1Editor.API.ModelObjects;

namespace SysadminsLV.Asn1Editor.API.Interfaces {
    public interface ITagDataEditorVM {
        void SetBinding(Asn1Lite asnNode, Boolean hex);
    }
}