using PinpointOnenote;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace PinpointUI.tabs
{
    public class LoginTypeToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is LoginTypes loginType)
            {
                switch (loginType)
                {
                    case LoginTypes.Password:
                        return "Password";
                    case LoginTypes.PinSix:
                        return "PIN (6)";
                    case LoginTypes.PinFour:
                        return "PIN (4)";
                    case LoginTypes.NotSet:
                        return "Not Set";
                }
            }
            return "Unknown";
        }



        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string loginTypeString)
            {
                switch (loginTypeString)
                {
                    case "Password":
                        return LoginTypes.Password;
                    case "PIN (6)":
                        return LoginTypes.PinSix;
                    case "PIN (4)":
                        return LoginTypes.PinFour;
                    case "Not Set":
                        return LoginTypes.NotSet;
                }
            }
            return null;
        }
    }
}