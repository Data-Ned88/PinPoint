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
        public string IsValidTooltip
        {
            get
            {
                if (IsValidPinPointInstance == null)
                {
                    return "This section is locked.\n\nGo to OneNote and unlock this section,\nthen click 'Refresh Sections' below.";
                }
                else if (IsValidPinPointInstance == true)
                {
                    return "This section is a valid PinPoint password section.";
                }
                else { return "This section is not a valid PinPoint password section."; }
            }
        }
        public XmlNode SectionXML { get; set; }
    }
}
