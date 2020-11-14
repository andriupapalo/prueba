using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Homer_MVC.IcebergModel
{
    public class Prospecto
    {
        public List<Seguimiento> listaSeguimiento;


        public Prospecto()
        {
            listaSeguimiento = new List<Seguimiento>();
        }

        public int? tipoDocumento { get; set; }
        public string numDocumento { get; set; }
        public string digito_verificacion { get; set; }
        public int? tercero_id { get; set; }
        public string razonSocial { get; set; }
        [Display(Name = "primer nombre")]
        [Required(ErrorMessage = "El campo {0} es requerido")]
        public string prinom_tercero { get; set; }
        public string segnom_tercero { get; set; }
        [Display(Name = "primer apellido")]
        [Required(ErrorMessage = "El campo {0} es requerido")]
        public string apellido_tercero { get; set; }
        public string segapellido_tercero { get; set; }

        [Required(ErrorMessage = "El campo {0} es requerido")]
        [Display(Name = "medio comunicacion")]
        public int? medcomun_id { get; set; }

        [EmailAddress(ErrorMessage = "Direccion de correo invalida")]
        public string email_tercero { get; set; }

        public int? numeroFamiliares { get; set; }
        public int? numeroAmigos { get; set; }
        [Display(Name = "genero")]
        [Required(ErrorMessage = "El campo {0} es requerido")]
        public int? genero_tercero { get; set; }

        [Display(Name = "tipo tramite")]
        [Required(ErrorMessage = "El campo {0} es requerido")]
        public int tptrapros_id { get; set; }

        //[Display(Name = "fuente")]
        //[Required(ErrorMessage = "El campo {0} es requerido")]
        public int? tporigen_id { get; set; }

        [Display(Name = "asesor")]
        //[ValidaSiRequiereAsesor]
        //[Range(1, int.MaxValue,ErrorMessage = "Debe asignar un asesor")]
        //[Required(ErrorMessage = "El campo {0} es requerido")]
        public int? asesor_id { get; set; }
        [Display(Name = "Buscar asesor")]
       
        //[Range(1, int.MaxValue,ErrorMessage = "Debe asignar un asesor")]
        //[Required(ErrorMessage = "El campo {0} es requerido")]
        public int? buscarAsesor_id { get; set; }

        public int? subfuente_id { get; set; }
        public int? temperatura { get; set; }
        public string observacion { get; set; }

        [MinLength(5, ErrorMessage = "Minimo 7 caracteres")]
        [MaxLength(10, ErrorMessage = "Maximo 10 caracteres")]
        public string telf_tercero { get; set; }

        public string modelo { get; set; }
        public string modeloOpcional { get; set; }
        [Display(Name = "celular")]
        //[Required(ErrorMessage = "El campo {0} es requerido")]
        //[MinLength(10, ErrorMessage = "Minimo 10 caracteres")]
        //[MaxLength(15, ErrorMessage = "Maximo 15 caracteres")]
        public string celular_tercero { get; set; }

        public bool habdtautor_correo { get; set; }
        public bool habdtautor_celular { get; set; }
        public bool habdtautor_msm { get; set; }
        public bool habdtautor_whatsapp { get; set; }

        [Display(Name = "evento")]
        //[Required(ErrorMessage = "El campo {0} es requerido")]
        [ValidaLugarEvento]
        public int? eventoOrigen { get; set; }


        public class ValidaSiRequiereAsesor : ValidationAttribute
        {
            protected override ValidationResult IsValid(object value, ValidationContext validationContext)
            {
                Prospecto prospecto = (Prospecto)validationContext.ObjectInstance;
                //using (Iceberg_Context context = new Iceberg_Context())
                //{
                //var buscarSiRequiere = context.tp_origen.FirstOrDefault(x => x.tporigen_id == prospecto.tporigen_id);
                if (prospecto.tptrapros_id == 1 || prospecto.tptrapros_id == 2)
                {
                    //if (buscarSiRequiere.evento)
                    //{
                    if (value == null || string.IsNullOrEmpty(value.ToString()) || value.ToString() == "0")
                    {
                        return new ValidationResult("El campo " + validationContext.DisplayName + " es requerido");
                    }
                }
                // }
                //}
                return ValidationResult.Success;
            }
        }


        public class ValidaLugarEvento : ValidationAttribute
        {
            protected override ValidationResult IsValid(object value, ValidationContext validationContext)
            {
                Prospecto prospecto = (Prospecto)validationContext.ObjectInstance;
                using (Iceberg_Context context = new Iceberg_Context())
                {
                    tp_origen buscarSiRequiere =
                        context.tp_origen.FirstOrDefault(x => x.tporigen_id == prospecto.tporigen_id);
                    if (buscarSiRequiere != null)
                    {
                        if (buscarSiRequiere.evento)
                        {
                            if (value == null || string.IsNullOrEmpty(value.ToString()) || value.ToString() == "0")
                            {
                                return new ValidationResult("El campo " + validationContext.DisplayName +
                                                            " es requerido");
                            }
                        }
                    }
                }

                return ValidationResult.Success;
            }
        }


        public class Seguimiento
        {
            public string TipoSeguimiento { get; set; }
            public string Nota { get; set; }
            public string Fecha { get; set; }
            public string Usuario { get; set; }
        }
    }
}