using System;
using System.ComponentModel.DataAnnotations;

namespace Homer_MVC.IcebergModel
{
    public class ModeloVehiculosModel
    {
        [Display(Name = "codigo")]
        [Required(ErrorMessage = "El campo {0} es obligatorio")]
        public string modvh_codigo { get; set; }

        public DateTime modvhfec_creacion { get; set; }
        public int modvhuserid_creacion { get; set; }
        public int modvhid_licencia { get; set; }
        public int modvhuserid_actualizacion { get; set; }
        public DateTime? modvhfec_actualizacion { get; set; }

        [Display(Name = "nombre")]
        [Required(ErrorMessage = "El campo {0} es obligatorio")]
        public string modvh_nombre { get; set; }

        public bool modvh_estado { get; set; }

        [Display(Name = "razon inactivo")]
        [ValidaInactivoModelo]
        public string modvhrazoninactivo { get; set; }

        [Display(Name = "marca")]
        [Required(ErrorMessage = "El campo {0} es obligatorio")]
        public int? mar_vh_id { get; set; }

        public int? seg_vh_id { get; set; }
        public int? anio_modelo { get; set; }
        public string valor_modelo { get; set; }
        public string descripcion_modelo { get; set; }
        public decimal? capacidad { get; set; }
        public int? cilindraje { get; set; }
        public int? grupo_id { get; set; }
        public int? clase_id { get; set; }
        public int? tipo_id { get; set; }
        public int? combustible_id { get; set; }
        public int? perfil_id { get; set; }
        public int? tpmot_id { get; set; }
        public int? unidadcarga { get; set; }
        public int? tipocaja { get; set; }
        public int? clasificacion { get; set; }
        public int? diaslibresgmac { get; set; }
        public int? diaslibrescaplan { get; set; }
        public int? modelogkit { get; set; }

        public int? porcentaje_compra { get; set; }

        public decimal? porcentaje_iva { get; set; }

        public decimal? impuesto_Consumo { get; set; }
        //public Nullable<decimal> porcentaje_compra { get; set; }


        public int? id_porcentaje_iva { get; set; }
        public int? id_impoconsumo { get; set; }
        public int? id_porcentaje_compra { get; set; }

        public int? porcentaje_compra_modal { get; set; }

        public decimal? porcentaje_iva_modal { get; set; }

        public decimal? impuesto_Consumo_modal { get; set; }
        //public Nullable<decimal> porcentaje_compra { get; set; }


        public int? id_porcentaje_iva_modal { get; set; }
        public int? id_impoconsumo_modal { get; set; }
        public int? id_porcentaje_compra_modal { get; set; }
        public int? idequipamiento { get; set; }


        public class ValidaInactivoModelo : ValidationAttribute
        {
            protected override ValidationResult IsValid(object value, ValidationContext validationContext)
            {
                ModeloVehiculosModel modelo = (ModeloVehiculosModel)validationContext.ObjectInstance;

                if (!modelo.modvh_estado)
                {
                    if (value == null || string.IsNullOrEmpty(value.ToString()))
                    {
                        return new ValidationResult("El campo " + validationContext.DisplayName + " es requerido");
                    }
                }

                return ValidationResult.Success;
            }
        }
    }
}