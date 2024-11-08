using Battleship.Global;
using Battleship.Lobby;
using Battleship.Network;
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

namespace Battelship.Lobby
{
    /// <summary>
    /// Interaktionslogik für GameSettings.xaml
    /// </summary>
    public partial class GameSettingsPage : Page
    {
        private MQTTService? mqttService;
        private bool isWaiting = false;
        private HostSocketService? hostSocketService;
        private GameSetting.Mode mode;
        private static GameSettingsPage? instance;
        private List<(string, object)> sizeList = [
            ("5x5", 5),
            ("6x6", 6),
            ("7x7", 7),
            ("8x8", 8),
            ("9x9", 9),
            ("10x10", 10),
            ("11x11", 11),
            ("12x12", 12),
            ("13x13", 13),
            ("14x14", 14),
            ("15x15", 15),
            ("16x16", 16),
            ("17x17", 17),
            ("18x18", 18),
            ("19x19", 19),
            ("20x20", 20),
            ("21x21", 21),
            ("22x22", 22),
            ("23x23", 23),
            ("24x24", 24),
            ("25x25", 25),
            ("26x26", 26),
            ("27x27", 27),
            ("28x28", 28),
            ("29x29", 29),
            ("30x30", 30),
        ];

        public List<(string, object)> SizeList
        {
            get { return sizeList; }
            set { sizeList = value; }
        }
        private List<(string, object)> difficultList = [
            ("Sehr Leicht", GameSetting.Difficult.VeryEasy),
            ("Leicht", GameSetting.Difficult.Easy),
            ("Mittel", GameSetting.Difficult.Medium),
            ("Schwer", GameSetting.Difficult.Hard),
            ("Sehr schwer", GameSetting.Difficult.VeryHard),
            //("Unmöglich", GameSetting.Difficult.Impossible)
        ];

        public List<(string, object)> DifficultList
        {
            get { return difficultList; }
            set { difficultList = value; }
        }

        public static GameSettingsPage Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new GameSettingsPage();
                }
                return instance;
            }
        }

        public GameSettingsPage()
        {
            InitializeComponent();

            // Setze das DataContext, damit das Binding funktioniert
            this.DataContext = this;
        }

        public GameSettingsPage(MQTTService mqttService)
        {
            this.mqttService = mqttService;
            mqttService.OnClientConnected += () =>
            {
                Debug.WriteLine("Connected");
                if (isWaiting)
                {
                    isWaiting = false;
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        ApplyButton_Click(null, null);
                    });
                }
            };
            mqttService.OnDisconnected += () =>
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    Dialog.Show(Dialog.DialogType.Info, Dialog.ButtonType.Ok, "Disconnected", "The other player has disconnected");
                    Navigation.NavigateAndClear(new MenuPage());
                });
            };
            mode = GameSetting.Mode.PlayerVsPlayer;
            Debug.WriteLine("PlayerVsPlayer");

            InitializeComponent();

            // Setze das DataContext, damit das Binding funktioniert
            this.DataContext = this;
        }

        public GameSettingsPage(HostSocketService hostSocketService)
        {
            this.hostSocketService = hostSocketService;
            mode = GameSetting.Mode.PlayerVsPlayer;

            InitializeComponent();

            // Setze das DataContext, damit das Binding funktioniert
            this.DataContext = this;
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            Navigation.NavigateBack();
        }

        private void ApplyButton_Click(object sender, RoutedEventArgs e)
        {
            if (mode != GameSetting.Mode.PlayerVsPlayer)
            {
                foreach (RadioButton radio in ((StackPanel)GameModeGroup.Content).Children)
                {
                    if (radio.IsChecked == true)
                    {
                        mode = radio.Content.Equals("Normal") ? GameSetting.Mode.PlayerVsComputer : GameSetting.Mode.ComputerVsComputer;
                    }
                }
            }
            else if (mqttService != null)
            {
                if (mqttService.IsOpponentConnected)
                {
                    mqttService.SendGameSettings(
                        (int)FieldSize.SelectedValue,
                        FieldHitBonus.IsChecked,
                        FieldRestricedArea.IsChecked,
                        0,
                        BattleshipAmount.Value,
                        CruiserAmount.Value,
                        SubmarineAmount.Value,
                        DestroyerAmount.Value
                    );
                }
                else
                {
                    isWaiting = true;
                    Dialog.Show(Dialog.DialogType.Info, Dialog.ButtonType.Ok, "Warte auf Verbindung", "Warte auf Verbindung des Gegners");
                    return;
                }
            }
            else if (hostSocketService != null)
            {
                hostSocketService.Send(new LobbyServiceMessage(LobbyServiceMessage.MessageType.FieldSize, (string)FieldSize.SelectedValue));
            }
            Debug.WriteLine("SelectedValue: " + mode);
            Debug.WriteLine("SelectedValue: " + FieldSize.SelectedValue);
            Debug.WriteLine("SelectedValue: " + FieldDifficult.SelectedValue);
            Debug.WriteLine("SelectedValue: " + FieldHitBonus.IsChecked);
            Debug.WriteLine("SelectedValue: " + FieldRestricedArea.IsChecked);
            Debug.WriteLine("SelectedValue: " + BattleshipAmount.Value);
            Debug.WriteLine("SelectedValue: " + CruiserAmount.Value);
            Debug.WriteLine("SelectedValue: " + SubmarineAmount.Value);
            Debug.WriteLine("SelectedValue: " + DestroyerAmount.Value);
            Navigation.NavigateTo(new FleetManagerPage(
                new GameSetting(
                    mode,
                    (int)FieldSize.SelectedValue,
                    (GameSetting.Difficult)FieldDifficult.SelectedValue,
                    FieldHitBonus.IsChecked,
                    FieldRestricedArea.IsChecked,
                    BattleshipAmount.Value,
                    CruiserAmount.Value,
                    SubmarineAmount.Value,
                    DestroyerAmount.Value
                ),
                mqttService
            ));
        }
    }
}
