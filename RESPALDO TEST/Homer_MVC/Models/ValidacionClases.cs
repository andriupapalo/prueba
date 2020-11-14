using System;
using System.ComponentModel.DataAnnotations;

namespace Homer_MVC.IcebergModel
{
    public class ValidacionClases
    {
        public class Validattempario
        {
            [Display(Name = "codigo")]
            [Required(ErrorMessage = "El campo {0} es requerido")]
            public string codigo { get; set; }

            [Display(Name = "operacion")]
            [Required(ErrorMessage = "El campo {0} es requerido")]
            public string operacion { get; set; }

            [Display(Name = "tipo operacion")]
            [Required(ErrorMessage = "El campo {0} es requerido")]
            public int tipooperacion { get; set; }
        }


        public class Validathorario_taller
        {
            [Display(Name = "hora inicial")]
            [Required(ErrorMessage = "El campo {0} es requerido")]
            public TimeSpan hora_inicial { get; set; }

            [Display(Name = "hora final")]
            [Required(ErrorMessage = "El campo {0} es requerido")]
            public TimeSpan hora_final { get; set; }

            [Display(Name = "lapso tiempo")]
            [Required(ErrorMessage = "El campo {0} es requerido")]
            public int lapso_tiempo { get; set; }

            [Display(Name = "bodega")]
            [Required(ErrorMessage = "El campo {0} es requerido")]
            public int bodega_id { get; set; }
        }


        public class Validattipobahia
        {
            [Display(Name = "descripcion")]
            [Required(ErrorMessage = "El campo {0} es requerido")]
            public string descripcion { get; set; }

            [Display(Name = "razon inactivo")]
            [ValidaInactivottipobahia]
            public string razon_inactivo { get; set; }
        }

        public class ValidaInactivottipobahia : ValidationAttribute
        {
            protected override ValidationResult IsValid(object value, ValidationContext validationContext)
            {
                ttipobahia tipoBahia = (ttipobahia)validationContext.ObjectInstance;

                if (!tipoBahia.estado)
                {
                    if (value == null || string.IsNullOrEmpty(value.ToString()))
                    {
                        return new ValidationResult("El campo " + validationContext.DisplayName + " es requerido");
                    }
                }

                return ValidationResult.Success;
            }
        }


        public class Validatcitamotivoentrada
        {
            [Display(Name = "descripcion")]
            [Required(ErrorMessage = "El campo {0} es requerido")]
            public string descripcion { get; set; }

            [Display(Name = "razon inactivo")]
            [ValidaInactivotcitamotivoentrada]
            public string razon_inactivo { get; set; }
        }

        public class ValidaInactivotcitamotivoentrada : ValidationAttribute
        {
            protected override ValidationResult IsValid(object value, ValidationContext validationContext)
            {
                tcitamotivoentrada motivoEntrada = (tcitamotivoentrada)validationContext.ObjectInstance;

                if (!motivoEntrada.estado)
                {
                    if (value == null || string.IsNullOrEmpty(value.ToString()))
                    {
                        return new ValidationResult("El campo " + validationContext.DisplayName + " es requerido");
                    }
                }

                return ValidationResult.Success;
            }
        }


        public class Validattipocita
        {
            [Display(Name = "descripcion")]
            [Required(ErrorMessage = "El campo {0} es requerido")]
            public string descripcion { get; set; }

            [Display(Name = "razon inactivo")]
            [ValidaInactivottipocita]
            public string razon_inactivo { get; set; }
        }

        public class ValidaInactivottipocita : ValidationAttribute
        {
            protected override ValidationResult IsValid(object value, ValidationContext validationContext)
            {
                ttipocita tipoCita = (ttipocita)validationContext.ObjectInstance;

                if (!tipoCita.estado)
                {
                    if (value == null || string.IsNullOrEmpty(value.ToString()))
                    {
                        return new ValidationResult("El campo " + validationContext.DisplayName + " es requerido");
                    }
                }

                return ValidationResult.Success;
            }
        }


        public class Validatcitamotivocancela
        {
            [Display(Name = "descripcion")]
            [Required(ErrorMessage = "El campo {0} es requerido")]
            public string Descripcion { get; set; }

            [Display(Name = "razon inactivo")]
            [ValidaInactivotcitamotivocancelas]
            public string razon_inactivo { get; set; }
        }

        public class ValidaInactivotcitamotivocancelas : ValidationAttribute
        {
            protected override ValidationResult IsValid(object value, ValidationContext validationContext)
            {
                tcitamotivocancela motivo = (tcitamotivocancela)validationContext.ObjectInstance;

                if (!motivo.estado)
                {
                    if (value == null || string.IsNullOrEmpty(value.ToString()))
                    {
                        return new ValidationResult("El campo " + validationContext.DisplayName + " es requerido");
                    }
                }

                return ValidationResult.Success;
            }
        }


        public class Validatcitasestados
        {
            [Display(Name = "tipo estado")]
            [Required(ErrorMessage = "El campo {0} es requerido")]
            public string tipoestado { get; set; }

            [Display(Name = "descripcion")]
            [Required(ErrorMessage = "El campo {0} es requerido")]
            public string Descripcion { get; set; }

            [Display(Name = "razon inactivo")]
            [ValidaInactivotcitasestados]
            public string razon_inactivo { get; set; }
        }

        public class ValidaInactivotcitasestados : ValidationAttribute
        {
            protected override ValidationResult IsValid(object value, ValidationContext validationContext)
            {
                tcitasestados estadoCita = (tcitasestados)validationContext.ObjectInstance;

                if (!estadoCita.estado)
                {
                    if (value == null || string.IsNullOrEmpty(value.ToString()))
                    {
                        return new ValidationResult("El campo " + validationContext.DisplayName + " es requerido");
                    }
                }

                return ValidationResult.Success;
            }
        }


        public class Validadocumentos_posfechados
        {
            [Display(Name = "nit")]
            [Required(ErrorMessage = "El campo {0} es requerido")]
            public int idtercero { get; set; }

            [Display(Name = "banco")]
            [Required(ErrorMessage = "El campo {0} es requerido")]
            public int banco { get; set; }

            [Display(Name = "numero cheque")]
            [Required(ErrorMessage = "El campo {0} es requerido")]
            public string numero_cheque { get; set; }

            [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:MM/dd/yyyy}")]
            [Display(Name = "fecha recibido")]
            [Required(ErrorMessage = "El campo {0} es requerido")]
            public DateTime fecrecibido { get; set; }

            [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:MM/dd/yyyy}")]
            [Display(Name = "fecha consignación")]
            [Required(ErrorMessage = "El campo {0} es requerido")]
            public DateTime fecconsignacion { get; set; }

            [Display(Name = "valor")]
            [Required(ErrorMessage = "El campo {0} es requerido")]
            public decimal Valor { get; set; }

            [Display(Name = "cuenta banco")]
            [Required(ErrorMessage = "El campo {0} es requerido")]
            public string cuentabanco { get; set; }

            [Display(Name = "perfil contable")]
            [Required(ErrorMessage = "El campo {0} es requerido")]
            public int idperfil { get; set; }
        }


        public class Validatbahias
        {
            [Display(Name = "bodega")]
            [Required(ErrorMessage = "El campo {0} es requerido")]
            public int bodega { get; set; }

