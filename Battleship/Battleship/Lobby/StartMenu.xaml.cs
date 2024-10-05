using Battelship.Lobby;
using Battelship;
using System.Windows;
using System.Windows.Controls;

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

        private void singelPlayerButton_Click(object sender, RoutedEventArgs e)
        {
            Navigation.navigateTo(GameSettings.get());
        }

        private void multiPlayerButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void loadGameButton_Click(object sender, RoutedEventArgs e)
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
