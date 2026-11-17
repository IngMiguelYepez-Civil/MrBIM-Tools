using System;
using System.Linq;
using System.Reflection;
using Autodesk.Revit.UI;
using Autodesk.Revit.DB;
using Autodesk.Revit.Attributes;

namespace MrBIM.Tools
{
    public class App : IExternalApplication
    {
        public Result OnStartup(UIControlledApplication app)
        {
            string version = app.ControlledApplication.VersionNumber;
            if (version != "2024" && version != "2025") return Result.Failed;
            try
            {
                // La pestaña puede existir si hay otros módulos MrBIM cargados.
                try { app.CreateRibbonTab("MrBIM"); }
                catch (Autodesk.Revit.Exceptions.ArgumentException) { }
                RibbonPanel panel = app.GetRibbonPanels("MrBIM").FirstOrDefault(p => p.Name == "Control de calidad")
                    ?? app.CreateRibbonPanel("MrBIM", "Control de calidad");
                string assembly = Assembly.GetExecutingAssembly().Location;
                var review = new PushButtonData("MrBIMReviewParameters", "Revisar\nparámetros", assembly,
                    typeof(ReviewCommand).FullName);
                review.ToolTip = "Detecta parámetros obligatorios vacíos, ausentes o con nombres duplicados.";
                review.AvailabilityClassName = typeof(ProjectAvailability).FullName;
                panel.AddItem(review);
                var about = new PushButtonData("MrBIMAbout", "Acerca de\nMrBIM", assembly,
                    typeof(AboutCommand).FullName);
                about.ToolTip = "Muestra la versión, autoría y alcance de MrBIM Tools.";
                panel.AddItem(about);
                return Result.Succeeded;
            }
            catch (Exception ex)
            {
                TaskDialog.Show("MrBIM Tools", "No se pudo cargar la interfaz.\n" + ex.Message);
                return Result.Failed;
            }
        }
        public Result OnShutdown(UIControlledApplication app) { return Result.Succeeded; }
    }

    public class ProjectAvailability : IExternalCommandAvailability
    {
        public bool IsCommandAvailable(UIApplication app, CategorySet selectedCategories)
        {
            return app.ActiveUIDocument != null && !app.ActiveUIDocument.Document.IsFamilyDocument;
        }
    }

    [Transaction(TransactionMode.ReadOnly)]
    public class AboutCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData data, ref string message, ElementSet elements)
        {
            TaskDialog.Show("MrBIM Tools", "MrBIM Tools 0.2.0 — Prototipo independiente\n"
                + "Ing. Miguel Oswaldo Yépez González\n\n"
                + "Revisión de parámetros obligatorios, selección de hallazgos y exportación CSV.\n"
                + "Dirigido a Revit 2024 y 2025.\n\n"
                + "Compilación y validación en Revit pendientes.\n"
                + "No modifica parámetros ni geometría.");
            return Result.Succeeded;
        }
    }
}
