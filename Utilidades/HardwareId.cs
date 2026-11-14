// BIBLIOTECA UTILIDADES – identificador del equipo para la autenticación basada en hardware.
// Combina datos de la placa base, el procesador y la BIOS (WMI) y los convierte en un hash SHA-256.
// El identificador NO se puede escribir a mano: depende del hardware de la máquina.
using System;
using System.Management;
using System.Security.Cryptography;
using System.Text;

namespace Utilidades
{
    public static class HardwareId
    {
        private static string cache;

        public static string Obtener()
        {
            if (cache != null) return cache;

            string datos = Wmi("Win32_BaseBoard", "SerialNumber") + "|" +
                           Wmi("Win32_Processor", "ProcessorId") + "|" +
                           Wmi("Win32_BIOS", "SerialNumber");

            // Si WMI no devolvió nada (equipo virtual o sin permisos), se usa el nombre del equipo.
            if (datos.Replace("|", "").Trim().Length == 0) datos = Environment.MachineName;

            using (SHA256 sha = SHA256.Create())
            {
                byte[] h = sha.ComputeHash(Encoding.UTF8.GetBytes(datos));
                var sb = new StringBuilder(64);
                foreach (byte b in h) sb.Append(b.ToString("x2"));
                cache = sb.ToString();
            }
            return cache;
        }

        public static string NombreEquipo()
        {
            return Environment.MachineName;
        }

        // Consulta WMI con clase y propiedad fijas (no hay texto del usuario dentro de la consulta).
        private static string Wmi(string clase, string propiedad)
        {
            try
            {
                using (var buscador = new ManagementObjectSearcher("SELECT " + propiedad + " FROM " + clase))
                {
                    foreach (ManagementBaseObject o in buscador.Get())
                    {
                        object v = o[propiedad];
                        if (v != null) return v.ToString().Trim();
                    }
                }
            }
            catch
            {
                // sin acceso a WMI: se devuelve vacío
            }
            return "";
        }
    }
}
