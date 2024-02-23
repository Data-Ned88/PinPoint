using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace PinpointOnenote.OneNoteClasses
{
    public class OneNoteQuickStyleDef

    {
        // The defaults for this are based on the p tag.
        public string name { get; set; }
        public string fontColor { get; set; } = "automatic";
        public string highlightColor { get; set; } = "automatic";
        public string font { get; set; } = "Calibri";
        public string fontSize { get; set; } = "11.0";
        public string spaceBefore { get; set; } = "0.0";
        public string spaceAfter { get; set; } = "0.0";

        public OneNoteQuickStyleDef (string Name)
        {
            name = Name; // constructor to build it with a simple name and accept the rest of the defaults.
        }
        public OneNoteQuickStyleDef(Dictionary<string,string> kwargs)
        {
            if (!kwargs.ContainsKey("name"))
            {
                throw new Exception("Script tried to instantiate a OneNoteQuickStyleDef object with no name property.");
            }
            foreach (var property in GetType().GetProperties())
            {
                if (kwargs.ContainsKey(property.Name))
                {
                    property.SetValue(this, kwargs[property.Name]);
                }
            }
        }
    }
}
