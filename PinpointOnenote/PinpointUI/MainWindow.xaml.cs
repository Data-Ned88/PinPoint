using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using PinpointOnenote;
using PinpointUI.modals;
using PinpointUI.tabs;

namespace PinpointUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            
    }

        //Microsoft.Office.Interop.OneNote.Application app = OnenoteMethods.InstantiateOneNoteApp();
        //bool OnenoteOpen = OnenoteMethods.IsOnenoteOpen(app);
        //List<string> OpenNBNames = OnenoteMethods.GetAvailableNotebooks(app);

        private void BtnLandingTabExit_Click(object sender, RoutedEventArgs e)
        {
            //Application.Current.Shutdown();
            LandingExitConfirm exitConfirm = new LandingExitConfirm(this);
            Opacity = 0.6;
            exitConfirm.ShowDialog();
            Opacity = 1;
            if (exitConfirm.ExitChoice == true)
            {
                Application.Current.Shutdown();
            }

        }
        private void ValidateMoveToSetup([CallerMemberName] string ButtonName = null)
        {
            if (ButtonName == "LandingCreate_Click")
            {
                LandingWarning.Text = "OneNote is Closed.\nYou must open the OneNote Desktop app first before using PinPoint to create a password section.";
            }
            else
            {
                LandingWarning.Text = "OneNote is Closed.\nYou must open the OneNote Desktop app first before using PinPoint to edit passwords in a previously saved section.";
            }
            if (Process.GetProcessesByName("onenote").Any())
            {
                LandingTab.Visibility = Visibility.Collapsed;
                OneNoteTab.IsSelected = true;


                OneNoteManagementTab OneNoteManager = new OneNoteManagementTab(this, ButtonName);
                OneNoteTab.Content = OneNoteManager;



            }
            else
            {
                LandingWarning.Visibility = Visibility.Visible;
            }
        }
        private void LandingCreate_Click(object sender, RoutedEventArgs e)
        {

            ValidateMoveToSetup();
        }

        private void LandingLoad_Click(object sender, RoutedEventArgs e)
        {
            ValidateMoveToSetup();
        }
    }
}
