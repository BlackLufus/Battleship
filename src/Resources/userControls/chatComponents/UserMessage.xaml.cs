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

namespace Battleship.Resources.userControls.chatComponents
{
    /// <summary>
    /// Interaktionslogik für UserMessage.xaml
    /// </summary>
    public partial class UserMessage : UserControl
    {
        public UserMessage()
        {
            InitializeComponent();
        }

        // Username DependencyProperty
        public static readonly DependencyProperty UsernameProperty = DependencyProperty.Register(
            "Username", // The name of the property
            typeof(string), // The type of the property
            typeof(Chat), // The type of element, where the property is registered
            new PropertyMetadata("Username") // Default value and Property-Metadata
        );

        // CLR-Wrapper für das DependencyProperty
        public string Username
        {
            get => (string)GetValue(UsernameProperty);
            set => SetValue(UsernameProperty, value);
        }

        // Message DependencyProperty
        public static readonly DependencyProperty MessageProperty = DependencyProperty.Register(
            "Message", // The name of the property
            typeof(string), // The type of the property
            typeof(Chat), // The type of element, where the property is registered
            new PropertyMetadata("Message") // Default value and Property-Metadata
        );

        // CLR-Wrapper für das DependencyProperty
        public string Message
        {
            get => (string)GetValue(MessageProperty);
            set => SetValue(MessageProperty, value);
        }
    }
}
