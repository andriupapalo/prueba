using Homer_MVC.IcebergModel;
using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;

namespace Homer_MVC.Controllers
{
    public class estanteriasController : Controller
    {
        private readonly Iceberg_Context context = new Iceberg_Context();


        // GET: estanterias
        public ActionResult Create()
        {
            int bodega = Convert.ToInt32(Session["user_bodega"]);
            //ViewBag.bodegas = new SelectList(context.bodega_concesionario.Where(x => x.bodccs_estado && x.id == bodega).OrderBy(x => x.bodccs_nombre), "id", "bodccs_nombre");
            ViewBag.bodegas = new SelectList(context.bodega_concesionario.OrderBy(x => x.bodccs_nombre), "id", "bodccs_nombre");
            return View();
        }

        [HttpPost]
        public ActionResult Create(estanterias nuevo)
        {
            int buscar = context.estanterias
                .Where(x => x.descripcion == nuevo.descripcion && x.id_area == nuevo.id_area).Count();

            if (buscar == 0)
            {
                nuevo.fec_creacion = DateTime.Now;
                nuevo.user_creacion = Convert.ToInt32(Session["user_usuarioid"]);
                //nuevo.fec_actualizacion = null;
                context.estanterias.Add(nuevo);
                context.SaveChanges();
                TempData["mensaje"] = "El registro de la estantería fue exitoso!";
            }
            else
            {
                TempData["mensaje_error"] = "El registro que ingreso ya se encuentra, por favor valide!";
            }

            int bodega = Convert.ToInt32(Session["user_bodega"]);
            ViewBag.bodegas =
                new SelectList(
                    context.bodega_concesionario.Where(x => x.bodccs_estado && x.id == bodega)
                        .OrderBy(x => x.bodccs_nombre), "id", "bodccs_nombre");
            return View(nuevo);
        }


        public ActionResult edit(int? id)
        {
            var buscar = (from a in context.estanterias
                          join b in context.area_bodega
                              on a.id_area equals b.areabod_id
                          join c in context.bodega_concesionario
                              on b.id_bodega equals c.id
                          where a.id == id
                          select new
                          {
                              estanteria = a.descripcion,
                              a.id_area,
                              b.id_bodega,
                              area = b.areabod_nombre,
                              bodega = c.bodccs_nombre,
                              a.estado,
                              a.razon_inactivo,
                              a.id
                          }).First();

            estanterias editar = new estanterias
            {
                estado = buscar.estado,
                id_area = buscar.id_area,
                descripcion = buscar.estanteria,
                razon_inactivo = buscar.razon_inactivo
            };
            int bodega = Convert.ToInt32(Session["user_bodega"]);
            System.Collections.Generic.List<bodega_concesionario> bodegas = context.bodega_concesionario.Where(x => x.bodccs_estado && x.id == bodega)
                .OrderBy(x => x.bodccs_nombre).ToList();
            ViewBag.bodegas = new SelectList(bodegas, "id", "bodccs_nombre", buscar.id_bodega);
            //ViewBag.bod_selec = buscar.id_bodega;
            ViewBag.id_area =
                new SelectList(context.area_bodega.Where(x => x.areabod_estado).OrderBy(x => x.areabod_nombre),
                    "areabod_id", "areabod_nombre", buscar.id_area);

            return View(editar);
        }

        [HttpPost]
        public ActionResult edit(estanterias editar)
        {
            estanterias buscar = context.estanterias.Find(editar.id);

            if (buscar != null)
            {
                buscar.descripcion = editar.descripcion;
                buscar.id_area = editar.id_area;
                buscar.razon_inactivo = editar.razon_inactivo;
                buscar.estado = editar.estado;
                buscar.fec_actualizacion = DateTime.Now;
                buscar.user_actualizacion = Convert.ToInt32(Session["user_usuarioid"]);
                context.Entry(buscar).State = EntityState.Modified;
                context.SaveChanges();
            }

            int bodega = Convert.ToInt32(Session["user_bodega"]);
            System.Collections.Generic.List<bodega_concesionario> bodegas = context.bodega_concesionario.Where(x => x.bodccs_estado && x.id == bodega)
                .OrderBy(x => x.bodccs_nombre).ToList();
            ViewBag.bodegas = new SelectList(bodegas, "id", "bodccs_nombre");
            ViewBag.id_area =
                new SelectList(context.area_bodega.Where(x => x.areabod_estado).OrderBy(x => x.areabod_nombre),
                    "areabod_id", "areabod_nombre", editar.id_area);

            return View(editar);
        }

        public JsonResult buscarAreas(int? id_bodega)
        {
            var buscar = context.area_bodega.Where(x => x.areabod_estado && x.id_bodega == id_bodega).Select(x => new
            {
                x.areabod_nombre,
                x.areabod_id
            }).ToList();

            return Json(buscar, JsonRequestBehavior.AllowGet);
        }

        public JsonResult buscarEstanterias(int? id_area)
        {
            var buscar = context.estanterias.Where(x => x.estado && x.id_area == id_area).Select(x => new
            {
                x.descripcion,
                x.id
            }).ToList();

            return Json(buscar, JsonRequestBehavior.AllowGet);
        }

        public JsonResult buscarUbicaciones(int? id_estanteria)
        {
            var buscar = context.ubicacion_repuestobod.Where(x => x.ubirpto_estado && x.id_estanteria == id_estanteria)
                .Select(x => new
                {
                    x.descripcion,
                    x.id
                }).ToList();

            return Json(buscar, JsonRequestBehavior.AllowGet);
        }

        public JsonResult browser()
        {
            int bod = Convert.ToInt32(Session["user_bodega"]);
            var buscar = (from a in context.estanterias
                          join b in context.area_bodega
                              on a.id_area equals b.areabod_id
                          join c in context.bodega_concesionario
                              on b.id_bodega equals c.id
                          where c.id == bod
                          select new
                          {
                              estanteria = a.descripcion,
                              area = b.areabod_nombre,
                              bodega = c.bodccs_nombre,
                              estado = a.estado ? "Activo" : "Incativo",
                              a.id
                          }).ToList();

            return Json(buscar, JsonRequestBehavior.AllowGet);
        }
    }
}