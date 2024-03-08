using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace PinpointOnenote.OneNoteClasses
{
    public class OneNoteTable
    {
        // has Headers, data (List>List>OE), custom col widths (dictionary), table color theme (from XML), table sizing (from xml)
        public bool hasHeaders { get; set; } //does the table have a header row? Controls the use of font size and shading in xml params
        public List<List<OneNoteTableCell>> dataRows { get; set; } = new List<List<OneNoteTableCell>>(); //This includes headers in row 1 if they exist.
        // Below - for any columns that you want to fix or cap width.
        // Each key is the position of the column (start at 0), and the value is a dictionary of string/string, where the ...
        // ... key is "width" (+ number as text for value) or "type" (with value of either "fix"/ "cap_at").
        public Dictionary<int, string> colWidths { get; set; } = new Dictionary<int, string>();
        public XElement colorXml { get; set; } //MANDATORY - colour patterns from the XML resource (ColorTheme)
        public XElement sizingXml { get; set; } //MANDATORY - font weights for the table from the XML resource (TableSizing)
        public bool bordersVisible { get; set; }

    }
}
