using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
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

    public static void SendEmail(string toEmail, string subject, string body)
    {
        try
        {
            MailMessage mail = new MailMessage();
            SmtpClient SmtpServer = new SmtpClient("smtp.gmail.com");

            string fromEmail = "@gmail.com";
            string password = "xxxx xxxx xxxx xxxx"; // סיסמת האפליקציה (16 תווים)

            mail.From = new MailAddress(fromEmail);

            // ולידציה בסיסית למקרה שהמייל ריק
            if (string.IsNullOrWhiteSpace(toEmail))
                throw new Exception("כתובת המייל של הנמען ריקה");

            mail.To.Add(toEmail);
            mail.Subject = subject;
            mail.Body = body;


            // הגדרות שרת
            SmtpServer.Port = 587;
            SmtpServer.Credentials = new NetworkCredential(fromEmail, password);
            SmtpServer.EnableSsl = true;

            SmtpServer.Send(mail);
        }
        catch (Exception ex)
        {

            throw new Exception($"שגיאה בשליחת מייל: {ex.Message}");
        }
    }
}
