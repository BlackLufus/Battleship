using Battelship;
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

namespace Battleship.Lobby
{
    /// <summary>
    /// Interaktionslogik für GeneralSettings.xaml
    /// </summary>
    public partial class GeneralSettingsPage : Page
    {
        private static GeneralSettingsPage? instance;
        private GeneralSettingsPage()
        {
            InitializeComponent();
        }

        public static GeneralSettingsPage get()
        {
            if (instance == null)
            {
                instance = new GeneralSettingsPage();
            }
            return instance;
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            Navigation.NavigateBack();
        }
    }
}
