using Battleship.Lobby;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Xml.Linq;

namespace Battelship
{
    class Navigation
    {
        private static Frame? mainFrame;
        private static Navigation? mainFrameInstance;
        private static Navigation? instance;
        private Frame currentFrame;
        private List<Page> pages = new List<Page>();

        private Navigation(Frame frame)
        {
            this.currentFrame = frame;
        }

        /**
         * Register the main frame
         * @param frame The frame to register
         */
        public static void RegisterFrame(Frame frame)
        {
            mainFrame = frame;
        }

        /**
         * Register the main page for the main frame
         * @param page The page to register
         * @throws Exception If the main frame is not set up
         */
        public static void RegisterPage(Page page)
        {
            if (mainFrame == null)
            {
                throw new Exception("Main frame not set up");
            }
            mainFrame?.Navigate(page);
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
                instance.currentFrame.NavigationService.Navigate(page);
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
            instance.currentFrame.NavigationService.Navigate(page);
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
                instance.currentFrame.NavigationService.Navigate(instance.pages[instance.pages.Count - 1]);
            }
        }
    }
}
