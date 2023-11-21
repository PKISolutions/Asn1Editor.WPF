using SysadminsLV.Asn1Editor.API.ViewModel;

namespace SysadminsLV.Asn1Editor.Views.Windows;
/// <summary>
/// Interaction logic for OidLookupEditorWindow.xaml
/// </summary>
public partial class OidLookupEditorWindow {
    public OidLookupEditorWindow() {
        var vm = new OidEditorVM();
        vm.ReloadCommand.Execute(null);
        DataContext = vm;
        InitializeComponent();
    }
}
