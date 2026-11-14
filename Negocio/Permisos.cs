// CAPA NEGOCIO – niveles de acceso por rol.
// Los menús del formulario se ocultan según esta tabla, y ADEMÁS cada operación de negocio la vuelve a
// comprobar: así, aunque alguien salte la pantalla, la capa de negocio rechaza la operación.
//
//                  Administrador   Vendedor   Almacenero
// Clientes              sí            sí          no
// Productos (stock)     sí            no          sí
// Ventas                sí            sí          no
// Reportes              sí         solo propios   no
// Usuarios              sí            no          no
// Anular ventas         sí            no          no
using System;

namespace Negocio
{
    public enum Modulo { Clientes, Productos, Ventas, Reportes, Usuarios, Anulaciones }

    public static class Permisos
    {
        public const string Administrador = "Administrador";
        public const string Vendedor = "Vendedor";
        public const string Almacenero = "Almacenero";

        public static readonly string[] Roles = { Administrador, Vendedor, Almacenero };

        public static bool Puede(string rol, Modulo m)
        {
            if (rol == Administrador) return true;
            switch (m)
            {
                case Modulo.Clientes: return rol == Vendedor;
                case Modulo.Ventas: return rol == Vendedor;
                case Modulo.Reportes: return rol == Vendedor;
                case Modulo.Productos: return rol == Almacenero;
                default: return false;          // Usuarios y Anulaciones: solo Administrador
            }
        }

        // Lanza excepción si no hay sesión o el rol no tiene acceso a ninguno de los módulos indicados.
        public static void Exigir(params Modulo[] modulos)
        {
            var u = Sesion.Actual;
            if (u != null)
            {
                foreach (Modulo m in modulos)
                    if (Puede(u.Rol, m)) return;
            }
            throw new UnauthorizedAccessException("No tiene permiso para realizar esta operación.");
        }
    }
}
