using System.Collections;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace HOK.Core.WpfUtilities
{
    public class DataGridExtension : System.Windows.Controls.DataGrid
    {
        public DataGridExtension()
        {
            SelectionChanged += DataGridEx_SelectionChanged;

            // add event handler for single click editing
            EventManager.RegisterClassHandler(typeof(System.Windows.Controls.DataGrid), PreviewMouseLeftButtonDownEvent,
                new RoutedEventHandler(EventHelper.DataGridPreviewMouseLeftButtonDownEvent));
        }

        private void DataGridEx_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SelectedItemsList = SelectedItems;
        }

        public IList SelectedItemsList
        {
            get => (IList)GetValue(SelectedItemsListProperty);
            set => SetValue(SelectedItemsListProperty, value);
        }

        public static readonly DependencyProperty SelectedItemsListProperty =
            DependencyProperty.Register("SelectedItemsList", typeof(IList<>), typeof(DataGridExtension), new PropertyMetadata(null));
    }

    /// <summary>
    /// Event helper for handling left mouse button click so that it goes into EditMode.
    /// </summary>
    public static class EventHelper
    {
        internal static void DataGridPreviewMouseLeftButtonDownEvent
            (object sender, RoutedEventArgs e)
        {
            var mbe = e as MouseButtonEventArgs;

            DependencyObject obj = null;
            if (mbe != null)
            {
                obj = mbe.OriginalSource as DependencyObject;
                while (obj != null && !(obj is DataGridCell))
                {
                    obj = VisualTreeHelper.GetParent(obj);
                }
            }

            DataGridCell cell = null;

            if (obj != null)
                cell = obj as DataGridCell;

            if (cell == null || cell.IsEditing || cell.IsReadOnly) return;
            if (!cell.IsFocused)
            {
                cell.Focus();
            }
            var dataGrid = FindVisualParent<System.Windows.Controls.DataGrid>(cell);
            if (dataGrid == null) return;
            if (dataGrid.SelectionUnit
                != DataGridSelectionUnit.FullRow)
            {
                if (!cell.IsSelected)
                    cell.IsSelected = true;
            }
            else
            {
                var row = FindVisualParent<DataGridRow>(cell);
                if (row != null && !row.IsSelected)
                {
                    row.IsSelected = true;
                }
            }
        }

        private static T FindVisualParent<T>(UIElement element) where T : UIElement
        {
            var parent = element;
            while (parent != null)
            {
                var correctlyTyped = parent as T;
                if (correctlyTyped != null)
                {
                    return correctlyTyped;
                }

                parent = VisualTreeHelper.GetParent(parent) as UIElement;
            }
            return null;
        }
    }
}
