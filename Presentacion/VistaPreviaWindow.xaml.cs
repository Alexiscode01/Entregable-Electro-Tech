// CAPA PRESENTACIÓN (WPF) – vista previa del documento y envío a la impresora
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace Presentacion
{
    public partial class VistaPreviaWindow : Window
    {
        private readonly Func<FlowDocument> constructor;
        private readonly string trabajo;

        public VistaPreviaWindow(Func<FlowDocument> constructor, string trabajo)
        {
            InitializeComponent();
            this.constructor = constructor;
            this.trabajo = trabajo;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                visor.Document = constructor();

                List<InfoImpresora> impresoras = ServicioImpresion.ListarImpresoras();
                cboImpresora.ItemsSource = impresoras;
                InfoImpresora elegida = impresoras.FirstOrDefault(i => i.Predeterminada);
                if (elegida != null) cboImpresora.SelectedItem = elegida;
                else if (impresoras.Count > 0) cboImpresora.SelectedIndex = 0;
                else lblCola.Text = "No se encontraron impresoras instaladas.";
            }
            catch (Exception ex)
            {
                MessageBox.Show("No se pudo preparar la vista previa: " + ex.Message, "Error");
            }
        }

        private void cboImpresora_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var info = cboImpresora.SelectedItem as InfoImpresora;
            lblCola.Text = info == null ? "" : "Estado: " + info.Estado + "  ·  Trabajos en cola: " + info.Trabajos;
        }

        private void btnImprimir_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var info = cboImpresora.SelectedItem as InfoImpresora;
                ServicioImpresion.Imprimir(constructor, trabajo, info == null ? null : info.Nombre);
            }
            catch (Exception ex)
            {
                MessageBox.Show("No se pudo imprimir: " + ex.Message, "Error");
            }
        }

        private void btnCerrar_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
