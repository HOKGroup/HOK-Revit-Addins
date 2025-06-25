using HOK.Core.Utilities;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.DB.ExtensibleStorage;

namespace HOK.Core.BackgroundTasks
{
    public static class Rules
    {
        const string HOK_PRINT_SET_PARAM_NAME = "Add ALL Sheets to HOK Print Set";
        const string HOK_PRINT_SET_NAME = "zz_HOK Automated Print Set";

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
                    if (!doc.IsModifiable)
                    {
                        tr.Start();
                    }
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
                    if (tr.GetStatus() == TransactionStatus.Started)
                    {
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
            // Store the original settings to restore later
            PrintRange origRange = pm.PrintRange;

            // Ensure that the print manager setting is set to selected views
            if (pm.PrintRange != PrintRange.Select)
            {
                pm.PrintRange = PrintRange.Select;
            }

            ViewSheetSetting origVSS = pm.ViewSheetSetting;
            ViewSheetSetting existingVSS = pm.ViewSheetSetting;
            // Save the previously selected viewsheet set so we can set it back to the original at the end
            var existingViewSheetSet = existingVSS.CurrentViewSheetSet;

            var hokSheetSet = new FilteredElementCollector(doc)
                .OfClass(typeof(ViewSheetSet))
                .Cast<ViewSheetSet>();

            if (hokSheetSet.Select(ss => ss.Name).Contains(HOK_PRINT_SET_NAME))
            {
                using (Transaction tr = new Transaction(doc, "Set Current View Sheet Set"))
                {
                    if (!doc.IsModifiable)
                    {
                        tr.Start();
                    }
                    existingVSS.CurrentViewSheetSet = hokSheetSet.First(
                    ss => ss.Name == HOK_PRINT_SET_NAME);
                    if (tr.GetStatus() == TransactionStatus.Started)
                    {
                        tr.Commit();
                    }
                }
            }

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
                if (!doc.IsModifiable)
                {
                    tr.Start();
                }
                try
                {
                    if (hokSheetSet != null && ((ViewSheetSet)existingVSS.CurrentViewSheetSet).Name == HOK_PRINT_SET_NAME)
                    {
                        existingVSS.CurrentViewSheetSet.Views = newVSS;
                        existingVSS.Save();
                    }
                    else
                    {
                        existingVSS.CurrentViewSheetSet.Views = newVSS;
                        existingVSS.SaveAs(HOK_PRINT_SET_NAME);
                    }
                }
                catch (Exception ex)
                {
                    existingVSS.CurrentViewSheetSet.Views = newVSS;
                    existingVSS.SaveAs(HOK_PRINT_SET_NAME);
                }
                if (tr.GetStatus() == TransactionStatus.Started)
                    tr.Commit();
            }

            // Part 2: Automatically enable the additional workset so that it's included in the published set list

            // Get the schema ExportViewSheetSetListWith64BitId

            var schemas = Schema.ListSchemas();

            var exportListSchema = Schema.ListSchemas().First(s => s.SchemaName == "ExportViewSheetSetListSchemaWith64BitId");

            // Get the ExportViewViewSheetSetIdList field
            var exportSheetSetIdList = exportListSchema.GetField("ExportViewViewSheetSetIdList");

            // Get the entity
            Entity entity = doc.ProjectInformation.GetEntity(exportListSchema);
            if (entity.Schema == null)
            {
                entity = new Entity(exportListSchema);
            }

            // Get the enabled view sheets sets for publishing
            IList<ElementId> viewSheetSetIds;
            try
            {
                viewSheetSetIds = entity.Get<IList<ElementId>>(exportSheetSetIdList);
            }
            catch
            {
                viewSheetSetIds = new List<ElementId>();
            }


            // Add the additional view sheet set, first get the view sheet set id
            var existingViewSheetSetId = new FilteredElementCollector(doc)
                .OfClass(typeof(ViewSheetSet))
                .Cast<ViewSheetSet>()
                .FirstOrDefault(vs => vs.Name == HOK_PRINT_SET_NAME)
                .Id;

            // Add the new sheet set to the published list if it isn't in already
            if (!viewSheetSetIds.Contains(existingViewSheetSetId))
            {
                viewSheetSetIds.Add(existingViewSheetSetId);
            }

            // Set it in a transaction
            
            {
                using (Transaction tr = new Transaction(doc, "Set Published Export Sheet Set List"))
                {
                    if (!doc.IsModifiable)
                    { 
                        tr.Start();
                    }
                    try
                    {
                        entity.Set(exportSheetSetIdList, viewSheetSetIds);
                        doc.ProjectInformation.SetEntity(entity);
                    }
                    catch (Exception ex)
                    {
                        _ = ex.Message;
                    }
                    if (tr.GetStatus() == TransactionStatus.Started)
                    {
                        tr.Commit();
                    }
                }
            }

            // Reset the original print range settings in the print manager
            using (Transaction tr = new Transaction(doc, "Reset Original Print Settings"))
            {
                if (!doc.IsModifiable)
                {
                    tr.Start();
                }
                pm.ViewSheetSetting.CurrentViewSheetSet = origVSS.CurrentViewSheetSet;
                pm.PrintRange = origRange;
                if (tr.GetStatus() == TransactionStatus.Started)
                {
                    tr.Commit();
                }
            }
        }

        public static void PreventPartialCADExplosions(object sender, Autodesk.Revit.UI.Events.ExecutedEventArgs arg)
        {
            TaskDialog.Show(
                "Partial Explode Cancelled",
                "Exploding a CAD file can drastically increase file size and is not recommended."
            );
        }
        public static void PreventFullCADExplosions(object sender, Autodesk.Revit.UI.Events.ExecutedEventArgs arg)
        {
            TaskDialog.Show(
                "Full Explode Cancelled",
                "Exploding a CAD file can drastically increase file size and is not recommended."
            );
        }
    }
}
