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
    /// Interaktionslogik für Selector.xaml
    /// </summary>
    public partial class Selector : UserControl
    {
        private int selectedIndex = -1;

        public Selector()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty SelectedItemProperty =
            DependencyProperty.Register(
                "SelectedItem",
                typeof(string),
                typeof(Selector),
                new PropertyMetadata("")
            );

        public string SelectedItem
        {
            get { return (string)GetValue(SelectedItemProperty); }
            set { SetValue(SelectedItemProperty, value); }
        }

        public static readonly DependencyProperty IndexProperty = DependencyProperty.Register(
            "Index",
            typeof(string),
            typeof(Selector),
            new PropertyMetadata("0")
        ); // Standardwert 0

        public string Index
        {
            get { return (string)GetValue(IndexProperty); }
            set { SetValue(IndexProperty, value); }
        }

        // DependencyProperty für die Items (Liste von Strings)
        public static readonly DependencyProperty ItemsProperty = DependencyProperty.Register(
            "Items",
            typeof(List<(string, object)>),
            typeof(Selector),
            new PropertyMetadata(new List<(string, object)>())
        );

        // Eigenschaft für die Liste
        public List<(string, object)> Items
        {
            get { return (List<(string, object)>)GetValue(ItemsProperty); }
            set { SetValue(ItemsProperty, value); }
        }

        public int SelectedIndex
        {
            get { return selectedIndex; }
            set { SetSelectedItem(value); }
        }

        public object SelectedValue
        {
            get { return Items[selectedIndex].Item2; }
        }

        private void SetSelectedItem(int index)
        {
            if (index >= 0 && index < Items.Count)
            {
                SelectedItem = Items[index].Item1;
                selectedIndex = index;
            }
        }

        // Linker Pfeil-Click-Event-Handler
        private void LeftArrow_Click(object sender, RoutedEventArgs e)
        {
            SetSelectedItem(selectedIndex - 1);
        }

        // Rechter Pfeil-Click-Event-Handler
        private void RightArrow_Click(object sender, RoutedEventArgs e)
        {
            SetSelectedItem(selectedIndex + 1);
        }

        private void TextBox_Loaded(object sender, RoutedEventArgs e)
        {
            SetSelectedItem(selectedIndex == -1 ? int.Parse(Index) : selectedIndex);
        }
    }
}
