using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
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
using Battelship.UI.Lobby;
using Battleship.Core.Global;
using Battleship.Core.Network;
using Battleship.UI.Navi;
using Battleship.UI.Resources.Components;

namespace Battleship.UI.Lobby
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

        private void HandleSettings(
            int size,
            bool hitBonus,
            bool restrictedArea,
            int carrierAmount,
            int battleshipAmount,
            int cruiserAmount,
            int submarineAmount,
            int destroyerAmount
        )
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                // Remove handler for settings
                exchangeHandler!.OnSettings -= HandleSettings;
                // Create game settings
                GameSetting gameSetting = new(
                    GameSetting.Mode.PlayerVsPlayer,
                    size,
                    null,
                    hitBonus,
                    restrictedArea,
                    battleshipAmount,
                    cruiserAmount,
                    submarineAmount,
                    destroyerAmount,
                    carrierAmount: carrierAmount
                );
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
                Dialog.Show(
                    Dialog.DialogType.Info,
                    Dialog.ButtonType.Ok,
                    "Disconnected",
                    "Der Gegner hat das Spiel verlassen"
                );
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
                Dialog.Show(
                    Dialog.DialogType.Info,
                    Dialog.ButtonType.Ok,
                    "Timeout",
                    "Der Gegner antwortet nicht mehr."
                );
            });
        }

        private void ToggleButtonState(bool enable)
        {
            // Enable Connect and Private Game button
            ConnectButton.IsEnabled = enable;
            PrivateGameButton.IsEnabled = enable;
        }

        private void OnlineModusClick(object sender, RoutedEventArgs e)
        {
            if (PublicRadioButton == null)
                return;

            if (PublicRadioButton.IsChecked == true)
            {
                PublicSettings.Visibility = Visibility.Visible;
                PrivateSettings.Visibility = Visibility.Hidden;
            }
            else
            {
                PublicSettings.Visibility = Visibility.Hidden;
                PrivateSettings.Visibility = Visibility.Visible;
            }
        }

        private bool IsGameIdValid(string input, out string gameId)
        {
            Match match = Regex.Match(input, @"^[a-zA-Z0-9_-]{1,16}$");

            if (!match.Success)
                Dialog.Show(
                    Dialog.DialogType.Error,
                    Dialog.ButtonType.Ok,
                    "Ungültige Spiel-ID",
                    "Die angegebene Spiel-ID ist ungültig!\nEs sind folgenden Zweichen erlaubt: ^[a-zA-Z0-9_-]{1,16}$"
                );

            gameId = input;

            return match.Success;
        }

        private bool IsPasswordValid(string input, out string password)
        {
            Match match = Regex.Match(input, @"^[a-zA-Z0-9]{16}$");

            if (!match.Success)
                Dialog.Show(
                    Dialog.DialogType.Error,
                    Dialog.ButtonType.Ok,
                    "Ungültiges Passwort",
                    "Das angegebene Passwort muss exakt 16 Zeichen enthalten!\nEs sind folgenden Zweichen erlaubt: ^[a-zA-Z0-9]{16,16}$"
                );

            password = input;

            return match.Success;
        }

        private bool IsIpAddressValid(string input, out IPAddress? ipAddress)
        {
            Match match = Regex.Match(input, @"\b\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}\b");

            if (!match.Success)
                Dialog.Show(
                    Dialog.DialogType.Error,
                    Dialog.ButtonType.Ok,
                    "Ungültige IP Adresse",
                    "Die angegebene IP Adresse ist ungültig!\nEs sind folgenden Zweichen erlaubt: \\b\\d{1,3}\\.\\d{1,3}\\.\\d{1,3}\\.\\d{1,3}\\b"
                );

            ipAddress = match.Success ? IPAddress.Parse(input) : null;

            return match.Success;
        }

        private bool IsPortValid(string input, out int port)
        {
            bool Success = int.TryParse(input, out port) && port >= 1024 && port <= 65535;

            if (!Success)
                Dialog.Show(
                    Dialog.DialogType.Error,
                    Dialog.ButtonType.Ok,
                    "Ungültiger Port",
                    "Der angegebene Port muss zwischen 1024 und 65535 liegen!"
                );

            return Success;
        }

        private async void ConnectButtonClick(object sender, RoutedEventArgs e)
        {
            // Create Exchange Handler object, add Connect handler and connect
            if (PublicRadioButton.IsChecked == true)
            {
                if (!IsGameIdValid(GameId.Text, out string gameId))
                    return;

                if (!IsPasswordValid(Password.Text, out string password))
                    return;

                exchangeHandler = new ExchangeHandler(gameId, password, Variables.Username, false);
            }
            else
            {
                if (!IsIpAddressValid(IPAddressInput.Text, out IPAddress? ipAddress))
                    return;

                if (!IsPortValid(PortInput.Text, out int port))
                    return;

                exchangeHandler = new ExchangeHandler(ipAddress!, port, Variables.Username, false);
            }

            // Disable buttons
            ToggleButtonState(false);

            // Add ConnAck handler and connect
            exchangeHandler.OnConnectAck += HandleConnAck;
            await exchangeHandler.Connect();
        }

        private async void PrivateGameButton_Click(object sender, RoutedEventArgs e)
        {
            // Create Exchange Handler object, add Connect handler and connect
            if (PublicRadioButton.IsChecked == true)
            {
                if (!IsGameIdValid(GameId.Text, out string gameId))
                    return;

                if (!IsPasswordValid(Password.Text, out string password))
                    return;

                exchangeHandler = new ExchangeHandler(gameId, password, Variables.Username, true);
            }
            else
            {
                if (!IsIpAddressValid(IPAddressInput.Text, out IPAddress? ipAddress))
                    return;

                if (!IsPortValid(PortInput.Text, out int port))
                    return;

                exchangeHandler = new ExchangeHandler(ipAddress!, port, Variables.Username, true);
            }

            // Disable buttons
            ToggleButtonState(false);

            // Add ConnAck handler and connect
            exchangeHandler.OnConnect += HandleConnect;
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
    }
}
