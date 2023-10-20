using SysadminsLV.Asn1Editor.API.Interfaces;

namespace SysadminsLV.Asn1Editor.Views.Windows; 

/// <summary>
/// Interaction logic for TagDataEditor.xaml
/// </summary>
public partial class TagDataEditor : ITagDataEditor {
    public TagDataEditor(ITagDataEditorVM vm) {
        InitializeComponent();
        DataContext = vm;
    }
}