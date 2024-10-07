using Battelship;
using Battleship.Lobby;
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
    public partial class WelcomePage : Page
    {
        private static WelcomePage? instance;
        private WelcomePage()
        {
            InitializeComponent();
        }

        public static WelcomePage get()
        {
            if (instance == null)
            {
                instance = new WelcomePage();
            }
            return instance;
        }

        private void applyButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(UsernameInput.Text))
            {
                DialogHandler.Show(DialogHandler.DialogType.Warning, DialogHandler.ButtonType.Ok, "Benutzername wird benötigt!", "Bitte gib einen Benutzernamen ein.");
                return;
            }
            Navigation.NavigateTo(StartMenuPage.get());
            Debug.WriteLine("Button clicked");
        }
    }
}
