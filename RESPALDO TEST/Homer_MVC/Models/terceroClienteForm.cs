using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using Homer_MVC.IcebergModel;
namespace Homer_MVC.Models
{
    public class terceroClienteForm
    {

        public int? cltercero_id { get; set; }
        public int? usuario_modi { get; set; }
        public int? tercliid_licencia { get; set; }
        public Nullable<System.DateTime> terclifec_creacion { get; set; }
        public Nullable<int> tercliuserid_creacion { get; set; }
        public Nullable<System.DateTime> terclifec_actualizacion { get; set; }
        public Nullable<int> tercliuserid_actualizacion { get; set; }
        public Nullable<int> numhijos_tercero { get; set; }
        [ValidaEdades]
        public string edades_hijos { get; set; }
        [Required]
        public int? tercero_id { get; set; }
        [Required]
        public int? tpocupacion_id { get; set; }
        [Required]
        public int? tphobby_id { get; set; }
        [Required]
        public int? tpdpte_id { get; set; }
        [Required]
        public int? edocivil_id { get; set; }       
        public int? idsegmentacion { get; set; }
       


        [ValidaContable]
        public bool exentoiva { get; set; }
        [ValidaContable]
        public string cupocredito { get; set; }
        public Nullable<int> dia_nofacturad { get; set; }
        public Nullable<int> dia_nofacturah { get; set; }
        public float dscto_rep { get; set; }
        public float dscto_mo { get; set; }
        [Required]
        public Nullable<int> tipo_cliente { get; set; }
        public Nullable<int> cod_pago_id { get; set; }
        
        public string telefono { get; set; }
        public string pagina_web { get; set; }
        public Nullable<int> base_retencion { get; set; }
        public bool retencion { get; set; }
        public bool contribucion { get; set; }
        public string lprecios_repuestos { get; set; }
        public Nullable<int> lprecios_vehiculos { get; set; }
        public bool bloqueado { get; set; }
        public string motivo_bloqueado { get; set; }
        public int tiempo_para_bloqueo { get; set; }
        [ValidaContable]
        public string fec_cupo_limite { get; set; }

        /*
        public Nullable<int> actividadEconomica_id { get; set; }
        public Nullable<int> tpregimen_id { get; set; }
        */

       
        public class ValidaEdades : ValidationAttribute
        {
            protected override ValidationResult IsValid(object value, ValidationContext validationContext)
            {
                terceroClienteForm registro = (terceroClienteForm)validationContext.ObjectInstance;

                if (registro.numhijos_tercero!=null && registro.numhijos_tercero>0)
                {
                    if (value == null || string.IsNullOrEmpty(value.ToString()))
                    {
                        return new ValidationResult("El campo " + validationContext.DisplayName + " es requerido");
                    }
                }
                return ValidationResult.Success;
            }
        }

        public class ValidaContable : ValidationAttribute
        {
            private readonly Iceberg_Context context2 = new Iceberg_Context();
            protected override ValidationResult IsValid(object value, ValidationContext validationContext)
            {
            terceroClienteForm registro = (terceroClienteForm)validationContext.ObjectInstance;

                if (registro.usuario_modi != null)
                {
                    //verifico si tiene permisos para modificar contable
                    int usuario = registro.usuario_modi.Value;
                    //Validamos que el usuario loguado tenga el rol y el permiso para modificar los valores del pedido
                    int permiso = (from u in context2.users
                                   join r in context2.rols
                                       on u.rol_id equals r.rol_id
                                   join ra in context2.rolacceso
                                       on r.rol_id equals ra.idrol
                                   where u.user_id == usuario /*&& u.rol_id == 4*/ && ra.idpermiso == 9
                                   select new
                                   {
                                       u.user_id,
                                       u.rol_id,
                                       r.rol_nombre,
                                       ra.idpermiso
                                   }).Count();
                    if (permiso > 0)
                    {
                        if (value == null || string.IsNullOrEmpty(value.ToString()))
                        {
                            return new ValidationResult("El campo " + validationContext.DisplayName + " es requerido");
                        }
                    }
                    
                }
                return ValidationResult.Success;
            }
        }
    }
}