            [Display(Name = "tipo bahia")]
            [Required(ErrorMessage = "El campo {0} es requerido")]
            public int tipo_bahia { get; set; }

            [Display(Name = "codigo")]
            [Required(ErrorMessage = "El campo {0} es requerido")]
            public string codigo_bahia { get; set; }

            [Display(Name = "tipo tecnico")]
            [Required(ErrorMessage = "El campo {0} es requerido")]
            public int tipo_tecnico { get; set; }

            [Display(Name = "razon inactivo")]
            [ValidaInactivotbahias]
            public string razon_inactivo { get; set; }
        }

        public class ValidaInactivotbahias : ValidationAttribute
        {
            protected override ValidationResult IsValid(object value, ValidationContext validationContext)
            {
                tbahias bahia = (tbahias)validationContext.ObjectInstance;

                if (!bahia.estado)
                {
                    if (value == null || string.IsNullOrEmpty(value.ToString()))
                    {
                        return new ValidationResult("El campo " + validationContext.DisplayName + " es requerido");
                    }
                }

                return ValidationResult.Success;
            }
        }


        public class Validattipotecnico
        {
            [Display(Name = "descripcion")]
            [Required(ErrorMessage = "El campo {0} es requerido")]
            public string Descripcion { get; set; }

            [Display(Name = "Razón Inactividad")]
            [ValidaInactivottipotecnico]
            public string razon_inactivo { get; set; }
        }

        public class ValidaInactivottipotecnico : ValidationAttribute
        {
            protected override ValidationResult IsValid(object value, ValidationContext validationContext)
            {
                ttipotecnico tipo = (ttipotecnico)validationContext.ObjectInstance;

                if (!tipo.estado)
                {
                    if (value == null || string.IsNullOrEmpty(value.ToString()))
                    {
                        return new ValidationResult("El campo " + validationContext.DisplayName + " es requerido");
                    }
                }

                return ValidationResult.Success;
            }
        }


        public class Validatcamptaller
        {
            [Display(Name = "nombre")]
            [Required(ErrorMessage = "El campo {0} es requerido")]
            public string nombre { get; set; }

            [Display(Name = "fecha inicio")]
            [Required(ErrorMessage = "El campo {0} es requerido")]
            public DateTime fecha_inicio { get; set; }

            [Display(Name = "Razón Inactividad")]
            [ValidaInactivotcamptaller]
            public string razon_inactivo { get; set; }
        }

        public class ValidaInactivotcamptaller : ValidationAttribute
        {
            protected override ValidationResult IsValid(object value, ValidationContext validationContext)
            {
                tcamptaller motivo = (tcamptaller)validationContext.ObjectInstance;

                if (!motivo.estado)
                {
                    if (value == null || string.IsNullOrEmpty(value.ToString()))
                    {
                        return new ValidationResult("El campo " + validationContext.DisplayName + " es requerido");
                    }
                }

                return ValidationResult.Success;
            }
        }


        public class Validarsolicitudesrepuestos
        {
            [Display(Name = "bodega")]
            [Required(ErrorMessage = "El campo {0} es requerido")]
            public int bodega { get; set; }

            [Display(Name = "referencia")]
            [Required(ErrorMessage = "El campo {0} es requerido")]
            public string ref_codigo { get; set; }

            [Display(Name = "cantidad")]
            [Required(ErrorMessage = "El campo {0} es requerido")]
            public int cantidad { get; set; }

            [Display(Name = "tipo solicitud")]
            [Required(ErrorMessage = "El campo {0} es requerido")]
            public int tiposolicitud { get; set; }
        }


        public class Validausuarios_autorizaciones
        {
            [Display(Name = "usuario")]
            [Required(ErrorMessage = "El campo {0} es requerido")]
            public int user_id { get; set; }

            [Display(Name = "bodega")]
            [Required(ErrorMessage = "El campo {0} es requerido")]
            public int bodega_id { get; set; }

            [Display(Name = "tipo autorizacion")]
            [Required(ErrorMessage = "El campo {0} es requerido")]
            public int? tipoautorizacion { get; set; }
        }


        public class Validarordencompra
        {
            [Display(Name = "tipo documento")]
            [Required(ErrorMessage = "El campo {0} es requerido")]
            public int idtipodoc { get; set; }

            [Display(Name = "bodega")]
            [ValidaInactivoValidaBodegaEnCero]
            public int bodega { get; set; }

            [Display(Name = "proveedor")]
            [Required(ErrorMessage = "El campo {0} es requerido")]
            public int proveedor { get; set; }

            [Display(Name = "destinatario")]
            [Required(ErrorMessage = "El campo {0} es requerido")]
            public int destinatario { get; set; }

            [Display(Name = "vendedor")]
            [Required(ErrorMessage = "El campo {0} es requerido")]
            public int vendedor { get; set; }

            [Display(Name = "condicion pago")]
            [Required(ErrorMessage = "El campo {0} es requerido")]
            public int condicion_pago { get; set; }

            [Display(Name = "dias validez")]
            [Required(ErrorMessage = "El campo {0} es requerido")]
            public int diasvalidez { get; set; }

            [Display(Name = "direccion")]
            [Required(ErrorMessage = "El campo {0} es requerido")]
            public int? direccion { get; set; }

            [Display(Name = "tipo orden")]
            [Required(ErrorMessage = "El campo {0} es requerido")]
            public int? tipoorden { get; set; }
        }

        public class ValidaInactivoValidaBodegaEnCero : ValidationAttribute
        {
            protected override ValidationResult IsValid(object value, ValidationContext validationContext)
            {
                rordencompra motivo = (rordencompra)validationContext.ObjectInstance;

                if (motivo.bodega == 0)
                {
                    return new ValidationResult("El campo " + validationContext.DisplayName + " es requerido");
                }

                return ValidationResult.Success;
            }
        }


        public class Validamotcompra
        {
            [Display(Name = "motivo")]
            [Required(ErrorMessage = "El campo {0} es requerido")]
            public string Motivo { get; set; }

            [Display(Name = "razon inactivo")]
            [ValidaInactivoValidamotcompra]
            public string razon_inactivo { get; set; }
        }

        public class ValidaInactivoValidamotcompra : ValidationAttribute
        {
            protected override ValidationResult IsValid(object value, ValidationContext validationContext)
            {
                motcompra motivo = (motcompra)validationContext.ObjectInstance;

                if (!motivo.estado)
                {
                    if (value == null || string.IsNullOrEmpty(value.ToString()))
                    {
                        return new ValidationResult("El campo " + validationContext.DisplayName + " es requerido");
                    }
                }

                return ValidationResult.Success;
            }
        }


        public class Validartipocompra
        {
            [Display(Name = "descripcion")]
            [Required(ErrorMessage = "El campo {0} es requerido")]
            public string descripcion { get; set; }

            [Display(Name = "razon inactivo")]
            [ValidaInactivoRTipoCompra]
            public string razon_inactivo { get; set; }
        }

        public class ValidaInactivoRTipoCompra : ValidationAttribute
        {
            protected override ValidationResult IsValid(object value, ValidationContext validationContext)
            {
                rtipocompra tipo = (rtipocompra)validationContext.ObjectInstance;

                if (!tipo.estado)
                {
                    if (value == null || string.IsNullOrEmpty(value.ToString()))
                    {
                        return new ValidationResult("El campo " + validationContext.DisplayName + " es requerido");
                    }
                }

                return ValidationResult.Success;
            }
        }


