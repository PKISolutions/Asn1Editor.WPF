using System.Windows;

namespace SysadminsLV.Asn1Editor.API.Utils.WPF {
    abstract class WindowFactoryBase {
        protected Window hwnd;
        void setParent() {
            WindowCollection windows = Application.Current.Windows;
            Window           parent  = null;
            if (windows.Count > 0) {
#if DEBUG
                parent = windows[windows.Count - 3];
#else
                parent = windows[windows.Count - 2];
#endif
            }
            hwnd.Owner = parent;
        }
        protected void ShowAsWindow() {
            setParent();
            hwnd.Show();
        }
        protected void ShowAsDialog() {
            setParent();
            hwnd.ShowDialog();
        }
    }
}
