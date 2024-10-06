using Battelship;
using Battelship.Lobby;
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

namespace Battleship
{
    /// <summary>
    /// Interaktionslogik für Login.xaml
    /// </summary>
    public partial class Login : Page
    {
        private static Login? instance;
        HostSocketService hostSocket;
        ClientSocketService clientSocket;
        private Login()
        {
            InitializeComponent();
        }

        public static Login get()
        {
            if (instance == null)
            {
                instance = new Login();
            }
            return instance;
        }

        private void usernamePlaceholder_GotFocus(object sender, RoutedEventArgs e)
        {
            /*Debug.WriteLine("Got focus");
            UsernamePlaceholder.Visibility = Visibility.Hidden;
            UsernameInput.Focus();*/
        }

        private void usernameInput_LostFocus(object sender, RoutedEventArgs e)
        {
            /*if (UsernameInput.Text == "")
            {
                UsernamePlaceholder.Visibility = Visibility.Visible;
            }*/
        }

        private void applyButton_Click(object sender, RoutedEventArgs e)
        {
            Navigation.navigateTo(StartMenu.get());
            Debug.WriteLine("Button clicked");
        }

        private void startButton_Click(object sender, RoutedEventArgs e)
        {
            hostSocket = new HostSocketService("192.168.178.33", 12345);
            hostSocket.Start();

            clientSocket = new ClientSocketService("192.168.178.33", 12345);
            clientSocket.Connect();
        }

            private void hostButton_Click(object sender, RoutedEventArgs e)
        {
            hostSocket.Disconnect();
        }

        private void clientButton_Click(object sender, RoutedEventArgs e)
        {
            clientSocket.Send(new LobbyServiceMessage(LobbyServiceMessage.MessageType.Name, "client"));
        }
    }
}
