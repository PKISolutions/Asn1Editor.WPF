using System;
using System.Windows;
using Microsoft.Win32;

namespace SysadminsLV.Asn1Editor.API.Utils;

static class Tools {
    public static MessageBoxResult MsgBox(String header, String message, MessageBoxImage image = MessageBoxImage.Error, MessageBoxButton button = MessageBoxButton.OK) {
        WindowCollection windows = Application.Current.Windows;
        Window hwnd = null;
        if (windows.Count > 0) {
            hwnd = windows[windows.Count - 1];
        }
        return hwnd == null
            ? MessageBox.Show(message, header, button, image)
            : MessageBox.Show(hwnd, message, header, button, image);
    }
    public static Boolean TryGetSaveFileName(out String filePath) {
        filePath = null;
        var dlg = new SaveFileDialog {
            FileName = "",
            Filter = "All files (*.*)|*.*"
        };
        if (dlg.ShowDialog() == true) {
            filePath = dlg.FileName.Trim();
            return true;
        }

        return false;
    }
    public static Boolean TryGetOpenFileName(out String filePath) {
        filePath = null;
        var dlg = new OpenFileDialog {
            FileName = "",
            DefaultExt = ".*",
            Filter = "All files (*.*)|*.*"
        };
        if (dlg.ShowDialog() == true) {
            filePath = dlg.FileName.Trim();
            return true;
        }

        return false;
    }
}