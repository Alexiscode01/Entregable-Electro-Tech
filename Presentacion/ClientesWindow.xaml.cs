// CAPA PRESENTACIÓN (WPF) – clientes: registrar, editar y buscar
using System;
using System.Windows;
using System.Windows.Controls;
using Entidades;
using Negocio;

namespace Presentacion
{
    public partial class ClientesWindow : Window
    {
        private readonly ClienteBLL bll = new ClienteBLL();
        private int idSeleccionado = 0;      // 0 = cliente nuevo

        public ClientesWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Listar();
        }

        private void Listar()
        {
            try
            {
                dgClientes.ItemsSource = bll.Buscar(txtBuscar.Text.Trim());
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
                var c = new Cliente
                {
                    Id = idSeleccionado,
                    Documento = txtDocumento.Text,
                    Nombre = txtNombre.Text,
                    Telefono = txtTelefono.Text,
                    Email = txtEmail.Text
                };
                bll.Guardar(c);
                MessageBox.Show(idSeleccionado == 0 ? "Cliente registrado." : "Cliente actualizado.");
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
            txtDocumento.Clear(); txtNombre.Clear(); txtTelefono.Clear(); txtEmail.Clear();
            lblModo.Text = "Nuevo cliente";
            dgClientes.SelectedItem = null;
            txtDocumento.Focus();
        }

        // Al elegir una fila, sus datos pasan a los campos para editarlos.
        private void dgClientes_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var c = dgClientes.SelectedItem as Cliente;
            if (c == null) return;
            idSeleccionado = c.Id;
            txtDocumento.Text = c.Documento;
            txtNombre.Text = c.Nombre;
            txtTelefono.Text = c.Telefono;
            txtEmail.Text = c.Email;
            lblModo.Text = "Editando cliente";
        }
    }
}
