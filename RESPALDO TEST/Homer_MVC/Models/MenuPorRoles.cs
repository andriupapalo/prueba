namespace Homer_MVC.ModeloVehiculos
{
    public class MenuPorRoles
    {
        public MenuPorRoles()
        {
            visible = false;
        }

        public int idMenu { get; set; }
        public string nombreMenu { get; set; }
        public int? padreId { get; set; }
        public string url { get; set; }
        public string icono { get; set; }
        public bool visible { get; set; }
    }
}