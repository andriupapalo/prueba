using System.ComponentModel.DataAnnotations;

namespace Homer_MVC.IcebergModel
{
    public class modelo_terceroempledo
    {
        public int emp_tercero_id { get; set; }
        public string teremp_fec_creacion { get; set; }
        public int? teremp_userid_creacion { get; set; }
        public string teremp_fec_actualizacion { get; set; }
        public int? teremp_userid_actualizacion { get; set; }
        public bool teremp_estado { get; set; }
        public int? teremp_cargo { get; set; }
        public int? teremp_id_licencia { get; set; }
        public string razoninactivo_id { get; set; }
        [Required]
        public int? tercero_id { get; set; }
        public string fecha_contratacion { get; set; }
    }
}