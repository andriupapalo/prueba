using System;

namespace Homer_MVC.IcebergModel
{
    public class Ubicacion_RepuestoModel
    {
        public int id { get; set; }
        public string id_estanteria { get; set; }
        public int area_bodega { get; set; }
        public string descripcion { get; set; }
        public int? ubirptoid_licencia { get; set; }
        public DateTime ubirptofec_creacion { get; set; }
        public int ubirptouserid_creacion { get; set; }
        public bool ubirpto_estado { get; set; }
        public int? ubirptouserid_actualizacion { get; set; }
        public DateTime? ubirptofec_actualizacion { get; set; }

        public string ubirptorazoninactivo { get; set; }
        //public string nombreArea { get; set; }
    }
}