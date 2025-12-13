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

namespace Battleship.Resources.Components
{
    /// <summary>
    /// Interaktionslogik für Dialog.xaml
    /// </summary>
    public partial class Dialog : UserControl
    {
        public enum DialogType
        {
            Info,
            Warning,
            Error
        }

        public enum ButtonType
        {
            Ok = 0,
            OkCancel = 1,
            Apply = 2,
            ApplyCancel = 3,
            YesNo = 4,
            YesNoCancel = 5
        }

        public enum Result
        {
            Ok,
            Cancel,
            Apply,
            Yes,
            No
        }

        private static Grid? DialogContainer;
        private static Grid? DialogGrid;

        public static void Setup(Grid DialogContainer, Grid DialogGrid)
        {
            Dialog.DialogContainer = DialogContainer;
            Dialog.DialogGrid = DialogGrid;
        }

        private readonly Action<Result>? callback;
        public Dialog(Action<Result>? callback)
        {
            this.callback = callback;
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

        public void SetButtonOption(int buttonID, string buttonText, Result result)
        {
            Button button = buttonID switch
            {
                1 => ButtonOption1,
                2 => ButtonOption2,
                3 => ButtonOption3,
                _ => throw new ArgumentOutOfRangeException(nameof(buttonID))
            };
            button.Content = buttonText;
            button.Visibility = Visibility.Visible;
            button.Click += (sender, e) => {
                DialogContainer!.Visibility = Visibility.Hidden;
                DialogGrid!.Children.Clear();
                callback?.Invoke(result);
            };
        }

        public static void Show(DialogType type, ButtonType buttons, string header, string text, Action<Result>? callback = null)
        {
            if (DialogContainer == null || DialogGrid == null)
            {
                throw new Exception("DialogHandler not set up");
            }
            Debug.WriteLine("Showing dialog");
            Dialog dialog = new Dialog(callback);
            if (buttons == ButtonType.Ok || buttons == ButtonType.OkCancel)
            {
                dialog.SetButtonOption(1, "OK", Result.Ok);
            }
            if (buttons == ButtonType.Apply || buttons == ButtonType.ApplyCancel)
            {
                dialog.SetButtonOption(1, "Übernehmen", Result.Apply);
            }
            if (buttons == ButtonType.YesNo || buttons == ButtonType.YesNoCancel)
            {
                dialog.SetButtonOption(1, "Ja", Result.Yes);
                dialog.SetButtonOption(2, "Nein", Result.No);
            }
            if (buttons == ButtonType.OkCancel || buttons == ButtonType.ApplyCancel || buttons == ButtonType.YesNoCancel)
            {
                dialog.SetButtonOption(3, "Abbruch", Result.Cancel);
            }
            dialog.Header = header;
            dialog.Text = text;
            DialogContainer!.Visibility = Visibility.Visible;
            DialogGrid!.Children.Add(dialog);
        }
    }
}
