using System;
using SysadminsLV.Asn1Parser;

namespace SysadminsLV.Asn1Editor.API.ModelObjects;
class EncodingTypeEntry {
    public EncodingTypeEntry(EncodingType encodingType, String displayName, EncodingGroup encodingGroup) {
        EncodingType = encodingType;
        DisplayName = displayName;
        EncodingGroup = encodingGroup;
    }

    public EncodingType EncodingType { get; set; }
    public String DisplayName { get; set; }
    public EncodingGroup EncodingGroup { get; set; }
}
enum EncodingGroup {
    PEM,
    Hexadecimal
}