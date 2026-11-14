// CAPA PRESENTACIÓN (WPF) – arma los documentos (FlowDocument) que se ven en la vista previa y se imprimen.
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using Entidades;
using Negocio;

namespace Presentacion
{
    public static class ConstructorDocumentos
    {
        // Reporte genérico: título, subtítulo, tabla con los datos y una línea final (totales).
        public static FlowDocument DesdeTabla(string titulo, string subtitulo, DataTable tabla, string pie)
        {
            var doc = new FlowDocument
            {
                FontFamily = new FontFamily("Segoe UI"),
                FontSize = 11,
                PagePadding = new Thickness(40)
            };

            doc.Blocks.Add(new Paragraph(new Run("TECH SOLUTIONS"))
            { FontSize = 20, FontWeight = FontWeights.Bold, Margin = new Thickness(0) });
            doc.Blocks.Add(new Paragraph(new Run(titulo))
            { FontSize = 14, FontWeight = FontWeights.SemiBold, Margin = new Thickness(0, 2, 0, 0) });
            doc.Blocks.Add(new Paragraph(new Run(subtitulo))
            { Foreground = Brushes.DimGray, Margin = new Thickness(0, 2, 0, 10) });

            var t = new Table { CellSpacing = 0 };
            foreach (DataColumn c in tabla.Columns) t.Columns.Add(new TableColumn());

            var grupo = new TableRowGroup();
            var cabecera = new TableRow { Background = Brushes.LightSteelBlue };
            foreach (DataColumn c in tabla.Columns)
                cabecera.Cells.Add(Celda(Encabezado(c.ColumnName), true, EsNumero(c.DataType)));
            grupo.Rows.Add(cabecera);

            foreach (DataRow fila in tabla.Rows)
            {
                var tr = new TableRow();
                foreach (DataColumn c in tabla.Columns)
                    tr.Cells.Add(Celda(Formato(fila[c]), false, EsNumero(c.DataType)));
                grupo.Rows.Add(tr);
            }
            t.RowGroups.Add(grupo);
            doc.Blocks.Add(t);

            if (!string.IsNullOrEmpty(pie))
            {
                doc.Blocks.Add(new Paragraph(new Run(pie))
                { FontWeight = FontWeights.Bold, FontSize = 12, TextAlignment = TextAlignment.Right, Margin = new Thickness(0, 10, 0, 0) });
            }
            doc.Blocks.Add(new Paragraph(new Run("Impreso el " + DateTime.Now.ToString("dd/MM/yyyy HH:mm") +
                                                 (Sesion.Actual == null ? "" : " por " + Sesion.Actual.Login)))
            { Foreground = Brushes.Gray, FontSize = 9, Margin = new Thickness(0, 14, 0, 0) });
            return doc;
        }

        // Comprobante de una venta recién registrada.
        public static FlowDocument Comprobante(Venta venta, string cliente, IList<DetalleVenta> detalle)
        {
            var tabla = new DataTable();
            tabla.Columns.Add("Codigo", typeof(string));
            tabla.Columns.Add("Producto", typeof(string));
            tabla.Columns.Add("Cantidad", typeof(int));
            tabla.Columns.Add("PrecioUnitario", typeof(decimal));
            tabla.Columns.Add("Subtotal", typeof(decimal));
            foreach (DetalleVenta d in detalle)
                tabla.Rows.Add(d.Codigo, d.Producto, d.Cantidad, d.PrecioUnitario, d.Subtotal);

            string sub = "Cliente: " + cliente + "   |   Fecha: " + DateTime.Now.ToString("dd/MM/yyyy HH:mm") +
                         "   |   Vendedor: " + (Sesion.Actual == null ? "" : Sesion.Actual.Login);
            return DesdeTabla("Comprobante de venta N° " + venta.Id, sub, tabla,
                              "TOTAL: S/ " + venta.Total.ToString("N2", CultureInfo.CurrentCulture));
        }

        private static TableCell Celda(string texto, bool negrita, bool derecha)
        {
            var p = new Paragraph(new Run(texto))
            {
                Margin = new Thickness(0),
                TextAlignment = derecha ? TextAlignment.Right : TextAlignment.Left
            };
            if (negrita) p.FontWeight = FontWeights.Bold;
            return new TableCell(p)
            {
                Padding = new Thickness(4, 2, 4, 2),
                BorderBrush = Brushes.LightGray,
                BorderThickness = new Thickness(0, 0, 0, 1)
            };
        }

        private static string Encabezado(string columna)
        {
            if (columna == "IdVenta") return "N° Venta";
            if (columna == "PrecioUnitario") return "P. Unitario";
            return columna;
        }

        private static bool EsNumero(Type tipo)
        {
            return tipo == typeof(int) || tipo == typeof(long) || tipo == typeof(decimal);
        }

        private static string Formato(object valor)
        {
            if (valor == null || valor == DBNull.Value) return "";
            if (valor is DateTime) return ((DateTime)valor).ToString("dd/MM/yyyy HH:mm");
            if (valor is decimal) return ((decimal)valor).ToString("N2", CultureInfo.CurrentCulture);
            return Convert.ToString(valor, CultureInfo.CurrentCulture);
        }
    }
}
