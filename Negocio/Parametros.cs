// CAPA NEGOCIO – parámetros de la aplicación (vienen del Singleton de configuración de la capa de Datos).
// La capa de Presentación los pide aquí y nunca habla directamente con la capa de Datos.
using Datos;

namespace Negocio
{
    public static class Parametros
    {
        public static int MinutosInactividad { get { return Configuracion.Instancia.MinutosInactividad; } }
        public static int UmbralStockBajo { get { return Configuracion.Instancia.UmbralStockBajo; } }
        public static bool AutenticacionHardware { get { return Configuracion.Instancia.AutenticacionHardware; } }
    }
}
