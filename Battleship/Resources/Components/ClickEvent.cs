using Battleship.Logic.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace Battleship.Resources.Components
{
    public class ClickEvent : IDisposable
    {
        private UserControl? component;
        private readonly Action callback;
        private Border? element;
        private bool mouseDown = false;
        public bool MouseDown => mouseDown;

        public bool IsPressed {
            get
            {
                Mouse.Capture(null);
                bool isPressed = mouseDown && component!.IsMouseOver;
                Mouse.Capture(component);
                return isPressed;
            }
        }


        private static Thread? thread;
        private static List<ClickEvent> clickEvents = [];

        private static void HandleEventClick(ClickEvent clickEvent)
        {
            clickEvents.Add(clickEvent);

            if (thread == null)
            {
                thread = new Thread(() =>
                {
                    ClickEvent? currentClickEvent = null;
                    bool isPressed = false;
                    while (true)
                    {
                        if (currentClickEvent == null)
                        {
                            for (int i = 0; i < clickEvents.Count; i++)
                            {
                                //Debug.WriteLine(i + ": ClickEvent: " + clickEvents[i].mouseDown);
                                if (clickEvents[i].mouseDown)
                                {
                                    currentClickEvent = clickEvents[i];
                                    break;
                                }
                            }
                        }
                        else if (currentClickEvent != null)
                        {
                            if (currentClickEvent.mouseDown)
                            {
                                Application.Current.Dispatcher.Invoke(() =>
                                {
                                    if (currentClickEvent.IsPressed)
                                    {
                                        isPressed = true;
                                        currentClickEvent.element.Visibility = Visibility.Visible;
                                    }
                                    else
                                    {
                                        isPressed = false;
                                        currentClickEvent.element.Visibility = Visibility.Hidden;
                                    }
                                });
                            }
                            else if (!currentClickEvent.mouseDown)
                            {
                                if (isPressed)
                                {
                                    isPressed = false;
                                    Application.Current.Dispatcher.Invoke(() =>
                                    {
                                        currentClickEvent.element.Visibility = Visibility.Hidden;
                                    });
                                }
                                currentClickEvent = null;
                            }
                        }
                        try
                        {
                            Thread.Sleep(1);
                        }
                        catch (ThreadInterruptedException)
                        {
                            break;
                        }
                    }
                });
                ThreadListener.AddThread(thread);
                thread.Start();
            }
        }

        public ClickEvent(UserControl component, Action callback, Border? element)
        {
            this.component = component;
            this.callback = callback;
            this.element = element;

            component.PreviewMouseLeftButtonDown += OnMouseLeftButtonDown;
            component.PreviewMouseLeftButtonUp += OnMouseLeftButtonUp;

            if (element != null)
            {
                HandleEventClick(this);
            }
        }

        private void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            //Debug.WriteLine("PreviewMouseLeftButtonDown");
            mouseDown = true;
            Mouse.Capture(component);
        }

        private void OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Mouse.Capture(null);
            //Debug.WriteLine(mouseDown);
            //Debug.WriteLine(component!.IsMouseOver);
            if (!mouseDown || !component.IsMouseOver)
            {
                e.Handled = true;
            }
            else
            {
                //Debug.WriteLine("PreviewMouseLeftButtonUp");
                callback();
            }
            mouseDown = false;
            //Debug.WriteLine("End of Event");
        }

        public void Dispose()
        {
            // Wenn das component nicht null ist, unregistere die Ereignisse
            if (component != null)
            {
                //Debug.WriteLine("Dispose");
                component.PreviewMouseLeftButtonUp -= OnMouseLeftButtonUp;
                component.PreviewMouseLeftButtonDown -= OnMouseLeftButtonDown;
                ThreadListener.RemoveThread(thread!);
                thread!.Interrupt();

                // Setze die Referenz auf null, um die GC zu erleichtern
                component = null;
            }

            GC.SuppressFinalize(this);
        }
    }
}
