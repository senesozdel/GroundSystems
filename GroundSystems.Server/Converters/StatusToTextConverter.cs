using GroundSystems.Server.Models.Enums;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace GroundSystems.Server.Converters
{
    public class StatusToTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is SensorStatus status)
            {
                switch (status)
                {
                    case SensorStatus.Nominal:
                        return "İyi";
                    case SensorStatus.Warning:
                        return "Uyarı";
                    case SensorStatus.Critical:
                        return "Kritik";
                    case SensorStatus.Offline:
                        return "İnaktif";
                    default:
                        return "Bilinmiyor";
                }
            }
            return "Bilinmiyor";

        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
