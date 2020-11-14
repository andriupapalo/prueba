using Homer_MVC.IcebergModel;
using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace Homer_MVC.Controllers
{
    public class ubicacion_repuestoController : Controller
    {
        private readonly Iceberg_Context context = new Iceberg_Context();


        public void listas(ubicacion_repuesto modelo)
        {
            ViewBag.bodega =
                new SelectList(context.bodega_concesionario.Where(x => x.bodccs_estado).OrderBy(x => x.bodccs_nombre),
                    "id", "bodccs_nombre", modelo.bodega);
            var referencias = (from referencia in context.icb_referencia
                               where referencia.modulo == "R" && referencia.ref_estado
                               select new
                               {
                                   referencia.ref_codigo,
                                   ref_descripcion = "(" + referencia.ref_codigo + ") " + referencia.ref_descripcion
                               }).OrderBy(x => x.ref_descripcion).ToList();
            ViewBag.idarea =
                new SelectList(context.area_bodega.Where(x => x.areabod_estado).OrderBy(x => x.areabod_nombre),
                    "areabod_id", "areabod_nombre", modelo.idarea);
            ViewBag.id_estanteria = new SelectList(context.estanterias.Where(x => x.estado).OrderBy(x => x.descripcion),
                "id", "descripcion", modelo.id_estanteria);
            //ViewBag.codigo = new SelectList(referencias, "ref_codigo", "ref_descripcion", modelo.codigo);
            if (!string.IsNullOrWhiteSpace(modelo.codigo))
            {
                //veo cual es el codigo
                icb_referencia codigodes = context.icb_referencia.Where(d => d.ref_codigo == modelo.codigo).FirstOrDefault();
                if (codigodes != null)
                {
                    ViewBag.codigo = codigodes.ref_codigo + " | " + codigodes.ref_descripcion;
                    modelo.codigo = codigodes.ref_codigo + " | " + codigodes.ref_descripcion;
                }
            }
            else
            {
                ViewBag.codigo = "";
            }

            ViewBag.ubicacion =
                new SelectList(context.ubicacion_repuestobod.Where(x => x.ubirpto_estado).OrderBy(x => x.descripcion),
                    "id", "descripcion", modelo.ubicacion);
        }

        public ActionResult Create(int? menu, string id, string cadena = "")
        {
            string idrepuesto = "";

            listas(new ubicacion_repuesto());
            //hallamos el nombre de la referencia
            if (!string.IsNullOrWhiteSpace(id))
            {
                icb_referencia refer = context.icb_referencia.Where(d => d.ref_codigo == id).FirstOrDefault();
                if (refer != null)
                {
                    //var codigo2 = codigo.Split('|');
                    idrepuesto = id + " | " + refer.ref_descripcion;
                }
            }

            ViewBag.refer = idrepuesto;


            BuscarFavoritos(menu);

            if (cadena != "")
            {
                ViewBag.cadena = cadena;
                var Subcadena = cadena.Split(',');
                string dataBogeda = Subcadena[0];
                string idreferencia = Subcadena[1];


                ViewBag.Id_bodega_origen = dataBogeda;
                ViewBag.CodReferencia = idreferencia;

                return View();
            }

            return View();
        }

        // POST: ubi_rpto/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(ubicacion_repuesto ubi_rpto, int? menu)
        {
            //consulta si el registro esta en BD
            int nom = (from a in context.ubicacion_repuesto
                       where a.codigo == ubi_rpto.codigo
                             && a.bodega == ubi_rpto.bodega
                       select a.ubicacion).Count();

            string idrepuesto = "";
            string codigo = ubi_rpto.codigo;
            if (codigo.Contains("|"))
            {
                string[] codigo2 = codigo.Split('|');
                idrepuesto = codigo2[0];
            }

            if (!string.IsNullOrWhiteSpace(idrepuesto))
            {
                int buscarIguales = context.ubicacion_repuesto.Where(x =>
                    x.codigo == idrepuesto && x.bodega == ubi_rpto.bodega
                                           && x.ubicacion == ubi_rpto.ubicacion && x.idarea == ubi_rpto.idarea).Count();

                if (buscarIguales == 0)
                {
                    ubi_rpto.codigo = idrepuesto;
                    ubi_rpto.bodega = ubi_rpto.bodega;
                    ubi_rpto.ubicacion = ubi_rpto.ubicacion;
                    ubi_rpto.idarea = ubi_rpto.idarea;
                    ubi_rpto.id_estanteria = ubi_rpto.id_estanteria;
                    ubi_rpto.fec_creacion = DateTime.Now;
                    ubi_rpto.userid_creacion = Convert.ToInt32(Session["user_usuarioid"]);
                    ubi_rpto.notaUbicacion = ubi_rpto.notaUbicacion;
                    context.ubicacion_repuesto.Add(ubi_rpto);
                    int guardar = context.SaveChanges();
                    if (guardar != 0)
                    {
                        TempData["mensaje"] = "El registro de la nueva ubicacion de repuestos fue exitoso!";
                        return RedirectToAction("Create");
                    }
                }
                else
                {
                    TempData["mensaje_error"] = "El registro que ingreso ya se encuentra, por favor valide!";
                }
            }
            else
            {
                TempData["mensaje_error"] =
                    "El registro que ingreso no corresponde con un código existente, por favor valide!";
            }


            listas(ubi_rpto);
            BuscarFavoritos(menu);
            return View(ubi_rpto);
        }

        // GET: ubi_rpto/Edit/5
        public ActionResult update(int? id, int? menu)
        {
            //valida si el id es null
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            ubicacion_repuesto ubi_rpto = context.ubicacion_repuesto.FirstOrDefault(x => x.id == id);
            if (ubi_rpto == null)
            {
                return HttpNotFound();
            }
            //consulta el nombre de usuario creacion con el id, lo envia a la vista a traves de ViewBag
            users creator = context.users.Find(ubi_rpto.userid_creacion);
            ViewBag.user_nombre_cre = creator.user_nombre + " " + creator.user_apellido;

            //consulta el nombre de usuario actualizacion con el id, lo envia a la vista a traves de ViewBag
            users modificator = context.users.Find(ubi_rpto.user_idactualizacion);
            if (modificator != null)
            {
                ViewBag.user_nombre_act = modificator.user_nombre + " " + modificator.user_apellido;
            }

            listas(ubi_rpto);
            BuscarFavoritos(menu);
            return View(ubi_rpto);
        }

        // POST: ubi_rpto/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult update(ubicacion_repuesto ubi_rpto, int? menu)
        {
            if (ModelState.IsValid)
            {
                string idrepuesto = "";
                string codigo = ubi_rpto.codigo;
                if (codigo.Contains("|"))
                {
                    string[] codigo2 = codigo.Split('|');
                    idrepuesto = codigo2[0];
                }

                ;
                int buscarIguales = context.ubicacion_repuesto.Where(x =>
                        x.codigo == idrepuesto && x.bodega == ubi_rpto.bodega && x.ubicacion == ubi_rpto.ubicacion &&
                        x.idarea == ubi_rpto.idarea)
                    .Count();
                //consulta si el registro esta en BD teniendo en cuenta el id de consulta
                int nom = (from a in context.ubicacion_repuesto
                           where a.codigo == ubi_rpto.codigo
                                 && a.bodega == ubi_rpto.bodega
                           select a.ubicacion).Count();

                if (buscarIguales == 0)
                {
                    ubi_rpto.codigo = idrepuesto;
                    ubi_rpto.fec_actualizacion = DateTime.Now;
                    ubi_rpto.user_idactualizacion = Convert.ToInt32(Session["user_usuarioid"]);
                    ubi_rpto.notaUbicacion = ubi_rpto.notaUbicacion;
                    context.Entry(ubi_rpto).State = EntityState.Modified;
                    context.SaveChanges();
                    ConsultaDatosCreacion(ubi_rpto);
                    TempData["mensaje"] = "La actualización de la ubicacion fue exitosa!";
                }
                else
                {
                    TempData["mensaje_error"] = "El registro que ingreso ya se encuentra, por favor valide!";
                }
            }

            ConsultaDatosCreacion(ubi_rpto);
            TempData["mensaje_vacio"] = "Campos vacios, por favor valide!";
            listas(ubi_rpto);
            BuscarFavoritos(menu);
            return View(ubi_rpto);
        }

        public void ConsultaDatosCreacion(ubicacion_repuesto ubi_rpto)
        {
            //consulta el nombre de usuario creacion con el id, lo envia a la vista a traves de ViewBag
            users creator = context.users.Find(ubi_rpto.userid_creacion);
            ViewBag.user_nombre_cre = creator.user_nombre + " " + creator.user_apellido;

            users modificator = context.users.Find(ubi_rpto.user_idactualizacion);
            if (modificator != null)
            {
                ViewBag.user_nombre_act = modificator.user_nombre + " " + modificator.user_apellido;
                ViewBag.user_fec_act = modificator.userfec_actualizacion.ToString();
            }
        }

        public JsonResult BuscarUbiRptoPaginadas()
        {
            var data = (from ubicaciones in context.ubicacion_repuesto
                        join bodega in context.bodega_concesionario
                            on ubicaciones.bodega equals bodega.id
                        join ubiBod in context.ubicacion_repuestobod
                            on ubicaciones.ubicacion equals ubiBod.id
                        join referencia in context.icb_referencia
                            on ubicaciones.codigo equals referencia.ref_codigo
                        join area in context.area_bodega
                            on ubicaciones.idarea equals area.areabod_id into zz
                        from area in zz.DefaultIfEmpty()
                        select new
                        {
                            ubicaciones.id,
                            bodega.bodccs_nombre,
                            referencia.ref_codigo,
                            referencia.ref_descripcion,
                            ubiBod.descripcion,
                            area = area.areabod_nombre != null ? area.areabod_nombre : "",
                            nota=ubicaciones.notaUbicacion != null ? ubicaciones.notaUbicacion : "",
                        }).ToList();
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public JsonResult cargarUbicacionEditar(int? idArea, string idRepuesto)
        {
            var buscar2 = (from a in context.ubicacion_repuestobod
                               //join b in context.area_bodega
                               //on a.idarea equals b.areabod_id
                           where a.ubirpto_estado /*&& a.idarea == idArea*/
                           select new
                           {
                               a.id,
                               a.descripcion
                           }).OrderBy(x => x.descripcion).ToList();
            int buscarRefer = context.ubicacion_repuesto.Where(x => x.codigo == idRepuesto).Select(x => x.ubicacion)
                .FirstOrDefault();

            var datos = buscar2.Select(x => new
            {
                x.id,
                x.descripcion
            }).ToList();

            var data = new
            {
                buscarRefer,
                datos
            };

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public JsonResult cargarUbicacion(int? idArea)
        {
            var buscar2 = (from a in context.ubicacion_repuestobod
                               //join b in context.area_bodega
                               //on a.idarea equals b.areabod_id
                           where a.ubirpto_estado /*&& a.idarea == idArea*/
                           select new
                           {
                               a.id,
                               a.descripcion
                           }).OrderBy(x => x.descripcion).ToList();

            var datos = buscar2.Select(x => new
            {
                x.id,
                x.descripcion
            }).ToList();

            return Json(datos, JsonRequestBehavior.AllowGet);
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
    }
}