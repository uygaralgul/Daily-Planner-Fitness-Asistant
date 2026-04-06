using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using DailyPlanner.Services;

namespace DailyPlanner.Converters
{
    public class BoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type t, object p, CultureInfo c)
            => value is bool b && b ? Visibility.Visible : Visibility.Collapsed;
        public object ConvertBack(object value, Type t, object p, CultureInfo c)
            => value is Visibility v && v == Visibility.Visible;
    }

    public class InverseBoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type t, object p, CultureInfo c)
            => value is bool b && b ? Visibility.Collapsed : Visibility.Visible;
        public object ConvertBack(object value, Type t, object p, CultureInfo c)
            => throw new NotImplementedException();
    }

    public class BoolToOpacityConverter : IValueConverter
    {
        public object Convert(object value, Type t, object p, CultureInfo c)
            => value is bool b && b ? 0.45 : 1.0;
        public object ConvertBack(object value, Type t, object p, CultureInfo c)
            => throw new NotImplementedException();
    }

    public class DayStatusToColorConverter : IValueConverter
    {
        public object Convert(object value, Type t, object p, CultureInfo c)
        {
            if (value is DayStatus status)
            {
                return status switch
                {
                    DayStatus.Completed => new SolidColorBrush(Color.FromRgb(16, 185, 129)),   // emerald
                    DayStatus.Partial   => new SolidColorBrush(Color.FromRgb(245, 158, 11)),   // amber
                    DayStatus.HasTasks  => new SolidColorBrush(Color.FromRgb(99, 102, 241)),   // indigo
                    _                   => new SolidColorBrush(Color.FromRgb(203, 213, 225)),   // slate
                };
            }
            return new SolidColorBrush(Colors.Transparent);
        }
        public object ConvertBack(object value, Type t, object p, CultureInfo c) => throw new NotImplementedException();
    }

    public class DayStatusToSymbolConverter : IValueConverter
    {
        public object Convert(object value, Type t, object p, CultureInfo c)
        {
            if (value is DayStatus status)
            {
                return status switch
                {
                    DayStatus.Completed => "✓",
                    DayStatus.Partial   => "◑",
                    DayStatus.HasTasks  => "●",
                    _                   => "",
                };
            }
            return "";
        }
        public object ConvertBack(object value, Type t, object p, CultureInfo c) => throw new NotImplementedException();
    }

    public class SelectedDayBorderConverter : IValueConverter
    {
        public object Convert(object value, Type t, object p, CultureInfo c)
            => value is bool b && b ? 2.5 : 0.0;
        public object ConvertBack(object value, Type t, object p, CultureInfo c) => throw new NotImplementedException();
    }

    public class StrikethroughConverter : IValueConverter
    {
        public object Convert(object value, Type t, object p, CultureInfo c)
            => value is bool b && b
                ? TextDecorations.Strikethrough
                : null;
        public object ConvertBack(object value, Type t, object p, CultureInfo c) => throw new NotImplementedException();
    }

    /// <summary>null/empty → Brushes.Transparent (buton her zaman hit-testable). "#RRGGBB" → SolidColorBrush.</summary>
    public class HexToSolidBrushConverter : IValueConverter
    {
        public object Convert(object value, Type t, object p, CultureInfo c)
        {
            if (value is string hex && !string.IsNullOrEmpty(hex))
            {
                try
                {
                    var color = (Color)ColorConverter.ConvertFromString(hex);
                    return new SolidColorBrush(color);
                }
                catch { }
            }
            // Transparent: null'dan farklı olarak Border hit-test'e katılır
            return Brushes.Transparent;
        }
        public object ConvertBack(object value, Type t, object p, CultureInfo c) => throw new NotImplementedException();
    }

    /// <summary>Renk atanmışsa Black döner (kontrastlı okuma), atanmamışsa null (üst elementten inherit).</summary>
    public class HexToForegroundConverter : IValueConverter
    {
        public object Convert(object value, Type t, object p, CultureInfo c)
            => value is string hex && !string.IsNullOrEmpty(hex)
                ? Brushes.Black
                : DependencyProperty.UnsetValue;
        public object ConvertBack(object value, Type t, object p, CultureInfo c) => throw new NotImplementedException();
    }
}
