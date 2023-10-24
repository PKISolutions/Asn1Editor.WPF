using System;
using System.Collections.Generic;
using System.Security.Cryptography;

namespace SysadminsLV.Asn1Editor.API;
/// <summary>
/// Represents object identifier (OID) resolver helper class.
/// </summary>
public static class OidResolver {
    static readonly Dictionary<String, String> _oidLookup = new();
    static readonly Dictionary<String, String> _nameLookup = new();

    /// <summary>
    /// Attempts to resolve OID value to friendly name.
    /// </summary>
    /// <param name="oidValue">OID value to resolve.</param>
    /// <returns>Resolved friendly name or null if no mapping defined.</returns>
    public static String ResolveOid(String oidValue) {
        oidValue ??= String.Empty;
        if (_oidLookup.TryGetValue(oidValue, out String resolvedOid)) {
            return resolvedOid;
        }

        return new Oid(oidValue).FriendlyName;
    }
    /// <summary>
    /// Resolves OID friendly name to OID value.
    /// </summary>
    /// <param name="friendlyName">OID friendly name.</param>
    /// <returns>Resolved OID value or null if no mapping defined.</returns>
    public static String ResolveFriendlyName(String friendlyName) {
        friendlyName ??= String.Empty;
        if (_nameLookup.TryGetValue(friendlyName, out String resolvedOid)) {
            return resolvedOid;
        }

        var oid = new Oid(friendlyName);
        if (friendlyName.Equals(oid.Value)) {
            return null;
        }

        return oid.Value;
    }
    /// <summary>
    /// Resolves OID value to friendly name or friendly name to OID value.
    /// </summary>
    /// <param name="oidValueOrFriendlyName">OID value or friendly name.</param>
    /// <returns>Resolved value or null if no mapping defined for passed OID string.</returns>
    public static String Resolve(String oidValueOrFriendlyName) {
        String resolvedOid = ResolveOid(oidValueOrFriendlyName);
        if (resolvedOid == null) {
            return ResolveFriendlyName(oidValueOrFriendlyName);
        }

        return resolvedOid;
    }
    /// <summary>
    /// Adds new OID mapping to internal lookup dictionary.
    /// OID mapping will be overwritten if specified OID value was already added to dictionary.
    /// </summary>
    /// <param name="oidValue">OID value in a digit-dot form.</param>
    /// <param name="friendlyName">A friendly name for OID value.</param>
    /// <exception cref="ArgumentNullException">
    ///     Either, <strong>oidValue</strong> or <strong>friendlyName</strong> parameters are null, empty or whitespace sequences.
    /// </exception>
    public static void Add(String oidValue, String friendlyName) {
        if (String.IsNullOrWhiteSpace(oidValue)) {
            throw new ArgumentNullException(nameof(oidValue));
        }
        if (String.IsNullOrWhiteSpace(friendlyName)) {
            throw new ArgumentNullException(nameof(friendlyName));
        }
        _oidLookup[oidValue] = friendlyName;
        _nameLookup[friendlyName] = oidValue;
    }
}
