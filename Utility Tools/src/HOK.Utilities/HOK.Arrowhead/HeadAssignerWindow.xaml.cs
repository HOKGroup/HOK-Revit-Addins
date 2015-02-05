using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace HOK.Arrowhead
{
    /// <summary>
    /// Interaction logic for HeadAssignerWindow.xaml
    /// </summary>
    public partial class HeadAssignerWindow : Window
    {
        private Autodesk.Revit.UI.UIApplication m_app;
        private Document m_doc;
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
                ElementId eId = new ElementId(BuiltInParameter.ALL_MODEL_FAMILY_NAME);
                ParameterValueProvider provider = new ParameterValueProvider(eId);
                FilterStringRuleEvaluator evaluator = new FilterStringEquals();
                FilterRule rule = new FilterStringRule(provider, evaluator, "Arrowhead", false);
                ElementParameterFilter filter = new ElementParameterFilter(rule);
                FilteredElementCollector collector = new FilteredElementCollector(m_doc).OfClass(typeof(ElementType)).WherePasses(filter);
                List<Element> leaders = collector.ToElements().ToList();

                foreach (Element element in leaders)
                {
                    if (!string.IsNullOrEmpty(element.Name))
                    {
                        Arrowhead arrowhead = new Arrowhead(element);
                        arrowheadList.Add(arrowhead);
                    }
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
                MessageBox.Show("Failed to collect the Arrowhead types.\n"+ex.Message, "Collect Arrowheads", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void buttonApply_Click(object sender, RoutedEventArgs e)
        {
            if (UpdateLeaders())
            {
                this.DialogResult = true;
            }
        }

        private bool UpdateLeaders()
        {
            bool result = false;
            try
            {
                int i = 0;
                if (null != comboBoxArrow.SelectedValue)
                {
                    Arrowhead arrowhead = comboBoxArrow.SelectedItem as Arrowhead;
                    ElementId selectedId=comboBoxArrow.SelectedValue as ElementId;

                    FilteredElementCollector collector = new FilteredElementCollector(m_doc);
                    List<Family> families = collector.OfClass(typeof(Family)).Cast<Family>().ToList();

                    progressBar.Visibility = System.Windows.Visibility.Visible;
                    statusLable.Visibility = System.Windows.Visibility.Visible;
                    statusLable.Text = "Applying Changes . .";

                    progressBar.Minimum = 0;
                    progressBar.Maximum = families.Count;
                    progressBar.Value = 0;

                    double value = 0;
                    UpdateProgressBarDelegate updatePbDelegate = new UpdateProgressBarDelegate(progressBar.SetValue);

                    using (Transaction t = new Transaction(m_doc))
                    {
                        foreach (Family family in families)
                        {
                            value += 1;
#if RELEASE2015
                            List<ElementId> symbolIds = family.GetFamilySymbolIds().ToList();
                            foreach (ElementId eId in symbolIds)
                            {
                                FamilySymbol fs = m_doc.GetElement(eId) as FamilySymbol;
                                if (null != fs)
                                {
                                    Parameter param = fs.get_Parameter(BuiltInParameter.LEADER_ARROWHEAD);
                                    if (null != param)
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
                            }
#else
                            foreach (FamilySymbol fs in family.Symbols)
                            {
                                Parameter param = fs.get_Parameter(BuiltInParameter.LEADER_ARROWHEAD);
                                if (null != param)
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
#endif

                            Dispatcher.Invoke(updatePbDelegate, System.Windows.Threading.DispatcherPriority.Background, new object[] { ProgressBar.ValueProperty, value });
                        }
                    }

                    progressBar.Visibility = System.Windows.Visibility.Hidden;
                    statusLable.Text = "Done";
                    MessageBox.Show(i.ToString() + " Annotation Symbol Types are updated with "+arrowhead.ArrowName, "Annotation Symbols Types", MessageBoxButton.OK, MessageBoxImage.Information);
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

        private delegate void UpdateProgressBarDelegate(System.Windows.DependencyProperty dp, Object value);

        private void buttonCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }

    public class Arrowhead
    {
        private Element arrowElement=null;
        private string arrowName="";
        private ElementId elementId=ElementId.InvalidElementId;

        public Element ArrowElement { get{return arrowElement;} set{arrowElement=value;}}
        public string ArrowName {get{return arrowName;}set{arrowName=value;}}
        public ElementId ArrowElementId {get{return elementId;}set{elementId=value;}}

        public Arrowhead(Element element)
        {
            arrowElement=element;
            arrowName=element.Name;
            elementId=element.Id;
        }
    }
}
