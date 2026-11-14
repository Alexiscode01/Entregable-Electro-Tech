// CAPA PRESENTACIÓN (WPF) – registrar una venta (cliente + varios productos) e imprimir el comprobante
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using Entidades;
using Negocio;

namespace Presentacion
{
    public partial class VentasWindow : Window
    {
        private readonly ClienteBLL clientes = new ClienteBLL();
        private readonly ProductoBLL productos = new ProductoBLL();
        private readonly VentaBLL ventas = new VentaBLL();
        private readonly ObservableCollection<DetalleVenta> lineas = new ObservableCollection<DetalleVenta>();

        // Elemento del combo de productos: texto que se ve + el producto real.
        private class ItemProducto
        {
            public string Texto { get; set; }
            public Producto Producto { get; set; }
        }

        public VentasWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                cboCliente.ItemsSource = clientes.Buscar("");
                dgLineas.ItemsSource = lineas;
                CargarProductos();
                ActualizarTotal();
            }
            catch (Exception ex)
            {
                MessageBox.Show("No se pudo cargar: " + ex.Message, "Error");
            }
        }

        // Solo se ofrecen productos con stock; se vuelve a llamar después de cada venta.
        private void CargarProductos()
        {
            List<ItemProducto> items = productos.Buscar("")
                .Where(p => p.Stock > 0)
                .Select(p => new ItemProducto
                {
                    Producto = p,
                    Texto = p.Codigo + " – " + p.Nombre + "   (S/ " + p.Precio.ToString("N2") + ", stock " + p.Stock + ")"
                })
                .ToList();
            cboProducto.ItemsSource = items;
        }

        private void btnAgregar_Click(object sender, RoutedEventArgs e)
        {
            var item = cboProducto.SelectedItem as ItemProducto;
            if (item == null)
            {
                MessageBox.Show("Seleccione un producto.", "Validación");
                return;
            }
            int cantidad;
            if (!int.TryParse(txtCantidad.Text.Trim(), out cantidad) || cantidad <= 0)
            {
                MessageBox.Show("La cantidad debe ser un número entero mayor que 0.", "Validación");
                return;
            }

            DetalleVenta existente = lineas.FirstOrDefault(l => l.IdProducto == item.Producto.Id);
            int yaEnVenta = existente == null ? 0 : existente.Cantidad;

            // Aviso rápido en pantalla; la validación definitiva la hace la base de datos dentro de la transacción.
            if (yaEnVenta + cantidad > item.Producto.Stock)
            {
                MessageBox.Show("Stock insuficiente. Disponible: " + item.Producto.Stock + ".", "Validación");
                return;
            }

            if (existente != null)
            {
                existente.Cantidad += cantidad;
                dgLineas.Items.Refresh();
            }
            else
            {
                lineas.Add(new DetalleVenta
                {
                    IdProducto = item.Producto.Id,
                    Codigo = item.Producto.Codigo,
                    Producto = item.Producto.Nombre,
                    Cantidad = cantidad,
                    PrecioUnitario = item.Producto.Precio
                });
            }
            txtCantidad.Text = "1";
            ActualizarTotal();
        }

        private void btnQuitar_Click(object sender, RoutedEventArgs e)
        {
            var d = dgLineas.SelectedItem as DetalleVenta;
            if (d == null) return;
            lineas.Remove(d);
            ActualizarTotal();
        }

        private void ActualizarTotal()
        {
            decimal total = lineas.Sum(l => l.Subtotal);
            lblTotal.Text = "Total: S/ " + total.ToString("N2");
        }

        private void btnRegistrar_Click(object sender, RoutedEventArgs e)
        {
            if (cboCliente.SelectedValue == null)
            {
                MessageBox.Show("Seleccione un cliente.", "Validación");
                return;
            }
            try
            {
                var snapshot = lineas.ToList();                       // copia para el comprobante
                string nombreCliente = ((Cliente)cboCliente.SelectedItem).Nombre;

                Venta v = ventas.Registrar((int)cboCliente.SelectedValue, snapshot);   // transacción COMMIT / ROLLBACK

                lineas.Clear();
                ActualizarTotal();
                CargarProductos();                                    // el stock ya cambió

                MessageBoxResult r = MessageBox.Show(
                    "Venta N° " + v.Id + " registrada.\nTotal: S/ " + v.Total.ToString("N2") +
                    "\n\n¿Desea imprimir el comprobante?",
                    "Venta registrada", MessageBoxButton.YesNo, MessageBoxImage.Information);
                if (r == MessageBoxResult.Yes) MostrarComprobante(v, nombreCliente, snapshot);
            }
            catch (ArgumentException ex)            // regla de negocio no cumplida
            {
                MessageBox.Show(ex.Message, "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            catch (InvalidOperationException ex)    // por ejemplo, stock insuficiente: la transacción hizo ROLLBACK
            {
                MessageBox.Show(ex.Message + "\nLa venta NO se registró y el stock no cambió.", "Venta cancelada",
                                MessageBoxButton.OK, MessageBoxImage.Warning);
                CargarProductos();
            }
            catch (Exception ex)
            {
                MessageBox.Show("No se pudo registrar la venta: " + ex.Message, "Error",
                                MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Los precios vienen de la base de datos (Venta.Detalles); los nombres, de lo que se eligió en pantalla.
        private void MostrarComprobante(Venta v, string cliente, List<DetalleVenta> snapshot)
        {
            List<DetalleVenta> detalle = (from d in v.Detalles
                                          join l in snapshot on d.IdProducto equals l.IdProducto
                                          select new DetalleVenta
                                          {
                                              IdProducto = d.IdProducto,
                                              Codigo = l.Codigo,
                                              Producto = l.Producto,
                                              Cantidad = d.Cantidad,
                                              PrecioUnitario = d.PrecioUnitario
                                          }).ToList();

            var vista = new VistaPreviaWindow(() => ConstructorDocumentos.Comprobante(v, cliente, detalle),
                                              "Comprobante de venta " + v.Id) { Owner = this };
            vista.ShowDialog();
        }
    }
}
