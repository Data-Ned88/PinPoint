using PinpointOnenote;
using System;
using System.Collections.Generic;
using System.Linq;
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
using System.Xml.Linq;

namespace PinpointUI.modals
{
    /// <summary>
    /// Interaction logic for ConfirmPublish.xaml
    /// </summary>
    public partial class ConfirmPublish : Window
    {
        public List<string> fontListItems { get; set; } = new List<string> { "Arial", "Calibri", "Times New Roman" };
        public List<string> fontSizeListItems { get; set; } = new List<string> { "9 pt", "10 pt", "11 pt", "12 pt", "14 pt"};
        public List<SelectableColourTheme> coloursAvailable { get; set; } = new List<SelectableColourTheme>();

        private Dictionary<string, string> fontSizeLookup = new Dictionary<string, string> { { "9 pt", "Small" },
            { "10 pt", "Small_Regular" },{ "11 pt", "Regular" },{ "12 pt","Large_Regular" },{ "14 pt","Large" } };

        private Dictionary<string, AllowableFonts> fontLookup = new Dictionary<string, AllowableFonts>
        {
            {"Arial", AllowableFonts.Arial},{"Calibri",AllowableFonts.Calibri},{"Times New Roman",AllowableFonts.TimesNewRoman}
        };
        public bool ExitChoice { get; set; }
        public AllowableFonts SelectedFont { get; set; }
        public string SelectedFontSize { get; set; }
        public string SelectedTheme { get; set; }

        private string fontListBoxSelected;
        private string fontSizeListBoxSelected;
        private SelectableColourTheme selectedColourTheme;
        XElement stylingresource = XElement.Parse(PinpointOnenote.Properties.Resources.OneNotePageAndElementStyles);
        private Dictionary<string, string> dataPassedIn = null;
        public ConfirmPublish(Dictionary<string, string> inputParam = null)
        {
            DataContext = this;
            if (inputParam != null)
            {
                dataPassedIn = inputParam;
            }
            coloursAvailable.Add(new SelectableColourTheme("Grey","Standard Black", "#D9D9D9", "#FFFFFF"));
            coloursAvailable.Add(new SelectableColourTheme("Blue", "Blue", "#D9E1F2", "#FFFFFF"));
            coloursAvailable.Add(new SelectableColourTheme("Green", "Green", "#E2EFDA", "#FFFFFF"));
            coloursAvailable.Add(new SelectableColourTheme("Yellow", "Yellow", "#FFE699", "#FFF2CC"));

            InitializeComponent();
        }

        private void btnConfirm_Click(object sender, RoutedEventArgs e)
        {
            ExitChoice = false;
            Close();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            ExitChoice = true;
            Close();
        }

        private void FontListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (FontListBox.IsLoaded)
            {
                fontListBoxSelected = FontListBox.SelectedItem.ToString();
                SelectedFont = fontLookup[fontListBoxSelected];
            }

        }

        private void FontSizeListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (FontSizeListBox.IsLoaded)
            {
                fontSizeListBoxSelected = FontSizeListBox.SelectedItem.ToString();
                SelectedFontSize = fontSizeLookup[fontSizeListBoxSelected];
            }

        }

        private void colourThemeGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (colourThemeGrid.IsLoaded)
            {
                selectedColourTheme = (SelectableColourTheme)colourThemeGrid.SelectedItem;
                SelectedTheme = selectedColourTheme.ConfigKey;
            }

        }

        private void colourThemeGrid_Loaded(object sender, RoutedEventArgs e)
        {
            if (dataPassedIn != null)
            {
                XElement colourItemXml = stylingresource.Descendants("ColorTheme").Where(x => x.Attribute("titleShade").Value == dataPassedIn["titleShade"]).FirstOrDefault();
                if(colourItemXml != null)
                {
                    string colourKey = colourItemXml.Attribute("name").Value;
                    colourThemeGrid.SelectedIndex = coloursAvailable.FindIndex(x => x.ConfigKey == colourKey);
                }
            }
            //XElement tableCol = stylingresource.Descendants("ColorTheme").Where(x => x.Attribute("name").Value == confirmPub.SelectedTheme).First();
            //XElement tableSize = stylingresource.Descendants("TableSizing").Where(x => x.Attribute("name").Value == confirmPub.SelectedFontSize).First();
            //XElement tabColourEl = stylingresource.Elements("BaseStyles").Where(x => x.Attribute("name").Value == "Base").First().Elements("SectionTabCol").FirstOrDefault();

        }

        private void FontSizeListBox_Loaded(object sender, RoutedEventArgs e)
        {
            if (dataPassedIn != null)
            {
                XElement sizingItemXml = stylingresource.Descendants("TableSizing").Where(x => x.Attribute("fontSizeTableHead").Value == dataPassedIn["fontSizeTableHead"]).FirstOrDefault();
                if (sizingItemXml != null)
                {
                    string textsizeKey = sizingItemXml.Attribute("fontSizeText").Value.Replace(".0","") + " pt";
                    FontSizeListBox.SelectedIndex = fontSizeListItems.IndexOf(textsizeKey);
                }
            }
        }

        private void FontListBox_Loaded(object sender, RoutedEventArgs e)
        {
            if (dataPassedIn != null)
            {

                if (fontListItems.Contains(dataPassedIn["fontFamily"]))
                {
                    FontListBox.SelectedIndex = fontListItems.IndexOf(dataPassedIn["fontFamily"]);
                }
            }
        }
    }
}
