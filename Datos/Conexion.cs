// Proyecto: Datos (Biblioteca de clases)
// Referencias: System.Configuration
using System;
using System.Data.SqlClient;

namespace Datos
{
    // Patrón Singleton: una sola instancia que entrega las conexiones a SQL Server.
    public sealed class Conexion
    {
        private static readonly Lazy<Conexion> instancia = new Lazy<Conexion>(() => new Conexion());
        private readonly string cadena;

        private Conexion()                              // constructor privado
        {
            cadena = Configuracion.Instancia.CadenaConexion;
        }

        public static Conexion Instancia { get { return instancia.Value; } }   // la única instancia (segura con hilos)

        // Cada operación crea y cierra SU PROPIA conexión (no se comparte abierta, así funciona bien con hilos).
        public SqlConnection Crear()
        {
            return new SqlConnection(cadena);
        }
    }
}
