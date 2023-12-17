using System;

namespace SysadminsLV.Asn1Editor.API.ModelObjects;

/// <summary>
/// Represents ASN.1 node text value options.
/// This enumeration has a <see cref="FlagsAttribute"/> attribute that allows a bitwise combination of its member values.
/// </summary>
[Flags]
enum AsnViewValueOptions {
    /// <summary>
    /// None options supported
    /// </summary>
    None                  = 0,
    /// <summary>
    /// Node value supports printable text representation in addition to hex dump.
    /// </summary>
    SupportsPrintableText = 1,
}