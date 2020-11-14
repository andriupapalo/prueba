using System;
using System.ComponentModel.DataAnnotations;

namespace Homer_MVC.IcebergModel
{
    public class UsuarioModel
    {
        public int user_id { get; set; }

        [Display(Name = "identificacion")]
        [Required(ErrorMessage = "El campo {0} es obligatorio")]
        public int user_numIdent { get; set; }

        [Display(Name = "nombre")]
        [Required(ErrorMessage = "El campo {0} es obligatorio")]
        public string user_nombre { get; set; }

        [Display(Name = "apellido")]
        [Required(ErrorMessage = "El campo {0} es obligatorio")]
        public string user_apellido { get; set; }

        [Display(Name = "email")]
        [Required(ErrorMessage = "El campo {0} es obligatorio")]
        [EmailAddress(ErrorMessage = "Direccion de correo invalida")]
        public string user_email { get; set; }

        [Display(Name = "telefono")]
        [Required(ErrorMessage = "El campo {0} es obligatorio")]
        public string user_telefono { get; set; }

        [Display(Name = "usuario")]
        [Required(ErrorMessage = "El campo {0} es obligatorio")]
        public string user_usuario { get; set; }

        [Display(Name = "contraseña")]
        [Required(ErrorMessage = "El campo {0} es obligatorio")]
        public string user_password { get; set; }

        [Display(Name = "confirmar contraseña")]
        [Required(ErrorMessage = "El campo {0} es obligatorio")]
        public string user_confirPassword { get; set; }

        [Display(Name = "direccion")]
        [Required(ErrorMessage = "El campo {0} es obligatorio")]
        public string user_direccion { get; set; }

        [Display(Name = "tipo documento")]
        [Required(ErrorMessage = "El campo {0} es obligatorio")]
        public int tpdoc_id { get; set; }

        [Display(Name = "rol")]
        [Required(ErrorMessage = "El campo {0} es obligatorio")]
        public int rol_id { get; set; }

        [Display(Name = "ciudad")]
        [Required(ErrorMessage = "El campo {0} es obligatorio")]
        public int ciu_id { get; set; }

        [Display(Name = "razon inactivo")]
        [ValidaInactivoUsuario]
        public string user_razoninactivo { get; set; }

        public bool user_estado { get; set; }

        [Display(Name = "bodega")]
        [Required(ErrorMessage = "El campo {0} es obligatorio")]
        public int bodccs_cod { get; set; }
        [Display(Name = "bodega visualizacion")]
        public int? bodegas_visualizacion { get; set; }

        public DateTime? userfec_creacion { get; set; }
        public DateTime? userfec_actualizacion { get; set; }
        public int? userid_creacion { get; set; }
        public int? userid_actualizacion { get; set; }

        [Display(Name = "fecha inicial")]
        [ValidaFechaInicialPlanta]
        //public DateTime? fechainiplanta { get; set; }
        public string fechainiplanta { get; set; }

        [Display(Name = "fecha final")]
        [ValidaFechaFinalPlanta]
        //public DateTime? fechafinplanta { get; set; }
        public string fechafinplanta { get; set; }

        public int? dpto_id { get; set; }
        public bool aut_repuestos { get; set; }

        public class ValidaFechaInicialPlanta : ValidationAttribute
        {
            protected override ValidationResult IsValid(object value, ValidationContext validationContext)
            {
                UsuarioModel usuario = (UsuarioModel)validationContext.ObjectInstance;

                if (usuario.fechafinplanta != null)
                {
                    if (value == null || string.IsNullOrEmpty(value.ToString()))
                    {
                        return new ValidationResult("Si asigno una fecha final debe asignar tambien una inicial");
                    }
                }

                return ValidationResult.Success;
            }
        }


        public class ValidaFechaFinalPlanta : ValidationAttribute
        {
            protected override ValidationResult IsValid(object value, ValidationContext validationContext)
            {
                UsuarioModel usuario = (UsuarioModel)validationContext.ObjectInstance;

                if (usuario.fechainiplanta != null)
                {
                    if (value == null || string.IsNullOrEmpty(value.ToString()))
                    {
                        return new ValidationResult("Si asigno una fecha inicial debe asignar tambien una final");
                    }
                }

                return ValidationResult.Success;
            }
        }


        public class ValidaInactivoUsuario : ValidationAttribute
        {
            protected override ValidationResult IsValid(object value, ValidationContext validationContext)
            {
                UsuarioModel usuario = (UsuarioModel)validationContext.ObjectInstance;

                if (!usuario.user_estado)
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