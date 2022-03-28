using System;
using System.Linq;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
//Review: удалены неиспользуемые сборки  
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
                .Where(f => !string.IsNullOrWhiteSpace(f.LookupParameter("BD_Состав кабельной продукции")?.AsString()))     //Review: возможен NullReferenceException          
                .ToList();

            //Review: предлагаю использовать одну форму написания LINQ, например через цепочку вызова методов, что будет выглядеть следующим образом:
            //var cabelsBunch = allElements.Where(cabel =>
            //    cabel.LookupParameter("BD_Состав кабельной продукции")
            //        .AsString()
            //        .Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries)
            //        .Length > 1)
            //        .ToDictionary(x => x, y => y.LookupParameter("BD_Состав кабельной продукции")?.AsString());

            //Отобрать элементы у которых более одной строки в BD_Состав кабельной продукции
            var cabelsBunch = (from cabel in allElements
                              where cabel.LookupParameter("BD_Состав кабельной продукции").AsString().Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries).Length>1
                              select cabel).ToDictionary(x => x, y => y.LookupParameter("BD_Состав кабельной продукции")?.AsString());
            
            //Review: ToDictionary был вынесен в LINQ-запрос выше
            foreach (var cabel in cabelsBunch)
                using (var transaction = new Transaction(doc))                      //Review: из-за того, что транзакция находится внутри цикла - будет постоянно открываться и коммититься - замедление работы Revit:
                                                                                    //Review: можно создать TransactionGroup или переместить цикл внутрь транзакции
                {
                    transaction.Start("Cabel calculation");
                    var tempStr = "";                                                                                              //Review: предлагаю использовать var, если тип разрешим компилятором
                    var countElement = cabel.Value.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);      //Review: Split применяется дважды по отношению к одной и той же строке - можно использовать однажды в цепочке методов LINQ-запроса
                    var locationElement = (cabel.Key.Location as LocationCurve)?.Curve;                                       //Review: возможен NullReferenceException и проверку также можно перенести в LINQ-запрос
                    if (locationElement == null) continue;                                                                         //Review: пропуск элементов, не являющихся LocationCurve
                    
                    //Review: альтернативный вариант проверки типа:
                    //if (cabel.Key.Location is LocationCurve locationElement)
                    //{
                    //    ... код с использованием locationElement;
                    //}

                    var levelElement = cabel.Key.Document.GetElement(cabel.Key.LevelId) as Level;                    
                    var element = cabel.Key as FamilySymbol;
                    for(var i = 0; i < countElement.Length; i++)
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
