// CAPA PRESENTACIÓN (WPF) – productos e inventario (stock)
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using Entidades;
using Negocio;

namespace Presentacion
{
    public partial class ProductosWindow : Window
    {
        private readonly ProductoBLL bll = new ProductoBLL();
        private int idSeleccionado = 0;      // 0 = producto nuevo

        public ProductosWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            txtPrecio.Text = "0.00";
            txtStock.Text = "0";
            Listar();
        }

        private void Listar()
        {
            try
            {
                dgProductos.ItemsSource = bll.Buscar(txtBuscar.Text.Trim());
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error");
            }
        }

        private void btnBuscar_Click(object sender, RoutedEventArgs e)
        {
            Listar();
        }

        private void btnGuardar_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                decimal precio;
                int stock;
                if (!decimal.TryParse(txtPrecio.Text.Trim(), NumberStyles.Number, CultureInfo.CurrentCulture, out precio))
                    throw new ArgumentException("El precio no es un número válido.");
                if (!int.TryParse(txtStock.Text.Trim(), NumberStyles.Integer, CultureInfo.CurrentCulture, out stock))
                    throw new ArgumentException("El stock debe ser un número entero.");

                var p = new Producto
                {
                    Id = idSeleccionado,
                    Codigo = txtCodigo.Text,
                    Nombre = txtNombre.Text,
                    Categoria = txtCategoria.Text,
                    Precio = precio,
                    Stock = stock
                };
                bll.Guardar(p);
                MessageBox.Show(idSeleccionado == 0 ? "Producto registrado." : "Producto actualizado.");
                Limpiar();
                Listar();
            }
            catch (ArgumentException ex)          // regla de negocio no cumplida
            {
                MessageBox.Show(ex.Message, "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            catch (Exception ex)
            {
                MessageBox.Show("No se pudo guardar: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnNuevo_Click(object sender, RoutedEventArgs e)
        {
            Limpiar();
        }

        private void Limpiar()
        {
            idSeleccionado = 0;
            txtCodigo.Clear(); txtNombre.Clear(); txtCategoria.Clear();
            txtPrecio.Text = "0.00"; txtStock.Text = "0";
            lblModo.Text = "Nuevo producto";
            dgProductos.SelectedItem = null;
            txtCodigo.Focus();
        }

        private void dgProductos_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var p = dgProductos.SelectedItem as Producto;
            if (p == null) return;
            idSeleccionado = p.Id;
            txtCodigo.Text = p.Codigo;
            txtNombre.Text = p.Nombre;
            txtCategoria.Text = p.Categoria;
            txtPrecio.Text = p.Precio.ToString("0.00", CultureInfo.CurrentCulture);
            txtStock.Text = p.Stock.ToString(CultureInfo.CurrentCulture);
            lblModo.Text = "Editando producto";
        }
    }
}
