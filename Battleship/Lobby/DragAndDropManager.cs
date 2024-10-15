using Battleship.Playground;
using Battleship.Resources.Components;
using Battleship.services;
using Battleship.Services;
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
        public delegate void DragEnterEventHandler(DragShip dragShip, int row, int Column);
        public event DragEnterEventHandler? DragEnterEvent;

        public delegate void DragLeaveEventHandler(DragShip dragShip, int row, int Column);
        public event DragLeaveEventHandler? DragLeaveEvent;

        public delegate void DragDropEventHandler(DragShip dragShip, int row, int Column, Action<int, int> callback);
        public event DragDropEventHandler? DragDropEvent;

        public delegate void UpdateEventHandler();
        public event UpdateEventHandler? UpdateEvent;

        public delegate void AddShipEventHandler(UIElement image);
        public event AddShipEventHandler? AddShipEvent;

        public delegate void RemoveShipEventHandler(UIElement image);
        public event RemoveShipEventHandler? RemoveShipEvent;

        private readonly BattleField battleField;
        private readonly GameSetting gameSetting;

        private readonly List<DragShip> dragShips = [];
        public List<DragShip> DragShips => dragShips;

        private DragShip? currentDragShip;

        public DragAndDropManager(GameSetting gameSetting, BattleField battleField)
        {
            this.gameSetting = gameSetting;
            this.battleField = battleField;
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

        public void HandleDragOverEvent(object sender, DragEventArgs e)
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
            });
        }

        public void StartDrag(Ship.ShipType shipType, Point offset)
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

                    DragDrop.DoDragDrop(dragShip.element, dragShip, DragDropEffects.All);
                    dragShips.Remove(dragShip);

                    UpdateEvent?.Invoke();
                    
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

        public void RemoveShip(object sender, MouseButtonEventArgs e)
        {
            int row = Grid.GetRow((UIElement)sender);
            int column = Grid.GetColumn((UIElement)sender);
            foreach (DragShip dragShip in dragShips)
            {
                if (BattleField.IsShipOnPosition(dragShip, row, column))
                {
                    battleField.RemoveShip(dragShip.row, dragShip.column, dragShip.shipType, dragShip.shipOrientation, FieldState.Ship);

                    RemoveShipEvent?.Invoke(dragShip.element);

                    dragShips.Remove(dragShip);
                    dragShip.Dispose();

                    UpdateEvent?.Invoke();

                    break;
                }
            }
        }

        public void Randomize(List<Ship> shipList)
        {
            Reset();

            if (battleField.Randomize(shipList))
            {
                foreach (Ship ship in shipList)
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

                    UpdateEvent?.Invoke();
                }
            }
            else
            {
                Dialog.Show(Dialog.DialogType.Error, Dialog.ButtonType.Ok, "Randomize failed", "Randomize failed, please try again.");
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
            battleField.Reset();
            UpdateEvent?.Invoke();
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
