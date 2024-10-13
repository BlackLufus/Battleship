using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Battleship.Resources.Components
{
    /// <summary>
    /// Interaktionslogik für Slider.xaml
    /// </summary>
    public partial class SliderButton : UserControl
    {
        private Slider? slider;
        private Canvas? canvas;
        private Label? label;
        public SliderButton()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty MaxValueProperty =
            DependencyProperty.Register("MaxValue", typeof(string), typeof(SliderButton), new PropertyMetadata("0")); // Standardwert 0

        public string MaxValue
        {
            get { return (string)GetValue(MaxValueProperty); }
            set { SetValue(MaxValueProperty, value); }
        }

        public static readonly DependencyProperty MinValueProperty =
            DependencyProperty.Register("MinValue", typeof(string), typeof(SliderButton), new PropertyMetadata("0")); // Standardwert 0

        public string MinValue
        {
            get { return (string)GetValue(MinValueProperty); }
            set { SetValue(MinValueProperty, value); }
        }

        public static readonly DependencyProperty StartValueProperty =
            DependencyProperty.Register("StartValue", typeof(string), typeof(SliderButton), new PropertyMetadata("0")); // Standardwert 0

        public string StartValue
        {
            get { return (string)GetValue(StartValueProperty); }
            set { SetValue(StartValueProperty, value); }
        }

        public int Value
        {
            get { return (int)slider.Value; }
            set { slider.Value = value; }
        }

        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Debug.WriteLine(e.NewValue);
            if (label != null)
                label.Content = e.NewValue;
        }

        private void Canvas_Loaded(object sender, RoutedEventArgs e)
        {
            canvas = (Canvas)sender;
        }

        private void Slider_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Debug.WriteLine("MouseDown");
            if (canvas != null)
                canvas.Visibility = Visibility.Visible;
        }

        private void Slider_MouseUp(object sender, MouseButtonEventArgs e)
        {
            Debug.WriteLine("MouseUp");
            if (canvas != null)
                canvas.Visibility = Visibility.Collapsed;
        }

        private void Label_Loaded(object sender, RoutedEventArgs e)
        {
            label = (Label)sender;
        }

        private void Slider_Loaded(object sender, RoutedEventArgs e)
        {
            slider = (Slider)sender;
        }
    }
}
