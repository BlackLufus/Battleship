using Battleship.Lobby;
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

namespace Battelship.Lobby
{
    /// <summary>
    /// Interaktionslogik für GameSettings.xaml
    /// </summary>
    public partial class GameSettingsPage : Page
    {
        ExchangeHandler? exchangeHandler;

        private GameSetting.Mode mode;
        private static GameSettingsPage? instance;

        private List<(string, object)> sizeList = Enumerable.Range(5, 26).Select(i => ($"{i}x{i}", (object)i)).ToList();

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
            ("Sehr schwer", GameSetting.Difficult.VeryHard)
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
                    instance = new GameSettingsPage();
                return instance;
            }
        }

        public GameSettingsPage()
        {
            InitializeComponent();

            // Setze das DataContext, damit das Binding funktioniert
            this.DataContext = this;
        }

        public GameSettingsPage(ExchangeHandler exchangeHandler)
        {
            this.exchangeHandler = exchangeHandler;
            this.exchangeHandler.OnTimeout += HandleTimeout;
            this.exchangeHandler.OnDisconnect += HandleDisconnect;
            this.exchangeHandler.OnSettingsAck += HandleSettingsAck;

            // Set game mode to PlayerVsPlayer
            mode = GameSetting.Mode.PlayerVsPlayer;

            InitializeComponent();

            this.DataContext = this;
        }

        private void HandleTimeout()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                // Dispose exchange handler
                exchangeHandler?.Close();

                // Show dialog to inform user
                Dialog.Show(Dialog.DialogType.Info, Dialog.ButtonType.Ok, "Timeout", "Dein Gegner antwortet nicht mehr.");

                // Navigate back to previous page
                Navigation.NavigateBack();
            });
        }

        private void HandleDisconnect()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                // Dispose exchange handler
                exchangeHandler?.Close();

                // Show dialog to inform user
                Dialog.Show(Dialog.DialogType.Info, Dialog.ButtonType.Ok, "Spiel zu Ende", "Dein Gegner hat das Spiel verlassen");

                // Navigate back to previous page
                Navigation.NavigateBack();
            });
        }

        private void HandleSettingsAck()
        {
            // Return if exchange handler does not exist
            if (exchangeHandler == null)
                return;

            Application.Current.Dispatcher.Invoke(() =>
            {
                // Remove all handlers
                exchangeHandler.OnSettingsAck -= HandleSettingsAck;
                exchangeHandler.OnTimeout -= HandleTimeout;
                exchangeHandler.OnDisconnect -= HandleDisconnect;

                // Apply settings
                ApplySettings();
            });
        }

        private void ApplySettings()
        {
            Debug.WriteLine("SelectedValue: " + mode);
            Debug.WriteLine("SelectedValue: " + FieldSize.SelectedValue);
            Debug.WriteLine("SelectedValue: " + FieldDifficult.SelectedValue);
            Debug.WriteLine("SelectedValue: " + FieldHitBonus.IsChecked);
            Debug.WriteLine("SelectedValue: " + FieldRestricedArea.IsChecked);
            Debug.WriteLine("SelectedValue: " + BattleshipAmount.Value);
            Debug.WriteLine("SelectedValue: " + CruiserAmount.Value);
            Debug.WriteLine("SelectedValue: " + SubmarineAmount.Value);
            Debug.WriteLine("SelectedValue: " + DestroyerAmount.Value);

            // Navigate to Fleet Manager page
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
                exchangeHandler
            ));
        }

        private async void BackButton_Click(object sender, RoutedEventArgs e)
        {
            // Send Disconnect and dispose object
            if (exchangeHandler != null)
            {
                await exchangeHandler.SendDisconnect();
                await exchangeHandler.Close();
            }

            // Navigate back to previous page
            Navigation.NavigateBack();
        }

        private void ApplyButton_Click(object sender, RoutedEventArgs e)
        {
            // Multiplayer procedure
            if (mode == GameSetting.Mode.PlayerVsPlayer)
            {
                // Send Settings
                exchangeHandler?.SendSettings(
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
            // Singleplayer procedure
            else
            {
                foreach (RadioButton radio in ((StackPanel)GameModeGroup.Content).Children)
                {
                    if (radio.IsChecked == true)
                    {
                        mode = radio.Content.Equals("Normal") ? GameSetting.Mode.PlayerVsComputer : GameSetting.Mode.ComputerVsComputer;
                    }
                }
                // Apply settings
                ApplySettings();
            }
        }
    }
}
