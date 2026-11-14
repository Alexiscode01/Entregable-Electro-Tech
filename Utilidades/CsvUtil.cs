// BIBLIOTECA UTILIDADES – exportación de reportes a CSV (abre bien en Excel)
using System;
using System.Data;
using System.Globalization;
using System.IO;
using System.Text;

namespace Utilidades
{
    public static class CsvUtil
    {
        // Escapa comillas y evita que Excel ejecute como fórmula un texto que empiece con = + - @
        public static string Campo(string s)
        {
            s = s ?? "";
            if (s.Length > 0 && "=+-@".IndexOf(s[0]) >= 0) s = "'" + s;
            return "\"" + s.Replace("\"", "\"\"") + "\"";
        }

        public static void Exportar(DataTable tabla, string ruta)
        {
            var sb = new StringBuilder();
            var cabecera = new string[tabla.Columns.Count];
            for (int i = 0; i < cabecera.Length; i++) cabecera[i] = Campo(tabla.Columns[i].ColumnName);
            sb.AppendLine(string.Join(",", cabecera));

            foreach (DataRow fila in tabla.Rows)
            {
                var campos = new string[tabla.Columns.Count];
                for (int i = 0; i < campos.Length; i++) campos[i] = Valor(fila[i]);
                sb.AppendLine(string.Join(",", campos));
            }
            File.WriteAllText(ruta, sb.ToString(), new UTF8Encoding(true));
        }

        private static string Valor(object v)
        {
            if (v == null || v == DBNull.Value) return "\"\"";
            if (v is DateTime) return Campo(((DateTime)v).ToString("yyyy-MM-dd HH:mm"));
            if (v is decimal) return ((decimal)v).ToString("0.00", CultureInfo.InvariantCulture);
            if (v is int || v is long) return Convert.ToString(v, CultureInfo.InvariantCulture);
            return Campo(Convert.ToString(v, CultureInfo.InvariantCulture));
        }
    }
}
