using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace HOK.ColorSchemeEditor
{
    /// <summary>
    /// Interaction logic for RangeWindow.xaml
    /// </summary>
    public partial class RangeWindow : Window
    {
        private double minValue = 0;
        private double maxValue = 0;

        public double MinValue { get { return minValue; } set { minValue = value; } }
        public double MaxValue { get { return maxValue; } set { maxValue = value; } }

        public RangeWindow()
        {
            InitializeComponent();
        }

        private void buttonOK_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (ValidateValues())
                {
                    this.DialogResult = true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to create a new color scheme entry with the range of values.\n"+ex.Message, "New Color Scheme Entry", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private bool ValidateValues()
        {
            bool result = false;
            try
            {
                minValue = 0;
                maxValue = 0;

                if (checkBoxMin.IsChecked == true && checkBoxMax.IsChecked == true)
                {
                    MessageBox.Show("Cannot set the Minimum and Maximum options at the same time.", "Invalid Selection", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return false;
                }

                if (checkBoxMin.IsChecked==true)
                {
                    minValue = double.MinValue;
                }
                else
                {
                    if (!double.TryParse(textBoxMin.Text, out minValue))
                    {
                        MessageBox.Show("Please enter a valid start number for the range.", "Invalid Start Value", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return false;
                    }
                    if (minValue <= double.MinValue)
                    {
                        MessageBox.Show("The entered start number is less than the system minimum value.", "Invalid Start Value", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return false;
                    }
                }

                if (checkBoxMax.IsChecked == true)
                {
                    maxValue = double.MaxValue;
                }
                else
                {
                    if (!double.TryParse(textBoxMax.Text, out maxValue))
                    {
                        MessageBox.Show("Please enter a valid end number for the range.", "Invalid End Value", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return false;
                    }
                    if (maxValue >= double.MaxValue)
                    {
                        MessageBox.Show("The entered end number is greater than the system maximum value.", "Invalid End Value", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return false;
                    }
                }
                
                result = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to validate the range of values.\n"+ex.Message, "Validate Values", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return result;
        }

        private void buttonCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void checkBoxMin_Checked(object sender, RoutedEventArgs e)
        {
            if (checkBoxMin.IsChecked==true)
            {
                textBoxMin.Text = "";
                textBoxMin.IsEnabled = false;
            }
        }

        private void checkBoxMin_Unchecked(object sender, RoutedEventArgs e)
        {
            if (checkBoxMin.IsChecked == false)
            {
                textBoxMin.IsEnabled = true;
            }
        }

        private void checkBoxMax_Checked(object sender, RoutedEventArgs e)
        {
            if (checkBoxMax.IsChecked == true)
            {
                textBoxMax.Text = "";
                textBoxMax.IsEnabled = false;
            }
        }

        private void checkBoxMax_Unchecked(object sender, RoutedEventArgs e)
        {
            if (checkBoxMax.IsChecked == false)
            {
                textBoxMax.IsEnabled = false;
            }
        }

        
    }
}
