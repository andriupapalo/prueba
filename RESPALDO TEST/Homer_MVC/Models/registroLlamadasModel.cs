using System;

namespace Homer_MVC.IcebergModel
{
    public class registroLlamadasModel
    {
        public int regllam_id { get; set; }
        public string regllam_verbalizacion { get; set; }
        public DateTime? regllam_prox_fecha { get; set; }
        public DateTime regllam_fecela { get; set; }
        public int tercero_id { get; set; }
        public int? cotizacion_id { get; set; }
        public int statusprospecto_id { get; set; }
        public int tpllamada_rescate_id { get; set; }
        public int? regllam_usuela { get; set; }
    }
}