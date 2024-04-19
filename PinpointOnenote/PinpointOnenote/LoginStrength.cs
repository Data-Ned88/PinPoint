using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace PinpointOnenote
{
    public class LoginStrength
    {
        public int Score { get; set; } = -99;
        public string cellColour { get; set; } = "#FFFFFF";
        public string ScoreText { get; set; } = "";

        public LoginStrength(LoginTypes lType , string passwordValue, string usernameValue, bool hasTwoFA)
        {
            XElement AlgorithmsXml = XElement.Parse(Properties.Resources.StaticAndTestData).Element("Algorithms");
            XElement AlgorithmFormatsXml = XElement.Parse(Properties.Resources.OneNotePageAndElementStyles).Element("StrengthScoreFormats");

            // The above 2 do for all types of login algorithm
            //TODO have these as input parameters to this contructor method, so taht the parsing only need happen on application startup.
            // For this, we will need to have the LoginType (which uses this) having them both as properties.
            // Any method that contructs a LoginEntry or list of loginEntry will have to take them both in as well.


            string notSetCellColour = AlgorithmFormatsXml.Elements("StrengthScoreType")
                    .Where(x => x.Attribute("typeName").Value == "NotSet").First().Elements("ScoreFormat").First().Attribute("cellShade").Value;

            if (lType == LoginTypes.NotSet)
            {
                Score = -99;
                cellColour = notSetCellColour;
                ScoreText = "Login type not defined.";
            }
            else if (passwordValue == null)
            {
                Score = -99;
                cellColour = notSetCellColour;
                ScoreText = "Password or PIN not set to anything.";
            }
            else if (lType == LoginTypes.Password && usernameValue == null)
            {
                Score = -99;
                cellColour = notSetCellColour;
                ScoreText = "Password with no username value.";
            }
            else if (lType == LoginTypes.PinFour)
            {
                IEnumerable<XElement> scoreFormats = AlgorithmFormatsXml.Elements("StrengthScoreType")
                                .Where(x => x.Attribute("typeName").Value == "PinFour").First().Elements("ScoreFormat");

                Dictionary<string, string> pinFourStrength = LoginFunctionality.PinFourScoreValues(passwordValue);
                Score = int.Parse(pinFourStrength["score"]);
                if (pinFourStrength["score"] == "-99")
                {
                    cellColour = notSetCellColour;
                }
                else
                {
                    cellColour = scoreFormats.Where(x => x.Attribute("score").Value == pinFourStrength["score"])
                                            .First().Attribute("cellShade").Value;
                }
                
                ScoreText = pinFourStrength["scoreText"];
            }
            else if (lType == LoginTypes.PinSix)
            {
                IEnumerable<XElement> scoreFormats = AlgorithmFormatsXml.Elements("StrengthScoreType")
                                .Where(x => x.Attribute("typeName").Value == "PinSix").First().Elements("ScoreFormat");

                Dictionary<string, string> pinSixStrength = LoginFunctionality.PinSixScoreValues(passwordValue);
                Score = int.Parse(pinSixStrength["score"]);
                if (pinSixStrength["score"] == "-99")
                {
                    cellColour = notSetCellColour;
                }
                else
                {
                    cellColour = scoreFormats.Where(x => x.Attribute("score").Value == pinSixStrength["score"])
                                            .First().Attribute("cellShade").Value;
                }
                ScoreText = pinSixStrength["scoreText"];
            }
            else // password with all the correct info
            {
                Score = -99;
                cellColour = AlgorithmFormatsXml.Elements("StrengthScoreType")
                    .Where(x => x.Attribute("typeName").Value == "NotSet").First().Elements("ScoreFormat").First().Attribute("cellShade").Value;
                ScoreText = "Password or PIN not set to anything.";

                int passwordLength = passwordValue.Length;
                int passwordComplexity = LoginFunctionality.PasswordComplexityLevel(passwordValue);
                if (passwordLength < 1 || passwordComplexity < 0)
                {
                    // not set
                    Score = -99;
                    cellColour = notSetCellColour;
                    ScoreText = "Password not set to anything.";
                }
                else
                {
                    int passwordStrengthScore = 0;
                    string passwordLengthLook = passwordLength.ToString();
                    string passwordComplexityLook = "level_" + passwordComplexity.ToString();
                    if (passwordLength > 18)
                    {
                        passwordLengthLook = "GT18";
                    }
                    XElement scoresForLength = AlgorithmsXml.Elements("AlgorithmLookup")
                        .Where(x => x.Attribute("name").Value == "Password").First()
                            .Elements("LengthScore").Where(y => y.Attribute("lengthValue").Value == passwordLengthLook).First();

                    string passwordStrengthScoreLookup = scoresForLength.Attribute(passwordComplexityLook).Value;
                    passwordStrengthScore = int.Parse(passwordStrengthScoreLookup);
                    IEnumerable<XElement> scoreFormats = AlgorithmFormatsXml.Elements("StrengthScoreType")
                                .Where(x => x.Attribute("typeName").Value == "Password").First().Elements("ScoreFormat");

                    StringBuilder scoreTextBuilder = new StringBuilder();
                    scoreTextBuilder.AppendLine(scoreFormats.Where(x => x.Attribute("score").Value == passwordStrengthScoreLookup)
                                            .First().Attribute("scoreText").Value);




                    // Add 40 to the password strength score if 2FA
                    if (hasTwoFA)
                    {
                        passwordStrengthScore += 40;
                        scoreTextBuilder.AppendLine("Security score for this login has been increased by 2-factor authentication.");
                    }
                    // Subtract 40 from the password strength score if it stemps from the user name
                    if (LoginFunctionality.PasswordStemsFromUserName(passwordValue, usernameValue))
                    {
                        passwordStrengthScore -= 40;
                        scoreTextBuilder.AppendLine("Security score for this login is weaker because the password contains the username as a stem.");
                    }
                    // min 0, max 100
                    if (passwordStrengthScore < 0)
                    {
                        passwordStrengthScore = 0;
                    }

                    if (passwordStrengthScore > 96)
                    {
                        passwordStrengthScore = 96;
                    }
                    passwordStrengthScoreLookup = passwordStrengthScoreLookup.ToString();
                    ScoreText = scoreTextBuilder.ToString();
                    cellColour = scoreFormats.Where(x => x.Attribute("score").Value == passwordStrengthScoreLookup)
                                            .First().Attribute("cellShade").Value;

                    if (passwordStrengthScore == 96)
                    {
                        Score = 100;
                    }
                    else
                    {
                        Score = passwordStrengthScore;
                    }

                }
                





            }



        }
        public LoginStrength() { }
    }
}
