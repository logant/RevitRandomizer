using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace Logant.Revit
{
    public class ExtEventHandler : IExternalEventHandler
    {
        public int MaxInt { get; set; }
        public int MinInt { get; set; }
        public double MinDouble { get; set; }
        public double MaxDouble { get; set; }
        public bool IntegersOnly { get; set; }

        public ICollection<ElementId> Elements { get; set; }
        public Document Doc { get; set; }
        public Parameter Parameter { get; set; }

        public void Execute(UIApplication app)
        {
            try
            {
                RunRandomizer();
            }
            catch { }
        }

        private void RunRandomizer()
        {
            Random rand = new Random();

            if (IntegersOnly)
            {
                if (MinInt != int.MinValue && MaxInt != int.MinValue)
                {
                    using (Transaction randomTrans = new Transaction(Doc, "Randomize"))
                    {
                        randomTrans.Start();
                        foreach (ElementId eid in Elements)
                        {
                            Element elem = Doc.GetElement(eid);
                            try
                            {
                                int r = rand.Next(MinInt, MaxInt);
                                elem.get_Parameter(Parameter.Definition).Set(r);
                            }
                            catch { }
                        }
                        randomTrans.Commit();
                    }
                }
            }
            else
            {
                if (MinDouble != double.NaN && MaxDouble != double.NaN)
                {
                    using (Transaction randomTrans = new Transaction(Doc, "Randomize"))
                    {
                        randomTrans.Start();

                        foreach (ElementId eid in Elements)
                        {
                            Element elem = Doc.GetElement(eid);
                            try
                            {
                                double range = MaxDouble - MinDouble;
                                double r = rand.NextDouble() * range + MinDouble;
                                if (Parameter.Definition.ParameterType == ParameterType.Length)
                                {
                                    Units units = Doc.GetUnits();
                                    r = UnitUtils.ConvertToInternalUnits(r, Parameter.DisplayUnitType);
                                }
                                elem.get_Parameter(Parameter.Definition).Set(r);
                            }
                            catch (Exception ex) { Autodesk.Revit.UI.TaskDialog.Show("Error", ex.Message); }
                        }

                        randomTrans.Commit();
                    }
                }
            }
        }

        public string GetName()
        {
            return "Random External Event Handler";
        }
    }
}
