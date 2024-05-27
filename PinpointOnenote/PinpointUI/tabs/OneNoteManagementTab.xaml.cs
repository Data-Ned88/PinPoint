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
using System.Windows.Navigation;
using System.Windows.Shapes;
using PinpointOnenote;
using System.Xml;
using PinpointUI;
using System.Runtime.InteropServices;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading;

namespace PinpointUI.tabs
{
    /// <summary>
    /// Interaction logic for OneNoteManagementTab.xaml
    /// </summary>
    public partial class OneNoteManagementTab : UserControl, INotifyPropertyChanged
    {
        private MainWindow callingWindow;
        private string callingButtonName;
        private string colTwoHeaderPlacehold;
        private string createNewSectionLabelPlacehold = "Provide a name for your new PinPoint password section in {0}.\n(Max. 25 characters and letters, numbers and spaces only)";
        Microsoft.Office.Interop.OneNote.Application app = OnenoteMethods.InstantiateOneNoteApp();





        private int notebookSelIndex = 0;
        private OneNoteSection selectedSection;
        private XmlNode selectedNotebook;
        private XmlDocument hier;
        private XmlNamespaceManager nsmgr;

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private List<XmlNode> notebookslist;
        public List<XmlNode> Notebookslist
        {
            get { return notebookslist;}
            set { notebookslist = value;
                //selectedNotebook = notebookslist[notebookSelIndex];
                OnPropertyChanged();               
            }
        }

        private List<OneNoteSection> sectionsList;
        public List<OneNoteSection> SectionsList
        {
            get { return sectionsList;}
            set { sectionsList = value;
                OnPropertyChanged();
            }
            
        }



        public OneNoteManagementTab(MainWindow caller,string cbn)
        {
            //The constructor method for the tab.
            //1.It sets this code behind as the XAML tab's data context for the purposes of data binding.
            //2. It passes the input parameter caller (the MainWindow which has instantiated this) to the private variable callingWindow.
            // ... This is so that the constructed OneNoteManagementTab object can manipulate the MainWindow as needed (hide it basically).
            // 3. Instantiates the XML hierarchy and namespace manager for the open OneNote App into hier and nsmgr variables. 
            // ... These are refreshed whenever the btnRefreshSectionInfo button is clicked, which is designed to ask OneNote for latest section info.
            // ... her and nsmgr also rig up the notebooks list on the left hadn lsitbox, and the sectiosn grid for the selected item for notebooks in LH listbox.
            // 4. Finally, selected notebook is set as the first item in the noteboos list, so that selected notebook can inform what sectionsList is, which appears ...
            // ... on the DataGrid present in 'Edit existing' mode). 
            DataContext = this;
            callingWindow = caller;
            callingButtonName = cbn;
            hier = OnenoteMethods.GetOneNoteHierarchy(app);
            nsmgr = OnenoteMethods.GetOneNoteNSMGR(hier);

            notebookslist = OnenoteMethods.GetAvailableNotebooks(hier, nsmgr);
            selectedNotebook = notebookslist[0];
            if (callingButtonName == "LandingCreate_Click")
            {
                colTwoHeaderPlacehold = "Selected notebook: {0}";
                
                //sectionsList = OnenoteMethods.GetSectionsInNotebook(selectedNotebook);
            }
            else
            {
                sectionsList = OnenoteMethods.GetSectionsInNotebook(app,selectedNotebook);
                colTwoHeaderPlacehold = "Sections in your selected notebook ({0})";
            }
            
            InitializeComponent();

            if (callingButtonName == "LandingCreate_Click")
            {
                
                gridNewSectionDEntry.Visibility = Visibility.Visible;
                gridSections.Visibility = Visibility.Hidden;
                ONT_ActionButtonsCreateNew.Visibility = Visibility.Visible;
                ONT_ActionButtonsLoadExist.Visibility = Visibility.Hidden;
            }
            else
            {
                gridNewSectionDEntry.Visibility = Visibility.Hidden;
                gridSections.Visibility = Visibility.Visible;
                ONT_ActionButtonsCreateNew.Visibility = Visibility.Hidden;
                ONT_ActionButtonsLoadExist.Visibility = Visibility.Visible;
            }

            txthdrAvailableSections.Text = string.Format(colTwoHeaderPlacehold, selectedNotebook.Attributes["name"].Value);
            newSectionLabel.Content = string.Format(createNewSectionLabelPlacehold, selectedNotebook.Attributes["name"].Value);
        }

        private void OneNoteTabBackToWelcome_Click(object sender, RoutedEventArgs e)
        {
            // User wants to go back to the welcome page.
            callingWindow.OneNoteTab.IsSelected = false;
            callingWindow.LandingTab.Visibility = Visibility.Visible;
            callingWindow.OneNoteTab.Visibility = Visibility.Hidden;
            callingWindow.LandingTab.IsSelected = true;
            callingWindow.OneNoteTab.Content = null;
            Marshal.FinalReleaseComObject(app);
            app = null;
            GC.Collect();
            GC.WaitForPendingFinalizers();

        }

        private void listAvailableNotebooks_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            selectedNotebook = (XmlNode)listAvailableNotebooks.SelectedItem;
            if (selectedNotebook != null)
            {
                this.Cursor = Cursors.Wait;
                notebookSelIndex = listAvailableNotebooks.SelectedIndex;
                txthdrAvailableSections.Text = string.Format(colTwoHeaderPlacehold, selectedNotebook.Attributes["name"].Value);
                if (newSectionLabel != null)
                {
                    newSectionLabel.Content = string.Format(createNewSectionLabelPlacehold, selectedNotebook.Attributes["name"].Value);
                }
                if (callingButtonName == "LandingLoad_Click")
                {
                    SectionsList = OnenoteMethods.GetSectionsInNotebook(app,selectedNotebook);
                }
                this.Cursor = Cursors.Arrow;
            }
        }

