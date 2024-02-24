using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PinpointOnenote.OneNoteClasses
{
    /// <summary>
    /// The base OneNoteOE holds a <List> of these in its OEChildren property.
    /// It contains 3 props: a type (table or Base OE), and 2 lists - a list of OeTable and a list of BaseOE
    /// They both get instantiated as empty and should only hold 1 element, which is called in a .FirstOrDefault();
    /// The type property controls which one gets looked at. Wrapping them in lists makes it easier to be flexible.
    /// </summary>
    public class OneNoteOEChild // I THINK THIS IS DEFUNCT
    {
        public OneNoteOEType Type { get; set; }
        public List<OneNoteOE> BaseOE { get; set; } = new List<OneNoteOE>();
        public List<OneNoteTable> TableOE { get; set; } = new List<OneNoteTable>();
        public bool isWrapped { get; set; } = false; // should we wrap the child in an OEChildren?
    }
}
