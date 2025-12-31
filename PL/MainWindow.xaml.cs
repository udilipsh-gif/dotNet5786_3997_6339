using PL;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Configuration;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace PL;

public partial class MainWindow : Window
{

    /// <summary>
    /// Initializes a new instance of the MainWindow class.
    /// </summary>
    public MainWindow()
    {
        InitializeComponent();

    } 

    /// <summary>
    /// Closes all open windows except the main window (MainWindow).
    /// </summary>
    /// <remarks>
    /// Iterates through all currently open application windows and closes any window
    /// that is not the main window. Used when performing database operations to ensure
    /// no stale data is displayed in other windows.
    /// </remarks>
    private void CloseAllWindowsExceptMain()
    {
        foreach (Window window in Application.Current.Windows)
        {
            if (window != this && window.GetType() != typeof(MainWindow))
            {
                window.Close();
            }
        }
    }


    private void btnManagerWindow_Click(object sender, RoutedEventArgs e)
    {
        OpenOrActivateWindow<ManagerWindow>();
    }

    private void OpenOrActivateWindow<T>() where T : Window, new()
    {
        // חיפוש חלון פתוח מהסוג המבוקש באוסף החלונות של האפליקציה
        var existingWindow = Application.Current.Windows.OfType<T>().FirstOrDefault();

        if (existingWindow != null)
        {
            // אם החלון כבר קיים:
            // 1. אם הוא ממוזער - נחזיר אותו למצב רגיל
            if (existingWindow.WindowState == WindowState.Minimized)
            {
                existingWindow.WindowState = WindowState.Normal;
            }

            // 2. נביא אותו לקדמת המסך (פוקוס)
            existingWindow.Activate();
        }
        else
        {
            // אם החלון לא קיים - ניצור מופע חדש ונציג אותו
            var newWindow = new T();
            newWindow.Show(); // שימוש ב-Show לא חוסם את החלון הראשי
        }
    }


    private void MainWindow_Close(object sender, System.EventArgs e)
    {
        CloseAllWindowsExceptMain();
    }
}
