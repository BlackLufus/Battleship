using Battelship.Lobby;
using Battelship;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

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
            Navigation.navigateTo(GameSettings.get());
        }

        private void DuosButton_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {

        }

        private void OnlineButton_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {

        }

        private void LoadGameButton_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {

        }

        private void settingsButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void exitButton_Click(object sender, RoutedEventArgs e)
        {

        }

    }
}
