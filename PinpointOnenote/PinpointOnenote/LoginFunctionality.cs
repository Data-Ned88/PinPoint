﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using System.Globalization;

namespace PinpointOnenote
{
    public static class LoginFunctionality
    {
        public static bool PasswordStemsFromUserName(string passV, string userV)
        {

            bool outputbool = false;

            int passLength = passV.Length;
            int userLength = userV.Length;

            
            bool stemThresholdMet = userLength >=5 && passLength - userLength <= 10;

            //For it to count as a stem:
            //1. the user needs to at least 5 letters (to stop over reporting)
            //2. and the password needs to have under 11 letters once you take the stem away from it. Otherwise, you would be left with a kind of strong password anyway,
            //  ...which would mitigate the security lost from stemming.

            if (!stemThresholdMet) 
            {
                return false; // any match would't count anyway
            }

            if (passV.ToLower().Contains(userV.ToLower()))
            {
                return true; // stem threshold has been met and simple check proves password contains user.
            }

            //Further analysis required as the threshold has been met but the simple match has failed.
            // Build a regex search term from userV which anticipates common text-to-numbers/symbols password replacements in the password.

            Dictionary<string, string> passwordReps = new Dictionary<string, string> { { "a", "4" }, { "b", "8" }, { "e", "3" }, 
                                                            { "g", "9" }, { "i", "\\!" }, { "l", "1" }, { "o", "0" }, { "s", "\\$" }, { "t", "7" } };

            string[] regexReserveds = { ".", "\\", "=", "[", "]", "(", ")", "{", "}", "?", "!","+","*" };
            StringBuilder sb = new StringBuilder();
            foreach (char t in userV.ToLower())
            {
                string t_string = t.ToString();
                if (regexReserveds.Contains(t_string))
                {
                    t_string = "\\" + t_string;
                }

                if (passwordReps.ContainsKey(t.ToString()))
                {
                    sb.Append("[" + t_string + passwordReps[t.ToString()] + "]");
                }
                else { sb.Append(t_string); }
            }
            string regexUserV = sb.ToString();
            Regex rxUser = new Regex(regexUserV);

            if (rxUser.IsMatch(passV.ToLower()))
            {
                outputbool = true;
            }


            return outputbool;
        }

        public static int PasswordComplexityLevel (string passwordValue)
        {
            //Level 0: Numeric only (10 chars)
            //Level 1: Single case letters only  (26)|| symbols only (20)
            //Level 2: MixedCase letters only (26+26) || Numbers + Single Case (10+26) || Single Case + Symbol (26+20) || Numbers + Symbol (10+20)
            //Level 3: Numbers + MixedCase (10 + 26 + 26) || Mixed Case + Symbol (26 + 26 + 20) || single + symbol + number (26 +20 +10)
            //Level 4: Numbers + Mixedcase + Symbol (10 +26+26+20)

            int level;
            //string levelString = "";

            Regex rxSymbol = new Regex(@"\W|_");
            Regex rxNumber = new Regex(@"\d");
            Regex rxUcase = new Regex(@"[A-Z]");
            Regex rxLcase = new Regex(@"[a-z]");

            bool symbolMatch = rxSymbol.IsMatch(passwordValue);
            bool numberMatch = rxNumber.IsMatch(passwordValue);
            bool upperMatch = rxUcase.IsMatch(passwordValue);
            bool lowerMatch = rxLcase.IsMatch(passwordValue);
            bool mixedMatch = lowerMatch && upperMatch;

            if (symbolMatch && mixedMatch && numberMatch)
            {
                level = 4;
                //levelString = "4: Numbers + Mixed Case + Symbol";
            }
            else if (mixedMatch && (numberMatch || symbolMatch))
            {
                level =3;
                //levelString = "3. Numbers + MixedCase (10 + 26 + 26) || Mixed Case + Symbol (26 + 26 + 20)";
            }
            else if (numberMatch && symbolMatch && (lowerMatch || upperMatch))
            {
                level = 3;
                //levelString = "3. single + symbol + number (26 +20 +10)";
            }
            else if (mixedMatch)
            {
                level = 2;
                //levelString = "2. MixedCase letters only (26+26)";
            }
            else if (numberMatch && symbolMatch)
            {
                level = 2;
                //levelString = "2. Numbers + Symbol (10+20)";
            }
            else if (numberMatch && (lowerMatch || upperMatch))
            {
                level = 2;
                //levelString = "2. Numbers + Single Case (10+26)";
            }
            else if (symbolMatch && (lowerMatch || upperMatch))
            {
                level = 2;
                //levelString = "2. Symbol + Single Case (10+26)";
            }
            else if (symbolMatch || lowerMatch || upperMatch)
            {
                level = 1;
                //levelString = "1. Single case letters only (26)|| symbols only (20)";
            }
            else if (numberMatch)
            {
                level = 0;
                //levelString = "0. Numbers only";
            }
            else
            {
                level = -1;
                //levelString = "-1. Nothing at all";
            }
            return level;
        }

