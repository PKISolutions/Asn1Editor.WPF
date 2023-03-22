using System;
using System.Diagnostics;
using System.IO;

namespace SysadminsLV.Asn1Editor.API.Utils; 

class Logger : IDisposable {
    readonly String       basePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), @"Sysadmins LV\Asn1Editor");
    readonly StreamWriter sesstionStream;

    public Logger() {
        Directory.CreateDirectory(Path.Combine(basePath, "Logs"));
        String dt = "Log-" + DateTime.Now.ToString("ddMMyyyyHHmmss");
        sesstionStream = new StreamWriter(Path.Combine(basePath, "Logs", $"{dt}-{Process.GetCurrentProcess().Id}.log")) { AutoFlush = true };
    }
    public void Write(String s) {
        sesstionStream.WriteLine(s);
    }
    public void Write(Exception e) {
        String dt = DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss");
        sesstionStream.WriteLine($"[{dt}]" + " An exception has been thrown:");
        Exception ex = e;
        do {
            sesstionStream.WriteLine($"\tError message: {ex.Message}\r\n\tStack trace:\r\n{ex.StackTrace.Replace("   ", "\t\t")}");
            ex = ex.InnerException;
        } while (ex != null);
    }
    public void Dispose() {
        sesstionStream?.Dispose();
    }
}