// CAPA NEGOCIO – reportes con parámetros (procedimientos almacenados) y resúmenes con LINQ to DataSet
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Datos;
using Entidades;

namespace Negocio
{
    // Resumen calculado con LINQ sobre la tabla del reporte.
    public class ResumenVentas
    {
        public int CantidadVentas { get; set; }                 // ventas activas
        public int CantidadAnuladas { get; set; }
        public decimal Total { get; set; }                      // suma de ventas activas
        public decimal Promedio { get; set; }
        public List<KeyValuePair<string, decimal>> PorVendedor { get; set; }
    }

    public class ReporteBLL
    {
        private readonly ReporteDAL dal = new ReporteDAL();

        // El Vendedor solo ve lo suyo; el Administrador puede filtrar por vendedor o ver todos.
        private static int? FiltroVendedor(int? idVendedor)
        {
            return Sesion.Actual.Rol == Permisos.Vendedor ? Sesion.Actual.Id : idVendedor;
        }

        private static void ValidarFechas(DateTime desde, DateTime hasta)
        {
            if (desde.Date > hasta.Date)
                throw new ArgumentException("La fecha inicial no puede ser mayor que la final.");
        }

        public DataTable VentasPorFechas(DateTime desde, DateTime hasta, int? idVendedor)
        {
            Permisos.Exigir(Modulo.Reportes);
            ValidarFechas(desde, hasta);
            return dal.Ventas(desde, hasta, FiltroVendedor(idVendedor));
        }

        public DataTable DetalleVenta(int idVenta)
        {
            Permisos.Exigir(Modulo.Reportes);
            int? soloUsuario = Sesion.Actual.Rol == Permisos.Vendedor ? Sesion.Actual.Id : (int?)null;
            return dal.DetalleVenta(idVenta, soloUsuario);
        }

        public DataTable ProductosMasVendidos(DateTime desde, DateTime hasta, int? idVendedor)
        {
            Permisos.Exigir(Modulo.Reportes);
            ValidarFechas(desde, hasta);
            return dal.ProductosMasVendidos(desde, hasta, FiltroVendedor(idVendedor));
        }

        public DataTable StockBajo(int minimo)
        {
            Permisos.Exigir(Modulo.Productos);
            if (minimo < 0) throw new ArgumentException("El stock mínimo no puede ser negativo.");
            return dal.StockBajo(minimo);
        }

        // LINQ to DataSet: filtra, suma, promedia y agrupa sin volver a consultar la base de datos.
        public ResumenVentas Resumir(DataTable ventas)
        {
            List<DataRow> activas = ventas.AsEnumerable()
                .Where(r => r.Field<string>("Estado") == "Activa")
                .ToList();

            return new ResumenVentas
            {
                CantidadVentas = activas.Count,
                CantidadAnuladas = ventas.Rows.Count - activas.Count,
                Total = activas.Sum(r => r.Field<decimal>("Total")),
                Promedio = activas.Count == 0 ? 0m : activas.Average(r => r.Field<decimal>("Total")),
                PorVendedor = activas
                    .GroupBy(r => r.Field<string>("Vendedor"))
                    .Select(g => new KeyValuePair<string, decimal>(g.Key, g.Sum(r => r.Field<decimal>("Total"))))
                    .OrderByDescending(p => p.Value)
                    .ToList()
            };
        }
    }
}
