using Battelship.Lobby;
using Battleship.Logic.Global;
using Battleship.Logic.Network;
using Battleship.Logic.Services;
using Battleship.Resources.Components;
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

namespace Battleship.Lobby
{
    /// <summary>
    /// Interaktionslogik für OnlinePage.xaml
    /// </summary>
    public partial class OnlinePage : Page
    {
        private ExchangeHandler? exchangeHandler;
        private static OnlinePage? instance;

        private bool isWaiting = false;

        public static OnlinePage Instance
        {
            get
            {
                instance ??= new OnlinePage();
                return instance;
            }
        }

        private OnlinePage()
        {
            InitializeComponent();
        }

        private void HandleTimeout()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                ToggleButtonState();
            });
        }

        private void HandleSettings(int size, bool hitBonus, bool restrictedArea, int carrierAmount, int battleshipAmount, int cruiserAmount, int submarineAmount, int destroyerAmount)
        {
            Debug.WriteLine("!!!HandleSettings!!!");
            if (exchangeHandler != null)
            {
                exchangeHandler.OnSettings -= HandleSettings;
                GameSetting gameSetting = new(GameSetting.Mode.PlayerVsPlayer, size, null, hitBonus, restrictedArea, battleshipAmount, cruiserAmount, submarineAmount, destroyerAmount, carrierAmount: carrierAmount);
                Application.Current.Dispatcher.Invoke(() =>
                {
                    ToggleButtonState();
                    Navigation.NavigateTo(new FleetManagerPage(gameSetting, exchangeHandler));
                });
            }
        }

        private void HandleConnect()
        {
            if (exchangeHandler != null)
            {
                exchangeHandler.OnConnect -= HandleConnect;
                Application.Current.Dispatcher.Invoke(() =>
                {
                    Navigation.NavigateTo(new GameSettingsPage(exchangeHandler));
                });
            }
        }

        private void HandleConnAck()
        {
            if (exchangeHandler != null)
            {
                exchangeHandler.OnConnectAck -= HandleConnAck;
                exchangeHandler.OnSettings += HandleSettings;
            }
        }

        private void HandleDisconnect()
        {
            if (exchangeHandler != null)
            {
                exchangeHandler.OnSettings -= HandleSettings;

                Application.Current.Dispatcher.Invoke(() =>
                {
                    Dialog.Show(Dialog.DialogType.Info, Dialog.ButtonType.Ok, "Disconnected", "The other player has disconnected");
                    ToggleButtonState();
                });
            }
        }


        private async void PrivateGameButton_Click(object sender, RoutedEventArgs e)
        {
            string gameId = GameIDInput.Text;
            string key = PasswordInput.Text;

            if (gameId.Trim().Length == 0)
                Dialog.Show(Dialog.DialogType.Error, Dialog.ButtonType.Ok, "Ungültige Game ID", "Die eingegebene ID ist nicht gültig!");

            // If success full create an Exchange handler object
            exchangeHandler = new ExchangeHandler(GameIDInput.Text, PasswordInput.Text, Variables.Username, true);
            await exchangeHandler.Connect();
            exchangeHandler.OnConnect += HandleConnect;
        }

        private async void ConnectToPrivateGameButton_Click(object sender, RoutedEventArgs e)
        {
            string gameId = GameIDInput.Text;
            string key = PasswordInput.Text;

            if (gameId.Trim().Length == 0)
                Dialog.Show(Dialog.DialogType.Error, Dialog.ButtonType.Ok, "Ungültige Game ID", "Die eingegebene ID ist nicht gültig!");

            exchangeHandler = new ExchangeHandler(GameIDInput.Text, PasswordInput.Text, Variables.Username, false);
            await exchangeHandler.Connect();
            exchangeHandler.OnTimeout += HandleTimeout;
            exchangeHandler.OnConnectAck += HandleConnAck;
            exchangeHandler.OnDisconnect += HandleDisconnect;
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            if (exchangeHandler != null)
                exchangeHandler.SendDisconnect();
            ToggleButtonState();
            Navigation.NavigateBack();
        }

        private void ToggleButtonState()
        {

            if (isWaiting)
            {
                ConnectButton.IsEnabled = false;
                PrivateGameButton.IsEnabled = false;
            }
            else
            {
                ConnectButton.IsEnabled = true;
                PrivateGameButton.IsEnabled = true;
            }
        }
    }
}
