// CAPA PRESENTACIÓN (WPF) – GESTIÓN DEL SERVICIO DE IMPRESIÓN con System.Printing y PrintDialog.
// Permite listar las impresoras del equipo, ver cuántos trabajos tienen en cola y enviar un documento a imprimir.
using System;
using System.Collections.Generic;
using System.Printing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace Presentacion
{
    public class InfoImpresora
    {
        public string Nombre { get; set; }
        public int Trabajos { get; set; }            // trabajos pendientes en la cola
        public string Estado { get; set; }
        public bool Predeterminada { get; set; }
    }

    public static class ServicioImpresion
    {
        public static List<InfoImpresora> ListarImpresoras()
        {
            var lista = new List<InfoImpresora>();
            try
            {
                string predeterminada = null;
                try
                {
                    using (PrintQueue q = LocalPrintServer.GetDefaultPrintQueue())
                        predeterminada = q.Name;
                }
                catch { }

                using (var servidor = new LocalPrintServer())
                {
                    PrintQueueCollection colas = servidor.GetPrintQueues(
                        new[] { EnumeratedPrintQueueTypes.Local, EnumeratedPrintQueueTypes.Connections });
                    foreach (PrintQueue cola in colas)
                    {
                        try
                        {
                            lista.Add(new InfoImpresora
                            {
                                Nombre = cola.Name,
                                Trabajos = cola.NumberOfJobs,
                                Estado = cola.QueueStatus.ToString(),
                                Predeterminada = cola.Name == predeterminada
                            });
                        }
                        catch { }
                        finally { cola.Dispose(); }
                    }
                }
            }
            catch
            {
                // sin servicio de impresión disponible: se devuelve la lista vacía
            }
            return lista;
        }

        // Imprime en la impresora elegida (el usuario confirma en el cuadro de impresión de Windows).
        // El documento se pide al constructor para tener una copia nueva con el tamaño de página de la impresora.
        public static void Imprimir(Func<FlowDocument> constructor, string nombreTrabajo, string impresora)
        {
            var dialogo = new PrintDialog();
            if (!string.IsNullOrEmpty(impresora))
            {
                try
                {
                    using (var servidor = new LocalPrintServer())
                        dialogo.PrintQueue = servidor.GetPrintQueue(impresora);
                }
                catch { }
            }
            if (dialogo.ShowDialog() != true) return;

            FlowDocument doc = constructor();
            doc.PageHeight = dialogo.PrintableAreaHeight;
            doc.PageWidth = dialogo.PrintableAreaWidth;
            doc.ColumnGap = 0;
            doc.ColumnWidth = dialogo.PrintableAreaWidth;
            dialogo.PrintDocument(((IDocumentPaginatorSource)doc).DocumentPaginator, nombreTrabajo);
        }
    }
}
