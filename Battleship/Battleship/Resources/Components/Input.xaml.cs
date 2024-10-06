using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
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

namespace Battleship.Resources.Components
{
    /// <summary>
    /// Interaktionslogik für Input.xaml
    /// </summary>
    public partial class Input : UserControl
    {
        private bool isEmpty = false;
        private TextBox? inputField;
        public Input()
        {
            InitializeComponent();
        }

        // Placeholder DependencyProperty
        public static readonly DependencyProperty PlaceholderProperty =
            DependencyProperty.Register(
                "Placeholder",  // Der Name des Propertys
                typeof(string),  // Der Typ des Propertys
                typeof(MenuButton),  // Der Typ, in dem das Property registriert wird
                new PropertyMetadata("User_0000")  // Standardwert und Property-Metadata
            );

        // CLR-Wrapper für das DependencyProperty
        public string Placeholder
        {
            get => (string)GetValue(PlaceholderProperty);
            set => SetValue(PlaceholderProperty, value);
        }

        private void Image_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {

        }

        private void InputField_GotFocus(object sender, RoutedEventArgs e)
        {
            CheckInputContent((TextBox)sender, true);
        }

        private void InputField_LostFocus(object sender, RoutedEventArgs e)
        {
            CheckInputContent((TextBox)sender, false);
        }

        private void InputField_Loaded(object sender, RoutedEventArgs e)
        {
            inputField = (TextBox)sender;
            CheckInputContent((TextBox)sender, false);
        }

        private void CheckInputContent(TextBox textBox, bool gotFocus)
        {
            if (gotFocus && isEmpty)
            {
                isEmpty = false;
                textBox.Text = "";
                textBox.Foreground = new SolidColorBrush(Colors.Black);
            }
            else if (textBox.Text == "")
            {
                isEmpty = true;
                textBox.Text = (string)GetValue(PlaceholderProperty);
                textBox.Foreground = new SolidColorBrush(Colors.Gray);
            }
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            inputField!.Text = "";
            CheckInputContent(inputField, false);
        }
    }
}
