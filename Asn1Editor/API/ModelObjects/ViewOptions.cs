using System;

namespace SysadminsLV.Asn1Editor.API.ModelObjects {
    class ViewOptions {
        public Boolean ShowTagNumber { get; set; }
        public Boolean ShowNodeOffset { get; set; }
        public Boolean ShowNodeLength { get; set; }
        public Boolean ShowInHex { get; set; }
        public Boolean ShowContent { get; set; }
        public Boolean ShowNodePath { get; set; }
        public Boolean ShowPayload { get; set; }
    }
}
