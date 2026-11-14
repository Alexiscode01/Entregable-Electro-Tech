// CAPA DATOS – reportes con PROCEDIMIENTOS ALMACENADOS y parámetros (SqlParameter).
// Cada método devuelve un DataTable listo para mostrar, imprimir o exportar.
using System;
using System.Data;
using System.Data.SqlClient;

namespace Datos
{
    public class ReporteDAL
    {
        // Ventas entre dos fechas. idUsuario = null -> de todos los vendedores.
        public DataTable Ventas(DateTime desde, DateTime hasta, int? idUsuario)
        {
            return Ejecutar("dbo.sp_ReporteVentas",
                Fecha("@Desde", desde), Fecha("@Hasta", hasta), Entero("@IdUsuario", idUsuario));
        }

        // Productos de una venta. idUsuario != null -> solo si la venta es de ese vendedor.
        public DataTable DetalleVenta(int idVenta, int? idUsuario)
        {
            return Ejecutar("dbo.sp_DetalleVenta", Entero("@IdVenta", idVenta), Entero("@IdUsuario", idUsuario));
        }

        public DataTable ProductosMasVendidos(DateTime desde, DateTime hasta, int? idUsuario)
        {
            return Ejecutar("dbo.sp_VentasPorProducto",
                Fecha("@Desde", desde), Fecha("@Hasta", hasta), Entero("@IdUsuario", idUsuario));
        }

        public DataTable StockBajo(int minimo)
        {
            return Ejecutar("dbo.sp_StockBajo", Entero("@Minimo", minimo));
        }

        // ---- ayudas ----
        private static DataTable Ejecutar(string procedimiento, params SqlParameter[] parametros)
        {
            var tabla = new DataTable();
            using (SqlConnection cn = Conexion.Instancia.Crear())
            using (SqlCommand cmd = new SqlCommand(procedimiento, cn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddRange(parametros);
                using (var adaptador = new SqlDataAdapter(cmd))
                {
                    adaptador.Fill(tabla);          // abre y cierra la conexión por sí solo
                }
            }
            tabla.TableName = "Reporte";
            return tabla;
        }

        private static SqlParameter Fecha(string nombre, DateTime valor)
        {
            return new SqlParameter(nombre, SqlDbType.Date) { Value = valor.Date };
        }

        private static SqlParameter Entero(string nombre, int? valor)
        {
            return new SqlParameter(nombre, SqlDbType.Int) { Value = valor.HasValue ? (object)valor.Value : DBNull.Value };
        }
    }
}