        public class Validavingresovehiculo
        {
            [Display(Name = "descripcion")]
            [Required(ErrorMessage = "El campo {0} es requerido")]
            public string descripcion { get; set; }

            [Display(Name = "tipo respuesta")]
            [Required(ErrorMessage = "El campo {0} es requerido")]
            public string tiporespuesta { get; set; }

            [Display(Name = "razon inactivo")]
            [ValidaInactivoVIngresoVehiculo]
            public string razon_inactivo { get; set; }
        }


        public class ValidaInactivoVIngresoVehiculo : ValidationAttribute
        {
            protected override ValidationResult IsValid(object value, ValidationContext validationContext)
            {
                vingresovehiculo checkIngreso = (vingresovehiculo)validationContext.ObjectInstance;

                if (!checkIngreso.estado)
                {
                    if (value == null || string.IsNullOrEmpty(value.ToString()))
                    {
                        return new ValidationResult("El campo " + validationContext.DisplayName + " es requerido");
                    }
                }

                return ValidationResult.Success;
            }
        }


        public class encab_documento
        {
            public int perfilContable { get; set; }
        }


        public class Validaperfil_contable_documento
        {
            [Display(Name = "tipo documento")]
            [Required(ErrorMessage = "El campo {0} es requerido")]
            public string tipo { get; set; }

            [Display(Name = "codigo")]
            [Required(ErrorMessage = "El campo {0} es requerido")]
            public int codigo { get; set; }

            [Display(Name = "descripcion")]
            [Required(ErrorMessage = "El campo {0} es requerido")]
            public string descripcion { get; set; }
        }


        public class Validaresolucionfactura
        {
            [Display(Name = "tipo documento")]
            [Required(ErrorMessage = "El campo {0} es requerido")]
            public int tipodoc { get; set; }

            [Display(Name = "resolucion")]
            [Required(ErrorMessage = "El campo {0} es requerido")]
            public string resolucion { get; set; }

            [Display(Name = "fecha inicial")]
            [Required(ErrorMessage = "El campo {0} es requerido")]
            public DateTime fechaini { get; set; }

            [Display(Name = "fecha final")]
            [Required(ErrorMessage = "El campo {0} es requerido")]
            public DateTime fechafin { get; set; }

            [Display(Name = "consecutivo inicial")]
            [Required(ErrorMessage = "El campo {0} es requerido")]
            public int consecini { get; set; }

            [Display(Name = "consecutivo final")]
            [Required(ErrorMessage = "El campo {0} es requerido")]
            public int consecfin { get; set; }

            [Display(Name = "numero facturas")]
            [Required(ErrorMessage = "El campo {0} es requerido")]
            public int numfacturas { get; set; }

            [Display(Name = "razon inactivo")]
            [ValidaInactivoResolucionFactura]
            public string razon_inactivo { get; set; }

            [Display(Name = "dias de alerta")]
            [Required(ErrorMessage = "El campo {0} es requerido")]
            public int diasaviso { get; set; }

            [Display(Name = "consecutivo alerta")]
            [Required(ErrorMessage = "El campo {0} es requerido")]
            public int consecaviso { get; set; }
        }

        public class ValidaInactivoResolucionFactura : ValidationAttribute
        {
            protected override ValidationResult IsValid(object value, ValidationContext validationContext)
            {
                resolucionfactura resolucion = (resolucionfactura)validationContext.ObjectInstance;

                if (!resolucion.estado)
                {
                    if (value == null || string.IsNullOrEmpty(value.ToString()))
                    {
                        return new ValidationResult("El campo " + validationContext.DisplayName + " es requerido");
                    }
                }

                return ValidationResult.Success;
            }
        }


        public class Validaicb_vehiculo_eventos
        {
            [Display(Name = "plan mayor")]
            [Required(ErrorMessage = "El campo {0} es requerido")]
            public string planmayor { get; set; }

            [Display(Name = "tipo evento")]
            [Required(ErrorMessage = "El campo {0} es requerido")]
            public int id_tpevento { get; set; }

            [Display(Name = "fecha")]
            [Required(ErrorMessage = "El campo {0} es requerido")]
            public DateTime fechaevento { get; set; }
        }


        public class Validavaccesoriomodelo
        {
            [Display(Name = "modelo")]
            [Required(ErrorMessage = "El campo {0} es requerido")]
            public string modeloid { get; set; }
        }


        public class Validaveventosorigen
        {
            [Display(Name = "origen evento")]
            [Required(ErrorMessage = "El campo {0} es requerido")]
            public int? evento_id { get; set; }

            [Display(Name = "descripcion")]
            [Required(ErrorMessage = "El campo {0} es requerido")]
            public string descripcion { get; set; }

            [Display(Name = "fecha inicio")]
            [Required(ErrorMessage = "El campo {0} es requerido")]
            public DateTime fechaini { get; set; }

            [Display(Name = "fecha fin")]
            [Required(ErrorMessage = "El campo {0} es requerido")]
            public DateTime fechafin { get; set; }
        }


        public class Validatp_origen
        {
            [Display(Name = "nombre")]
            [Required(ErrorMessage = "El campo {0} es requerido")]
            public string tporigen_nombre { get; set; }

            [Display(Name = "razon inactivo")]
            [ValidaTipoOrigenInactivo]
            public string tporigenrazoninactivo { get; set; }
        }

        public class ValidaTipoOrigenInactivo : ValidationAttribute
        {
            protected override ValidationResult IsValid(object value, ValidationContext validationContext)
            {
                tp_origen tipoOrigen = (tp_origen)validationContext.ObjectInstance;

                if (!tipoOrigen.tporigen_estado)
                {
                    if (value == null || string.IsNullOrEmpty(value.ToString()))
                    {
                        return new ValidationResult("El campo " + validationContext.DisplayName + " es requerido");
                    }
                }

                return ValidationResult.Success;
            }
        }


        public class Validaicb_gravedad_averias
        {
            [Display(Name = "codigo")]
            [Required(ErrorMessage = "El campo {0} es requerido")]
            public string grave_codigo { get; set; }

            [Display(Name = "descripcion")]
            [Required(ErrorMessage = "El campo {0} es requerido")]
            public string grave_descripcion { get; set; }

            [Display(Name = "razon inactivo")]
            [Required(ErrorMessage = "El campo {0} es requerido")]
            public string grave_razon_inactivo { get; set; }
        }


        public class Validaicb_averias
        {
            [Display(Name = "codigo")]
            [Required(ErrorMessage = "El campo {0} es requerido")]
            public string ave_codigo { get; set; }

            [Display(Name = "descripcion")]
            [Required(ErrorMessage = "El campo {0} es requerido")]
            public string ave_descripcion { get; set; }

            [Display(Name = "razon inactivo")]
            [Required(ErrorMessage = "El campo {0} es requerido")]
            public string ave_razon_inactivo { get; set; }
        }


        public class Validatramitador_vh
        {
            [Display(Name = "documento")]
            [Required(ErrorMessage = "El campo {0} es requerido")]
            public string tramitador_documento { get; set; }

            [Display(Name = "primer nombre")]
            [Required(ErrorMessage = "El campo {0} es requerido")]
            public string tramitadorpri_nombre { get; set; }

