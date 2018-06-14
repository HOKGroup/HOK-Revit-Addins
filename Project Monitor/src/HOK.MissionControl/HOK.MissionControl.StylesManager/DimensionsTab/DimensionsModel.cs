using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;

namespace HOK.MissionControl.StylesManager.DimensionsTab
{
    public class DimensionsModel
    {
        private Document _doc { get; set; }

        public DimensionsModel(Document doc)
        {
            _doc = doc;
        }
    }
}
