using System;

namespace Homer_MVC.IcebergModel
{
    public class vdocumentosflotaform
    {
        //public int id { get; set; }
        public string documento { get; set; }
        public int? id_licencia { get; set; }
        public DateTime fec_creacion { get; set; }
        public int userid_creacion { get; set; }
        public DateTime? fec_actualizacion { get; set; }
        public int? user_idactualizacion { get; set; }
        public bool estado { get; set; }
        public string razon_inactivo { get; set; }
        public int id_tipo_documento { get; set; }
        public int iddocumento { get; set; }
        public bool estadotipo { get; set; }
    }
}