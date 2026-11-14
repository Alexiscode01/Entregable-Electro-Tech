// CAPA NEGOCIO – registro de ventas y reportes
using System;
using System.Collections.Generic;
using Datos;
using Entidades;
using Utilidades;

namespace Negocio
{
    public class VentaBLL
    {
        private readonly VentaDAL dal = new VentaDAL();

        public Venta Registrar(int idCliente, IEnumerable<DetalleVenta> lineas)
        {
            Permisos.Exigir(Modulo.Ventas);

            if (idCliente <= 0)
                throw new ArgumentException("Seleccione un cliente.");

            // Une líneas repetidas del mismo producto y valida cantidades.
            var porProducto = new Dictionary<int, DetalleVenta>();
            foreach (DetalleVenta l in lineas)
            {
                if (l.Cantidad <= 0 || l.Cantidad > 10000)
                    throw new ArgumentException("La cantidad debe estar entre 1 y 10 000.");
                DetalleVenta existente;
                if (porProducto.TryGetValue(l.IdProducto, out existente))
                    existente.Cantidad += l.Cantidad;
                else
                    porProducto[l.IdProducto] = new DetalleVenta { IdProducto = l.IdProducto, Cantidad = l.Cantidad };
            }
            if (porProducto.Count == 0)
                throw new ArgumentException("Agregue al menos un producto a la venta.");

            var venta = new Venta
            {
                IdCliente = idCliente,
                IdUsuario = Sesion.Actual.Id          // el vendedor es SIEMPRE el de la sesión
            };
            venta.Detalles.AddRange(porProducto.Values);

            // Los precios NO vienen de la pantalla: la capa de datos usa los de la base de datos.
            Venta guardada = dal.Registrar(venta);    // transacción COMMIT / ROLLBACK
            Bitacora.Registrar("VENTA REGISTRADA idVenta=" + guardada.Id + " total=" + guardada.Total.ToString("0.00") +
                               " por=" + Sesion.Actual.Login);
            return guardada;
        }

        // Solo el Administrador. La transacción de anulación vive en el procedimiento almacenado sp_AnularVenta:
        // repone el stock y marca la venta como Anulada, o no hace nada si algo falla (ROLLBACK).
        public void Anular(int idVenta)
        {
            Permisos.Exigir(Modulo.Anulaciones);
            if (idVenta <= 0) throw new ArgumentException("Seleccione una venta.");
            dal.Anular(idVenta);
            Bitacora.Registrar("VENTA ANULADA idVenta=" + idVenta + " por=" + Sesion.Actual.Login);
        }
    }
}
