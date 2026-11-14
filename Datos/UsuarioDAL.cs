// CAPA DATOS – tabla Usuario.
// Todas las consultas son PARAMETRIZADAS: el texto que escribe el usuario viaja como DATO,
// nunca como parte del SQL. Así se evita la inyección SQL.
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Entidades;

namespace Datos
{
    public class UsuarioDAL
    {
        // Columnas que se leen siempre, en el MISMO orden que usa el método Leer().
        private const string COLUMNAS =
            "IdUsuario, Login, ClaveHash, Sal, Rol, IntentosFallidos, Bloqueado";

        // Convierte null en DBNull para que SQL Server reciba un valor válido.
        private static object ADato(string valor)
        {
            return (object)valor ?? DBNull.Value;
        }

        // Busca al usuario que intenta iniciar sesión.
        public Usuario BuscarPorLogin(string login)
        {
            using (SqlConnection cn = Conexion.Instancia.Crear())
            using (SqlCommand cmd = new SqlCommand(
                "SELECT " + COLUMNAS + " FROM Usuario WHERE Login = @login", cn))
            {
                // @login con el MISMO tipo y tamaño que la columna: VARCHAR(30)
                cmd.Parameters.Add("@login", SqlDbType.VarChar, 30).Value = ADato(login);
                cn.Open();
                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    if (!dr.Read()) return null;
                    return Leer(dr);
                }
            }
        }

        public void Insertar(Usuario u)
        {
            using (SqlConnection cn = Conexion.Instancia.Crear())
            using (SqlCommand cmd = new SqlCommand(
                "INSERT INTO Usuario (Login, ClaveHash, Sal, Rol) VALUES (@login, @hash, @sal, @rol)", cn))
            {
                cmd.Parameters.Add("@login", SqlDbType.VarChar, 30).Value = ADato(u.Login);
                cmd.Parameters.Add("@hash", SqlDbType.VarChar, 100).Value = ADato(u.ClaveHash);
                cmd.Parameters.Add("@sal", SqlDbType.VarChar, 50).Value = ADato(u.Sal);
                cmd.Parameters.Add("@rol", SqlDbType.VarChar, 20).Value = ADato(u.Rol);
                cn.Open();
                cmd.ExecuteNonQuery();
            }
        }

        public int Contar()
        {
            using (SqlConnection cn = Conexion.Instancia.Crear())
            using (SqlCommand cmd = new SqlCommand("SELECT COUNT(*) FROM Usuario", cn))
            {
                cn.Open();
                return (int)cmd.ExecuteScalar();
            }
        }

        // Suma un intento fallido y bloquea la cuenta al tercero.
        public void SumarIntentoFallido(string login)
        {
            using (SqlConnection cn = Conexion.Instancia.Crear())
            using (SqlCommand cmd = new SqlCommand(
                "UPDATE Usuario SET IntentosFallidos = IntentosFallidos + 1, " +
                "Bloqueado = CASE WHEN IntentosFallidos + 1 >= 3 THEN 1 ELSE Bloqueado END " +
                "WHERE Login = @login", cn))
            {
                cmd.Parameters.Add("@login", SqlDbType.VarChar, 30).Value = ADato(login);
                cn.Open();
                cmd.ExecuteNonQuery();
            }
        }

        public void ReiniciarIntentos(int idUsuario)
        {
            using (SqlConnection cn = Conexion.Instancia.Crear())
            using (SqlCommand cmd = new SqlCommand(
                "UPDATE Usuario SET IntentosFallidos = 0 WHERE IdUsuario = @id", cn))
            {
                cmd.Parameters.Add("@id", SqlDbType.Int).Value = idUsuario;
                cn.Open();
                cmd.ExecuteNonQuery();
            }
        }

        // ===== Búsqueda de usuarios: SIEMPRE parametrizada =====
        // Si escriben  ' OR 1=1 --  se busca literalmente ese texto como Login
        // (no existe ningún usuario así) y devuelve una lista vacía.
        public List<Usuario> Buscar(string login)
        {
            using (SqlConnection cn = Conexion.Instancia.Crear())
            using (SqlCommand cmd = new SqlCommand(
                "SELECT " + COLUMNAS + " FROM Usuario WHERE Login = @login", cn))
            {
                cmd.Parameters.Add("@login", SqlDbType.VarChar, 30).Value = ADato(login);
                return LeerLista(cn, cmd);
            }
        }

        // El administrador desbloquea una cuenta (vuelve a tener 3 intentos).
        public void Desbloquear(int idUsuario)
        {
            using (SqlConnection cn = Conexion.Instancia.Crear())
            using (SqlCommand cmd = new SqlCommand(
                "UPDATE Usuario SET Bloqueado = 0, IntentosFallidos = 0 WHERE IdUsuario = @id", cn))
            {
                cmd.Parameters.Add("@id", SqlDbType.Int).Value = idUsuario;
                cn.Open();
                cmd.ExecuteNonQuery();
            }
        }

        // Usuarios de un rol (para el filtro de vendedores en los reportes).
        public List<Usuario> ListarPorRol(string rol)
        {
            using (SqlConnection cn = Conexion.Instancia.Crear())
            using (SqlCommand cmd = new SqlCommand(
                "SELECT " + COLUMNAS + " FROM Usuario WHERE Rol = @rol ORDER BY Login", cn))
            {
                cmd.Parameters.Add("@rol", SqlDbType.VarChar, 20).Value = ADato(rol);
                return LeerLista(cn, cmd);
            }
        }

        private List<Usuario> LeerLista(SqlConnection cn, SqlCommand cmd)
        {
            var lista = new List<Usuario>();
            cn.Open();
            using (SqlDataReader dr = cmd.ExecuteReader())
            {
                while (dr.Read()) lista.Add(Leer(dr));
            }
            return lista;
        }

        // Columna (tipo SQL) -> propiedad (tipo C#):  INT -> int · VARCHAR -> string · BIT -> bool
        private Usuario Leer(SqlDataReader dr)
        {
            return new Usuario
            {
                Id = dr.GetInt32(0),
                Login = dr.GetString(1),
                ClaveHash = dr.GetString(2),
                Sal = dr.GetString(3),
                Rol = dr.GetString(4),
                IntentosFallidos = dr.GetInt32(5),
                Bloqueado = dr.GetBoolean(6)
            };
        }
    }
}