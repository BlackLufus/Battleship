namespace Battleship.services
{
    abstract class EventListener(String id)
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

        public static void RemoveAllListenerById(String id)
        {
            eventListeners?.RemoveAll(listener => listener.id == id);
        }
    }
}
