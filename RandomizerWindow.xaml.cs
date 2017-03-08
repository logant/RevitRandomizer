using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using Autodesk.Revit;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace Logant.Revit
{
    /// <summary>
    /// Interaction logic for RandomizerWindow.xaml
    /// </summary>
    public partial class RandomizerWindow : Window
    {
        // Brushes for the button fills
        LinearGradientBrush eBrush = new LinearGradientBrush(
            System.Windows.Media.Color.FromArgb(255, 245, 245, 245), 
            System.Windows.Media.Color.FromArgb(255, 195, 195, 195), 
            new System.Windows.Point(0, 0), 
            new System.Windows.Point(0, 1));
        SolidColorBrush lBrush = new SolidColorBrush(System.Windows.Media.Color.FromArgb(0, 0, 0, 0));

        Document doc;
        Dictionary<string, Element> typeElems;
        FamilySymbol selectedSymbol = null;

        ExternalEvent _exEvent = null;
        ExtEventHandler _handler = null;

        public Document Doc { get { return doc; } }

        public RandomizerWindow(Document document, ExternalEvent exEvent, ExtEventHandler handler)
        {
            doc = document;
            _exEvent = exEvent;
            _handler = handler;
            
            InitializeComponent();

            Categories categories = doc.Settings.Categories;
            List<Category> cats = new List<Category>();
            foreach (Category cat in categories)
            {
                cats.Add(cat);
            }
            cats.Sort((x, y) => x.Name.CompareTo(y.Name));

            categoryComboBox.ItemsSource = cats;
            categoryComboBox.DisplayMemberPath = "Name";
            categoryComboBox.SelectedIndex = 0;
        }

        private void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            try { DragMove(); }
            catch { }
        }

        private void categoryComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                Category cat = categoryComboBox.SelectedItem as Category;

                if (cat != null)
                {
                    List<string> elementNames = new List<string>();
                    FilteredElementCollector collector = new FilteredElementCollector(doc).OfCategoryId(cat.Id).WhereElementIsElementType();
                    typeElems = new Dictionary<string, Element>();
                    foreach (Element elem in collector)
                    {
                        try
                        {
                            ElementType eType = elem as ElementType;
                            FamilyInstanceFilter instFilter = new FamilyInstanceFilter(doc, eType.Id);
                            ICollection<ElementId> instanceCol = new FilteredElementCollector(doc).OfCategoryId(eType.Category.Id).WherePasses(instFilter).ToElementIds();
                            if (instanceCol.Count == 0)
                                continue;

                            string name = eType.FamilyName + ": " + eType.Name + " [" + instanceCol.Count.ToString() + "]";
                            elementNames.Add(name);
                            typeElems.Add(name, elem);
                        }
                        catch { }
                    }
                    elementNames.Sort();

                    typeComboBox.ItemsSource = elementNames;
                    try
                    {
                        typeComboBox.SelectedIndex = 0;
                    }
                    catch { }
                }
                else
                    typeComboBox.ItemsSource = "Invalid Category";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error:\n" + ex.ToString());
            }
        }

        private void typeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string elementName = string.Empty;
            Element selectedType = null;
            try
            {
                elementName = typeComboBox.SelectedItem as string;
                selectedType = typeElems[elementName];
            }
            catch { }

            
            if(selectedType != null)
            {
                // Try to find an instance of this type.
                ElementType eType = selectedType as ElementType;
                FamilyInstanceFilter instFilter = new FamilyInstanceFilter(doc, eType.Id);
                ICollection<ElementId> instanceCol = new FilteredElementCollector(doc).OfCategoryId(eType.Category.Id).WherePasses(instFilter).ToElementIds();
                Element found = doc.GetElement(instanceCol.First());
                if (found != null)
                {
                    List<Parameter> parameters = new List<Parameter>();
                    foreach (Parameter p in found.Parameters)
                    {
                        if((p.StorageType == StorageType.Double || p.StorageType == StorageType.Integer) && !p.IsReadOnly)
                        {
                            parameters.Add(p);
                        }
                    }

                    parameters.Sort((x, y) => x.Definition.Name.CompareTo(y.Definition.Name));

                    paramListBox.ItemsSource = parameters;
                    paramListBox.DisplayMemberPath = "Definition.Name";
                    selectedSymbol = eType as FamilySymbol;
                 }
            }
            else
            {
                paramListBox.ItemsSource = new List<Parameter>();
                selectedSymbol = null;
            }
        }

        private void cancelButton_Click(object sender, RoutedEventArgs e)
        {
            Hide();
        }

        private void cancelButton_MouseEnter(object sender, MouseEventArgs e)
        {
            cancelRect.Fill = eBrush;
        }

        private void cancelButton_MouseLeave(object sender, MouseEventArgs e)
        {
            cancelRect.Fill = lBrush;
        }

        private void randomizeButton_Click(object sender, RoutedEventArgs e)
        {
            Parameter param = paramListBox.SelectedItem as Parameter;
            if (param == null)
                Autodesk.Revit.UI.TaskDialog.Show("Test", "Parameter is null");

            FamilyInstanceFilter instFilter = new FamilyInstanceFilter(doc, selectedSymbol.Id);
            ICollection<ElementId> instanceCol = new FilteredElementCollector(doc).OfCategoryId(selectedSymbol.Category.Id).WherePasses(instFilter).ToElementIds();

            int minInt;
            int maxInt;
            if (!int.TryParse(minTextBox.Text, out minInt))
                minInt = int.MinValue;
            if (!int.TryParse(maxTextBox.Text, out maxInt))
                maxInt = int.MinValue;

            double minDouble;
            double maxDouble;
            if (!double.TryParse(minTextBox.Text, out minDouble))
                minDouble = double.NaN;
            if (!double.TryParse(maxTextBox.Text, out maxDouble))
                maxDouble = double.NaN;

            _handler.MaxInt = maxInt;
            _handler.MinInt = minInt;
            _handler.MinDouble = minDouble;
            _handler.MaxDouble = maxDouble;
            _handler.IntegersOnly = integerCheckBox.IsChecked.Value;
            _handler.Elements = instanceCol;
            _handler.Doc = doc;
            _handler.Parameter = param;
            _exEvent.Raise();

            //Random rand = new Random();

            //if (integerCheckBox.IsChecked.HasValue && integerCheckBox.IsChecked.Value)
            //{
            //    if (minInt != int.MinValue && maxInt != int.MinValue)
            //    {
            //        using (Transaction randomTrans = new Transaction(doc, "Randomize"))
            //        {
            //            randomTrans.Start();
            //            foreach (ElementId eid in instanceCol)
            //            {
            //                Element elem = doc.GetElement(eid);
            //                try
            //                {
            //                    int r = rand.Next(minInt, maxInt);
            //                    elem.get_Parameter(param.Definition).Set(r);
            //                }
            //                catch { }
            //            }
            //            randomTrans.Commit();
            //        }
            //    }
            //}
            //else 
            //{
            //    if (minDouble != double.NaN && maxDouble != double.NaN)
            //    {
            //        using (Transaction randomTrans = new Transaction(doc, "Randomize"))
            //        {
            //            randomTrans.Start();

            //            foreach (ElementId eid in instanceCol)
            //            {
            //                Element elem = doc.GetElement(eid);
            //                try
            //                {
            //                    double range = maxDouble - minDouble;
            //                    double r = rand.NextDouble() * range + minDouble;
            //                    if(param.Definition.ParameterType == ParameterType.Length)
            //                    {
            //                        Units units = doc.GetUnits();
            //                        r = UnitUtils.ConvertToInternalUnits(r, param.DisplayUnitType);
            //                    }
            //                    elem.get_Parameter(param.Definition).Set(r);
            //                }
            //                catch (Exception ex) { Autodesk.Revit.UI.TaskDialog.Show("Error", ex.Message); }
            //            }

            //            randomTrans.Commit();
            //        }
            //    }
            //}
        }

        private void randomizeButton_MouseEnter(object sender, MouseEventArgs e)
        {
            randomizeRect.Fill = eBrush;
        }

        private void randomizeButton_MouseLeave(object sender, MouseEventArgs e)
        {
            randomizeRect.Fill = lBrush;
        }

        private void resetButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Clear out all of the selections
                minTextBox.Text = string.Empty;
                maxTextBox.Text = string.Empty;

                categoryComboBox.SelectedIndex = 0;
                integerCheckBox.IsChecked = false;
            }
            catch (Exception ex)
            {
                TaskDialog.Show("Error", ex.ToString());
            }
        }

        private void resetButton_MouseEnter(object sender, MouseEventArgs e)
        {
            resetRect.Fill = eBrush;
        }

        private void resetButton_MouseLeave(object sender, MouseEventArgs e)
        {
            resetRect.Fill = lBrush;
        }
    }
}
