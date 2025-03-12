using System.Windows;
using Visibility = System.Windows.Visibility;
using System.Collections.Generic;
using System;

namespace HOK.ColorBasedIssueFinder
{
    public static class WindowController
    {
        private static readonly List<Window> Windows = new List<Window>();

        public static bool Focus<T>() where T : Window
        {
            var type = typeof(T);
            foreach (var window in Windows)
            {
                if (window.GetType() == type)
                {
                    if (window.WindowState == WindowState.Minimized)
                        window.WindowState = WindowState.Normal;
                    if (window.Visibility != Visibility.Visible)
                        window.Show();
                    window.Focus();
                    return true;
                }
            }
            return false;
        }

        public static void Show(Window window)
        {
            RegisterWindow(window);
            window.Show();
        }
        public static void Show(Window window, IntPtr handle)
        {
            RegisterWindow(window);
            window.Show();
        }
        public static void Show<T>() where T : Window
        {
            var type = typeof(T);
            foreach (var window in Windows)
            {
                if (window.GetType() == type)
                    window.Show();
            }
        }
        public static void Hide<T>() where T : Window
        {
            var type = typeof(T);
            foreach (var window in Windows)
                if (window.GetType() == type)
                    window.Hide();
        }
        public static void Close<T>() where T : Window
        {
            var type = typeof(T);
            for (var i = Windows.Count - 1; i >=0; i--)
            {
                var window = Windows[i];
                if (window.GetType() == type)
                    window.Close();
            }
        }
        private static void RegisterWindow(Window window)
        {
            Windows.Add(window);
            window.Closed += (sender, _) =>
            {
                var modelessWindow = (Window)sender;
                Windows.Remove(modelessWindow);
            };
        }
    }
}
