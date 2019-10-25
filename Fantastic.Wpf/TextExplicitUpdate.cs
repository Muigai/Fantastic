using System.Windows;
using System.Windows.Controls;
using Fantastic.Utils;

namespace Fantastic.Wpf
{
    public static class TextExplicitUpdate
    {

        public static readonly DependencyProperty TextExplicitUpdateValueProperty =
            DependencyProperty.RegisterAttached(
                "TextExplicitUpdateValue",
                typeof(string),
                typeof(TextBox),
                null
                );


        public static void SetTextExplicitUpdateValue(TextBox element, string value)
        {
            element.SetValue(TextExplicitUpdateValueProperty, value);
        }

        public static string GetTextExplicitUpdateValue(TextBox element)
        {
            return (string)element.GetValue(TextExplicitUpdateValueProperty);
        }
    }

}
