using IBM.Cloud.SDK.Core.Authentication.Iam;
using IBM.Cloud.SDK.Core.Http.Exceptions;
using IBM.Watson.TextToSpeech.v1;
using System;
using System.IO;
using System.Speech.AudioFormat;
using System.Speech.Synthesis;

namespace ConsoleAppAudios
{
    public class libSintetizador
    {
        #region "Informacion de los audios instalados"
        public static void infoVoice()
        {
            // Initialize a new instance of the SpeechSynthesizer.  
            using (SpeechSynthesizer synth = new SpeechSynthesizer())
            {

                // Output information about all of the installed voices.   
                //Console.WriteLine("Installed voices -");
                foreach (InstalledVoice voice in synth.GetInstalledVoices())
                {
                    VoiceInfo info = voice.VoiceInfo;
                    string AudioFormats = "";
                    foreach (SpeechAudioFormatInfo fmt in info.SupportedAudioFormats)
                    {
                        AudioFormats += String.Format("{0}\n",
                        fmt.EncodingFormat.ToString());
                    }

                    Console.WriteLine(" Name:          " + info.Name);
                    Console.WriteLine(" Culture:       " + info.Culture);
                    Console.WriteLine(" Age:           " + info.Age);
                    Console.WriteLine(" Gender:        " + info.Gender);
                    Console.WriteLine(" Description:   " + info.Description);
                    Console.WriteLine(" ID:            " + info.Id);
                    Console.WriteLine(" Enabled:       " + voice.Enabled);

                    if (info.SupportedAudioFormats.Count != 0)
                    {
                        //Console.WriteLine(" Audio formats: " + AudioFormats);
                    }
                    else
                    {
                        //Console.WriteLine(" No supported audio formats found");
                    }

                    string AdditionalInfo = "";
                    foreach (string key in info.AdditionalInfo.Keys)
                    {
                        AdditionalInfo += String.Format("  {0}: {1}\n", key, info.AdditionalInfo[key]);
                    }

                    //Console.WriteLine(" Additional Info - " + AdditionalInfo);
                    //Console.WriteLine();
                }
            }

            // Initialize a new instance of the SpeechSynthesizer.
            using (SpeechSynthesizer synth = new SpeechSynthesizer())
            {

                // Output information about all of the installed voices. 
                Console.WriteLine("Installed voices -");
                foreach (InstalledVoice voice in synth.GetInstalledVoices())
                {
                    VoiceInfo info = voice.VoiceInfo;
                    Console.WriteLine(" Voice Name: " + info.Name);
                }
            }

            Console.WriteLine();
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }
        #endregion

