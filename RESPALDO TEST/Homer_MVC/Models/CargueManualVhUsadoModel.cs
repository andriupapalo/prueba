using System.ComponentModel.DataAnnotations;

namespace Homer_MVC.IcebergModel
{
    public class CargueManualVhUsadoModel
    {
        public int icbvh_id { get; set; }

        public string icbvhrazoninactivo { get; set; }

        //[Display(Name = "tipo")]
        //[Required(ErrorMessage = "El campo {0} es requerido")]
        //public Nullable<int> tpvh_id { get; set; }
        public int? marcvh_id { get; set; }

        [Display(Name = "modelo")]
        [Required(ErrorMessage = "El campo {0} es requerido")]
        public string modvh_id { get; set; }

        [Display(Name = "color")]
        [Required(ErrorMessage = "El campo {0} es requerido")]
        public string colvh_id { get; set; }

        [Display(Name = "serie")]
        [Required(ErrorMessage = "El campo {0} es requerido")]
        public string vin { get; set; }

        [Display(Name = "numero motor")]
        [Required(ErrorMessage = "El campo {0} es requerido")]
        public string nummot_vh { get; set; }

        [Display(Name = "placa")]
        [Required(ErrorMessage = "El campo {0} es requerido")]
        public string plac_vh { get; set; }

        [Display(Name = "año")]
        [Required(ErrorMessage = "El campo {0} es requerido")]
        public int anio_vh { get; set; }

        [Display(Name = "bodega")]
        [Required(ErrorMessage = "El campo {0} es requerido")]
        public string cod_bod { get; set; }

        [Display(Name = "proveedor")]
        [Required(ErrorMessage = "El campo {0} es requerido")]
        public int? proveedor_id { get; set; }

        public int doc_registros { get; set; }
        public int condicion_pago { get; set; }
        public long? num_pedido { get; set; }
        public string notas { get; set; }
        public bool es_flota { get; set; }
    }
}