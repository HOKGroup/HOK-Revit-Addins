using Autodesk.Revit.DB;
using Autodesk.Revit.DB.ExternalService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HOK.KeynoteManager
{
    public class Application:IExternalDBApplication
    {
        public ExternalDBApplicationResult OnShutdown(Autodesk.Revit.ApplicationServices.ControlledApplication application)
        {
            return ExternalDBApplicationResult.Succeeded;
        }

        public ExternalDBApplicationResult OnStartup(Autodesk.Revit.ApplicationServices.ControlledApplication application)
        {
            try
            {
                ExternalService externalResourceService = ExternalServiceRegistry.GetService(ExternalServices.BuiltInExternalServices.ExternalResourceService);

                if (externalResourceService == null)
                {
                    return ExternalDBApplicationResult.Failed;
                }

                IExternalResourceServer keynoteServer = new KeynoteServer(application);
                externalResourceService.AddServer(keynoteServer);
                return ExternalDBApplicationResult.Succeeded;
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
            return ExternalDBApplicationResult.Succeeded;
        }
    }
}
