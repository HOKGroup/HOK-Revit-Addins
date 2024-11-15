using System;
using System.Windows.Forms;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB.Events;
using Autodesk.Revit.UI;
using Autodesk.Revit.DB;
using HOK.Core.Utilities;
using HOK.MissionControl.Core.Schemas;
using HOK.MissionControl.Core.Utils;
using HOK.RoomsToMass.ParameterAssigner;
using Form = System.Windows.Forms.Form;
using Nice3point.Revit.Toolkit.External;

namespace HOK.RoomsToMass
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    [Journaling(JournalingMode.NoCommandData)]
    public class AssignerCommand : ExternalCommand
    {
        private UIApplication m_app;
        private Document m_doc;

        public override void Execute()
        {
            try
            {
                m_app = Context.UiApplication;
                m_doc = Context.ActiveDocument;
                Log.AppendLog(LogMessageType.INFO, "Started");

                // (Konrad) We are gathering information about the addin use. This allows us to
                // better maintain the most used plug-ins or discontiue the unused ones.
                AddinUtilities.PublishAddinLog(
                    new AddinLog("MassTools-MassCommands", Application.VersionNumber));

                m_app.Application.FailuresProcessing += OnFailuresProcessing;
                var linkedFilesForm = new Form_LinkedFiles(m_app);
                if (linkedFilesForm.ShowDialog() == DialogResult.OK)
                {
                    var assignForm = new Form_Assigner(m_app)
                    {
                        WorksetDictionary = linkedFilesForm.WorksetDictionary,
                        IntegratedMassList = linkedFilesForm.IntegratedMassList,
                        LinkedMassDictionary = linkedFilesForm.LinkedMassDictionary,
                        ElementDictionary = linkedFilesForm.ElementDictionary,
                        ElementCategories = linkedFilesForm.ElementCategories,
                        MassParameters = linkedFilesForm.MassParameters,
                        SelectedSourceType = linkedFilesForm.SelectedSourceType,
                        ParameterMaps = linkedFilesForm.ParameterMaps
                    };

                    linkedFilesForm.Close();

                    if (assignForm.ShowDialog() == DialogResult.OK)
                    {
                        assignForm.Close();
                    }
                }

                Log.AppendLog(LogMessageType.INFO, "Ended.");
            }
            catch (Exception ex)
            {
                Log.AppendLog(LogMessageType.EXCEPTION, ex.Message);
                Result = Result.Failed;
            }
        }

        private static void OnFailuresProcessing(object sender, FailuresProcessingEventArgs e)
        {
            var failuresAccessor = e.GetFailuresAccessor();
            var fmas = failuresAccessor.GetFailureMessages();
            if (fmas.Count == 0)
            {
                e.SetProcessingResult(FailureProcessingResult.Continue);
                return;
            }

            foreach (var fma in fmas)
            {
                var severity = fma.GetSeverity();
                try
                {
                    if (severity == FailureSeverity.Warning)
                    {
                        failuresAccessor.DeleteWarning(fma);
                        e.SetProcessingResult(FailureProcessingResult.Continue);
                    }
                    else
                    {
                        e.SetProcessingResult(FailureProcessingResult.ProceedWithRollBack);
                    }
                }
                catch (Exception ex)
                {
                    Log.AppendLog(LogMessageType.EXCEPTION, ex.Message);
                }
            }
        }  
    }
}
