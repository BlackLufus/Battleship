using Battelship.Lobby;
using Battelship;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Diagnostics;

namespace Battleship.Lobby
{
    /// <summary>
    /// Interaktionslogik für StartMenu.xaml
    /// </summary>
    public partial class StartMenuPage : Page
    {
        private static StartMenuPage? instance;

        private StartMenuPage()
        {
            InitializeComponent();
        }

        public static StartMenuPage get()
        {
            if (instance == null)
            {
                instance = new StartMenuPage();
            }
            return instance;
        }

        private void SingelPlayerButton_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Navigation.NavigateTo(GameSettingsPage.Get());
            Debug.WriteLine("DuosButton clicked");
        }

        private void DuosButton_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Debug.WriteLine("DuosButton clicked");
            Navigation.NavigateTo(MultiplayerSetupPage.Get());
        }

        private void OnlineButton_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Debug.WriteLine("OnlineButton clicked");
        }

        private void LoadGameButton_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Debug.WriteLine("LoadGameButton clicked");
        }

        private void settingsButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void exitButton_Click(object sender, RoutedEventArgs e)
        {

        }

    }
}
