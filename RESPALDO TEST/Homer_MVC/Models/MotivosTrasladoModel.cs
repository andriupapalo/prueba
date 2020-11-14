using System;
using System.ComponentModel.DataAnnotations;

namespace Homer_MVC.Models
{
    public class MotivosTrasladoModel
    {
        public int id { get; set; }
        [Required]
        public string motivo { get; set; }
        public DateTime fec_creacion { get; set; }
        public int user_creacion { get; set; }
        public DateTime fec_actualizacion { get; set; }
        public int user_actualizacion { get; set; }
        [Required]
        public bool estado { get; set; }
        [ValidaInactivo]
        [Display(Name = "Razón Inactividad")]
        public string razon_inactivo { get; set; }

        public class ValidaInactivo : ValidationAttribute
        {
            protected override ValidationResult IsValid(object value, ValidationContext validationContext)
            {
                MotivosTrasladoModel razon = (MotivosTrasladoModel)validationContext.ObjectInstance;
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