using Battelship;
using Battelship.Lobby;
using Battleship.Lobby;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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

namespace Battleship
{
    /// <summary>
    /// Interaktionslogik für Login.xaml
    /// </summary>
    public partial class Login : Page
    {
        private static Login? instance;
        private Login()
        {
            InitializeComponent();
        }

        public static Login get()
        {
            if (instance == null)
            {
                instance = new Login();
            }
            return instance;
        }

        private void usernamePlaceholder_GotFocus(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("Got focus");
            usernamePlaceholder.Visibility = Visibility.Hidden;
            usernameInput.Focus();
        }

        private void usernameInput_LostFocus(object sender, RoutedEventArgs e)
        {
            if (usernameInput.Text == "")
            {
                usernamePlaceholder.Visibility = Visibility.Visible;
            }
        }

        private void applyButton_Click(object sender, RoutedEventArgs e)
        {
            Navigation.navigateTo(StartMenu.get());
            Debug.WriteLine("Button clicked");
        }
    }
}
