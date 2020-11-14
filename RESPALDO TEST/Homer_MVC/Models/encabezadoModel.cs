namespace Homer_MVC.Models
{
    public class encabezadoModel
    {
        public int? id { get; set; }
        public string encabezado { get; set; }
        public string tipoConteo { get; set; }
        public int? bodegas { get; set; }
        public int?[] areas { get; set; }
        public int?[] ubicaciones { get; set; }
        public int?[] estanterias { get; set; }
        public string fechaInicia { get; set; }
        public string fechaTermina { get; set; }


    }
}