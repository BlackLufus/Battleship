namespace Battleship.Core.Services
{
    abstract class EventListener(string id)
    {
        private static List<EventListener>? eventListeners = new List<EventListener>();
        private string id = id;

        public static void AddListener(EventListener listener)
        {
            eventListeners?.Add(listener);
        }

        public static void RemoveListener(EventListener listener)
        {
            eventListeners?.Remove(listener);
        }

        public static void RemoveAllListenerById(string id)
        {
            eventListeners?.RemoveAll(listener => listener.id == id);
        }
    }
}
