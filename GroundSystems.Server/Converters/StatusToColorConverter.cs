using GroundSystems.Server.Models.Enums;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;

namespace GroundSystems.Server.Converters
{
    public class StatusToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is SensorStatus status)
            {
                switch (status)
                {
                    case SensorStatus.Nominal:
                        return new SolidColorBrush(Colors.Green);
                    case SensorStatus.Warning:
                        return new SolidColorBrush(Colors.Yellow);
                    case SensorStatus.Critical:
                        return new SolidColorBrush(Colors.Red);
                    case SensorStatus.Offline:
                        return new SolidColorBrush(Colors.Gray);
                }
            }

            return new SolidColorBrush(Colors.Gray);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
