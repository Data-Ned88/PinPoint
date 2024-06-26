﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PinpointOnenote
{
    public class LoginEntry
    {
        public int id { get; set; } = -99;
        public LoginTypes LoginType { get; set; } = LoginTypes.NotSet;
        public string LoginDescription { get; set; }
        public string LoginUrl { get; set; }
        public string LoginUsername { get; set; }
        public string LoginPass { get; set; }
        public bool HasTwoFa { get; set; } = false;
        public string TwoFaMethod { get; set; }
        public DateTime? LastModified { get; set; } = null;
        public int LastModifiedSort { get; set; } = -1;

        public LoginStrength LoginStrength 
        { 
            get
            { 
                return new LoginStrength(LoginType, LoginPass, LoginUsername, HasTwoFa); 
            } 
        }

        public LoginEntry()
        {

        }


        public LoginEntry(LoginEntry copyTarget)
        {
            LoginType = copyTarget.LoginType;
            LoginDescription = copyTarget.LoginDescription;
            LoginUrl = copyTarget.LoginUrl;
            LoginUsername = copyTarget.LoginUsername;
            LoginPass = copyTarget.LoginPass;
            HasTwoFa = copyTarget.HasTwoFa;
            TwoFaMethod = copyTarget.TwoFaMethod;
            LastModified = copyTarget.LastModified;
        }
    }
}
