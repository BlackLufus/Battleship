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

namespace Battleship.Resources.Components
{
    /// <summary>
    /// Interaktionslogik für AcceptButton.xaml
    /// </summary>
    public partial class AcceptButton : UserControl
    {
        public AcceptButton()
        {
            InitializeComponent();
        }

        // Definieren eines benutzerdefinierten Ereignisses
        public static readonly RoutedEvent ClickEvent = EventManager.RegisterRoutedEvent(
            "Click",
            RoutingStrategy.Bubble,
            typeof(RoutedEventHandler),
            typeof(AcceptButton)
        );

        public event RoutedEventHandler Click
        {
            add { AddHandler(ClickEvent, value); }
            remove { RemoveHandler(ClickEvent, value); }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            // Hier können Sie die gewünschte Logik einfügen
            RaiseEvent(new RoutedEventArgs(ClickEvent));
        }
    }
}
