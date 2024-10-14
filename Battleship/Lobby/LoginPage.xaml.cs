using Battelship;
using Battleship.Lobby;
using Battleship.Resources.Components;
using Battleship.Services;
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

namespace Battleship.Lobby
{
    /// <summary>
    /// Interaktionslogik für WelcomePage.xaml
    /// </summary>
    public partial class LoginPage : Page
    {
        public LoginPage()
        {
            InitializeComponent();
        }

        private void applyButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(UsernameInput.Text))
            {
                Dialog.Show(Dialog.DialogType.Warning, Dialog.ButtonType.Ok, "Benutzername wird benötigt!", "Bitte gib einen Benutzernamen ein.");
                return;
            }
            Navigation.NavigateTo(new MenuPage());
            Debug.WriteLine("Button clicked");
        }
    }
}
