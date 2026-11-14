// CAPA PRESENTACIÓN (WPF) – inicio de sesión con 3 intentos
using System;
using System.Windows;
using Entidades;
using Negocio;

namespace Presentacion
{
    public partial class LoginWindow : Window
    {
        private readonly UsuarioBLL bll = new UsuarioBLL();
        private const int MaxIntentos = 3;
        private int intentos = 0;      // intentos fallidos en esta pantalla

        public LoginWindow()
        {
            InitializeComponent();
        }

        private void btnIngresar_Click(object sender, RoutedEventArgs e)
        {
            lblMensaje.Text = "";
            btnIngresar.IsEnabled = false;
            try
            {
                Usuario u = bll.IniciarSesion(txtLogin.Text.Trim(), txtClave.Password);
                intentos = 0;
                txtClave.Clear();                       // no dejar la contraseña en pantalla

                var principal = new MainWindow(u);
                principal.Closed += (s, a) =>
                {
                    // Si el usuario eligió "Cerrar sesión" (o se cerró por inactividad), se muestra el login limpio.
                    if (principal.CerroSesion) NuevoLogin();
                };
                principal.Show();
                Hide();
            }
            catch (CuentaBloqueadaException ex)
            {
                lblMensaje.Text = ex.Message;
                txtClave.Clear();
            }
            catch (EquipoNoAutorizadoException ex)
            {
                lblMensaje.Text = ex.Message + " Pida a un administrador autorizado que registre este equipo.";
                txtClave.Clear();
            }
            catch (UnauthorizedAccessException ex)
            {
                intentos++;
                txtClave.Clear();
                if (intentos >= MaxIntentos)
                {
                    MessageBox.Show("Superó los " + MaxIntentos + " intentos permitidos.\n" +
                                    "La cuenta fue bloqueada. Contacte al administrador.",
                                    "Acceso bloqueado", MessageBoxButton.OK, MessageBoxImage.Warning);
                    Application.Current.Shutdown();     // se cierra la aplicación
                    return;
                }
                lblMensaje.Text = ex.Message + " Intento " + intentos + " de " + MaxIntentos + ".";
                txtClave.Focus();
            }
            catch (Exception ex)
            {
                lblMensaje.Text = "No se pudo conectar: " + ex.Message;
            }
            finally
            {
                btnIngresar.IsEnabled = true;
            }
        }

        // Deja el login como nuevo después de cerrar sesión.
        private void NuevoLogin()
        {
            intentos = 0;
            txtLogin.Clear();
            txtClave.Clear();
            lblMensaje.Text = "";
            Show();
            txtLogin.Focus();
        }

        // El login solo se cierra de verdad cuando el usuario pulsa la X: ahí termina la aplicación.
        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            Application.Current.Shutdown();
        }
    }
}
