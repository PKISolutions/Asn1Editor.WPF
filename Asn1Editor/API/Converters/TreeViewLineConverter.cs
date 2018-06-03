using System;
using System.Windows.Controls;
using System.Windows.Data;

namespace Asn1Editor.API.Converters {
    class TreeViewLineConverter : IValueConverter {
        public Object Convert(Object value, Type targetType, Object parameter, System.Globalization.CultureInfo culture) {
            TreeViewItem item = (TreeViewItem)value;
            ItemsControl ic = ItemsControl.ItemsControlFromItemContainer(item);
            return ic.ItemContainerGenerator.IndexFromContainer(item) == ic.Items.Count - 1;
        }

        public Object ConvertBack(Object value, Type targetType, Object parameter, System.Globalization.CultureInfo culture) {
            return false;
        }
    }
}
