using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Battleship.Core.Game
{
    // DragShip class to hold drag information about a ship being dragged
    public class DragShip : Ship
    {
        private Canvas canvas;
        private readonly double cellSize;
        public Image img;

        // True is DragShip is selected and dragged
        public bool IsDragging = false;

        public Point offset;
        public Point mousePos;

        // Set starting offsets based on ship type
        public readonly int startTopOffset;

        // Left offsets are negative to position ships correctly
        public readonly int startLeftOffset;

        public DragShip(Canvas canvas, double cellSize, ShipType type)
            : base(type, ShipOrientation.Horizontal)
        {
            this.canvas = canvas;
            this.cellSize = cellSize;

            img = new Image()
            {
                Width = cellSize * (int)type,
                Height = cellSize,
                Source = RotateImage(
                    new BitmapImage(
                        new Uri(
                            "pack://application:,,,/Resources/Images/"
                                + type.ToString().ToLower()
                                + ".png"
                        )
                    ),
                    270
                ),
            };

            // Initial top offset position
            startTopOffset = type switch
            {
                ShipType.Battleship => 10,
                ShipType.Cruiser => 74,
                ShipType.Submarine => 153,
                ShipType.Destroyer => 242,
                _ => 0,
            };

            // Initial left offset position
            startLeftOffset = type switch
            {
                ShipType.Battleship => -140,
                ShipType.Cruiser => -155,
                ShipType.Submarine => -170,
                ShipType.Destroyer => -185,
                _ => 0,
            };
        }

        /// <summary>
        /// Updates the canvas element
        /// </summary>
        /// <param name="canvas">The new canvas element to replace the old element</param>
        public void updateCanvas(Canvas canvas)
        {
            this.canvas.Children.Clear();
            this.canvas = canvas;
        }

        /// <summary>
        /// Shows the drag ship at the canvas element
        /// </summary>
        /// <param name="dragShipManager">Once this is set, a ship can be towed</param>
        public void Show(DragShipManager? dragShipManager = null)
        {
            // Check if canvas already contains the img element
            if (!canvas.Children.Contains(img))
                canvas.Children.Add(img);

            // Procedure if drag ship manager is set
            if (dragShipManager != null)
            {
                // Sets the object to the start position outside the canvas element
                Place(startLeftOffset, startTopOffset);

                // Add mouse left button down event
                img.MouseLeftButtonDown += (s, e) =>
                {
                    dragShipManager.StartDrag(this, e.GetPosition(canvas));
                };

                // Add mouse move event
                img.MouseMove += (s, e) =>
                {
                    if (IsDragging)
                        dragShipManager.Move(this, e.GetPosition(canvas));
                };

                // Add mouse left button up event
                img.MouseLeftButtonUp += (s, e) =>
                {
                    dragShipManager.EndDrag(this, e.GetPosition(canvas));
                };

                // Add mouse right button down event
                img.MouseRightButtonDown += (s, e) =>
                {
                    // Ignore when object is not dragged by user
                    if (!IsDragging)
                        return;
                    // Change the orientation of the object
                    Rotate(
                        orientation == ShipOrientation.Horizontal
                            ? ShipOrientation.Vertical
                            : ShipOrientation.Horizontal,
                        false
                    );
                };
            }
            else
            {
                // Ohterwise the rotation is set to the ship object and placed to the exact canvas element position
                Rotate(orientation);
                Place();
            }
        }

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
        public void Rotate(ShipOrientation newOrientation, bool ignoreDragPosition = true)
        {
            // Old Size
            double oldWidth = img.Width;
            double oldHeight = img.Height;

            // New Orientation and Size
            if (newOrientation == ShipOrientation.Horizontal)
            {
                orientation = ShipOrientation.Horizontal;
                img.Width = cellSize * (int)type;
                img.Height = cellSize;
            }
            else
            {
                orientation = ShipOrientation.Vertical;
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
                new BitmapImage(
                    new Uri(
                        "pack://application:,,,/Resources/Images/"
                            + type.ToString().ToLower()
                            + ".png"
                    )
                ),
                (int)orientation
            );

            // Set new position based on mouse position and new offset
            if (!ignoreDragPosition)
                SetDragPosition(new Point(mousePos.X - offset.X, mousePos.Y - offset.Y));
        }

        /// <summary>
        /// Sets the position of the dragged element, clamped to the canvas bounds
        /// </summary>
        /// <param name="element">The UI element to move</param>
        /// <param name="position">The desired position</param>
        private void SetDragPosition(Point imgPos)
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
        public void Place(double? left = null, double? top = null)
        {
            double x = col * cellSize;
            double y = row * cellSize;

            // Set position on canvas
            Canvas.SetLeft(img, (double)(left == null ? x : left));
            Canvas.SetTop(img, (double)(top == null ? y : top));

            mousePos = new Point(x, y);
        }
    }
}
