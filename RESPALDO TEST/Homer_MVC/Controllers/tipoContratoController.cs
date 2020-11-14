using Homer_MVC.IcebergModel;
using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace Homer_MVC.Controllers
{
    public class tipoContratoController : Controller
    {
        private readonly Iceberg_Context context = new Iceberg_Context();

        // GET: tipoContrato
        public ActionResult Create(int? menu)
        {
            BuscarFavoritos(menu);
            return View(new tipocontratocomercial { tpcontrato__estado = true });
        }

        [HttpPost]
        public ActionResult Create(tipocontratocomercial modelo, int? menu)
        {
            if (ModelState.IsValid)
            {
                tipocontratocomercial buscarNombre =
                    context.tipocontratocomercial.FirstOrDefault(x => x.descripcion == modelo.descripcion);
                if (buscarNombre != null)
                {
                    TempData["mensaje_error"] =
                        "El nombre del tipo de contrato que ingreso ya se encuentra, por favor valide!";
                }
                else
                {
                    modelo.tpcontrato_fec_creacion = DateTime.Now;
                    modelo.tpcontrato_userid_creacion = Convert.ToInt32(Session["user_usuarioid"]);
                    context.tipocontratocomercial.Add(modelo);
                    int guardar = context.SaveChanges();
                    if (guardar > 0)
                    {
                        TempData["mensaje"] = "El registro del nuevo tipo de contrato fue exitoso!";
                        return RedirectToAction("Create");
                    }
                }
            }

            BuscarFavoritos(menu);
            return View(modelo);
        }


        public ActionResult update(int? id, int? menu)
        {
            //valida si el id es null
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            tipocontratocomercial tipo = context.tipocontratocomercial.Find(id);
            if (tipo == null)
            {
                return HttpNotFound();
            }

            consultaDatosCreacion(tipo);
            BuscarFavoritos(menu);
            return View(tipo);
        }


        [HttpPost]
        public ActionResult update(tipocontratocomercial modelo, int? menu)
        {
            bool guardar = false;
            if (ModelState.IsValid)
            {
                tipocontratocomercial buscarNombre =
                    context.tipocontratocomercial.FirstOrDefault(x => x.descripcion == modelo.descripcion);
                if (buscarNombre != null)
                {
                    if (buscarNombre.id != modelo.id)
                    {
                        TempData["mensaje_error"] =
                            "El nombre del tipo de contrato que ingreso ya se encuentra, por favor valide!";
                    }
                    else
                    {
                        DateTime fechaHoy = DateTime.Now;
                        int buscarSiNoSeUsa = context.contratoscomerciales.Where(x =>
                            x.tipocontrato == modelo.id && DbFunctions.TruncateTime(x.fechafinal) >=
                            DbFunctions.TruncateTime(fechaHoy)).Count();
                        if (buscarSiNoSeUsa > 0 && modelo.tpcontrato__estado == false)
                        {
                            TempData["mensaje_error"] = "El tipo de documento se usa actualmente en " +
                                                        buscarSiNoSeUsa +
                                                        " contrato(s), por tanto no se puede desactivar!";
                        }
                        else
                        {
                            modelo.tpcontrato_userid_actualizacion = Convert.ToInt32(Session["user_usuarioid"]);
                            modelo.tpcontrato_fec_actualizacion = DateTime.Now;
                            buscarNombre.tpcontrato_userid_actualizacion = Convert.ToInt32(Session["user_usuarioid"]);
                            buscarNombre.tpcontrato_fec_actualizacion = DateTime.Now;
                            buscarNombre.tpcontrato__estado = modelo.tpcontrato__estado;
                            buscarNombre.tpcontrato_razoninactivo = modelo.tpcontrato_razoninactivo;
                            buscarNombre.descripcion = modelo.descripcion;
                            context.Entry(buscarNombre).State = EntityState.Modified;
                            guardar = context.SaveChanges() > 0;
                        }
                    }
                }
                else
                {
                    DateTime fechaHoy = DateTime.Now;
                    int buscarSiNoSeUsa = context.contratoscomerciales
                        .Where(x => x.tipocontrato == modelo.id && x.fechafinal <= fechaHoy).Count();
                    if (buscarSiNoSeUsa > 0 && modelo.tpcontrato__estado == false)
                    {
                        TempData["mensaje_error"] = "El tipo de documento se usa actualmente en " + buscarSiNoSeUsa +
                                                    " contrato(s), por tanto no se puede desactivar!";
                    }
                    else
                    {
                        modelo.tpcontrato_userid_actualizacion = Convert.ToInt32(Session["user_usuarioid"]);
                        modelo.tpcontrato_fec_actualizacion = DateTime.Now;
                        context.Entry(modelo).State = EntityState.Modified;
                        guardar = context.SaveChanges() > 0;
                    }
                }
            }

            if (guardar)
            {
                TempData["mensaje"] = "El registro del tipo de contrato se actualizo correctamente!";
            }

            consultaDatosCreacion(modelo);
            BuscarFavoritos(menu);
            return View(modelo);
        }


        public void consultaDatosCreacion(tipocontratocomercial modelo)
        {
            //consulta el nombre de usuario creacion con el id, lo envia a la vista a traves de ViewBag
            users creador = (from c in context.users
                             join b in context.tipocontratocomercial on c.user_id equals b.tpcontrato_userid_creacion
                             where b.tpcontrato_userid_creacion == modelo.tpcontrato_userid_creacion
                             select c).FirstOrDefault();

            ViewBag.user_nombre_cre = creador != null ? creador.user_nombre + " " + creador.user_apellido : null;

            //consulta el nombre de usuario actualizacion con el id, lo envia a la vista a traves de ViewBag
            users actualizador = (from c in context.users
                                  join b in context.tipocontratocomercial on c.user_id equals b.tpcontrato_userid_actualizacion
                                  where b.tpcontrato_userid_actualizacion == modelo.tpcontrato_userid_actualizacion
                                  select c).FirstOrDefault();

            ViewBag.user_nombre_act =
                actualizador != null ? actualizador.user_nombre + " " + actualizador.user_apellido : null;
        }


        public JsonResult BuscarTiposContrato()
        {
            var buscarTipos = (from tipoContrato in context.tipocontratocomercial
                               select new
                               {
                                   tipoContrato.id,
                                   tipoContrato.descripcion,
                                   estado = tipoContrato.tpcontrato__estado ? "Activo" : "Inactivo"
                               }).ToList();
            return Json(buscarTipos, JsonRequestBehavior.AllowGet);
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