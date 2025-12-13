using Battleship.Logic.Services;
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
    /// Interaktionslogik für LobbyFrame.xaml
    /// </summary>
    public partial class LobbyMainPage : Page
    {
        public LobbyMainPage(bool setup = false)
        {
            InitializeComponent();

            Navigation.Setup(LobbyMainFrame);
            Navigation.NavigateAndClear(setup ? new LoginPage() : new MenuPage());
        }

        private void Settings_Click(object sender, RoutedEventArgs e)
        {
            Navigation.NavigateTo(GeneralSettingsPage.get());
        }
    }
}
