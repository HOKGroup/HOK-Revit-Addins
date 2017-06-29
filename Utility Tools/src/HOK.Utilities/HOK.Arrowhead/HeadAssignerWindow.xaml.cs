using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

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

        private void CollectArrowheads()
        {
            try
            {
                var eId = new ElementId(BuiltInParameter.ALL_MODEL_FAMILY_NAME);
                var provider = new ParameterValueProvider(eId);
                FilterStringRuleEvaluator evaluator = new FilterStringEquals();
                FilterRule rule = new FilterStringRule(provider, evaluator, "Arrowhead", false);
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
                MessageBox.Show("Failed to collect the Arrowhead types.\n" + ex.Message, "Collect Arrowheads", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void buttonApply_Click(object sender, RoutedEventArgs e)
        {
            if (UpdateLeaders())
            {
                DialogResult = true;
            }
        }

        private bool UpdateLeaders()
        {
            var result = false;
            try
            {
                var i = 0;
                if (null != comboBoxArrow.SelectedValue)
                {
                    var arrowhead = comboBoxArrow.SelectedItem as Arrowhead;
                    var selectedId=comboBoxArrow.SelectedValue as ElementId;

                    var collector = new FilteredElementCollector(m_doc);
                    var families = collector.OfClass(typeof(Family)).Cast<Family>().ToList();

                    progressBar.Visibility = System.Windows.Visibility.Visible;
                    statusLable.Visibility = System.Windows.Visibility.Visible;
                    statusLable.Text = "Applying Changes . .";

                    progressBar.Minimum = 0;
                    progressBar.Maximum = families.Count;
                    progressBar.Value = 0;

                    double value = 0;
                    var updatePbDelegate = new UpdateProgressBarDelegate(progressBar.SetValue);

                    using (var t = new Transaction(m_doc))
                    {
                        foreach (var family in families)
                        {
                            value += 1;
                            var symbolIds = family.GetFamilySymbolIds().ToList();
                            foreach (var eId in symbolIds)
                            {
                                var fs = m_doc.GetElement(eId) as FamilySymbol;
                                var param = fs?.get_Parameter(BuiltInParameter.LEADER_ARROWHEAD);
                                if (param != null)
                                {
                                    t.Start("Update Leaders");
                                    try
                                    {
                                        param.Set(selectedId);
                                        i++;
                                        t.Commit();
                                    }
                                    catch { t.RollBack(); }
                                }
                            }
                            Dispatcher.Invoke(updatePbDelegate, System.Windows.Threading.DispatcherPriority.Background, new object[] { ProgressBar.ValueProperty, value });
                        }
                    }

                    progressBar.Visibility = System.Windows.Visibility.Hidden;
                    statusLable.Text = "Done";
                    MessageBox.Show(i + " Annotation Symbol Types are updated with " + arrowhead.ArrowName, "Annotation Symbols Types", MessageBoxButton.OK, MessageBoxImage.Information);
                    result = true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to update leaders.\n"+ex.Message, "Update Leaders", MessageBoxButton.OK, MessageBoxImage.Warning);
                result = false;
            }
            return result;
        }

        private delegate void UpdateProgressBarDelegate(DependencyProperty dp, object value);

        private void buttonCancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }

    /// <summary>
    /// 
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
