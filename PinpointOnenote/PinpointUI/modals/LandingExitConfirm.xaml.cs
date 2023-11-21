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

namespace PinpointUI.modals
{
    /// <summary>
    /// Interaction logic for LandingExitConfirm.xaml
    /// </summary>
    public partial class LandingExitConfirm : Window
    {
        public bool ExitChoice { get; set; }
        public LandingExitConfirm(Window parentWindow)
        {
            Owner = parentWindow;
            InitializeComponent();
        }

        private void btnExit_Click(object sender, RoutedEventArgs e)
        {
            ExitChoice = true;
            Close();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            ExitChoice = false;
            Close();
        }
    }
}
