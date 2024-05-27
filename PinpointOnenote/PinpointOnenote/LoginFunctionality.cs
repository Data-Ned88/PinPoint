using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using System.Globalization;
using System.Diagnostics.SymbolStore;
using System.Security.Cryptography;

namespace PinpointOnenote
{
    public static class LoginFunctionality
    {
        public static bool PasswordOne(string passV, string compare="password")
        {
            bool outputbool = false;
            Dictionary<string, string> passwordReps = new Dictionary<string, string> { { "a", "4" },{ "o", "0" } };

            StringBuilder sb = new StringBuilder();
            foreach (char t in compare.ToLower())
            {
                string t_string = t.ToString();

                if (passwordReps.ContainsKey(t.ToString()))
                {
                    sb.Append("[" + t_string + passwordReps[t.ToString()] + "]");
                }
                else { sb.Append(t_string); }
            }
            string regexCompareV = sb.ToString();
            Regex rxCompare = new Regex(regexCompareV);

            if (rxCompare.IsMatch(passV.ToLower()))
            {
                outputbool = true;
            }
            return outputbool;
        }
        public static bool PasswordStemsFromUserName(string passV, string userV)
        {

            bool outputbool = false;

            string EmailPattern = @"^([a-zA-Z0-9._%\+&]+)@[^@\s]+\.[a-zA-Z]{2,}$";
            Regex Emailregex = new Regex(EmailPattern);
            Match email = Emailregex.Match(userV);
            if (email.Success)
            {
                userV = email.Groups[1].Value;
            }


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



        public static bool isValidPinSix (string input)
        {
            Regex rxPinSixValid = new Regex(@"^[0-9]{6}$");
            if (input == null)
            {
                return false;
            }
            else if (rxPinSixValid.IsMatch(input)) {
                return true;
            }
            else
            {
                return false;
            }
        }
        public static bool isValidPinFour(string input)
        {
            Regex rxPinFourValid = new Regex(@"^[0-9]{4}$");
            if (input == null)
            {
                return false;
            }
            else if (rxPinFourValid.IsMatch(input))
            {
                return true;
            }
            else
            {
                return false;
            }
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
            else if (isSequence(pinV))
            {
                returnDict.Add("score", "32");
                returnDict.Add("scoreText", "PIN is a straight sequence of numbers (eg 345678)");
            }
            else if (isSequence(pinV,true))
            {
                returnDict.Add("score", "48");
                returnDict.Add("scoreText", "PIN is an up-down/down-up pyramid sequence of numbers (eg 234432)");
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

        public static bool isSequence(string pinVal, bool Pyramid = false)
        {
            //Pin 6 only as the pyramid sequencing is hard-coded to 6.
            bool outputBool = true;

            List<int> numberList = new List<int>();
            foreach (char c in pinVal)
            {
                // Add each character as a string to the list
                numberList.Add(int.Parse(c.ToString()));
            }

            if (Pyramid)
            {
                List<int> firstHalf = numberList.GetRange(0,3);
                List<int> secondHalf = numberList.GetRange(3, 3);
                secondHalf.Reverse();
                int prevNumber = -99;
                int lastdiff = 500;
                int shIncr = 0;
                foreach (int num in firstHalf)
                {
                    int shEquivalent = secondHalf[shIncr];
                    int diff = num - prevNumber;
                    if (prevNumber != -99) //We're not looking at the placehodler for the first go
                    {
                        if (!(Math.Abs(diff) >= 1 && Math.Abs(diff) <= 2)) //It's gone up or or down by more than 1 or 2
                        {
                            outputBool = false;
                            return outputBool;
                        }
                        else if (Math.Abs(lastdiff) < 20 && lastdiff != diff) //The last diff is something sensible (max 9 (9-0 or 0-9)), and it's not the same as the latest one
                        {
                            outputBool = false;
                            return outputBool;
                        }
                    }
                    if (num != shEquivalent)
                    {
                        outputBool = false;
                        return outputBool;
                    }
                    prevNumber = num;
                    lastdiff = diff;
                    shIncr++;
                }


            }
            else
            {
                int prevNumber = -99;
                int lastdiff = 500;
                foreach (int num in numberList)//1
                {
                    int diff = num - prevNumber; //100
                    if (prevNumber != -99) //We're not looking at the placehodler for the first go
                    {
                        if (!(Math.Abs(diff) >=1 && Math.Abs(diff) <= 2)) //It's gone up or or down by more than 1 or 2
                        {
                            outputBool = false;
                            return outputBool;
                        }
                        else if (Math.Abs(lastdiff) < 20 && lastdiff!=diff) //The last diff is something sensible (max 9 (9-0 or 0-9)), and it's not the same as the latest one
                        {
                            outputBool = false;
                            return outputBool;
                        }
                    }
                    prevNumber = num;
                    lastdiff = diff;
                }
            }


            return outputBool;
        }
        public static string generateSecureRandomPassword(int nCharacters = 13, bool hasSymbols = true)
        {
            // Define character sets for password generation
            const string letters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";
            const string digits = "01234567890123456789";
            const string symbols = "!@#$%^&*()-_!@#$%^&*()-_";

            // Combine character sets based on input options
            string charsToUse = letters + digits;
            if (hasSymbols)
            {
                charsToUse += symbols;
            }

            // Initialize a random number generator for secure randomness
            RandomNumberGenerator rng = RandomNumberGenerator.Create();

            // Create a StringBuilder to store the password
            StringBuilder password = new StringBuilder(nCharacters);

            // Generate random characters until the password reaches the desired length
            byte[] randomBuffer = new byte[nCharacters];
            rng.GetBytes(randomBuffer);

            for (int i = 0; i < nCharacters; i++)
            {
                // Convert the random byte into a range index for the charsToUse string
                int rangeIndex = randomBuffer[i] % charsToUse.Length;
                // Append the selected character to the password
                password.Append(charsToUse[rangeIndex]);
            }

            // Return the generated password as a string
            return password.ToString();
        }
    

        public static string generateSecurePasswordFromStem(string stem)
        {
            // Take in a stem, convert its letters to lowercase and sub in sybols/numbers if theya re in a list of available replacements...
            // ... also pad it with a random combination of Symbol,number, upprecase, symbol on both sides (same padding for eas of memory).
            string symbols = "!@#$%^&*()-_!@#$%^&*()-_";
            string numbers = "0123456789";
            string ucase = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            
            // Initialize a random number generator
            //Random random = new Random();
            RandomNumberGenerator rng = RandomNumberGenerator.Create();
            byte[] randomBuffer = new byte[2];
            rng.GetBytes(randomBuffer);
            int rangeIndex;

            //Phase 1 - convert stem characters into number and symbol equivalents.

            Dictionary<string, string> passwordReps = new Dictionary<string, string> { { "a", "4" }, { "b", "8" }, { "e", "3" },
                                                            { "g", "9" }, { "i", "!" }, { "l", "1" }, { "o", "0" }, { "s", "$" }, { "t", "7" } };

            
            StringBuilder sb = new StringBuilder();
            
            StringBuilder sbPadding = new StringBuilder();
            rangeIndex = randomBuffer[0] % symbols.Length;
            sbPadding.Append(symbols[rangeIndex]);
            rangeIndex = randomBuffer[0] % numbers.Length;
            sbPadding.Append(numbers[rangeIndex]);
            rangeIndex = randomBuffer[0] % ucase.Length;
            sbPadding.Append(ucase[rangeIndex]);
            rangeIndex = randomBuffer[1] % symbols.Length;
            sbPadding.Append(symbols[rangeIndex]);
            string Padding = sbPadding.ToString();

            sb.Append(Padding);

            
            foreach (char t in stem)
            {
                
                string t_string_lower = t.ToString().ToLower();


                if (passwordReps.ContainsKey(t_string_lower))
                {
                    sb.Append(passwordReps[t_string_lower]);
                }
                else 
                { 
                    sb.Append(t_string_lower);
                }
                
            }
            sb.Append(Padding);


            // Return the generated password as a string
            return sb.ToString();
        }

        public static string generateRandomPin(int pLength = 4)
        {
            string numbers = "0123456789";
            //Random random = new Random();


            RandomNumberGenerator rng = RandomNumberGenerator.Create();
            byte[] randomBuffer = new byte[6];
            rng.GetBytes(randomBuffer);
            int rangeIndex;


            StringBuilder sb = new StringBuilder();
            rangeIndex = randomBuffer[0] % numbers.Length;
            sb.Append(numbers[rangeIndex]);
            rangeIndex = randomBuffer[1] % numbers.Length;
            sb.Append(numbers[rangeIndex]);
            rangeIndex = randomBuffer[2] % numbers.Length;
            sb.Append(numbers[rangeIndex]);
            rangeIndex = randomBuffer[3] % numbers.Length;
            sb.Append(numbers[rangeIndex]);

            if (pLength == 6)
            {
                rangeIndex = randomBuffer[4] % numbers.Length;
                sb.Append(numbers[rangeIndex]);
                rangeIndex = randomBuffer[5] % numbers.Length;
                sb.Append(numbers[rangeIndex]);
            }

            return sb.ToString();
        }

        public static string generateSecurePinFour()
        {
            string pinFour = generateRandomPin();
            Dictionary<string, string> testPIN = PinFourScoreValues(pinFour);
            bool passesTest = testPIN["score"] == "100";

            while (!passesTest)
            {
                pinFour = generateRandomPin();
                testPIN = PinFourScoreValues(pinFour);
                passesTest = testPIN["score"] == "100";
            }

            return pinFour;
        }
        public static string generateSecurePinSix()
        {
            string pinSix = generateRandomPin(6);
            Dictionary<string, string> testPIN = PinSixScoreValues(pinSix);
            bool passesTest = testPIN["score"] == "100";

            while (!passesTest)
            {
                pinSix = generateRandomPin(6);
                testPIN = PinSixScoreValues(pinSix);
                passesTest = testPIN["score"] == "100";
            }

            return pinSix;
        }

        public static List<LoginEntry> HydrateIdAndModifiedSort(List<LoginEntry> pBank)
        {
            // self hydrate ids
            for (int i = 0; i < pBank.Count; i++)
            {
                pBank[i].id = i;
            }
            //Sort so that blank last modified's are at the bottom.
            List<LoginEntry> sortedLoginEntries = pBank.OrderBy(entry => entry.LastModified == null)
                .ThenByDescending(entry => entry.LastModified).ToList();


            int lastmodifiedSortInc = 1;
            for (int i = 0; i < pBank.Count; i++)
            {
                sortedLoginEntries[i].LastModifiedSort = lastmodifiedSortInc;
                if (sortedLoginEntries[i].LastModified != null)
                {
                    lastmodifiedSortInc++;
                }
            }

            return sortedLoginEntries.OrderBy(entry => entry.id).ToList();
        }
        public static Dictionary<string, Dictionary<string, int>> GetExactShares (List<LoginEntry> passwordBank, LoginTypes lType)
        {
            Dictionary<string, Dictionary<string, int>> returnDict = new Dictionary<string, Dictionary<string, int>>();

            List<LoginEntry> passwordBankLoginTypeSubset = passwordBank.Where(x => x.LoginType == lType).ToList();

            if (passwordBankLoginTypeSubset.Any())
            {
                HashSet<string> uniquePasses = new HashSet<string>();
                foreach (LoginEntry le in passwordBankLoginTypeSubset)
                {
                    uniquePasses.Add(le.LoginPass);
                }
                foreach (string passValue in uniquePasses)
                {
                    Dictionary<string, int> passDict = new Dictionary<string, int>();
                    int countShares = 0;
                    int dvs = 0;
                    if (passwordBankLoginTypeSubset.Where(x => x.LoginPass == passValue).Count() > 1)
                    {
                        countShares = passwordBankLoginTypeSubset.Where(x => x.LoginPass == passValue).Count() - 1;
                        int minStrength = passwordBankLoginTypeSubset.Where(x => x.LoginPass == passValue).Select(x => x.LoginStrength.Score).Min();
                        dvs = countShares * (100 - minStrength);
                    }
                    passDict.Add("count_shared", countShares);
                    passDict.Add("dvs_total", dvs);
                    if (lType == LoginTypes.Password) // If it's a password, check for exact shares against Pin4 and Pin6 and and 100 for each.
                    {
                        int countSharesPinFour = passwordBank.Where(x => x.LoginType == LoginTypes.PinFour && x.LoginPass == passValue).Count();
                        int countSharesPinSix = passwordBank.Where(x => x.LoginType == LoginTypes.PinSix && x.LoginPass == passValue).Count();
                        passDict.Add("count_shared_pin_four", countSharesPinFour);
                        passDict.Add("dvs_total_pin_four", countSharesPinFour * 100);
                        passDict.Add("count_shared_pin_six", countSharesPinSix);
                        passDict.Add("dvs_total_pin_six", countSharesPinSix * 100);
                    }
                    returnDict.Add(passValue, passDict);
                }
            }


            return returnDict;
        }
        public static List<Dictionary<string,int>> getPotentialStems (string pWord,int stringLength) //TESTED
        {
            // This is used by getAllPotentialStemsForPassword and returns all the potential stems in a password, based on defined stem identifiaction regex algorithms.
            // stringLength int param sets the exact length of the text bit of the stem. You would run this from 4 to infinite value for stringLEngth until the length of lsit return = 0. Then stop.
            List<Dictionary<string, int>> returnList = new List<Dictionary<string, int>>();
            Dictionary<string, string> passwordReps = new Dictionary<string, string> { { "4","a" }, {"8","b" }, {"3","e" },
                                                            {"9","g" }, { "!","i" }, { "1","l" }, {  "0","o" }, { "$", "s"}, { "7","t" } };

            //1. Need to get the above to be non-greedy, or get it so that it brings back all possible combinations of it, regardless of overlap.
            string regexStemMainString = "(?=([a-zA-Z1347890\\$\\!]{" + stringLength.ToString() + "}))";
            Regex regexStemMain = new Regex(regexStemMainString);
            Regex consonantClusters = new Regex(@"[sdfqwtlpmnbvcxzgjklh]{4,}");
            Regex consonantClustersExpansive = new Regex(@"(?=([dfqwtlpmnbvcxzgjkl]{3}))");
            Regex numberFalsePositives = new Regex(@"[1347890\\$\\!]{1,6}$|^[1347890\\$\\!]{1,6}"); // disqualification: number masks at the ends shouldn't count.

            Regex numbersBeforeMatch = new Regex(@"[0-9]{1,6}$");
            Regex numbersAfterMatch = new Regex(@"^[0-9]{1,6}");

            List<Match> firstPass = regexStemMain.Matches(pWord).Cast<Match>().ToList();



            //2. Then we lowercase them and translate out of number/char replacements for letters (B!ll >bill)
            //3. Then validate them against the consonant clustering, disqualifying them if so.

            List<Dictionary<string, Match>> cleanMatches = new List<Dictionary<string, Match>>();

            foreach (Match m in firstPass)
            {
                string rawMatch = m.Groups[1].Value;
                string matchReduced = rawMatch.ToLower();
                foreach (string k in passwordReps.Keys)
                {
                    matchReduced = matchReduced.Replace(k, passwordReps[k]); //2.
                }
                bool consClusters = consonantClusters.IsMatch(matchReduced);
                bool consClustersExpansiveGTonce = consonantClustersExpansive.Matches(matchReduced).Count > 1;
                bool suppress = consClusters || consClustersExpansiveGTonce || numberFalsePositives.IsMatch(rawMatch);
                if (!suppress) // 3. Validation and disqualification
                {
                    cleanMatches.Add(new Dictionary<string, Match> { { matchReduced, m } });
                }
            }


            //4. Then look at their originals in the password, and see if they have 0-6 solid numbers either side.
            //5. Then return the normalised stem to the list, both on its own and with the number versions. (all combos of number from 1 to highest found.)

            foreach (Dictionary<string, Match> cM in cleanMatches)
            {
                foreach(string cmK in cM.Keys)
                {
                    returnList.Add(new Dictionary<string, int> { { cmK, cM[cmK].Index } }
                        ); //5. part 1. Add the reduced version as a potential match in itself

                    string whatsBefore = pWord.Substring(0, cM[cmK].Groups[1].Index);
                    string whatsAfter = pWord.Substring(cM[cmK].Groups[1].Index + cM[cmK].Groups[1].Length);

                    //4.part 1. before
                    if (numbersBeforeMatch.IsMatch(whatsBefore))
                    {
                        string leadingnumbersMatched = numbersBeforeMatch.Match(whatsBefore).Value;
                        int indexBeforeIncr = 0;
                        for (int i = leadingnumbersMatched.Length -1; i >= 0; i--)
                        {
                            indexBeforeIncr++;
                            returnList.Add(new Dictionary<string, int> {
                                { leadingnumbersMatched.Substring(i) + cmK, cM[cmK].Index - indexBeforeIncr} 
                                }
                                ); // 5 part 2. reduced match with leading numbers 1 to highest found
                        }
                    }
                    //4.part 2. after
                    if (numbersAfterMatch.IsMatch(whatsAfter))
                    {
                        string trailingnumbersMatched = numbersAfterMatch.Match(whatsAfter).Value;
                        for (int i = 1; i <= trailingnumbersMatched.Length; i++)
                        { //cmK + trailingnumbersMatched.Substring(0,i)
                            returnList.Add(new Dictionary<string, int> {
                                { cmK + trailingnumbersMatched.Substring(0,i), cM[cmK].Index}
                                }
                                ); // 5 part 2. reduced match with trailing numbers 1 to highest found
                        }
                    }
                }
            }
            return returnList;
        }
        public static List<Dictionary<string, int>> getAllPotentialStemsForPassword(string pWord)
        {
            List<Dictionary<string, int>> allStems = new List<Dictionary<string, int>>();
            List<Dictionary<string, int>> stemsNLength;

            int stemsReturned = 1; // sets up the while loop
            int stemLength = 4; // minimum value
            while (stemsReturned > 0)
            {
                stemsNLength = getPotentialStems(pWord, stemLength);
                if (stemsNLength.Count > 0)
                {
                    allStems.AddRange(stemsNLength);
                }
                stemsReturned = stemsNLength.Count;
                stemLength++;
            }
            return allStems;
        }
        public static Dictionary<string, int> getAllSharedPasswordStemsInBank(List<List<Dictionary<string, int>>> allPasswordStemLists)
        {
            Dictionary<string, int>  uniqueSharedStems = new Dictionary<string, int>();

            foreach (List<Dictionary<string, int>> stemsForOnePassword in allPasswordStemLists)
            {
                // we want a hashset of all the keys (the normalised passwords)
                HashSet<string> uniqueStemsInPw = new HashSet<string>();
                foreach (Dictionary<string, int> stemKVpair in stemsForOnePassword)
                {
                    foreach (string k in stemKVpair.Keys)
                    {
                        uniqueStemsInPw.Add(k);
                    }
                }
                foreach (string uStem in uniqueStemsInPw)
                {
                    if (uniqueSharedStems.ContainsKey(uStem))
                    {
                        uniqueSharedStems[uStem] += 1;
                    }
                    else
                    {
                        uniqueSharedStems.Add(uStem, 1);
                    }
                }
            }

            // We now have the dictionary of all stems across passwords with count of password shared in. Remove where count = 1
            uniqueSharedStems = uniqueSharedStems.Where(pair => pair.Value > 1).ToDictionary(pair => pair.Key, pair => pair.Value);


            return uniqueSharedStems;
        }
        public static Dictionary<string, List<int>> getPasswordShareMatrix (Dictionary<string, int> sharedStems, 
            Dictionary<int, List<Dictionary<string, int>>> passwordIdsWithStems)
        {
            // INPUT PARAMS:
            // sharedStems - this is the result of getAllSharedPasswordStemsInBank - all shared stems across the bank
            // passwordIdsWithStems for each password in a bank (hydrated with ID), have its ID as the key and run getAllPotentialStemsForPassword for the value.

            Dictionary<string, List<int>> matrix = new Dictionary<string, List<int>>();

            // first step is to redact input param 2 (passwordIdsWithStems) to just the passwords that have  values in the sharedStems keys

            Dictionary<int, List<Dictionary<string, int>>> stemSharingPasswords = new Dictionary<int, List<Dictionary<string, int>>>();

            foreach (int k in passwordIdsWithStems.Keys)
            {
                List<Dictionary<string, int>> stems = passwordIdsWithStems[k];
                int countSharedStems = stems.Where(x => x.Keys.Where(y => sharedStems.ContainsKey(y)).Count() > 0).Count();
                if (countSharedStems > 0)
                {
                    stemSharingPasswords.Add(k, stems);
                }
            }

            Dictionary<string, List<int>> allStemsWithSharingIDs = new Dictionary<string, List<int>>();

            foreach (string stemString in sharedStems.Keys)
            {
                List<int> passwordsUsing = new List<int>();
                foreach (int passwordID in stemSharingPasswords.Keys)
                {
                    List<string> passwordStems = stemSharingPasswords[passwordID].Select(x => x.Keys.First()).ToList();
                    if (passwordStems.Contains(stemString))
                    {
                        passwordsUsing.Add(passwordID);
                    }
                }
                allStemsWithSharingIDs.Add(stemString, passwordsUsing);
            }
            if (allStemsWithSharingIDs.Any())
            {
                Dictionary<string, List<int>> cleanStemsWithSharingIDs = new Dictionary<string, List<int>>();
                List<string> sortedStems = allStemsWithSharingIDs.Keys.ToList().OrderByDescending(x => x.Length).ToList();
                int longestStemLength = sortedStems.Select(x => x.Length).Max();
                List<string> longestStems = sortedStems.Where(x => x.Length == longestStemLength).ToList();
                List<string> shorterStems = sortedStems.Where(x => x.Length != longestStemLength).ToList();

                foreach (string l in longestStems)
                {
                    cleanStemsWithSharingIDs.Add(l, allStemsWithSharingIDs[l]);
                }
                foreach (string s in shorterStems)
                {
                    List<string> enclosingLongerStems = cleanStemsWithSharingIDs.Keys.Where(x => x.Contains(s)).ToList();
                    if (enclosingLongerStems.Count == 0) // this item is not covered by a longer stem. Add it to the dictionary unmolested
                    {
                        cleanStemsWithSharingIDs.Add(s, allStemsWithSharingIDs[s]);
                    }
                    else // it's covered by at least 1 longer stem: for each longer stem its covered by, redact its ids against the that stem, then add.
                    {
                        List<int> sMembers = new List<int>(); 
                        
                        foreach (int member in allStemsWithSharingIDs[s])
                        {
                            sMembers.Add(member);
                        }

                        foreach (string elsx in enclosingLongerStems)
                        {
                            List<int> elsxMembers = cleanStemsWithSharingIDs[elsx];
                            sMembers = sMembers.Except(elsxMembers).ToList();
                        }
                        cleanStemsWithSharingIDs.Add(s, sMembers);
                    }
                }
                matrix = cleanStemsWithSharingIDs.Where(pair => pair.Value.Count > 0).ToDictionary(pair => pair.Key, pair => pair.Value);
            }
            return matrix;
        }
        public static Dictionary<string, Dictionary<string, int>> GetPasswordStems (List<LoginEntry>  pBank)
        {
            //pBank param is the WHOLE PASSWORD BANK. It needs to be id-hydrated.

            LoginEntry f = pBank.FirstOrDefault();
            if (f != null && f.id == -99) // -99 is the default id for unhyrdated login entries
            {
                throw new Exception("You are running GetPasswordStems on a password bank that is not hydrated with IDs");
            }

            Dictionary<string, Dictionary<string, int>> passwordStems = new Dictionary<string, Dictionary<string, int>>();

            List<LoginEntry> pBankPasswordsOnly = pBank.Where(x => x.LoginType == LoginTypes.Password).ToList();
            List<LoginEntry> pBankPinSixOnly = pBank.Where(x => x.LoginType == LoginTypes.PinSix).ToList();
            List<LoginEntry> pBankPinFourOnly = pBank.Where(x => x.LoginType == LoginTypes.PinFour).ToList();

            List<List<Dictionary<string, int>>> allStemLists = new List<List<Dictionary<string, int>>>(); //holds all passwords with all pt stems stems and their positional indexes in the passwrods.
            Dictionary<int, List<Dictionary<string, int>>> passwordIdsWithStems = new Dictionary<int, List<Dictionary<string, int>>>(); // holds passwordIds with their potential stems.
            foreach (LoginEntry p in pBankPasswordsOnly)
            {
                int pid = p.id;
                List<Dictionary<string, int>> stems = getAllPotentialStemsForPassword(p.LoginPass);
                allStemLists.Add(stems);
                passwordIdsWithStems.Add(pid, stems);
            }

            Dictionary<string, int> pBankShares = getAllSharedPasswordStemsInBank(allStemLists); // all shared stems with their counts.
            Dictionary<string, List<int>> matrix = getPasswordShareMatrix(pBankShares, passwordIdsWithStems); // stem, list ID for all shared stems, cross-deduplciated prioritising msot complex.


            foreach (string stemKey in matrix.Keys)
            {
                List<LoginEntry> affectedPasswords = pBankPasswordsOnly.Where(x => matrix[stemKey].Contains(x.id)).ToList();
                List<LoginEntry> affectedPinSix = pBankPinSixOnly.Where(x => stemKey.Contains(x.LoginPass)).ToList();
                List<LoginEntry> affectedPinFour = pBankPinFourOnly.Where(x => stemKey.Contains(x.LoginPass)).ToList();

                Dictionary<string, int> stemMetrics = new Dictionary<string, int>();


                //1. Do the scoring for each password allocated the stem.
                int passwordCount = affectedPasswords.Count;
                int passwordsDVS = 0;
                //1a. Find weakest password and add 100 minus its strength score
                LoginEntry weakestPassword = affectedPasswords.OrderBy(x => x.LoginStrength.Score).First();
                passwordsDVS += 100 - weakestPassword.LoginStrength.Score;

                //1b. For the rest of the passwords...
                List<LoginEntry> restOfPasswords = affectedPasswords.Where(x => x.id != weakestPassword.id).ToList();
                foreach (LoginEntry pw in restOfPasswords)
                {
                    
                    Dictionary<string, int> stemPositionDict = passwordIdsWithStems[pw.id].Where(x => x.Keys.First() == stemKey).First();

                    // Get the original password minus the stem, calculate the score for that, and add 100- new score to the dvs.

                    int startIndexStem = stemPositionDict[stemKey];
                    int lengthStem = stemKey.Length;
                    string partBeforeStem = pw.LoginPass.Substring(0, startIndexStem);
                    string partAfterStem = pw.LoginPass.Substring(startIndexStem + lengthStem);
                    string redactedPassword = partBeforeStem + partAfterStem;

                    if (redactedPassword.Length == 0)
                    {
                        passwordsDVS += 100;
                    }
                    else
                    {
                        LoginStrength ls = new LoginStrength(LoginTypes.Password, redactedPassword, pw.LoginUsername, pw.HasTwoFa);
                        passwordsDVS += 100 - ls.Score; 
                    }
                }
                stemMetrics.Add("count_passwords", passwordCount);
                stemMetrics.Add("total_dvs_passwords", passwordsDVS);

                //2 Do the scoring for each Pin 6 found in the stem
                int pinSixCount = 0;
                int pinSixDVS = 0;

                foreach (LoginEntry psix in affectedPinSix)
                {
                    if (stemKey.Contains(psix.LoginPass))
                    {
                        pinSixCount++;
                        pinSixDVS += 100;
                    }
                }

                stemMetrics.Add("count_pin_six", pinSixCount);
                stemMetrics.Add("total_dvs_pin_six", pinSixDVS);

                //3 Do the scoring for each Pin 4 found in the stem
                int pinFourCount = 0;
                int pinFourDVS = 0;

                foreach (LoginEntry pfour in affectedPinFour)
                {
                    if (stemKey.Contains(pfour.LoginPass))
                    {
                        string pfourRegex = "\\D" + pfour.LoginPass + "\\D|^" + pfour.LoginPass + "\\D|" + pfour.LoginPass + "$";
                        Regex rx = new Regex(pfourRegex);
                        if (rx.IsMatch(stemKey)) // can't be surrounded by other numbers.
                        {
                            pinFourCount++;
                            pinFourDVS += 100;
                        }
                    }
                }

                stemMetrics.Add("count_pin_four", pinFourCount);
                stemMetrics.Add("total_dvs_pin_four", pinFourDVS);

                passwordStems.Add(stemKey, stemMetrics);
            }

            return passwordStems;
        }
    }
}
