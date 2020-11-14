using System;

namespace Homer_MVC.IcebergModel
{
    public class recibidoTrasladosModel
    {
        public int id { get; set; }
        public int idtraslado { get; set; }
        public string refcodigo { get; set; }
        public int cantidad { get; set; }
        public bool recibido { get; set; }
        public DateTime? fecharecibido { get; set; }
        public int? userrecibido { get; set; }
        public DateTime fechatraslado { get; set; }
        public int usertraslado { get; set; }
        public decimal costo { get; set; }
        public string notas { get; set; }
        public int idorigen { get; set; }
        public int iddestino { get; set; }
    }
}