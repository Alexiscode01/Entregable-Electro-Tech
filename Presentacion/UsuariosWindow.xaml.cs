// CAPA PRESENTACIÓN (WPF) – usuarios: registrar (con confirmación de contraseña), buscar (consulta segura),
// desbloquear cuentas y autorizar equipos (autenticación basada en hardware)
using System;
using System.Windows;
using System.Windows.Controls;
using Entidades;
using Negocio;

namespace Presentacion
{
    public partial class UsuariosWindow : Window
    {
        private readonly UsuarioBLL bll = new UsuarioBLL();
        private readonly bool primerAdministrador;

        // primerAdministrador = true: aún no hay usuarios; se crea el Administrador y se cierra.
        public UsuariosWindow(bool primerAdministrador)
        {
            InitializeComponent();
            this.primerAdministrador = primerAdministrador;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            cboRol.ItemsSource = UsuarioBLL.Roles;
            cboRol.SelectedIndex = 0;
            if (primerAdministrador)
            {
                Title = "Tech Solutions – Configuración inicial: primer administrador";
                lblTitulo.Text = "Crear el primer administrador";
                cboRol.IsEnabled = false;                // el primer usuario siempre es Administrador
                panelAdministracion.Visibility = Visibility.Collapsed;
                dgUsuarios.Visibility = Visibility.Collapsed;
            }
            else
            {
                lblEquipo.Text = "Equipo actual: " + bll.NombreEquipoActual();
            }
        }

        private void btnRegistrar_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                bll.Registrar(txtLogin.Text.Trim(), txtClave.Password, txtConfirmar.Password, cboRol.SelectedItem as string);
                MessageBox.Show("Usuario registrado. Se guardaron la sal y el hash, no la contraseña.");
                txtLogin.Clear(); txtClave.Clear(); txtConfirmar.Clear();
                if (primerAdministrador) Close();
            }
            catch (ArgumentException ex)
            {
                MessageBox.Show(ex.Message, "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            catch (Exception ex)
            {
                MessageBox.Show("No se pudo registrar: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // La búsqueda es parametrizada: si escriben  ' OR 1=1 --  se busca como texto y no devuelve nada.
        private void btnBuscar_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                dgUsuarios.ItemsSource = bll.Buscar(txtBuscar.Text);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error en la consulta: " + ex.Message, "Error");
            }
        }

        private void btnDesbloquear_Click(object sender, RoutedEventArgs e)
        {
            var u = dgUsuarios.SelectedItem as Usuario;
            if (u == null)
            {
                MessageBox.Show("Busque y seleccione un usuario de la tabla.", "Desbloquear");
                return;
            }
            try
            {
                bll.Desbloquear(u.Id);
                MessageBox.Show("Cuenta de \"" + u.Login + "\" desbloqueada.", "Desbloquear");
                btnBuscar_Click(sender, e);              // refresca la tabla
            }
            catch (Exception ex)
            {
                MessageBox.Show("No se pudo desbloquear: " + ex.Message, "Error");
            }
        }

        private void btnAutorizar_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult r = MessageBox.Show(
                "¿Autorizar este equipo (" + bll.NombreEquipoActual() + ") para el rol Administrador?",
                "Autenticación por hardware", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (r != MessageBoxResult.Yes) return;
            try
            {
                bll.AutorizarEquipoActual();
                MessageBox.Show("Equipo autorizado.", "Autenticación por hardware");
            }
            catch (ArgumentException ex)
            {
                MessageBox.Show(ex.Message, "Autenticación por hardware");
            }
            catch (Exception ex)
            {
                MessageBox.Show("No se pudo autorizar: " + ex.Message, "Error");
            }
        }

        // Nunca se muestran el hash ni la sal en pantalla.
        private void dgUsuarios_AutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            if (e.PropertyName == "ClaveHash" || e.PropertyName == "Sal")
            {
                e.Cancel = true;
                return;
            }
            if (e.PropertyName == "IntentosFallidos") e.Column.Header = "Intentos fallidos";
        }
    }
}
