using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Media.Imaging;
using Battleship.Lobby;
using System.Windows.Media;
using Battleship.Global;

namespace Battleship.Playground
{
    public class PlaygroundUI
    {
        public enum FieldState
        {
            Restrict = -2,
            Miss = -1,
            Water = 0,
            Ship = 1,
            Hit = 2,
            Sunk = 3
        }

        private static void MarkField(GameSetting gameSetting, Button element, FieldState state)
        {
            if (state == FieldState.Hit || state == FieldState.Sunk)
            {
                Image image = new()
                {
                    Source = new BitmapImage(new Uri("pack://application:,,,/Resources/Images/fire.png")),
                    Width = gameSetting.CellSize - 2,
                    Height = gameSetting.CellSize - 2,
                };
                element.Content = image;
            }
            else if (state == FieldState.Miss)
            {
                Image image = new()
                {
                    Source = new BitmapImage(new Uri("pack://application:,,,/Resources/Images/red-cross.png")),
                    Width = gameSetting.CellSize - 2,
                    Height = gameSetting.CellSize - 2,
                };
                element.Content = image;
            }
            else if (state == FieldState.Restrict && gameSetting.RestrictedArea)
            {
                Image image = new()
                {
                    Source = new BitmapImage(new Uri("pack://application:,,,/Resources/Images/restriction.png")),
                    Width = gameSetting.CellSize - 2,
                    Height = gameSetting.CellSize - 2,
                };
                element.Content = image;
            }
        }

        public static void UpdatePlayground(GameSetting gameSetting, Grid playgroundGrid, Playground playground)
        {
            for (int row = 0; row < gameSetting.BoardSize; row++)
            {
                for (int col = 0; col < gameSetting.BoardSize; col++)
                {
                    Button button = (Button)playgroundGrid.Children.Cast<UIElement>().First(e => Grid.GetRow(e) == row && Grid.GetColumn(e) == col);
                    MarkField(gameSetting, button, (FieldState)playground.Field[row, col]);
                }
            }
        }
    }
}
