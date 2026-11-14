// CAPA PRESENTACIÓN (WPF) – arranque de la aplicación
using System;
using System.Windows;
using Negocio;

namespace Presentacion
{
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            try
            {
                // Si todavía no hay usuarios, primero se crea el Administrador.
                var bll = new UsuarioBLL();
                if (!bll.HayUsuarios())
                {
                    new UsuariosWindow(true).ShowDialog();
                    if (!bll.HayUsuarios())          // cerró sin registrar
                    {
                        Shutdown();
                        return;
                    }
                }
                new LoginWindow().Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show("No se pudo conectar con la base de datos.\n\n" + ex.Message +
                                "\n\nRevise el Data Source en TechSolutions.exe.config y que haya ejecutado el script SQL.",
                                "Tech Solutions", MessageBoxButton.OK, MessageBoxImage.Error);
                Shutdown();
            }
        }
    }
}
