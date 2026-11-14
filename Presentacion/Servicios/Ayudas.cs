// CAPA PRESENTACIÓN – ayudas comunes para las tablas (DataGrid)
using System;
using System.Windows.Controls;
using System.Windows.Data;

namespace Presentacion
{
    public static class Ayudas
    {
        // Da formato a las columnas que se generan solas a partir de un DataTable (reportes).
        public static void ConfigurarColumna(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            if (e.PropertyName == "IdVenta") e.Column.Header = "N° Venta";
            else if (e.PropertyName == "PrecioUnitario") e.Column.Header = "P. Unitario";

            var columna = e.Column as DataGridTextColumn;
            if (columna == null) return;
            var enlace = columna.Binding as Binding;
            if (enlace == null) return;

            if (e.PropertyType == typeof(DateTime)) enlace.StringFormat = "dd/MM/yyyy HH:mm";
            else if (e.PropertyType == typeof(decimal)) enlace.StringFormat = "N2";
        }
    }
}
