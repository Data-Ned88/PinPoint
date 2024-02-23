using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace PinpointOnenote.OneNoteClasses
{
    //The file is called OneNoteSpan.cs but contains all the granular classes needed to format onenote pages.  
    public class OneNoteSpan
    {
        public string rawText { get; set; } //MANDATORY - text to appear on the Page without HTML and CSS wrapping. For width calculation.
        //OPTIONAL - Populate the below if the span within a line has to differ from the rest of the line in font size. Must be in (9.0,10.0,10.5,11.0-16.0,18.0)
        public string customFontWeight { get; set; }
        //OPTIONAL - Same as above comment/property but for font family. Must be in (Arial,Calibri,Times New Roman).
        public string customFont { get; set; }
        public bool isBold { get; set; } // boldness of text can only be set at the CDATA/span level.
        public string HTML { get; set; } //MANDATORY - HTML formatted text to add to the XML.      
    }


}
