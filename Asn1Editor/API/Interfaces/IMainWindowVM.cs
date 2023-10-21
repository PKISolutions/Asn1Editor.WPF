using System;
using System.Threading.Tasks;

namespace SysadminsLV.Asn1Editor.API.Interfaces;

public interface IMainWindowVM {
    Boolean IsModified { get; }
    Boolean RequestFileSave();
    Task DropFileAsync(String filePath);
    Task OpenExistingAsync(String filePath);
    Task OpenRawAsync(String base64String);
}