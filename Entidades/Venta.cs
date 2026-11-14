// Proyecto: Entidades
using System;
using System.Collections.Generic;

namespace Entidades
{
    public class Venta
    {
        public int Id { get; set; }
        public int IdCliente { get; set; }
        public int IdUsuario { get; set; }
        public DateTime Fecha { get; set; }
        public decimal Total { get; set; }
        public string ClienteNombre { get; set; }     // para reportes
        public string Vendedor { get; set; }          // para reportes
        public List<DetalleVenta> Detalles { get; set; }

        public Venta()
        {
            Detalles = new List<DetalleVenta>();
        }
    }
}
