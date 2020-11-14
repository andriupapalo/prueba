using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Homer_MVC.Models
{
    public class agendaAlistamientoModel
    {
        //select* from icb_vehiculo_eventos where id_tpevento = 5 
        //select* from icb_tpeventos

        public int id { get; set; }
        public int idpedido { get; set; }
        public int? otid { get; set; }
        public bool estadoAlis { get; set; }
        public int? motivo { get; set; }
        [Display(Name = " Véhículo ")]
        public string modeloVh { get; set; }
        [Display(Name = " Número de Serie ")]
        public string serieVh { get; set; }
        [Display(Name = " Color ")]
        public string colorVh { get; set; }
        [Display(Name = " Modelo ")]
        public int? anioModeloVh { get; set; }
        [Display(Name = " Plan Mayor ")]
        public string planMayorVh { get; set; }
        [Display(Name = " Placa ")]
        public string placaVh { get; set; }
        [Display(Name = " Asesor ")]
        public string asesor { get; set; }
        [Display(Name = " Cédula Cliente ")]
        public string cedulaVh { get; set; }
        public int? clienteIdVh { get; set; }
        [Display(Name = " Cliente ")]
        public string clienteVh { get; set; }
        [Display(Name = " Ubicación Actual ")]
        public int? ubivh_id { get; set; }
        public string ubicacionVh { get; set; }
        [Display(Name = " Entregar en ")]
        public int bodegaVh { get; set; }
        public List<listaGeneral> bodegaListVh { get; set; }
        [Display(Name = " Carrocería ")]
        public bool carroceriaVh { get; set; }
        [Display(Name = " Fecha Envío de Carrocería ")]
        public string fcEnvioCarrocVh { get; set; }
        [Display(Name = " Fecha Llegada de Carrocería ")]
        public string fcLlegadaCarrocVh { get; set; }
        [Display(Name = " Fecha de Preentrega ")]
        public string fcPreentregaVh { get; set; }
        [Display(Name = " Fecha de Entrega ")]
        public string fcEntregaVh { get; set; }

    }
}