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

namespace Battelship.Lobby
{
    /// <summary>
    /// Interaktionslogik für GameSettings.xaml
    /// </summary>
    public partial class GameSettings : Page
    {
        private static GameSettings? instance;

        private GameSettings()
        {
            InitializeComponent();
        }

        public static GameSettings get()
        {
            if (instance == null)
            {
                instance = new GameSettings();
            }
            return instance;
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            Navigation.navigateBack();
        }

        private void ApplyButton_Click(object sender, RoutedEventArgs e)
        {
            Navigation.navigateTo(FleetManager.get());
        }
    }
}
