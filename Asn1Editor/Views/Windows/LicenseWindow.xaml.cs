using System;
using System.Windows.Input;
using SysadminsLV.WPF.OfficeTheme.Toolkit.Commands;

namespace SysadminsLV.Asn1Editor.Views.Windows {
    /// <summary>
    /// Interaction logic for LicenseWindow.xaml
    /// </summary>
    public partial class LicenseWindow {
        public LicenseWindow() {
            InitializeComponent();
            CloseCommand = new RelayCommand(o => Close());
        }

        public ICommand CloseCommand { get; }
        public String Html => Properties.Resources.License;
    }
}
