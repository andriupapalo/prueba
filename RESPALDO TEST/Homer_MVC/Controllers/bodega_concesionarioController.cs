using Homer_MVC.IcebergModel;
using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace Homer_MVC.Controllers
{
    public class bodega_concesionarioController : Controller
    {
        private readonly Iceberg_Context context = new Iceberg_Context();

        public ActionResult Create(int? menu)
        {
            ViewBag.areas =
                new SelectList(context.area_bodega.Where(x => x.areabod_estado).OrderBy(x => x.areabod_nombre),
                    "areabod_id", "areabod_nombre");
            ViewBag.bodccscentro_id = new SelectList(context.centro_costo.OrderBy(x => x.centcst_nombre), "centcst_id",
                "centcst_nombre");
            ViewBag.pais_id = new SelectList(context.nom_pais.OrderBy(x => x.pais_nombre), "pais_id", "pais_nombre");
            ViewBag.departamento_id = new SelectList(context.nom_departamento.Take(0), "dpto_id", "dpto_nombre");
            ViewBag.ciudad_id = new SelectList(context.nom_ciudad.Take(0), "ciu_id", "ciu_nombre");
            IQueryable<icb_modulo_enlaces> enlacesBuscar = context.icb_modulo_enlaces.Where(x => x.enl_modulo == 63);
            string enlaces = "";
            foreach (icb_modulo_enlaces item in enlacesBuscar)
            {
                Menus buscarEnlace = context.Menus.FirstOrDefault(x => x.idMenu == item.id_modulo_destino);
                enlaces += "<li><a href='" + buscarEnlace.url + "'>" + buscarEnlace.nombreMenu + "</a></li>";
            }

            ViewBag.nombreEnlaces = enlaces;
            bodega_concesionario crearBodega = new bodega_concesionario { bodccs_estado = true };
            BuscarFavoritos(menu);
            return View(crearBodega);
        }


        [HttpPost]
        public ActionResult Create(bodega_concesionario modelo, int? menu)
        {
            if (ModelState.IsValid)
            {
                int result = 0;
                bodega_concesionario bodegaNombre = context.bodega_concesionario.FirstOrDefault(x =>
                    x.bodccs_nombre == modelo.bodccs_nombre || x.bodccs_cod == modelo.bodccs_cod);
                if (bodegaNombre != null)
                {
                    result = -1;
                    TempData["mensaje_error"] = "El nombre o el codigo de la bodega ya se encuentra registrado";
                }
                else
                {
                    modelo.codigobac = modelo.codigobac;
                    modelo.bodccsfec_creacion = DateTime.Now;
                    modelo.bodccsuserid_creacion = Convert.ToInt32(Session["user_usuarioid"]);
                    bodega_concesionario AddBodega = context.bodega_concesionario.Add(modelo);

                    /*aca ponemos el codigo para agregar la bodega de una vez en la tabla mesactualbodega*/

                    context.mesactualbodega.Add(new mesactualbodega
                    {
                        idbodega = modelo.id,
                        anoactual = DateTime.Now.Year,
                        mesacual = DateTime.Now.Month,
                        fec_creacion = DateTime.Now,
                        userid_creacion = Convert.ToInt32(Session["user_usuarioid"])
                    });

                    result = context.SaveChanges();
                    // Get last row add into database 
                    bodega_concesionario LastAdd = context.bodega_concesionario.OrderByDescending(x => x.bodccsfec_creacion).First();


                    result = context.SaveChanges();
                    if (result > 0)
                    {
                        TempData["mensaje"] = "La bodega se creo exitosamente";
                        ViewBag.areas =
                            new SelectList(
                                context.area_bodega.Where(x => x.areabod_estado).OrderBy(x => x.areabod_nombre),
                                "areabod_id", "areabod_nombre");
                        ViewBag.bodccscentro_id = new SelectList(context.centro_costo.OrderBy(x => x.centcst_nombre),
                            "centcst_id", "centcst_nombre");
                        ViewBag.pais_id = new SelectList(context.nom_pais.OrderBy(x => x.pais_nombre), "pais_id",
                            "pais_nombre");
                        ViewBag.departamento_id = new SelectList(context.nom_departamento.OrderBy(x => x.dpto_nombre),
                            "dpto_id", "dpto_nombre");
                        ViewBag.ciudad_id = new SelectList(context.nom_ciudad.OrderBy(x => x.ciu_nombre), "ciu_id",
                            "ciu_nombre");
                        BuscarFavoritos(menu);
                        return RedirectToAction("Create", new { menu });
                    }
                }
            }
            else
            {
                TempData["mensaje_error"] = "Error al ingresar los datos, por favor valide";
            }

            ViewBag.areas =
                new SelectList(context.area_bodega.Where(x => x.areabod_estado).OrderBy(x => x.areabod_nombre),
                    "areabod_id", "areabod_nombre");
            ViewBag.bodccscentro_id = new SelectList(context.centro_costo.OrderBy(x => x.centcst_nombre), "centcst_id",
                "centcst_nombre");
            ViewBag.pais_id = new SelectList(context.nom_pais.OrderBy(x => x.pais_nombre), "pais_id", "pais_nombre");
            ViewBag.departamento_id =
                new SelectList(
                    context.nom_departamento.Where(x => x.pais_id == modelo.pais_id).OrderBy(x => x.dpto_nombre),
                    "dpto_id", "dpto_nombre", modelo.departamento_id);
            ViewBag.ciudad_id =
                new SelectList(
                    context.nom_ciudad.Where(x => x.dpto_id == modelo.departamento_id).OrderBy(x => x.ciu_nombre),
                    "ciu_id", "ciu_nombre", modelo.ciudad_id);
            BuscarFavoritos(menu);
            return View(modelo);
        }


        public JsonResult BuscarDeptoPorPais(int paisId)
        {
            var buscaDepto = context.nom_departamento.Where(x => x.pais_id == paisId).OrderBy(x => x.dpto_nombre)
                .Select(x => new
                {
                    x.dpto_id,
                    x.dpto_nombre
                });
            return Json(buscaDepto, JsonRequestBehavior.AllowGet);
        }


        public JsonResult BuscarCiudadPorDepto(int deptoId)
        {
            var buscaCiudad = context.nom_ciudad.Where(x => x.dpto_id == deptoId).OrderBy(x => x.ciu_nombre).Select(x =>
                new
                {
                    x.ciu_id,
                    x.ciu_nombre
                });
            return Json(buscaCiudad, JsonRequestBehavior.AllowGet);
        }


        // GET: bod_ccs/Edit/5
        public ActionResult update(int? id, int? menu)
        {
            //valida si el id es null
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            bodega_concesionario bod_ccs = context.bodega_concesionario.FirstOrDefault(x => x.id == id);
            if (bod_ccs == null)
            {
                return HttpNotFound();
            }
            //consulta el nombre de usuario creacion con el id, lo envia a la vista a traves de ViewBag
            users creator = context.users.Find(bod_ccs.bodccsuserid_creacion);

            ViewBag.user_nombre_cre = creator.user_nombre + " " + creator.user_apellido;

            //consulta el nombre de usuario actualizacion con el id, lo envia a la vista a traves de ViewBag
            users modificator = context.users.Find(bod_ccs.bodccsuserid_actualizacion);
            if (modificator != null)
            {
                ViewBag.user_nombre_act = modificator.user_nombre + " " + modificator.user_apellido;
            }

            ViewBag.areas =
                new SelectList(context.area_bodega.Where(x => x.areabod_estado).OrderBy(x => x.areabod_nombre),
                    "areabod_id", "areabod_nombre");
            ViewBag.bodccscentro_id = new SelectList(context.centro_costo.OrderBy(x => x.centcst_nombre), "centcst_id",
                "centcst_nombre");
            ViewBag.pais_id = new SelectList(context.nom_pais.OrderBy(x => x.pais_nombre), "pais_id", "pais_nombre");
            ViewBag.departamento_id = new SelectList(context.nom_departamento.OrderBy(x => x.dpto_nombre), "dpto_id",
                "dpto_nombre");
            ViewBag.ciudad_id = new SelectList(context.nom_ciudad.OrderBy(x => x.ciu_nombre), "ciu_id", "ciu_nombre");

            IQueryable<icb_modulo_enlaces> enlacesBuscar = context.icb_modulo_enlaces.Where(x => x.enl_modulo == 63);
            string enlaces = "";
            foreach (icb_modulo_enlaces item in enlacesBuscar)
            {
                Menus buscarEnlace = context.Menus.FirstOrDefault(x => x.idMenu == item.id_modulo_destino);
                enlaces += "<li><a href='" + buscarEnlace.url + "'>" + buscarEnlace.nombreMenu + "</a></li>";
            }

            ViewBag.nombreEnlaces = enlaces;
            BuscarFavoritos(menu);
            return View(bod_ccs);
        }


        [HttpPost]
        public ActionResult update(bodega_concesionario modelo, int? menu)
        {
            if (ModelState.IsValid)
            {
                int result = 0;
                bodega_concesionario bodegaActual = context.bodega_concesionario.FirstOrDefault(x => x.bodccs_cod == modelo.bodccs_cod);
                bodega_concesionario buscaPorNombre =
                    context.bodega_concesionario.FirstOrDefault(x => x.bodccs_nombre == modelo.bodccs_nombre);
                // Cuando el nombre asignado ya lo tiene otra bodega
                if (buscaPorNombre != null && bodegaActual != null &&
                    bodegaActual.bodccs_cod != buscaPorNombre.bodccs_cod)
                {
                    TempData["mensaje_error"] = "El codigo o el nombre ya estan asignados a otra bodega";
                }
                else
                {
                    // Actualizo los datos
                    bodegaActual.bodccsuserid_actualizacion = Convert.ToInt32(Session["user_usuarioid"]);
                    bodegaActual.bodccsfec_actualizacion = DateTime.Now;
                    bodegaActual.bodccs_nombre = modelo.bodccs_nombre;
                    bodegaActual.bodccs_estado = modelo.bodccs_estado;
                    bodegaActual.bodccsrazoninactivo = modelo.bodccsrazoninactivo;
                    bodegaActual.bodccs_direccion = modelo.bodccs_direccion;
                    bodegaActual.bodccs_cod = modelo.bodccs_cod;
                    bodegaActual.ciudad_id = modelo.ciudad_id;
                    bodegaActual.departamento_id = modelo.departamento_id;
                    bodegaActual.pais_id = modelo.pais_id;
                    bodegaActual.bodccscentro_id = modelo.bodccscentro_id;
                    bodegaActual.es_puntoventa = modelo.es_puntoventa;
                    bodegaActual.es_repuestos = modelo.es_repuestos;
                    bodegaActual.es_taller = modelo.es_taller;
                    bodegaActual.es_vehiculos = modelo.es_vehiculos;
                    bodegaActual.codigobac = modelo.codigobac;
                    context.Entry(bodegaActual).State = EntityState.Modified;
                    result = context.SaveChanges();
                    // Primero se elimina las areas que tiene esa bodega y luego se agregan las nuevas o actualizadas
                    //var areasBodega = context.bodega_area.Where(x => x.id_bodega == bodegaActual.id).ToList();
                    //foreach (var area in areasBodega)
                    //{
                    //    context.Entry(area).State = EntityState.Deleted;
                    //};
                    //context.SaveChanges();
                    // -1 significa que le han quitado todas las areas, por tanto no se agrega ninguna a esa bodega
                    //var areas = Request["areas"];
                    //if (!string.IsNullOrEmpty(areas))
                    //{
                    //    string[] areasId = areas.Split(',');
                    //    foreach (var substring in areasId)
                    //    {
                    //        context.bodega_area.Add(new bodega_area
                    //        {
                    //            id_bodega = bodegaActual.id,
                    //            areabod_id = Convert.ToInt32(substring)
                    //        });
                    //    }
                    //    result = context.SaveChanges();
                    //}
                    if (result > 0)
                    {
                        TempData["mensaje"] = "El registro de la bodega se ha actualizado correctamente";
                    }
                }
            }

            ViewBag.areas =
                new SelectList(context.area_bodega.Where(x => x.areabod_estado).OrderBy(x => x.areabod_nombre),
                    "areabod_id", "areabod_nombre");
            ViewBag.bodccscentro_id = new SelectList(context.centro_costo.OrderBy(x => x.centcst_nombre), "centcst_id",
                "centcst_nombre");
            ViewBag.pais_id = new SelectList(context.nom_pais.OrderBy(x => x.pais_nombre), "pais_id", "pais_nombre");
            ViewBag.departamento_id = new SelectList(context.nom_departamento.OrderBy(x => x.dpto_nombre), "dpto_id",
                "dpto_nombre");
            ViewBag.ciudad_id = new SelectList(context.nom_ciudad.OrderBy(x => x.ciu_nombre), "ciu_id", "ciu_nombre");
            ConsultaDatosCreacion(modelo);
            BuscarFavoritos(menu);
            return View(modelo);
        }


        public void ConsultaDatosCreacion(bodega_concesionario bodega)
        {
            //consulta el nombre de usuario creacion con el id, lo envia a la vista a traves de ViewBag
            users creator = context.users.Find(bodega.bodccsuserid_creacion);
            if (creator != null)
            {
                ViewBag.user_nombre_cre = creator.user_nombre + " " + creator.user_apellido;
            }

            users modificator = context.users.Find(bodega.bodccsuserid_actualizacion);
            if (modificator != null)
            {
                ViewBag.user_nombre_act = modificator.user_nombre + " " + modificator.user_apellido;
                ViewBag.user_fec_act = modificator.userfec_actualizacion.ToString();
            }
        }



        public JsonResult BuscarBodegasPaginadas()
        {
            var data = context.bodega_concesionario.ToList().Select(x => new
            {
                x.id,
                x.bodccs_cod,
                x.bodccs_nombre,
                x.bodccs_direccion,
                bodccs_estado = x.bodccs_estado ? "Activo" : "Inactivo"
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




        public ActionResult ConfiguracionAnticipo(int? menu)
        {


            int idbodega = Convert.ToInt32(Session["user_bodega"]);

            int  rol = Convert.ToInt32(Session["user_rolid"]);

            string permiso = (from a in context.rolacceso
                              join b in context.rolpermisos on a.idpermiso equals b.id
                              where a.idrol == rol && b.codigo =="P39"

                              select b.codigo
                                ).FirstOrDefault();

            if (permiso != null)
            {
                ViewBag.permiso = "si";
            }
            else {
                ViewBag.permiso = "no";
            }

            ViewBag.valanticipo = context.bodega_concesionario.Where(x => x.id == idbodega).Select(x => x.porcentaje_anticipo).FirstOrDefault();
            return View();
        }


        [HttpPost]
        public ActionResult ConfiguracionAnticipo()
        {
            int idbodega = Convert.ToInt32(Session["user_bodega"]);


            bodega_concesionario bodega =
                  context.bodega_concesionario.FirstOrDefault(x => x.id == idbodega);

            int valor = Convert.ToInt32(Request["ValAnticipo"]);



            bodega.porcentaje_anticipo = valor;

            context.Entry(bodega).State = EntityState.Modified;
            context.SaveChanges();


            ViewBag.valanticipo = context.bodega_concesionario.Where(x => x.id == idbodega).Select(x => x.porcentaje_anticipo).FirstOrDefault();

            TempData["mensaje"] = "El valor del anticipo de  la bodega se ha actualizado correctamente";
            return View();

        }


    }
}