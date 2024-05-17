using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PinpointUI.modals
{
    public class SelectableColourTheme
    {
        /// <summary>
        /// This is esoteric - it's only to help map to the data grid on the ConfirmPublish modal.
        /// </summary>
        public string ConfigKey { get; set; }
        public string ThemeDisplayName { get; set; }
        public string MainHex { get; set; }
        public string AlternateHex { get; set; }

        public SelectableColourTheme(string configkey, string themedisplay, string mainhex, string alternatehex)
        {
            ConfigKey = configkey;
            ThemeDisplayName = themedisplay;
            MainHex = mainhex;
            AlternateHex = alternatehex;
        }
    }
}
