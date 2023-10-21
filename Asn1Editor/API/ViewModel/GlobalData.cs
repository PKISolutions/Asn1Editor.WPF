using System;
using System.Collections.Generic;
using System.Reflection;

namespace SysadminsLV.Asn1Editor.API.ViewModel;
class GlobalData {
    public GlobalData() {
        AppVersion = Assembly.GetEntryAssembly().GetName().Version.ToString(3);
    }
    public String AppVersion { get; set; }
    /// <summary>
    /// Represents OID mapping dictionary. Key represents OID value, value represents OID friendly display name.
    /// </summary>
    public static Dictionary<String, String> OIDs { get; } = new();
}
