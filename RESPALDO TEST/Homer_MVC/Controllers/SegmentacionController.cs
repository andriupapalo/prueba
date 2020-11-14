using Homer_MVC.IcebergModel;
using Homer_MVC.Models;
using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;

namespace Homer_MVC.Controllers
{
    public class SegmentacionController : Controller
    {
        private readonly Iceberg_Context context = new Iceberg_Context();

        // GET: Segmentacion
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Create(int? menu)
        {
            if (Session["user_usuarioid"] != null)
            {
                SegmentacionClienteForm segVh = new SegmentacionClienteForm
                {
                    estado = true
                };

                listas(segVh);
                BuscarFavoritos(menu);
                return View(segVh);
            }

            return RedirectToAction("Login", "Login");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(SegmentacionClienteForm seg_vh, int? menu)
        {
            if (Session["user_usuarioid"] != null)
            {
                if (ModelState.IsValid)
                {
                    //consulta si el registro esta en BD
                    int nom = (from a in context.segmentacion
                               where a.descripcion == seg_vh.descripcion
                               select a.descripcion).Count();

                    if (nom == 0)
                    {
                        int bod = Convert.ToInt32(Session["user_bodega"].ToString());
                        int bodega = context.bodega_concesionario.Where(d => d.id == bod).Select(d => d.concesionarioid)
                            .FirstOrDefault();
                        segmentacion segmentacion = new segmentacion
                        {
                            cantidad_vehiculos = seg_vh.cantidad_vehiculos,
                            concesionario = bodega,
                            descripcion = seg_vh.descripcion,
                            estado = seg_vh.estado,
                            evalua_cantidad = seg_vh.evalua_cantidad,
                            evalua_polizas = seg_vh.evalua_polizas,
                            evalua_revisiones = seg_vh.evalua_revisiones,
                            evalua_soat = seg_vh.evalua_soat,
                            poliza_dia = seg_vh.poliza_dia,
                            razon_inactivo = seg_vh.razon_inactivo,
                            revisiones_dia = seg_vh.revisiones_dia,
                            soat_dia = seg_vh.soat_dia
                        };
                        segmentacion.fec_creacion = DateTime.Now;
                        segmentacion.userid_creacion = Convert.ToInt32(Session["user_usuarioid"]);
                        context.segmentacion.Add(segmentacion);
                        context.SaveChanges();
                        TempData["mensaje"] = "El registro del nuevo segmento de Clientes fue exitoso!";
                        return RedirectToAction("Create", new { menu });
                    }

                    TempData["mensaje_error"] = "El registro que ingreso ya se encuentra, por favor valide!";
                }

                TempData["mensaje_vacio"] = "Campos vacios, por favor valide!";

                IQueryable<icb_modulo_enlaces> enlacesBuscar = context.icb_modulo_enlaces.Where(x => x.enl_modulo == 28);
                string enlaces = "";
                foreach (icb_modulo_enlaces item in enlacesBuscar)
                {
                    Menus buscarEnlace = context.Menus.FirstOrDefault(x => x.idMenu == item.id_modulo_destino);
                    enlaces += "<li><a href='" + buscarEnlace.url + "'>" + buscarEnlace.nombreMenu + "</a></li>";
                }

                ViewBag.nombreEnlaces = enlaces;
                listas(seg_vh);
                BuscarFavoritos(menu);
                return View(seg_vh);
            }

            return RedirectToAction("Login", "Login");
        }


        public ActionResult Update(int? id, int? menu)
        {
            if (Session["user_usuarioid"] != null)
            {
                if (id != null)
                {
                    segmentacion segmentacion = context.segmentacion.Where(d => d.id == id).FirstOrDefault();
                    if (segmentacion != null)
                    {
                        SegmentacionClienteForm segVh = new SegmentacionClienteForm
                        {
                            id = segmentacion.id,
                            cantidad_vehiculos = segmentacion.cantidad_vehiculos,
                            concesionario = segmentacion.concesionario,
                            descripcion = segmentacion.descripcion,
                            estado = segmentacion.estado,
                            evalua_cantidad = segmentacion.evalua_cantidad,
                            evalua_polizas = segmentacion.evalua_polizas,
                            evalua_revisiones = segmentacion.evalua_revisiones,
                            evalua_soat = segmentacion.evalua_soat,
                            poliza_dia = segmentacion.poliza_dia,
                            razon_inactivo = segmentacion.razon_inactivo,
                            revisiones_dia = segmentacion.revisiones_dia,
                            soat_dia = segmentacion.soat_dia,
                            userid_creacion = segmentacion.userid_creacion,
                            fec_creacion = segmentacion.fec_creacion,
                            id_licencia = segmentacion.id_licencia,
                            fec_actualizacion = segmentacion.fec_actualizacion,
                            user_idactualizacion = segmentacion.user_idactualizacion
                        };

                        listas(segVh);
                        BuscarFavoritos(menu);
                        return View(segVh);
                    }

                    TempData["mensaje_error"] = "No se ha suministrado un id de segmentacion válido";
                    return RedirectToAction("Create", "Segmentacion", new { menu });
                }

                return RedirectToAction("Create", "Segmentacion", new { menu });
            }

            return RedirectToAction("Login", "Login");
        }


        [HttpPost]
        public ActionResult Update(SegmentacionClienteForm seg_vh, int? menu)
        {
            if (Session["user_usuarioid"] != null)
            {
                if (ModelState.IsValid)
                {
                    //consulta si el registro esta en BD para ese concesionario con otro nombre
                    int nom = (from a in context.segmentacion
                               where a.descripcion == seg_vh.descripcion && a.concesionario == seg_vh.concesionario &&
                                     a.id != seg_vh.id
                               select a.descripcion).Count();

                    if (nom == 0)
                    {
                        //actualizo para este
                        segmentacion segmentacion = context.segmentacion.Where(d => d.id == seg_vh.id).FirstOrDefault();

                        segmentacion.cantidad_vehiculos = seg_vh.cantidad_vehiculos;
                        segmentacion.concesionario = seg_vh.concesionario.Value;
                        segmentacion.descripcion = seg_vh.descripcion;
                        segmentacion.estado = seg_vh.estado;
                        segmentacion.evalua_cantidad = seg_vh.evalua_cantidad;
                        segmentacion.evalua_polizas = seg_vh.evalua_polizas;
                        segmentacion.evalua_revisiones = seg_vh.evalua_revisiones;
                        segmentacion.evalua_soat = seg_vh.evalua_soat;
                        segmentacion.poliza_dia = seg_vh.poliza_dia;
                        segmentacion.razon_inactivo = seg_vh.razon_inactivo;
                        segmentacion.revisiones_dia = seg_vh.revisiones_dia;
                        segmentacion.soat_dia = seg_vh.soat_dia;
                        segmentacion.fec_actualizacion = DateTime.Now;
                        segmentacion.user_idactualizacion = Convert.ToInt32(Session["user_usuarioid"]);
                        segmentacion.fec_creacion = DateTime.Now;
                        segmentacion.userid_creacion = Convert.ToInt32(Session["user_usuarioid"]);
                        context.Entry(segmentacion).State = EntityState.Modified;
                        context.SaveChanges();
                        TempData["mensaje"] = "La actualización del segmento de Clientes fue exitosa!";
                        // return RedirectToAction("Create", new { menu = menu });
                    }
                    else
                    {
                        TempData["mensaje_error"] =
                            "El nombre ingresado ya se encuentra asignado a otro segmento, por favor valide!";
                    }
                }

                TempData["mensaje_vacio"] = "Campos vacios, por favor valide!";

                IQueryable<icb_modulo_enlaces> enlacesBuscar = context.icb_modulo_enlaces.Where(x => x.enl_modulo == 28);
                string enlaces = "";
                foreach (icb_modulo_enlaces item in enlacesBuscar)
                {
                    Menus buscarEnlace = context.Menus.FirstOrDefault(x => x.idMenu == item.id_modulo_destino);
                    enlaces += "<li><a href='" + buscarEnlace.url + "'>" + buscarEnlace.nombreMenu + "</a></li>";
                }

                ViewBag.nombreEnlaces = enlaces;
                listas(seg_vh);
                BuscarFavoritos(menu);
                return View(seg_vh);
            }

            return RedirectToAction("Login", "Login");
        }


        public void listas(SegmentacionClienteForm segVh)
        {
            int bodega = 1;
            //busco el concesionario de la persona que está logueada
            int bod = Convert.ToInt32(Session["user_bodega"].ToString());
            bodega = context.bodega_concesionario.Where(d => d.id == bod).Select(d => d.concesionarioid)
                .FirstOrDefault();
            //if (Session["user_bodega"] != null)
            //{
            //    var bod = Convert.ToInt32(Session["user_bodega"].ToString());
            //    bodega = context.bodega_concesionario.Where(d => d.id == bod).Select(d => d.concesionarioid).FirstOrDefault();
            //    var concesionarios = context.concesionario.Where(d => d.estado == true && d.id==bodega).Select(d => new { id = d.id, nombre = d.nombre }).ToList();
            //    ViewBag.concesionario = new SelectList(concesionarios, "id", "nombre", segVh.concesionario);
            //}
            //else
            //{
            //    var concesionarios = context.concesionario.Where(d => d.estado == true).Select(d => new { id = d.id, nombre = d.nombre }).ToList();
            //    ViewBag.concesionario = new SelectList(concesionarios, "id", "nombre", segVh.concesionario);
            //}
            segVh.concesionario = bodega;
        }


        public void ConsultaDatosCreacion(segmento_vehiculo segmento_vh)
        {
            //consulta el nombre de usuario creacion con el id, lo envia a la vista a traves de ViewBag
            users creator = context.users.Find(segmento_vh.segvhuserid_creacion);
            ViewBag.user_nombre_cre = creator.user_nombre + " " + creator.user_apellido;

            users modificator = context.users.Find(segmento_vh.segvhuserid_actualizacion);
            if (modificator != null)
            {
                ViewBag.user_nombre_act = modificator.user_nombre + " " + modificator.user_apellido;
                ViewBag.user_fec_act = modificator.userfec_actualizacion.ToString();
            }
        }


        public JsonResult BuscarSegmentosPaginadas()
        {
            var data = context.segmentacion.ToList().Select(x => new
            {
                x.concesionario1.nombre,
                x.id,
                x.descripcion,
                segvh_estado = x.estado ? "Activo" : "Inactivo"
            });
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public void BuscarFavoritos(int? menu)
        {
            int usuarioActual = Convert.ToInt32(Session["user_usuarioid"]);

            var buscarFavoritosSeleccionados = (from favoritos in context.favoritos
                                                join menu2 in context.Menus
                                                    on favoritos.idmenu equals menu2.idMenu
                                                where favoritos.idusuario == usuarioActual && favoritos.seleccionado
                                                select new
                                                {
                                                    favoritos.seleccionado,
                                                    favoritos.cantidad,
                                                    menu2.idMenu,
                                                    menu2.nombreMenu,
                                                    menu2.url
                                                }).OrderByDescending(x => x.cantidad).ToList();

            bool esFavorito = false;

            foreach (var favoritosSeleccionados in buscarFavoritosSeleccionados)
            {
                if (favoritosSeleccionados.idMenu == menu)
                {
                    esFavorito = true;
                    break;
                }
            }

            if (esFavorito)
            {
                ViewBag.Favoritos =
                    "<div id='areaFavoritos'><i class='fa fa-close'></i>&nbsp;&nbsp;<a href='#' onclick='AgregarQuitarFavorito();return false;'>Quitar de Favoritos</a><div>";
            }
            else
            {
                ViewBag.Favoritos =
                    "<div id='areaFavoritos'><i class='fa fa-check'></i>&nbsp;&nbsp;<a href='#' onclick='AgregarQuitarFavorito();return false;'>Agregar a Favoritos</a></div>";
            }

            ViewBag.id_menu = menu != null ? menu : 0;
        }

        //public JsonResult ajustarSegmentacion()
        //{
        //    //busco los concesionarios disponibles

        //    var conc = context.concesionario.Where(d => d.estado == true).Select(d => new { id = d.id }).ToList();
        //    //por cada concesionario busco las segmentaciones activas disponibles
        //    {

        //    }
        //}
    }
}