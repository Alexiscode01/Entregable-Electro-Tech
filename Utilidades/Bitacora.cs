// BIBLIOTECA UTILIDADES – bitácora de eventos de seguridad y operaciones (archivo de texto).
// Ruta: %LocalAppData%\TechSolutions\bitacora.log   (nunca lanza excepciones: registrar no debe romper la aplicación)
using System;
using System.IO;

namespace Utilidades
{
    public static class Bitacora
    {
        private static readonly object candado = new object();

        public static void Registrar(string evento)
        {
            try
            {
                string carpeta = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "TechSolutions");
                Directory.CreateDirectory(carpeta);

                // Se quitan saltos de línea para que nadie pueda falsificar líneas de la bitácora.
                string limpio = (evento ?? "").Replace("\r", " ").Replace("\n", " ");
                if (limpio.Length > 300) limpio = limpio.Substring(0, 300);

                lock (candado)   // varios hilos pueden escribir a la vez
                {
                    File.AppendAllText(Path.Combine(carpeta, "bitacora.log"),
                        DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " | " + limpio + Environment.NewLine);
                }
            }
            catch
            {
                // si no se puede escribir, se ignora
            }
        }
    }
}
