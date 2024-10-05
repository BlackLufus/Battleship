using Battleship.services;
using System.Diagnostics;
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
            DragDrop,
            None
        }

        private List<DragShip> dragShips = [];
        private Canvas ShipsCanvas;
        private Grid DragAndDropGrid;
        private DragShip? currentDragShip;
        private BattleField battleField;

        public DragAndDropManager(Canvas ShipsCanvas, Grid DragAndDropGrid, int size)
        {
            this.ShipsCanvas = ShipsCanvas;
            this.DragAndDropGrid = DragAndDropGrid;
            this.battleField = new BattleField(size, true);
            KeyListener.Event().KeyDownEvent += KeyListener_KeyDownEvent;
        }

        private void KeyListener_KeyDownEvent(Key key)
        {
            if (this.currentDragShip != null)
            {
                battleField.RemoveLastShip();
                Image image = (Image)this.currentDragShip.element;
                if (key == Key.Left || key == Key.Right)
                {
                    this.currentDragShip.Rotate(DragShip.ShipOrientation.Horizontal);
                    image.Width = 28 * (int)this.currentDragShip.shipType;
                    image.Height = 28;
                    Debug.WriteLine(image.Width);
                }
                else if (key == Key.Up || key == Key.Down)
                {
                    this.currentDragShip.Rotate(DragShip.ShipOrientation.Vertical);
                    image.Width = 28;
                    image.Height = 28 * (int)this.currentDragShip.shipType;
                    Debug.WriteLine(image.Height);
                }
                image.Source = RotateImage(new BitmapImage(new Uri("pack://application:,,,/Resources/Images/" + this.currentDragShip.shipType.ToString().ToLower() + ".png")), (int)this.currentDragShip.shipOrientation);
                Point mousePosition = this.currentDragShip.mousePosition;
                SetDragPosition(this.currentDragShip.element, new Point(mousePosition.X - this.currentDragShip.offset.X, mousePosition.Y - this.currentDragShip.offset.Y));
            }
        }

        private static TransformedBitmap RotateImage(BitmapSource source, double angle)
        {
            TransformedBitmap transform = new();
            transform.BeginInit();
            transform.Source = source;
            transform.Transform = new RotateTransform(angle);
            transform.EndInit();
            return transform;
        }

        public void HandleDragOverEvent(object sender, DragEventArgs e)
        {
            DragShip dragShip = (DragShip)e.Data.GetData(typeof(DragShip));
            UpdateField(dragShip, Grid.GetRow((UIElement)sender), Grid.GetColumn((UIElement)sender), DragEvent.DragEnter);
        }

        public void HandleDragLeaveEvent(object sender, DragEventArgs e)
        {
            DragShip dragShip = (DragShip)e.Data.GetData(typeof(DragShip));
            UpdateField(dragShip, Grid.GetRow((UIElement)sender), Grid.GetColumn((UIElement)sender), DragEvent.DragLeave);
        }

        public void HandleDropEvent(object sender, DragEventArgs e)
        {
            DragShip dragShip = (DragShip)e.Data.GetData(typeof(DragShip));
            UpdateField(dragShip, Grid.GetRow((UIElement)sender), Grid.GetColumn((UIElement)sender), DragEvent.DragDrop);
        }

        public void StartDrag(Ship.ShipType shipType, Point offset)
        {
            Image element = new()
            {
                Width = 28 * (int)shipType,
                Height = 28,
                Source = RotateImage(new BitmapImage(new Uri("pack://application:,,,/Resources/Images/" + shipType.ToString().ToLower() + ".png")), 270)
            };

            this.ShipsCanvas.Children.Add(element);

            DragShip dragShip = new(element, offset, shipType);

            currentDragShip = dragShip;

            dragShip.element.QueryContinueDrag += Element_QueryContinueDrag;

            DragDrop.DoDragDrop(dragShip.element, dragShip, DragDropEffects.All);
        }

        public void RedragShip(object sender, MouseButtonEventArgs e)
        {
            int row = Grid.GetRow((UIElement)sender);
            int column = Grid.GetColumn((UIElement)sender);
            foreach (DragShip dragShip in dragShips)
            {
                if (BattleField.IsShipOnPosition(dragShip, row, column))
                {
                    this.currentDragShip = dragShip;
                    dragShip.offset = new Point(e.GetPosition(dragShip.element).X, e.GetPosition(dragShip.element).Y);
                    dragShip.element.QueryContinueDrag += Element_QueryContinueDrag;
                    battleField.RemoveShip(dragShip.row, dragShip.column, dragShip.shipType, dragShip.shipOrientation, FieldState.Ship);
                    Update();
                    DragDrop.DoDragDrop(dragShip.element, dragShip, DragDropEffects.All);
                    dragShips.Remove(dragShip);
                    break;
                }
            }
        }

        private void Element_QueryContinueDrag(object sender, QueryContinueDragEventArgs e)
        {
            // Wenn die linke Maustaste losgelassen wird und das Ziel nicht erreicht wird
            if (e.Action == DragAction.Cancel || (e.KeyStates & DragDropKeyStates.LeftMouseButton) == 0)
            {
                this.ShipsCanvas.Children.Remove((Image)sender);
                this.currentDragShip?.Dispose();
                this.currentDragShip = null;

                // Der Drag-Vorgang wurde außerhalb eines Ziehziels abgebrochen
                e.Action = DragAction.Cancel;
            }
        }

        public void RemoveShip(object sender, MouseButtonEventArgs e)
        {
            int row = Grid.GetRow((UIElement)sender);
            int column = Grid.GetColumn((UIElement)sender);
            foreach (DragShip dragShip in dragShips)
            {
                if (BattleField.IsShipOnPosition(dragShip, row, column))
                {
                    battleField.RemoveShip(dragShip.row, dragShip.column, dragShip.shipType, dragShip.shipOrientation, FieldState.Ship);
                    this.ShipsCanvas.Children.Remove(dragShip.element);
                    Update();
                    dragShips.Remove(dragShip);
                    dragShip.Dispose();
                    break;
                }
            }
        }

        public void Randomize()
        {
            Reset();
            List<Ship> shipList = [];
            for (int i = 0; i < 4; i++)
            {
                Ship.ShipType shipType = Ship.ShipType.Battleship;
                Ship ship = new(shipType, Ship.ShipOrientation.Horizontal);
                shipList.Add(ship);
            }
            for (int i = 0; i < 3; i++)
            {
                Ship.ShipType shipType = Ship.ShipType.Cruiser;
                Ship ship = new(shipType, Ship.ShipOrientation.Horizontal);
                shipList.Add(ship);
            }
            for (int i = 0; i < 2; i++)
            {
                Ship.ShipType shipType = Ship.ShipType.Submarine;
                Ship ship = new(shipType, Ship.ShipOrientation.Horizontal);
                shipList.Add(ship);
            }
            for (int i = 0; i < 1; i++)
            {
                Ship.ShipType shipType = Ship.ShipType.Destroyer;
                Ship ship = new(shipType, Ship.ShipOrientation.Horizontal);
                shipList.Add(ship);
            }

            battleField.Randomize(shipList);

            foreach (Ship ship in shipList)
            {
                Image element = new()
                {
                    Width = 28 * (ship.shipOrientation == Ship.ShipOrientation.Horizontal ? (int)ship.shipType : 1),
                    Height = 28 * (ship.shipOrientation == Ship.ShipOrientation.Vertical ? (int)ship.shipType : 1),
                    Source = RotateImage(new BitmapImage(new Uri("pack://application:,,,/Resources/Images/" + ship.shipType.ToString().ToLower() + ".png")), (int)ship.shipOrientation)
                };

                this.ShipsCanvas.Children.Add(element);

                DragShip dragShip = new(element, new Point(0, 0), ship.shipType, ship.shipOrientation)
                {
                    row = ship.row,
                    column = ship.column
                };

                this.dragShips.Add(dragShip);

                Canvas.SetLeft(dragShip.element, 195 + dragShip.column * 28);
                Canvas.SetTop(dragShip.element, 0 + dragShip.row * 28);

                Update();
            }
        }

        private static void SetDragPosition(UIElement element, Point position)
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
            Canvas.SetLeft(element, position.X);
            Canvas.SetTop(element, position.Y);
        }

        public void MoveDragShip(Point mousePosition)
        {
            if (currentDragShip != null)
            {
                currentDragShip.SetMousePosition(mousePosition);
                SetDragPosition(currentDragShip.element, new Point(mousePosition.X - currentDragShip.offset.X, mousePosition.Y - currentDragShip.offset.Y));
            }
        }

        private void UpdateField(DragShip dragShip, int row, int column, DragEvent? dragEvent)
        {
            if (dragEvent == DragEvent.DragEnter)
            {
                if (battleField.SetShip(
                dragShip.shipOrientation == DragShip.ShipOrientation.Vertical ? row -= (int)dragShip.offset.Y / 28 : row,
                dragShip.shipOrientation == DragShip.ShipOrientation.Horizontal ? column -= (int)dragShip.offset.X / 28 : column,
                dragShip.shipType,
                dragShip.shipOrientation,
                FieldState.Marked))
                {
                    return;
                }
                else
                {
                    Canvas.SetLeft(dragShip.element, 195 + column * 28);
                    Canvas.SetTop(dragShip.element, 0 + row * 28);
                }
            }
            else if (dragEvent == DragEvent.DragDrop)
            {
                if (!battleField.SetShip(
                dragShip.shipOrientation == DragShip.ShipOrientation.Vertical ? row -= (int)(dragShip.offset.Y / 28) : row,
                dragShip.shipOrientation == DragShip.ShipOrientation.Horizontal ? column -= (int)(dragShip.offset.X / 28) : column,
                dragShip.shipType,
                dragShip.shipOrientation,
                FieldState.Ship))
                {
                    return;
                }
                else
                {
                    Canvas.SetTop(dragShip.element, 0 + row * 28);
                    Canvas.SetLeft(dragShip.element, 198 + column * 28);
                    ShipsCanvas.Children.Add(dragShip.element);
                    dragShip.row = row;
                    dragShip.column = column;
                    dragShips.Add(dragShip);
                }
            }
            else if (dragEvent == DragEvent.DragLeave)
            {
                if (!battleField.RemoveShip(
                    dragShip.shipOrientation == DragShip.ShipOrientation.Vertical ? row -= (int)dragShip.offset.Y / 28 : row,
                    dragShip.shipOrientation == DragShip.ShipOrientation.Horizontal ? column -= (int)dragShip.offset.X / 28 : column,
                    dragShip.shipType,
                    dragShip.shipOrientation,
                    FieldState.Marked))
                {
                    return;
                }
            }
            Update();
        }

        private void Update()
        {
            for (int x = 0; x < battleField.size; x++)
            {
                for (int y = 0; y < battleField.size; y++)
                {
                    StackPanel stackPanel = (StackPanel)DragAndDropGrid.Children.Cast<UIElement>().First(e => Grid.GetRow(e) == x && Grid.GetColumn(e) == y);
                    MarkField(stackPanel, (BattleField.FieldState)battleField.field[x, y]);
                }
            }
        }

        private static void MarkField(StackPanel element, BattleField.FieldState fieldState)
        {
            if (fieldState == BattleField.FieldState.Marked)
            {
                //element.Background = Brushes.Green;
                element.Background = new BrushConverter().ConvertFrom("#44ff0000") as Brush;
                element.Children.Clear();
            }
            else if (fieldState == BattleField.FieldState.Water)
            {
                element.Background = new BrushConverter().ConvertFrom("#22000000") as Brush;
                element.Children.Clear();
            }
            else if (fieldState == BattleField.FieldState.Ship)
            {
                //element.Background = Brushes.Red;
                element.Background = new BrushConverter().ConvertFrom("#22000000") as Brush;
                element.Children.Clear();
            }
            else
            {
                Image image = new()
                {
                    Source = new BitmapImage(new Uri("pack://application:,,,/Resources/Images/restricted.png"))
                };
                element.Children.Add(image);
            }
        }

        public void Reset()
        {
            dragShips.ForEach(
                dragShip => {
                    this.ShipsCanvas.Children.Remove(dragShip.element);
                    dragShip.Dispose();
                });
            dragShips.Clear();
            battleField.Reset();
            Update();
        }
    }

    public class DragShip(UIElement element, Point offset, Ship.ShipType shipType, Ship.ShipOrientation shipOrientation = Ship.ShipOrientation.Horizontal) : Ship(shipType, shipOrientation)
    {
        public UIElement element = element;

        public Point offset = offset;
        public Point mousePosition;

        public void SetMousePosition(Point mousePosition)
        {
            this.mousePosition = mousePosition;
        }

        public void Rotate(ShipOrientation shipOrientation)
        {
            if (this.shipOrientation != shipOrientation)
            {
                if (shipOrientation == ShipOrientation.Horizontal)
                {
                    (offset.Y, offset.X) = (offset.X, offset.Y);
                }
                else if (shipOrientation == ShipOrientation.Vertical)
                {
                    (offset.Y, offset.X) = (offset.X, offset.Y);
                }

            }
            this.shipOrientation = shipOrientation;
        }
    }
}
