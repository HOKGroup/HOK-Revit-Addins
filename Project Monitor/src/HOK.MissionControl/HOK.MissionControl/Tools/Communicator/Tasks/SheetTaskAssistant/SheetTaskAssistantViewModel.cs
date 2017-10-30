using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GalaSoft.MvvmLight;

namespace HOK.MissionControl.Tools.Communicator.Tasks.SheetTaskAssistant
{
    public class SheetTaskAssistantViewModel: ViewModelBase
    {
        public SheetTaskAssistantModel Model { get; set; }

        public SheetTaskAssistantViewModel(SheetTaskAssistantModel model)
        {
            Model = model;
        }
    }
}
