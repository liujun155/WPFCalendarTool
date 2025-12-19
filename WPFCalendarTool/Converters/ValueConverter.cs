using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using WPFCalendarTool.Models;
using Application = System.Windows.Application;
using Brush = System.Windows.Media.Brush;
using Color = System.Windows.Media.Color;

namespace WPFCalendarTool.Converters
{
    public class BoolToVisibilityConverter : IValueConverter
    {
        /// <summary>Converts a value.</summary>
        /// <param name="value">The value produced by the binding source.</param>
        /// <param name="targetType">The type of the binding target property.</param>
        /// <param name="parameter">The converter parameter to use.</param>
        /// <param name="culture">The culture to use in the converter.</param>
        /// <returns>A converted value. If the method returns <see langword="null" />, the valid null value is used.</returns>
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                return boolValue ? Visibility.Visible : Visibility.Collapsed;
            }
            return Visibility.Collapsed;
        }

        /// <summary>Converts a value.</summary>
        /// <param name="value">The value that is produced by the binding target.</param>
        /// <param name="targetType">The type to convert to.</param>
        /// <param name="parameter">The converter parameter to use.</param>
        /// <param name="culture">The culture to use in the converter.</param>
        /// <returns>A converted value. If the method returns <see langword="null" />, the valid null value is used.</returns>
        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return null;
        }
    }

    /// <summary>
    /// 将布尔值转换为字体粗细（True = Bold, False = Normal）
    /// </summary>
    public class BoolToFontWeightConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                return boolValue ? FontWeights.Bold : FontWeights.Normal;
            }
            return FontWeights.Normal;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return null;
        }
    }

    /// <summary>
    /// 根据是否当前月转换前景色透明度（True = 完全不透明, False = 半透明）
    /// </summary>
    public class MonthOpacityConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is bool isCurrentMonth)
            {
                // 当前月份全亮，非当前月份半透明
                if (targetType == typeof(Brush))
                {
                    var color = Application.Current.Resources["ForegroundBrush"] as SolidColorBrush
                        ?? new SolidColorBrush(Colors.White);

                    var opacity = isCurrentMonth ? 1.0 : 0.3;
                    var newColor = Color.FromArgb(
                        (byte)(color.Color.A * opacity),
                        color.Color.R,
                        color.Color.G,
                        color.Color.B);

                    return new SolidColorBrush(newColor);
                }

                // 如果目标类型是 double（Opacity 属性）
                if (targetType == typeof(double))
                {
                    return isCurrentMonth ? 1.0 : 0.3;
                }
            }

            return Application.Current.Resources["ForegroundBrush"] ?? new SolidColorBrush(Colors.White);
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return null;
        }
    }

    /// <summary>
    /// 根据是否周末转换前景色（True = 红色, False = 默认颜色）
    /// </summary>
    public class WeekendColorConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is bool isWeekend)
            {
                return isWeekend
                    ? new SolidColorBrush(Color.FromRgb(0xFF, 0x6B, 0x6B))
                    : Application.Current.Resources["ForegroundBrush"] ?? new SolidColorBrush(Colors.White);
            }
            return Application.Current.Resources["ForegroundBrush"] ?? new SolidColorBrush(Colors.White);
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return null;
        }
    }

    /// <summary>
    /// 假日类型转换为文本标记（休/班）
    /// </summary>
    public class HolidayTypeToTextConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is HolidayType holidayType)
            {
                return holidayType switch
                {
                    HolidayType.Holiday => "休",
                    HolidayType.WorkDay => "班",
                    HolidayType.RestDay => "休",
                    _ => string.Empty
                };
            }
            return string.Empty;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return null;
        }
    }

    /// <summary>
    /// 假日类型转换为背景色
    /// </summary>
    public class HolidayTypeToColorConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is HolidayType holidayType)
            {
                return holidayType switch
                {
                    HolidayType.Holiday => new SolidColorBrush(Color.FromArgb(200, 255, 107, 107)), // 红色半透明
                    HolidayType.WorkDay => new SolidColorBrush(Color.FromArgb(200, 100, 181, 246)), // 蓝色半透明
                    HolidayType.RestDay => new SolidColorBrush(Color.FromArgb(150, 158, 158, 158)), // 灰色半透明（可选）
                    _ => new SolidColorBrush(Colors.Transparent)
                };
            }
            return new SolidColorBrush(Colors.Transparent);
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return null;
        }
    }

    /// <summary>
    /// 假日类型可见性转换器
    /// </summary>
    public class HolidayTypeToVisibilityConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is HolidayType holidayType)
            {
                // 只显示法定节假日和调休工作日的角标
                return (holidayType == HolidayType.Holiday || holidayType == HolidayType.WorkDay)
                    ? Visibility.Visible
                    : Visibility.Collapsed;
            }
            return Visibility.Collapsed;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
