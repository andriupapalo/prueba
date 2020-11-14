using System.ComponentModel.DataAnnotations;

namespace Homer_MVC.IcebergModel
{
    public class CargueManualVhModel
    {
        public int icbvh_id { get; set; }

        [Display(Name = "plan mayor")]
        [Required(ErrorMessage = "El campo {0} es requerido")]
        public string plan_mayor { get; set; }

        public string icbvhrazoninactivo { get; set; }

        [Display(Name = "bodega")]
        [Required(ErrorMessage = "El campo {0} es requerido")]
        public int id_bod { get; set; }

        [Display(Name = "proveedor")]
        [Required(ErrorMessage = "El campo {0} es requerido")]
        public int proveedor_id { get; set; }

        [Display(Name = "nit pago")]
        [Required(ErrorMessage = "El campo {0} es requerido")]
        public int? nit_pago_id { get; set; }

        public int doc_registros { get; set; }
        public int condicion_pago { get; set; }
        public long num_pedido { get; set; }
        public string notas { get; set; }
        public bool es_flota { get; set; }

        [ValidaCero]
        [Display(Name = "costo sin iva")]
        public string costosiniva_vh { get; set; }

        public string costototal_vh { get; set; }
        public string iva_vh { get; set; }
        public int perfilContable { get; set; }
        public string valorFlete { get; set; }
        public string porcentajeIvaFlete { get; set; }
        public string valorDescuentoPie { get; set; }
        public string codigo_modelo { get; set; }
        public string descripcion_modelo { get; set; }
        public string anio_modelo { get; set; }
        public string color_modelo { get; set; }
        public string retencion { get; set; }
        public string retencion_iva { get; set; }
        public string retencion_ica { get; set; }
        public int? concepto1_id { get; set; }
        public int? concepto2_id { get; set; }

        public class ValidaCero : ValidationAttribute
        {
            protected override ValidationResult IsValid(object value, ValidationContext validationContext)
            {
                CargueManualVhModel modelo = (CargueManualVhModel)validationContext.ObjectInstance;

                if (modelo.costosiniva_vh == "0")
                {
                    return new ValidationResult("El campo " + validationContext.DisplayName + " no puede ser cero");
                }

                return ValidationResult.Success;
            }
        }
    }
}