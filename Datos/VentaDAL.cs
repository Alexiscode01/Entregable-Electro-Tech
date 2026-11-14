// CAPA DATOS – ventas con TRANSACCIÓN (COMMIT / ROLLBACK) y consultas para reportes.
// Una venta toca tres tablas (Venta, DetalleVenta y Producto.Stock): o se guardan TODAS o NINGUNA.
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Entidades;

namespace Datos
{
    public class VentaDAL
    {
        // Registra la venta completa. Devuelve la venta con su Id y su Total reales.
        public Venta Registrar(Venta v)
        {
            using (SqlConnection cn = Conexion.Instancia.Crear())
            {
                cn.Open();
                using (SqlTransaction tx = cn.BeginTransaction())          // BEGIN TRANSACTION
                {
                    try
                    {
                        // 1) Cabecera de la venta (el total se calcula al final con los precios de la BD)
                        using (SqlCommand cmd = new SqlCommand(
                            "INSERT INTO Venta (IdCliente, IdUsuario, Total) VALUES (@cli, @usu, 0); " +
                            "SELECT CAST(SCOPE_IDENTITY() AS INT);", cn, tx))
                        {
                            cmd.Parameters.Add("@cli", SqlDbType.Int).Value = v.IdCliente;
                            cmd.Parameters.Add("@usu", SqlDbType.Int).Value = v.IdUsuario;
                            v.Id = (int)cmd.ExecuteScalar();
                        }

                        decimal total = 0;
                        foreach (DetalleVenta d in v.Detalles)
                        {
                            // 2) Descuenta el stock SOLO si alcanza y devuelve el precio vigente (operación atómica)
                            object precio;
                            using (SqlCommand cmd = new SqlCommand(
                                "UPDATE Producto SET Stock = Stock - @cant " +
                                "OUTPUT inserted.Precio " +
                                "WHERE IdProducto = @prod AND Stock >= @cant", cn, tx))
                            {
                                cmd.Parameters.Add("@cant", SqlDbType.Int).Value = d.Cantidad;
                                cmd.Parameters.Add("@prod", SqlDbType.Int).Value = d.IdProducto;
                                precio = cmd.ExecuteScalar();
                            }
                            if (precio == null)
                                throw new InvalidOperationException(
                                    "Stock insuficiente o producto inexistente: " + NombreProducto(cn, tx, d.IdProducto) + ".");

                            decimal pu = (decimal)precio;

                            // 3) Detalle de la venta
                            using (SqlCommand cmd = new SqlCommand(
                                "INSERT INTO DetalleVenta (IdVenta, IdProducto, Cantidad, PrecioUnitario) " +
                                "VALUES (@venta, @prod, @cant, @pu)", cn, tx))
                            {
                                cmd.Parameters.Add("@venta", SqlDbType.Int).Value = v.Id;
                                cmd.Parameters.Add("@prod", SqlDbType.Int).Value = d.IdProducto;
                                cmd.Parameters.Add("@cant", SqlDbType.Int).Value = d.Cantidad;
                                SqlParameter p = cmd.Parameters.Add("@pu", SqlDbType.Decimal);
                                p.Precision = 10; p.Scale = 2; p.Value = pu;
                                cmd.ExecuteNonQuery();
                            }

                            d.PrecioUnitario = pu;
                            total += pu * d.Cantidad;
                        }

                        // 4) Total final de la venta
                        using (SqlCommand cmd = new SqlCommand(
                            "UPDATE Venta SET Total = @total WHERE IdVenta = @id", cn, tx))
                        {
                            SqlParameter p = cmd.Parameters.Add("@total", SqlDbType.Decimal);
                            p.Precision = 12; p.Scale = 2; p.Value = total;
                            cmd.Parameters.Add("@id", SqlDbType.Int).Value = v.Id;
                            cmd.ExecuteNonQuery();
                        }

                        tx.Commit();                                        // COMMIT: todo se guarda
                        v.Total = total;
                        return v;
                    }
                    catch
                    {
                        try { tx.Rollback(); } catch { }                    // ROLLBACK: no queda nada a medias
                        throw;
                    }
                }
            }
        }

        private static string NombreProducto(SqlConnection cn, SqlTransaction tx, int idProducto)
        {
            using (SqlCommand cmd = new SqlCommand(
                "SELECT Nombre FROM Producto WHERE IdProducto = @id", cn, tx))
            {
                cmd.Parameters.Add("@id", SqlDbType.Int).Value = idProducto;
                object r = cmd.ExecuteScalar();
                return r == null ? "(desconocido)" : (string)r;
            }
        }

        // Anula una venta con el procedimiento almacenado sp_AnularVenta.
        // La transacción (BEGIN TRAN / COMMIT / ROLLBACK) está escrita en T-SQL dentro del procedimiento.
        public void Anular(int idVenta)
        {
            try
            {
                using (SqlConnection cn = Conexion.Instancia.Crear())
                using (SqlCommand cmd = new SqlCommand("dbo.sp_AnularVenta", cn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@IdVenta", SqlDbType.Int).Value = idVenta;
                    cn.Open();
                    cmd.ExecuteNonQuery();
                }
            }
            catch (SqlException ex)
            {
                // 50001 = error propio del procedimiento (venta inexistente o ya anulada)
                if (ex.Number == 50001) throw new InvalidOperationException(ex.Message);
                throw;
            }
        }
    }
}
