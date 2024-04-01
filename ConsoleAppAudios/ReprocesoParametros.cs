
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Text;

namespace ConsoleAppAudios
{

    public class ReprocesoParametros
    {
        public int intNumHilo;
        public string strArchivoLog = string.Empty;
        public List<ModelValor> ListaValores;
    }

    public class ModelValor
    {
        public string Valor = string.Empty;
        public string NombreArchivo = string.Empty;
        public string NombreCarpeta = string.Empty;
    }

    public class GeneraReproceso
    {

        public void Reproceso(object argParametros)
        {
            int intPosicion = 0;
            string STR_SEPARADOR = "|";
            string CARPETA_DESCARGA = ConfigurationManager.AppSettings["CARPETA_DESTINO"].ToString();
            int AudioVel = int.Parse(ConfigurationManager.AppSettings["AUDIO_VEL"].ToString());
            string rutaCarpeta = Path.Combine(Environment.CurrentDirectory, CARPETA_DESCARGA);

            if (!Directory.Exists(rutaCarpeta))
            {
                // La carpeta no existe, así que la creamos.
                Directory.CreateDirectory(rutaCarpeta);
            }

            //Recupera parametros
            ReprocesoParametros objParametros = (ReprocesoParametros)argParametros;

            //Si no viene ningun prestadorId para procesar, retorna
            if (objParametros.ListaValores == null || objParametros.ListaValores.Count <= 0) return;

            //Crea archivo de log
            StreamWriter fileArchivoLog = new StreamWriter(objParametros.strArchivoLog, false, Encoding.UTF8);

            try
            {
                foreach (ModelValor item in objParametros.ListaValores)
                {
                    string RutaCarpeta = string.Empty;
                    //Si tiene subcarpeta la creamos
                    if (!String.IsNullOrEmpty(item.NombreCarpeta))
                    {
                        RutaCarpeta = rutaCarpeta + " \\ " + item.NombreCarpeta;

                        if (!Directory.Exists(RutaCarpeta))
                            Directory.CreateDirectory(RutaCarpeta);
                    }
                    else
                        RutaCarpeta = rutaCarpeta;

                    //Creamos archivo de audio.
                    libSintetizador.SynthetizerSave(item.Valor, item.NombreArchivo, RutaCarpeta, AudioVel, 100, "ANA");

                    //Procesa registro
                    DateTime horaInicio = DateTime.Now;
                    double numSegundos = (DateTime.Now - horaInicio).TotalSeconds;

                    //Guarda log
                    intPosicion++;
                    fileArchivoLog.WriteLine("{1:00}{0}{2:00000}{0}{3:dd/MM/yyyy} {3:HH:mm:ss}{0}{4:000}{0}{5}",
                                             STR_SEPARADOR,
                                             objParametros.intNumHilo,
                                             intPosicion,
                                             horaInicio,
                                             numSegundos,
                                             item.NombreArchivo);

                }
            }
            catch (Exception ex)
            {
                string strMensaje = ex.Message.Replace("\r\n", ". ").Replace("\n", ". ").Replace("\r", ". ");
                if (ex.InnerException != null)
                    strMensaje += ex.InnerException.Message.Replace("\r\n", ". ").Replace("\n", ". ").Replace("\r", ". ");
                fileArchivoLog.WriteLine("{1:00}{0}{2}", STR_SEPARADOR, objParametros.intNumHilo, strMensaje);
            }

            //Cierra archivo
            fileArchivoLog.Flush();
            fileArchivoLog.Close();

        }

    }
}