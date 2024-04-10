using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace PinpointOnenote
{
    public class OneNoteSection
    {
        // This is section in the sense of a section within a Notebook containing pages, not a QSI-tagged section as part of a OneNote Page.
        public string SectionName { get; set; }
        public string SectionID { get; set; }
        public string SectionGroup { get; set; }
        public string SectionGroupID { get; set; }
        public string Notebook { get; set; }
        public bool IsLocked { get; set; }
        public bool? IsValidPinPointInstance { get; set; }
        public string IsValidPinPointInstanceDisplay {
            get {
                if (IsValidPinPointInstance == null)
                {
                    return "NA";
                }
                else if (IsValidPinPointInstance == true)
                {
                    return "Yes";
                }
                else { return "No"; }
            } 
        }
        public string IsValidTooltip { get; set; }

        public XmlNode SectionXML { get; set; }
    }
}
