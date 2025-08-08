using Avalonia.Controls;
using Avalonia.Data.Converters;
using Avalonia.Media;
using System;
using System.Globalization;

namespace AvaloniaTest.Converters
{
    public class AlternatingRowColorConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is ListBoxItem item)
            {
                var parent = ItemsControl.ItemsControlFromItemContainer(item);
                var index = parent?.IndexFromContainer(item) ?? -1;
                return index % 2 == 0 ? Color.Parse("#cfd4e6ff") : Color.Parse("#8f95c9ff");
            }
            return Colors.White;
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}