using Battelship;
using Battelship.Lobby;
using Battleship.Network;
using Battleship.Resources.Components;
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
    public partial class MultiplayerSetupPage : Page
    {
        private static MultiplayerSetupPage? instance;

        public static MultiplayerSetupPage Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new MultiplayerSetupPage();
                }
                return instance;
            }
        }

        private MultiplayerSetupPage()
        {
            InitializeComponent();
        }

        private void JoinButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void HostButton_Click(object sender, RoutedEventArgs e)
        {
            if (AddressInput.Text.Equals(""))
            {
                Dialog.Show(Dialog.DialogType.Error, Dialog.ButtonType.Ok, "ID-Address wird benötigt!", "Bitte geben Sie die ID-Adresse ein, um ein Spiel zu hosten.");
            }
            else if (PortInput.Text.Equals(""))
            {
                Dialog.Show(Dialog.DialogType.Error, Dialog.ButtonType.Ok, "Port wird benötigt!", "Bitte geben Sie den Port ein, um ein Spiel zu hosten.");
            }
            else
            {
                HostSocketService hostSocketService = new HostSocketService(AddressInput.Text, int.Parse(PortInput.Text));
                hostSocketService.Start();
                Navigation.NavigateTo(new GameSettingsPage());
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            Navigation.NavigateBack();
        }
    }
}
