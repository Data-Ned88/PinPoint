using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PinpointOnenote
{
    public class LoginEntry
    {
        public int id { get; set; }
        public LoginTypes LoginType { get; set; } = LoginTypes.NotSet;
        public string LoginDescription { get; set; }
        public string LoginUrl { get; set; }
        public string LoginUsername { get; set; }
        public string LoginPass { get; set; }
        public bool HasTwoFa { get; set; } = false;
        public string TwoFaMethod { get; set; }
        public DateTime? LastModified { get; set; } = null;
        public int LastModifiedSort { get; set; } = -1;
        public int LoginStrength { get; set; } = -99;
    }
}
