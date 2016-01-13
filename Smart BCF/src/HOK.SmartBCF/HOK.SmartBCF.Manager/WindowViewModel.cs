using HOK.SmartBCF.Schemas;
using HOK.SmartBCF.UserControls;
using HOK.SmartBCF.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace HOK.SmartBCF.Manager
{
    internal class WindowViewModel
    {
        private BCFViewModel bcfViewModel;

        public BCFViewModel BCFView { get { return bcfViewModel; } set { bcfViewModel = value; } }
        
        public WindowViewModel()
        {
            bcfViewModel = new BCFViewModel(false);
        }

    }
}
