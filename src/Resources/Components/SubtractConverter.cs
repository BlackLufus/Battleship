using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Battleship.Resources.Components
{
    class SubtractConverter : IValueConverter
    {
        public object Convert(
            object value,
            Type targetType,
            object parameter,
            System.Globalization.CultureInfo culture
        )
        {
            if (value is double height)
            {
                return height - 6; // Subtrahiere 4 von der tatsächlichen Höhe
            }
            return value;
        }

        public object ConvertBack(
            object value,
            Type targetType,
            object parameter,
            System.Globalization.CultureInfo culture
        )
        {
            return null;
        }
    }
}
