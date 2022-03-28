using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System.Reflection;
using System.Windows.Media.Imaging;

namespace CalculationCable
{
    public class Start : IExternalApplication
    {
        internal static Start _app = null;
        public static Start Instance => _app;
        public Result OnStartup(UIControlledApplication application)
        {
            _app=this;
            var tabPanelName = "BIMDATA";
            try
            {
                application.GetRibbonPanels(tabPanelName);
            }
            catch
            {
                application.CreateRibbonTab(tabPanelName);
            }

            var ribbonPanel= application.GetRibbonPanels(tabPanelName).FirstOrDefault(x=>x.Name==tabPanelName) ?? application.CreateRibbonPanel(tabPanelName, tabPanelName);
            ribbonPanel.Name = tabPanelName;
            ribbonPanel.Title = tabPanelName;

            var button = new PushButtonData("Cabel calculate", "Cabel calculate", Assembly.GetExecutingAssembly().Location, typeof(Command).FullName)
            {
                LargeImage = new BitmapImage(new Uri(
                    "pack://application:,,,/CalculationCable;component/Resources/cabel.png"))
            };
            ribbonPanel.AddItem(button);


            return Result.Succeeded;
        }
        public Result OnShutdown(UIControlledApplication application)
        {
            return Result.Succeeded;
        }
    }
}
