// CAPA DATOS – tabla Cliente (consultas parametrizadas)
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Entidades;

namespace Datos
{
    public class ClienteDAL
    {
        private const string COLUMNAS = "IdCliente, Documento, Nombre, Telefono, Email";

        public void Insertar(Cliente c)
        {
            try
            {
                using (SqlConnection cn = Conexion.Instancia.Crear())
                using (SqlCommand cmd = new SqlCommand(
                    "INSERT INTO Cliente (Documento, Nombre, Telefono, Email) VALUES (@doc, @nom, @tel, @mail)", cn))
                {
                    cmd.Parameters.Add("@doc", SqlDbType.VarChar, 11).Value = Util.ADato(c.Documento);
                    cmd.Parameters.Add("@nom", SqlDbType.VarChar, 100).Value = Util.ADato(c.Nombre);
                    cmd.Parameters.Add("@tel", SqlDbType.VarChar, 15).Value = Util.ADato(c.Telefono);
                    cmd.Parameters.Add("@mail", SqlDbType.VarChar, 100).Value = Util.ADato(c.Email);
                    cn.Open();
                    cmd.ExecuteNonQuery();
                }
            }
            catch (SqlException ex)
            {
                if (Util.EsDuplicado(ex)) throw new ArgumentException("Ya existe un cliente con ese documento.");
                throw;
            }
        }

        public void Actualizar(Cliente c)
        {
            try
            {
                using (SqlConnection cn = Conexion.Instancia.Crear())
                using (SqlCommand cmd = new SqlCommand(
                    "UPDATE Cliente SET Documento = @doc, Nombre = @nom, Telefono = @tel, Email = @mail " +
                    "WHERE IdCliente = @id", cn))
                {
                    cmd.Parameters.Add("@id", SqlDbType.Int).Value = c.Id;
                    cmd.Parameters.Add("@doc", SqlDbType.VarChar, 11).Value = Util.ADato(c.Documento);
                    cmd.Parameters.Add("@nom", SqlDbType.VarChar, 100).Value = Util.ADato(c.Nombre);
                    cmd.Parameters.Add("@tel", SqlDbType.VarChar, 15).Value = Util.ADato(c.Telefono);
                    cmd.Parameters.Add("@mail", SqlDbType.VarChar, 100).Value = Util.ADato(c.Email);
                    cn.Open();
                    cmd.ExecuteNonQuery();
                }
            }
            catch (SqlException ex)
            {
                if (Util.EsDuplicado(ex)) throw new ArgumentException("Ya existe otro cliente con ese documento.");
                throw;
            }
        }

        // Busca por documento o nombre (texto vacío = todos) con LINQ to SQL.
        // LINQ genera un SELECT parametrizado y escapa los comodines (% _ [), así que no hay inyección SQL.
        public List<Cliente> Buscar(string texto)
        {
            texto = (texto ?? "").Trim();
            using (SqlConnection cn = Conexion.Instancia.Crear())
            using (var db = new TechDataContext(cn))
            {
                var consulta = from c in db.Clientes
                               where c.Documento.Contains(texto) || c.Nombre.Contains(texto)
                               orderby c.Nombre
                               select new Cliente
                               {
                                   Id = c.Id,
                                   Documento = c.Documento,
                                   Nombre = c.Nombre,
                                   Telefono = c.Telefono ?? "",
                                   Email = c.Email ?? ""
                               };
                return consulta.ToList();
            }
        }
    }
}
