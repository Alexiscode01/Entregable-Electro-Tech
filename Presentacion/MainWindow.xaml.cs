// CAPA PRESENTACIÓN (WPF) – menú principal: menús según el rol, cerrar sesión, Timer de inactividad
// y alerta de stock bajo en un hilo (Thread) aparte.
using System;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using Entidades;
using Negocio;

namespace Presentacion
{
    public partial class MainWindow : Window
    {
        private readonly int minutosInactividad = Parametros.MinutosInactividad;
        private readonly ProductoBLL productos = new ProductoBLL();

        // Dos Timer: uno por segundo (reloj + control de inactividad) y otro cada minuto (alerta de stock).
        private readonly DispatcherTimer timerSegundo = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
        private readonly DispatcherTimer timerAlertas = new DispatcherTimer { Interval = TimeSpan.FromMinutes(1) };
        private DateTime ultimaActividad = DateTime.Now;

        // true cuando el usuario cierra la sesión (en vez de cerrar toda la aplicación)
        public bool CerroSesion { get; private set; }

        public MainWindow(Usuario u)
        {
            InitializeComponent();

            lblSesion.Text = "Usuario: " + u.Login + " (" + u.Rol + ")";
            lblBienvenida.Text = "Bienvenido, " + u.Login + ".\n" +
                                 "La sesión se cierra sola tras " + minutosInactividad + " minutos sin actividad.";

            // Niveles de acceso: cada rol ve solo lo que necesita (ver la tabla en Negocio/Permisos.cs)
            mnuClientes.Visibility = Mostrar(Permisos.Puede(u.Rol, Modulo.Clientes));
            mnuProductos.Visibility = Mostrar(Permisos.Puede(u.Rol, Modulo.Productos));
            mnuVentas.Visibility = Mostrar(Permisos.Puede(u.Rol, Modulo.Ventas));
            mnuReportes.Visibility = Mostrar(Permisos.Puede(u.Rol, Modulo.Reportes) || Permisos.Puede(u.Rol, Modulo.Productos));
            mnuUsuarios.Visibility = Mostrar(Permisos.Puede(u.Rol, Modulo.Usuarios));

            // Cada movimiento de mouse o tecla en la aplicación reinicia el contador de inactividad.
            InputManager.Current.PreProcessInput += InputManager_PreProcessInput;

            timerSegundo.Tick += timerSegundo_Tick;
            timerSegundo.Start();
            lblReloj.Text = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");

            if (Permisos.Puede(u.Rol, Modulo.Productos))
            {
                timerAlertas.Tick += timerAlertas_Tick;
                timerAlertas.Start();
                ConsultarAlertas();
            }
        }

        private static Visibility Mostrar(bool visible)
        {
            return visible ? Visibility.Visible : Visibility.Collapsed;
        }

        private void mnuClientes_Click(object sender, RoutedEventArgs e) { new ClientesWindow { Owner = this }.ShowDialog(); }
        private void mnuProductos_Click(object sender, RoutedEventArgs e) { new ProductosWindow { Owner = this }.ShowDialog(); }
        private void mnuVentas_Click(object sender, RoutedEventArgs e) { new VentasWindow { Owner = this }.ShowDialog(); }
        private void mnuReportes_Click(object sender, RoutedEventArgs e) { new ReportesWindow { Owner = this }.ShowDialog(); }
        private void mnuUsuarios_Click(object sender, RoutedEventArgs e) { new UsuariosWindow(false) { Owner = this }.ShowDialog(); }

        private void mnuCerrarSesion_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult r = MessageBox.Show("¿Desea cerrar la sesión?", "Cerrar sesión",
                                                 MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (r != MessageBoxResult.Yes) return;
            CerroSesion = true;   // el LoginWindow oculto volverá a mostrarse
            Close();
        }

        // ---------------- Timer: inactividad ----------------
        private void InputManager_PreProcessInput(object sender, PreProcessInputEventArgs e)
        {
            InputEventArgs entrada = e.StagingItem.Input;
            if (entrada is MouseEventArgs || entrada is KeyboardEventArgs)
                ultimaActividad = DateTime.Now;
        }

        private void timerSegundo_Tick(object sender, EventArgs e)
        {
            lblReloj.Text = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
            if ((DateTime.Now - ultimaActividad).TotalMinutes >= minutosInactividad)
                CerrarPorInactividad();
        }

        private void CerrarPorInactividad()
        {
            timerSegundo.Stop();
            timerAlertas.Stop();

            // Cierra cualquier ventana que estuviera abierta (clientes, ventas, etc.) para que nada quede a la vista.
            foreach (Window w in Application.Current.Windows.Cast<Window>().ToList())
                if (w != this && !(w is LoginWindow)) w.Close();

            MessageBox.Show("La sesión se cerró por inactividad.", "Sesión finalizada",
                            MessageBoxButton.OK, MessageBoxImage.Information);
            CerroSesion = true;
            Close();
        }

        // ---------------- Timer + Thread: alerta de stock bajo ----------------
        private void timerAlertas_Tick(object sender, EventArgs e)
        {
            ConsultarAlertas();
        }

        // La consulta se hace en un hilo aparte para que la pantalla no se congele; el resultado vuelve al hilo de la interfaz.
        private void ConsultarAlertas()
        {
            int umbral = Parametros.UmbralStockBajo;
            var hilo = new Thread(() =>
            {
                try
                {
                    int cantidad = productos.ContarStockBajo(umbral);
                    Dispatcher.Invoke(new Action(() =>
                    {
                        lblAlerta.Text = cantidad == 0
                            ? ""
                            : "Alerta: " + cantidad + " producto(s) con stock de " + umbral + " o menos";
                    }));
                }
                catch
                {
                    // si falla la consulta se mantiene el último aviso
                }
            });
            hilo.Name = "AlertaStock";
            hilo.IsBackground = true;      // no impide cerrar la aplicación
            hilo.Start();
        }

        protected override void OnClosed(EventArgs e)
        {
            InputManager.Current.PreProcessInput -= InputManager_PreProcessInput;
            timerSegundo.Stop();
            timerAlertas.Stop();
            Sesion.Cerrar();                        // ya no hay usuario con sesión abierta
            base.OnClosed(e);
            // Si cerró con la X termina la aplicación; si cerró sesión, se vuelve al login.
            if (!CerroSesion) Application.Current.Shutdown();
        }
    }
}
