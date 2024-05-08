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



        
        public ICommand CopyCellCommand { get; private set; }
        private void CopyCell(object parameter) //input param not useful
        {
            // Logic to copy cell content to the clipboard
            int i = existingPasswords.CurrentCell.Column.DisplayIndex;

            TypeConverter converter = TypeDescriptor.GetConverter(typeof(LoginTypes)); // need this to convert Enums to descriptions.

            LoginEntry leClicked = (LoginEntry)existingPasswords.CurrentCell.Item;
            List<string> gridOrderedProperties = new List<string>
            {
                // switch these around in order depending on the column layout of your data grid.
                leClicked.LoginDescription,
                converter.ConvertToString(leClicked.LoginType),leClicked.LoginUrl,leClicked.LoginUsername,
                leClicked.LoginPass,leClicked.HasTwoFa.ToString(),leClicked.TwoFaMethod,leClicked.LoginStrength.Score.ToString()
            };
            string valueReturnable = gridOrderedProperties[i];
            Clipboard.SetText(valueReturnable);
        }

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
            CopyCellCommand = new RelayCommand(CopyCell); // Has to happen before initialise in order to work.
            InitializeComponent();
            
            pwordTabSectionTitle.Text = mainBannerText;
            pwordTabSectionSubTitle.Text = subBannerText;
            setVisibilitySinglePasswordEditorConstants();
            toggleVisibilitySinglePasswordEditor("new", Visibility.Hidden);
            toggleVisibilitySinglePasswordEditor("sel", Visibility.Hidden);
            singleItemAreaHeader.Text = "";
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
        private void toggleVisibilitySinglePasswordEditor(string set = "new", Visibility _mode = Visibility.Hidden)
        {
            //This does alternates for NEW Password vs edit existing password

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
                btnCloseNewEditor.Visibility = _mode;

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
                btnUndoChanges.Visibility = _mode;
            }
        }

        private void setVisibilitySinglePasswordEditorConstants(Visibility _mode = Visibility.Hidden)
        {
            singleItemDescLabel.Visibility = _mode;
            singleItemTypeLabel.Visibility = _mode;
            singleItemUrlLabel.Visibility = _mode;
            singleItemUsernameLabel.Visibility = _mode;
            singleItemPassPinLabel.Visibility = _mode;
            singleItemTwoFaLabel.Visibility = _mode;
            singleItemTwoFaMethodLabel.Visibility = _mode;
            singleItemStengthLabel.Visibility = _mode;
            btnPassPinAuto.Visibility = _mode;
        }
        private void setVisibilityDeletionButtons(Visibility _mode = Visibility.Hidden)
        {
            btnDeleteAll.Visibility = _mode;
            btnDeleteSelected.Visibility = _mode;
        }

        private void existingPasswords_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            if (existingPasswords.SelectedItems.Count == 1)
            {
                selectedLogin = (LoginEntry)existingPasswords.SelectedItem;
                toggleVisibilitySinglePasswordEditor("new", Visibility.Hidden);
                toggleVisibilitySinglePasswordEditor("sel", Visibility.Visible);
                setVisibilitySinglePasswordEditorConstants(Visibility.Visible);
                btnDeleteSelected.Visibility = Visibility.Visible;
                //singleItemAreaHeader.Text = "Edit Selected Login";


            }
            else if (existingPasswords.SelectedItems.Count > 1)
            {
                toggleVisibilitySinglePasswordEditor("new", Visibility.Hidden);
                toggleVisibilitySinglePasswordEditor("sel", Visibility.Hidden);
                setVisibilitySinglePasswordEditorConstants();
                btnDeleteSelected.Visibility = Visibility.Visible;
                singleItemAreaHeader.Text = String.Format("{0} logins selected", existingPasswords.SelectedItems.Count.ToString());
            }
            else
            {
                //TODO - I don't like this. It hides everything except the labels when no passwords are selected. Hide the labels as well.??
                toggleVisibilitySinglePasswordEditor("new", Visibility.Hidden);
                toggleVisibilitySinglePasswordEditor("sel", Visibility.Hidden);
                setVisibilitySinglePasswordEditorConstants();
                btnDeleteSelected.Visibility = Visibility.Hidden;
                singleItemAreaHeader.Text = "";
            }
        }

        private void ExistingPasswords_Loaded(object sender, RoutedEventArgs e)
        {
            // Subscribe to the event when the data grid is loaded
            // Ned - we're doing this becuase we can't have edit selected login in the SelectionChanged handler, becuase it will override the save status message from the Update button on existing records.
            existingPasswords.AddHandler(DataGridRow.MouseLeftButtonUpEvent, new MouseButtonEventHandler(OnRowClicked), true);
        }

        private void OnRowClicked(object sender, MouseButtonEventArgs e)
        {
            if (existingPasswords.SelectedItems.Count == 1)
            {
                singleItemAreaHeader.Text = "Edit Selected Login";
            }

        }


        private void btnNewPassInExisting_Click(object sender, RoutedEventArgs e)
        {
            toggleVisibilitySinglePasswordEditor("new", Visibility.Visible);
            toggleVisibilitySinglePasswordEditor("sel", Visibility.Hidden);
            setVisibilitySinglePasswordEditorConstants(Visibility.Visible);
            btnDeleteSelected.Visibility = Visibility.Hidden;
            existingPasswords.SelectedItem = null;
            singleItemAreaHeader.Text = "New Login";
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
            
            singleItemAreaHeader.Text = "Changes saved to selected login.";



        }

        private void btnUndoChanges_Click(object sender, RoutedEventArgs e)
        {
            toggleVisibilitySinglePasswordEditor("new", Visibility.Hidden);
            toggleVisibilitySinglePasswordEditor("sel", Visibility.Hidden);
            setVisibilitySinglePasswordEditorConstants();
            btnDeleteSelected.Visibility = Visibility.Hidden;
            existingPasswords.SelectedItem = null;
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
            //TODO new modal
        }

        private void selItemPassPinInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            // This is clever. When  the value in the pin/password box of the existing item editor is changed, it calculates the logins core for it on the fly so taht the user can preview.
            if (existingPasswords.SelectedItems.Count > 0)
            {
                LoginStrength lsFly = new LoginStrength((LoginTypes)selItemTypeInput.SelectedItem, selItemPassPinInput.Text, selItemUsernameInput.Text, (bool)selItemTwoFaInput.IsChecked);
                selItemStrengthInput.Text = lsFly.Score.ToString();
            }
            //TODO Make one of these for each input field in the single item editor which affects the login score (Type as ENum, 2FA as bool, user, and password.)
        }

        private void btnCloseNewEditor_Click(object sender, RoutedEventArgs e)
        {
            toggleVisibilitySinglePasswordEditor("new", Visibility.Hidden);
            toggleVisibilitySinglePasswordEditor("sel", Visibility.Hidden);
            setVisibilitySinglePasswordEditorConstants();
            btnDeleteSelected.Visibility = Visibility.Hidden;
            existingPasswords.SelectedItem = null;
            singleItemAreaHeader.Text = "";
        }
    }
}
