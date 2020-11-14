using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Homer_MVC.Models
{
    public class TpDocRegistroModel
    {
        public int? tpdoc_id { get; set; }
        [Display(Name = "tipo documento")]
        [Required(ErrorMessage = "El campo {0} es requerido")]
        public int? sw { get; set; }
        [Display(Name = "Prefijo")]
        [Required(ErrorMessage = "El campo {0} es requerido")]
        public string prefijo { get; set; }
        [Display(Name = "Descripcion")]
        [Required(ErrorMessage = "El campo {0} es requerido")]
        public string tpdoc_nombre { get; set; }
        public int? tpdocid_licencia { get; set; }
        public DateTime tpdocfec_creacion { get; set; }
        public int tpdocuserid_creacion { get; set; }
        public DateTime? tpdocfec_actualizacion { get; set; }
        public int? tpdocuserid_actualizacion { get; set; }
        [Display(Name = "Estado")]
        [Required(ErrorMessage = "El campo {0} es requerido")]
        public bool tpdoc_estado { get; set; }
        [Display(Name = "Razon Inactivo")]
        [ValidaRazonInactivo]
        public string tpdocrazoninactivo { get; set; }
        public float retencion { get; set; }
        public decimal baseretencion { get; set; }
        public float retica { get; set; }
        public decimal baseica { get; set; }
        public float retiva { get; set; }
        public decimal baseiva { get; set; }
        public int? bodega { get; set; }
        public string texto1 { get; set; }
        public string texto2 { get; set; }
        public string texto3 { get; set; }
        public string texto4 { get; set; }
        public int? concepto1 { get; set; }
        public float ret1 { get; set; }
        public decimal baseret1 { get; set; }
        public float ret2 { get; set; }
        public decimal baseret2 { get; set; }
        public int? concepto2 { get; set; }
        public int? tipo { get; set; }
        public bool aplicaniff { get; set; }
        public bool consecano { get; set; }
        public bool consecmes { get; set; }
        public bool interno { get; set; }
        public bool entrada_salida { get; set; }
        public int? doc_interno_asociado { get; set; }

        public class ValidaRazonInactivo : ValidationAttribute
        {
            protected override ValidationResult IsValid(object value, ValidationContext validationContext)
            {
                TpDocRegistroModel registro = (TpDocRegistroModel)validationContext.ObjectInstance;
               
                    if (!registro.tpdoc_estado)
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