using System;
using System.Collections.Generic;

namespace SysadminsLV.Asn1Editor.API.Interfaces {
    public interface IMainWindowVM {
        void DropFile(String filePath);
        void OpenExisting(String filePath);
        void OpenRaw(String base64String);
        void OpenRaw(IEnumerable<Byte> rawBytes);
    }
}