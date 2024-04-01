using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace ConsoleAppAudios
{
    class Inicio
    {
        static void Main(string[] args)
        {

            //List<ModelValor> ListValor = Crear_Numeros(1, 1000);
            //List<ModelValor> ListValor = Crear_NumeroPesos(LlenaNumeroPesos());
            List<ModelValor> ListValor = Crear_Texto(LeerArchivo());
            //List<ModelValor> ListValor = Crear_Fecha();


            #region Parametros de ejecución

            //Obtiene cantidad de nucleos del equipo y calculo los threads a crear
            int numNucleos = Environment.ProcessorCount;

            int pausaHilo = int.Parse(ConfigurationManager.AppSettings["MULTIPROCESO_PAUSA"].ToString());

            //Calcula cantidad de threads que se generaran
            int numThreads = 1;
            if (ConfigurationManager.AppSettings["MULTIPROCESO_HABILITADO"].ToString() == "1")
                numThreads = Convert.ToInt32(numNucleos * Convert.ToDouble(ConfigurationManager.AppSettings["MULTIPROCESO_FACTOR"].ToString()));

            //Construye nombre del archivo de log base para todos los hilos y nombre del archivo de salida general
            string strFileNameBase = libLog.ConstruyeNombreArchivoLog();
            string strFileNameSalidaGeneral = strFileNameBase + ".txt";

            #endregion
            
            #region Ejecución del proceso

            //Calculo de cantidad de valores por hilo
            int ValorPorProceso = Convert.ToInt32(Math.Ceiling((Convert.ToDecimal(ListValor.Count()) / numThreads)));

            //Recupera proceso asociado al job
            Process objProcess = Process.GetCurrentProcess();

            //Crea tantos hilos con tareas de reproceso como núcleos del sistema
            Thread[] arrThread = new Thread[numThreads];
            ReprocesoParametros[] arrParametros = new ReprocesoParametros[numThreads];
            for (int i = 0; i < numThreads; ++i)
            {
                //Crea hilo, asociado al reproceso
                arrThread[i] = new Thread(Inicio.ReprocesoEjecucion);
                arrThread[i].Name = String.Format("Reproceso: {0:00} de {1:00}", (i + 1), numThreads);
                arrThread[i].IsBackground = true;
                arrThread[i].Priority = ThreadPriority.AboveNormal;

                //Crea parametros
                arrParametros[i] = new ReprocesoParametros();
                arrParametros[i].intNumHilo = (i + 1);
                arrParametros[i].strArchivoLog = String.Format("{0} {1:00}-{2:00}.txt", strFileNameBase, (i + 1), numThreads);
                arrParametros[i].ListaValores = ListValor.Take(ValorPorProceso).ToList();

                //Descarta los Datos ya utilizados
                ListValor = ListValor.Skip(ValorPorProceso).ToList();

                //Ejecuta hilo y realiza pausa de 1 seg
                arrThread[i].Start(arrParametros[i]);
                if (pausaHilo > 0) { Thread.Sleep(1 * pausaHilo); };

                //Asigna nuevo hilos a nucleo particular
                objProcess.Refresh();
                objProcess.Threads[objProcess.Threads.Count - 1].IdealProcessor = (i % numNucleos);
                objProcess.Threads[objProcess.Threads.Count - 1].ProcessorAffinity = (IntPtr)(1L << (i % numNucleos));
            }

            //Espera ejecución de todos los hilos
            for (int i = 0; i < arrThread.Length; i++) arrThread[i].Join();

            //Concatena y elimina archivos de salida de todos los hilos
            StreamWriter objLogGeneral = new StreamWriter(strFileNameSalidaGeneral, false, Encoding.UTF8);
            for (int i = 0; i < arrParametros.Length; i++)
            {
                //Concatena archivo de log individual y lo elimina
                if (File.Exists(arrParametros[i].strArchivoLog))
                {
                    objLogGeneral.Write(File.ReadAllText(arrParametros[i].strArchivoLog, Encoding.UTF8));
                    objLogGeneral.Flush();
                    File.Delete(arrParametros[i].strArchivoLog);
                }
            }
            objLogGeneral.Flush();
            objLogGeneral.Close();

            #endregion

        }


        #region Creación de Números
        private static List<ModelValor> Crear_Numeros(int p_Inicio, int p_Fin)
        {
            List<ModelValor> ListResult = new List<ModelValor>();

            int NombreCarpetaN = 0;
            int SubCarpeta = NombreCarpetaN;

            for (int item = p_Inicio; item <= p_Fin; item++)
            {
                ModelValor mValor = new ModelValor();
                mValor.Valor = libSintetizador.NumeroALetras(double.Parse(item.ToString())) + " " + libSintetizador.ObtenerSufijoMoneda(double.Parse(item.ToString()));
                mValor.NombreArchivo = item.ToString() + ".wav";

                if (NombreCarpetaN > 2)
                    mValor.NombreCarpeta = (SubCarpeta).ToString();

                if (item == SubCarpeta)
                    SubCarpeta = item + NombreCarpetaN;

                ListResult.Add(mValor);
            }

            return ListResult;
        }
        #endregion

        #region Creación de Números Pesos
        private static List<double> LlenaNumeroPesos()
        {
            List<double> mValorD = new List<double> {
            28918862,
            28195835,
            26954227,
            25609847,
            23084639,
            22699931,
            22363549,
            22055825,
            20998763,
            19959515,
            19635130,
            19154728,
            18969014,
            18729844,
            18660536,
            18552342,
            18515232,
            18419677,
            18156200,
            17743051
            };

            //Filtramos por los valores unicos
            mValorD = mValorD.Distinct().ToList();

            return mValorD;
        }

        private static List<ModelValor> Crear_NumeroPesos(List<double> p_Valor)
        {
            List<ModelValor> ListResult = new List<ModelValor>();

            foreach (double item in p_Valor)
            {
                ModelValor mValor = new ModelValor();
                mValor.Valor = libSintetizador.NumeroALetras(item) + " " + libSintetizador.ObtenerSufijoMoneda(item);
                mValor.NombreArchivo = item.ToString() + ".wav";

                ListResult.Add(mValor);
            }

            return ListResult;
        }
        #endregion

        #region Creación de Fechas
        private static List<ModelValor> Crear_Fecha()
        {
            List<ModelValor> ListResult = new List<ModelValor>();

            DateTime FechaInicio = new DateTime(2009, 12, 31);
            DateTime FechaFin = new DateTime(2031, 1, 1);
            int IntervaloF = 1;

            while ((FechaInicio = FechaInicio.AddDays(IntervaloF)) <= FechaFin)
            {
                ModelValor mValor = new ModelValor();
                mValor.Valor = libSintetizador.NumeroALetras(double.Parse(FechaInicio.Day.ToString()), true) + " de " + FechaInicio.ToString("MMMM") + " del " + libSintetizador.NumeroALetras(double.Parse(FechaInicio.Year.ToString()), true);
                mValor.NombreArchivo = FechaInicio.Year.ToString() + (FechaInicio.Month >= 10 ? FechaInicio.Month.ToString() : "0" + FechaInicio.Month.ToString()) + (FechaInicio.Day >= 10 ? FechaInicio.Day.ToString() : "0" + FechaInicio.Day.ToString()) + ".wav";
                mValor.NombreCarpeta = FechaInicio.Year.ToString();

                ListResult.Add(mValor);
            }

            return ListResult;
        }
        #endregion

        #region Creación de Texto
        private static List<dtoTexto> LlenaTexto()
        {
            List<dtoTexto> mValorD = new List<dtoTexto>();

            //EXCEL
            //="mValorD.Add(new dtoTexto() { Texto = """ & F2 & """, Nombre = """ & G2 & """ });"
            mValorD.Add(new dtoTexto() { Texto = "MARIA PAZ GILLET", Nombre = "136598147" });
            mValorD.Add(new dtoTexto() { Texto = "RODRIGO URETA", Nombre = "106736278" });
            mValorD.Add(new dtoTexto() { Texto = "ARAM STAVROS MUTIS", Nombre = "75000820" });
            mValorD.Add(new dtoTexto() { Texto = "JORGE JAVIER VERDUGO", Nombre = "83180625" });
            mValorD.Add(new dtoTexto() { Texto = "FERNANDO ANTONIO CORNEJO", Nombre = "119117119" });
            mValorD.Add(new dtoTexto() { Texto = "JAIME VALENCIA", Nombre = "211377852" });
            mValorD.Add(new dtoTexto() { Texto = "GIANCARLO PERSI", Nombre = "132526737" });
            mValorD.Add(new dtoTexto() { Texto = "VICTOR IGNACIO CONCHA", Nombre = "171031060" });
            mValorD.Add(new dtoTexto() { Texto = "DAVID ANDRES BORTNICK", Nombre = "160164115" });
            mValorD.Add(new dtoTexto() { Texto = "DIEGO CESAR MENESES", Nombre = "12636024K" });
            mValorD.Add(new dtoTexto() { Texto = "MARIA LORETO GANA", Nombre = "63790958" });
            mValorD.Add(new dtoTexto() { Texto = "JOSE MANUEL AYUSO", Nombre = "144872932" });
            mValorD.Add(new dtoTexto() { Texto = "SADY JEFREY HERRERA", Nombre = "73764424" });
            mValorD.Add(new dtoTexto() { Texto = "JORGE HUMBERTO LECANNELIER", Nombre = "10966670K" });
            mValorD.Add(new dtoTexto() { Texto = "JUAN MANUEL FERNANDO VEGA", Nombre = "60373981" });
            mValorD.Add(new dtoTexto() { Texto = "ARTURO PULIDO", Nombre = "65622335" });
            mValorD.Add(new dtoTexto() { Texto = "MANUEL JOSE ERRAZURIZ", Nombre = "126284934" });
            mValorD.Add(new dtoTexto() { Texto = "DANIEL IVAN MONTENEGRO", Nombre = "103123836" });
            mValorD.Add(new dtoTexto() { Texto = "MARCO ANTONIO GUZMAN", Nombre = "104154042" });
            mValorD.Add(new dtoTexto() { Texto = "FRANCISCA NICOLE NAVARRO", Nombre = "16303123K" });

            return mValorD;
        }
        private static List<ModelValor> Crear_Texto(List<dtoTexto> p_Model)
        {
            List<ModelValor> ListResult = new List<ModelValor>();

            foreach (dtoTexto item in p_Model)
            {

                ModelValor mValor = new ModelValor();
                mValor.Valor = item.Texto;
                mValor.NombreArchivo = item.Nombre + ".wav";

                ListResult.Add(mValor);
            }

            return ListResult;
        }
        #endregion

        #region ReprocesoEjecucion
        private static void ReprocesoEjecucion(object argParametros)
        {
            //Crea objeto de clase para ejecutar proceso y lo ejecuta
            GeneraReproceso cReproceso = new GeneraReproceso();
            cReproceso.Reproceso(argParametros);
        }
        #endregion

        private static List<dtoTexto> LeerArchivo()
        {
            List<dtoTexto> mValorD = new List<dtoTexto>();

            string CARPETA_DESCARGA = ConfigurationManager.AppSettings["ARCHIVO_ORIGEN"].ToString();
            string rutaArchivo = Path.Combine(Environment.CurrentDirectory, CARPETA_DESCARGA);

            // Leer el archivo de texto
            if (File.Exists(rutaArchivo))
            {
                string[] lineas = File.ReadAllLines(rutaArchivo);

                foreach (string linea in lineas)
                {
                    string[] valores = linea.Split('|');
                    if (valores.Length == 2)
                    {
                        string nombre = valores[1].Trim();
                        string texto = valores[0].Trim();
                        mValorD.Add(new dtoTexto { Nombre = nombre, Texto = texto });
                    }
                }
            }
            else
            {
                Console.WriteLine("El archivo de texto no existe.");
            }

            return mValorD;
        }


    }
}
