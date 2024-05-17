using PinpointOnenote;
using PinpointUI.tabs;
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
using System.Windows.Shapes;

namespace PinpointUI.modals
{
    /// <summary>
    /// Interaction logic for CsvLoad.xaml
    /// </summary>
    public partial class CsvLoad : Window
    {
        public bool ExitChoice { get; set; }

        private List<string> publicSelectableColumns;
        private List<string> openingStateColumns { get; set; } = new List<string> { "No CSV Selected" };
        private List<string> columnsFromCSVLoad;

        private string selectedFileData;
        public List<LoginEntry> ReturnPasswordBank { get; set; }

        public Dictionary<string,string> MappingChoice { get; set; } //This gets returned. set by FillMappingDict() void only after validation on selected items.

        

        public CsvLoad()
        {
            DataContext = this;
            publicSelectableColumns = openingStateColumns;

            InitializeComponent();


        }

        private void btnSelectCSVFile_Click(object sender, RoutedEventArgs e)
        {
            // Create an instance of OpenFileDialog
            Microsoft.Win32.OpenFileDialog openFileDialog = new Microsoft.Win32.OpenFileDialog();
            // Filter for CSV files
            openFileDialog.Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*";
            // Show the dialog and check if the user selected a file
            if (openFileDialog.ShowDialog() == true)
            {
                // Get the selected file path
                string selectedFilePath = openFileDialog.FileName;
                // Update the TextBlock with the selected file path
                textBlockSelectedFilePath.Text = selectedFilePath;
                textBlockSelectedFilePath.ToolTip = selectedFilePath;
                DataParsers.LoadFile(selectedFilePath);
                columnsFromCSVLoad = DataParsers.LoadPasswordBankHeadersFromCsvData(DataParsers.LoadFile(selectedFilePath));
                Console.WriteLine("Loaded CSV no Error");
                publicSelectableColumns = columnsFromCSVLoad;

                setIndexNullCBoxes();
                switchItemsColNamesCBoxes();        
                resetIndexZeroCBoxes();
                
            }
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

        private void fillMappingDictLoadDataAndReturn()
        {
            MappingChoice.Add("LoginDescription", comboBoxLoginDescription.SelectedItem.ToString());
            MappingChoice.Add("LoginType", comboBoxLoginType.SelectedItem.ToString());
            MappingChoice.Add("LoginUrl", comboBoxLoginUrl.SelectedItem.ToString());
            MappingChoice.Add("LoginUsername", comboBoxLoginUsername.SelectedItem.ToString());
            MappingChoice.Add("LoginPass", comboBoxLoginPass.SelectedItem.ToString());
            MappingChoice.Add("HasTwoFa", comboBoxHasTwoFa.SelectedItem.ToString());
            MappingChoice.Add("TwoFaMethod", comboBoxTwoFaMethod.SelectedItem.ToString());

            //TODO Add in the logic to Load the data into a password bank and return with exitchoice = false

            //THis replaces btnConfirm_Click

        }
        private bool canFillMappingDict()
        {
            return false;
            //TODO - only true if all the combox selected items are columns from the user selection.
        }
        public RelayCommand fnUpdateItemInGrid_UpdateButton => new RelayCommand(execute => { fillMappingDictLoadDataAndReturn(); },
                                                        canExecute => { return canFillMappingDict(); });
        private void resetIndexZeroCBoxes()
        {
            if (comboBoxLoginDescription.IsLoaded)
            {
                comboBoxLoginDescription.SelectedIndex = 0;
                comboBoxLoginType.SelectedIndex = 0;
                comboBoxLoginUrl.SelectedIndex = 0;
                comboBoxLoginUsername.SelectedIndex = 0;
                comboBoxLoginPass.SelectedIndex = 0;
                comboBoxHasTwoFa.SelectedIndex = 0;
                comboBoxTwoFaMethod.SelectedIndex = 0;
            }
        }
        private void setIndexNullCBoxes()
        {
            comboBoxLoginDescription.SelectedItem = null;
            comboBoxLoginType.SelectedItem = null;
            comboBoxLoginUrl.SelectedItem = null;
            comboBoxLoginUsername.SelectedItem = null;
            comboBoxLoginPass.SelectedItem = null;
            comboBoxHasTwoFa.SelectedItem = null;
            comboBoxTwoFaMethod.SelectedItem = null;
        }

        private void switchItemsColNamesCBoxes()
        {
            comboBoxLoginDescription.ItemsSource = publicSelectableColumns;
            comboBoxLoginType.ItemsSource = publicSelectableColumns;
            comboBoxLoginUrl.ItemsSource = publicSelectableColumns;
            comboBoxLoginUsername.ItemsSource = publicSelectableColumns;
            comboBoxLoginPass.ItemsSource = publicSelectableColumns;
            comboBoxHasTwoFa.ItemsSource = publicSelectableColumns;
            comboBoxTwoFaMethod.ItemsSource = publicSelectableColumns;
        }

        private void comboBoxTwoFaMethod_Loaded(object sender, RoutedEventArgs e)
        {
            comboBoxTwoFaMethod.ItemsSource = openingStateColumns;
            comboBoxTwoFaMethod.SelectedIndex = 0;
            
        }

        private void comboBoxLoginDescription_Loaded(object sender, RoutedEventArgs e)
        {
            comboBoxLoginDescription.ItemsSource = openingStateColumns;
            comboBoxLoginDescription.SelectedIndex = 0;
            
        }

        private void comboBoxLoginType_Loaded(object sender, RoutedEventArgs e)
        {
            comboBoxLoginType.ItemsSource = openingStateColumns;
            comboBoxLoginType.SelectedIndex = 0;
            
        }

        private void comboBoxLoginUrl_Loaded(object sender, RoutedEventArgs e)
        {
            comboBoxLoginUrl.ItemsSource = openingStateColumns;
            comboBoxLoginUrl.SelectedIndex = 0;
            
        }

        private void comboBoxLoginUsername_Loaded(object sender, RoutedEventArgs e)
        {
            comboBoxLoginUsername.ItemsSource = openingStateColumns;
            comboBoxLoginUsername.SelectedIndex = 0;
            
        }

        private void comboBoxLoginPass_Loaded(object sender, RoutedEventArgs e)
        {
            comboBoxLoginPass.ItemsSource = openingStateColumns;
            comboBoxLoginPass.SelectedIndex = 0;
            
        }

        private void comboBoxHasTwoFa_Loaded(object sender, RoutedEventArgs e)
        {
            comboBoxHasTwoFa.ItemsSource = openingStateColumns;
            comboBoxHasTwoFa.SelectedIndex = 0;
            
        }



    }
}
