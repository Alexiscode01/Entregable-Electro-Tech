// CAPA DATOS – tabla Producto (consultas parametrizadas)
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Entidades;

namespace Datos
{
    public class ProductoDAL
    {
        private const string COLUMNAS = "IdProducto, Codigo, Nombre, Categoria, Precio, Stock";

        public void Insertar(Producto p)
        {
            try
            {
                using (SqlConnection cn = Conexion.Instancia.Crear())
                using (SqlCommand cmd = new SqlCommand(
                    "INSERT INTO Producto (Codigo, Nombre, Categoria, Precio, Stock) " +
                    "VALUES (@cod, @nom, @cat, @precio, @stock)", cn))
                {
                    Parametros(cmd, p);
                    cn.Open();
                    cmd.ExecuteNonQuery();
                }
            }
            catch (SqlException ex)
            {
                if (Util.EsDuplicado(ex)) throw new ArgumentException("Ya existe un producto con ese código.");
                throw;
            }
        }

        public void Actualizar(Producto p)
        {
            try
            {
                using (SqlConnection cn = Conexion.Instancia.Crear())
                using (SqlCommand cmd = new SqlCommand(
                    "UPDATE Producto SET Codigo = @cod, Nombre = @nom, Categoria = @cat, " +
                    "Precio = @precio, Stock = @stock WHERE IdProducto = @id", cn))
                {
                    cmd.Parameters.Add("@id", SqlDbType.Int).Value = p.Id;
                    Parametros(cmd, p);
                    cn.Open();
                    cmd.ExecuteNonQuery();
                }
            }
            catch (SqlException ex)
            {
                if (Util.EsDuplicado(ex)) throw new ArgumentException("Ya existe otro producto con ese código.");
                throw;
            }
        }

        private static void Parametros(SqlCommand cmd, Producto p)
        {
            cmd.Parameters.Add("@cod", SqlDbType.VarChar, 20).Value = Util.ADato(p.Codigo);
            cmd.Parameters.Add("@nom", SqlDbType.VarChar, 100).Value = Util.ADato(p.Nombre);
            cmd.Parameters.Add("@cat", SqlDbType.VarChar, 50).Value = Util.ADato(p.Categoria);
            SqlParameter precio = cmd.Parameters.Add("@precio", SqlDbType.Decimal);
            precio.Precision = 10; precio.Scale = 2; precio.Value = p.Precio;
            cmd.Parameters.Add("@stock", SqlDbType.Int).Value = p.Stock;
        }

        // Busca por código, nombre o categoría (texto vacío = todos) con LINQ to SQL.
        public List<Producto> Buscar(string texto)
        {
            texto = (texto ?? "").Trim();
            using (SqlConnection cn = Conexion.Instancia.Crear())
            using (var db = new TechDataContext(cn))
            {
                var consulta = from p in db.Productos
                               where p.Codigo.Contains(texto) || p.Nombre.Contains(texto) || p.Categoria.Contains(texto)
                               orderby p.Nombre
                               select new Producto
                               {
                                   Id = p.Id,
                                   Codigo = p.Codigo,
                                   Nombre = p.Nombre,
                                   Categoria = p.Categoria ?? "",
                                   Precio = p.Precio,
                                   Stock = p.Stock
                               };
                return consulta.ToList();
            }
        }

        // Cuántos productos tienen stock igual o menor al mínimo (alerta del Timer). Un solo COUNT en SQL.
        public int ContarStockBajo(int minimo)
        {
            using (SqlConnection cn = Conexion.Instancia.Crear())
            using (var db = new TechDataContext(cn))
            {
                return db.Productos.Count(p => p.Stock <= minimo);
            }
        }
    }
}