        private void btnRefreshSectionInfo_Click(object sender, RoutedEventArgs e)
        {
            hier = OnenoteMethods.GetOneNoteHierarchy(app);
            nsmgr = OnenoteMethods.GetOneNoteNSMGR(hier);
            Notebookslist = OnenoteMethods.GetAvailableNotebooks(hier, nsmgr);
            selectedNotebook = notebookslist[notebookSelIndex];
            listAvailableNotebooks.SelectedIndex = notebookSelIndex;
            SectionsList = OnenoteMethods.GetSectionsInNotebook(app,selectedNotebook);
            txthdrAvailableSections.Text = string.Format(colTwoHeaderPlacehold, selectedNotebook.Attributes["name"].Value);

        }

        private void gridSections_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            selectedSection = (OneNoteSection)gridSections.SelectedItem;
        }

        private void showTestMessage(OneNoteSection selectedSection)
        {
            //Temporary simple message box placeholder which is called by the fnLoadSection RelayCommand on the load button as "execute".
            //This should open the passwords edit grid final tab for the valid section that the user selects.
            string message_ = $"You have selected {selectedSection.SectionName}!";
            MessageBox.Show(message_);
        }
        private void showNewSectionErrorMessage (string em)
        {
            MessageBox.Show(em, "Invalid Section Name", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void loadSelectedValidSection ()
        {
            
             PasswordSectionEditor PasswordEditor = new PasswordSectionEditor(this, callingWindow,
                                                    callingButtonName, app, selectedNotebook.Attributes["name"].Value,
                                                    hier, nsmgr, selectedSection.SectionName,selectedSection
                                                    );
            callingWindow.PasswordsTab.Content = PasswordEditor;
            callingWindow.OneNoteTab.Visibility = Visibility.Collapsed;
            callingWindow.PasswordsTab.IsSelected = true;
            callingWindow.PasswordsTab.Visibility = Visibility.Visible;
            callingWindow.OneNoteTab.IsSelected = false;




        }
        private void CloseTabAndLoadEditor() 
        {
            Mouse.OverrideCursor = Cursors.Wait;
            loadSelectedValidSection();
            Mouse.OverrideCursor = null; // This is the failsafe when your async/await just won't work. 


        }

        private bool isValidselectedSection(OneNoteSection selectedSection)
        {
            //called by the fnLoadSection RelayCommand on the load button as "canExecute".
            //---Validates that the section selected is a valid pinpoint. Greys the button out if not.
            bool returnable = false;
            btnLoadSection.Cursor = Cursors.Arrow;
            if (selectedSection != null)
            {
                if (selectedSection.IsValidPinPointInstance == true) // going to want to change this to true once we've worked out the function!!!
                {
                    returnable = true;
                    btnLoadSection.Cursor = Cursors.Hand;
                }
                
            }
            return returnable;
        }



        public RelayCommand fnLoadSection => new RelayCommand( execute => { CloseTabAndLoadEditor(); }, //showTestMessage(selectedSection)
                                                        canExecute => { return isValidselectedSection(selectedSection); });

        private void btnCreateSection_Click(object sender, RoutedEventArgs e)
        {
            string newSectionNameData = newSectionName.Text;
            bool validSection = false;
            StringBuilder message = new StringBuilder();
            message.AppendLine("Invalid section name:");
            if (string.IsNullOrEmpty(newSectionName.Text))
            {
                message.AppendLine("->You cannot choose an empty section name.");
            }
            else if (newSectionName.Text.Length >= 26)
            {
                message.AppendLine(string.Format("->Your chosen section name is too long ({0} chars).", newSectionName.Text.Length));
            }
            else
            {
                Regex rx = new Regex(@"[^\w\s]");
                Match match = rx.Match(newSectionNameData);
                if (match.Success)
                {
                    message.AppendLine("->Your chosen section name contains a non-word character.");
                }
                else
                {
                    validSection = true;
                }
            }
            if (validSection)
            {
                // Instantiate Password Editor for new section
                //...Pass through the MainWIndowcalling button name (which should suffice for new/existing),
                //...the section name
                //... the notebook name
                //... isNew bool
                //... the app object. I think probs best (or at the very least quite easy to reninitilize the heirarchy and namespacemanager)
                //... this(just in case)
                // callingWindow (MainWindow object) - so that you can manipulate which tab is visible.


                PasswordSectionEditor PasswordEditor = new PasswordSectionEditor(this,callingWindow,
                                                    callingButtonName,app, selectedNotebook.Attributes["name"].Value,
                                                    hier,nsmgr, newSectionNameData
                                                    );
                callingWindow.PasswordsTab.Content = PasswordEditor;
                callingWindow.PasswordsTab.Visibility = Visibility.Visible;
                callingWindow.OneNoteTab.IsSelected = false;
                callingWindow.PasswordsTab.IsSelected = true;
                callingWindow.OneNoteTab.Visibility = Visibility.Collapsed;

                

                
            }
            else
            {
                showNewSectionErrorMessage(message.ToString());
            }
        }
    } //closes class
} //closes namespace
