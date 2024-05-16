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

        public SecurityReport(ObservableCollection<LoginEntry> passwordBank,string passwordBankName) //Instantiation
        {
            pBank = new List<LoginEntry>();
            foreach (LoginEntry le in passwordBank)
            {
                pBank.Add(
                    new LoginEntry(le)
                    );
            }
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


            textBlockPassBankName.Text = passwordBankName;
            textBlockPassBankDVS.Text = string.Format("{0} ({1})", pBankLBS.totalScoreAll.ToString("N0"),pBankLBS.scoreRange);
            textBlockPassBankDVS.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(pBankLBS.scoreRangeColor));

            // Tree view headers

            treeViewSingleLogins.Header = string.Format("Logins as Single Items: {0} DVS Points ({1} total items)", 
                pBankLBS.singleLoginPoints.ToString("N0"), pBank.Count.ToString("N0"));


            treeViewExactShares.Header = string.Format("Passwords/PINs shared by multiple logins: {0} DVS Points ({1} items)",
                sharesAllDVS.ToString("N0"), sharesAllCount.ToString("N0"));

            treeViewStems.Header = string.Format("Password stems shared by multiple logins: {0} DVS Points ({1} stems found)",
                pBankLBS.totalScoreStems.ToString("N0"), pBankLBS.passwordStems.Keys.Count.ToString("N0"));

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
                string shareTypeSubhead = GetSharesTypeHeader(sharePropVal, typeToHeader[_type]);
                if (shareTypeSubhead != "")
                {
                    TreeViewItem shareTypeTV = GetTreeViewItem(shareTypeSubhead);
                    treeViewExactShares.Items.Add(shareTypeTV);
                    foreach (string k in sharePropVal.Keys)
                    {
                        Console.WriteLine("----" + k);
                        Dictionary<string, int> propertiesForShareItem = sharePropVal[k];
                        
                        if (_type == "Passwords") //additional level of looping (shares vs PINS 4/6)
                        {
                            
                            string shareItemHead = GetSharesItemHeader(k, propertiesForShareItem, typeToHeader[_type]);
                            if (shareItemHead != "")
                            {
                                TreeViewItem shareItemTV = GetTreeViewItem(shareItemHead);
                                shareTypeTV.Items.Add(shareItemTV);
                                
                                foreach (string _mutual_type in typeLoopControl)
                                {
                                    string granularHead = GetGranularShareHeaderPasswords(propertiesForShareItem, typeToHeader[_mutual_type], typeToGranularShareSearch[_mutual_type]);
                                    if (granularHead != "")
                                    {
                                        TreeViewItem shareGranularItemTV = GetTreeViewItem(granularHead);
                                        shareItemTV.Items.Add(shareGranularItemTV);
                                    }
                                }                                
                            }

                        }
                        else
                        {
                            string shareItemHead = GetSharesItemHeader(k, propertiesForShareItem, typeToHeader[_type]);
                            if (shareItemHead != "")
                            {
                                TreeViewItem shareItemTV = GetTreeViewItem(shareItemHead);
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
                string stemItemHead = GetSharesItemHeader(sk, propertiesForStemItem, typeToHeader["Passwords"],true);
                if (stemItemHead != "")
                {
                    TreeViewItem stemItemTV = GetTreeViewItem(stemItemHead);
                    treeViewStems.Items.Add(stemItemTV);
                    foreach (string _mutual_type in typeLoopControl)
                    {
                        string stemGranularHead = GetGranularShareHeaderPasswords(propertiesForStemItem, typeToHeader[_mutual_type], typeToGranularStemSearch[_mutual_type],true);
                        if (stemGranularHead != "")
                        {
                            TreeViewItem stemGranularItemTV = GetTreeViewItem(stemGranularHead);
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
                tv.Header = string.Format("{0}: {1} DVS Points ({2} items)", headerFormat, pBankLBS.singleLoginPointsBreakdown[propertyKey][formatOne].ToString("N0"),
                                                        pBankLBS.singleLoginPointsBreakdown[propertyKey][FormatTwo].ToString("N0"));

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

        private string GetSharesTypeHeader(Dictionary<string, Dictionary<string, int>> dataValue, string headerFormat)
        {


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
                return string.Format("{0}: {1} DVS Points ({2} items)", headerFormat, totalPoints.ToString("N0"), countShares.ToString("N0"));
            }
            else
            {
                return "";
            }
        }

        private string GetSharesItemHeader (string itemHeaderVal, Dictionary<string, int> dataValue, string headerFormat, bool forStems = false)
        {

            int totalPoints = 0;
            int countShares = 0;

            string dvs_total = "dvs_total";
            string count_shared = "count_shared";
            string dvs_total_pin_four = "dvs_total_pin_four";
            string count_shared_pin_four = "count_shared_pin_four";
            string dvs_total_pin_six = "dvs_total_pin_six";
            string count_shared_pin_six = "count_shared_pin_six";
            string formatstring = "{0}: {1} DVS Points (shared {2} times)";
            if (forStems)
            {
                dvs_total = "total_dvs_passwords";
                count_shared = "count_passwords";

                count_shared_pin_six = "count_pin_six";
                dvs_total_pin_six = "total_dvs_pin_six";
                
                dvs_total_pin_four = "total_dvs_pin_four";
                count_shared_pin_four = "count_pin_four";

                formatstring = "{0}: {1} DVS Points (used in {2} items)";
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
                return "";
            }
            else
            {
                return string.Format(formatstring, itemHeaderVal, totalPoints.ToString("N0"), countShares.ToString("N0"));
            }
        }
        private string GetGranularShareHeaderPasswords(Dictionary<string, int> dataValue, string headerFormat,string searchable, bool forStems = false)
        {
            string searchableCount = "count_shared" + searchable;
            string searchableDVS = "dvs_total" + searchable;
            string formatstring = "Shared with other {0}: {1} DVS Points ({2} times)";
            if (forStems)
            {
                searchableCount = "count" + searchable;
                searchableDVS = "total_dvs" + searchable;
                formatstring = "{0}: {1} DVS Points (used in {2} items)";
            }
            int totalPoints = dataValue[searchableDVS];
            int countShares = dataValue[searchableCount];

            if (countShares <= 0)
            {
                return "";
            }
            else
            {
                return string.Format(formatstring, headerFormat, totalPoints.ToString("N0"), countShares.ToString("N0"));
            }
        }




        private TreeViewItem GetTreeViewItem(string header)
        {
            TreeViewItem tv = new TreeViewItem();
            tv.Header = header;
            return tv;
        }

        private Dictionary<string, Dictionary<string, int>> GetPropertyValue (LoginBankStrength inputStrength, string classPropertyName)
        {
            PropertyInfo pi = inputStrength.GetType().GetProperty(classPropertyName);
            Dictionary<string, Dictionary<string, int>> dataValue = (Dictionary<string, Dictionary<string, int>>)pi.GetValue(inputStrength);
            return dataValue;
        }


    }
}
