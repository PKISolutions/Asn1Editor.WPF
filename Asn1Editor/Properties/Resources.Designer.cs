﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace SysadminsLV.Asn1Editor.Properties {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "17.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class Resources {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Resources() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("SysadminsLV.Asn1Editor.Properties.Resources", typeof(Resources).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Overrides the current thread's CurrentUICulture property for all
        ///   resource lookups using this strongly typed resource class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to &lt;!DOCTYPE html&gt;
        ///
        ///&lt;html lang=&quot;en&quot; xmlns=&quot;http://www.w3.org/1999/xhtml&quot;&gt;
        ///&lt;head&gt;
        ///    &lt;meta charset=&quot;utf-8&quot; /&gt;
        ///    &lt;title&gt;License Agreement&lt;/title&gt;
        ///    &lt;style type=&quot;text/css&quot;&gt;
        ///        body {
        ///            font: 13pt Helvetica
        ///        }
        ///
        ///        .smaller {
        ///            font-size: small;
        ///        }
        ///    &lt;/style&gt;
        ///&lt;/head&gt;
        ///&lt;body&gt;
        ///    &lt;h1&gt;Microsoft Public License (Ms-PL)&lt;/h1&gt;
        ///
        ///    &lt;p&gt;This license governs use of the accompanying software. If you use the software, you accept this license. If you do not ac [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string License {
            get {
                return ResourceManager.GetString("License", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Tag    : {0} (0x{0:X2}) : {1}
        ///Offset : {2} (0x{2:X2})
        ///Length : {3} (0x{3:X2})
        ///Depth  : {4}
        ///Path   : {5}
        ///.
        /// </summary>
        internal static string TagEditorHeaderTemplate {
            get {
                return ResourceManager.GetString("TagEditorHeaderTemplate", resourceCulture);
            }
        }
    }
}
