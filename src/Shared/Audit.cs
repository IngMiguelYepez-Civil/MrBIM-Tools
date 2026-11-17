using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;

namespace MrBIM.Tools
{
    public class Finding
    {
        public long ElementId { get; set; }
        public string UniqueId { get; set; }
        public string Category { get; set; }
        public string Element { get; set; }
        public string Parameter { get; set; }
        public string State { get; set; }
        public string Source { get; set; }
        public string Detail { get; set; }
    }

    public class AuditReport
    {
        public string Project { get; set; }
        public string Revit { get; set; }
        public string Scope { get; set; }
        public string Policy { get; set; }
        public string Parameters { get; set; }
        public DateTime Created { get; set; } = DateTime.Now;
        public int ElementCount { get; set; }
        public long CheckCount { get; set; }
        public long OkCount { get; set; }
        public List<Finding> Findings { get; } = new List<Finding>();
    }

    public static class Audit
    {
        public static bool IsModelInstance(Element element)
        {
            return element != null && !(element is ElementType) && element.Category != null
                && element.Category.CategoryType == CategoryType.Model;
        }

        public static Finding Check(Document doc, Element element, string name, bool allowType)
        {
            string source = "Instancia";
            try
            {
                IList<Parameter> found = element.GetParameters(name);
                if (found.Count == 0 && allowType)
                {
                    source = "Tipo";
                    Element type = doc.GetElement(element.GetTypeId());
                    found = type == null ? new List<Parameter>() : type.GetParameters(name);
                }
                if (found.Count == 0) return Make(element, name, "NO_EXISTE", "", "");
                if (found.Count > 1) return Make(element, name, "AMBIGUO", source, "Nombre duplicado en " + source);
                Parameter parameter = found[0];
                object value;
                switch (parameter.StorageType)
                {
                    case StorageType.String: value = parameter.AsString(); break;
                    case StorageType.Integer: value = parameter.AsInteger(); break;
                    case StorageType.Double: value = parameter.AsDouble(); break;
                    case StorageType.ElementId: value = parameter.AsElementId().Value; break;
                    default: return Make(element, name, "NO_EVALUABLE", source, "Sin almacenamiento evaluable");
                }
                if (Rules.IsBlank(parameter.StorageType.ToString(), value, parameter.HasValue))
                    return Make(element, name, "VACIO", source, "");
                return null; // Comprobación satisfactoria.
            }
            catch (Exception ex) { return Make(element, name, "ERROR_LECTURA", source, ex.Message); }
        }

        private static Finding Make(Element element, string parameter, string state, string source, string detail)
        {
            string name;
            try { name = element.Name ?? ""; } catch { name = ""; }
            return new Finding {
                ElementId = element.Id.Value, UniqueId = element.UniqueId,
                Category = element.Category.Name, Element = name,
                Parameter = parameter, State = state, Source = source, Detail = detail
            };
        }
    }
}
