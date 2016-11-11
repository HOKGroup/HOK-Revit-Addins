using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace ARUP.IssueTracker.UserControls
{
    /// <summary>
    /// Interaction logic for FilterGroup.xaml
    /// </summary>
    public partial class FilterGroup : UserControl
    {
        public List<CheckBox> checkboxes = new List<CheckBox>();

        public static readonly DependencyProperty HeaderProperty =
            DependencyProperty.Register("Header", typeof(string), typeof(FilterGroup), new UIPropertyMetadata(""));

        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register("Value", typeof(string), typeof(FilterGroup), new UIPropertyMetadata(""));

        public FilterGroup()
        {
            InitializeComponent();
        }

        public string Value
        {
            get { return (string)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }
        public string Header
        {
            get { return (string)GetValue(HeaderProperty); }
            set { SetValue(HeaderProperty, value); }
        }
        public void Clear()
        {
            foreach (var cb in checkboxes)
            {
                cb.IsChecked = false;
            }
        }
        public void updateList(List<String> names)
        {
            groupb.Header = Header;
            //all.IsChecked = true;
            foreach (var cb in checkboxes)
            {
                statuspanel.Children.Remove(cb);
            }
            checkboxes = new List<CheckBox>();
            foreach (var s in names)
            {

                CheckBox cb = new CheckBox();
                cb.Content = s;
                cb.Margin = new Thickness(2);
                //cb.IsChecked = true;
                cb.Checked += new RoutedEventHandler(cb_Checked);
                cb.Unchecked += new RoutedEventHandler(cb_Unchecked);
                statuspanel.Children.Add(cb);
                checkboxes.Add(cb);
            }
        }
        private void all_Checked(object sender, RoutedEventArgs e)
        {

            foreach (var cb in checkboxes)
            {
                if (!cb.IsChecked.Value)
                    cb.IsChecked = true;
            }
        }
        private void cb_Checked(object sender, RoutedEventArgs e)
        {
            //if (all.IsChecked.Value)
            //    return;
            //bool allck = true;

            //foreach (var cb in checkboxes)
            //{
            //    if (cb.IsChecked == false)
            //        allck = false;
            //}
            //all.IsChecked = allck;


        }
        private void cb_Unchecked(object sender, RoutedEventArgs e)
        {
            //all.IsChecked = false;
        }

        private void all_Unchecked(object sender, RoutedEventArgs e)
        {
            //foreach (var cb in checkboxes)
            //{
            //    cb.IsChecked = false;
            //}
        }
        public string Result
        {
            get
            {
                string returnfilter = "";
                //status

                int tot = 0;
                foreach (var cb in checkboxes)
                {
                    if (cb.IsChecked.Value)
                        tot++;
                }
                if (tot > 0)
                {
                    returnfilter = "+AND+" + Value + "+in+(";
                    int i = 1;
                    foreach (var cb in checkboxes)
                    {
                        if (cb.IsChecked.Value)
                        {
                            string comma = (i < tot) ? "," : "";
                            returnfilter += "\"" + cb.Content.ToString() + "\"" + comma;
                            i++;
                        }
                    }
                    returnfilter += ")";

                }

                return returnfilter;
            }

        }
    }
}
