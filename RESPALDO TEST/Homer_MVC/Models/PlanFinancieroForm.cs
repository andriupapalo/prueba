using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Homer_MVC.Models
{
    public class PlanFinancieroForm
    {
        public int plan_id { get; set; }
        public Nullable<int> plan_licencia { get; set; }
        public Nullable<System.DateTime> plan_fecela { get; set; }
        public Nullable<int> plan_usuela { get; set; }
        [Required]
        public string plan_descripcion { get; set; }
        [Required]
        public bool plan_estado { get; set; }
        [Required]
        public string plan_nombre { get; set; }
        public string plan_imagen { get; set; }
        public bool plan_comision { get; set; }
        [Required]
        public string plan_porcentaje_comision { get; set; }
        public Nullable<int> plan_usuario_actualizacion { get; set; }
        public Nullable<System.DateTime> plan_fecha_actualizacion { get; set; }
        public string plan_razon_inactivo { get; set; }
        [Required]
        public int? idfinanciera { get; set; }
        [Required]
        public string tasa_interes { get; set; }

    }
}