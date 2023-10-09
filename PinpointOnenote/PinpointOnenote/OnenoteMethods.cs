using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using Microsoft.Office.Interop.OneNote;

namespace PinpointOnenote
{
    public static class OnenoteMethods
    {
        public static bool IsOnenoteOpen ()
        {
            Microsoft.Office.Interop.OneNote.Application app = new Microsoft.Office.Interop.OneNote.Application();

            if (app.Windows.Count < 1)
            {
                return false;
            }
            else
            {
                return true;
            }
        }
    }
}
