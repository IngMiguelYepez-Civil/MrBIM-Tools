using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace MrBIM.Tools
{
    public static class CsvReport
    {
        public static void Write(string path, AuditReport report)
        {
            using (var writer = new StreamWriter(path, false, new UTF8Encoding(true)))
            {
                writer.Write(Rules.CsvLine(new object[] {
                    "Proyecto", "Revit", "Fecha", "Alcance", "Búsqueda", "Parámetros exigidos", "Elementos revisados",
                    "Comprobaciones", "OK", "Hallazgos", "ElementId", "UniqueId", "Categoría", "Elemento", "Parámetro", "Estado", "Origen", "Detalle"
                }));
                var meta = new object[] { report.Project, report.Revit, report.Created.ToString("o"), report.Scope,
                    report.Policy, report.Parameters, report.ElementCount, report.CheckCount, report.OkCount, report.Findings.Count };
                if (report.Findings.Count == 0)
                    writer.Write(Rules.CsvLine(meta.Concat(new object[] { "", "", "", "", "", "SIN_HALLAZGOS", "", "" })));
                else foreach (Finding finding in report.Findings)
                    writer.Write(Rules.CsvLine(meta.Concat(new object[] { finding.ElementId, finding.UniqueId,
                        finding.Category, finding.Element, finding.Parameter, finding.State, finding.Source, finding.Detail })));
            }
        }
    }
}
