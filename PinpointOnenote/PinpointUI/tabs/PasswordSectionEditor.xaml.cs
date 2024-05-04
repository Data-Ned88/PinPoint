using PinpointOnenote;
using PinpointUI.modals;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
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

        private ObservableCollection<LoginEntry> passwordBank;
        public ObservableCollection<LoginEntry> PasswordBank
        {
            get { return passwordBank; }
            set
            {
                passwordBank = value;
                OnPropertyChanged();
            }

        }
        private List<LoginEntry> passwordBankOriginal;

        private LoginEntry selectedLogin;
        public LoginEntry SelectedLogin
        {
            get { return selectedLogin; }
            set
            {
                selectedLogin = value;
                OnPropertyChanged();
            }

        }

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
                passwordBank = new ObservableCollection<LoginEntry>();
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
                passwordBankOriginal = DataParsers.GetPasswordsFromValidPage(passwordBankPageContent, passwordBankPageContent.Root.Name.Namespace);
                passwordBankOriginal = LoginFunctionality.HydrateIdAndModifiedSort(passwordBankOriginal);
                passwordBank = new ObservableCollection<LoginEntry>();
                foreach (LoginEntry le in passwordBankOriginal)
                {
                    passwordBank.Add(
                        le
                        );
                }
                //passwordBankOriginal = LoginFunctionality.HydrateIdAndModifiedSort(passwordBankOriginal);

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
            //Application.Current.Shutdown();
            LandingExitConfirm exitConfirm = new LandingExitConfirm(mainCallingWindow);
            Opacity = 0.6;
            exitConfirm.ShowDialog();
            Opacity = 1;
            if (exitConfirm.ExitChoice == true)
            {
                Marshal.FinalReleaseComObject(app);
                app = null;
                GC.Collect();
                GC.WaitForPendingFinalizers();
                Application.Current.Shutdown();
            }
            
        }
        private void setVisibilitySinglePasswordEditor(string set = "new", Visibility _mode = Visibility.Hidden)
        {
            
            if (set ==  "new")
            {
                newItemDescInput.Visibility = _mode;
                newItemTypeInput.Visibility = _mode;
                newItemUrlInput.Visibility = _mode;
                newItemUsernameInput.Visibility = _mode;
                newItemPassPinInput.Visibility = _mode;
                newItemTwoFaInput.Visibility = _mode;
                newItemTwoFaMethodInput.Visibility = _mode;
                newItemStrengthInput.Visibility = _mode;
                btnAddNew.Visibility = _mode;
            }
            else
            {
                selItemDescInput.Visibility = _mode;
                selItemTypeInput.Visibility = _mode;
                selItemUrlInput.Visibility = _mode;
                selItemUsernameInput.Visibility = _mode;
                selItemPassPinInput.Visibility = _mode;
                selItemTwoFaInput.Visibility = _mode;
                selItemTwoFaMethodInput.Visibility = _mode;
                selItemStrengthInput.Visibility = _mode;
                btnUpdate.Visibility = _mode;
            }
        }
        private void existingPasswords_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (existingPasswords.SelectedItems.Count == 1)
            {
                selectedLogin = (LoginEntry)existingPasswords.SelectedItem;
                setVisibilitySinglePasswordEditor("new", Visibility.Hidden);
                setVisibilitySinglePasswordEditor("sel", Visibility.Visible);

            }
            else if (existingPasswords.SelectedItems.Count > 1)
            {
                setVisibilitySinglePasswordEditor("new", Visibility.Hidden);
                setVisibilitySinglePasswordEditor("sel", Visibility.Hidden);
            }
            else
            {
                //TODO - I don't like this. It hides everything except the labels when no passwords are selected. Hdel the labels as well.??
                setVisibilitySinglePasswordEditor("new", Visibility.Visible);
                setVisibilitySinglePasswordEditor("sel", Visibility.Hidden);
            }
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
            // THis pushes the update to the Grid, which is read only. It don't want it dynamically updating.
            BindingExpression be = selItemDescInput.GetBindingExpression(TextBox.TextProperty);
            be.UpdateSource();
            be = selItemTypeInput.GetBindingExpression(ComboBox.SelectedValueProperty);
            be.UpdateSource();
            be = selItemUrlInput.GetBindingExpression(TextBox.TextProperty);
            be.UpdateSource();
            be = selItemUsernameInput.GetBindingExpression(TextBox.TextProperty);
            be.UpdateSource();
            be = selItemPassPinInput.GetBindingExpression(TextBox.TextProperty);
            be.UpdateSource();
            be = selItemTwoFaInput.GetBindingExpression(CheckBox.IsCheckedProperty);
            be.UpdateSource();
            be = selItemTwoFaMethodInput.GetBindingExpression(TextBox.TextProperty);
            be.UpdateSource();
            be = selItemStrengthInput.GetBindingExpression(TextBox.TextProperty);
            be.UpdateSource();
            existingPasswords.Items.Refresh(); // need to do this to get the Strength scores in the grid to update.


            //selectedLogin = (LoginEntry)existingPasswords.SelectedItem;
            //selectedLogin.LoginStrength.Score = 99;
        }

        private void btnUndoChanges_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnDeleteSelected_Click(object sender, RoutedEventArgs e)
        {
            if (existingPasswords.SelectedItems.Count > 0)
            {
                var selItems = existingPasswords.SelectedItems;
                var selItemsList = new ArrayList(selItems);
                foreach (var item in selItemsList)
                {
                    PasswordBank.Remove((LoginEntry)item);
                }

            }
        }

        private void btnDeleteAll_Click(object sender, RoutedEventArgs e)
        {
            PasswordBank.Clear();
        }

        private void btnPassPinAuto_Click(object sender, RoutedEventArgs e)
        {
            
        }

        private void selItemPassPinInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            // This is clever. When  the value in the pin/password box of the existing item editor is changed, it calculates the logins core for it on the fly so taht the user can preview.
            LoginStrength lsFly = new LoginStrength((LoginTypes)selItemTypeInput.SelectedItem, selItemPassPinInput.Text, selItemUsernameInput.Text, (bool)selItemTwoFaInput.IsChecked);
            selItemStrengthInput.Text = lsFly.Score.ToString();

            //TODO Make one of these for each input field in the single item editor which affects the login score (Type as ENum, 2FA as bool, user, and password.)
        }
    }
}
