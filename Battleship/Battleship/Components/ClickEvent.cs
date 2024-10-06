using Battleship.Services;
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

namespace Battleship.Components
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


        private Thread? thread;

        public ClickEvent(UserControl component, Action callback, Border element)
        {
            this.component = component;
            this.callback = callback;
            this.element = element;

            component.PreviewMouseLeftButtonDown += OnMouseLeftButtonDown;
            component.PreviewMouseLeftButtonUp += OnMouseLeftButtonUp;

            Thread thread = new Thread(() =>
            {
                bool isPressed = false;
                while (true)
                {
                    if (mouseDown)
                    {
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            if (IsPressed)
                            {
                                isPressed = true;
                                element.Visibility = Visibility.Visible;
                            }
                            else
                            {
                                isPressed = false;
                                element.Visibility = Visibility.Hidden;
                            }
                        });
                    }
                    else if (isPressed)
                    {
                        isPressed = false;
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            element.Visibility = Visibility.Hidden;
                        });
                    }
                    Thread.Sleep(100);
                }
            });
            ThreadListener.AddThread(thread);
            thread.Start();
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
