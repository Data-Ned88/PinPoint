using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace PinpointOnenote
{
    public enum LoginTypes
    {
        Password,
        PinSix,
        PinFour,
        NotSet
    }
    public enum  OneNoteOEType
    {
        Table,
        BaseOE,
        Section
    }
    public enum AllowableFonts // THis has a method GetAllowableFontAsStr in DataParsers.cs to translate these into string so taht they can query XML reosurces.
    {
        Arial,
        Calibri,
        TimesNewRoman
    }

}
