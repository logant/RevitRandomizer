using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace Logant.Revit
{
    [Transaction(TransactionMode.Manual)]
    public class RandomizerCmd : IExternalCommand
    {
        RandomizerWindow window = null;
        

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            try
            {
                RandomizerApp._Instance.ShowForm(commandData.Application.ActiveUIDocument.Document);
                return Result.Succeeded;
            }
            catch (Exception ex)
            {
                message = ex.Message;
                return Result.Failed;
            }
        }
    }
}
