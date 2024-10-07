using Battleship.Services;
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
    /// Interaktionslogik für Dialog.xaml
    /// </summary>
    public partial class Dialog : UserControl
    {
        public Dialog()
        {
            InitializeComponent();
        }

        // Dependency Property for Header
        public static readonly DependencyProperty HeaderProperty =
            DependencyProperty.Register("Header", typeof(string), typeof(Dialog), new PropertyMetadata(string.Empty));

        public string Header
        {
            get { return (string)GetValue(HeaderProperty); }
            set { SetValue(HeaderProperty, value); }
        }

        // Dependency Property for Text
        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof(string), typeof(Dialog), new PropertyMetadata(string.Empty));

        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        public void SetButtonOption(int button, string buttonText, Action<DialogHandler.Result> action, DialogHandler.Result result)
        {
            Button button1 = button switch
            {
                1 => ButtonOption1,
                2 => ButtonOption2,
                3 => ButtonOption3,
                _ => throw new ArgumentOutOfRangeException(nameof(button))
            };
            button1.Content = buttonText;
            button1.Visibility = Visibility.Visible;
            button1.Click += (sender, e) => action(result);
        }
    }
}
