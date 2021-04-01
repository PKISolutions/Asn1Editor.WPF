using System.IO;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using SysadminsLV.WPF.OfficeTheme.Toolkit.Commands;

namespace SysadminsLV.Asn1Editor.Views.Windows {
    /// <summary>
    /// Interaction logic for LicenseWindow.xaml
    /// </summary>
    public partial class LicenseWindow {
        public LicenseWindow() {
            CloseCommand = new RelayCommand(o => Close());
            InitializeComponent();
            loadEula();
            DataContext = this;
        }
        public ICommand CloseCommand { get; }

        void loadEula() {
            if (File.Exists("EULA.rtf")) {
                var textRange = new TextRange(rtb.Document.ContentStart, rtb.Document.ContentEnd);
                using (var fileStream = new FileStream("EULA.rtf", FileMode.OpenOrCreate)) {
                    textRange.Load(fileStream, DataFormats.Rtf);
                }
            }
        }
    }
}
