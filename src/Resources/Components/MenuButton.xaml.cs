using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using System.Windows.Threading;

namespace Battleship.Resources.Components
{
    /// <summary>
    /// Interaktionslogik für MenuButton.xaml
    /// </summary>
    public partial class MenuButton : UserControl
    {
        private int nextId = 0;
        private int id;
        ClickEvent clickEvent;
        public event RoutedEventHandler Click;

        public MenuButton()
        {
            InitializeComponent();
            this.id = nextId++;
            //clickEvent = new(this, () => OnClick(), MainComponent);
        }

        public static readonly DependencyProperty IsPressedProperty = DependencyProperty.Register(
            "IsPressed",
            typeof(bool),
            typeof(MenuButton),
            new PropertyMetadata(false)
        );

        public bool IsPressed
        {
            get
            {
                IsPressed = clickEvent.IsPressed;
                return (bool)GetValue(IsPressedProperty);
            }
            set { SetValue(IsPressedProperty, value); } // Setter implementieren, um die UI zu aktualisieren
        }

        public static readonly DependencyProperty TextProperty = DependencyProperty.Register(
            "MenuButtonText",
            typeof(string),
            typeof(MenuButton),
            new PropertyMetadata("")
        );

        public string Text
        {
            get
            {
                Debug.WriteLine("Get Text");
                return (string)GetValue(TextProperty);
            }
            set { SetValue(TextProperty, value); }
        }

        public static readonly DependencyProperty ImageSourceProperty = DependencyProperty.Register(
            "ImageSource",
            typeof(ImageSource),
            typeof(MenuButton),
            new PropertyMetadata(null)
        );

        public ImageSource ImageSource
        {
            get { return (ImageSource)GetValue(ImageSourceProperty); }
            set { SetValue(ImageSourceProperty, value); }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Click?.Invoke(this, e);
        }
    }
}
