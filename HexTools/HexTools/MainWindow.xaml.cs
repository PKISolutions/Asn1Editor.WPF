using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        ObservableCollection<String> _items = new ObservableCollection<string>();
        public MainWindow() {
            InitializeComponent();
            _items.Add("a");
            _items.Add("b");
        }
        public ReadOnlyObservableCollection<string> Items
        {
            get { return new ReadOnlyObservableCollection<string>(_items); }
        }

        void ButtonBase_OnClick(object Sender, RoutedEventArgs E) {
            _items.RemoveAt(0);
        }
    }
}
