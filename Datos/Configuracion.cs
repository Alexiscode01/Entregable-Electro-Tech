// CAPA DATOS – Patrón Singleton para la GESTIÓN DE CONFIGURACIÓN.
// Lee una sola vez el App.config (cadena de conexión y parámetros) y lo comparte con toda la aplicación.
using System;
using System.Configuration;

namespace Datos
{
    public sealed class Configuracion
    {
        // Lazy<T> garantiza una sola instancia aunque varios hilos la pidan a la vez.
        private static readonly Lazy<Configuracion> instancia =
            new Lazy<Configuracion>(() => new Configuracion());

        public static Configuracion Instancia { get { return instancia.Value; } }

        public string CadenaConexion { get; private set; }
        public int MinutosInactividad { get; private set; }
        public int UmbralStockBajo { get; private set; }
        public bool AutenticacionHardware { get; private set; }

        private Configuracion()                       // constructor privado
        {
            CadenaConexion = ConfigurationManager.ConnectionStrings["TechSolutions"].ConnectionString;
            MinutosInactividad = LeerEntero("MinutosInactividad", 5, 1, 480);
            UmbralStockBajo = LeerEntero("UmbralStockBajo", 5, 0, 100000);
            AutenticacionHardware = !string.Equals(
                ConfigurationManager.AppSettings["AutenticacionHardware"], "false", StringComparison.OrdinalIgnoreCase);
        }

        private static int LeerEntero(string clave, int porDefecto, int minimo, int maximo)
        {
            int valor;
            if (!int.TryParse(ConfigurationManager.AppSettings[clave], out valor)) return porDefecto;
            return valor < minimo || valor > maximo ? porDefecto : valor;
        }
    }
}
