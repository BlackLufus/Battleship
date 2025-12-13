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
    public class DragShipManager(double cellSize, BattleField battleField)
    {
        // Event when a ship is removed from the battlefield
        public delegate void OnShipRemovedEventHandler(DragShip dragger);
        public event OnShipRemovedEventHandler? OnShipRemovedEvent;

        // Variables provided by class constructor
        private readonly double cellSize = cellSize;
        private readonly BattleField battleField = battleField;

        // Represents the number of updates send per second or the number of frames are rendered per second
        private readonly int fps = 120;

        // Stores the datetime object of the last update
        private DateTime last = DateTime.Now;


        /// <summary>
        /// Get grid position from mouse position
        /// </summary>
        /// <param name="mousePos">Mouse position</param>
        /// <returns>True, when successful otherwise False</returns>
        private bool GetGridPos(DragShip dragShip, Point mousePos)
        {
            if (dragShip == null)
                return false;

            double x = mousePos.X - dragShip.offset.X;
            double y = mousePos.Y - dragShip.offset.Y;

            dragShip.col = (int)Math.Round(x / cellSize);
            dragShip.row = (int)Math.Round(y / cellSize);

            return true;
        }

        /// <summary>
        /// Starts dragging a ship from the given mouse position
        /// </summary>
        /// <param name="ship">The ship to drag</param>
        /// <param name="mousePos">The mouse position</param>
        public void StartDrag(DragShip dragShip, Point mousePos)
        {
            dragShip.offset = new Point(mousePos.X - Canvas.GetLeft(dragShip.img),
                                    mousePos.Y - Canvas.GetTop(dragShip.img));

            Mouse.Capture(dragShip.img);

            if (!dragShip.IsDragging) dragShip.IsDragging = true;

            var gridPos = GetGridPos(dragShip, mousePos);
            if (gridPos)
                // Register dragger to battleField
                battleField.Mark(dragShip.row, dragShip.col, dragShip.type, dragShip.orientation).ToString();
        }

        /// <summary>
        /// Moves the currently dragged ship to the given mouse position
        /// </summary>
        /// <param name="mousePos">The mouse position</param>
        public void Move(DragShip dragShip, Point mousePos)
        {
            double delta = 1000 / fps;
            DateTime now = DateTime.Now;
            var diffInMillies = (now - last).TotalMilliseconds;

            if (diffInMillies < delta)
                return;

            last = now;

            // Calculate new position
            double x = mousePos.X - dragShip.offset.X;
            double y = mousePos.Y - dragShip.offset.Y;

            // Set new position
            Canvas.SetLeft(dragShip.img, x);
            Canvas.SetTop(dragShip.img, y);

            // Update mouse position in dragger
            dragShip.SetMousePosition(mousePos);

            // Calculate grid position
            var gridPos = GetGridPos(dragShip, mousePos);
            if (gridPos)
            {
                //Debug.WriteLine($"x {x} y {y} row {currentDrag.row} col {currentDrag.col}");
                // Mark it on the battlefield
                battleField.Mark(dragShip.row, dragShip.col, dragShip.type, dragShip.orientation);
            }
        }

        /// <summary>
        /// Ends the dragging of the current ship at the given mouse position
        /// </summary>
        /// <param name="mousePos">The mouse position</param>
        public void EndDrag(DragShip dragShip, Point mousePos)
        {
            // Release mouse capture
            Mouse.Capture(null);

            // Try to add the ship to the battlefield
            if (dragShip.IsDragging && battleField.Add())
            {
                // Place ship at the grid position
                dragShip.Place(cellSize);
            }
            else
            {
                // Return to start position
                Canvas.SetTop(dragShip.img, dragShip.startTopOffset);
                Canvas.SetLeft(dragShip.img, dragShip.startLeftOffset);
                dragShip.Rotate(cellSize, ShipOrientation.Horizontal);
                battleField.Remove();
            }

            // Reset dragging state
            dragShip.IsDragging = false;
        }
    }

    // DragShip class to hold drag information about a ship being dragged
    public class DragShip(Canvas canvas, Image img, Ship.ShipType type) : Ship(type, Ship.ShipOrientation.Horizontal)
    {
        public Canvas canvas = canvas;
        public Image img = img;

        // True is DragShip is selected and dragged
        public bool IsDragging = false;

        public Point offset;
        public Point mousePos;

        // Set starting offsets based on ship type
        public readonly int startTopOffset = type switch
        {
            ShipType.Battleship => 10,
            ShipType.Cruiser => 74,
            ShipType.Submarine => 153,
            ShipType.Destroyer => 242,
            _ => 0
        };
        // Left offsets are negative to position ships correctly
        public readonly int startLeftOffset = type switch
        {
            ShipType.Battleship => -140,
            ShipType.Cruiser => -155,
            ShipType.Submarine => -170,
            ShipType.Destroyer => -185,
            _ => 0
        };

        /// <summary>
        /// Sets the current mouse position
        /// </summary>
        /// <param name="p">The mouse position</param>
        public void SetMousePosition(Point p)
        {
            mousePos = p;
        }

        /// <summary>
        /// Rotates the currently dragged ship
        /// </summary>
        public void Rotate(double cellSize, Ship.ShipOrientation newOrientation, bool ignoreDragPosition = true)
        {

            // Old Size
            double oldWidth = img.Width;
            double oldHeight = img.Height;


            // New Orientation and Size
            if (newOrientation == Ship.ShipOrientation.Horizontal)
            {
                orientation = Ship.ShipOrientation.Horizontal;
                img.Width = cellSize * (int)type;
                img.Height = cellSize;
            }
            else
            {
                orientation = Ship.ShipOrientation.Vertical;
                img.Width = cellSize;
                img.Height = cellSize * (int)type;
            }

            // New Size
            double newWidth = img.Width;
            double newHeight = img.Height;

            // Correct offset so that mouse stays in the same place
            offset = new Point(
                offset.X * (newWidth / oldWidth),
                offset.Y * (newHeight / oldHeight)
            );

            // Set new
            // Image rotation
            img.Source = RotateImage(
                new BitmapImage(new Uri("pack://application:,,,/Resources/Images/" + type.ToString().ToLower() + ".png")),
                (int)orientation
            );

            // Set new position based on mouse position and new offset
            if (!ignoreDragPosition)
                SetDragPosition(img, new Point(mousePos.X - offset.X, mousePos.Y - offset.Y));
        }

        /// <summary>
        /// Sets the position of the dragged element, clamped to the canvas bounds
        /// </summary>
        /// <param name="element">The UI element to move</param>
        /// <param name="position">The desired position</param>
        private void SetDragPosition(Image img, Point imgPos)
        {
            if (imgPos.X < 0)
                imgPos.X = 0;
            else if (imgPos.X > canvas.Width)
                imgPos.X = canvas.Width;
            if (imgPos.Y < 0)
                imgPos.Y = 0;
            else if (imgPos.Y > canvas.Height)
                imgPos.Y = canvas.Height;

            // Set position of the element on the canvas
            Canvas.SetLeft(img, imgPos.X);
            Canvas.SetTop(img, imgPos.Y);
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
        public void Place(double cellSize)
        {
            double x = col * cellSize;
            double y = row * cellSize;

            // Set position on canvas
            Canvas.SetLeft(this.img, x);
            Canvas.SetTop(this.img, y);

            mousePos = new Point(x, y);
        }

        public void Dispose()
        {
            canvas.Children.Remove(img);
            img.Source = null;
        }
    }
}
