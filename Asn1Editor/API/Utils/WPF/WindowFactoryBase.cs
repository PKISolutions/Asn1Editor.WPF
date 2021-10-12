using System;
using System.Windows;

namespace SysadminsLV.Asn1Editor.API.Utils.WPF {
    abstract class WindowFactoryBase {
        protected Window hwnd;
        void setParent(Boolean mainWindowAsParent = false) {
            WindowCollection windows = Application.Current.Windows;
            Window parent = null;
            if (windows.Count > 0)
            {
                parent = mainWindowAsParent 
                    ? windows[0] 
                    : windows[windows.Count - 2];
            }
            hwnd.Owner = parent;
        }
        protected void ShowAsWindow(Boolean mainWindowAsParent = false) {
            setParent(mainWindowAsParent);
            hwnd.Show();
        }
        protected void ShowAsDialog() {
            setParent();
            hwnd.ShowDialog();
        }
    }
}