        #region Generación y guardado de audios
        public static void SynthetizerSave(string texto, string filename, string path, int velocidad = 0, int volumen = 50, string p_Voz = "ANA", bool p_SobreEscribir = false)
        {
            //Comprueba la URL de destino
            string UltimoCaracter = path.Substring(path.Length - 1);
            if (UltimoCaracter != "\\") { path = path + "\\"; }

            //Crea la ruta con el nombre del archivo
            string ruta = path + filename;

            if (p_Voz == "WATSON")
            {
                try
                {

                    IamAuthenticator authenticator = new IamAuthenticator(
                        apikey: "O9_3cRGZo3Gk74MpauiW4eR06WgLcfil1enlDs46vCNx"
                        );

                    TextToSpeechService textToSpeech = new TextToSpeechService(authenticator);
                    textToSpeech.SetServiceUrl("https://api.us-south.text-to-speech.watson.cloud.ibm.com/instances/c63a488f-571b-4ad8-8703-2b25eb190913");
                    textToSpeech.DisableSslVerification(true);
                    textToSpeech.WithHeader("Custom-Header", "header_value");

                    //var result = textToSpeech.ListVoices();

                    var result = textToSpeech.Synthesize(
                                                        text: texto,
                                                        accept: "audio/wav",
                                                        voice: "es-LA_SofiaV3Voice"
                                                        );

                    using (FileStream fs = File.Create(ruta))
                    {
                        result.Result.WriteTo(fs);
                        fs.Close();
                        result.Result.Close();
                    }

                }
                catch (ServiceResponseException e)
                {
                    Console.WriteLine("Error: " + e.Message);
                }
                catch (Exception e)
                {
                    Console.WriteLine("Error: " + e.Message);
                }
            }
            else
            {

                string Nombrevoz = string.Empty;
                //SpeechAudioFormatInfo synthFormat = new SpeechAudioFormatInfo(EncodingFormat.Pcm, 8000, 16, 1, 16000, 2, null);
                SpeechAudioFormatInfo synthFormat = new SpeechAudioFormatInfo(EncodingFormat.ALaw, 8000, 8, 1, 16000, 2, null); //Formato Vocalcom

                if (File.Exists(ruta) && p_SobreEscribir)
                {
                    File.Delete(ruta);
                }

                if (!File.Exists(ruta))
                {
                    using (MemoryStream memoryStream = new MemoryStream())
                    {
                        using (SpeechSynthesizer s = new SpeechSynthesizer())
                        {
                            //Buscamos audio segun voz enviada
                            foreach (InstalledVoice voice in s.GetInstalledVoices())
                            {
                                VoiceInfo info = voice.VoiceInfo;
                                if (info.Name.ToUpper().Contains(p_Voz))
                                {
                                    Nombrevoz = info.Name;
                                    break;
                                }
                            }

                            //Si no encontramos audio le paso el primero que encuentre en MX
                            if (String.IsNullOrEmpty(Nombrevoz))
                            {
                                foreach (InstalledVoice voice in s.GetInstalledVoices())
                                {
                                    VoiceInfo info = voice.VoiceInfo;
                                    if (info.Culture.Name.ToUpper().Contains("MX") || info.Culture.Name.ToUpper().Contains("ES"))
                                    {
                                        Nombrevoz = info.Name;
                                        break;
                                    }
                                }
                            }

                            s.SetOutputToDefaultAudioDevice();
                            s.Rate = velocidad;
                            s.Volume = volumen;
                            s.SelectVoice(Nombrevoz);
                            s.SetOutputToWaveFile(ruta, synthFormat);
                            s.Speak(texto);
                            s.Dispose();

                            //Console.WriteLine("Audio: " + filename);
                        }
                        memoryStream.Flush();
                    }
                }
            }
        }
        #endregion

