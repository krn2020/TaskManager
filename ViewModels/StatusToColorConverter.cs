using System;
using Avalonia.Data.Converters;
using Avalonia.Media;
using TaskManager.Models;
using TaskStatus = TaskManager.Models.TaskStatus;

namespace TaskManager.ViewModels
{
    public class StatusToColorConverter : IValueConverter
    {
        public static readonly StatusToColorConverter Instance = new();
        
        public object? Convert(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo culture)
        {
            if (value is TaskStatus status)
            {
                return status switch
                {
                    TaskStatus.Ожидает => new SolidColorBrush(Colors.Yellow),
                    TaskStatus.Выполняется => new SolidColorBrush(Colors.LightGreen),
                    TaskStatus.Завершена => new SolidColorBrush(Colors.Gray),
                    _ => new SolidColorBrush(Colors.White)
                };
            }
            return new SolidColorBrush(Colors.White);
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}