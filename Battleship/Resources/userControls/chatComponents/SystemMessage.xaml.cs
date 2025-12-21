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
    /// Interaktionslogik für SystemMessage.xaml
    /// </summary>
    public partial class SystemMessage : UserControl
    {
        public SystemMessage()
        {
            InitializeComponent();
        }

        // Message DependencyProperty
        public static readonly DependencyProperty MessageProperty =
            DependencyProperty.Register(
                "SystemMessageProperty",             // The name of the property
                typeof(string),             // The type of the property
                typeof(Chat),              // The type of element, where the property is registered
                new PropertyMetadata("Message")    // Default value and Property-Metadata
            );

        // CLR-Wrapper für das DependencyProperty
        public string Message
        {
            get => (string)GetValue(MessageProperty);
            set => SetValue(MessageProperty, value);
        }
    }
}
