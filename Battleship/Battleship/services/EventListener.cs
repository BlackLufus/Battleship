using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Battleship.services
{
    abstract class EventListener
    {
        private static List<EventListener>? eventListeners = new List<EventListener>();
        private string id;

        protected EventListener(String id)
        {
            this.id = id;
        }

        public static void AddListener(EventListener listener)
        {
            eventListeners?.Add(listener);
        }

        public static void RemoveListener(EventListener listener)
        {
            eventListeners?.Remove(listener);
        }

        public static void RemoveAllListenerById(String id)
        {
            eventListeners?.RemoveAll(listener => listener.id == id);
        }
    }
}
