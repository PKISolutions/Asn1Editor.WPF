using System.Windows.Input;

namespace SysadminsLV.Asn1Editor.API.Interfaces {
    public interface IAppCommands {
        ICommand ShowAboutCommand { get; }
        ICommand ShowSettingsCommand { get; }
        ICommand ShowConverterWindow { get; set; }
    }
}