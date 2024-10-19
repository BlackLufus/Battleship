using Battelship;
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

        private void PrivateGameButton_Click(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("PrivateGameButton clicked");
        }

        private void ConnectToPrivateGameButton_Click(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("ConnectToPrivateGameButton clicked");
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("BackButton clicked");
            Navigation.NavigateBack();
        }
    }
}
