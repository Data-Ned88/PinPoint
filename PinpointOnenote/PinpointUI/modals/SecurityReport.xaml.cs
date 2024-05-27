using PinpointOnenote;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace PinpointUI.modals
{
    /// <summary>
    /// Interaction logic for SecurityReport.xaml
    /// </summary>
    public partial class SecurityReport : Window
    {
        private List<LoginEntry> pBank;
        private LoginBankStrength pBankLBS;
        // aggregations from input class object
        private int sharesAllCount;
        private int sharesAllDVS;

        private List<string> typeLoopControl = new List<string> { "Passwords", "PinFours", "PinSixes" };
        private Dictionary<string, string> typeToSharePropName = new Dictionary<string, string> 
            { 
            {"Passwords","exactSharesPassword"},
            {"PinFours","exactSharesPinFour"},
            { "PinSixes","exactSharesPinSix"} 
            };
        private Dictionary<string, string> typeToHeader = new Dictionary<string, string>
            {
            {"Passwords","Passwords"},
            {"PinFours","PIN (4-digit)"},
            { "PinSixes","PIN (6-digit)"}
            };
        private Dictionary<string, string> typeToGranularShareSearch = new Dictionary<string, string>
            {
            {"Passwords",""},
            {"PinFours","_pin_four"},
            { "PinSixes","_pin_six"}
            };
        private Dictionary<string, string> typeToGranularStemSearch = new Dictionary<string, string>
            {
            {"Passwords","_passwords"},
            {"PinFours","_pin_four"},
            { "PinSixes","_pin_six"}
            };

        public SecurityReport(List<LoginEntry> pBank,string passwordBankName) //Instantiation
        {
            pBank = LoginFunctionality.HydrateIdAndModifiedSort(pBank);
            pBankLBS = new LoginBankStrength(pBank);


            // treeViewExactShares.Header value
            sharesAllCount = 0;

            
            foreach (string x in pBankLBS.exactSharesPassword.Keys)
            {
                sharesAllCount += pBankLBS.exactSharesPassword[x]["count_shared"];
                if (pBankLBS.exactSharesPassword[x].ContainsKey("count_shared_pin_four"))
                {
                    sharesAllCount += pBankLBS.exactSharesPassword[x]["count_shared_pin_four"];
                }
                if (pBankLBS.exactSharesPassword[x].ContainsKey("count_shared_pin_six"))
                {
                    sharesAllCount += pBankLBS.exactSharesPassword[x]["count_shared_pin_six"];
                }
            }
            foreach (string x in pBankLBS.exactSharesPinSix.Keys)
            {
                sharesAllCount += pBankLBS.exactSharesPinSix[x]["count_shared"];
            }
            foreach (string x in pBankLBS.exactSharesPinFour.Keys)
            {
                sharesAllCount += pBankLBS.exactSharesPinFour[x]["count_shared"];
            }


            sharesAllDVS = pBankLBS.totalScoreSharesPassword + pBankLBS.totalScoreSharesPinFour + pBankLBS.totalScoreSharesPinSix;
            // END treeViewExactShares.Header value

            InitializeComponent();


            //textBlockPassBankName.Text = string.Format("{0} ({1} valid PINS/Passwords)", passwordBankName, pBank.Where(x=> x.LoginType != LoginTypes.NotSet).Count().ToString("N0"));
            textBlockPassBankName.Inlines.Add(GetBoldRun(passwordBankName));
            textBlockPassBankName.Inlines.Add(GetRun(string.Format(" ({0} valid items)", pBank.Where(x => x.LoginType != LoginTypes.NotSet).Count().ToString("N0"))));
            textBlockPassBankDVS.Text = string.Format("{0} ({1})", pBankLBS.totalScoreAll.ToString("N0"),pBankLBS.scoreRange);
            textBlockPassBankDVS.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(pBankLBS.scoreRangeColor));

            // Tree view headers

            SetTreeViewItemHeader(treeViewSingleLogins,
                GetUnderlineRun("Logins as Single Items:"),
                GetRun(string.Format(" {0} DVS Points ({1} total items)",
                    pBankLBS.singleLoginPoints.ToString("N0"), pBank.Where(x => x.LoginType != LoginTypes.NotSet).Count().ToString("N0")))
                );

            //treeViewExactShares.Header = string.Format("Passwords/PINs shared by multiple logins: {0} DVS Points ({1} items)",
            //    sharesAllDVS.ToString("N0"), sharesAllCount.ToString("N0"));
            SetTreeViewItemHeader(treeViewExactShares,
                GetUnderlineRun("Passwords / PINs shared by multiple logins:"),
                GetRun(string.Format(" {0} DVS Points ({1} items)",
                sharesAllDVS.ToString("N0"), sharesAllCount.ToString("N0"))
                    )
                );
            //treeViewStems.Header = string.Format("Password stems shared by multiple logins: {0} DVS Points ({1} stems found)",
            //    pBankLBS.totalScoreStems.ToString("N0"), pBankLBS.passwordStems.Keys.Count.ToString("N0"));
            SetTreeViewItemHeader(treeViewStems,
                   GetUnderlineRun("Password stems shared by multiple logins:"),
                   GetRun(string.Format(" {0} DVS Points ({1} stems found)",
                pBankLBS.totalScoreStems.ToString("N0"), pBankLBS.passwordStems.Keys.Count.ToString("N0"))
                       )
                   );
            //Tree view: treeViewSingleLogins 3 x children with count and score

            foreach (string _type in typeLoopControl)
            {
                AddTreeViewItemIfNotZero(treeViewSingleLogins, pBankLBS, "singleLoginPointsBreakdown", _type, "count", typeToHeader[_type], "dvs_total", "count");
            }

            //Tree view: Shares

            foreach (string _type in typeLoopControl)
            {
                Console.WriteLine(_type);
                Dictionary<string, Dictionary<string, int>> sharePropVal = GetPropertyValue(pBankLBS, typeToSharePropName[_type]);
                List<Run> shareTypeSubhead = GetSharesTypeHeader(sharePropVal, typeToHeader[_type]);
                if (shareTypeSubhead[0].Text != "")
                {
                    TreeViewItem shareTypeTV = new TreeViewItem(); //GetTreeViewItem(shareTypeSubhead);
                    SetTreeViewItemHeader(shareTypeTV, shareTypeSubhead);

                    treeViewExactShares.Items.Add(shareTypeTV);
                    foreach (string k in sharePropVal.Keys)
                    {
                        Console.WriteLine("----" + k);
                        Dictionary<string, int> propertiesForShareItem = sharePropVal[k];
                        
                        if (_type == "Passwords") //additional level of looping (shares vs PINS 4/6)
                        {

                            List<Run> shareItemHead = GetSharesItemHeader(k, propertiesForShareItem, typeToHeader[_type]);
                            if (shareItemHead[0].Text != "")
                            {
                                TreeViewItem shareItemTV = new TreeViewItem(); // GetTreeViewItem(shareItemHead);
                                SetTreeViewItemHeader(shareItemTV, shareItemHead);
                                shareTypeTV.Items.Add(shareItemTV);
                                
                                foreach (string _mutual_type in typeLoopControl)
                                {
                                    List<Run> granularHead = GetGranularShareHeaderPasswords(propertiesForShareItem, typeToHeader[_mutual_type], typeToGranularShareSearch[_mutual_type]);
                                    if (granularHead[0].Text != "")
                                    {
                                        TreeViewItem shareGranularItemTV = new TreeViewItem(); //  GetTreeViewItem(granularHead);
                                        SetTreeViewItemHeader(shareGranularItemTV, granularHead);
                                        shareItemTV.Items.Add(shareGranularItemTV);
                                    }
                                }                                
                            }

                        }
                        else
                        {
                            List<Run> shareItemHead = GetSharesItemHeader(k, propertiesForShareItem, typeToHeader[_type]);
                            if (shareItemHead[0].Text != "")
                            {
                                TreeViewItem shareItemTV = new TreeViewItem(); // GetTreeViewItem(shareItemHead);
                                SetTreeViewItemHeader(shareItemTV, shareItemHead);
                                shareTypeTV.Items.Add(shareItemTV);
                                
                            }
                        }
                    }
                }
            }


            //Tree view: Stems
            Dictionary<string, Dictionary<string, int>> stemPropVal = GetPropertyValue(pBankLBS, "passwordStems");
            foreach (string sk in stemPropVal.Keys)
            {
                Dictionary<string, int> propertiesForStemItem = stemPropVal[sk];
                List<Run> stemItemHead = GetSharesItemHeader(sk, propertiesForStemItem, typeToHeader["Passwords"],true);
                if (stemItemHead[0].Text != "")
                {

                    TreeViewItem stemItemTV = new TreeViewItem(); //GetTreeViewItem(stemItemHead);
                    SetTreeViewItemHeader(stemItemTV, stemItemHead);
                    treeViewStems.Items.Add(stemItemTV);

                    foreach (string _mutual_type in typeLoopControl)
                    {
                        List<Run> stemGranularHead = GetGranularShareHeaderPasswords(propertiesForStemItem, typeToHeader[_mutual_type], typeToGranularStemSearch[_mutual_type],true);
                        if (stemGranularHead[0].Text != "")
                        {
                            TreeViewItem stemGranularItemTV = new TreeViewItem(); //GetTreeViewItem(stemGranularHead);
                            SetTreeViewItemHeader(stemGranularItemTV, stemGranularHead);
                            stemItemTV.Items.Add(stemGranularItemTV);
                        }
                    }

                }
            }

        }

        private void btnExit_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void AddTreeViewItemIfNotZero(TreeViewItem tvi, LoginBankStrength inputStrength, string classPropertyName,string propertyKey, 
            string compareToZero, string headerFormat, string formatOne, string FormatTwo = null) // BIT ESOTERIC, ONLY WORKS FOR SINGLE ITEM COUNTS AND DVS
        {
            PropertyInfo pi = inputStrength.GetType().GetProperty(classPropertyName);
            Dictionary<string, Dictionary<string, int>> dataValue = (Dictionary<string, Dictionary<string, int>>)pi.GetValue(inputStrength);

            if ((int)dataValue[propertyKey][compareToZero] > 0)
            {
                TreeViewItem tv = new TreeViewItem();
                //TODO SetTreeViewItemHeader(tv,GetRun1, GetRun2) and turn the below statement off.
                SetTreeViewItemHeader(tv,
                    GetUnderlineRun(string.Format("{0}:", headerFormat)),
                    GetRun(
                        string.Format(
                            " {0} DVS Points ({1} items)", 
                            pBankLBS.singleLoginPointsBreakdown[propertyKey][formatOne].ToString("N0"), 
                            pBankLBS.singleLoginPointsBreakdown[propertyKey][FormatTwo].ToString("N0")
                            )
                        )
                    );

                //tv.Header = string.Format("{0}: {1} DVS Points ({2} items)", headerFormat, pBankLBS.singleLoginPointsBreakdown[propertyKey][formatOne].ToString("N0"),
                                                       // pBankLBS.singleLoginPointsBreakdown[propertyKey][FormatTwo].ToString("N0"));

                tvi.Items.Add(tv);
            }
        }

        private void TreeViewItemIfNotZero(TreeViewItem tvi, LoginBankStrength inputStrength, string classPropertyName, string propertyKey,
            string compareToZero, string headerFormat, string formatOne, string FormatTwo = null)
        {
            PropertyInfo pi = inputStrength.GetType().GetProperty(classPropertyName);
            Dictionary<string, Dictionary<string, int>> dataValue = (Dictionary<string, Dictionary<string, int>>)pi.GetValue(inputStrength);

            if ((int)dataValue[propertyKey][compareToZero] > 0)
            {
                TreeViewItem tv = new TreeViewItem();
                tv.Header = string.Format("{0}: {1} DVS Points ({2} items)", headerFormat, pBankLBS.singleLoginPointsBreakdown[propertyKey][formatOne].ToString("N0"),
                                                        pBankLBS.singleLoginPointsBreakdown[propertyKey][FormatTwo].ToString("N0"));

                tvi.Items.Add(tv);
            }
        }

        private List<Run> GetSharesTypeHeader(Dictionary<string, Dictionary<string, int>> dataValue, string headerFormat)
        {
            List<Run> runs = new List<Run>();

            int totalPoints = 0;
            int countShares = 0;
            foreach (string x in dataValue.Keys)
            {
                totalPoints += dataValue[x]["dvs_total"];
                bool isShared = dataValue[x]["count_shared"] > 0;
                if (headerFormat == "Passwords")
                {
                    totalPoints += dataValue[x]["dvs_total_pin_four"];
                    if (!isShared && dataValue[x]["count_shared_pin_four"] > 0)
                    {
                        isShared = true;
                    }

                    totalPoints += dataValue[x]["dvs_total_pin_six"];
                    if (!isShared && dataValue[x]["count_shared_pin_six"] > 0)
                    {
                        isShared = true;
                    }
                }
                if (isShared)
                {
                    countShares++;
                }
            }


            if (countShares > 0)
            {
                //TODO Return this as a List<Run> where each run is a segment formatted how we like.
                runs.Add(GetUnderlineRun(string.Format("{0}:", headerFormat)));
                runs.Add(GetRun(string.Format(" {0} DVS Points ({1} items)", totalPoints.ToString("N0"), countShares.ToString("N0"))));
            }
            else
            {
                //TODO Return List<GetRun("")>
                runs.Add(GetRun(""));
            }
            return runs;
        }

        private List<Run> GetSharesItemHeader (string itemHeaderVal, Dictionary<string, int> dataValue, string headerFormat, bool forStems = false)
        {
            List<Run> runs = new List<Run>();
            int totalPoints = 0;
            int countShares = 0;

            string dvs_total = "dvs_total";
            string count_shared = "count_shared";
            string dvs_total_pin_four = "dvs_total_pin_four";
            string count_shared_pin_four = "count_shared_pin_four";
            string dvs_total_pin_six = "dvs_total_pin_six";
            string count_shared_pin_six = "count_shared_pin_six";
            string formatstring = ": {0} DVS Points (shared {1} times)";

            if (forStems)
            {
                dvs_total = "total_dvs_passwords";
                count_shared = "count_passwords";

                count_shared_pin_six = "count_pin_six";
                dvs_total_pin_six = "total_dvs_pin_six";
                
                dvs_total_pin_four = "total_dvs_pin_four";
                count_shared_pin_four = "count_pin_four";

                formatstring = ": {0} DVS Points (used in {1} items)";
            }



            totalPoints += dataValue[dvs_total];
            countShares += dataValue[count_shared];
            if (headerFormat == "Passwords")
            {
                totalPoints += dataValue[dvs_total_pin_four];
                countShares += dataValue[count_shared_pin_four];

                totalPoints += dataValue[dvs_total_pin_six];
                countShares += dataValue[count_shared_pin_six];

            }
            if (countShares <= 0)
            {
                //return "";
                runs.Add(GetRun(""));
            }
            else
            {
                runs.Add(GetBoldRun(itemHeaderVal));
                runs.Add(GetRun(string.Format(formatstring, totalPoints.ToString("N0"), countShares.ToString("N0"))));
                //TODO Return this as a List<Run> where each run is a segment formatted how we like.
                //return string.Format(formatstring, itemHeaderVal, totalPoints.ToString("N0"), countShares.ToString("N0"));
            }
            return runs;
        }
        private List<Run> GetGranularShareHeaderPasswords(Dictionary<string, int> dataValue, string headerFormat,string searchable, bool forStems = false)
        {
            List<Run> runs = new List<Run>();
            //Underline start, standard end
            string searchableCount = "count_shared" + searchable;
            string searchableDVS = "dvs_total" + searchable;
            string formatstring = "Shared with other {0}: {1} DVS Points ({2} times)";
            string runOneFmt = "Shared with other {0}:";
            string runTwoFmt = " {0} DVS Points ({1} times)";
            if (forStems)
            {
                searchableCount = "count" + searchable;
                searchableDVS = "total_dvs" + searchable;
                formatstring = "{0}: {1} DVS Points (used in {2} items)";
                runOneFmt = "{0}:";
            }
            int totalPoints = dataValue[searchableDVS];
            int countShares = dataValue[searchableCount];

            if (countShares <= 0)
            {
                runs.Add(GetRun(""));

            }
            else
            {
                //TODO Return this as a List<Run> where each run is a segment formatted how we like.
                runs.Add(GetUnderlineRun(string.Format(runOneFmt, headerFormat)));
                runs.Add(GetRun(string.Format(runTwoFmt, totalPoints.ToString("N0"), countShares.ToString("N0"))));
                //return string.Format(formatstring, headerFormat, totalPoints.ToString("N0"), countShares.ToString("N0"));
            }
            return runs;
        }




        private TreeViewItem GetTreeViewItem(string header)
        {
            //TODO - once we've converted the functiosn that produce the inputs into returners of List<Run> we convert the input type to List<Run>
            TreeViewItem tv = new TreeViewItem();
            //TODO SetTreeViewItemHeader(tv,header AS LIST<Run>) and turn the below statement off.
            tv.Header = header;
            return tv;
        }

        private Dictionary<string, Dictionary<string, int>> GetPropertyValue (LoginBankStrength inputStrength, string classPropertyName)
        {
            PropertyInfo pi = inputStrength.GetType().GetProperty(classPropertyName);
            Dictionary<string, Dictionary<string, int>> dataValue = (Dictionary<string, Dictionary<string, int>>)pi.GetValue(inputStrength);
            return dataValue;
        }

        private static Run GetBoldRun(string inputText)
        {
            Run boldRun = new Run(inputText);
            boldRun.FontWeight = FontWeights.Bold;

            return boldRun;
        }
        private static Run GetUnderlineRun(string inputText)
        {
            Run underlineRun = new Run(inputText);
            underlineRun.TextDecorations = TextDecorations.Underline;

            return underlineRun;
        }
        private static Run GetRun(string inputText)
        {
            Run normalRun = new Run(inputText);
            return normalRun;
        }
        private void SetTreeViewItemHeader(TreeViewItem tv, List<Run> runs)
        {
            //overload for SetTreeViewItemHeader which takes the runs as a list.
            if (runs.Count == 0)
            {
                SetTreeViewItemHeader(tv,GetRun(""));
            }
            else if (runs.Count == 1)
            {
                SetTreeViewItemHeader(tv, runs[0]);
            }
            else
            {
                SetTreeViewItemHeader(tv, runs[0], runs[1]);
            }
        }
        private void SetTreeViewItemHeader(TreeViewItem tv, Run runOne, Run runTwo = null)
        {
            // Create a TextBlock
            TextBlock headerTextBlock = new TextBlock();

            headerTextBlock.Inlines.Add(runOne);
            if (runTwo != null)
            {
                headerTextBlock.Inlines.Add(runTwo);
            }

            // Assign the TextBlock to the Header property of the TreeViewItem
            tv.Header = headerTextBlock;
        }

    }
}
