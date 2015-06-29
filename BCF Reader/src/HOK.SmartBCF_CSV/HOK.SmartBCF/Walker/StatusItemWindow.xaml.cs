using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using WinForm = System.Windows.Forms;

namespace HOK.SmartBCF.Walker
{
    public enum ColorSource
    {
        None,
        Action,
        Responsibility
    }

    public enum NewOrEdit
    {
        None,
        New,
        Edit
    }
    /// <summary>
    /// Interaction logic for StatusItemWindow.xaml
    /// </summary>
    public partial class StatusItemWindow : Window
    {
        private ColorDefinition colorDefinition = null;
        private ColorSource colorSource = ColorSource.None;
        private NewOrEdit newOrEdit = NewOrEdit.None;
        private List<string> definitionNames=new List<string>();
        private Random random = new Random();

        public ColorDefinition SelColorDefinition { get { return colorDefinition; } set { colorDefinition = value; } }
        public List<string> DefinitionNames { get { return definitionNames; } set { definitionNames = value; } }

        public StatusItemWindow(ColorDefinition definition, ColorSource source, NewOrEdit neworedit)
        {
            colorDefinition = definition;
            newOrEdit = neworedit;
            InitializeComponent();
            this.Title = newOrEdit.ToString() + " " + source.ToString() + " Item";
            groupBox.Header = source.ToString();

            textBoxName.Text = colorDefinition.ParameterValue;
            buttonColor.Background = colorDefinition.BackgroundColor;
        }

        private void buttonColor_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                WinForm.ColorDialog colorDialog = new WinForm.ColorDialog();
                if (WinForm.DialogResult.OK == colorDialog.ShowDialog())
                {
                    System.Drawing.Color color = colorDialog.Color;
                    colorDialog.Dispose();

                    colorDefinition.Color[0] = color.R;
                    colorDefinition.Color[1] = color.G;
                    colorDefinition.Color[2] = color.B;

                    System.Windows.Media.Color windowColor = System.Windows.Media.Color.FromRgb(colorDefinition.Color[0], colorDefinition.Color[1], colorDefinition.Color[2]);
                    colorDefinition.BackgroundColor = new SolidColorBrush(windowColor);

                    buttonColor.Background = colorDefinition.BackgroundColor;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to set color.\n"+ex.Message, "Set Color", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void buttonOK_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!string.IsNullOrEmpty(textBoxName.Text))
                {
                    if (!definitionNames.Contains(textBoxName.Text))
                    {
                        colorDefinition.ParameterValue = textBoxName.Text;
                        this.DialogResult = true;
                    }
                    else
                    {
                        MessageBox.Show(textBoxName.Text + " already exist in "+colorSource.ToString()+" items.", "Name Already Exist", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }                
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to add or edit item.\n"+ex.Message, this.Title, MessageBoxButton.OK, MessageBoxImage.Warning);
            }
           
        }

      
    }
}
