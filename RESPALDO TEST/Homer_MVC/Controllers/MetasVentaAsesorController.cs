using Homer_MVC.IcebergModel;
using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;

namespace Homer_MVC.Controllers
{
    public class MetasVentaAsesorController : Controller
    {
        private readonly Iceberg_Context context = new Iceberg_Context();

        // GET: MetasVentaAsesor
        public ActionResult MetasVentaAsesor()
        {
            return View();
        }

        // POST: MetasVentaAsesor/Create
        [HttpPost]
        public ActionResult MetasVentaAsesor(metas_asesor post)
        {
            if (ModelState.IsValid)
            {
                string lista = Request["bodega"];
                if (!string.IsNullOrEmpty(lista))
                {
                    string[] bodegasId = lista.Split(',');
                    foreach (string substring in bodegasId)
                    {
                        int sub = Convert.ToInt32(substring);
                        metas_asesor buscarMeta2 =
                            context.metas_asesor.FirstOrDefault(x => x.bodega == sub && x.meta == post.meta);
                        if (buscarMeta2 == null)
                        {
                            context.metas_asesor.Add(new metas_asesor
                            {
                                fec_creacion = DateTime.Now,
                                userid_creacion = Convert.ToInt32(Session["user_usuarioid"]),
                                bodega = Convert.ToInt32(substring),
                                meta = Convert.ToInt32(Request["meta"]),
                                valor = Convert.ToInt32(Request["valor"])
                            });
                            int guardarMeta = context.SaveChanges();

                            var nombre = (from c in context.metas_asesor
                                          join b in context.bodega_concesionario
                                              on c.bodega equals b.id
                                          join a in context.item_metas
                                              on c.meta equals a.id
                                          where sub == c.bodega
                                          select new
                                          {
                                              b.bodccs_nombre,
                                              a.descripcion
                                          }).FirstOrDefault();

                            TempData["mensaje"] = "El registro de " + nombre.descripcion + " en la bodega " +
                                                  nombre.bodccs_nombre + " metas fue exitoso!";
                        }
                        else
                        {
                            var nombre = (from c in context.metas_asesor
                                          join b in context.bodega_concesionario
                                              on c.bodega equals b.id
                                          join a in context.item_metas
                                              on c.meta equals a.id
                                          where sub == c.bodega
                                          select new
                                          {
                                              b.bodccs_nombre,
                                              a.descripcion
                                          }).FirstOrDefault();
                            TempData["mensaje_error"] = "La meta " + nombre.descripcion + " en la bodega " +
                                                        nombre.bodccs_nombre + " ya tiene un valor, por favor valide!";
                        }
                    }
                }
            }

            return View();
        }

        // GET: MetasVentaAsesor/Edit/5
        public ActionResult Editar(int id)
        {
            metas_asesor metas = context.metas_asesor.Find(id);
            var buscarDatos = (from a in context.metas_asesor
                               join b in context.bodega_concesionario
                                   on a.bodega equals b.id
                               join c in context.item_metas
                                   on a.meta equals c.id
                               where a.id == id
                               select new
                               {
                                   a.id,
                                   a.bodega,
                                   a.meta,
                                   b.bodccs_nombre,
                                   c.descripcion,
                                   a.valor
                               }
                ).FirstOrDefault();

            ViewBag.bodega = buscarDatos.bodccs_nombre;
            ViewBag.bodega_id = buscarDatos.bodega;
            ViewBag.meta = buscarDatos.descripcion;
            ViewBag.meta_id = buscarDatos.meta;
            ViewBag.valor = buscarDatos.valor;

            return View(metas);
        }

        // POST: MetasVentaAsesor/Edit/5
        [HttpPost]
        public ActionResult Editar(int id, metas_asesor post)
        {
            post.fec_actualizacion = DateTime.Now;
            post.user_idactualizacion = Convert.ToInt32(Session["user_usuarioid"]);
            decimal x = Convert.ToDecimal(Request["valor"]);
            post.valor = Convert.ToInt32(x);
            post.estado = true;
            context.Entry(post).State = EntityState.Modified;
            context.SaveChanges();

            var buscarDatos = (from a in context.metas_asesor
                               join b in context.bodega_concesionario
                                   on a.bodega equals b.id
                               join c in context.item_metas
                                   on a.meta equals c.id
                               where a.id == id
                               select new
                               {
                                   a.id,
                                   a.bodega,
                                   a.meta,
                                   b.bodccs_nombre,
                                   c.descripcion,
                                   a.valor
                               }
                ).FirstOrDefault();

            ViewBag.bodega = buscarDatos.bodccs_nombre;
            ViewBag.bodega_id = buscarDatos.bodega;
            ViewBag.meta = buscarDatos.descripcion;
            ViewBag.meta_id = buscarDatos.meta;
            ViewBag.valor = buscarDatos.valor;
            return View();
        }

        public JsonResult BuscarMetasPaginadas()
        {
            var buscarMetas = (from a in context.metas_asesor
                               join b in context.bodega_concesionario
                                   on a.bodega equals b.id
                               join c in context.item_metas
                                   on a.meta equals c.id
                               select new
                               {
                                   a.id,
                                   b.bodccs_nombre,
                                   c.descripcion,
                                   a.valor
                               }).ToList();

            return Json(buscarMetas, JsonRequestBehavior.AllowGet);
        }

        public JsonResult CargarMetas()
        {
            var metas = (from a in context.item_metas select new { a.id, a.descripcion }).ToList();


            return Json(metas, JsonRequestBehavior.AllowGet);
        }
    }
}