using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.Revit.UI;
using Autodesk.Revit.DB;
using Autodesk.Revit.ApplicationServices;
using HOK.AVFManager.GenericForms;
using System.Windows.Forms;
using HOK.AVFManager.GenericClasses;

namespace HOK.AVFManager
{
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Automatic)]
    [Autodesk.Revit.Attributes.Regeneration(Autodesk.Revit.Attributes.RegenerationOption.Manual)]
    public class Command:IExternalCommand
    {
        static AddInId m_appId = new AddInId(new Guid("52ED254A-B0C7-4D4D-B5B0-11A93DC42BA0"));
        private Autodesk.Revit.ApplicationServices.Application application;
        private Document doc;
        private Dictionary<Category, bool> visDictionary = new Dictionary<Category, bool>();
        private MainForm mainForm;
        private AnalysisCategory analysisCat;
        private SettingProperties settings;
        private bool pickFace = false;
        private bool pickRef = false;
        private string[] skipCategories = new string[] { "Rooms", "Areas", "Spaces" };

        public Result Execute(ExternalCommandData commandData, ref string message, Autodesk.Revit.DB.ElementSet elements)
        {
            application = commandData.Application.Application;
            doc = commandData.Application.ActiveUIDocument.Document;
            StoreOriginalVisibility(doc.ActiveView);

            CommandForm commandForm=new CommandForm();
            if(DialogResult.OK==commandForm.ShowDialog())
            {
                analysisCat = commandForm.SelectedAnalysisCategory;
                commandForm.Close();

                mainForm = new MainForm(doc, analysisCat);
                if (mainForm.FoundStyles)
                {
                    if (DialogResult.OK == mainForm.ShowDialog())
                    {
                        DisplayManinForm();
                    }
                }
            }
            return Result.Succeeded;
        }

        private void DisplayManinForm()
        {
            settings = mainForm.CurrentSettingProperites;
            analysisCat = mainForm.CurrentAnalysisCategory;
            pickFace = mainForm.IsPickFace;
            pickRef = mainForm.IsPickRef;
            mainForm.Close();

            if (pickRef)
            {
                mainForm = new MainForm(doc, analysisCat, settings);
                mainForm.PickReference(settings.RefCategory);
            }
            else
            {
                if (!skipCategories.Contains(settings.CategoryName)) { IsolateCategory(doc.ActiveView, settings.CategoryName); }
                mainForm = new MainForm(doc, analysisCat, settings);
                if (pickFace) { mainForm.PickFaces(settings.CategoryName); }
                else { mainForm.PickElements(settings.CategoryName); }
                if (!skipCategories.Contains(settings.CategoryName)) { ResetVisibility(doc.ActiveView); }
            }

            if (DialogResult.OK == mainForm.ShowDialog())
            {
                DisplayManinForm();
            }
        }

        public void StoreOriginalVisibility(Autodesk.Revit.DB.View view)
        {
            foreach (Category category in doc.Settings.Categories)
            {
                if (category.get_AllowsVisibilityControl(view))
                {
                    bool visiblity = category.get_Visible(view);
                    visDictionary.Add(category, visiblity);
                }
            }
        }

        public void IsolateCategory(Autodesk.Revit.DB.View view, string categoryName)
        {
            foreach (Category category in visDictionary.Keys)
            {
                if (category.Name == categoryName)
                {
#if RELEASE2013
                    view.setVisibility(category, true);
#else
                    view.SetVisibility(category, true);
#endif
                }
                else
                {
#if RELEASE2013
                    view.setVisibility(category, false);
#else
                    view.SetVisibility(category, false);
#endif
                }
            }
        }
        public void ResetVisibility(Autodesk.Revit.DB.View view)
        {
            foreach (Category category in visDictionary.Keys)
            {
#if RELEASE2013
                view.setVisibility(category, visDictionary[category]);
#else
                view.SetVisibility(category, visDictionary[category]);
#endif
            }
        }
    }
}
