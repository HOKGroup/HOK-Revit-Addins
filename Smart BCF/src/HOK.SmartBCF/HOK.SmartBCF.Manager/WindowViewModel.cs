using HOK.SmartBCF.Schemas;
using HOK.SmartBCF.UserControls;
using HOK.SmartBCF.Utils;
using HOK.SmartBCF.Windows;
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
        private AddViewModel addViewModel;
        private RelayCommand addViewCommand;

        public BCFViewModel BCFView { get { return bcfViewModel; } set { bcfViewModel = value; } }
        public AddViewModel AddView { get { return addViewModel; } set { addViewModel = value; } }
        public ICommand AddViewCommand { get { return addViewCommand; } }
      
        public WindowViewModel()
        {
            bcfViewModel = new BCFViewModel(false);

            addViewCommand = new RelayCommand(param => this.AddViewCommandExecuted(param));
        }

        public void AddViewCommandExecuted(object param)
        {
            try
            {
                if (addViewModel == null)
                {
                    addViewModel = new AddViewModel(bcfViewModel);
                    AddViewWindow viewWindow = new AddViewWindow();
                    viewWindow.DataContext = addViewModel;
                    viewWindow.Closed += WindowClosed;
                    if ((bool)viewWindow.ShowDialog())
                    {

                    }
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
        }

        public void WindowClosed(object sender, System.EventArgs e)
        {
            addViewModel = null;
        }

    }
}
