using System;
using System.ComponentModel.DataAnnotations;

namespace Homer_MVC.Models
{
    public class DocumentosCreditoModel
    {
        public int id { get; set; }
        [Required]
        [Display(Name = "nombre")]
        public string nombre { get; set; }
        [Required]
        [Display(Name = "obligatorio")]
        public bool obligatorio { get; set; }
        public bool estado { get; set; }
        [ValidaInactivo]
        [Display(Name = "Razón Inactividad")]
        public string razon_inactivo { get; set; }
        public DateTime fec_creacion { get; set; }
        public int userid_creacion { get; set; }
        public DateTime fec_actualizacion { get; set; }
        public int userid_actualizacion { get; set; }

        public class ValidaInactivo : ValidationAttribute
        {
            protected override ValidationResult IsValid(object value, ValidationContext validationContext)
            {
                DocumentosCreditoModel razon = (DocumentosCreditoModel)validationContext.ObjectInstance;
                if (!razon.estado)
                {
                    if (value == null || string.IsNullOrEmpty(value.ToString()))
                    {
                        return new ValidationResult("El campo " + validationContext.DisplayName + " es requerido.");
                    }
                }
                return ValidationResult.Success;
            }
        }
    }
}