// CAPA NEGOCIO – reglas de clientes
using System;
using System.Collections.Generic;
using System.Net.Mail;
using Datos;
using Entidades;

namespace Negocio
{
    public class ClienteBLL
    {
        private readonly ClienteDAL dal = new ClienteDAL();

        // Id = 0 -> cliente nuevo; Id > 0 -> se actualiza.
        public void Guardar(Cliente c)
        {
            Permisos.Exigir(Modulo.Clientes);

            c.Documento = (c.Documento ?? "").Trim();
            c.Nombre = (c.Nombre ?? "").Trim();
            c.Telefono = (c.Telefono ?? "").Trim();
            c.Email = (c.Email ?? "").Trim();

            if (!SoloDigitos(c.Documento) || (c.Documento.Length != 8 && c.Documento.Length != 11))
                throw new ArgumentException("El documento debe ser un DNI (8 dígitos) o un RUC (11 dígitos).");
            if (c.Nombre.Length == 0 || c.Nombre.Length > 100)
                throw new ArgumentException("El nombre es obligatorio y tiene como máximo 100 caracteres.");
            if (c.Telefono.Length > 0 && (!SoloDigitos(c.Telefono) || c.Telefono.Length < 6 || c.Telefono.Length > 15))
                throw new ArgumentException("El teléfono debe tener entre 6 y 15 dígitos.");
            if (c.Email.Length > 0 && !EmailValido(c.Email))
                throw new ArgumentException("El correo electrónico no es válido.");

            if (c.Id == 0) dal.Insertar(c); else dal.Actualizar(c);
        }

        public List<Cliente> Buscar(string texto)
        {
            Permisos.Exigir(Modulo.Clientes, Modulo.Ventas);
            return dal.Buscar(texto);
        }

        private static bool SoloDigitos(string s)
        {
            if (string.IsNullOrEmpty(s)) return false;
            foreach (char ch in s) if (ch < '0' || ch > '9') return false;
            return true;
        }

        private static bool EmailValido(string email)
        {
            if (email.Length > 100) return false;
            try { return new MailAddress(email).Address == email; }
            catch { return false; }
        }
    }
}
