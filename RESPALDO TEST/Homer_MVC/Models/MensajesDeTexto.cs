using System;
using System.Globalization;
using System.Net;
using Homer_MVC.IcebergModel;
using Newtonsoft.Json.Linq;

namespace Homer_MVC.Models
{
    public class MensajesDeTexto
    {
        private readonly Iceberg_Context context = new Iceberg_Context();
        public string telefono { get; set; }
        public string correo { get; set; }

        public string enviarmensaje(string telefono, string mensaje)
        {
            var mensajex = new log_mensajes_texto
            {
                telefono = telefono,
                mensaje = mensaje
            };
            var respuesta = "";
            try
            {
                var url = "https://api-messaging.wavy.global/v1/send-sms?destination==57" + telefono + "&messageText=" + mensaje;
                //var url= "https://api-messaging.movile.com/v1/send-sms?destination==57" +telefono + "&messageText=" + mensaje;
                //var url = "https://exiware.partner.atlasws-a.it.movile.com:10001/atlas/sendMessage/?destination=57" +
                //          telefono + "&messageText=" + mensaje + "&key=EXIWARE";

                var textFromFile = new WebClient().DownloadString(url);
                var json = JObject.Parse(textFromFile);
                var status = json.GetValue("status").ToString();
                var fecha = json.GetValue("date").ToString();
                var ticket = json.GetValue("ticket").ToString();

                mensajex.fecha_envio = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss", new CultureInfo("en-US"));
                mensajex.ticket = ticket;
                if (status != "OK")
                {
                    respuesta = "error";
                    mensajex.enviado = false;
                }
                else
                {
                    respuesta = "OK";
                    mensajex.enviado = true;
                }

                context.log_mensajes_texto.Add(mensajex);
                context.SaveChanges();
                
               // respuesta = "OK";
            }
            catch (Exception ex)
            {
                // Agregar seguimiento
                respuesta = "error";
                var errores = ex.InnerException != null ? ex.InnerException.Message!=null?ex.InnerException.Message:ex.Message : ex.Message;
                mensajex.enviado = false;
                context.SaveChanges();

            }


            return respuesta;
        }
    }
}