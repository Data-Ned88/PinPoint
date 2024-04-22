using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PinpointOnenote
{
    public class LoginBankStrength
    {
        // THis is the class that expresses the algorithm for strength across a Password bank (highest = worse), in DVS points.
        // It would be instantiated after hydration of a Password bank using the constructor method, and would be the data context for a one note page or interface.
        public int singleLoginPoints { get; set; } = 0; // 100 - Strength score for each item in bank

        //BELOW Dictionary with a string key for pin6, and as a value,
        //a dictionary of string/int where string keys are for count_shared, and DVS_points, which are how many times the Pin6 has been reused, and total DVS points caused by this respectively.
        public Dictionary<string, Dictionary<string, int>> exactSharesPinSix { get; set; }
        public Dictionary<string, Dictionary<string, int>> exactSharesPinFour { get; set; } //Same as above for Pin4
        public Dictionary<string, Dictionary<string, int>> exactSharesPassword { get; set; } // Same as above for Password


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

        public LoginBankStrength(List<LoginEntry> passwordBank)
        {
            //TODO - this is the constructor method based on a hydrated password bank. Finsih it.
            exactSharesPinSix = new Dictionary<string, Dictionary<string, int>>();
            exactSharesPinFour = new Dictionary<string, Dictionary<string, int>>();
            exactSharesPassword = new Dictionary<string, Dictionary<string, int>>();
            passwordStems = new Dictionary<string, Dictionary<string, int>>();
            singleLoginPoints = 0;
            totalScoreSharesPinSix = 0;
            totalScoreSharesPinFour = 0;
            totalScoreSharesPassword = 0;
            totalScoreStems = 0;

            totalScoreAll = totalScoreSharesPinSix + totalScoreSharesPinFour + totalScoreSharesPassword + totalScoreStems + singleLoginPoints;
        }

    }
}
