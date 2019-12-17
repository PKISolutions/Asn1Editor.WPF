using System;

namespace SysadminsLV.Asn1Editor.API.Interfaces {
    public interface IMainWindowVM {
        Boolean IsModified { get; }
        Boolean RequestFileSave();
        void DropFile(String filePath);
        void OpenExisting(String filePath);
        void OpenRaw(String base64String);
    }
}