            [Display(Name = "segundo nombre")]
            [Required(ErrorMessage = "El campo {0} es requerido")]
            public string tramitadorseg_nombre { get; set; }

            [Display(Name = "apellidos")]
            [Required(ErrorMessage = "El campo {0} es requerido")]
            public string tramitador_apellidos { get; set; }

            [Display(Name = "celular")]
            [Required(ErrorMessage = "El campo {0} es requerido")]
            public string tramitador_celular { get; set; }

            [Display(Name = "tipo")]
            [Required(ErrorMessage = "El campo {0} es requerido")]
            public int? tramitador_idtipo { get; set; }

            [Display(Name = "razon inactivo")]
            [ValidaTramitadorInactivo]
            public string tramitador_razoninactivo { get; set; }
        }

        public class ValidaTramitadorInactivo : ValidationAttribute
        {
            protected override ValidationResult IsValid(object value, ValidationContext validationContext)
            {
                tramitador_vh tramitador = (tramitador_vh)validationContext.ObjectInstance;

                if (!tramitador.tramitador_estado)
                {
                    if (value == null || string.IsNullOrEmpty(value.ToString()))
                    {
                        return new ValidationResult("El campo " + validationContext.DisplayName + " es requerido");
                    }
                }

                return ValidationResult.Success;
            }
        }


        public class Validaclasificacion_repuesto
        {
            [Display(Name = "nombre")]
            [Required(ErrorMessage = "El campo {0} es requerido")]
            public string clarpto_nombre { get; set; }

            [Display(Name = "razon inactivo")]
            [ValidaClasificacionRepuestoInactivo]
            public string clarptorazoninactivo { get; set; }
        }

        public class ValidaClasificacionRepuestoInactivo : ValidationAttribute
        {
            protected override ValidationResult IsValid(object value, ValidationContext validationContext)
            {
                clasificacion_repuesto clasificacionRepuesto = (clasificacion_repuesto)validationContext.ObjectInstance;

                if (!clasificacionRepuesto.clarpto_estado)
                {
                    if (value == null || string.IsNullOrEmpty(value.ToString()))
                    {
                        return new ValidationResult("El campo " + validationContext.DisplayName + " es requerido");
                    }
                }

                return ValidationResult.Success;
            }
        }


        public class Validagrupo_repuesto
        {
            [Display(Name = "nombre")]
            [Required(ErrorMessage = "El campo {0} es requerido")]
            public string grupo_nombre { get; set; }

            [Display(Name = "razon inactivo")]
            [ValidaGrupoRepuestoInactivo]
            public string gruporazoninactivo { get; set; }
        }

        public class ValidaGrupoRepuestoInactivo : ValidationAttribute
        {
            protected override ValidationResult IsValid(object value, ValidationContext validationContext)
            {
                grupo_repuesto grupoRepuesto = (grupo_repuesto)validationContext.ObjectInstance;

                if (!grupoRepuesto.grupo_estado)
                {
                    if (value == null || string.IsNullOrEmpty(value.ToString()))
                    {
                        return new ValidationResult("El campo " + validationContext.DisplayName + " es requerido");
                    }
                }

                return ValidationResult.Success;
            }
        }


        public class Validatptramitador_vh
        {
            [Display(Name = "nombre")]
            [Required(ErrorMessage = "El campo {0} es requerido")]
            public string tptramivh_nombre { get; set; }

            [Display(Name = "razon inactivo")]
            [ValidaTipoTramitadorInactivo]
            public string tptramivhrazoninactivo { get; set; }
        }

        public class ValidaTipoTramitadorInactivo : ValidationAttribute
        {
            protected override ValidationResult IsValid(object value, ValidationContext validationContext)
            {
                tptramitador_vh tipoTramitador = (tptramitador_vh)validationContext.ObjectInstance;

                if (!tipoTramitador.tptramivh_estado)
                {
                    if (value == null || string.IsNullOrEmpty(value.ToString()))
                    {
                        return new ValidationResult("El campo " + validationContext.DisplayName + " es requerido");
                    }
                }

                return ValidationResult.Success;
            }
        }


        public class Validaclacompra_vehiculo
        {
            [Display(Name = "nombre")]
            [Required(ErrorMessage = "El campo {0} es requerido")]
            public string clacompvh_nombre { get; set; }

            [Display(Name = "razon inactivo")]
            [ValidaClasificacionCompraInactivo]
            public string clacompvhrazoninactivo { get; set; }
        }

        public class ValidaClasificacionCompraInactivo : ValidationAttribute
        {
            protected override ValidationResult IsValid(object value, ValidationContext validationContext)
            {
                clacompra_vehiculo clasificacion = (clacompra_vehiculo)validationContext.ObjectInstance;

                if (!clasificacion.clacompvh_estado)
                {
                    if (value == null || string.IsNullOrEmpty(value.ToString()))
                    {
                        return new ValidationResult("El campo " + validationContext.DisplayName + " es requerido");
                    }
                }

                return ValidationResult.Success;
            }
        }


        public class Validaicb_tptramite_prospecto
        {
            [Display(Name = "nombre")]
            [Required(ErrorMessage = "El campo {0} es requerido")]
            public string tptrapros_descripcion { get; set; }

            [Display(Name = "razon inactivo")]
            [ValidaInactivoTpTramite_prospecto]
            public string tptrapros_razoninactivo { get; set; }
        }


        public class ValidaInactivoTpTramite_prospecto : ValidationAttribute
        {
            protected override ValidationResult IsValid(object value, ValidationContext validationContext)
            {
                icb_tptramite_prospecto tipoTRamite = (icb_tptramite_prospecto)validationContext.ObjectInstance;

                if (!tipoTRamite.tptrapros_estado)
                {
                    if (value == null || string.IsNullOrEmpty(value.ToString()))
                    {
                        return new ValidationResult("El campo " + validationContext.DisplayName + " es requerido");
                    }
                }

                return ValidationResult.Success;
            }
        }


        public class Validaicb_tpeventos
        {
            [Display(Name = "nombre")]
            [Required(ErrorMessage = "El campo {0} es requerido")]
            public string tpevento_nombre { get; set; }

            [Display(Name = "razon inactivo")]
            [ValidaTipoEventosInactivo]
            public string tpeventorazoninactivo { get; set; }

            [Display(Name = "codigo evento")]
            [Required(ErrorMessage = "El campo {0} es requerido")]
            public int? codigoevento { get; set; }
        }

        public class ValidaTipoEventosInactivo : ValidationAttribute
        {
            protected override ValidationResult IsValid(object value, ValidationContext validationContext)
            {
                icb_tpeventos tipoEventos = (icb_tpeventos)validationContext.ObjectInstance;

                if (!tipoEventos.tpevento_estado)
                {
                    if (value == null || string.IsNullOrEmpty(value.ToString()))
                    {
                        return new ValidationResult("El campo " + validationContext.DisplayName + " es requerido");
                    }
                }

                return ValidationResult.Success;
            }
        }


        public class Validaclasificacion_vehiculo
        {
            [Display(Name = "nombre")]
            [Required(ErrorMessage = "El campo {0} es requerido")]
            public string clavh_nombre { get; set; }

            [Display(Name = "razon inactivo")]
            [Required(ErrorMessage = "El campo {0} es requerido")]
            public string clavhrazoninactivo { get; set; }
        }


