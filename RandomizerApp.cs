using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

using Autodesk.Revit;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace Logant.Revit
{
    public class RandomizerApp : IExternalApplication
    {
        public static RandomizerApp _Instance = null;
        public static ExternalEvent ExEvent = null;
        public static ExtEventHandler Handler = null;

        RandomizerWindow window = null;

        public Result OnShutdown(UIControlledApplication application)
        {
            try
            {
                window.Close();
            }
            catch { }
            return Result.Succeeded;
        }

        public Result OnStartup(UIControlledApplication application)
        {
            try
            {
                _Instance = this;
                Handler = new ExtEventHandler();
                ExEvent = ExternalEvent.Create(Handler);

                string path = typeof(RandomizerApp).Assembly.Location;

                // Build the button
                PushButtonData randomPBD = new PushButtonData("Randomize", "Randomize", path, "Logant.Revit.RandomizerCmd")
                {
                    LargeImage = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(Properties.Resources.Static.GetHbitmap(), IntPtr.Zero, System.Windows.Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions()),
                    ToolTip = "Launch the Randomizer"
                };


                // Add the button to the panel
                RibbonPanel panel = application.CreateRibbonPanel("logant");
                panel.AddItem(randomPBD);


                return Result.Succeeded;
            }
            catch
            {
                return Result.Failed;
            }
        }

        public void ShowForm(UIDocument doc)
        {
            if(window == null)
            {
                System.Diagnostics.Process proc = System.Diagnostics.Process.GetCurrentProcess();
                IntPtr handle = proc.MainWindowHandle;
                window = new RandomizerWindow(doc, ExEvent, Handler);
                System.Windows.Interop.WindowInteropHelper helper = new System.Windows.Interop.WindowInteropHelper(window);
                helper.Owner = handle;
                window.Show();
            }
            else
            {
                // Compare the current document to the window's document
                if(doc.Document.PathName == window.Doc.PathName || doc.Document.Title == window.Doc.Title)
                    window.Show();
                else
                {
                    // Close the window and relaunch.
                    window.Close();

                    System.Diagnostics.Process proc = System.Diagnostics.Process.GetCurrentProcess();
                    IntPtr handle = proc.MainWindowHandle;
                    window = new RandomizerWindow(doc, ExEvent, Handler);
                    System.Windows.Interop.WindowInteropHelper helper = new System.Windows.Interop.WindowInteropHelper(window);
                    helper.Owner = handle;
                    window.Show();
                }
            }
           
        }
    }
}
