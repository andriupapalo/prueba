using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace Homer_MVC.Models
{
    public class CampanaTallerModel
    {
        public int? id { get; set; }
        public System.DateTime? fecha_creacion { get; set; }
        [Required]
        public string nombre { get; set; }
        public string fecha_inicio { get; set; }
        public string fecha_fin { get; set; }
        public string Descripcion { get; set; }
        public int? id_licencia { get; set; }
        public int? userid_creacion { get; set; }
        public DateTime? fec_actualizacion { get; set; }
        public int? user_idactualizacion { get; set; }
        public bool estado { get; set; }
        public string razon_inactivo { get; set; }
        public string referencia { get; set; }
        public string numerogwm { get; set; }
        public string numerocircular { get; set; }

        }
}