using PinpointOnenote;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PinpointUI.tabs
{
    public class LoginEntryInterfaceFunctionality
    {
        public static void HydrateIdColl(ObservableCollection<LoginEntryInterface> pBank)
        {
            // self hydrate ids
            for (int i = 0; i < pBank.Count; i++)
            {
                pBank[i].id = i;
            }
        }
        /// <summary>
        /// Converts an Interface-version of the password bank back to a normal version.
        /// </summary>
        /// <param name="pBank"></param>
        /// <returns></returns>
        public static List<LoginEntry> GetPublishableBankFromInterface (ObservableCollection<LoginEntryInterface> pBank)
        {
            List<LoginEntry>  returnPassBank = new List<LoginEntry>();

            foreach (LoginEntryInterface lei in pBank)
            {
                LoginEntry le = new LoginEntry();
                le.LoginType = lei.LoginType;
                le.LoginDescription = lei.LoginDescription;
                le.LoginUrl = lei.LoginUrl;
                le.LoginUsername = lei.LoginUsername;
                le.LoginPass = lei.LoginPass;
                le.HasTwoFa = lei.HasTwoFa;
                le.TwoFaMethod = lei.TwoFaMethod;
                le.LastModified = lei.LastModified;

                returnPassBank.Add(le);
            }

            return returnPassBank;
        }
    }
}
