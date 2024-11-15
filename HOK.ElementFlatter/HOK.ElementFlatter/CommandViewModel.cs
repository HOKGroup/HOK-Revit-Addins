using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using HOK.Core.Utilities;
using HOK.Core.WpfUtilities;
using HOK.ElementFlatter.Class;
using HOK.ElementFlatter.Commands;
using RelayCommand = CommunityToolkit.Mvvm.Input.RelayCommand;

namespace HOK.ElementFlatter
{
    public class CommandViewModel : INotifyPropertyChanged
    {
        private readonly UIApplication m_app;
        private readonly Document m_doc;
        private string statusText = "Ready";
        private ObservableCollection<CategoryInfo> categories = new ObservableCollection<CategoryInfo>();
        private readonly BuiltInCategory[] categoriesToSkip = {
            BuiltInCategory.OST_Areas, 
            BuiltInCategory.OST_CurtainWallPanels, 
            BuiltInCategory.OST_CurtainWallMullions,
            BuiltInCategory.OST_DetailComponents,
            BuiltInCategory.OST_HVAC_Zones,
            BuiltInCategory.OST_ShaftOpening, 
            BuiltInCategory.OST_StructuralFramingSystem
        };
 
        private readonly RelayCommand flattenCategoryCommand;
        private readonly RelayCommand flattenModelCommand;
        private readonly RelayCommand checkAllCommand;
        private readonly RelayCommand uncheckAllCommand;

        public string StatusText
        {
            get => statusText;
            set { statusText = value; NotifyPropertyChanged("StatusText"); }
        }
        public ObservableCollection<CategoryInfo> Categories
        {
            get => categories;
            set { categories = value; NotifyPropertyChanged("Categories"); }
        }

        public ICommand FlattenCategoryCommand => flattenCategoryCommand;
        public ICommand FlattenModelCommand => flattenModelCommand;
        public ICommand CheckAllCommand => checkAllCommand;
        public ICommand UncheckAllCommand => uncheckAllCommand;

        public CommandViewModel(UIApplication uiapp)
        {
            m_app = uiapp;
            m_doc = m_app.ActiveUIDocument.Document;

            flattenCategoryCommand = new RelayCommand(FlattenCategoryExecuted);
            flattenModelCommand = new RelayCommand(FlattenModelExecuted);
            checkAllCommand = new RelayCommand(CheckAllExecuted);
            uncheckAllCommand = new RelayCommand(UncheckAllExecuted);

            CollectModelCategories();
        }

