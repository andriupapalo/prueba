using Homer_MVC.IcebergModel;
using System.Linq;
using System.Web.Mvc;

namespace Homer_MVC.Controllers
{
    public class iniciovariosbController : Controller
    {
        private readonly Iceberg_Context context = new Iceberg_Context();

        // GET: iniciovariosb

        public ActionResult Index()
        {
            inicioconsecutivo_variosb buscar = context.inicioconsecutivo_variosb.FirstOrDefault();
            int ocultar = 0; // false
            string ref_cod = "";
            string ceros = "";

            if (buscar != null)
            {
                ocultar = 1; //true  
                int largo = buscar.valor_inicial.ToString().Length;
                for (int i = largo; i < 6; i++)
                {
                    ceros += "0";
                }

                ref_cod = ceros + buscar.valor_inicial;
            }

            ViewBag.ocultar = ocultar;
            ViewBag.ref_cod = ref_cod;

            return View();
        }

        [HttpPost]
        public ActionResult Index(inicioconsecutivo_variosb nuevo)
        {
            inicioconsecutivo_variosb buscar = context.inicioconsecutivo_variosb.FirstOrDefault();

            if (buscar == null)
            {
                context.inicioconsecutivo_variosb.Add(nuevo);
                context.SaveChanges();
            }

            return View();
        }
    }
}