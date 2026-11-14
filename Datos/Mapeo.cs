// CAPA DATOS – mapeo para LINQ to SQL (consultas LINQ que se traducen a SQL parametrizado).
// Referencias: System.Data.Linq
using System.Data;
using System.Data.Linq;
using System.Data.Linq.Mapping;

namespace Datos
{
    [Table(Name = "dbo.Cliente")]
    public class ClienteMap
    {
        [Column(Name = "IdCliente", IsPrimaryKey = true, IsDbGenerated = true)] public int Id { get; set; }
        [Column] public string Documento { get; set; }
        [Column] public string Nombre { get; set; }
        [Column] public string Telefono { get; set; }
        [Column] public string Email { get; set; }
    }

    [Table(Name = "dbo.Producto")]
    public class ProductoMap
    {
        [Column(Name = "IdProducto", IsPrimaryKey = true, IsDbGenerated = true)] public int Id { get; set; }
        [Column] public string Codigo { get; set; }
        [Column] public string Nombre { get; set; }
        [Column] public string Categoria { get; set; }
        [Column] public decimal Precio { get; set; }
        [Column] public int Stock { get; set; }
    }

    // Contexto de solo lectura: sin seguimiento de objetos, que es más rápido para consultas y reportes.
    public class TechDataContext : DataContext
    {
        public TechDataContext(IDbConnection conexion) : base(conexion)
        {
            ObjectTrackingEnabled = false;
        }

        public Table<ClienteMap> Clientes { get { return GetTable<ClienteMap>(); } }
        public Table<ProductoMap> Productos { get { return GetTable<ProductoMap>(); } }
    }
}
