// CAPA PRESENTACIÓN (WPF) – reportes con parámetros (procedimientos almacenados), generados en un HILO aparte,
// con vista previa e impresión, exportación a CSV y anulación de ventas (solo Administrador).
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using Entidades;
using Microsoft.Win32;
using Negocio;
using Utilidades;

namespace Presentacion
{
    public partial class ReportesWindow : Window
    {
        private readonly ReporteBLL reportes = new ReporteBLL();
        private readonly VentaBLL ventasBll = new VentaBLL();
        private readonly UsuarioBLL usuarios = new UsuarioBLL();

        private DataTable tablaVentas, tablaTop, tablaStock;
        private string subtituloVentas = "";
        private string pieVentas = "";
        private string subtituloStock = "";

        public ReportesWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            string rol = Sesion.Actual.Rol;
            bool verVentas = Permisos.Puede(rol, Modulo.Reportes);
            bool verStock = Permisos.Puede(rol, Modulo.Productos);

            dpDesde.SelectedDate = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
            dpHasta.SelectedDate = DateTime.Today;
            txtMinimo.Text = Parametros.UmbralStockBajo.ToString(CultureInfo.InvariantCulture);

            // Cada rol ve solo los reportes que le corresponden.
            panelParametros.Visibility = verVentas ? Visibility.Visible : Visibility.Collapsed;
            tabVentas.Visibility = verVentas ? Visibility.Visible : Visibility.Collapsed;
            tabTop.Visibility = verVentas ? Visibility.Visible : Visibility.Collapsed;
            tabStock.Visibility = verStock ? Visibility.Visible : Visibility.Collapsed;
            btnAnular.Visibility = Permisos.Puede(rol, Modulo.Anulaciones) ? Visibility.Visible : Visibility.Collapsed;

            try
            {
                if (verVentas)
                {
                    if (rol == Permisos.Vendedor)
                    {
                        cboVendedor.ItemsSource = new List<Usuario> { Sesion.Actual };   // el vendedor solo ve lo suyo
                        cboVendedor.SelectedIndex = 0;
                        cboVendedor.IsEnabled = false;
                    }
                    else
                    {
                        var lista = new List<Usuario> { new Usuario { Id = 0, Login = "(Todos)" } };
                        lista.AddRange(usuarios.ListarVendedores());
                        cboVendedor.ItemsSource = lista;
                        cboVendedor.SelectedIndex = 0;
                    }
                    tabReportes.SelectedItem = tabVentas;
                    Generar();
                }
                else
                {
                    tabReportes.SelectedItem = tabStock;
                    ConsultarStock();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("No se pudo preparar el reporte: " + ex.Message, "Error");
            }
        }

        private void btnGenerar_Click(object sender, RoutedEventArgs e)
        {
            Generar();
        }

        private void btnStockBajo_Click(object sender, RoutedEventArgs e)
        {
            ConsultarStock();
        }

        private void Ocupado(bool ocupado)
        {
            pbCarga.Visibility = ocupado ? Visibility.Visible : Visibility.Hidden;
            btnGenerar.IsEnabled = !ocupado;
            btnStockBajo.IsEnabled = !ocupado;
        }

        // ---------------- Reporte de ventas (en un Thread) ----------------
        private void Generar()
        {
            if (!dpDesde.SelectedDate.HasValue || !dpHasta.SelectedDate.HasValue)
            {
                MessageBox.Show("Elija las dos fechas.", "Validación");
                return;
            }
            // Se leen los parámetros ANTES de crear el hilo: el hilo no debe tocar los controles.
            DateTime desde = dpDesde.SelectedDate.Value;
            DateTime hasta = dpHasta.SelectedDate.Value;
            int? vendedor = null;
            string nombreVendedor = "Todos";
            var elegido = cboVendedor.SelectedItem as Usuario;
            if (elegido != null)
            {
                nombreVendedor = elegido.Login;
                if (elegido.Id > 0) vendedor = elegido.Id;
            }

            Ocupado(true);
            var hilo = new Thread(() =>
            {
                try
                {
                    DataTable ventas = reportes.VentasPorFechas(desde, hasta, vendedor);        // procedimiento almacenado
                    DataTable top = reportes.ProductosMasVendidos(desde, hasta, vendedor);      // procedimiento almacenado
                    ResumenVentas resumen = reportes.Resumir(ventas);                           // LINQ to DataSet

                    Dispatcher.Invoke(new Action(() =>
                    {
                        tablaVentas = ventas;
                        tablaTop = top;
                        dgVentas.ItemsSource = ventas.DefaultView;
                        dgTop.ItemsSource = top.DefaultView;
                        dgDetalle.ItemsSource = null;

                        string porVendedor = string.Join("  |  ", resumen.PorVendedor.ConvertAll(
                            p => p.Key + ": S/ " + p.Value.ToString("N2")));
                        pieVentas = "Ventas activas: " + resumen.CantidadVentas +
                                    "   Total: S/ " + resumen.Total.ToString("N2");
                        lblResumen.Text = pieVentas +
                                          "   Promedio: S/ " + resumen.Promedio.ToString("N2") +
                                          "   Anuladas: " + resumen.CantidadAnuladas +
                                          (porVendedor.Length > 0 ? "\nPor vendedor → " + porVendedor : "");
                        subtituloVentas = "Del " + desde.ToString("dd/MM/yyyy") + " al " + hasta.ToString("dd/MM/yyyy") +
                                          "   |   Vendedor: " + nombreVendedor;
                        Ocupado(false);
                    }));
                }
                catch (Exception ex)
                {
                    string mensaje = ex.Message;
                    Dispatcher.Invoke(new Action(() =>
                    {
                        Ocupado(false);
                        MessageBox.Show(mensaje, "No se pudo generar el reporte");
                    }));
                }
            });
            hilo.Name = "ReporteVentas";
            hilo.IsBackground = true;
            hilo.Start();
        }

