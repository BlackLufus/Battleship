using Battelship;
using Battelship.Lobby;
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
    /// Interaktionslogik für MultiplayerSetup.xaml
    /// </summary>
    public partial class MultiplayerSetup : Page
    {
        private static MultiplayerSetup instance;
        private MultiplayerSetup()
        {
            InitializeComponent();
        }

        public static MultiplayerSetup Get()
        {
            if (instance == null)
            {
                instance = new MultiplayerSetup();
            }
            return instance;
        }

        private void JoinButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void HostButton_Click(object sender, RoutedEventArgs e)
        {
            Navigation.navigateTo(GameSettings.Get());
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            Navigation.navigateBack();
        }
    }
}
