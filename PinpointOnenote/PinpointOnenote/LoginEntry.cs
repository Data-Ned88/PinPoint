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
        public LoginTypes LoginType { get; set; }
        public string LoginDescription { get; set; }
        public string LoginUrl { get; set; }
        public string LoginUsername { get; set; }
        public string LoginPass { get; set; }
        public bool HasTwoFa { get; set; }
        public string TwoFaMethod { get; set; }
        public DateTime LastModified { get; set; }
        public int LastModifiedSort { get; set; }
        public int LoginStrength { get; set; }
    }
}
