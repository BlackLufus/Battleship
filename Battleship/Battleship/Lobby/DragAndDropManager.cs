using Battleship.services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using static Battleship.Lobby.BattleField;

namespace Battleship.Lobby
{
    public class DragAndDropManager
    {
        enum DragEvent
        {
            DragEnter,
            DragLeave,
            DragDrop
        }

        private Canvas ShipsCanvas;
        private Grid DragAndDropGrid;
        private DragShip? dragShip;
        private BattleField battleField;

        public DragShip? DragShip { get; }

        public void SetDragShip(DragShip dragShip)
        {
            this.dragShip = dragShip;
        }

        public DragAndDropManager(Canvas ShipsCanvas, Grid DragAndDropGrid, int size)
        {
            this.ShipsCanvas = ShipsCanvas;
            this.DragAndDropGrid = DragAndDropGrid;
            this.battleField = new BattleField(size, true);
            KeyListener.Event().KeyDownEvent += KeyListener_KeyDownEvent;
        }

        private void KeyListener_KeyDownEvent(Key key)
        {
            Debug.WriteLine("Key pressed: " + key);
            if (dragShip != null)
            {
                battleField.RemoveLastShip();
                Debug.WriteLine("OffsetX: " + dragShip.offset.X);
                Debug.WriteLine("OffsetY: " + dragShip.offset.Y);
                Image image = (Image)dragShip.element;
                if (key == Key.Left || key == Key.Right)
                {
                    dragShip.Rotate(DragShip.ShipOrientation.Horizontal);
                    image.Width = 84;
                    image.Height = 28;
                }
                else if (key == Key.Up || key == Key.Down)
                {
                    dragShip.Rotate(DragShip.ShipOrientation.Vertical);
                    image.Width = 28;
                    image.Height = 84;
                }
                image.Source = RotateImage(new BitmapImage(new Uri("pack://application:,,,/Resources/Images/" + dragShip.shipType.ToString().ToLower() + ".png")), (int)dragShip.shipOrientation);
                Point mousePosition = dragShip.mousePosition;
                Debug.WriteLine("OffsetX: " + dragShip.offset.X);
                Debug.WriteLine("OffsetY: " + dragShip.offset.Y);
                Debug.WriteLine("Mouse position: " + mousePosition);
                SetDragPosition(dragShip.element, new Point(mousePosition.X - dragShip.offset.X, mousePosition.Y - dragShip.offset.Y));
            }
        }

        private BitmapSource RotateImage(BitmapSource source, double angle)
        {
            TransformedBitmap transform = new TransformedBitmap();
            transform.BeginInit();
            transform.Source = source;
            transform.Transform = new RotateTransform(angle);
            transform.EndInit();
            return transform;
        }

        public void HandleDragOverEvent(object sender, DragEventArgs e)
        {
            //Debug.WriteLine("Drag enter event");
            if (dragShip != null)
            {
                UpdateField(Grid.GetRow((UIElement)sender), Grid.GetColumn((UIElement)sender), DragEvent.DragEnter);
            }
            /*StackPanel stackPanel = (StackPanel)sender;
            stackPanel.Background = Brushes.Green;*/
        }

        public void HandleDragLeaveEvent(object sender, DragEventArgs e)
        {
            //Debug.WriteLine("Drag leave event");
            if (dragShip != null)
                UpdateField(Grid.GetRow((UIElement)sender), Grid.GetColumn((UIElement)sender), DragEvent.DragLeave);
            //StackPanel stackPanel = (StackPanel)sender;
            //stackPanel.Background = Brushes.Transparent;
        }

        public void HandleDropEvent(object sender, DragEventArgs e)
        {
            Debug.WriteLine("Drop event");
            if (dragShip != null)
                UpdateField(Grid.GetRow((UIElement)sender), Grid.GetColumn((UIElement)sender), DragEvent.DragDrop);
            if (dragShip != null)
            {
                Image image = (Image)dragShip.element;
                double left = Canvas.GetLeft(image);
                double top = Canvas.GetTop(image);
                Debug.WriteLine("Left: " + left);
                Debug.WriteLine("Top: " + top);
            }
            /*StackPanel stackPanel = (StackPanel)sender;
            stackPanel.Background = Brushes.Red;*/
        }

        public void EndDrag()         {
            if (dragShip != null)
            {
                this.ShipsCanvas.Children.Remove(dragShip.element);
                dragShip = null;
            }
        }

        public void StartDrag(Ship.ShipType shipType, Point offset)
        {
            Image element = new Image();
            element.Width = 28 * (int)shipType;
            element.Height = 28;
            element.Source = RotateImage(new BitmapImage(new Uri("pack://application:,,,/Resources/Images/" + shipType.ToString().ToLower() + ".png")), 270);
            this.ShipsCanvas.Children.Add(element);
            dragShip = new DragShip(element, offset, shipType);
            DragDrop.DoDragDrop(dragShip.element, dragShip, DragDropEffects.All);
        }