        /// <summary>
        /// Collects all model categories. Skip sub-categories and excluded categories.
        /// </summary>
        public void CollectModelCategories()
        {
            categories.Clear();
            try
            {
                var categorySet = m_doc.Settings.Categories;

                var filteredCatIds = ParameterFilterUtilities.GetAllFilterableCategories().ToList();
                foreach (var id in filteredCatIds)
                {
                    try
                    {
#if REVIT2024_OR_GREATER
                        var bltCat = (BuiltInCategory)id.Value;
#else
                        var bltCat = (BuiltInCategory)id.IntegerValue;
#endif
                        if (categoriesToSkip.Contains(bltCat)) { continue; }
                        if (bltCat != BuiltInCategory.INVALID)
                        {
                            var category = categorySet.get_Item(bltCat);
                            if (null != category)
                            {
                                if (null != category.Parent) { continue; } //skip sub-categories
                                if (category.CategoryType != CategoryType.Model) { continue; }
                                if (string.IsNullOrEmpty(category.Name)) { continue; }
                                if (!DirectShape.IsValidCategoryId(id, m_doc)) { continue; }

                                var catInfo = new CategoryInfo(category);
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
                        Log.AppendLog(LogMessageType.EXCEPTION, ex.Message);
                    }
                }

                Categories = new ObservableCollection<CategoryInfo>(categories.OrderBy(o => o.Name).ToList());
            }
            catch (Exception ex)
            {
                Log.AppendLog(LogMessageType.EXCEPTION, ex.Message);
            }
        }

        private void Flatten(IEnumerable<CategoryInfo> cats)
        {
            DirectShapeCreator.shapeLibrary = DirectShapeLibrary.GetDirectShapeLibrary(m_doc);
            foreach (var catInfo in cats)
            {
                using (var tg = new TransactionGroup(m_doc))
                {
                    tg.Start("Flatten " + catInfo.Name);
                    Log.AppendLog(LogMessageType.INFO, "Flatten " + catInfo.Name + " Started");
                    try
                    {
                        var index = categories.IndexOf(catInfo);
                        StatusText = "Flattening " + catInfo.Name + "..";
                        StatusBarManager.InitializeProgress("Flattening " + catInfo.Name + "..", catInfo.ElementIds.Count);

                        foreach (var elementId in catInfo.ElementIds)
                        {
                            StatusBarManager.StepForward();
                            using (var trans = new Transaction(m_doc))
                            {
                                trans.Start("Create Shape");
                                var failureHandlingOptions = trans.GetFailureHandlingOptions();
                                var failureHandler = new FailureHandler();
                                failureHandlingOptions.SetFailuresPreprocessor(failureHandler);
                                failureHandlingOptions.SetClearAfterRollback(true);
                                trans.SetFailureHandlingOptions(failureHandlingOptions);

                                try
                                {
                                    var shapeInfo = DirectShapeCreator.CreateDirectShapes(m_doc, catInfo, elementId);
                                    if (null != shapeInfo)
                                    {
                                        categories[index].CreatedShapes.Add(shapeInfo);
                                    }
                                    trans.Commit();
                                }
                                catch (Exception ex)
                                {
                                    Log.AppendLog(LogMessageType.EXCEPTION, ex.Message);
                                    trans.RollBack();
                                }
                            }
                        }

                        var shapesCreated = categories[index].CreatedShapes.Count;
                        Log.AppendLog(LogMessageType.INFO, shapesCreated + " elements are created as DirectShape in " + catInfo.Name);
                        tg.Assimilate();
                    }
                    catch (Exception ex)
                    {
                        Log.AppendLog(LogMessageType.EXCEPTION, ex.Message);
                        tg.RollBack();
                    }
                }
            }
            StatusBarManager.FinalizeProgress();

            CollectModelCategories();
        }

        /// <summary>
        /// Flatten only selected Categories.
        /// </summary>
        /// <param name="param"></param>
        public void FlattenCategoryExecuted()
        {
            try
            {
                var selectedCategories = categories.Where(x => x.IsChecked).ToList();
                if (!selectedCategories.Any()) return;

                Flatten(selectedCategories);
            }
            catch (Exception ex)
            {
                Log.AppendLog(LogMessageType.EXCEPTION, ex.Message);
            }
        }

        /// <summary>
        /// Flattens entire model.
        /// </summary>
        /// <param name="param"></param>
        public void FlattenModelExecuted()
        {
            try
            {
                Flatten(categories);
            }
            catch (Exception e)
            {
                Log.AppendLog(LogMessageType.EXCEPTION, e.Message);
            }
        }

        /// <summary>
        /// Check all Categories.
        /// </summary>
        /// <param name="param"></param>
        public void CheckAllExecuted()
        {
            try
            {
                foreach (var t in categories)
                {
                    t.IsChecked = true;
                }
            }
            catch (Exception ex)
            {
                Log.AppendLog(LogMessageType.EXCEPTION, ex.Message);
            }
        }

        /// <summary>
        /// Uncheck all Categories.
        /// </summary>
        /// <param name="param"></param>
        public void UncheckAllExecuted()
        {
            try
            {
                foreach (var t in categories)
                {
                    t.IsChecked = false;
                }
            }
            catch (Exception ex)
            {
                Log.AppendLog(LogMessageType.EXCEPTION, ex.Message);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(string info)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(info));
        }
    }
}
