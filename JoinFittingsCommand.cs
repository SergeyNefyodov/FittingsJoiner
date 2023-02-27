using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FittingsJoiner
{
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    public class JoinFittingsCommand : IExternalCommand
    {
        static AddInId addInId = new AddInId(new Guid("CA46BA90-C633-4BA3-9B37-C1481FEC8E3A"));
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            //View view = new View();
            //ViewModel viewModel = new ViewModel(commandData);
            Model.commandData = commandData;
            //view.DataContext = viewModel;
            //viewModel.CloseRequest += (s, e) => view.Close();
            //viewModel.HideRequest += (s, e) => view.Hide();
            //viewModel.ShowRequest += (s, e) => view.ShowDialog();
            //view.ShowDialog();
            Model.JoinFittings();
            return Result.Succeeded;
        }
    }
}