        #region Transformamos el numero a texto (Palabras)
        public static string NumeroALetras(double value, bool bNum = false)
        {
            string num2Text; value = Math.Truncate(value);
            if (value == 0)
            {
                num2Text = "CERO";
            }
            else if (value == 1 && !bNum)
            {
                num2Text = "UN";
            }
            else if (value == 1 && bNum)
            {
                num2Text = "UNO";
            }
            else if (value == 2)
            {
                num2Text = "DOS";
            }
            else if (value == 3)
            {
                num2Text = "TRES";
            }
            else if (value == 4)
            {
                num2Text = "CUATRO";
            }
            else if (value == 5)
            {
                num2Text = "CINCO";
            }
            else if (value == 6)
            {
                num2Text = "SEIS";
            }
            else if (value == 7)
            {
                num2Text = "SIETE";
            }
            else if (value == 8)
            {
                num2Text = "OCHO";
            }
            else if (value == 9)
            {
                num2Text = "NUEVE";
            }
            else if (value == 10)
            {
                num2Text = "DIEZ";
            }
            else if (value == 11)
            {
                num2Text = "ONCE";
            }
            else if (value == 12)
            {
                num2Text = "DOCE";
            }
            else if (value == 13)
            {
                num2Text = "TRECE";
            }
            else if (value == 14)
            {
                num2Text = "CATORCE";
            }
            else if (value == 15)
            {
                num2Text = "QUINCE";
            }
            else if (value < 20)
            {
                num2Text = "DIECI" + NumeroALetras(value - 10, bNum);
            }
            else if (value == 20)
            {
                num2Text = "VEINTE";
            }
            else if (value < 30)
            {
                num2Text = "VEINTI" + NumeroALetras(value - 20, bNum);
            }
            else if (value == 30)
            {
                num2Text = "TREINTA";
            }
            else if (value == 40)
            {
                num2Text = "CUARENTA";
            }
            else if (value == 50)
            {
                num2Text = "CINCUENTA";
            }
            else if (value == 60)
            {
                num2Text = "SESENTA";
            }
            else if (value == 70)
            {
                num2Text = "SETENTA";
            }
            else if (value == 80)
            {
                num2Text = "OCHENTA";
            }
            else if (value == 90)
            {
                num2Text = "NOVENTA";
            }
            else if (value < 100)
            {
                num2Text = NumeroALetras(Math.Truncate(value / 10) * 10, bNum) + " Y " + NumeroALetras(value % 10, bNum);
            }
            else if (value == 100)
            {
                num2Text = "CIEN";
            }
            else if (value < 200)
            {
                num2Text = "CIENTO " + NumeroALetras(value - 100, bNum);
            }
            else if ((value == 200) || (value == 300) || (value == 400) || (value == 600) || (value == 800))
            {
                num2Text = NumeroALetras(Math.Truncate(value / 100), bNum) + "CIENTOS";
            }
            else if (value == 500)
            {
                num2Text = "QUINIENTOS";
            }
            else if (value == 700)
            {
                num2Text = "SETECIENTOS";
            }
            else if (value == 900)
            {
                num2Text = "NOVECIENTOS";
            }
            else if (value < 1000)
            {
                num2Text = NumeroALetras(Math.Truncate(value / 100) * 100, bNum) + " " + NumeroALetras(value % 100, bNum);
            }
            else if (value == 1000)
            {
                num2Text = "MIL";
            }
            else if (value < 2000)
            {
                num2Text = "MIL " + NumeroALetras(value % 1000, bNum);
            }
            else if (value < 1000000)
            {
                num2Text = NumeroALetras(Math.Truncate(value / 1000), bNum) + " MIL";
                if ((value % 1000) > 0)
                {
                    num2Text = num2Text + " " + NumeroALetras(value % 1000, bNum);
                }
            }
            else if (value == 1000000)
            {
                num2Text = "UN MILLON";
            }
            else if (value < 2000000)
            {
                num2Text = "UN MILLON " + NumeroALetras(value % 1000000, bNum);
            }
            else if (value < 1000000000000)
            {
                num2Text = NumeroALetras(Math.Truncate(value / 1000000), bNum) + " MILLONES ";
                if ((value - Math.Truncate(value / 1000000) * 1000000) > 0)
                {
                    num2Text = num2Text + " " + NumeroALetras(value - Math.Truncate(value / 1000000) * 1000000, bNum);
                }
            }
            else if (value == 1000000000000)
            {
                num2Text = "UN BILLON";
            }
            else if (value < 2000000000000)
            {
                num2Text = "UN BILLON " + NumeroALetras(value - Math.Truncate(value / 1000000000000) * 1000000000000, bNum);
            }
            else
            {
                num2Text = NumeroALetras(Math.Truncate(value / 1000000000000), bNum) + " BILLONES";
                if ((value - Math.Truncate(value / 1000000000000) * 1000000000000) > 0)
                {
                    num2Text = num2Text + " " + NumeroALetras(value - Math.Truncate(value / 1000000000000) * 1000000000000, bNum);
                }
            }

            return num2Text;
        }
        #endregion

        #region Obtiene Sufijo "Peso / Pesos / de Pesos"
        public static string ObtenerSufijoMoneda(double value)
        {
            string valor = value.ToString();
            string primerDigito = string.Empty;
            string UltimosDigitos = string.Empty;
            string resultado = string.Empty;

            primerDigito = valor.Substring(0, 1);
            UltimosDigitos = valor.Substring(1, valor.Length - 1);

            if (UltimosDigitos != "" && int.Parse(UltimosDigitos) > 0 && value < 999999)
            {
                resultado = " PESOS";
            }
            else if (UltimosDigitos != "" && int.Parse(UltimosDigitos) > 0 && value > 999999)
            {
                resultado = " PESOS";
            }
            else if (UltimosDigitos != "" && int.Parse(UltimosDigitos) == 0 && value > 999999)
            {
                resultado = " DE PESOS";
            }
            else if (UltimosDigitos != "" && int.Parse(UltimosDigitos) == 0 && value < 999999)
            {
                resultado = " PESOS";
            }
            else
            {
                resultado = " PESO";
            }

            return resultado;
        }
        #endregion
    }
}
