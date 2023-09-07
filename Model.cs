using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FittingsJoiner
{
    public static class Model
    {        
        public static ExternalCommandData commandData { get; set; }
        private static UIApplication uiapp { get => commandData.Application; }
        private static UIDocument uidoc { get => uiapp.ActiveUIDocument; }
        private static Document doc { get => uidoc.Document; }

        private static Element elementToMove { get; set; }
        private static Element elementToJoin { get; set; }

        public static void JoinFittings()
        {
            using (TransactionGroup transactionGroup = new TransactionGroup(doc, "Соединение фитингов"))
            {
                transactionGroup.Start();
                try
                {
                    while (true)
                    {
                        Connector[] connectors = FindNearestConnectors();
                        if (connectors[0] == null)
                        {
                            TaskDialog.Show("Ошибка", "Первый элемент должен иметь хотя бы один открытый соединитель");
                        }
                        if (connectors[1] == null)
                        {
                            TaskDialog.Show("Ошибка", "второй элемент должен иметь хотя бы один открытый соединитель");
                        }
                        XYZ translation = connectors[1].Origin - connectors[0].Origin;
                        XYZ forRotation = new XYZ(0,0,1);
                        double angle = Math.PI - CalculateAngle(connectors);
                        Line line = Line.CreateBound(connectors[0].Origin+ new XYZ(0, 0, 1), connectors[0].Origin); // + new XYZ(0, 0, 1)

                        using (Transaction transaction = new Transaction(doc, "Соединение фитингов"))
                        {
                            transaction.Start();
                            ElementTransformUtils.RotateElement(doc, elementToMove.Id, line, angle);
                            XYZ pointToOrient = connectors[1].CoordinateSystem.BasisZ;
                            XYZ pointToMove = connectors[0].CoordinateSystem.BasisZ;
                            if ((pointToOrient + pointToMove).IsZeroLength() == false)
                            {                               
                                ElementTransformUtils.RotateElement(doc, elementToMove.Id, line, -2*angle);
                            }

                            ElementTransformUtils.MoveElement(doc, elementToMove.Id, translation);
                            connectors[0].ConnectTo(connectors[1]);
                            transaction.Commit();
                        }
                    }
                }
                catch (OperationCanceledException )
                {
                    transactionGroup.Assimilate();
                }
            }
        }
        private static Connector[] FindNearestConnectors()
        {
            Connector[] connectors = new Connector[2];
            Reference fittingReference = uidoc.Selection.PickObject(ObjectType.Element, "Выберите присоединяемый элемент (который будет перемещён");
            Reference fittingReferenceToJoin = uidoc.Selection.PickObject(ObjectType.Element, "Выберите элемент, к которому будет выполнено присоединение");
            elementToMove = doc.GetElement(fittingReference);
            elementToJoin = doc.GetElement(fittingReferenceToJoin);
            Connector[] connectorsOfFirst = FindUnusedConnectors(elementToMove);
            Connector[] connectorsOfSecond = FindUnusedConnectors(elementToJoin);
            XYZ clickPoint1 = fittingReference.GlobalPoint;
            XYZ clickPoint2 = fittingReferenceToJoin.GlobalPoint;
            double distance = 20;
            foreach (Connector c in connectorsOfFirst)
            {
                if (distance > c.Origin.DistanceTo(clickPoint1))
                {
                    distance = c.Origin.DistanceTo(clickPoint1);
                    connectors[0] = c;
                }
            }
            distance = 20; //we should make it less
            foreach (Connector c in connectorsOfSecond)
            {
                if (distance > c.Origin.DistanceTo(clickPoint2))
                {
                    distance = c.Origin.DistanceTo(clickPoint2);
                    connectors[1] = c;
                }
            }
            return connectors;
        }
        private static Connector[] FindUnusedConnectors(Element element)
        {
            Connector[] connectors = new Connector[0];
            if (element is FamilyInstance familyInstance)
            {
                if (familyInstance.MEPModel != null)
                {
                    var ConSet = familyInstance.MEPModel.ConnectorManager.UnusedConnectors;
                    int n = ConSet.Size;
                    int i = 0;
                    connectors = new Connector[n];
                    foreach (Connector connector in ConSet)
                    {
                        if (connector.ConnectorType != ConnectorType.Invalid || connector.ConnectorType != ConnectorType.Logical)
                        {
                            connectors[i] = connector;
                        }
                        else
                        {
                            n--;
                        }
                        i++;
                    }
                    Array.Resize(ref connectors, n);
                }                
            }            
            return connectors;
        }
        private static double CalculateAngle(Connector[] connectors)
        {
            double firstRotation = (elementToMove.Location as LocationPoint).Rotation;
            double secondRotation = (elementToJoin.Location as LocationPoint).Rotation;

            Connector c1 = connectors[0];
            Connector c2 = connectors[1];
            
            XYZ pointToOrient = connectors[1].CoordinateSystem.BasisZ;
            XYZ pointToMove = connectors[0].CoordinateSystem.BasisZ;     
           
            
            double result = pointToOrient.AngleTo(pointToMove);
            return result;
        }
        
    }
}
