// CAPA NEGOCIO – reglas de inicio de sesión, registro de usuarios y autenticación por hardware
using System;
using System.Collections.Generic;
using Datos;
using Entidades;
using Utilidades;

namespace Negocio
{
    // Se lanza cuando la cuenta ya está bloqueada por 3 intentos fallidos.
    public class CuentaBloqueadaException : UnauthorizedAccessException
    {
        public CuentaBloqueadaException()
            : base("Cuenta bloqueada por 3 intentos fallidos. Pida al administrador que la desbloquee.") { }
    }

    // Se lanza cuando el Administrador entra desde un equipo que no está autorizado.
    public class EquipoNoAutorizadoException : UnauthorizedAccessException
    {
        public EquipoNoAutorizadoException()
            : base("Este equipo no está autorizado para el rol Administrador.") { }
    }

    public class UsuarioBLL
    {
        private readonly UsuarioDAL dal = new UsuarioDAL();
        private readonly EquipoDAL equipos = new EquipoDAL();
        public static readonly string[] Roles = Permisos.Roles;

        public Usuario IniciarSesion(string login, string clave)
        {
            Usuario u = dal.BuscarPorLogin(login);        // 1) trae SU sal y SU hash

            // Cuenta bloqueada: no se prueba la contraseña ni se suman más intentos.
            if (u != null && u.Bloqueado)
            {
                Bitacora.Registrar("LOGIN BLOQUEADO usuario=" + login);
                throw new CuentaBloqueadaException();
            }

            // 2) y 3) Verificar calcula el hash de lo escrito con esa sal y lo compara con el guardado
            bool correcta;
            if (u == null)
            {
                Seguridad.Hash(clave ?? "", Seguridad.NuevaSal());   // mismo tiempo de respuesta si el usuario no existe
                correcta = false;
            }
            else
            {
                correcta = Seguridad.Verificar(clave ?? "", u.Sal, u.ClaveHash);
            }

            if (!correcta)
            {
                if (u != null) dal.SumarIntentoFallido(login);   // al tercer intento se bloquea
                Bitacora.Registrar("LOGIN FALLIDO usuario=" + login);
                // Mensaje genérico: no se dice si falló el usuario o la contraseña.
                throw new UnauthorizedAccessException("Usuario o contraseña incorrectos.");
            }
            dal.ReiniciarIntentos(u.Id);

            // Autenticación basada en hardware: el Administrador solo entra desde un equipo autorizado.
            if (u.Rol == Permisos.Administrador && !EquipoPermitido())
            {
                Bitacora.Registrar("LOGIN EQUIPO NO AUTORIZADO usuario=" + login + " equipo=" + HardwareId.NombreEquipo());
                throw new EquipoNoAutorizadoException();
            }

            Sesion.Iniciar(u);
            Bitacora.Registrar("LOGIN CORRECTO usuario=" + login + " rol=" + u.Rol);
            return u;
        }

        // La regla se aplica solo si está activada en App.config y ya hay al menos un equipo autorizado.
        private bool EquipoPermitido()
        {
            if (!Parametros.AutenticacionHardware) return true;
            if (equipos.Contar() == 0) return true;
            return equipos.Existe(HardwareId.Obtener());
        }

        // Autoriza el equipo actual (solo Administrador).
        public void AutorizarEquipoActual()
        {
            Permisos.Exigir(Modulo.Usuarios);
            string hw = HardwareId.Obtener();
            if (equipos.Existe(hw)) throw new ArgumentException("Este equipo ya está autorizado.");
            equipos.Insertar(hw, HardwareId.NombreEquipo());
            Bitacora.Registrar("EQUIPO AUTORIZADO equipo=" + HardwareId.NombreEquipo());
        }

        public string NombreEquipoActual()
        {
            return HardwareId.NombreEquipo();
        }

        public void Registrar(string login, string clave, string confirmar, string rol)
        {
            // El primer usuario (Administrador) se crea sin sesión; después solo un Administrador registra.
            bool primero = !HayUsuarios();
            if (!primero) Permisos.Exigir(Modulo.Usuarios);

            if (string.IsNullOrWhiteSpace(login) || login.Length > 30)
                throw new ArgumentException("El usuario es obligatorio y tiene como máximo 30 caracteres.");
            string problema = PoliticaClaves.Validar(clave, login);
            if (problema != null)
                throw new ArgumentException(problema);
            if (clave != confirmar)
                throw new ArgumentException("Las contraseñas no coinciden.");
            if (Array.IndexOf(Roles, rol) < 0)
                throw new ArgumentException("Rol no válido.");
            if (dal.BuscarPorLogin(login) != null)
                throw new ArgumentException("Ese usuario ya existe.");

            string sal = Seguridad.NuevaSal();           // 1) genera una sal aleatoria
            string hash = Seguridad.Hash(clave, sal);    // 2) calcula el hash de la clave con esa sal
            dal.Insertar(new Usuario { Login = login, Sal = sal, ClaveHash = hash, Rol = rol });   // 3) guarda (NO la clave)
            Bitacora.Registrar("USUARIO REGISTRADO usuario=" + login + " rol=" + rol);

            // El equipo donde se crea el primer Administrador queda autorizado automáticamente.
            if (primero && equipos.Contar() == 0)
                equipos.Insertar(HardwareId.Obtener(), HardwareId.NombreEquipo());
        }

        public bool HayUsuarios()
        {
            return dal.Contar() > 0;
        }

        // Búsqueda siempre parametrizada (segura contra inyección SQL).
        public List<Usuario> Buscar(string login)
        {
            Permisos.Exigir(Modulo.Usuarios);
            return dal.Buscar(login);
        }

        public void Desbloquear(int idUsuario)
        {
            Permisos.Exigir(Modulo.Usuarios);
            dal.Desbloquear(idUsuario);
            Bitacora.Registrar("CUENTA DESBLOQUEADA idUsuario=" + idUsuario);
        }

        // Vendedores para el filtro de los reportes.
        public List<Usuario> ListarVendedores()
        {
            Permisos.Exigir(Modulo.Reportes);
            return dal.ListarPorRol(Permisos.Vendedor);
        }
    }
}
