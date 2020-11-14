using Homer_MVC.IcebergModel;
using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace Homer_MVC.Controllers
{
    public class medioComunicacionController : Controller
    {
        private readonly Iceberg_Context context = new Iceberg_Context();

        // GET: medioComunicacion/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: medioComunicacion/Create
        [HttpPost]
        public ActionResult Create(icb_medio_comunicacion medio, int? menu)
        {
            if (ModelState.IsValid)
            {
                //consulta si el registro esta en BD
                int nom = (from a in context.icb_medio_comunicacion
                           where a.medcomun_descripcion == medio.medcomun_descripcion
                           select a.medcomun_descripcion).Count();

                if (nom == 0)
                {
                    medio.medcomun_feccreacion = DateTime.Now;
                    medio.medcomun_usercreacion = Convert.ToInt32(Session["user_usuarioid"]);
                    context.icb_medio_comunicacion.Add(medio);
                    bool guardar = context.SaveChanges() > 0;
                    if (guardar)
                    {
                        TempData["mensaje"] = "El registro del nuevo rol fue exitoso!";
                        return RedirectToAction("Create", new { id = medio.medcomun_id, menu });
                    }

                    TempData["mensaje_error"] = "Error de conexion!";
                    return View(medio);
                }

                TempData["mensaje_error"] = "Ya se encuentra registrado un medio de comunicacion con el mismo nombre";
                return View(medio);
            }

            TempData["mensaje_error"] = "No fue posible guardar el registro, por favor valide!";
            return View(medio);
        }

        // GET: medioComunicacion/Edit/5
        public ActionResult update(int? id, int? menu)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            icb_medio_comunicacion medio = context.icb_medio_comunicacion.Find(id);
            if (medio == null)
            {
                return HttpNotFound();
            }
            //listas(medio);
            //BuscarFavoritos(menu);
            return View(medio);
        }

        // POST: medioComunicacion/Edit/5
        [HttpPost]
        public ActionResult update(icb_medio_comunicacion medio, int? menu)
        {
            if (ModelState.IsValid)
            {
                //consulta si el registro esta en BD teniendo en cuenta el id de consulta
                int nom = (from a in context.icb_medio_comunicacion
                           where a.medcomun_descripcion == medio.medcomun_descripcion || a.medcomun_id == medio.medcomun_id
                           select a.medcomun_descripcion).Count();

                if (nom == 1)
                {
                    medio.medcomun_fecactualizacion = DateTime.Now;
                    medio.medcomun_useractualizacion = Convert.ToInt32(Session["user_usuarioid"]);
                    context.Entry(medio).State = EntityState.Modified;

                    bool guardar = context.SaveChanges() > 0;
                    if (guardar)
                    {
                        TempData["mensaje"] = "La actualización del medio de comunicacion fue exitoso!";
                        return View(medio);
                    }

                    TempData["mensaje_error"] = "Error de conexion!";
                    return View(medio);
                }

                TempData["mensaje_error"] = "El registro que ingreso ya se encuentra, por favor valide!";
            }

            //var enlacesBuscar = context.icb_modulo_enlaces.Where(x => x.enl_modulo == 14);
            //string enlaces = "";
            //foreach (var item in enlacesBuscar)
            //{
            //    var buscarEnlace = context.Menus.FirstOrDefault(x => x.idMenu == item.id_modulo_destino);
            //    enlaces += "<li><a href='" + buscarEnlace.url + "'>" + buscarEnlace.nombreMenu + "</a></li>";
            //}
            //ViewBag.nombreEnlaces = enlaces;
            //ConsultaDatosCreacion(tp_hobby);
            //BuscarFavoritos(menu);
            return View(medio);
        }

        public JsonResult buscarMedios()
        {
            var data = context.icb_medio_comunicacion.ToList().Select(x => new
            {
                x.medcomun_id,
                x.medcomun_descripcion,
                estado = x.medcomun_estado ? "Activo" : "Inactivo"
            });

            return Json(data, JsonRequestBehavior.AllowGet);
        }
    }
}