        public class Validatipo_vehiculo
        {
            [Display(Name = "nombre")]
            [Required(ErrorMessage = "El campo {0} es requerido")]
            public string tpvh_nombre { get; set; }

            [Display(Name = "razon inactivo")]
            [ValidaTipoVehiculoInactivo]
            public string tpvhrazoninactivo { get; set; }
        }

        public class ValidaTipoVehiculoInactivo : ValidationAttribute
        {
            protected override ValidationResult IsValid(object value, ValidationContext validationContext)
            {
                tipo_vehiculo tipoVehiculo = (tipo_vehiculo)validationContext.ObjectInstance;

                if (!tipoVehiculo.tpvh_estado)
                {
                    if (value == null || string.IsNullOrEmpty(value.ToString()))
                    {
                        return new ValidationResult("El campo " + validationContext.DisplayName + " es requerido");
                    }
                }

                return ValidationResult.Success;
            }
        }


        public class Validaicb_terceros
        {
            [Display(Name = "Documento")]
            [Required(ErrorMessage = "El campo {0} es requerido")]
            public string doc_tercero { get; set; }

            [Display(Name = "Primer Nombre")]
            [Required(ErrorMessage = "El campo {0} es requerido")]
            public string prinom_tercero { get; set; }

            [Display(Name = "Primer Apellido")]
            [Required(ErrorMessage = "El campo {0} es requerido")]
            public string apellido_tercero { get; set; }

            [Display(Name = "Genero")]
            [Required(ErrorMessage = "El campo {0} es requerido")]
            public int? genero_tercero { get; set; }

            public string razon_social { get; set; }
        }

        public class Validatpservicio_vehiculo
        {
            [Display(Name = "nombre")]
            [Required(ErrorMessage = "El campo {0} es requerido")]
            public string tpserv_nombre { get; set; }

            [Display(Name = "razon inactivo")]
            [ValidaTipoServicioInactivo]
            public string tpservrazoninactivo { get; set; }
        }

        public class ValidaTipoServicioInactivo : ValidationAttribute
        {
            protected override ValidationResult IsValid(object value, ValidationContext validationContext)
            {
                tpservicio_vehiculo tipoServicio = (tpservicio_vehiculo)validationContext.ObjectInstance;

                if (!tipoServicio.tpserv_estado)
                {
                    if (value == null || string.IsNullOrEmpty(value.ToString()))
                    {
                        return new ValidationResult("El campo " + validationContext.DisplayName + " es requerido");
                    }
                }

                return ValidationResult.Success;
            }
        }


        public class ValidaperfilTributario
        {
            [Display(Name = "bodega")]
            [Required(ErrorMessage = "El campo {0} es requerido")]
            public string bodega { get; set; }

            [Display(Name = "sw")]
            [Required(ErrorMessage = "El campo {0} es requerido")]
            public int sw { get; set; }

            [Display(Name = "regimen")]
            [Required(ErrorMessage = "El campo {0} es requerido")]
            public int tipo_regimenid { get; set; }

            [Display(Name = "ret. fuente")]
            [Required(ErrorMessage = "El campo {0} es requerido")]
            public string retfuente { get; set; }

            [Display(Name = "ret. iva")]
            [Required(ErrorMessage = "El campo {0} es requerido")]
            public string retiva { get; set; }

            [Display(Name = "ret. ica")]
            [Required(ErrorMessage = "El campo {0} es requerido")]
            public string retica { get; set; }

            [Display(Name = "razon inactivo")]
            [Required(ErrorMessage = "El campo {0} es requerido")]
            public string razon_inactivo { get; set; }
        }


        public class Validaubicacion_vehiculo
        {
            [Display(Name = "nombre")]
            [Required(ErrorMessage = "El campo {0} es requerido")]
            public string ubivh_nombre { get; set; }

            [Display(Name = "razon inactivo")]
            [ValidaUbicacionVehiculoInactiva]
            public string ubivhrazoninactivo { get; set; }
        }


        public class ValidaUbicacionVehiculoInactiva : ValidationAttribute
        {
            protected override ValidationResult IsValid(object value, ValidationContext validationContext)
            {
                ubicacion_vehiculo ubicacion = (ubicacion_vehiculo)validationContext.ObjectInstance;

                if (!ubicacion.ubivh_estado)
                {
                    if (value == null || string.IsNullOrEmpty(value.ToString()))
                    {
                        return new ValidationResult("El campo " + validationContext.DisplayName + " es requerido");
                    }
                }

                return ValidationResult.Success;
            }
        }


        public class Validatpmotor_vehiculo
        {
            [Display(Name = "nombre")]
            [Required(ErrorMessage = "El campo {0} es requerido")]
            public string tpmot_nombre { get; set; }

            [Display(Name = "razon inactivo")]
            [ValidaTipoMotorInactivo]
            public string tpmotrazoninactivo { get; set; }
        }

        public class ValidaTipoMotorInactivo : ValidationAttribute
        {
            protected override ValidationResult IsValid(object value, ValidationContext validationContext)
            {
                tpmotor_vehiculo tipoMotor = (tpmotor_vehiculo)validationContext.ObjectInstance;

                if (!tipoMotor.tpmot_estado)
                {
                    if (value == null || string.IsNullOrEmpty(value.ToString()))
                    {
                        return new ValidationResult("El campo " + validationContext.DisplayName + " es requerido");
                    }
                }

                return ValidationResult.Success;
            }
        }


        public class Validatpcaja_vehiculo
        {
            [Display(Name = "nombre")]
            [Required(ErrorMessage = "El campo {0} es requerido")]
            public string tpcaj_nombre { get; set; }

            [Display(Name = "razon inactivo")]
            [ValidaTipoCajaInactiva]
            public string tpcajrazoninactivo { get; set; }
        }

        public class ValidaTipoCajaInactiva : ValidationAttribute
        {
            protected override ValidationResult IsValid(object value, ValidationContext validationContext)
            {
                tpcaja_vehiculo tipoCaja = (tpcaja_vehiculo)validationContext.ObjectInstance;

                if (!tipoCaja.tpcaj_estado)
                {
                    if (value == null || string.IsNullOrEmpty(value.ToString()))
                    {
                        return new ValidationResult("El campo " + validationContext.DisplayName + " es requerido");
                    }
                }

                return ValidationResult.Success;
            }
        }


        public class Validasegmento_vehiculo
        {
            [Display(Name = "nombre")]
            [Required(ErrorMessage = "El campo {0} es requerido")]
            public string segvh_nombre { get; set; }

            [Display(Name = "razon inactivo")]
            [ValidaSegmentoModelo]
            public string segvhrazoninactivo { get; set; }
        }

        public class ValidaSegmentoModelo : ValidationAttribute
        {
            protected override ValidationResult IsValid(object value, ValidationContext validationContext)
            {
                segmento_vehiculo segmento = (segmento_vehiculo)validationContext.ObjectInstance;

                if (!segmento.segvh_estado)
                {
                    if (value == null || string.IsNullOrEmpty(value.ToString()))
                    {
                        return new ValidationResult("El campo " + validationContext.DisplayName + " es requerido");
                    }
                }

                return ValidationResult.Success;
            }
        }


