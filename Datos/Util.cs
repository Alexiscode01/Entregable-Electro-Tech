// CAPA DATOS – utilidades comunes para las consultas
using System;

namespace Datos
{
    internal static class Util
    {
        // null -> DBNull para que SQL Server reciba un valor válido.
        public static object ADato(object valor)
        {
            return valor ?? DBNull.Value;
        }

        // Prepara un texto para usarlo en LIKE (el usuario no puede meter comodines propios).
        // Se usa junto con:  LIKE @texto ESCAPE '\'
        public static string PatronLike(string texto)
        {
            if (texto == null) texto = "";
            texto = texto.Replace("\\", "\\\\").Replace("%", "\\%").Replace("_", "\\_").Replace("[", "\\[");
            return "%" + texto + "%";
        }

        // 2627 y 2601 = violación de UNIQUE en SQL Server.
        public static bool EsDuplicado(System.Data.SqlClient.SqlException ex)
        {
            return ex.Number == 2627 || ex.Number == 2601;
        }
    }
}
