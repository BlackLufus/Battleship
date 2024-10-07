using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Battelship
{
    class Navigation
    {
        private static Navigation? instance;
        private Frame frame;
        private List<Page> pages = new List<Page>();

        private Navigation(Frame frame)
        {
            this.frame = frame;
        }

        public static void Setup(Frame frame)
        {
            instance = new Navigation(frame);
        }


        public static void NavigateTo(Page page)
        {
            if (instance == null)
            {
                throw new Exception("Navigation not set up");
            }
            if (instance.pages.Count == 0 || instance.pages.Last() != page)
            {
                Debug.WriteLine("Navigating to " + page);
                instance.pages.Add(page);
                instance.frame.NavigationService.Navigate(page);
            }
        }

        public static void NavigateAndClear(Page page)
        {
            if (instance == null)
            {
                throw new Exception("Navigation not set up");
            }
            Debug.WriteLine("Navigating to " + page + " and clear");
            instance.pages.Clear();
            instance.pages.Add(page);
            instance.frame.NavigationService.Navigate(page);
        }

        public static void NavigateBack()
        {
            if (instance == null)
            {
                throw new Exception("Navigation not set up");
            }
            if (instance.pages.Count > 1)
            {
                Debug.WriteLine("Navigating back");
                instance.pages.RemoveAt(instance.pages.Count - 1);
                instance.frame.NavigationService.Navigate(instance.pages[instance.pages.Count - 1]);
            }
        }
    }
}
