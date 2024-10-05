using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Battleship.services
{
    public class DragListener
    {
        private static DragListener? instance;

        private Thread? thread;

        public delegate void DragCancelEventHandler(Key key);

        public event DragCancelEventHandler? DragCancelEvent;

        private DragListener()
        {
            Thread thread = new Thread(() =>
            {
                List<Key> keys = new List<Key>();
                while (true)
                {
                    Thread.Sleep(1); // CPU schonen
                }
            });
        }

    
        public static DragListener Event()
        {
            if (instance == null)
            {
                instance = new DragListener();
            }
            return instance;
        }
    }
}
