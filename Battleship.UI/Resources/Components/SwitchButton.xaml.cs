using System.Windows.Controls;

namespace Battleship.UI.Resources.Components
{
    /// <summary>
    /// Interaktionslogik für Switch.xaml
    /// </summary>
    public partial class SwitchButton : UserControl
    {
        public SwitchButton()
        {
            InitializeComponent();
        }

        public bool IsChecked
        {
            get { return SwitchButtonComponent.IsChecked == true; }
        }
    }
}
