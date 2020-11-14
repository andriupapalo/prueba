namespace Homer_MVC.Models
{
    public class notificacionLlegadaClienteModel
    {
        public int id { get; set; }
        public int cliente_id { get; set; }
        public int asesor_id { get; set; }
        public bool leido { get; set; }
        public string descripcion { get; set; }
        public string planmayor { get; set; }
    }
}