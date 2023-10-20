namespace SysadminsLV.Asn1Editor.API.Interfaces; 

public interface ITreeViewVM {
    IDataSource DataSource { get; }
    ITreeCommands TreeCommands { get; }
}