        public class Validavtipo
        {
            [Display(Name = "nombre")]
            [Required(ErrorMessage = "El campo {0} es requerido")]
            public string nombre { get; set; }

            [Display(Name = "razon inactivo")]
            [ValidaVTipoModelo]
            public string razoninactivo { get; set; }
        }

        public class ValidaVTipoModelo : ValidationAttribute
        {
            protected override ValidationResult IsValid(object value, ValidationContext validationContext)
            {
                vtipo tipo = (vtipo)validationContext.ObjectInstance;

                if (!tipo.estado)
                {
                    if (value == null || string.IsNullOrEmpty(value.ToString()))
                    {
                        return new ValidationResult("El campo " + validationContext.DisplayName + " es requerido");
                    }
                }

                return ValidationResult.Success;
            }
        }


        public class Validavperfil
        {
            [Display(Name = "nombre")]
            [Required(ErrorMessage = "El campo {0} es requerido")]
            public string nombre { get; set; }

            [Display(Name = "razon inactivo")]
            [Required(ErrorMessage = "El campo {0} es requerido")]
            public string razoninactivo { get; set; }
        }


        public class Validavgrupo
        {
            [Display(Name = "nombre")]
            [Required(ErrorMessage = "El campo {0} es requerido")]
            public string nombre { get; set; }

            [Display(Name = "razon inactivo")]
            [ValidaVGrupoModelo]
            public string razoninactivo { get; set; }
        }

        public class ValidaVGrupoModelo : ValidationAttribute
        {
            protected override ValidationResult IsValid(object value, ValidationContext validationContext)
            {
                vgrupo grupo = (vgrupo)validationContext.ObjectInstance;

                if (!grupo.estado)
                {
                    if (value == null || string.IsNullOrEmpty(value.ToString()))
                    {
                        return new ValidationResult("El campo " + validationContext.DisplayName + " es requerido");
                    }
                }

                return ValidationResult.Success;
            }
        }


        public class Validavclase
        {
            [Display(Name = "nombre")]
            [Required(ErrorMessage = "El campo {0} es requerido")]
            public string nombre { get; set; }

            [Display(Name = "razon inactivo")]
            [ValidaVClaseModelo]
            public string razoninactivo { get; set; }
        }

        public class ValidaVClaseModelo : ValidationAttribute
        {
            protected override ValidationResult IsValid(object value, ValidationContext validationContext)
            {
                vclase clase = (vclase)validationContext.ObjectInstance;

                if (!clase.estado)
                {
                    if (value == null || string.IsNullOrEmpty(value.ToString()))
                    {
                        return new ValidationResult("El campo " + validationContext.DisplayName + " es requerido");
                    }
                }

                return ValidationResult.Success;
            }
        }


        public class Validacolor_vehiculo
        {
            [Display(Name = "codigo")]
            [Required(ErrorMessage = "El campo {0} es requerido")]
            public string colvh_id { get; set; }

            [Display(Name = "nombre")]
            [Required(ErrorMessage = "El campo {0} es requerido")]
            public string colvh_nombre { get; set; }

            [Display(Name = "razon inactivo")]
            [ValidaInactivoColor]
            public string colvhrazoninactivo { get; set; }
        }

        public class ValidaInactivoColor : ValidationAttribute
        {
            protected override ValidationResult IsValid(object value, ValidationContext validationContext)
            {
                color_vehiculo color = (color_vehiculo)validationContext.ObjectInstance;

                if (!color.colvh_estado)
                {
                    if (value == null || string.IsNullOrEmpty(value.ToString()))
                    {
                        return new ValidationResult("El campo " + validationContext.DisplayName + " es requerido");
                    }
                }

                return ValidationResult.Success;
            }
        }


        public class Validamarca_vehiculo
        {
            [Display(Name = "nombre")]
            [Required(ErrorMessage = "El campo {0} es requerido")]
            public string marcvh_nombre { get; set; }

            [Display(Name = "razon inactivo")]
            [ValidaInactivoMarca]
            public string marcvhrazoninactivo { get; set; }
        }

        public class ValidaInactivoMarca : ValidationAttribute
        {
            protected override ValidationResult IsValid(object value, ValidationContext validationContext)
            {
                marca_vehiculo marca = (marca_vehiculo)validationContext.ObjectInstance;

                if (!marca.marcvh_estado)
                {
                    if (value == null || string.IsNullOrEmpty(value.ToString()))
                    {
                        return new ValidationResult("El campo " + validationContext.DisplayName + " es requerido");
                    }
                }

                return ValidationResult.Success;
            }
        }


        public class Validaubicacion_bodega
        {
            [Display(Name = "bodega")]
            [Required(ErrorMessage = "El campo {0} es requerido")]
            public string bodega { get; set; }

            [Display(Name = "ubicacion")]
            [Required(ErrorMessage = "El campo {0} es requerido")]
            public string ubicacion { get; set; }

            [Display(Name = "descripcion")]
            [Required(ErrorMessage = "El campo {0} es requerido")]
            public string descripcion { get; set; }

            [Display(Name = "razon inactivo")]
            [ValidaInactivoUbicacionBodega]
            public string razon_inactivo { get; set; }

            [Display(Name = "tipo")]
            [Required(ErrorMessage = "El campo {0} es requerido")]
            public int tipo { get; set; }
        }

        public class ValidaInactivoUbicacionBodega : ValidationAttribute
        {
            protected override ValidationResult IsValid(object value, ValidationContext validationContext)
            {
                ubicacion_bodega ubicacion_bodega = (ubicacion_bodega)validationContext.ObjectInstance;

                if (!ubicacion_bodega.estado)
                {
                    if (value == null || string.IsNullOrEmpty(value.ToString()))
                    {
                        return new ValidationResult("El campo " + validationContext.DisplayName + " es requerido");
                    }
                }

                return ValidationResult.Success;
            }
        }


        public class Validatercero_proveedor
        {
            public int tercero_id { get; set; }

            [Display(Name = "actividad economica")]
            [Required(ErrorMessage = "El campo {0} es requerido")]
            public int acteco_id { get; set; }

            [Display(Name = "tipo regimen")]
            [Required(ErrorMessage = "El campo {0} es requerido")]
            public int tpregimen_id { get; set; }

            [Display(Name = "tipo proveedor")]
            [Required(ErrorMessage = "El campo {0} es requerido")]
            public int? tipo_proveedor { get; set; }
        }


        public class Validaubicacion_repuesto
        {
            [Display(Name = "referencia")]
            [Required(ErrorMessage = "El campo {0} es requerido")]
            public string codigo { get; set; }

            [Display(Name = "bodega")]
            [Required(ErrorMessage = "El campo {0} es requerido")]
            public string bodega { get; set; }

            [Display(Name = "ubicacion")]
            [Required(ErrorMessage = "El campo {0} es requerido")]
            public string ubicacion { get; set; }
        }


        public class Validarols
        {
            [Display(Name = "nombre")]
            [Required(ErrorMessage = "El campo {0} es obligatorio")]
            public string rol_nombre { get; set; }

            [Display(Name = "razon inactivo")]
            [ValidaInactivoRol]
            public string rol_razoninactivo { get; set; }

            [Display(Name = "dias")]
            [Required(ErrorMessage = "El campo {0} es obligatorio")]
            public int? dias_expiracion_clave { get; set; }
        }

