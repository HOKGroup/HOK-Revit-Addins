using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using HOK.ElementFlatter.Class;
using HOK.ElementFlatter.Commands;
using HOK.ElementFlatter.Util;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace HOK.ElementFlatter
{
    public class CommandViewModel : INotifyPropertyChanged
    {
        private UIApplication m_app;
        private Document m_doc;
        private string statusText = "Ready";
        private ObservableCollection<CategoryInfo> categories = new ObservableCollection<CategoryInfo>();
        private BuiltInCategory[] categoriesToSkip = new BuiltInCategory[] 
        {
            BuiltInCategory.OST_Areas, 
            BuiltInCategory.OST_CurtainWallPanels, 
            BuiltInCategory.OST_CurtainWallMullions,
            BuiltInCategory.OST_DetailComponents,
            BuiltInCategory.OST_HVAC_Zones,
            BuiltInCategory.OST_ShaftOpening
        };
 
        private RelayCommand flattenCategoryCommand;
        private RelayCommand flattenModelCommand;
        private RelayCommand checkAllCommand;
        private RelayCommand uncheckAllCommand;

        public string StatusText { get { return statusText; } set { statusText = value; NotifyPropertyChanged("StatusText"); } }
        public ObservableCollection<CategoryInfo> Categories { get { return categories; } set { categories = value; NotifyPropertyChanged("Categories"); } }

        public ICommand FlattenCategoryCommand { get { return flattenCategoryCommand; } }
        public ICommand FlattenModelCommand { get { return flattenModelCommand; } }
        public ICommand CheckAllCommand { get { return checkAllCommand; } }
        public ICommand UncheckAllCommand { get { return uncheckAllCommand; } }
        
        public CommandViewModel(UIApplication uiapp)
        {
            m_app = uiapp;
            m_doc = m_app.ActiveUIDocument.Document;

            flattenCategoryCommand = new RelayCommand(param => this.FlattenCategoryExecuted(param));
            flattenModelCommand = new RelayCommand(param => this.FlattenModelExecuted(param));
            checkAllCommand = new RelayCommand(param => this.CheckAllExecuted(param));
            uncheckAllCommand = new RelayCommand(param => this.UncheckAllExecuted(param));

            CollectModelCategories();
            LogManager.SetLogPath(m_doc);
        }

        public void CollectModelCategories()
        {
            categories.Clear();
            try
            {
                Categories categorySet = m_doc.Settings.Categories;

                List<ElementId> filteredCatIds = ParameterFilterUtilities.GetAllFilterableCategories().ToList();
                foreach (ElementId id in filteredCatIds)
                {
                    try
                    {
                        BuiltInCategory bltCat = (BuiltInCategory)id.IntegerValue;
                        if (categoriesToSkip.Contains(bltCat)) { continue; }
                        if (bltCat != BuiltInCategory.INVALID)
                        {
                            Category category = categorySet.get_Item(bltCat);
                            if (null != category)
                            {
                                if (null != category.Parent) { continue; } //skip sub-categories
                                if (category.CategoryType != CategoryType.Model) { continue; }
                                //if (!category.HasMaterialQuantities) { continue; }
                                if (string.IsNullOrEmpty(category.Name)) { continue; }
                                if (!DirectShape.IsValidCategoryId(id, m_doc)) { continue; }

                                CategoryInfo catInfo = new CategoryInfo(category);
                                catInfo.ElementIds = catInfo.GetElementIds(m_doc);
                                if (catInfo.ElementIds.Count > 0)
                                {
                                    categories.Add(catInfo);
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        string message = ex.Message;
                    }
                }

                this.Categories = new ObservableCollection<CategoryInfo>(categories.OrderBy(o => o.Name).ToList());
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to collect categories.\n" + ex.Message, "Collect Categories", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        public void FlattenCategoryExecuted(object param)
        {
            try
            {
                StringBuilder strBuilder = new StringBuilder();
                var selectedCategories = from cat in categories where cat.IsChecked select cat;
                if (selectedCategories.Count() > 0)
                {
                    LogManager.ClearLog();
                    LogManager.AppendLog(LogMessageType.INFO,"Element Flatter Started");
                    foreach (CategoryInfo catInfo in selectedCategories)
                    {
                        using (TransactionGroup tg = new TransactionGroup(m_doc))
                        {
                            tg.Start("Flatten " + catInfo.Name);
                            LogManager.AppendLog(LogMessageType.INFO, "Flatten " + catInfo.Name + " Started");
                            try
                            {
                                int index = categories.IndexOf(catInfo);
                                this.StatusText = "Flattening " + catInfo.Name + "..";
                                ProgressManager.InitializeProgress("Flattening " + catInfo.Name + "..", catInfo.ElementIds.Count);

                                foreach (ElementId elementId in catInfo.ElementIds)
                                {
                                    ProgressManager.StepForward();
                                    using (Transaction trans = new Transaction(m_doc))
                                    {
                                        trans.Start("Create Shape");
                                        FailureHandlingOptions failureHandlingOptions = trans.GetFailureHandlingOptions();
                                        FailureHandler failureHandler = new FailureHandler();
                                        failureHandlingOptions.SetFailuresPreprocessor(failureHandler);
                                        failureHandlingOptions.SetClearAfterRollback(true);
                                        trans.SetFailureHandlingOptions(failureHandlingOptions);

                                        try
                                        {
                                            DirectShapeInfo shapeInfo = DirectShapeCreator.CreateDirectShapes(m_doc, catInfo, elementId);
                                            if (null != shapeInfo)
                                            {
                                                categories[index].CreatedShapes.Add(shapeInfo);
                                            }
                                            trans.Commit();
                                        }
                                        catch (Exception ex)
                                        {
                                            string message = ex.Message;
                                            trans.RollBack();
                                        }
                                    }
                                }

                                int shapesCreated = categories[index].CreatedShapes.Count;
                                LogManager.AppendLog(LogMessageType.INFO, shapesCreated + " elements are created as DirectShape in " + catInfo.Name);
                                tg.Assimilate();
                            }
                            catch (Exception ex)
                            {
                                tg.RollBack();
                                string message = ex.Message;
                            }
                        }
                    }
                    ProgressManager.FinalizeProgress();
                    LogManager.AppendLog(LogMessageType.INFO, "Finished");
                    LogManager.WriteLog();

                    CollectModelCategories();
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
        }

        public void FlattenModelExecuted(object param)
        {
            try
            {
                LogManager.ClearLog();
                LogManager.AppendLog(LogMessageType.INFO, "Element Flatter Started");
                bool sequenced = MakeSequenceOfCategories();
                using (TransactionGroup tg = new TransactionGroup(m_doc))
                {
                    tg.Start("Flatten Model");
                    try
                    {
                        foreach (CategoryInfo catInfo in categories)
                        {
                            LogManager.AppendLog(LogMessageType.INFO, "Flatten " + catInfo.Name + " Started");
                            int index = categories.IndexOf(catInfo);
                            this.StatusText = "Flattening " + catInfo.Name + "..";
                            ProgressManager.InitializeProgress("Flattening " + catInfo.Name + "..", catInfo.ElementIds.Count);

                            foreach (ElementId elementId in catInfo.ElementIds)
                            {
                                ProgressManager.StepForward();
                                using (Transaction trans = new Transaction(m_doc))
                                {
                                    trans.Start("Create Shape");
                                    FailureHandlingOptions failureHandlingOptions = trans.GetFailureHandlingOptions();
                                    FailureHandler failureHandler = new FailureHandler();
                                    failureHandlingOptions.SetFailuresPreprocessor(failureHandler);
                                    failureHandlingOptions.SetClearAfterRollback(true);
                                    trans.SetFailureHandlingOptions(failureHandlingOptions);

                                    try
                                    {
                                        DirectShapeInfo shapeInfo = DirectShapeCreator.CreateDirectShapes(m_doc, catInfo, elementId);
                                        if (null != shapeInfo)
                                        {
                                            categories[index].CreatedShapes.Add(shapeInfo);
                                        }
                                        trans.Commit();
                                    }
                                    catch (Exception ex)
                                    {
                                        string message = ex.Message;
                                        trans.RollBack();
                                    }
                                }
                            }

                            int shapesCreated = categories[index].CreatedShapes.Count;
                            LogManager.AppendLog(LogMessageType.INFO, shapesCreated + " elements are created as DirectShape in " + catInfo.Name);
                        }
                        tg.Assimilate();
                    }
                    catch (Exception ex)
                    {
                        tg.RollBack();
                        string message = ex.Message;
                        LogManager.AppendLog(LogMessageType.EXCEPTION, ex.Message);
                    }
                }
                
                ProgressManager.FinalizeProgress();
                LogManager.AppendLog(LogMessageType.INFO, "Finished");
                LogManager.WriteLog();

                CollectModelCategories();
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
        }

        private bool MakeSequenceOfCategories()
        {
            bool sequenced = false;
            try
            {
                BuiltInCategory[] prioritizedCategories = new BuiltInCategory[]
                {
                    BuiltInCategory.OST_Rooms, BuiltInCategory.OST_Walls, BuiltInCategory.OST_Floors, BuiltInCategory.OST_Roofs, BuiltInCategory.OST_Ceilings
                };

                for (int i = prioritizedCategories.Length-1; i >-1; i--)
                {
                    var catFound = from cat in categories where cat.BltCategory == prioritizedCategories[i] select cat;
                    if (catFound.Count() > 0)
                    {
                        CategoryInfo catInfo = catFound.First();
                        categories.Remove(catInfo);
                        categories.Insert(0,catInfo);
                    }
                }
                sequenced = true;
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
            return sequenced;
        }

        public void CheckAllExecuted(object param)
        {
            try
            {
                for (int i = 0; i < categories.Count; i++)
                {
                    categories[i].IsChecked = true;
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
        }

        public void UncheckAllExecuted(object param)
        {
            try
            {
                for (int i = 0; i < categories.Count; i++)
                {
                    categories[i].IsChecked = false;
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }
    }
}
