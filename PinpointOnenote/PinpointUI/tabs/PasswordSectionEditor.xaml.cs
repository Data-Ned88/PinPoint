using PinpointOnenote;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml;
using System.Xml.Linq;

namespace PinpointUI.tabs
{
    /// <summary>
    /// Interaction logic for PasswordSectionEditor.xaml
    /// </summary>
    public partial class PasswordSectionEditor : UserControl, INotifyPropertyChanged
    {
        private bool isNew = false;
        private MainWindow mainCallingWindow;
        private string mainCallingButtonName;
        private OneNoteManagementTab callingOneNoteTab;
        private Microsoft.Office.Interop.OneNote.Application app;
        private XmlDocument hier;
        private XmlNamespaceManager nsmgr;
        private string notebookId;
        private string notebookName;
        private string sectionName;
        private OneNoteSection onSection;
        private string mainBannerPlaceholder = "OneNote Password Section: {0}";
        private string subBannerPlaceholder = "(Notebook: {0})"; // optional switch later to "(Notebook: {0} Section Group: {1})"
        private string mainBannerText;
        private string subBannerText;
        private string passwordBankPageId;
        private XDocument passwordBankPageContent;
        



        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private List<LoginEntry> passwordBank;
        public List<LoginEntry> PasswordBank
        {
            get { return passwordBank; }
            set
            {
                passwordBank = value;
                OnPropertyChanged();
            }

        }
        private List<LoginEntry> passwordBankOriginal;

        public PasswordSectionEditor(OneNoteManagementTab inpSecCaller, MainWindow inpMwCaller, 
            string inpMwCbn, Microsoft.Office.Interop.OneNote.Application inpApp,
            string inpNotebookName, XmlDocument inpHierarchy, XmlNamespaceManager inpNsmgr, string inpSectionName, OneNoteSection inpSection = null)
        {
            DataContext = this;
            if (inpMwCbn == "LandingCreate_Click")
            {
                isNew = true;
            }
            mainCallingWindow = inpMwCaller;
            callingOneNoteTab = inpSecCaller;
            app = inpApp;
            notebookName = inpNotebookName;            
            hier = inpHierarchy;
            nsmgr = inpNsmgr;
            notebookId = OnenoteMethods.GetNotebookID(OnenoteMethods.GetAvailableNotebooks(hier, nsmgr), notebookName);
            sectionName = inpSectionName;

            if (isNew)
            {
                mainBannerText = string.Format(mainBannerPlaceholder, inpSectionName);
                subBannerText = string.Format(subBannerPlaceholder, notebookName);
                passwordBank = new List<LoginEntry>();
                passwordBankOriginal = new List<LoginEntry>();

    }
            else
            {
                mainBannerText = string.Format(mainBannerPlaceholder, inpSectionName);
                if (inpSection.SectionGroup != null)
                {
                    subBannerPlaceholder = "(Notebook: {0} Section Group: {1})";
                    subBannerText = string.Format(subBannerPlaceholder, notebookName, inpSection.SectionGroup);
                }
                else
                {
                    subBannerText = string.Format(subBannerPlaceholder, notebookName);
                }
                // hydrate password Bank with the PasswordBank page from the section.
                passwordBankPageId = OneNotePageFmtMethods.GetPageIdInSection(inpSection.SectionXML, "Password Bank"); // THIS WILL HAVE BEEN TESTED AS VALID BY PREV GUI.
                passwordBankPageContent = OneNotePageFmtMethods.GetPageXmlLinq(app, passwordBankPageId);
                passwordBank = DataParsers.GetPasswordsFromValidPage(passwordBankPageContent, passwordBankPageContent.Root.Name.Namespace);
                passwordBank = LoginFunctionality.HydrateIdAndModifiedSort(passwordBank);
                passwordBankOriginal = new List<LoginEntry>();
                foreach (LoginEntry le in passwordBank)
                {
                    passwordBankOriginal.Add(
                        new LoginEntry(le)
                        );
                }
                passwordBankOriginal = LoginFunctionality.HydrateIdAndModifiedSort(passwordBankOriginal);

            }


            InitializeComponent();

            pwordTabSectionTitle.Text = mainBannerText;
            pwordTabSectionSubTitle.Text = subBannerText;


        }

        private void PwordTabBackToSections_Click(object sender, RoutedEventArgs e)
        {

        }

        private void PwordTabSave_Click(object sender, RoutedEventArgs e)
        {

        }

        private void PwordTabClear_Click(object sender, RoutedEventArgs e)
        {

        }

        private void PwordTabExit_Click(object sender, RoutedEventArgs e)
        {

        }

        private void existingPasswords_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void btnNewPassInExisting_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnImportFromFile_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnAddNew_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnUpdate_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnUndoChanges_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnDeleteSelected_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnDeleteAll_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnPassPinAuto_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
