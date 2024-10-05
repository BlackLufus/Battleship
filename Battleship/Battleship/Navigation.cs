using System;
using System.Collections.Generic;
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

        public static void setup(Frame frame)
        {
            if (instance == null)
            {
                instance = new Navigation(frame);
            }
        }


        public static void navigateTo(Page page)
        {
            if (instance == null)
            {
                throw new Exception("Navigation not set up");
            }
            if (instance.pages.Count == 0 || instance.pages.Last() != page)
            {
                instance.pages.Add(page);
                instance.frame.NavigationService.Navigate(page);
            }
        }

        public static void navigateAndClear(Page page)
        {
            if (instance == null)
            {
                throw new Exception("Navigation not set up");
            }
            instance.pages.Clear();
            instance.pages.Add(page);
            instance.frame.NavigationService.Navigate(page);
        }

        public static void navigateBack()
        {
            if (instance == null)
            {
                throw new Exception("Navigation not set up");
            }
            if (instance.pages.Count > 1)
            {
                instance.pages.RemoveAt(instance.pages.Count - 1);
                instance.frame.NavigationService.Navigate(instance.pages[instance.pages.Count - 1]);
            }
        }
    }
}
