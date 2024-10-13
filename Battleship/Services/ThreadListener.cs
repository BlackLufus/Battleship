using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Battleship.Services
{
    public class ThreadListener
    {
        private static List<Thread> threads = [];
        private ThreadListener() { }

        public static void AddThread(Thread thread)
        {
            threads.Add(thread);
        }

        public static void RemoveThread(Thread thread)
        {
            threads.Remove(thread);
        }

        public static void StopAllThreads()
        {
            foreach (Thread thread in threads)
            {
                thread.Interrupt();
            }
        }
    }
}
