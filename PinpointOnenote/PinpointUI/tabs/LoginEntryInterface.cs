using PinpointOnenote;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PinpointUI.tabs
{
    public class LoginEntryInterface : LoginEntry
    {
        public string InterfaceStatusColour { get; set; } = "#FFFFFF";
        public string InterfaceStatusIcon { get; set; } = "";

        public LoginEntryInterface() : base() { } //copy of the base class raw constructor


        public LoginEntryInterface(LoginEntry copyTarget) : base(copyTarget) //copy of the constructor that isntatiates from a copy (clones)
        {
            if (copyTarget is LoginEntryInterface interfaceTarget)
            {
                InterfaceStatusColour = interfaceTarget.InterfaceStatusColour;
                InterfaceStatusIcon = interfaceTarget.InterfaceStatusIcon;
            }
        }
    }
}