        public class ValidaInactivoRol : ValidationAttribute
        {
            protected override ValidationResult IsValid(object value, ValidationContext validationContext)
            {
                rols rol = (rols)validationContext.ObjectInstance;

                if (!rol.rol_estado)
                {
                    if (value == null || string.IsNullOrEmpty(value.ToString()))
                    {
                        return new ValidationResult("El campo " + validationContext.DisplayName + " es requerido");
                    }
                }

                return ValidationResult.Success;
            }
        }


        public class Validaicb_sysparameter
        {
            [Display(Name = "codigo")]
            [Required(ErrorMessage = "El campo {0} es obligatorio")]
            public string syspar_cod { get; set; }

            [Display(Name = "valor")]
            [Required(ErrorMessage = "El campo {0} es obligatorio")]
            public string syspar_value { get; set; }

            [Display(Name = "descripcion")]
            [Required(ErrorMessage = "El campo {0} es obligatorio")]
            public string syspar_description { get; set; }

            [Display(Name = "nombre")]
            [Required(ErrorMessage = "El campo {0} es obligatorio")]
            public string syspar_nombre { get; set; }

            [Display(Name = "razon inactivo")]
            [Required(ErrorMessage = "El campo {0} es obligatorio")]
            public string sysparrazoninactivo { get; set; }
        }


        public class Validamenus
        {
            [Display(Name = "nombre")]
            [Required(ErrorMessage = "El campo {0} es obligatorio")]
            public string nombreMenu { get; set; }
        }


        public class Validaacteco_tercero
        {
            [Display(Name = "numero")]
            [Required(ErrorMessage = "El campo {0} es requerido")]
            public string nroacteco_tercero { get; set; }

            [Display(Name = "nombre")]
            [Required(ErrorMessage = "El campo {0} es requerido")]
            public string acteco_nombre { get; set; }

            [Display(Name = "razon inactivo")]
            [Required(ErrorMessage = "El campo {0} es requerido")]
            public string acteco_razoninactivo { get; set; }
        }


        public class Validaicb_vehiculo
        {
            [Display(Name = "plan mayor")]
            [Required(ErrorMessage = "El campo {0} es obligatorio")]
            public string plan_mayor { get; set; }

            [Display(Name = "modelo")]
            [Required(ErrorMessage = "El campo {0} es obligatorio")]
            public string modvh_id { get; set; }

            [Display(Name = "color")]
            [Required(ErrorMessage = "El campo {0} es obligatorio")]
            public string colvh_id { get; set; }

            [Display(Name = "serie")]
            [Required(ErrorMessage = "El campo {0} es obligatorio")]
            public string vin { get; set; }

            [Display(Name = "numero motor")]
            [Required(ErrorMessage = "El campo {0} es obligatorio")]
            public string nummot_vh { get; set; }

            [Display(Name = "año")]
            [Required(ErrorMessage = "El campo {0} es obligatorio")]
            public int? anio_vh { get; set; }

            [Display(Name = "tipo")]
            [Required(ErrorMessage = "El campo {0} es obligatorio")]
            public int? tpvh_id { get; set; }

            [Display(Name = "marca")]
            [Required(ErrorMessage = "El campo {0} es obligatorio")]
            public int? marcvh_id { get; set; }
        }


        public class Validatp_doc_sw
        {
            [Display(Name = "sw")]
            [Required(ErrorMessage = "El campo {0} es requerido")]
            public int sw { get; set; }

            [Display(Name = "descripcion")]
            [Required(ErrorMessage = "El campo {0} es requerido")]
            public string Descripcion { get; set; }

            [Display(Name = "razon inactivo")]
            [Required(ErrorMessage = "El campo {0} es requerido")]
            public string razon_inactivo { get; set; }
        }


        public class Validatp_doc_registros
        {
            [Display(Name = "sw")]
            [Required(ErrorMessage = "El campo {0} es requerido")]
            public string sw { get; set; }

            [Display(Name = "nombre")]
            [Required(ErrorMessage = "El campo {0} es requerido")]
            public string tpdoc_nombre { get; set; }

            [Display(Name = "razon inactivo")]
            [Required(ErrorMessage = "El campo {0} es requerido")]
            public string tpdocrazoninactivo { get; set; }

            [Display(Name = "prefijo")]
            [Required(ErrorMessage = "El campo {0} es requerido")]
            public string prefijo { get; set; }

            [Display(Name = "tipo")]
            [Required(ErrorMessage = "El campo {0} es requerido")]
            public int? tipo { get; set; }
        }


        public class Validanom_departamento
        {
            [Display(Name = "pais")]
            [Required(ErrorMessage = "El campo {0} es obligatorio")]
            public int pais_id { get; set; }

            [Display(Name = "codigo")]
            [Required(ErrorMessage = "El campo {0} es obligatorio")]
            public string cod_dpto { get; set; }

            [Display(Name = "nombre")]
            [Required(ErrorMessage = "El campo {0} es obligatorio")]
            public string dpto_nombre { get; set; }

            [Display(Name = "razon inactivo")]
            [Required(ErrorMessage = "El campo {0} es obligatorio")]
            public string dpto_razoninactivo { get; set; }
        }


        public class Validanom_pais
        {
            [Display(Name = "codigo")]
            [Required(ErrorMessage = "El campo {0} es obligatorio")]
            public string cod_pais { get; set; }

            [Display(Name = "nombre")]
            [Required(ErrorMessage = "El campo {0} es obligatorio")]
            public string pais_nombre { get; set; }

            [Display(Name = "razon inactivo")]
            [Required(ErrorMessage = "El campo {0} es obligatorio")]
            public string pais_razoninactivo { get; set; }
        }


        public class Validanom_ciudad
        {
            [Display(Name = "departamento")]
            [Required(ErrorMessage = "El campo {0} es obligatorio")]
            public int dpto_id { get; set; }

            [Display(Name = "codigo")]
            [Required(ErrorMessage = "El campo {0} es obligatorio")]
            public string cod_ciudad { get; set; }

            [Display(Name = "nombre")]
            [Required(ErrorMessage = "El campo {0} es obligatorio")]
            public string ciu_nombre { get; set; }

            [Display(Name = "razon inactivo")]
            [Required(ErrorMessage = "El campo {0} es obligatorio")]
            public string ciu_razoninactivo { get; set; }
        }


        public class Validaarea_bodega
        {
            [Display(Name = "nombre")]
            [Required(ErrorMessage = "El campo {0} es requerido")]
            public string areabod_nombre { get; set; }

            [Display(Name = "razon inactivo")]
            [ValidaInactivoAreaBodega]
            public string areabodrazoninactivo { get; set; }
        }


        public class ValidaInactivoAreaBodega : ValidationAttribute
        {
            protected override ValidationResult IsValid(object value, ValidationContext validationContext)
            {
                area_bodega area = (area_bodega)validationContext.ObjectInstance;

                if (!area.areabod_estado)
                {
                    if (value == null || string.IsNullOrEmpty(value.ToString()))
                    {
                        return new ValidationResult("El campo " + validationContext.DisplayName + " es requerido");
                    }
                }

                return ValidationResult.Success;
            }
        }


        #region Perfil_contable_referencia

        public class Validaperfilcontable_referencia
        {
            [Display(Name = "descripcion")]
            [Required(ErrorMessage = "El campo {0} es requerido")]
            public string descripcion { get; set; }

