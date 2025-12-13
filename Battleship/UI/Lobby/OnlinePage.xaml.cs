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
        private MQTTService? mqttService;
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

        private void PrivateGameButton_Click(object sender, RoutedEventArgs e)
        {
            mqttService = new MQTTService(GameIDInput.Text, PasswordInput.Text, Variables.Username, true);
            Debug.WriteLine("PrivateGameButton clicked");
            Navigation.NavigateTo(new GameSettingsPage(mqttService));
        }

        private void ConnectToPrivateGameButton_Click(object sender, RoutedEventArgs e)
        {
            isWaiting = true;
            mqttService = new MQTTService(GameIDInput.Text, PasswordInput.Text, Variables.Username, false);
            mqttService.OnConnectionTimeout += () =>
            {
                if (isWaiting)
                {
                    MessageBox.Show("Timeout");
                    isWaiting = false;
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        ToggleButtonState();
                    });
                }
            };
            mqttService.OnConnectionSuccess += () =>
            {
                Application.Current.Dispatcher.Invoke(() =>
                    Dialog.Show(Dialog.DialogType.Info, Dialog.ButtonType.Ok, "Connected", "You have successfully connected to the game")
                );
            };
            mqttService.OnDisconnected += () =>
            {
                if (isWaiting)
                {
                    isWaiting = false;
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        Dialog.Show(Dialog.DialogType.Info, Dialog.ButtonType.Ok, "Disconnected", "The other player has disconnected");
                        ToggleButtonState();
                    });
                }
            };
            mqttService.OnGameSettings += (int size, bool hitBonus, bool restrictedArea, int carrierAmount, int battleshipAmount, int cruiserAmount, int submarineAmount, int destroyerAmount) =>
            {
                if (isWaiting)
                {
                    isWaiting = false;
                    GameSetting gameSetting = new(GameSetting.Mode.PlayerVsPlayer, size, null, hitBonus, restrictedArea, battleshipAmount, cruiserAmount, submarineAmount, destroyerAmount, carrierAmount: carrierAmount);
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        ToggleButtonState();
                        Navigation.NavigateTo(new FleetManagerPage(gameSetting, mqttService));
                    });
                }
            };
            Debug.WriteLine("ConnectToPrivateGameButton clicked");
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            isWaiting = false;
            ToggleButtonState();
            if (mqttService != null)
            {
                mqttService.Disconnected();
            }
            Debug.WriteLine("BackButton clicked");
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
