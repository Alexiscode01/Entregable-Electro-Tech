// Proyecto: Entidades
namespace Entidades
{
    public class DetalleVenta
    {
        public int IdProducto { get; set; }
        public string Codigo { get; set; }
        public string Producto { get; set; }          // nombre del producto (para mostrar)
        public int Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
        public decimal Subtotal { get { return Cantidad * PrecioUnitario; } }
    }
}
