using Battleship.Global;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using static Battleship.Global.Ship;

namespace Battleship.Lobby
{
    // Simple drag and drop handler for ships
    public class DragShipManager(double cellSize, BattleField battleField, Canvas canvas)
    {
        // Event when a ship is removed from the battlefield
        public delegate void OnShipRemovedEventHandler(Dragger dragger);
        public event OnShipRemovedEventHandler? OnShipRemovedEvent;

        // Currently dragged ship
        public Dragger? currentDrag;

        private readonly double cellSize = cellSize;
        private readonly BattleField battleField = battleField;
        private readonly Canvas canvas = canvas;

        private bool isDragging = false;

        /// <summary>
        /// Starts dragging a ship from the given mouse position
        /// </summary>
        /// <param name="ship">The ship to drag</param>
        /// <param name="mousePos">The mouse position</param>
        public void StartDrag(Dragger ship, Point mousePos)
        {
            currentDrag = ship;
            ship.offset = new Point(mousePos.X - Canvas.GetLeft(ship.element),
                                    mousePos.Y - Canvas.GetTop(ship.element));

            double x = mousePos.X - currentDrag.offset.X;
            double y = mousePos.Y - currentDrag.offset.Y;

            int col = (int)((x + (cellSize / 2)) / cellSize);
            int row = (int)((y + (cellSize / 2)) / cellSize);

            Mouse.Capture(ship.element);

            battleField.Mark(row, col, ship.type, ship.orientation);
        }

        /// <summary>
        /// Moves the currently dragged ship to the given mouse position
        /// </summary>
        /// <param name="mousePos">The mouse position</param>
        public void Move(Point mousePos)
        {
            if (!isDragging) isDragging = true;
            if (currentDrag == null) return;

            // Calculate new position
            double x = mousePos.X - currentDrag.offset.X;
            double y = mousePos.Y - currentDrag.offset.Y;

            // Set new position
            Canvas.SetLeft(currentDrag.element, x);
            Canvas.SetTop(currentDrag.element, y);

            // Update mouse position in dragger
            currentDrag.SetMousePosition(mousePos);

            // Calculate grid position
            currentDrag.col = (int)((x + (cellSize / 2)) / cellSize);
            currentDrag.row = (int)((y + (cellSize / 2)) / cellSize);

            // Mark it on the battlefield
            battleField.Mark(currentDrag.row, currentDrag.col, currentDrag.type, currentDrag.orientation);
        }

        /// <summary>
        /// Ends the dragging of the current ship at the given mouse position
        /// </summary>
        /// <param name="mousePos">The mouse position</param>
        public void EndDrag(Point mousePos)
        {
            if (!isDragging)
                currentDrag = null;
            if (currentDrag == null) return;
            
            // Release mouse capture
            Mouse.Capture(null);

            // Try to add the ship to the battlefield
            if (battleField.Add())
            {
                // Place ship at the grid position
                Place(currentDrag, currentDrag.row, currentDrag.col);
            }
            else
            {
                // Return to start position
                Canvas.SetTop(currentDrag.element, currentDrag.startTopOffset);
                Canvas.SetLeft(currentDrag.element, currentDrag.startLeftOffset);
            }

            // Reset dragging state
            isDragging = false;
            currentDrag = null;
        }

        /// <summary>
        /// Rotates the currently dragged ship
        /// </summary>
        public void RotateCurrent()
        {
            if (currentDrag == null) return;

            var ship = currentDrag;
            var image = (Image)ship.element;

            Point mouse = ship.mousePos;

            // Old Size
            double oldWidth = image.Width;
            double oldHeight = image.Height;

            // New Orientation and Size
            if (ship.orientation == Ship.ShipOrientation.Vertical)
            {
                ship.orientation = Ship.ShipOrientation.Horizontal;
                image.Width = cellSize * (int)ship.type;
                image.Height = cellSize;
            }
            else
            {
                ship.orientation = Ship.ShipOrientation.Vertical;
                image.Width = cellSize;
                image.Height = cellSize * (int)ship.type;
            }

            // New Size
            double newWidth = image.Width;
            double newHeight = image.Height;

            // Correct offset so that mouse stays in the same place
            ship.offset = new Point(
                ship.offset.X * (newWidth / oldWidth),
                ship.offset.Y * (newHeight / oldHeight)
            );

            // Image rotation
            image.Source = RotateImage(
                new BitmapImage(new Uri("pack://application:,,,/Resources/Images/" + ship.type.ToString().ToLower() + ".png")),
                (int)ship.orientation
            );

            // Set new position based on mouse position and new offset
            SetDragPosition(ship.element, new Point(mouse.X - ship.offset.X, mouse.Y - ship.offset.Y));
        }


        /// <summary>
        /// Sets the position of the dragged element, clamped to the canvas bounds
        /// </summary>
        /// <param name="element">The UI element to move</param>
        /// <param name="position">The desired position</param>
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
            else if (position.X > canvas.Width)
            {
                position.X = canvas.Width;
            }
            if (position.Y < 0)
            {
                position.Y = 0;
            }
            else if (position.Y > canvas.Height)
            {
                position.Y = canvas.Height;
            }

            // Set position of the element on the canvas
            Canvas.SetLeft(element, position.X);
            Canvas.SetTop(element, position.Y);
        }

        /// <summary>
        /// Rotates the given image by the specified angle
        /// </summary>
        /// <param name="source">The source image</param>
        /// <param name="angle">The angle to rotate</param>
        /// <returns>The rotated image</returns>
        public static TransformedBitmap RotateImage(BitmapSource source, double angle)
        {
            TransformedBitmap transform = new();
            transform.BeginInit(); // Initialize the transform
            transform.Source = source; // Set the source image
            transform.Transform = new RotateTransform(angle); // Apply rotation
            transform.EndInit(); // Finalize the transform
            return transform;
        }

        /// <summary>
        /// Places the ship image at the specified grid position
        /// </summary>
        /// <param name="ship">The ship to place</param>
        /// <param name="row">The grid row</param>
        /// <param name="col">The grid column</param>
        private void Place(Dragger ship, int row, int col)
        {
            double x = col * cellSize;
            double y = row * cellSize;

            // Set position on canvas
            Canvas.SetLeft(ship.element, x);
            Canvas.SetTop(ship.element, y);
        }
    }

    // Dragger class to hold drag information about a ship being dragged
    public class Dragger
    {
        public Image element;
        public int row;
        public int col;
        public Ship.ShipType type;
        public Ship.ShipOrientation orientation = Ship.ShipOrientation.Horizontal;

        public Point offset;
        public Point mousePos;

        public readonly int startTopOffset;
        public readonly int startLeftOffset;

        public Dragger(Image element, ShipType type)
        {
            this.element = element;
            this.type = type;

            // Set starting offsets based on ship type
            startTopOffset = type switch
            {
                ShipType.Battleship => 10,
                ShipType.Cruiser => 74,
                ShipType.Submarine => 153,
                ShipType.Destroyer => 242,
                _ => 0
            };
            // Left offsets are negative to position ships correctly
            startLeftOffset = type switch
            {
                ShipType.Battleship => -150,
                ShipType.Cruiser => -160,
                ShipType.Submarine => -180,
                ShipType.Destroyer => -190,
                _ => 0
            };
        }

        /// <summary>
        /// Sets the current mouse position
        /// </summary>
        /// <param name="p">The mouse position</param>
        public void SetMousePosition(Point p)
        {
            mousePos = p;
        }
    }
}
