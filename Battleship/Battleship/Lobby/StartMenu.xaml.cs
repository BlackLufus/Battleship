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
    public partial class StartMenu : Page
    {
        private static StartMenu? instance;

        private StartMenu()
        {
            InitializeComponent();
        }

        public static StartMenu get()
        {
            if (instance == null)
            {
                instance = new StartMenu();
            }
            return instance;
        }

        private void SingelPlayerButton_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Navigation.navigateTo(GameSettings.Get());
            Debug.WriteLine("DuosButton clicked");
        }

        private void DuosButton_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Debug.WriteLine("DuosButton clicked");
            Navigation.navigateTo(MultiplayerSetup.Get());
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
