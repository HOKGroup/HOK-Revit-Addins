using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace HOK.BCFReader.GenericForms
{
    public partial class ImageForm : Form
    {
        private Image snapShot = null;
        public ImageForm(Image image)
        {
            snapShot = image;
            InitializeComponent();
        }

        private void ImageForm_Load(object sender, EventArgs e)
        {
            pictureBox.Image = snapShot;
        }
    }
}
