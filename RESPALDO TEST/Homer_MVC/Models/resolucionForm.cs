using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Homer_MVC.Models
{
    public class resolucionForm
    {
        public int? id { get; set; }
        [Required]
        public int? tipodoc { get; set; }
        [Required]
        public string resolucion { get; set; }
        [Required]
        public string fechaini { get; set; }
        [Required]
        public string fechafin { get; set; }
        [Required]
        public int consecini { get; set; }
        [Required]
        public int consecfin { get; set; }
        [Required]
        public int numfacturas { get; set; }
        public Nullable<int> id_licencia { get; set; }
        public System.DateTime fec_creacion { get; set; }
        public int userid_creacion { get; set; }
        public Nullable<System.DateTime> fec_actualizacion { get; set; }
        public Nullable<int> user_idactualizacion { get; set; }
        public bool estado { get; set; }
        public string razon_inactivo { get; set; }
        public int grupo { get; set; }
        [Required]
        public int diasaviso { get; set; }
        [Required]
        public int consecaviso { get; set; }
    }
}