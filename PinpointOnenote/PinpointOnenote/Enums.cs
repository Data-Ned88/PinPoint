using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace PinpointOnenote
{


    [TypeConverter(typeof(EnumDescriptionTypeConverter))]
    public enum LoginTypes
    {
        [Description("Password")]
        Password,
        [Description("PIN (6)")]
        PinSix,
        [Description("PIN (4)")]
        PinFour,
        [Description("Not Set")]
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
