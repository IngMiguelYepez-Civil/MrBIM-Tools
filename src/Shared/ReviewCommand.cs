using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Forms = System.Windows.Forms;

namespace MrBIM.Tools
{
    [Transaction(TransactionMode.ReadOnly)]
    public class ReviewCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData data, ref string message, ElementSet elementSet)
        {
            UIDocument uiDoc = data.Application.ActiveUIDocument;
            if (uiDoc == null || uiDoc.Document.IsFamilyDocument)
            {
                TaskDialog.Show("MrBIM Tools", "Abra un proyecto de Revit para revisar sus parámetros.");
                return Result.Cancelled;
            }
            Document doc = uiDoc.Document;
            string year = data.Application.Application.VersionNumber;
            if (year != "2024" && year != "2025") return Result.Cancelled;
            var owner = new RevitWindow(data.Application.MainWindowHandle);
            try
            {
                int scope;
                using (var scopeForm = new ScopeForm())
                {
                    if (scopeForm.ShowDialog(owner) != Forms.DialogResult.OK) return Result.Cancelled;
                    scope = scopeForm.Scope;
                }
                List<Element> elements;
                if (scope == 0)
                    elements = uiDoc.Selection.GetElementIds().Select(id => doc.GetElement(id)).Where(Audit.IsModelInstance).ToList();
                else
                {
                    if (scope == 1 && !FilteredElementCollector.IsViewValidForElementIteration(doc, doc.ActiveView.Id))
                    {
                        TaskDialog.Show("MrBIM Tools", "Esta vista no permite consultar elementos. Elija otra vista o Proyecto completo.");
                        return Result.Cancelled;
                    }
                    using (var collector = scope == 1 ? new FilteredElementCollector(doc, doc.ActiveView.Id) : new FilteredElementCollector(doc))
                        elements = collector.WhereElementIsNotElementType().Where(Audit.IsModelInstance).ToList();
                }
                if (elements.Count == 0)
                {
                    TaskDialog.Show("MrBIM Tools", "No hay instancias de categorías de modelo en este alcance.");
                    return Result.Cancelled;
                }
                var categories = elements.Select(e => e.Category).GroupBy(c => c.Id.Value)
                    .Select(g => new CategoryChoice(g.Key, g.First().Name)).OrderBy(c => c.Name).ToList();
                HashSet<long> chosen;
                List<string> names;
                bool allowType;
                using (var config = new ConfigurationForm(categories))
                {
                    if (config.ShowDialog(owner) != Forms.DialogResult.OK) return Result.Cancelled;
                    chosen = config.SelectedCategories;
                    names = config.ParameterNames;
                    allowType = config.AllowType;
                }
                elements = elements.Where(e => chosen.Contains(e.Category.Id.Value)).ToList();
                var report = new AuditReport {
                    Project = doc.Title, Revit = year, Scope = ScopeForm.ScopeNames[scope],
                    Policy = allowType ? "Instancia; tipo solo si no existe en instancia" : "Solo instancia",
                    Parameters = string.Join("; ", names), ElementCount = elements.Count
                };
                using (var progress = new AuditProgressForm(elements.Count))
                {
                    progress.Run(owner, () => {
                        for (int i = 0; i < elements.Count; i++)
                        {
                            if (progress.CancelRequested) return;
                            Element element = elements[i];
                            foreach (string parameter in names)
                            {
                                Finding finding = Audit.Check(doc, element, parameter, allowType);
                                report.CheckCount++;
                                if (finding == null) report.OkCount++;
                                else report.Findings.Add(finding);
                            }
                            if (i % 20 == 0 || i + 1 == elements.Count) progress.UpdateCount(i + 1);
                        }
                    });
                    if (progress.CancelRequested) return Result.Cancelled;
                }
                using (var results = new ResultsForm(report))
                {
                    results.ShowDialog(owner);
                    if (results.SelectedIds.Count > 0)
                    {
                        var ids = results.SelectedIds.Select(id => new ElementId(id)).ToList();
                        uiDoc.Selection.SetElementIds(ids);
                        if (results.Locate)
                        {
                            try { uiDoc.ShowElements(ids); }
                            catch (Exception ex) { TaskDialog.Show("MrBIM Tools", "Elementos seleccionados. No se pudo ajustar la vista:\n" + ex.Message); }
                        }
                    }
                }
                return Result.Succeeded;
            }
            catch (Exception ex)
            {
                message = "MrBIM Tools: " + ex.Message;
                return Result.Failed;
            }
        }
    }
}