        public static Dictionary<string,string> PinSixScoreValues(string pinV)
        {
            Dictionary<string, string> returnDict = new Dictionary<string, string>();
            string[] toptwenty = { "123456", "123123", "111111", "121212", "123321", "666666", "000000", "364321", "696969", 
                "112233", "159753", "292513", "131313", "123654","222222", "789456", "999999", "101010", "777777","007007" };
            List<string> DatePatterns = new List<string> { "MMddyy", "ddMMyy", "MMyyyy", "yyyyMM"};

            Regex rxPinSixValid = new Regex(@"[0-9]{6}");
            Regex rxallSameNumber = new Regex(@"(\d)\1{5}");
            Regex rxSameThreeTwice = new Regex(@"(\d\d\d)\1");
            Regex rxSameTwoThrice = new Regex(@"(\d\d)\1{2}");
            Regex rxTwoOfThreeIdentical = new Regex(@"(\d)\1{2}(\d)\2{2}");
            Regex rxThreeOfTwoIdentical = new Regex(@"(\d)\1(\d)\2(\d)\3");

            if (pinV == "")
            {
                returnDict.Add("score", "-99");
                returnDict.Add("scoreText", "PIN not set.");
            }
            else if (!rxPinSixValid.IsMatch(pinV))
            {
                returnDict.Add("score", "-99");
                returnDict.Add("scoreText", "PIN is not a valid 6-digit PIN.");
            }
            else if (toptwenty.Contains(pinV))
            {
                returnDict.Add("score", "0");
                returnDict.Add("scoreText", "In top-20 most selected 6-digit PINs.");
            }
            else if (rxallSameNumber.IsMatch(pinV))
            {
                returnDict.Add("score", "0");
                returnDict.Add("scoreText", "PIN is the same number repeated 6 times");
            }
            else if (isMatchDate(pinV, DatePatterns))
            {
                returnDict.Add("score", "16");
                returnDict.Add("scoreText", "Matches a memorable date pattern.");
            }
            else if (rxSameThreeTwice.IsMatch(pinV))
            {
                returnDict.Add("score", "32");
                returnDict.Add("scoreText", "PIN is the same 3-digit combo twice.");
            }
            else if (rxTwoOfThreeIdentical.IsMatch(pinV))
            {
                returnDict.Add("score", "32");
                returnDict.Add("scoreText", "PIN is two sets of a triplicate number (eg. 000111).");
            }
            else if (rxSameTwoThrice.IsMatch(pinV))
            {
                returnDict.Add("score", "48");
                returnDict.Add("scoreText", "PIN is the same 2-digit combo three times (eg.676767).");
            }
            else if (rxThreeOfTwoIdentical.IsMatch(pinV))
            {
                returnDict.Add("score", "48");
                returnDict.Add("scoreText", "PIN is three sets of a duplicate number (eg. 445566).");
            }

            else
            {
                returnDict.Add("score", "100");
                returnDict.Add("scoreText", "PIN does not match predictable guess pattern and is secure.");
            }


            return returnDict;
        }

