using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Runtime.ExceptionServices;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;
using HOK.Core.Utilities;

namespace HOK.Core.BackgroundTasks
{
    public static class Rules
    {
        const string HOK_PRINT_SET_PARAM_NAME = "Add ALL Sheets to HOK Print Set";
        const string HOK_PRINT_SET_NAME = "_HOK Automated Print Set";

        public static void AddAllSheetsToPrintSet(Document doc)
        {
            // First ensure that the HOK_PRINT_SET_PARAM_NAME parameter exists and is enabled
            var hokPrintSetParamId = GlobalParametersManager.FindByName(
                doc,
                HOK_PRINT_SET_PARAM_NAME
            );

            // If parameter is not found, create it
            if (hokPrintSetParamId == ElementId.InvalidElementId)
            {
                // Creating the parameter
                using (Transaction tr = new Transaction(doc, "Create HOK Print Set Parameter"))
                {
                    if (doc.IsModifiable)
                    {
                        GlobalParameter hokPrintSetParam = GlobalParameter.Create(
                            doc,
                            HOK_PRINT_SET_PARAM_NAME,
#if REVIT2022_OR_GREATER
                            SpecTypeId.Boolean.YesNo
#else
                            ParameterType.YesNo
#endif
                        );
                        // Set the default value to on/true
                        hokPrintSetParam.SetValue(new IntegerParameterValue(1));
                    }
                    else
                    {
                        tr.Start();
                        GlobalParameter hokPrintSetParam = GlobalParameter.Create(
                            doc,
                            HOK_PRINT_SET_PARAM_NAME,
#if REVIT2022_OR_GREATER
                            SpecTypeId.Boolean.YesNo
#else
                            ParameterType.YesNo
#endif
                        );
                        // Set the default value to on/true
                        hokPrintSetParam.SetValue(new IntegerParameterValue(1));
                        tr.Commit();
                    }
                }
            }
            hokPrintSetParamId = GlobalParametersManager.FindByName(
                doc,
                HOK_PRINT_SET_PARAM_NAME
            );
            try
            {
                GlobalParameter hokPrintSetParameter = doc.GetElement(hokPrintSetParamId) as GlobalParameter;
                if ((hokPrintSetParameter.GetValue() as IntegerParameterValue).Value != 1)
                {
                    return;
                }
            }
            catch (Exception ex)
            {
                Log.AppendLog(LogMessageType.EXCEPTION, "Failed to get Global Parameter \"Add ALL Sheets to HOK Print Set\". Exception message: " + ex.Message);
            }

            PrintManager pm = doc.PrintManager;

            // Ensure that the print manager setting is set to selected views
            if (pm.PrintRange != PrintRange.Select)
            {
                pm.PrintRange = PrintRange.Select;
            }

            ViewSheetSetting existingVSS = pm.ViewSheetSetting;
            // Save the previously selected viewsheet set so we can set it back to the original at the end
            var existingViewSheetSet = existingVSS.CurrentViewSheetSet;

            var hokSheetSet = new FilteredElementCollector(doc)
                .OfClass(typeof(ViewSheetSet))
                .Cast<ViewSheetSet>();

            if (hokSheetSet.Select(ss => ss.Name).Contains(HOK_PRINT_SET_NAME))
                existingVSS.CurrentViewSheetSet = hokSheetSet.First(
                    ss => ss.Name == HOK_PRINT_SET_NAME
                );

            List<ElementId> sheetsInModel = new FilteredElementCollector(doc)
                .OfClass(typeof(ViewSheet))
                .ToElements()
                .Cast<ViewSheet>()
                .Select(s => s.Id)
                .ToList();

            // Add the excluded sheets to the sheet set
            ViewSet newVSS = new ViewSet();
            foreach (var viewId in sheetsInModel)
            {
                View currView = (View)doc.GetElement(viewId);
                if (!newVSS.Contains(currView))
                {
                    newVSS.Insert(currView);
                }
            }
            using (Transaction tr = new Transaction(doc, "Add Sheets to View Sheet Set"))
            {
                if (doc.IsModifiable)
                {
                    if (
                        hokSheetSet != null
                        && ((ViewSheetSet)existingVSS.CurrentViewSheetSet).Name
                            == HOK_PRINT_SET_NAME
                    )
                    {
                        try
                        {
                            existingVSS.CurrentViewSheetSet.Views = newVSS;
                            existingVSS.Save();
                        }
                        catch (Exception ex)
                        {
                            Log.AppendLog(LogMessageType.EXCEPTION, "Failed to set sheets to print set \"_HOK Automated Print Set\". Exception message: " + ex.Message);
                        }
                    }
                    else
                    {
                        existingVSS.CurrentViewSheetSet.Views = newVSS;
                        existingVSS.SaveAs(HOK_PRINT_SET_NAME);
                    }
                }
                else
                {
                    tr.Start();
                    if (
                        hokSheetSet != null
                        && ((ViewSheetSet)existingVSS.CurrentViewSheetSet).Name
                            == HOK_PRINT_SET_NAME
                    )
                    {
                        try
                        {
                            existingVSS.CurrentViewSheetSet.Views = newVSS;
                            existingVSS.Save();
                        }
                        catch { }
                    }
                    else
                    {
                        existingVSS.CurrentViewSheetSet.Views = newVSS;
                        existingVSS.SaveAs(HOK_PRINT_SET_NAME);
                    }
                    tr.Commit();
                }
            }
        }
    }
}
