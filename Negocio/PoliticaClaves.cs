// CAPA NEGOCIO – política de contraseñas: qué contraseñas se aceptan al registrar un usuario.
// Una contraseña solo con números o muy corta se rechaza.
using System.Collections.Generic;
using System.Linq;

namespace Negocio
{
    public static class PoliticaClaves
    {
        public const int Minimo = 8;
        public const int Maximo = 64;

        // Palabras muy usadas: se rechazan aunque se les agreguen números o símbolos (por ejemplo Admin#2026).
        private static readonly string[] Comunes =
        {
            "password", "contrasena", "contraseña", "admin", "administrador", "administrator", "qwerty", "abc",
            "letmein", "welcome", "iloveyou", "techsolutions", "tech", "usuario", "vendedor", "almacenero", "peru", "lima"
        };

        // Devuelve null si la contraseña es válida; si no, el mensaje que explica qué le falta.
        public static string Validar(string clave, string login)
        {
            if (string.IsNullOrEmpty(clave)) return "Escriba una contraseña.";
            if (clave.Any(char.IsWhiteSpace)) return "La contraseña no puede contener espacios.";

            var faltan = new List<string>();
            if (clave.Length < Minimo || clave.Length > Maximo) faltan.Add("entre " + Minimo + " y " + Maximo + " caracteres");
            if (!clave.Any(char.IsUpper)) faltan.Add("una letra mayúscula");
            if (!clave.Any(char.IsLower)) faltan.Add("una letra minúscula");
            if (!clave.Any(char.IsDigit)) faltan.Add("un número");
            if (!clave.Any(c => !char.IsLetterOrDigit(c))) faltan.Add("un símbolo (por ejemplo # $ % & * !)");
            if (faltan.Count > 0)
                return "Contraseña insegura. Debe tener: " + string.Join(", ", faltan) + ".";

            string minuscula = clave.ToLowerInvariant();
            login = (login ?? "").Trim().ToLowerInvariant();
            if (login.Length >= 3 && minuscula.Contains(login))
                return "La contraseña no puede contener el nombre de usuario.";

            string soloLetras = new string(minuscula.Where(char.IsLetter).ToArray());
            if (Comunes.Contains(soloLetras))
                return "La contraseña es demasiado común. Elija otra.";

            return null;
        }
    }
}
