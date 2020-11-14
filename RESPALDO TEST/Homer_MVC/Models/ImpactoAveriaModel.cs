using System;
using System.ComponentModel.DataAnnotations;

namespace Homer_MVC.Models
{
    public class ImpactoAveriaModel
    {
        public int impacto_id { get; set; }
        public DateTime impacto_fec_creacion { get; set; }
        public int impacto_userid_creacion { get; set; }
        public DateTime impacto_fec_actualizacion { get; set; }
        public int impacto_userid_actualizacion { get; set; }
        [Required]
        public string impacto_descripcion { get; set; }
        [Required]
        public bool impacto_estado { get; set; }
        [ValidaInactivo]
        [Display(Name = "Razón Inactividad")]
        public string impacto_razon_inactivo { get; set; }

        public class ValidaInactivo : ValidationAttribute
        {
            protected override ValidationResult IsValid(object value, ValidationContext validationContext)
            {
                ImpactoAveriaModel razon = (ImpactoAveriaModel)validationContext.ObjectInstance;
                if (!razon.impacto_estado)
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