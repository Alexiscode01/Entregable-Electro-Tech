// CAPA NEGOCIO – reglas de productos e inventario
using System;
using System.Collections.Generic;
using Datos;
using Entidades;

namespace Negocio
{
    public class ProductoBLL
    {
        private readonly ProductoDAL dal = new ProductoDAL();

        // Id = 0 -> producto nuevo; Id > 0 -> se actualiza.
        public void Guardar(Producto p)
        {
            Permisos.Exigir(Modulo.Productos);

            p.Codigo = (p.Codigo ?? "").Trim();
            p.Nombre = (p.Nombre ?? "").Trim();
            p.Categoria = (p.Categoria ?? "").Trim();

            if (p.Codigo.Length == 0 || p.Codigo.Length > 20)
                throw new ArgumentException("El código es obligatorio y tiene como máximo 20 caracteres.");
            if (p.Nombre.Length == 0 || p.Nombre.Length > 100)
                throw new ArgumentException("El nombre es obligatorio y tiene como máximo 100 caracteres.");
            if (p.Categoria.Length > 50)
                throw new ArgumentException("La categoría tiene como máximo 50 caracteres.");
            if (p.Precio <= 0 || p.Precio > 99999999m)
                throw new ArgumentException("El precio debe ser mayor que 0.");
            if (p.Stock < 0)
                throw new ArgumentException("El stock no puede ser negativo.");

            if (p.Id == 0) dal.Insertar(p); else dal.Actualizar(p);
        }

        // Lo usan Almacén (inventario) y Ventas (elegir productos).
        public List<Producto> Buscar(string texto)
        {
            Permisos.Exigir(Modulo.Productos, Modulo.Ventas);
            return dal.Buscar(texto);
        }

        // Alerta de stock bajo (la usa el Timer de la pantalla principal).
        public int ContarStockBajo(int minimo)
        {
            Permisos.Exigir(Modulo.Productos);
            return dal.ContarStockBajo(minimo);
        }
    }
}
