using PinpointOnenote;
using PinpointOnenote.OneNoteClasses;
using PinpointUI.modals;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
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
        private string sectionId;
        private OneNoteSection onSection;
        private string mainBannerPlaceholder = "OneNote Password Section: {0}";
        private string subBannerPlaceholder = "(Notebook: {0})"; // optional switch later to "(Notebook: {0} Section Group: {1})"
        private string mainBannerText;
        private string subBannerText;
        private string passwordBankPageId;
        private XDocument passwordBankPageContent;
        XElement stylingresource = XElement.Parse(PinpointOnenote.Properties.Resources.OneNotePageAndElementStyles);
        private int countUpdates = 0;
        private Dictionary<string, string> formattingFromPassBankOnenote;

        //The below is a mapping dict to hold the row state (added/deleted/uncahnged/modified) for a LoginEntry Item.
        //It is populated and updated by UpdateRowState beneath it, which is itself triggered by the add new/Update Existing buttons,
        //      so that the correct row state for that Login Entry is available to the existingPasswordsDatagrid to act on it when its LoadingRow handler function is triggered.
        //      It is scbrubbed clean by the "Clear Button".
        private string GetBrainToolTip(object ltype = null)
        {
            if (ltype == null || (LoginTypes)ltype == LoginTypes.NotSet || (LoginTypes)ltype == LoginTypes.Password) {
                return "Generate Secure Password";
            }
            else if ((LoginTypes)ltype == LoginTypes.PinSix)
            {
                return "Generate Secure 6-Digit PIN";
            }
            else
            {
                return "Generate Secure 4-Digit PIN";
            }
        }
        private string GetBrainOutput(object ltype = null)
        {
            if (ltype == null || (LoginTypes)ltype == LoginTypes.NotSet || (LoginTypes)ltype == LoginTypes.Password)
            {
                return LoginFunctionality.generateSecureRandomPassword(13);
            }
            else if ((LoginTypes)ltype == LoginTypes.PinSix)
            {
                return LoginFunctionality.generateSecurePinSix();
            }
            else
            {
                return LoginFunctionality.generateSecurePinFour();
            }
        }
        private void RenderBrainOutput()
        {
            object ltype;
            if (selItemTypeInput.Visibility == Visibility.Visible)
            {
                ltype = selItemTypeInput.SelectedItem;
                selItemPassPinInput.Text = GetBrainOutput(ltype);
            }
            else if (newItemTypeInput.Visibility == Visibility.Visible)
            {
                ltype = newItemTypeInput.SelectedItem;
                newItemPassPinInput.Text = GetBrainOutput(ltype);
            }
        }


        public LoginTypes SelectedLoginTypeNewPasswords { get; set; } = LoginTypes.NotSet;
        public Brush OriginalBorderBrushTextBoxInputs { get; set; }
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

        private ObservableCollection<LoginEntryInterface> passwordBank;
        public ObservableCollection<LoginEntryInterface> PasswordBank
        {
            get { return passwordBank; }
            set
            {
                passwordBank = value;
                OnPropertyChanged();
            }

        }
        private List<LoginEntry> passwordBankOriginal;


        private LoginEntryInterface selectedLogin;
        public LoginEntryInterface SelectedLogin
        {
            get { return selectedLogin; }
            set
            {
                selectedLogin = value;
                OnPropertyChanged();
            }

        }
        #region Instantiantion Code
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
                passwordBank = new ObservableCollection<LoginEntryInterface>();
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
                sectionId = inpSection.SectionID;
                // hydrate password Bank with the PasswordBank page from the section.
                passwordBankPageId = OneNotePageFmtMethods.GetPageIdInSection(inpSection.SectionXML, "Password Bank"); // THIS WILL HAVE BEEN TESTED AS VALID BY PREV GUI.
                passwordBankPageContent = OneNotePageFmtMethods.GetPageXmlLinq(app, passwordBankPageId);
                passwordBankOriginal = DataParsers.GetPasswordsFromValidPage(passwordBankPageContent, passwordBankPageContent.Root.Name.Namespace);
                formattingFromPassBankOnenote = DataParsers.GetFormattingFromValidPage(passwordBankPageContent);
                passwordBankOriginal = LoginFunctionality.HydrateIdAndModifiedSort(passwordBankOriginal);
                passwordBank = new ObservableCollection<LoginEntryInterface>();
                foreach (LoginEntry le in passwordBankOriginal)
                {
                    passwordBank.Add(
                        new LoginEntryInterface(le)
                        );
                }
                LoginEntryInterfaceFunctionality.HydrateIdColl(passwordBank);
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
            OriginalBorderBrushTextBoxInputs = selItemPassPinInput.BorderBrush;
            btnPassPinAuto.ToolTip = "Generate Secure Password";
        }
        #endregion
        private void PwordTabBackToSections_Click(object sender, RoutedEventArgs e)
        {
            // User wants to go back to the previous page (onenote sections).
            mainCallingWindow.OneNoteTab.IsSelected = true;
            mainCallingWindow.OneNoteTab.Visibility = Visibility.Visible;
            mainCallingWindow.PasswordsTab.IsSelected = false;
            mainCallingWindow.PasswordsTab.Visibility = Visibility.Hidden;
            mainCallingWindow.OneNoteTab.Visibility = Visibility.Visible;
            mainCallingWindow.PasswordsTab.Content = null;
        }

        private void PwordTabSave_Click(object sender, RoutedEventArgs e)
        {

        }

        private void PwordTabClear_Click(object sender, RoutedEventArgs e)
        {
            PasswordBank = new ObservableCollection<LoginEntryInterface>();
            foreach (LoginEntry le in passwordBankOriginal)
            {
                PasswordBank.Add(
                    new LoginEntryInterface(le)
                    );
            }
            existingPasswords.SelectedItem = null;
            existingPasswords.Items.Refresh();
            toggleVisibilitySinglePasswordEditor("new", Visibility.Hidden);
            toggleVisibilitySinglePasswordEditor("sel", Visibility.Hidden);
            setVisibilitySinglePasswordEditorConstants();
            btnDeleteSelected.Visibility = Visibility.Hidden;
            singleItemAreaHeader.Text = "";
            existingPasswords.SelectedItem = null;
            countUpdates = 0;
            pwordTabSectionTitle.Text = mainBannerText;//mainBannerText + "*";

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
                newItemStrengthLabel.Visibility = _mode;
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
                selItemStrengthLabel.Visibility = _mode;
                btnUpdate.Visibility = _mode;
                btnUndoChanges.Visibility = _mode;
            }
        }

        private void setToDefaultSinglePasswordEditorBordersTTs(string set = "new")
        {
            //This does alternates for NEW Password vs edit existing password

            if (set == "new")
            {
                BorderAndToolTip(newItemDescInput);
                BorderAndToolTip(newItemUsernameInput);
                BorderAndToolTip(newItemPassPinInput);
            }
            else
            {
                BorderAndToolTip(selItemDescInput);
                BorderAndToolTip(selItemUsernameInput);
                BorderAndToolTip(selItemPassPinInput);
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
                selectedLogin = (LoginEntryInterface)existingPasswords.SelectedItem;
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
                toggleVisibilitySinglePasswordEditor("new", Visibility.Hidden);
                toggleVisibilitySinglePasswordEditor("sel", Visibility.Hidden);
                setVisibilitySinglePasswordEditorConstants();
                btnDeleteSelected.Visibility = Visibility.Hidden;
                singleItemAreaHeader.Text = "";
            }
            setToDefaultSinglePasswordEditorBordersTTs("sel");
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
            singleItemAreaHeader.Text = "New Login";
        }

        private void btnImportFromFile_Click(object sender, RoutedEventArgs e)
        {
            //Tester Function to ascertain that the OneNote confirmation form broadly works.
            CsvLoad csvLoad = new CsvLoad();
            Opacity = 0.6;
            csvLoad.ShowDialog();
            Opacity = 1;
            if (csvLoad.ExitChoice == false) //The user did not cancel and we have a password bank
            {
                //do Nothing
                foreach (LoginEntry csv_le in csvLoad.ReturnPasswordBank)
                {
                    LoginEntryInterface nle = new LoginEntryInterface(csv_le);
                    nle.InterfaceStatusColour = "#349D1F";
                    nle.InterfaceStatusIcon = "\u002B";
                    PasswordBank.Add(nle);
                }
                LoginEntryInterfaceFunctionality.HydrateIdColl(PasswordBank);
            }
        }


        private void btnAddNew_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnUpdate_Click(object sender, RoutedEventArgs e)
        {
            fnUpdateSingleItemInGrid();
        }

        #region Add and update execute and canExecute inputs

        private void fnUpdateSingleItemInGrid ()
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

            existingPasswords.Items.Refresh(); // need to do this to get the Strength scores in the grid to update.
            selectedLogin.LastModified = DateTime.Now;
            selectedLogin.InterfaceStatusIcon = "\u270E";
            selectedLogin.InterfaceStatusColour = "#D28E14";
            singleItemAreaHeader.Text = "Changes saved to selected login.";
            countUpdates++;
            pwordTabSectionTitle.Text = mainBannerText + "*";

            //UpdateRowState(selectedLogin.id, DataRowState.Modified);
        }

        private bool fnCanUpdateSingleItemInGrid()
        {
            bool returnable = false;
            bool userNameCheckOnPasswords = true;
            btnUpdate.Cursor = Cursors.Arrow;
            if (selItemStrengthLabel.IsLoaded)
            {
                if (existingPasswords.SelectedItems.Count > 0 && selItemTypeInput.SelectedItem != null)
                {
                    LoginStrength lsFly = new LoginStrength((LoginTypes)selItemTypeInput.SelectedItem, selItemPassPinInput.Text, selItemUsernameInput.Text, (bool)selItemTwoFaInput.IsChecked);
                    bool generatesScore = lsFly.Score != -99;
                    bool populatedDescription = selItemDescInput.Text != null && selItemDescInput.Text.Length > 0;
                    if ((LoginTypes)selItemTypeInput.SelectedItem == LoginTypes.Password)
                    {
                        if (selItemUsernameInput.Text == null || selItemUsernameInput.Text.Length == 0)
                        {
                            userNameCheckOnPasswords = false;
                        }
                    }
                    if (generatesScore && populatedDescription && userNameCheckOnPasswords)
                    {
                        if ((LoginTypes)selItemTypeInput.SelectedItem == LoginTypes.Password)
                        {
                            btnUpdate.Cursor = Cursors.Hand;
                            returnable = true;
                        }
                        else if (((LoginTypes)selItemTypeInput.SelectedItem == LoginTypes.PinSix) && LoginFunctionality.isValidPinSix(selItemPassPinInput.Text))
                        {
                            btnUpdate.Cursor = Cursors.Hand;
                            returnable = true;
                        }
                        else if (((LoginTypes)selItemTypeInput.SelectedItem == LoginTypes.PinFour) && LoginFunctionality.isValidPinFour(selItemPassPinInput.Text))
                        {
                            btnUpdate.Cursor = Cursors.Hand;
                            returnable = true;
                        }
                        
                    }
                }

            }

            return returnable;
        }
        private bool fnCanAddSingleItemToGrid()
        {
            bool returnable = false;
            bool userNameCheckOnPasswords = true;
            btnAddNew.Cursor = Cursors.Arrow;
            if (newItemStrengthLabel.IsLoaded)
            {
                LoginStrength lsFly = new LoginStrength((LoginTypes)newItemTypeInput.SelectedItem, newItemPassPinInput.Text, newItemUsernameInput.Text, (bool)newItemTwoFaInput.IsChecked);
                bool generatesScore = lsFly.Score != -99;
                bool populatedDescription = newItemDescInput.Text != null && newItemDescInput.Text.Length > 0;
                if ((LoginTypes)newItemTypeInput.SelectedItem == LoginTypes.Password)
                {
                    if (newItemUsernameInput.Text == null || newItemUsernameInput.Text.Length == 0)
                    {
                        userNameCheckOnPasswords = false;
                    }
                }
                if (generatesScore && populatedDescription && userNameCheckOnPasswords)
                {
                    if ((LoginTypes)newItemTypeInput.SelectedItem == LoginTypes.Password)
                    {
                        btnAddNew.Cursor = Cursors.Hand;
                        returnable = true;
                    }
                    else if (((LoginTypes)newItemTypeInput.SelectedItem == LoginTypes.PinSix) && LoginFunctionality.isValidPinSix(newItemPassPinInput.Text))
                    {
                        btnAddNew.Cursor = Cursors.Hand;
                        returnable = true;
                    }
                    else if (((LoginTypes)newItemTypeInput.SelectedItem == LoginTypes.PinFour) && LoginFunctionality.isValidPinFour(newItemPassPinInput.Text))
                    {
                        btnAddNew.Cursor = Cursors.Hand;
                        returnable = true;
                    }
                }
            }

            return returnable;
        }

        private void fnAddSingleItemToGrid()
        {
            LoginEntryInterface newEntryFromForm = new LoginEntryInterface();
            newEntryFromForm.LoginDescription = newItemDescInput.Text;
            newEntryFromForm.LoginType = (LoginTypes)newItemTypeInput.SelectedItem;
            newEntryFromForm.LoginUrl = newItemUrlInput.Text;
            newEntryFromForm.LoginUsername = newItemUsernameInput.Text;
            newEntryFromForm.LoginPass = newItemPassPinInput.Text;
            newEntryFromForm.HasTwoFa = (bool)newItemTwoFaInput.IsChecked;
            newEntryFromForm.TwoFaMethod = newItemTwoFaMethodInput.Text;
            newEntryFromForm.LastModified = DateTime.Now;
            newEntryFromForm.InterfaceStatusIcon = "\u002B";
            newEntryFromForm.InterfaceStatusColour = "#349D1F";


            passwordBank.Add(newEntryFromForm);

            existingPasswords.Items.Refresh();
            toggleVisibilitySinglePasswordEditor("new", Visibility.Hidden);
            toggleVisibilitySinglePasswordEditor("sel", Visibility.Hidden);
            setVisibilitySinglePasswordEditorConstants();
            btnDeleteSelected.Visibility = Visibility.Hidden;
            singleItemAreaHeader.Text = "";
            existingPasswords.SelectedItem = null;
            countUpdates++;
            pwordTabSectionTitle.Text = mainBannerText + "*";
            newItemDescInput.Text = null;
            newItemUrlInput.Text = null;
            newItemTypeInput.SelectedItem = LoginTypes.NotSet;
            newItemUsernameInput.Text = null;
            newItemPassPinInput.Text = null;
            newItemTwoFaMethodInput.Text = null;
            newItemTwoFaInput.IsChecked = false;

            //UpdateRowState(newEntryFromForm.id, DataRowState.Added);

        }

        #endregion
        #region RelayCommands for Add and update
        public RelayCommand fnNewItemToGrid_NewButton => new RelayCommand(execute => { fnAddSingleItemToGrid(); },
                                                                canExecute => { return fnCanAddSingleItemToGrid(); });
        public RelayCommand fnUpdateItemInGrid_UpdateButton => new RelayCommand(execute => { fnUpdateSingleItemInGrid(); },
                                                                canExecute => { return fnCanUpdateSingleItemInGrid(); });
        #endregion

        #region Publish To OneNote RelayCOmmand and 2 x precedent functions
        private bool canPublishToOneNote() 
        {
            PwordTabSave.Cursor = Cursors.Arrow;
            bool returnable = false;
            if (passwordBank.Count > 0)
            {
                PwordTabSave.Cursor = Cursors.Hand;
                returnable = true;
            }
            return returnable;
        }



        private async void ActionPublishOneNote(ConfirmPublish cp)
        {
            XElement tableCol = stylingresource.Descendants("ColorTheme").Where(x => x.Attribute("name").Value == cp.SelectedTheme).First();
            XElement tableSize = stylingresource.Descendants("TableSizing").Where(x => x.Attribute("name").Value == cp.SelectedFontSize).First();
            XElement tabColourEl = stylingresource.Elements("BaseStyles").Where(x => x.Attribute("name").Value == "Base").First().Elements("SectionTabCol").FirstOrDefault();
            //Hydrate passwordbank and prepare for publication
            List<LoginEntry> passwordBankPublish = LoginEntryInterfaceFunctionality.GetPublishableBankFromInterface(passwordBank);

            //Convert the password bank data to OneNote schema
            passwordBankPublish = LoginFunctionality.HydrateIdAndModifiedSort(passwordBankPublish);
            OneNoteTable passwordBankPublishTable = DataParsers.BuildTableFromPasswordBank(passwordBankPublish, tableSize, tableCol, cp.SelectedFont);
            List<OneNoteOE> passwordPageData = DataParsers.BuildPasswordBankPageData(passwordBankPublishTable, tableSize, cp.SelectedFont);

            if (isNew)
            {
                //1.Create Section
                //1a. Necessary params
                List<OneNoteSection> sectionsthisNotebook = OnenoteMethods.GetSectionsInNotebook(app, hier, nsmgr, notebookName);
                string sectionColour = "#F6B078";
                if (tabColourEl != null)
                {
                    sectionColour = tabColourEl.Value.ToString();
                }
                sectionId = OnenoteMethods.AddSectionToNotebook(app, ref hier, ref nsmgr, sectionName, ref sectionsthisNotebook, notebookId, sectionColour);

                //2.Create Notes and Instructions Page
                string newNotesPageId = OneNotePageFmtMethods.AddOneNoteNewPage(app, sectionId, "Notes and Instructions");

                //3.Create Password Bank Page
                string newPasswordBankPageId = OneNotePageFmtMethods.AddOneNoteNewPage(app, sectionId, "Password Bank");

                //4. Get the section XML again updated with the new page IDs, then prepare the section-id>page-Id lookup table.
                sectionsthisNotebook = OnenoteMethods.GetSectionsInNotebook(app, hier, nsmgr, notebookName);
                onSection = sectionsthisNotebook.Where(x => x.SectionID == sectionId).First();
                Dictionary<string, Dictionary<string, object>> newSectionItemsLookup = OnenoteMethods.GetSectionPagesLookup(app, onSection.SectionXML); //Password Section page links lookup

                //5.Rendering
                //5.a. Render notes page

                XElement notesResource = XElement.Parse(PinpointOnenote.Properties.Resources.StaticAndTestData);
                XElement notesPageStaticXml = notesResource.Descendants("Page").Where(x => x.Attribute("name").Value == "Notes and Instructions").First();
                List<OneNoteOE> notesPageData = DataParsers.BuildPageDataFromXml(notesPageStaticXml, tableSize, tableCol, AllowableFonts.Arial, newSectionItemsLookup);
                XDocument renderNotesPage = OneNotePageFmtMethods.RenderOneNotePage(app, newNotesPageId, notesPageData, true);


                //6. Render new Password bank page with data created at the top of this function.
                XDocument renderPasswordPage = OneNotePageFmtMethods.RenderOneNotePage(app, newPasswordBankPageId, passwordPageData, true);

                isNew = false;
            }
            else
            {
                //Update Password Bank Page
                XDocument renderPasswordPage = OneNotePageFmtMethods.RenderOneNotePage(app, passwordBankPageId, passwordPageData);

            }
            //Set Password Bank Original to passwordBank (permanent save.)
            passwordBankOriginal = LoginEntryInterfaceFunctionality.GetPublishableBankFromInterface(passwordBank);
            PasswordBank = LoginEntryInterfaceFunctionality.ResetPasswordBankChangeIcons(PasswordBank);

        }


        private async Task PublishToOneNote()
        {
            Opacity = 0.6;
            Dictionary<string, string> modalParamConfirmPublish = null;
            if (!isNew)
            {
                modalParamConfirmPublish = formattingFromPassBankOnenote;
            }

            ConfirmPublish confirmPub = new ConfirmPublish(modalParamConfirmPublish);
            
            confirmPub.ShowDialog();
            

            if (confirmPub.ExitChoice == false)
            {
                modals.ProgressBar pb = new modals.ProgressBar();
                pb.Show();
                await Task.Run(() =>  { ActionPublishOneNote(confirmPub); }); 
                // THis works because of all the async/await, and because the progress bar is a modal, and becuase the Action function does stuff to data only, not anything to form elements.
                // It can therefore survive on the background thread.
                pb.Close();
                pwordTabSectionTitle.Text = mainBannerText;
            }
            Opacity = 1;
        }
        
        public RelayCommand fnPublishToOneNoteButtonCmd => new RelayCommand(async execute => await PublishToOneNote() ,
                                                                canExecute => { return canPublishToOneNote(); });

        private void PublishToOneNoteForm()
        {
            //Tester Function to ascertain that the OneNote confirmation form broadly works.
            ConfirmPublish confirmPub = new ConfirmPublish();
            Opacity = 0.6;
            confirmPub.ShowDialog();
            Opacity = 1;
            if (confirmPub.ExitChoice == false)
            {
                //Console.WriteLine(confirmPub.SelectedTheme);
                //Console.WriteLine(confirmPub.SelectedFontSize);
                //Console.WriteLine(confirmPub.SelectedFont.ToString());
            }
        }





    #endregion


    private void btnUndoChanges_Click(object sender, RoutedEventArgs e)
        {
            toggleVisibilitySinglePasswordEditor("new", Visibility.Hidden);
            toggleVisibilitySinglePasswordEditor("sel", Visibility.Hidden);
            setVisibilitySinglePasswordEditorConstants();
            setToDefaultSinglePasswordEditorBordersTTs("sel");
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
                    PasswordBank.Remove((LoginEntryInterface)item);
                }
                pwordTabSectionTitle.Text = mainBannerText + "*";
            }
        }

        private void btnDeleteAll_Click(object sender, RoutedEventArgs e)
        {
            PasswordBank.Clear();
            pwordTabSectionTitle.Text = mainBannerText + "*";
        }

        private void singleEditorScoreFormat(ComboBox TypeInput,TextBox PassPinInput, TextBox UsernameInput, CheckBox ItemTwoFaInput, Label StrengthLabel)
        {
            LoginStrength lsFly = new LoginStrength((LoginTypes)TypeInput.SelectedItem, PassPinInput.Text, UsernameInput.Text, (bool)ItemTwoFaInput.IsChecked);
            StrengthLabel.Content = lsFly.Score.ToString();
            StrengthLabel.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(lsFly.cellColour));
            StrengthLabel.ToolTip = lsFly.ScoreText;
        }

        private void btnPassPinAuto_Click(object sender, RoutedEventArgs e)
        {
            RenderBrainOutput();
        }


        private void BorderAndToolTip (TextBox newOrExistingTextBox, bool setRed=false, string toolTipText = null)
        {
            if (newOrExistingTextBox != null)
            {
                if (setRed)
                {
                    newOrExistingTextBox.BorderBrush = Brushes.Red;
                }
                else
                {
                    newOrExistingTextBox.BorderBrush = OriginalBorderBrushTextBoxInputs;
                }

                newOrExistingTextBox.ToolTip = toolTipText;
            }

        }
        private void passTypeConditionalRedBorderAndToolTip(TextBox newOrExistingTextBox,ComboBox predicateComboBox)
        {
            LoginTypes selItemLoginType = (LoginTypes)predicateComboBox.SelectedItem;
            if (selItemLoginType == LoginTypes.PinFour)
            {
                if (!LoginFunctionality.isValidPinFour(newOrExistingTextBox.Text))
                {
                    BorderAndToolTip(newOrExistingTextBox, true, "Not a valid 4-digit PIN");
                }
                else
                {
                    BorderAndToolTip(newOrExistingTextBox);
                }
            }
            else if (selItemLoginType == LoginTypes.PinSix)
            {
                if (!LoginFunctionality.isValidPinSix(newOrExistingTextBox.Text))
                {
                    BorderAndToolTip(newOrExistingTextBox, true, "Not a valid 6-digit PIN");
                }
                else
                {
                    BorderAndToolTip(newOrExistingTextBox);
                }
            }
            else if (selItemLoginType == LoginTypes.Password)
            {
                if (newOrExistingTextBox.Text == null || newOrExistingTextBox.Text.Length == 0)
                {
                    BorderAndToolTip(newOrExistingTextBox, true, "Password is empty.");
                }
                else
                {
                    BorderAndToolTip(newOrExistingTextBox);
                }
            }
            else // LoginTypes.NotSet or null
            {
                BorderAndToolTip(newOrExistingTextBox);
            }
        }

        private void selItemPassPinInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            // This is clever. When  the value in the pin/password box of the existing item editor is changed, it calculates the logins core for it on the fly so taht the user can preview.
            if (existingPasswords.SelectedItems.Count > 0 && selItemTypeInput.SelectedItem != null)
            {
                passTypeConditionalRedBorderAndToolTip(selItemPassPinInput, selItemTypeInput);
                singleEditorScoreFormat(selItemTypeInput, selItemPassPinInput, selItemUsernameInput, selItemTwoFaInput, selItemStrengthLabel);
            }

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

        private void selItemTypeInput_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (existingPasswords.SelectedItems.Count > 0 && selItemTypeInput.SelectedItem != null)
            {
                //PIN/Password Text Box
                passTypeConditionalRedBorderAndToolTip(selItemPassPinInput, selItemTypeInput);
                LoginTypes selItemLoginType = (LoginTypes)selItemTypeInput.SelectedItem;
                //User Text Box (password only)
                if ((selItemUsernameInput.Text == null || selItemUsernameInput.Text.Length == 0) && selItemLoginType == LoginTypes.Password)
                {
                    BorderAndToolTip(selItemUsernameInput,true,"Logins of type 'Password' need usernames.");
                }
                else
                {
                    BorderAndToolTip(selItemUsernameInput);
                }
                singleEditorScoreFormat(selItemTypeInput, selItemPassPinInput, selItemUsernameInput, selItemTwoFaInput, selItemStrengthLabel);
                btnPassPinAuto.ToolTip = GetBrainToolTip(selItemTypeInput.SelectedItem);
            }
        }

        private void selItemUsernameInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (existingPasswords.SelectedItems.Count > 0 && selItemTypeInput.SelectedItem != null)
            {
                LoginTypes selItemLoginType = (LoginTypes)selItemTypeInput.SelectedItem;
                if ((selItemUsernameInput.Text == null || selItemUsernameInput.Text.Length == 0) && selItemLoginType == LoginTypes.Password)
                {
                    BorderAndToolTip(selItemUsernameInput, true, "Logins of type 'Password' need usernames.");
                }
                else
                {
                    BorderAndToolTip(selItemUsernameInput);
                }
                singleEditorScoreFormat(selItemTypeInput, selItemPassPinInput, selItemUsernameInput, selItemTwoFaInput, selItemStrengthLabel);
            }
        }

        private void selItemDescInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (selItemDescInput.Text == null || selItemDescInput.Text.Length == 0)
            {
                BorderAndToolTip(selItemDescInput, true, "Please add a value for Description");
            }
            else
            {
                BorderAndToolTip(selItemDescInput);
            }
        }

        private void selItemTwoFaInput_Checked(object sender, RoutedEventArgs e)
        {
            if (existingPasswords.SelectedItems.Count > 0 && selItemTypeInput.SelectedItem != null)
            {
                singleEditorScoreFormat(selItemTypeInput, selItemPassPinInput, selItemUsernameInput, selItemTwoFaInput, selItemStrengthLabel);
            }
        }

        private void newItemTwoFaInput_Checked(object sender, RoutedEventArgs e)
        {
            if (this.IsLoaded)
            {

                singleEditorScoreFormat(newItemTypeInput, newItemPassPinInput, newItemUsernameInput, newItemTwoFaInput, newItemStrengthLabel);
            }
        }

        private void newItemDescInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (this.IsLoaded)
            {

                if (newItemDescInput.Text == null || newItemDescInput.Text.Length == 0)
                {
                    BorderAndToolTip(newItemDescInput, true, "Please add a value for Description");
                }
                else
                {
                    BorderAndToolTip(newItemDescInput);
                }
            }
        }

        private void newItemTypeInput_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.IsLoaded) // THis stops selection change events being fired on Itnitialisation, when you first set the defaults.
            {

                //PIN/Password Text Box
                passTypeConditionalRedBorderAndToolTip(newItemPassPinInput, newItemTypeInput);
                LoginTypes newItemLoginType = (LoginTypes)newItemTypeInput.SelectedItem;
                //User Text Box (password only)
                if ((newItemUsernameInput.Text == null || newItemUsernameInput.Text.Length == 0) && newItemLoginType == LoginTypes.Password)
                {
                    BorderAndToolTip(newItemUsernameInput, true, "Logins of type 'Password' need usernames.");
                }
                else
                {
                    BorderAndToolTip(newItemUsernameInput);
                }
                singleEditorScoreFormat(newItemTypeInput, newItemPassPinInput, newItemUsernameInput, newItemTwoFaInput, newItemStrengthLabel);

                btnPassPinAuto.ToolTip = GetBrainToolTip(newItemTypeInput.SelectedItem);
            }
        }

        private void newItemUsernameInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (this.IsLoaded)
            {
                LoginTypes newItemLoginType = (LoginTypes)newItemTypeInput.SelectedItem;
                if ((newItemUsernameInput.Text == null || newItemUsernameInput.Text.Length == 0) && newItemLoginType == LoginTypes.Password)
                {
                    BorderAndToolTip(newItemUsernameInput, true, "Logins of type 'Password' need usernames.");
                }
                else
                {
                    BorderAndToolTip(newItemUsernameInput);
                }
                singleEditorScoreFormat(newItemTypeInput, newItemPassPinInput, newItemUsernameInput, newItemTwoFaInput, newItemStrengthLabel);
            }
        }

        private void newItemPassPinInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (this.IsLoaded)
            {
                passTypeConditionalRedBorderAndToolTip(newItemPassPinInput, newItemTypeInput);
                singleEditorScoreFormat(newItemTypeInput, newItemPassPinInput, newItemUsernameInput, newItemTwoFaInput, newItemStrengthLabel);
            }
        }


        private void DigiVulnScore_Click(object sender, RoutedEventArgs e)
        {
            List<LoginEntry> reportPasswordBank = LoginEntryInterfaceFunctionality.GetPublishableBankFromInterface(passwordBank);
            SecurityReport sr = new SecurityReport(reportPasswordBank, sectionName);
            Opacity = 0.6;
            sr.ShowDialog();
            Opacity = 1;
        }
    }
}
