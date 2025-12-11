using Battleship.Global;
using Battleship.Resources.Components;
using Battleship.services;
using Battleship.Services;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Battleship.Lobby
{
    public class DragAndDropManager
    {
        public delegate void DragEnterEventHandler(DragShip dragShip, int row, int Column);
        public event DragEnterEventHandler? DragEnterEvent;

        public delegate void DragLeaveEventHandler(DragShip dragShip, int row, int Column);
        public event DragLeaveEventHandler? DragLeaveEvent;

        public delegate void DragDropEventHandler(DragShip dragShip, int row, int Column, Action<int, int> callback);
        public event DragDropEventHandler? DragDropEvent;

        public delegate void AddShipEventHandler(UIElement image);
        public event AddShipEventHandler? AddShipEvent;

        public delegate void RemoveShipEventHandler(UIElement image);
        public event RemoveShipEventHandler? RemoveShipEvent;

        private readonly GameSetting gameSetting;

        private readonly List<DragShip> dragShips = [];
        public List<DragShip> DragShips => dragShips;

        private DragShip? currentDragShip;


        public DragAndDropManager(GameSetting gameSetting)
        {
            this.gameSetting = gameSetting;
            KeyListener.Event().KeyDownEvent += KeyListener_KeyDownEvent;
        }

        private void KeyListener_KeyDownEvent(Key key)
        {
            if (this.currentDragShip != null)
            {
                Image image = (Image)this.currentDragShip.element;
                if (key == Key.Left || key == Key.Right)
                {
                    this.currentDragShip.Rotate(DragShip.ShipOrientation.Horizontal);
                    image.Width = gameSetting.SingleFieldSize * (int)this.currentDragShip.shipType;
                    image.Height = gameSetting.SingleFieldSize;
                }
                else if (key == Key.Up || key == Key.Down)
                {
                    this.currentDragShip.Rotate(DragShip.ShipOrientation.Vertical);
                    image.Width = gameSetting.SingleFieldSize;
                    image.Height = gameSetting.SingleFieldSize * (int)this.currentDragShip.shipType;
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


        public UIElement StartDrag(Ship.ShipType shipType, Point offset)
        {
            Image element = new()
            {
                Width = gameSetting.SingleFieldSize * (int)shipType,
                Height = gameSetting.SingleFieldSize,
                Source = RotateImage(new BitmapImage(new Uri("pack://application:,,,/Resources/Images/" + shipType.ToString().ToLower() + ".png")), 270)
            };

            AddShipEvent?.Invoke(element);

            DragShip dragShip = new(element, offset, shipType);

            currentDragShip = dragShip;

            dragShip.element.QueryContinueDrag += Element_QueryContinueDrag;

            DragDrop.DoDragDrop(dragShip.element, dragShip, DragDropEffects.All);

            return element;
        }

        public void RedragShip(MouseButtonEventArgs e, int row, int column)
        {
            foreach (DragShip dragShip in dragShips)
            {
                if (dragShip.IsShipOnPosition(dragShip, row, column))
                {
                    this.currentDragShip = dragShip;

                    dragShip.offset = new Point(e.GetPosition(dragShip.element).X, e.GetPosition(dragShip.element).Y);
                    dragShip.element.QueryContinueDrag += Element_QueryContinueDrag;

                    DragDrop.DoDragDrop(dragShip.element, dragShip, DragDropEffects.All);
                    dragShips.Remove(dragShip);

                    break;
                }
            }
        }

        private void Element_QueryContinueDrag(object sender, QueryContinueDragEventArgs e)
        {
            // Wenn die linke Maustaste losgelassen wird und das Ziel nicht erreicht wird
            if (e.Action == DragAction.Cancel || (e.KeyStates & DragDropKeyStates.LeftMouseButton) == 0 || (e.KeyStates & DragDropKeyStates.RightMouseButton) != 0)
            {
                RemoveShipEvent?.Invoke((Image)sender);
                this.currentDragShip?.Dispose();
                this.currentDragShip = null;

                // Der Drag-Vorgang wurde außerhalb eines Ziehziels abgebrochen
                e.Action = DragAction.Cancel;
            }
        }

        public void HandleDragEnterEvent(object sender, DragEventArgs e)
        {
            DragShip dragShip = (DragShip)e.Data.GetData(typeof(DragShip));
            DragEnterEvent?.Invoke(dragShip, Grid.GetRow((UIElement)sender), Grid.GetColumn((UIElement)sender));
        }

        public void HandleDragLeaveEvent(object sender, DragEventArgs e)
        {
            DragShip dragShip = (DragShip)e.Data.GetData(typeof(DragShip));
            DragLeaveEvent?.Invoke(dragShip, Grid.GetRow((UIElement)sender), Grid.GetColumn((UIElement)sender));
        }

        public void HandleDropEvent(object sender, DragEventArgs e)
        {
            DragShip dragShip = (DragShip)e.Data.GetData(typeof(DragShip));
            DragDropEvent?.Invoke(dragShip, Grid.GetRow((UIElement)sender), Grid.GetColumn((UIElement)sender), (int row, int column) =>
            {
                dragShip.row = row;
                dragShip.column = column;
                dragShips.Add(dragShip);

                Canvas.SetTop(dragShip.element, 0 + dragShip.row * gameSetting.SingleFieldSize);
                Canvas.SetLeft(dragShip.element, 190 + dragShip.column * gameSetting.SingleFieldSize);

                AddShipEvent?.Invoke(dragShip.element);
            });
        }

        public DragShip? RemoveShip(int row, int column)
        {
            foreach (DragShip dragShip in dragShips)
            {
                if (dragShip.IsShipOnPosition(dragShip, row, column))
                {
                    var ship = dragShip;

                    RemoveShipEvent?.Invoke(dragShip.element);
                    dragShips.Remove(dragShip);

                    return ship;
                }
            }
            return null;
        }

        public void SetShips(List<Ship> ships)
        {
            Reset();
            foreach (Ship ship in ships)
            {
                Image element = new()
                {
                    Width = gameSetting.SingleFieldSize * (ship.shipOrientation == Ship.ShipOrientation.Horizontal ? (int)ship.shipType : 1),
                    Height = gameSetting.SingleFieldSize * (ship.shipOrientation == Ship.ShipOrientation.Vertical ? (int)ship.shipType : 1),
                    Source = RotateImage(new BitmapImage(new Uri("pack://application:,,,/Resources/Images/" + ship.shipType.ToString().ToLower() + ".png")), (int)ship.shipOrientation)
                };

                AddShipEvent?.Invoke(element);

                DragShip dragShip = new(element, new Point(0, 0), ship.shipType, ship.shipOrientation)
                {
                    row = ship.row,
                    column = ship.column
                };

                this.dragShips.Add(dragShip);

                Canvas.SetLeft(dragShip.element, 190 + (double)dragShip.column * gameSetting.SingleFieldSize);
                Canvas.SetTop(dragShip.element, 0 + (double)dragShip.row * gameSetting.SingleFieldSize);
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
            else if (position.X > Application.Current.MainWindow.Width)
            {
                position.X = Application.Current.MainWindow.Width;
            }
            if (position.Y < 0)
            {
                position.Y = 0;
            }
            else if (position.Y > Application.Current.MainWindow.Height)
            {
                position.Y = Application.Current.MainWindow.Height;
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

        public void Reset()
        {
            dragShips.ForEach(
                dragShip => {
                    RemoveShipEvent?.Invoke(dragShip.element);
                    dragShip.Dispose();
                });
            dragShips.Clear();
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
                    offset = new Point(offset.Y, offset.X);
                }
                else if (shipOrientation == ShipOrientation.Vertical)
                {
                    offset = new Point(offset.Y, offset.X);
                }

            }
            this.shipOrientation = shipOrientation;
        }

        /**
         * 
         * @Description Check if a ship is on a specific position
         * @Param ship The ship to check
         * @Param row The row to check
         * @Param column The column to check
         * @Return true if the ship is on the position, otherwise false
         */
        public bool IsShipOnPosition(Ship ship, int row, int column)
        {
            if (ship.shipOrientation == Ship.ShipOrientation.Vertical)
            {
                if (row < ship.row || row >= ship.row + (int)ship.shipType || column != ship.column)
                {
                    return false;
                }
            }
            else
            {
                if (column < ship.column || column >= ship.column + (int)ship.shipType || row != ship.row)
                {
                    return false;
                }
            }
            return true;
        }
    }
}
