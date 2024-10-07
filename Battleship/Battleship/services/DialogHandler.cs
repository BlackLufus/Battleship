using Battleship.Resources.Components;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Battleship.Services
{
    public class DialogHandler
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

        public static void Setup(Grid Dialog, Grid DialogGrid)
        {
            DialogHandler.DialogContainer = Dialog;
            DialogHandler.DialogGrid = DialogGrid;
        }

        public delegate void DialogCallbackHandler(Result result);
        public event DialogCallbackHandler? DialogCallback;
        private ButtonType buttonType;

        public static DialogHandler Show(DialogType type, ButtonType buttons, string header, string text)
        {
            if (DialogContainer == null || DialogGrid == null)
            {
                throw new Exception("DialogHandler not set up");
            }
            Debug.WriteLine("Showing dialog");
            DialogHandler dialogHandler = new DialogHandler();
            Dialog dialog = new Dialog();
            if (buttons == ButtonType.Ok || buttons == ButtonType.OkCancel)
            {
                dialog.SetButtonOption(1, "OK", dialogHandler.ButtonClicked, Result.Ok);
            }
            if (buttons == ButtonType.Apply || buttons == ButtonType.ApplyCancel)
            {
                dialog.SetButtonOption(1, "Übernehmen", dialogHandler.ButtonClicked, Result.Apply);
            }
            if (buttons == ButtonType.YesNo || buttons == ButtonType.YesNoCancel)
            {
                dialog.SetButtonOption(1, "Ja", dialogHandler.ButtonClicked, Result.Yes);
                dialog.SetButtonOption(2, "Nein", dialogHandler.ButtonClicked, Result.No);
            }
            if (buttons == ButtonType.OkCancel || buttons == ButtonType.ApplyCancel || buttons == ButtonType.YesNoCancel)
            {
                dialog.SetButtonOption(3, "Abbruch", dialogHandler.ButtonClicked, Result.Cancel);
            }
            dialog.Header = header;
            dialog.Text = text;
            DialogContainer!.Visibility = Visibility.Visible;
            DialogGrid!.Children.Add(dialog);
            return dialogHandler;
        }

        public void ButtonClicked(Result type)
        {
            DialogContainer!.Visibility = Visibility.Hidden;
            DialogGrid!.Children.Clear();
            DialogCallback?.Invoke(type);
        }
    }
}
