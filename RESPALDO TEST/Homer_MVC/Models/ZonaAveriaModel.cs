using System;
using System.ComponentModel.DataAnnotations;

namespace Homer_MVC.Models
{
    public class ZonaAveriaModel
    {
        public int zona_id { get; set; }
        public int zona_licencia { get; set; }
        public DateTime zona_fec_creacion { get; set; }
        public int zona_userid_creacion { get; set; }
        public DateTime zona_fec_actualizacion { get; set; }
        public int zona_userid_actualizacion { get; set; }
        [Required]
        public string zona_descripcion { get; set; }
        [Required]
        public bool zona_estado { get; set; }
        [ValidaInactivo]
        [Display(Name = "Razón Inactividad")]
        public string zona_razon_inactivo { get; set; }

        public class ValidaInactivo : ValidationAttribute
        {
            protected override ValidationResult IsValid(object value, ValidationContext validationContext)
            {
                ZonaAveriaModel razon = (ZonaAveriaModel)validationContext.ObjectInstance;
                if (!razon.zona_estado)
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