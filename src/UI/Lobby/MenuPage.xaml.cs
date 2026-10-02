using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Battelship.Lobby;
using Battleship.Logic.Services;

namespace Battleship.Lobby
{
    /// <summary>
    /// Interaktionslogik für StartMenu.xaml
    /// </summary>
    public partial class MenuPage : Page
    {
        public MenuPage()
        {
            InitializeComponent();
        }

        private void SingelPlayerButton_MouseLeftButtonDown(object sender, RoutedEventArgs e)
        {
            Navigation.NavigateTo(GameSettingsPage.Instance);
            Debug.WriteLine("DuosButton clicked");
        }

        private void DuosButton_MouseLeftButtonDown(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("DuosButton clicked");
            Navigation.NavigateTo(MultiplayerSetupPage.Instance);
        }

        private void OnlineButton_MouseLeftButtonDown(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("OnlineButton clicked");
            Navigation.NavigateTo(OnlinePage.Instance);
        }

        private void LoadGameButton_MouseLeftButtonDown(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("LoadGameButton clicked");
        }

        private void settingsButton_Click(object sender, RoutedEventArgs e) { }

        private void exitButton_Click(object sender, RoutedEventArgs e) { }
    }
}
