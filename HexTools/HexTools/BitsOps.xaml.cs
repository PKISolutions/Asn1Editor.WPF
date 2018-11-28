using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace HexTools {
    /// <summary>
    /// Interaction logic for BitsOps.xaml
    /// </summary>
    public partial class BitsOps : UserControl {
        public BitsOps() {
            InitializeComponent();
        }

        void TextPreview(Object sender, TextCompositionEventArgs e) {
            e.Handled = e.Text != "0" && e.Text != "1";
        }

        void textPastePreview(Object sender, DataObjectPastingEventArgs e) {
            if (e.DataObject.GetDataPresent(typeof(String))) {
                String text = (String)e.DataObject.GetData(typeof(String));
                if (!isTextAllowed(text)) {
                    e.CancelCommand();
                }
            } else {
                e.CancelCommand();
            }
        }
        static Boolean isTextAllowed(String text) {
            return text == "0" || text == "1";
        }

        void UIElement_OnPreviewMouseMove(Object sender, MouseEventArgs e) {
            if (((TextBlock)sender).Text == "0") {
                ((TextBlock)sender).Text = "1";
            } else {
                ((TextBlock)sender).Text = "0";
            }
        }
    }
}
