using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace HOK.SmartBCF.Utils
{
    public static class RoutedCommands
    {
        public static readonly RoutedUICommand MoveForward = new RoutedUICommand("MoveForward", "MoveForward", typeof(UserControl));
        public static readonly RoutedUICommand MoveBackward = new RoutedUICommand("MoveBackward", "MoveBackward", typeof(UserControl));
        public static readonly RoutedUICommand IsolateElement = new RoutedUICommand("IsolateElement", "IsolateElement", typeof(UserControl));
        public static readonly RoutedUICommand HighlightElement = new RoutedUICommand("HighlightElement", "HighlightElement", typeof(UserControl));
        public static readonly RoutedUICommand PlaceSectionBox = new RoutedUICommand("PlaceSectionBox", "PlaceSectionBox", typeof(UserControl));
        public static readonly RoutedUICommand WriteParameters = new RoutedUICommand("WriteParameters", "WriteParameters", typeof(UserControl));
    }
}
