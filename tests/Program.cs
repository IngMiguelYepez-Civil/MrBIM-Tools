using System;
using System.Linq;
using MrBIM.Tools;

internal static class Program
{
    private static int checks;
    private static void Check(bool pass, string name)
    {
        if (!pass) throw new Exception("FALLO: " + name);
        checks++;
        Console.WriteLine("OK: " + name);
    }
    private static int Main()
    {
        try
        {
            Check(Rules.ParseNames(" Código ; Zona;Código\nClasificación;;").SequenceEqual(new[] { "Código", "Zona", "Clasificación" }), "Nombres exactos y sin duplicados");
            Check(!Rules.IsBlank("Integer", 0, true), "Cero entero y No válidos");
            Check(!Rules.IsBlank("Double", 0.0, true), "Cero decimal válido");
            Check(Rules.IsBlank("Integer", 0, false), "Sin valor asignado");
            Check(Rules.IsBlank("String", " \t ", true), "Texto en blanco");
            Check(Rules.IsBlank("String", null, true), "Valor nulo");
            Check(Rules.IsBlank("Double", double.NaN, true), "NaN no válido");
            Check(Rules.IsBlank("Double", double.PositiveInfinity, true), "Infinito no válido");
            Check(Rules.IsBlank("ElementId", -1L, true), "ID no asignado");
            Check(!Rules.IsBlank("ElementId", -2L, true), "Opciones negativas válidas");
            Check(!Rules.IsBlank("ElementId", 5000000000L, true), "ID de 64 bits");
            Check(Rules.CsvCell("Texto; con \"comillas\"") == "\"Texto; con \"\"comillas\"\"\"", "Escape CSV");
            Check(Rules.CsvCell(" =1+1") == "\"' =1+1\"", "Protección frente a fórmulas");
            Check(Rules.CsvLine(new object[] { "Código", 0 }) == "\"Código\";\"0\"\r\n", "Delimitador y fin de línea CSV");
            Console.WriteLine(checks + " comprobaciones aprobadas.");
            return 0;
        }
        catch (Exception ex) { Console.Error.WriteLine(ex.Message); return 1; }
    }
}