        private void SetDragPosition(UIElement element, Point position)
        {
            if (element == null)
            {
                return;
            }
            if (position.X < 0)
            {
                position.X = 0;
            }
            else if (position.X > 450)
            {
                position.X = 450;
            }
            if (position.Y < 0)
            {
                position.Y = 0;
            }
            else if (position.Y > 250)
            {
                position.Y = 250;
            }
            Canvas.SetLeft(dragShip.element, position.X);
            Canvas.SetTop(dragShip.element, position.Y);
        }

        public void MoveDragShip(Point mousePosition)
        {
            if (dragShip != null)
            {
                dragShip.SetMousePosition(mousePosition);
                SetDragPosition(dragShip.element, new Point(mousePosition.X - dragShip.offset.X, mousePosition.Y - dragShip.offset.Y));
            }
        }

        private void UpdateField(int row, int column, DragEvent dragEvent)
        {
            if (this.dragShip == null)
            {
                return;
            }
            if (dragEvent == DragEvent.DragEnter)
            {
                if (battleField.SetShip(
                this.dragShip.shipOrientation == DragShip.ShipOrientation.Vertical ? row -= (int)this.dragShip.offset.Y / 28 : row,
                this.dragShip.shipOrientation == DragShip.ShipOrientation.Horizontal ? column -= (int)this.dragShip.offset.X / 28 : column,
                this.dragShip.shipType,
                this.dragShip.shipOrientation,
                FieldState.Marked))
                {
                    return;
                }
                else
                {
                    Canvas.SetLeft(this.dragShip.element, 195 + column * 28);
                    Canvas.SetTop(this.dragShip.element, 0 + row * 28);
                }
            }
            else if (dragEvent == DragEvent.DragDrop)
            {
                if (!battleField.SetShip(
                this.dragShip.shipOrientation == DragShip.ShipOrientation.Vertical ? row -= (int)(this.dragShip.offset.Y / 28) : row,
                this.dragShip.shipOrientation == DragShip.ShipOrientation.Horizontal ? column -= (int)(this.dragShip.offset.X / 28) : column,
                this.dragShip.shipType,
                this.dragShip.shipOrientation,
                FieldState.Ship))
                {
                    return;
                }
                else
                {
                    Canvas.SetTop(dragShip.element, 0 + row * 28);
                    Canvas.SetLeft(dragShip.element, 198 + column * 28);
                    this.dragShip = null;
                }
            }
            else if (dragEvent == DragEvent.DragLeave)
            {
                if (!battleField.RemoveShip(
                    this.dragShip.shipOrientation == DragShip.ShipOrientation.Vertical ? row -= (int)this.dragShip.offset.Y / 28 : row,
                    this.dragShip.shipOrientation == DragShip.ShipOrientation.Horizontal ? column -= (int)this.dragShip.offset.X / 28 : column,
                    this.dragShip.shipType,
                    this.dragShip.shipOrientation,
                    FieldState.Marked))
                {
                    return;
                }
            }
            for (int x = 0; x < battleField.size; x++)
            {
                for (int y = 0; y < battleField.size; y++)
                {
                    StackPanel stackPanel = (StackPanel)DragAndDropGrid.Children.Cast<UIElement>().First(e => Grid.GetRow(e) == x && Grid.GetColumn(e) == y);
                    MarkField(stackPanel, (BattleField.FieldState)battleField.field[x, y]);
                }
            }

        }

        private void MarkField(StackPanel element, BattleField.FieldState fieldState)
        {
            if (fieldState == BattleField.FieldState.Marked)
            {
                //element.Background = Brushes.Green;
            }
            else if (fieldState == BattleField.FieldState.Water)
            {
                //element.Background = (Brush)new BrushConverter().ConvertFrom("#22000000");
                element.Children.Clear();
            }
            else if (fieldState == BattleField.FieldState.Ship)
            {
                //element.Background = Brushes.Red;
            }
            else
            {
                Image image = new Image();
                image.Source = new BitmapImage(new Uri("pack://application:,,,/Resources/Images/restricted.png"));
                element.Children.Add(image);
            }
        }
    }

    public class DragShip : Ship
    {
        public UIElement element;

        public Point offset;
        public Point mousePosition;

        public void SetMousePosition(Point mousePosition)
        {
            this.mousePosition = mousePosition;
        }

        public DragShip(UIElement element, Point offset, ShipType shipType, ShipOrientation shipOrientation = ShipOrientation.Horizontal) : base(shipType, shipOrientation)
        {
            this.element = element;
            this.offset = offset;
        }

        public void Rotate(ShipOrientation shipOrientation)
        {
            Debug.WriteLine("Rotate: " + this.shipOrientation + " = " + shipOrientation + "(" + (this.shipOrientation != shipOrientation) + ")");
            if (this.shipOrientation != shipOrientation)
            {
                Debug.WriteLine(shipOrientation);
                if (shipOrientation == ShipOrientation.Horizontal)
                {
                    Debug.WriteLine("Horizontal");
                    double temp = offset.X;
                    offset.X = offset.Y;
                    offset.Y = temp;
                    Debug.WriteLine("OffsetX: " + offset.X);
                }
                else if (shipOrientation == ShipOrientation.Vertical)
                {
                    Debug.WriteLine("Vertical");
                    double temp = offset.X;
                    offset.X = offset.Y;
                    offset.Y = temp;
                }

            }
            this.shipOrientation = shipOrientation;
        }
    }
}
