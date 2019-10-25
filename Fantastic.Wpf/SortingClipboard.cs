using System.Windows;
using System.Windows.Controls;
using Fantastic.Utils;

namespace Fantastic.Wpf
{
    public static class SortingClipboard
    {

        public static readonly DependencyProperty SortAndClipboardPathProperty =
            DependencyProperty.RegisterAttached(
                "SortAndClipboardPath",
                typeof(string),
                typeof(DataGridColumn),
                null
                );


        public static void SetSortAndClipboardPath(DataGridColumn element, string value)
        {
            element.SetValue(SortAndClipboardPathProperty, value);
        }

        public static string GetSortAndClipboardPath(DataGridColumn element)
        {
            return (string)element.GetValue(SortAndClipboardPathProperty);
        }
    }

}
