using Battelship.Lobby;
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
