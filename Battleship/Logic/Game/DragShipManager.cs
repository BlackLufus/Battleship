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
using static Battleship.Logic.Board.Ship;

namespace Battleship.Logic.Board
{
    // Simple drag and drop handler for ships
    public class DragShipManager(double cellSize, DragShipBoard battleField)
    {
        // Variables provided by class constructor
        private readonly double cellSize = cellSize;
        private readonly DragShipBoard battleField = battleField;

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
                battleField.Mark(dragShip).ToString();
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
                battleField.Mark(dragShip);
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
            if (dragShip.IsDragging && battleField.Place(dragShip))
            {
                // Place ship at the grid position
                dragShip.Place();
            }
            else
            {
                // Return to start position
                Canvas.SetTop(dragShip.img, dragShip.startTopOffset);
                Canvas.SetLeft(dragShip.img, dragShip.startLeftOffset);
                dragShip.Rotate(ShipOrientation.Horizontal);
            }

            // Reset dragging state
            dragShip.IsDragging = false;
        }
    }
}
