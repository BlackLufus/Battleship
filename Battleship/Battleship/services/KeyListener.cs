using Battleship.Services;
using System.Windows;
using System.Windows.Input;

namespace Battleship.services
{
    internal class KeyListener
    {
        private static KeyListener? instance;

        public delegate void KeyDownEventHandler(Key key);

        public event KeyDownEventHandler? KeyDownEvent;

        private KeyListener()
        {
            Thread thread = new Thread(() =>
            {
                List<Key> keys = new List<Key>();
                while (true)
                {
                    // Iteriere über die gültigen Werte der Enum Key
                    foreach (Key key in Enum.GetValues(typeof(Key)))
                    {
                        // Überspringe ungültige Enum-Werte, wie Key.None
                        if (key == Key.None) continue;

                        // Überprüfe, ob die Taste gedrückt ist
                        if (Keyboard.IsKeyDown(key) && keys.IndexOf(key) == -1)
                        {
                            // Nutze den Dispatcher, um den UI-Zugriff auf den UI-Thread zurückzuführen
                            Application.Current.Dispatcher.Invoke(() =>
                            {
                                keys.Add(key);
                                KeyDownEvent?.Invoke(key);
                            });
                        }
                        else if (!Keyboard.IsKeyDown(key) && keys.IndexOf(key) != -1)
                        {
                            keys.Remove(key);
                        }
                    }
                    Thread.Sleep(1); // CPU schonen
                }
            });

            ThreadListener.AddThread(thread); // Füge den Thread zur Liste der Threads hinzu
            // Starte den Thread
            thread.SetApartmentState(ApartmentState.STA);
            thread.IsBackground = true;  // Stelle sicher, dass der Thread im Hintergrund läuft
            thread.Start();
        }

        public static KeyListener Event()
        {
            if (instance == null)
            {
                instance = new KeyListener();
            }
            return instance;
        }

    }
}
