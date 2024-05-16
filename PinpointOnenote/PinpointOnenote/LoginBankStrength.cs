using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace PinpointOnenote
{
    public class LoginBankStrength
    {
        // THis is the class that expresses the algorithm for strength across a Password bank (highest = worse), in DVS points.
        // It would be instantiated after hydration of a Password bank using the constructor method, and would be the data context for a one note page or interface.
        public int singleLoginPoints { get; set; } = 0; // 100 - Strength score for each item in bank

        public Dictionary<string, Dictionary<string, int>> singleLoginPointsBreakdown { get; set; }


        //BELOW Dictionary with a string key for pin6, and as a value,
        //a dictionary of string/int where string keys are for count_shared, and DVS_points, which are how many times the Pin6 has been reused, and total DVS points caused by this respectively.
        public Dictionary<string, Dictionary<string, int>> exactSharesPinSix { get; set; }
        public Dictionary<string, Dictionary<string, int>> exactSharesPinFour { get; set; } //Same as above for Pin4
        public Dictionary<string, Dictionary<string, int>> exactSharesPassword { get; set; } // Same as above for Password (with addtl count and dvs for pinSix and Pin Four)


        // BELOW: Dictionary with string key for each password stem (based on algorith rules), and string/int values for...
        // 1. count passwords used
        // 2. count passwords used cumulative DVS score
        // 3. count found in Pin 6
        // 4. count found in Pin 6 cumulative DVS score
        // 5. count found in Pin 4
        // 6. count found in Pin 4 cumulative DVS score
        // NB, it has to be a stem pre-used in passwords for it to qualify for the secondary analysis that may/may not produce K/V for points 3-6. ...
        // ...Logic for this being that the hacker would crack the password stem first (based on fishin or scoial engineering), then guess PINs with it if it contained numbers.
        public Dictionary<string, Dictionary<string, int>> passwordStems { get; set; }

        public int totalScoreSharesPinSix { get; set; }
        public int totalScoreSharesPinFour { get; set; }
        public int totalScoreSharesPassword { get; set; }
        public int totalScoreStems { get; set; }
        public int totalScoreAll { get; set; }
        public string scoreRange { get; set; } //= "Good", "Bad" or "Average" - This would retrieve a hex colour from an Xml lookup.
        public string scoreRangeColor { get; set; } //= "Good", "Bad" or "Average" - This would retrieve a hex colour from an Xml lookup.

        public LoginBankStrength(List<LoginEntry> passwordBank)
        {
            // For the total vulnerability score banding and presentation. May choose to have this assigned in the main app and passed to thsi constructure as an input param.
            XElement AlgorithmFormatsXml = XElement.Parse(Properties.Resources.OneNotePageAndElementStyles).Element("StrengthScoreFormats");


            // Hydrate it if not already with ID and LMS
            LoginEntry f = passwordBank.FirstOrDefault();
            if (f != null && f.id == -99) // -99 is the default id for unhydrated login entries.
            {
                passwordBank = LoginFunctionality.HydrateIdAndModifiedSort(passwordBank);
            }
            //TODO - this is the constructor method based on a hydrated password bank. Finsih it.
            exactSharesPinSix = LoginFunctionality.GetExactShares(passwordBank, LoginTypes.PinSix);
            exactSharesPinFour = LoginFunctionality.GetExactShares(passwordBank, LoginTypes.PinFour);
            exactSharesPassword = LoginFunctionality.GetExactShares(passwordBank, LoginTypes.Password);
            passwordStems = LoginFunctionality.GetPasswordStems(passwordBank);
            singleLoginPoints = passwordBank.Where(x=> x.LoginType != LoginTypes.NotSet).Select(x => 100 - x.LoginStrength.Score).Sum();


            singleLoginPointsBreakdown = new Dictionary<string, Dictionary<string, int>>();

            singleLoginPointsBreakdown.Add("Passwords",
                new Dictionary<string, int> { 
                    {"count",passwordBank.Where(x => x.LoginType == LoginTypes.Password).Count() }, 
                    { "dvs_total", passwordBank.Where(x => x.LoginType == LoginTypes.Password).Select(x => 100 - x.LoginStrength.Score).Sum() }
                                            } 
                );
            singleLoginPointsBreakdown.Add("PinFours",
                new Dictionary<string, int> {
                    {"count",passwordBank.Where(x => x.LoginType == LoginTypes.PinFour).Count() },
                    { "dvs_total", passwordBank.Where(x => x.LoginType == LoginTypes.PinFour).Select(x => 100 - x.LoginStrength.Score).Sum() }
                                            }
                );
            singleLoginPointsBreakdown.Add("PinSixes",
                new Dictionary<string, int> {
                    {"count",passwordBank.Where(x => x.LoginType == LoginTypes.PinSix).Count() },
                    { "dvs_total", passwordBank.Where(x => x.LoginType == LoginTypes.PinSix).Select(x => 100 - x.LoginStrength.Score).Sum() }
                                            }
                );

            totalScoreSharesPinSix = exactSharesPinSix.Keys.ToList().ConvertAll(x => exactSharesPinSix[x]["dvs_total"]).Sum();
            totalScoreSharesPinFour = exactSharesPinFour.Keys.ToList().ConvertAll(x => exactSharesPinFour[x]["dvs_total"]).Sum();
            totalScoreSharesPassword = exactSharesPassword.Keys.ToList()
                .ConvertAll(x => exactSharesPassword[x]["dvs_total"] + exactSharesPassword[x]["dvs_total_pin_four"] + exactSharesPassword[x]["dvs_total_pin_six"]).Sum();


            totalScoreStems = passwordStems.Keys.ToList()
                .ConvertAll(x => passwordStems[x]["total_dvs_passwords"] + passwordStems[x]["total_dvs_pin_four"] + passwordStems[x]["total_dvs_pin_six"]).Sum();



            scoreRange = "notassigned"; // Need a function to set the scoreRange. This requires testing for possible ranges.
            scoreRangeColor = "#000000"; // Need a function to set the scoreRange. This requires testing for possible ranges.


            totalScoreAll = totalScoreSharesPinSix + totalScoreSharesPinFour + totalScoreSharesPassword + totalScoreStems + singleLoginPoints;

            IEnumerable<XElement> scoreFormats = AlgorithmFormatsXml.Elements("StrengthScoreType")
                                .Where(x => x.Attribute("typeName").Value == "PasswordBankAggregate").First().Elements("ScoreFormat");


            XElement unlimited = scoreFormats.Where(x => x.Attribute("maxScore").Value == "unlimited").First();
            List<XElement> capped = scoreFormats.Where(x => x.Attribute("maxScore").Value != "unlimited").ToList();
            capped = capped.OrderBy(x => int.Parse(x.Attribute("maxScore").Value)).ToList();
            foreach (XElement band in capped)
            {
                int bandTopScore = int.Parse(band.Attribute("maxScore").Value);
                if (totalScoreAll <= bandTopScore)
                {
                    scoreRange = band.Attribute("scoreText").Value;
                    scoreRangeColor = band.Attribute("cellShade").Value;
                    break;
                }
            }
            if (scoreRange == "notassigned") // not picked up as under a cap in the for loop - therfore its higher than the highest cap, so give it red.
            {
                scoreRange = unlimited.Attribute("scoreText").Value;
                scoreRangeColor = unlimited.Attribute("cellShade").Value;
            }
        }
    }
}
