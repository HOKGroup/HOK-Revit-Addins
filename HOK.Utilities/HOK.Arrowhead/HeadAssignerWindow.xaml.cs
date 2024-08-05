using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Threading;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using HOK.Core.Utilities;
using Visibility = System.Windows.Visibility;

namespace HOK.Arrowhead
{
    /// <summary>
    /// Interaction logic for HeadAssignerWindow.xaml
    /// </summary>
    public partial class HeadAssignerWindow
    {
        private readonly UIApplication m_app;
        private readonly Document m_doc;
        private List<Arrowhead> arrowheadList = new List<Arrowhead>();

        public HeadAssignerWindow(UIApplication uiapp)
        {
            m_app = uiapp;
            m_doc = m_app.ActiveUIDocument.Document;

            InitializeComponent();
            CollectArrowheads();
        }

        private void buttonApply_Click(object sender, RoutedEventArgs e)
        {
            if (UpdateLeaders())
            {
                DialogResult = true;
            }
        }

        private delegate void UpdateProgressBarDelegate(DependencyProperty dp, object value);

        private void buttonCancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        /// <summary>
        /// Collects all available Arrowhead Styles in the project.
        /// </summary>
        private void CollectArrowheads()
        {
            try
            {
                var eId = new ElementId(BuiltInParameter.ALL_MODEL_FAMILY_NAME);
                var provider = new ParameterValueProvider(eId);
                FilterStringRuleEvaluator evaluator = new FilterStringEquals();
#if REVIT2022_OR_GREATER
                FilterRule rule = new FilterStringRule(provider, evaluator, "Arrowhead");
#else
                FilterRule rule = new FilterStringRule(provider, evaluator, "Arrowhead", false);
#endif
                var filter = new ElementParameterFilter(rule);
                var leaders = new FilteredElementCollector(m_doc)
                    .OfClass(typeof(ElementType))
                    .WherePasses(filter)
                    .ToElements();

                foreach (var element in leaders)
                {
                    if (string.IsNullOrEmpty(element.Name)) continue;

                    var arrowhead = new Arrowhead(element);
                    arrowheadList.Add(arrowhead);
                }

                var sortedlist = from arrow in arrowheadList orderby arrow.ArrowName select arrow;
                arrowheadList = sortedlist.ToList();

                comboBoxArrow.ItemsSource = arrowheadList;
                comboBoxArrow.DisplayMemberPath = "ArrowName";
                comboBoxArrow.SelectedValuePath = "ArrowElementId";

                if (arrowheadList.Count > 0)
                {
                    comboBoxArrow.SelectedIndex = 0;
                }

            }
            catch (Exception ex)
            {
                Log.AppendLog(LogMessageType.EXCEPTION, ex.Message);
            }
        }

        /// <summary>
        /// Updates all Leader Arrowheads.
        /// </summary>
        private bool UpdateLeaders()
        {
            var result = false;
            try
            {
                var i = 0;
                if (null != comboBoxArrow.SelectedValue)
                {
                    var arrowhead = (Arrowhead)comboBoxArrow.SelectedItem;
                    var selectedId = (ElementId)comboBoxArrow.SelectedValue;

                    // (Konrad) Updates Leader Arrowheads for all FamilySymbols and TextNotes
                    // We need a combined class filter here since TextNotes don't have Families.
                    var symbols = new FilteredElementCollector(m_doc).WherePasses(
                        new LogicalOrFilter(
                            new List<ElementFilter>
                            {
                                new ElementClassFilter(typeof(FamilySymbol)),
                                new ElementClassFilter(typeof(TextNoteType))
                            }))
                            .ToList();

                    progressBar.Visibility = Visibility.Visible;
                    statusLable.Visibility = Visibility.Visible;
                    statusLable.Text = "Applying Changes . .";

                    progressBar.Minimum = 0;
                    progressBar.Maximum = symbols.Count;
                    progressBar.Value = 0;

                    double value = 0;
                    var updatePbDelegate = new UpdateProgressBarDelegate(progressBar.SetValue);

                    using (var t = new Transaction(m_doc))
                    {
                        foreach (var family in symbols)
                        {
                            value += 1;
                            var param = family.get_Parameter(BuiltInParameter.LEADER_ARROWHEAD);
                            if (param != null)
                            {
                                t.Start("Update Leaders");
                                try
                                {
                                    param.Set(selectedId);
                                    i++;
                                    t.Commit();
                                }
                                catch
                                {
                                    Log.AppendLog(LogMessageType.ERROR, "Failed to update Leader. Rolling back.");
                                    t.RollBack();
                                }
                            }
                            Dispatcher.Invoke(updatePbDelegate, DispatcherPriority.Background, System.Windows.Controls.Primitives.RangeBase.ValueProperty, value);
                        }
                    }

                    progressBar.Visibility = Visibility.Hidden;
                    statusLable.Text = "Done";
                    MessageBox.Show(i + " Annotation Symbol Types are updated with " + arrowhead.ArrowName, "Annotation Symbols Types", MessageBoxButton.OK, MessageBoxImage.Information);
                    result = true;
                }
            }
            catch (Exception ex)
            {
                Log.AppendLog(LogMessageType.EXCEPTION, ex.Message);
                result = false;
            }
            return result;
        }
    }

    /// <summary>
    /// Arrowhead wrapper class.
    /// </summary>
    public class Arrowhead
    {
        public Element ArrowElement { get; set; }
        public string ArrowName { get; set; }
        public ElementId ArrowElementId { get; set; }

        public Arrowhead(Element element)
        {
            ArrowElement = element;
            ArrowName = element.Name;
            ArrowElementId = element.Id;
        }
    }
}
