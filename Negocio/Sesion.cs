// CAPA NEGOCIO – usuario que tiene la sesión abierta
using Entidades;

namespace Negocio
{
    public static class Sesion
    {
        public static Usuario Actual { get; private set; }

        public static void Iniciar(Usuario u)
        {
            Actual = u;
        }

        public static void Cerrar()
        {
            Actual = null;
        }
    }
}
