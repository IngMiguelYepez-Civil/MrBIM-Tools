using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace MrBIM.Tools
{
    public static class Rules
    {
        public static List<string> ParseNames(string text)
        {
            return (text ?? "").Split(new[] { ';', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(x => x.Trim()).Where(x => x.Length > 0).Distinct(StringComparer.Ordinal).ToList();
        }

        public static bool IsBlank(string storage, object value, bool hasValue)
        {
            if (!hasValue || value == null) return true;
            switch (storage)
            {
                case "String": return string.IsNullOrWhiteSpace((string)value);
                case "Double":
                    double number = Convert.ToDouble(value, CultureInfo.InvariantCulture);
                    return double.IsNaN(number) || double.IsInfinity(number);
                case "ElementId": return Convert.ToInt64(value, CultureInfo.InvariantCulture) == -1;
                default: return false; // 0 y No son valores válidos.
            }
        }

        public static string CsvCell(object value)
        {
            string text = Convert.ToString(value, CultureInfo.InvariantCulture) ?? "";
            string trimmed = text.TrimStart();
            if ((trimmed.Length > 0 && "=+-@".IndexOf(trimmed[0]) >= 0)
                || (text.Length > 0 && "\t\r\n".IndexOf(text[0]) >= 0)) text = "'" + text;
            return "\"" + text.Replace("\"", "\"\"") + "\"";
        }

        public static string CsvLine(IEnumerable<object> cells)
        {
            return string.Join(";", cells.Select(CsvCell)) + "\r\n";
        }
    }
}
