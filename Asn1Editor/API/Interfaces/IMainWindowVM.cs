using System;
using System.Threading.Tasks;
using SysadminsLV.Asn1Editor.API.ViewModel;

namespace SysadminsLV.Asn1Editor.API.Interfaces;

public interface IMainWindowVM {
    Boolean RequestFileSave(Asn1DocumentVM tab);
    Task DropFileAsync(String filePath);
    Task OpenExistingAsync(String filePath);
    Task OpenRawAsync(String base64String);
}