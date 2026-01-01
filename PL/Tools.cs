using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace PL;

internal static class Tools
{
    // Attached Property for numeric-only validation
    public static readonly DependencyProperty NumericOnlyProperty =
        DependencyProperty.RegisterAttached(
            "NumericOnly",
            typeof(bool),
            typeof(Tools),
            new PropertyMetadata(false, OnNumericOnlyChanged));

    public static bool GetNumericOnly(DependencyObject obj)
    {
        return (bool)obj.GetValue(NumericOnlyProperty);
    }

    public static void SetNumericOnly(DependencyObject obj, bool value)
    {
        obj.SetValue(NumericOnlyProperty, value);
    }

    private static void OnNumericOnlyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is TextBox textBox)
        {
            if ((bool)e.NewValue)
            {
                textBox.PreviewTextInput += TextBox_PreviewTextInput;
            }
            else
            {
                textBox.PreviewTextInput -= TextBox_PreviewTextInput;
            }
        }
    }

    private static void TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
    {
        e.Handled = !e.Text.All(char.IsDigit);
    }
}
