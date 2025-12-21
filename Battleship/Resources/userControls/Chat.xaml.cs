using Battleship.Resources.Components;
using Battleship.Resources.userControls.chatComponents;
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

namespace Battleship.Resources.userControls
{
    /// <summary>
    /// Interaktionslogik für Chat.xaml
    /// </summary>
    public partial class Chat : UserControl
    {
        public delegate void SendDelegate(string message);
        public event SendDelegate? OnSendButtonClick;

        private TextBox? inputField;

        private bool isEmpty = false;
        public bool IsEmpty { get { return isEmpty; } }

        private readonly Brush FocusBrush = (Brush)new BrushConverter().ConvertFrom("#eee");


        public Chat()
        {
            InitializeComponent();
        }

        // Placeholder DependencyProperty
        public static readonly DependencyProperty PlaceholderProperty =
            DependencyProperty.Register(
                "InputMessagePlaceholder",                  // The name of the property
                typeof(string),                             // The type of the property
                typeof(Chat),                              // The type of element, where the property is registered
                new PropertyMetadata("Add message here...") // Default value and Property-Metadata
            );

        // CLR-Wrapper für das DependencyProperty
        public string Placeholder
        {
            get => (string)GetValue(PlaceholderProperty);
            set => SetValue(PlaceholderProperty, value);
        }

        // Placeholder DependencyProperty
        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register(
                "InputMessage",             // The name of the property
                typeof(string),             // The type of the property
                typeof(Chat),              // The type of element, where the property is registered
                new PropertyMetadata("")    // Default value and Property-Metadata
            );

        // CLR-Wrapper für das DependencyProperty
        public string Text
        {
            get => (string)GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }

        public void AddMessage(string username, string message)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                UserMessage mesage = new UserMessage()
                {
                    Username = username,
                    Message = message
                };
                bool isScrollAtEnd = ScrollView.VerticalOffset >= ScrollView.ScrollableHeight - 5;
                MessagesContainer.Children.Add(mesage);
                if (isScrollAtEnd)
                    ScrollView.ScrollToEnd();
            });
        }

        public void AddSystemMessage(string message)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                SystemMessage mesage = new SystemMessage()
                {
                    Message = message
                };
                bool isScrollAtEnd = ScrollView.VerticalOffset >= ScrollView.ScrollableHeight - 5;
                MessagesContainer.Children.Add(mesage);
                if (isScrollAtEnd)
                    ScrollView.ScrollToEnd();
            });
        }

        private void CheckInputContent(TextBox textBox, bool gotFocus)
        {
            if (gotFocus && isEmpty)
            {
                isEmpty = false;
                textBox.Text = "";
                textBox.Foreground = FocusBrush;
            }
            else if (textBox.Text == "")
            {
                isEmpty = true;
                textBox.Text = (string)GetValue(PlaceholderProperty);
                textBox.Foreground = new SolidColorBrush(Colors.Gray);
            }
        }

        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            CheckInputContent((TextBox)sender, true);
        }

        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            CheckInputContent((TextBox)sender, false);
        }

        private void TextBox_Loaded(object sender, RoutedEventArgs e)
        {
            inputField = (TextBox)sender;
            CheckInputContent((TextBox)sender, false);
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (inputField == null)
                return;

            inputField.Text = "";
            CheckInputContent(inputField, false);
        }

        private void SendButton_Click(object sender, RoutedEventArgs e)
        {
            if (inputField == null)
                return;
            
            if (isEmpty)
                return;

            OnSendButtonClick?.Invoke(inputField.Text);

            inputField.Text = "";
            CheckInputContent(inputField, false);
        }

        private void Grid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            ClipRect.Rect = new Rect(0, 0, RootBorder.ActualWidth - 6, RootBorder.ActualHeight - 6);
        }

        private void TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (inputField == null)
                    return;

                if (isEmpty)
                    return;

                OnSendButtonClick?.Invoke(inputField.Text);

                inputField.Text = "";
            }
        }
    }
}
