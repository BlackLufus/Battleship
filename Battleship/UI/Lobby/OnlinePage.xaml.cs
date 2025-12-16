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

        private void HandleSettings(int size, bool hitBonus, bool restrictedArea, int carrierAmount, int battleshipAmount, int cruiserAmount, int submarineAmount, int destroyerAmount)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                // Remove handler for settings
                exchangeHandler!.OnSettings -= HandleSettings;
                // Create game settings
                GameSetting gameSetting = new(GameSetting.Mode.PlayerVsPlayer, size, null, hitBonus, restrictedArea, battleshipAmount, cruiserAmount, submarineAmount, destroyerAmount, carrierAmount: carrierAmount);
                // Enable Settings
                ToggleButtonState(true);
                // Navigate to Fleet Manager page
                Navigation.NavigateTo(new FleetManagerPage(gameSetting, exchangeHandler!));
            });
        }

        private void HandleConnect()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                // Enable Buttons
                ToggleButtonState(true);

                // Remove handler for Connect
                exchangeHandler!.OnConnect -= HandleConnect;

                // Navigate to Game Settings page
                Navigation.NavigateTo(new GameSettingsPage(exchangeHandler!));
            });
        }

        private void HandleConnAck()
        {
            // Remove handler for ConnAck
            exchangeHandler!.OnConnectAck -= HandleConnAck;

            // Add handler for Setting, Timeout and Disconnect
            exchangeHandler!.OnSettings += HandleSettings;
            exchangeHandler.OnTimeout += HandleTimeout;
            exchangeHandler.OnDisconnect += HandleDisconnect;
        }

        private void HandleDisconnect()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                // Close exchange handler
                exchangeHandler?.Close();

                // Enable buttons
                ToggleButtonState(true);

                // Show dialog to inform user
                Dialog.Show(Dialog.DialogType.Info, Dialog.ButtonType.Ok, "Disconnected", "Der Gegner hat das Spiel verlassen");
            });
        }

        private void HandleTimeout()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                // Close exchange handler
                exchangeHandler?.Close();

                // Enable buttons
                ToggleButtonState(true);

                // Show dialog to inform user
                Dialog.Show(Dialog.DialogType.Info, Dialog.ButtonType.Ok, "Timeout", "Der Gegner antwortet nicht mehr.");
            });
        }

        private async void PrivateGameButton_Click(object sender, RoutedEventArgs e)
        {
            // Get gameId and key
            string gameId = GameIDInput.Text;
            string key = PasswordInput.Text;

            // Check if input is correct
            if (gameId.Trim().Length == 0)
                Dialog.Show(Dialog.DialogType.Error, Dialog.ButtonType.Ok, "Ungültige Game ID", "Die eingegebene ID ist nicht gültig!");

            // Disable buttons
            ToggleButtonState(false);

            // Create Exchange Handler object, add Connect handler and connect
            exchangeHandler = new ExchangeHandler(GameIDInput.Text, PasswordInput.Text, Variables.Username, true);
            exchangeHandler.OnConnect += HandleConnect;
            await exchangeHandler.Connect();
        }

        private async void ConnectToPrivateGameButton_Click(object sender, RoutedEventArgs e)
        {
            // Get gameId and key
            string gameId = GameIDInput.Text;
            string key = PasswordInput.Text;

            // Check if input is correct
            if (gameId.Trim().Length == 0)
                Dialog.Show(Dialog.DialogType.Error, Dialog.ButtonType.Ok, "Ungültige Game ID", "Die eingegebene ID ist nicht gültig!");

            // Disable buttons
            ToggleButtonState(false);

            // Create Exchange Handler object, add ConnAck handler and connect
            exchangeHandler = new ExchangeHandler(GameIDInput.Text, PasswordInput.Text, Variables.Username, false);
            exchangeHandler.OnConnectAck += HandleConnAck;
            await exchangeHandler.Connect();
        }

        private async void BackButton_Click(object sender, RoutedEventArgs e)
        {
            // Send Disconnect and dispose object
            if (exchangeHandler != null)
            {
                await exchangeHandler.SendDisconnect();
                await exchangeHandler.Close();
            }
            
            // Enable buttons
            ToggleButtonState(true);

            // Navigate back to previous page
            Navigation.NavigateBack();
        }

        private void ToggleButtonState(bool enable)
        {
            // Enable Connect and Private Game button
            ConnectButton.IsEnabled = enable;
            PrivateGameButton.IsEnabled = enable;
        }
    }
}
