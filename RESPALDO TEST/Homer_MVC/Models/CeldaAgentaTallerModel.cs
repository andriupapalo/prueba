namespace Homer_MVC.Models
{
    public class CeldaAgentaTallerModel
    {
        public int IdBahia { get; set; }
        public string NombreCelda { get; set; }
        public string HoraInicio { get; set; }
        public string HoraFin { get; set; }
        public bool CeldaInactiva { get; set; }
        public string ColorCelda { get; set; }
        public int? IdCita { get; set; }
        public int? valRowSpan { get; set; }
        public bool hora { get; set; }

    }
}