namespace Homer_MVC.Models
{
    public class activoMotivoBajasModelo
    {
        public int id { get; set; }
        public string descripcion { get; set; }
        public int id_licencia { get; set; }
        public string fec_creacion { get; set; }
        public int userid_creacion { get; set; }
        public string fec_actualizacion { get; set; }
        public int? user_idactualizacion { get; set; }
        public bool estado { get; set; }
        public string razon_inactivo { get; set; }
    }
}