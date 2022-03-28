using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace CalculationCable
{
    [Transaction(TransactionMode.Manual)]
    internal class Command:IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            var doc = commandData.Application.ActiveUIDocument.Document;

            //Отобрать элементы содержащие BD_Состав кабельной продукции
            var allElements = new FilteredElementCollector(doc)
                .OfCategory(BuiltInCategory.OST_DuctCurves)
                .WhereElementIsNotElementType()
                .Where(f => !string.IsNullOrWhiteSpace(f.LookupParameter("BD_Состав кабельной продукции").AsString()))                 
                .ToList();

            //Отобрать элементы у которых более одной строки в BD_Состав кабельной продукции
            var cabelsBunch = from cabel in allElements
                              where cabel.LookupParameter("BD_Состав кабельной продукции").AsString().Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries).Length>1
                              select cabel;

            foreach (var cabel in cabelsBunch.ToDictionary(x => x, y => y.LookupParameter("BD_Состав кабельной продукции")?.AsString()))
            using (var transaction = new Transaction(doc))
            {
                transaction.Start("Cabel calculation");
                    string tempStr = "";
                    var countElement = cabel.Value.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
                    var locationElement = (cabel.Key.Location as LocationCurve).Curve;
                    var levelElement = cabel.Key.Document.GetElement(cabel.Key.LevelId) as Level;                    
                    var element = cabel.Key as FamilySymbol;
                    for(int i = 0; i < countElement.Length; i++)
                    {                        
                        if (i == 0)
                        {
                            cabel.Key.LookupParameter("BD_Состав кабельной продукции")?.Set(countElement[i]);
                        }
                        else
                        {
                            doc.Create.NewFamilyInstance(locationElement, element, levelElement,Autodesk.Revit.DB.Structure.StructuralType.NonStructural);
                            //newCabel.LookupParameter("BD_Состав кабельной продукции")?.Set(countElement[i]);
                        }
                    }

 

                    // Размножить элементы на количество дополнительных строк в BD_Состав кабельной продукции
                    // Удалить дополнительные строки из BD_Состав кабельной продукции
                    // Переенести данные из строки в параметры элемента






                    //foreach (var paramVal in elementsAndValues.Value.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries))
                    //    {
                    //        foreach (var value in paramVal.Split(new[] { ';' },StringSplitOptions.None))
                    //        {

                    //            tempString = tempString + value+"--";
                    //        }                        
                    //        tempString = tempString +'\n'+ "-----" + '\n';
                    //    }

                    TaskDialog.Show("test 02", tempStr);
                transaction.Commit();
            }
            return Result.Succeeded;
        }
        internal void CreateCabel()
        {

        }
    }


}
