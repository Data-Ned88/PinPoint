using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PinpointOnenote.OneNoteClasses
{
    public class OneNoteOE
    {
        //only for OE As Section. If this is true, any children of the OE are NOT indented, so don't wrap the children in an <OEChildren> ...
        // ... and we skip processing of the textLine, and inherited indents is not added to.
        public OneNoteOEType oeType { get; set; }
        public bool isHeaderless { get; set; } = false;  
        public OneNoteT textLine { get; set; } // main text with optional bullet point. Not used if section and isHeaderless, or if Oe Is Table.
        public OneNoteTable table { get; set; } // only use this if this is a OneNote table.
        //MANDATORY - give it the name of a custom tag you want to give it (section), or "p" for generic.
        //As a strategy, we're just using this for tagging. We'll override with style CSS proeprties on the OE at all times for ease of lineage tracking.
        public string quickStyleIndexName { get; set; }
        public string alignment { get; set; } = "left"; //(left/center/right)
        public string fontFamily { get; set; } //MANDATORY> This contributes to InheritedFont in textLine and to the CssStyle
        public string fontWeight { get; set; } //MANDATORY> This contributes to InheritedFontWeight in textLine and to the CssStyle
        public string fontColor { get; set; } = "black"; //MANDATORY> This contributes to the CssStyle
        public string CssStyle
        {
            get
            {
                return $"'font-family:{fontFamily};font-size:{fontWeight}pt;color:{fontColor}'";
            }
        }
        public int inheritedIndents { get; set; } = 0; // THis is ONLY USED to ADD onto the indentation for column width calculation. OneNote controls indents itself by recognising OEChildren

        public List<OneNoteOE> OEChildren = new List<OneNoteOE>();

    }
}