        // ---------------- Reporte de stock bajo (en un Thread) ----------------
        private void ConsultarStock()
        {
            int minimo;
            if (!int.TryParse(txtMinimo.Text.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out minimo) || minimo < 0)
            {
                MessageBox.Show("El stock mínimo debe ser un número entero (0 o más).", "Validación");
                return;
            }

            Ocupado(true);
            var hilo = new Thread(() =>
            {
                try
                {
                    DataTable stock = reportes.StockBajo(minimo);                               // procedimiento almacenado
                    Dispatcher.Invoke(new Action(() =>
                    {
                        tablaStock = stock;
                        dgStock.ItemsSource = stock.DefaultView;
                        subtituloStock = "Productos con stock de " + minimo + " o menos";
                        Ocupado(false);
                    }));
                }
                catch (Exception ex)
                {
                    string mensaje = ex.Message;
                    Dispatcher.Invoke(new Action(() =>
                    {
                        Ocupado(false);
                        MessageBox.Show(mensaje, "No se pudo consultar el stock");
                    }));
                }
            });
            hilo.Name = "ReporteStock";
            hilo.IsBackground = true;
            hilo.Start();
        }

        // Al elegir una venta se muestran sus productos.
        private void dgVentas_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var fila = dgVentas.SelectedItem as DataRowView;
            if (fila == null) return;
            try
            {
                dgDetalle.ItemsSource = reportes.DetalleVenta((int)fila["IdVenta"]).DefaultView;
            }
            catch (Exception ex)
            {
                MessageBox.Show("No se pudo cargar el detalle: " + ex.Message, "Error");
            }
        }

        // Anula la venta con el procedimiento sp_AnularVenta (transacción en SQL Server).
        private void btnAnular_Click(object sender, RoutedEventArgs e)
        {
            var fila = dgVentas.SelectedItem as DataRowView;
            if (fila == null)
            {
                MessageBox.Show("Seleccione una venta de la tabla.", "Anular venta");
                return;
            }
            int id = (int)fila["IdVenta"];
            MessageBoxResult r = MessageBox.Show(
                "¿Anular la venta N° " + id + "?\nSe repondrá el stock de sus productos.",
                "Anular venta", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (r != MessageBoxResult.Yes) return;
            try
            {
                ventasBll.Anular(id);
                MessageBox.Show("Venta anulada y stock repuesto.", "Anular venta");
                Generar();
            }
            catch (InvalidOperationException ex)
            {
                MessageBox.Show(ex.Message, "Anular venta", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            catch (Exception ex)
            {
                MessageBox.Show("No se pudo anular: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // ---------------- Imprimir y exportar el reporte de la pestaña activa ----------------
        private bool ObtenerActual(out DataTable tabla, out string titulo, out string subtitulo, out string pie)
        {
            tabla = null; titulo = ""; subtitulo = ""; pie = "";
            if (tabReportes.SelectedItem == tabVentas)
            {
                tabla = tablaVentas; titulo = "Reporte de ventas"; subtitulo = subtituloVentas; pie = pieVentas;
            }
            else if (tabReportes.SelectedItem == tabTop)
            {
                tabla = tablaTop; titulo = "Productos más vendidos"; subtitulo = subtituloVentas;
            }
            else if (tabReportes.SelectedItem == tabStock)
            {
                tabla = tablaStock; titulo = "Reporte de stock bajo"; subtitulo = subtituloStock;
            }
            return tabla != null && tabla.Rows.Count > 0;
        }

        private void btnImprimir_Click(object sender, RoutedEventArgs e)
        {
            DataTable tabla; string titulo, subtitulo, pie;
            if (!ObtenerActual(out tabla, out titulo, out subtitulo, out pie))
            {
                MessageBox.Show("No hay datos para imprimir. Genere primero el reporte.", "Imprimir");
                return;
            }
            DataTable copia = tabla;
            string t = titulo, s = subtitulo, p = pie;
            var vista = new VistaPreviaWindow(() => ConstructorDocumentos.DesdeTabla(t, s, copia, p), t) { Owner = this };
            vista.ShowDialog();
        }

        private void btnExportar_Click(object sender, RoutedEventArgs e)
        {
            DataTable tabla; string titulo, subtitulo, pie;
            if (!ObtenerActual(out tabla, out titulo, out subtitulo, out pie))
            {
                MessageBox.Show("No hay datos para exportar. Genere primero el reporte.", "Exportar");
                return;
            }
            var dialogo = new SaveFileDialog
            {
                Filter = "Archivo CSV (*.csv)|*.csv",
                FileName = titulo.Replace(' ', '_') + "_" + DateTime.Today.ToString("yyyyMMdd") + ".csv"
            };
            if (dialogo.ShowDialog() != true) return;
            try
            {
                CsvUtil.Exportar(tabla, dialogo.FileName);
                MessageBox.Show("Reporte exportado.", "Exportar");
            }
            catch (Exception ex)
            {
                MessageBox.Show("No se pudo exportar: " + ex.Message, "Error");
            }
        }

        private void Columna_AutoGenerating(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            Ayudas.ConfigurarColumna(sender, e);
        }

        private void btnCerrar_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