        public static Dictionary<string, string> PinFourScoreValues(string pinV)
        {
            Dictionary<string, string> returnDict = new Dictionary<string, string>();
            string[] toptwenty = { "1234", "1212", "1004", "2000", "6969", "1122", "1313", "4321", "2001", "1010"};
            string[] downTheMiddle = { "2580", "0852"};
            string[] incrementsOfTwo = { "2468", "0246", "4680", "0864", "6420", "8642", "1357", "3579", "9753", "7531"};

            List<string> DatePatternsYear = new List<string> { "yyyy"};
            List<string> DatePatternsDayMonth = new List<string> { "ddMM", "MMdd" };

            Regex rxPinFourValid = new Regex(@"[0-9]{4}");
            Regex rxallSameNumber = new Regex(@"(\d)\1{3}");
            Regex rxSameTwoTwice = new Regex(@"(\d\d)\1");
            Regex rxTwoOfTwoIdentical = new Regex(@"(\d)\1(\d)\2");


            if (pinV == "")
            {
                returnDict.Add("score", "-99");
                returnDict.Add("scoreText", "PIN not set.");
            }
            else if (!rxPinFourValid.IsMatch(pinV))
            {
                returnDict.Add("score", "-99");
                returnDict.Add("scoreText", "PIN is not a valid 4-digit PIN.");
            }
            else if (toptwenty.Contains(pinV) || rxallSameNumber.IsMatch(pinV))
            {
                returnDict.Add("score", "0");
                returnDict.Add("scoreText", "In top-20 most selected 4-digit PINs.");
            }
            else if (isMatchDate(pinV, DatePatternsYear))
            {
                returnDict.Add("score", "16");
                returnDict.Add("scoreText", "Matches a likely year of birth (1943-2024).");
            }
            else if (downTheMiddle.Contains(pinV))
            {
                returnDict.Add("score", "16");
                returnDict.Add("scoreText", "Straight down/up the middle of a phone keyboard - a commonly used pattern.");
            }
            else if (isMatchDate(pinV, DatePatternsDayMonth))
            {
                returnDict.Add("score", "32");
                returnDict.Add("scoreText", "Matches a DD/MM or MM/DD month-date pattern.");
            }
            else if (rxSameTwoTwice.IsMatch(pinV))
            {
                returnDict.Add("score", "48");
                returnDict.Add("scoreText", "PIN is the same 2-digit combo twice (eg 3636).");
            }
            else if (rxTwoOfTwoIdentical.IsMatch(pinV))
            {
                returnDict.Add("score", "48");
                returnDict.Add("scoreText", "PIN is two sets of a duplicate number (eg. 2244).");
            }
            else
            {
                returnDict.Add("score", "100");
                returnDict.Add("scoreText", "PIN does not match predictable guess pattern and is secure.");
            }

            return returnDict;
        }

        public static bool isMatchDate (string pinVal, List<string> dateFormats)
        {
            bool outputBool = false;

            DateTime minDate = DateTime.ParseExact("01/01/1943", "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None);
            DateTime maxDate = DateTime.Today;
            List<DateTime> matchedDates = new List<DateTime>();
            //DateTime? dateFound;
            foreach (string dateFormat in dateFormats)
            {
                if (dateFormat.EndsWith("yy") && !dateFormat.EndsWith("yyyy")) // It contains a 2-digit year. (has to come from 6-digit) pad with a 20 and a 19
                {
                    string pinStart = pinVal.Substring(0, pinVal.Length - 2);
                    string pinEnd = pinVal.Substring(pinVal.Length - 2, 2);
                    string matchattemptOne = pinStart + "19" + pinEnd;
                    string matchattemptTwo = pinStart + "20" + pinEnd;
                    string dateFormatAttempt = dateFormat + "yy";

                    if (DateTime.TryParseExact(matchattemptOne, dateFormatAttempt, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime parsedDate))
                    {
                        matchedDates.Add(parsedDate);
                    }
                    if (DateTime.TryParseExact(matchattemptTwo, dateFormatAttempt, CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedDate))
                    {
                        matchedDates.Add(parsedDate);
                    }
                }
                else if (dateFormat == "MMdd"|| dateFormat == "ddMM")
                {
                    string matchattemptOne = pinVal + "2000"; // leap year in range
                    string dateFormatAttempt = dateFormat + "yyyy";
                    if (DateTime.TryParseExact(matchattemptOne, dateFormatAttempt, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime parsedDate))
                    {
                        matchedDates.Add(parsedDate);
                    }

                }
                else
                {
                    if (DateTime.TryParseExact(pinVal, dateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime parsedDate))
                    {
                        matchedDates.Add(parsedDate);
                    }
                }
            }
            foreach (DateTime matchedDate in matchedDates)
            {
                bool isBetweenTargets = matchedDate >= minDate && matchedDate <= maxDate;
                if (isBetweenTargets)
                {
                    outputBool = true;
                    break;
                }
            }

            return outputBool;
        }
        
    }
}