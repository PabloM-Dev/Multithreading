
using System;
using System.Configuration;
using System.IO;

namespace ConsoleAppAudios
{
    public class libLog
    {
        public static string ConstruyeNombreArchivoLog()
        {

            string CARPETA_DESCARGA = ConfigurationManager.AppSettings["CARPETA_DESTINO"].ToString();
            string ruta = Path.Combine(Environment.CurrentDirectory, CARPETA_DESCARGA);

            if (!Directory.Exists(ruta))
            {
                // La carpeta no existe, así que la creamos.
                Directory.CreateDirectory(ruta);
            }

            string NombreArchivo = String.Format("_{0:yyyyMMdd} {0:HHmmss} Reproceso", DateTime.Now);
            string RutaLog = Path.Combine(@ruta, System.IO.Path.GetFileName(NombreArchivo));

            return (RutaLog);
        }
    }
}
