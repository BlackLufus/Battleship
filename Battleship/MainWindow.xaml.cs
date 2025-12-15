using Battleship.Lobby;
using Battleship.Logic.Global;
using Battleship.Logic.Services;
using Battleship.Resources.Components;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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

            Navigation.RegisterFrame(MainFrame);
            Navigation.RegisterPage(new LobbyMainPage(true));
            //Navigation.RegisterPage(new PlaygroundPage());

            Dialog.Setup(DialogContainer, DialogGrid);

            // Just an example to show how to use it proper.
            //string plaintext = "This is some sensitive data!";
            //string key = "abcdefghijklmnop";

            //Debug.WriteLine(plaintext);
            //Debug.WriteLine($"key: {key} ({Regex.IsMatch(key, AdvancedEncryptionStandard.pattern)})");

            //var ciphertext = AdvancedEncryptionStandard.Encrypt(plaintext, key);

            //Debug.WriteLine(ciphertext);

            //var decrypted = AdvancedEncryptionStandard.Decrypt(ciphertext!, key);

            //Debug.WriteLine(decrypted);

            //key = "abcdefghijklmnoo";
            //var decrypted2 = AdvancedEncryptionStandard.Decrypt(ciphertext!, key);

            //Debug.WriteLine(decrypted2);
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
