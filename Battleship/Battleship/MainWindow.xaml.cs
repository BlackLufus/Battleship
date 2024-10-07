using Battelship;
using Battleship.Lobby;
using Battleship.Resources.Components;
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
using System.Windows.Shapes;

namespace Battleship
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            Navigation.Setup(mainFrame);
            Navigation.NavigateAndClear(WelcomePage.get());

            DialogHandler.Setup(DialogContainer, DialogGrid);
        }

        private void Dialog_DialogCallback(DialogHandler.Result result)
        {
            Debug.WriteLine(result);
        }

        private void Settings_Click(object sender, RoutedEventArgs e)
        {
            Navigation.NavigateTo(GeneralSettingsPage.get());
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
            ThreadListener.StopAllThreads();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Exit_Click(null, null);
        }
    }
}
