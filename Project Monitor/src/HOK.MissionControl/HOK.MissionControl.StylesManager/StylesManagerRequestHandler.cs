using System;
using System.Threading;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using HOK.MissionControl.StylesManager.Tabs;

namespace HOK.MissionControl.StylesManager
{
    public class StylesManagerRequestHandler : IExternalEventHandler
    {
        public StylesManagerRequest Request { get; set; } = new StylesManagerRequest();
        public object Arg1 { get; set; }

        public string GetName()
        {
            return "Styles Manager External Event";
        }

        public void Execute(UIApplication app)
        {
            try
            {
                switch (Request.Take())
                {
                    case StylesRequestType.None:
                        {
                            return;
                        }
                    case StylesRequestType.FindView:
                        FindView(app);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="app"></param>
        private void FindView(UIApplication app)
        {
            var doc = app.ActiveUIDocument.Document;
            var dw = (DimensionWrapper)Arg1;
            var view = doc.GetElement(dw.OwnerViewId) as View;
            if (view == null) return;

            var dim = doc.GetElement(dw.DimensionId); 
            app.ActiveUIDocument.ActiveView = view;
            app.ActiveUIDocument.ShowElements(dim);
        }
    }

    public class StylesManagerRequest
    {
        private int _request = (int)StylesRequestType.None;

        public StylesRequestType Take()
        {
            return (StylesRequestType)Interlocked.Exchange(ref _request, (int)StylesRequestType.None);
        }

        public void Make(StylesRequestType request)
        {
            Interlocked.Exchange(ref _request, (int)request);
        }
    }

    public enum StylesRequestType
    {
        None,
        FindView
    }
}
