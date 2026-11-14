// CAPA DATOS – equipos autorizados para la autenticación basada en hardware
using System.Data;
using System.Data.SqlClient;

namespace Datos
{
    public class EquipoDAL
    {
        public int Contar()
        {
            using (SqlConnection cn = Conexion.Instancia.Crear())
            using (SqlCommand cmd = new SqlCommand("SELECT COUNT(*) FROM EquipoAutorizado", cn))
            {
                cn.Open();
                return (int)cmd.ExecuteScalar();
            }
        }

        public bool Existe(string hardwareId)
        {
            using (SqlConnection cn = Conexion.Instancia.Crear())
            using (SqlCommand cmd = new SqlCommand(
                "SELECT COUNT(*) FROM EquipoAutorizado WHERE HardwareId = @hw", cn))
            {
                cmd.Parameters.Add("@hw", SqlDbType.VarChar, 64).Value = hardwareId;
                cn.Open();
                return (int)cmd.ExecuteScalar() > 0;
            }
        }

        public void Insertar(string hardwareId, string nombre)
        {
            using (SqlConnection cn = Conexion.Instancia.Crear())
            using (SqlCommand cmd = new SqlCommand(
                "INSERT INTO EquipoAutorizado (HardwareId, Nombre) VALUES (@hw, @nom)", cn))
            {
                cmd.Parameters.Add("@hw", SqlDbType.VarChar, 64).Value = hardwareId;
                cmd.Parameters.Add("@nom", SqlDbType.VarChar, 100).Value = nombre;
                cn.Open();
                cmd.ExecuteNonQuery();
            }
        }
    }
}