            [Display(Name = "razon inactivo")]
            [ValidaInactivoPerfilContable]
            public string razon_inactivo { get; set; }
        }

        public class ValidaInactivoPerfilContable : ValidationAttribute
        {
            protected override ValidationResult IsValid(object value, ValidationContext validationContext)
            {
                perfilcontable_referencia perfil = (perfilcontable_referencia)validationContext.ObjectInstance;

                if (!perfil.estado)
                {
                    if (value == null || string.IsNullOrEmpty(value.ToString()))
                    {
                        return new ValidationResult("El campo " + validationContext.DisplayName + " es requerido");
                    }
                }

                return ValidationResult.Success;
            }
        }

        #endregion


        #region Ivc_doc_consecutivos

        public class Validaicb_doc_consecutivos
        {
            [Display(Name = "tipo documento")]
            [Required(ErrorMessage = "El campo {0} es requerido")]
            [ValidaTpDocDocumentoPorBodega]
            public int doccons_idtpdoc { get; set; }

            [Display(Name = "año")]
            [ValidaRequiereAnio]
            public int? doccons_ano { get; set; }

            [Display(Name = "mes")]
            [ValidaRequiereMes]
            public int? doccons_mes { get; set; }
        }

        public class ValidaTpDocDocumentoPorBodega : ValidationAttribute
        {
            protected override ValidationResult IsValid(object value, ValidationContext validationContext)
            {
                icb_doc_consecutivos doc = (icb_doc_consecutivos)validationContext.ObjectInstance;

                if (doc.doccons_idtpdoc == 0)
                {
                    return new ValidationResult("El campo " + validationContext.DisplayName + " es requerido");
                }

                return ValidationResult.Success;
            }
        }

        public class ValidaRequiereAnio : ValidationAttribute
        {
            protected override ValidationResult IsValid(object value, ValidationContext validationContext)
            {
                icb_doc_consecutivos doc = (icb_doc_consecutivos)validationContext.ObjectInstance;

                if (doc.doccons_requiere_anio)
                {
                    if (value == null || string.IsNullOrEmpty(value.ToString()))
                    {
                        return new ValidationResult("El campo " + validationContext.DisplayName + " es requerido");
                    }
                }

                return ValidationResult.Success;
            }
        }


        public class ValidaRequiereMes : ValidationAttribute
        {
            protected override ValidationResult IsValid(object value, ValidationContext validationContext)
            {
                icb_doc_consecutivos doc = (icb_doc_consecutivos)validationContext.ObjectInstance;

                if (doc.doccons_requiere_mes)
                {
                    if (value == null || string.IsNullOrEmpty(value.ToString()))
                    {
                        return new ValidationResult("El campo " + validationContext.DisplayName + " es requerido");
                    }
                }

                return ValidationResult.Success;
            }
        }

        #endregion

        #region CuentaPuc

        public class Validacuenta_puc
        {
            [Display(Name = "numero")]
            [Required(ErrorMessage = "El campo {0} es requerido")]
            public string cntpuc_numero { get; set; }

            [Display(Name = "descripcion")]
            [Required(ErrorMessage = "El campo {0} es requerido")]
            public string cntpuc_descp { get; set; }

            [Display(Name = "movimiento")]
            [ValidaMovimientoCuentaPuc(ErrorMessage = "El campo {0} es requerido")]
            public string mov_cnt { get; set; }

            [Display(Name = "concepto niff")]
            [ValidaConceptoNiffCuentaPuc(ErrorMessage = "El campo {0} es requerido")]
            public string concepniff { get; set; }

            [Display(Name = "razon inactivo")]
            [ValidaInactivoCuentaPuc(ErrorMessage = "El campo {0} es requerido")]
            public string cntpucrazoninactivo { get; set; }
        }

        public class ValidaInactivoCuentaPuc : ValidationAttribute
        {
            protected override ValidationResult IsValid(object value, ValidationContext validationContext)
            {
                cuenta_puc cuenta = (cuenta_puc)validationContext.ObjectInstance;

                if (!cuenta.cntpuc_estado)
                {
                    if (value == null || string.IsNullOrEmpty(value.ToString()))
                    {
                        return new ValidationResult("El campo " + validationContext.DisplayName + " es requerido");
                    }
                }

                return ValidationResult.Success;
            }
        }

        public class ValidaMovimientoCuentaPuc : ValidationAttribute
        {
            protected override ValidationResult IsValid(object value, ValidationContext validationContext)
            {
                cuenta_puc cuenta = (cuenta_puc)validationContext.ObjectInstance;

                if (cuenta.esafectable)
                {
                    if (value == null || string.IsNullOrEmpty(value.ToString()))
                    {
                        return new ValidationResult("El campo " + validationContext.DisplayName + " es requerido");
                    }
                }

                return ValidationResult.Success;
            }
        }

        public class ValidaConceptoNiffCuentaPuc : ValidationAttribute
        {
            protected override ValidationResult IsValid(object value, ValidationContext validationContext)
            {
                cuenta_puc cuenta = (cuenta_puc)validationContext.ObjectInstance;

                if (cuenta.esafectable)
                {
                    if (value == null || string.IsNullOrEmpty(value.ToString()))
                    {
                        return new ValidationResult("El campo " + validationContext.DisplayName + " es requerido");
                    }
                }

                return ValidationResult.Success;
            }
        }

        #endregion


        #region bodega_concesionario

        public class Validabodega_concesionario
        {
            [Display(Name = "codigo")]
            [Required(ErrorMessage = "El campo {0} es requerido")]
            public string bodccs_cod { get; set; }

            [Display(Name = "nombre")]
            [Required(ErrorMessage = "El campo {0} es requerido")]
            public string bodccs_nombre { get; set; }

            [Display(Name = "direccion")]
            [Required(ErrorMessage = "El campo {0} es requerido")]
            public string bodccs_direccion { get; set; }

            [Display(Name = "razon inactivo")]
            [ValidaInactivoBodega]
            public string bodccsrazoninactivo { get; set; }

            [Display(Name = "centro de costo")]
            [Required(ErrorMessage = "El campo {0} es requerido")]
            public int? bodccscentro_id { get; set; }

            [Display(Name = "pais")]
            [Required(ErrorMessage = "El campo {0} es requerido")]
            public int? pais_id { get; set; }

            [Display(Name = "ciudad")]
            [Required(ErrorMessage = "El campo {0} es requerido")]
            public int? ciudad_id { get; set; }

            [Display(Name = "departamento")]
            [Required(ErrorMessage = "El campo {0} es requerido")]
            public int? departamento_id { get; set; }

            [Display(Name = "codigo BAC")]
            [Required(ErrorMessage = "El campo {0} es requerido")]
            public string codigobac { get; set; }
        }


        public class ValidaInactivoBodega : ValidationAttribute
        {
            protected override ValidationResult IsValid(object value, ValidationContext validationContext)
            {
                bodega_concesionario bodega = (bodega_concesionario)validationContext.ObjectInstance;

                if (!bodega.bodccs_estado)
                {
                    if (value == null || string.IsNullOrEmpty(value.ToString()))
                    {
                        return new ValidationResult("El campo " + validationContext.DisplayName + " es requerido");
                    }
                }

                return ValidationResult.Success;
            }
        }

        #endregion
    }
}