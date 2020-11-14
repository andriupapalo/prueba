using Homer_MVC.IcebergModel;
using System;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace Homer_MVC.Controllers
{
    public class tipo_ChecklistController : Controller
    {
        private readonly Iceberg_Context context = new Iceberg_Context();

        public ActionResult Create(int? menu)
        {
            tipo_Checklist tpCheck = new tipo_Checklist
            {
                estado = true
            };


            BuscarFavoritos(menu);
            return View(tpCheck);
        }


        // POST: tomot_vh/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(tipo_Checklist tpCheckList, int? menu)
        {
            if (ModelState.IsValid)
            {
                //consulta si el registro esta en BD
                int nom = (from a in context.tipo_Checklist
                           where a.descripcion == tpCheckList.descripcion
                           select a.descripcion).Count();

                if (nom == 0)
                {
                    tpCheckList.fec_creacion = DateTime.Now;
                    tpCheckList.userid_creacion = Convert.ToInt32(Session["user_usuarioid"]);
                    context.tipo_Checklist.Add(tpCheckList);
                    try
                    {
                        context.SaveChanges();
                    }
                    catch (DbEntityValidationException dbEx)
                    {
                        Exception raise = dbEx;
                        foreach (DbEntityValidationResult validationErrors in dbEx.EntityValidationErrors)
                        {
                            foreach (DbValidationError validationError in validationErrors.ValidationErrors)
                            {
                                string message = string.Format("{0}:{1}",
                                    validationErrors.Entry.Entity,
                                    validationError.ErrorMessage);
                                // raise a new exception nesting
                                // the current instance as InnerException
                                raise = new InvalidOperationException(message, raise);
                                TempData["mensaje_error"] = raise;
                            }
                        }
                    }

                    TempData["mensaje"] = "El registro del nuevo tipo de check list fue exitoso!";
                    return RedirectToAction("Create");
                }

                TempData["mensaje_error"] = "El registro que ingreso ya se encuentra, por favor valide!";
            }

            TempData["mensaje_vacio"] = "Campos vacios, por favor valide!";

            BuscarFavoritos(menu);
            return View(tpCheckList);
        }


        // GET: tpmot_vh/Edit/5
        public ActionResult update(int? id, int? menu)
        {
            //valida si el id es null
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            tipo_Checklist tpCheckList = context.tipo_Checklist.Find(id);
            if (tpCheckList == null)
            {
                return HttpNotFound();
            }
            //consulta el nombre de usuario creacion con el id, lo envia a la vista a traves de ViewBag
            users creator = context.users.Find(tpCheckList.userid_creacion);

            ViewBag.user_nombre_cre = creator.user_nombre + " " + creator.user_apellido;

            //consulta el nombre de usuario actualizacion con el id, lo envia a la vista a traves de ViewBag
            users modificator = context.users.Find(tpCheckList.user_idactualizacion);
            if (modificator != null)
            {
                ViewBag.user_nombre_act = modificator.user_nombre + " " + modificator.user_apellido;
            }

            BuscarFavoritos(menu);
            return View(tpCheckList);
        }


        // POST: tpmot_vh/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult update(tipo_Checklist tpCheckList, int? menu)
        {
            if (ModelState.IsValid)
            {
                //consulta si el registro esta en BD teniendo en cuenta el id de consulta
                int nom = (from a in context.tipo_Checklist
                           where a.descripcion == tpCheckList.descripcion || a.id == tpCheckList.id
                           select a.descripcion).Count();

                if (nom == 1)
                {
                    tpCheckList.fec_actualizacion = DateTime.Now;
                    tpCheckList.user_idactualizacion = Convert.ToInt32(Session["user_usuarioid"]);
                    context.Entry(tpCheckList).State = EntityState.Modified;
                    context.SaveChanges();
                    ConsultaDatosCreacion(tpCheckList);
                    TempData["mensaje"] = "La actualización del tipo de motor fue exitosa!";
                    BuscarFavoritos(menu);
                    return View(tpCheckList);
                }

                TempData["mensaje_error"] = "El registro que ingreso ya se encuentra, por favor valide!";
            }

            ConsultaDatosCreacion(tpCheckList);
            TempData["mensaje_vacio"] = "Campos vacios, por favor valide!";


            BuscarFavoritos(menu);
            return View(tpCheckList);
        }


        public void ConsultaDatosCreacion(tipo_Checklist tpCheckList)
        {
            //consulta el nombre de usuario creacion con el id, lo envia a la vista a traves de ViewBag
            users creator = context.users.Find(tpCheckList.userid_creacion);
            ViewBag.user_nombre_cre = creator.user_nombre + " " + creator.user_apellido;

            users modificator = context.users.Find(tpCheckList.user_idactualizacion);
            if (modificator != null)
            {
                ViewBag.user_nombre_act = modificator.user_nombre + " " + modificator.user_apellido;
                ViewBag.user_fec_act = modificator.userfec_actualizacion.ToString();
            }
        }


        public JsonResult buscartipochecklist()
        {
            var data = context.tipo_Checklist.ToList().Select(x => new
            {
                x.id,
                x.descripcion,
                estado = x.estado ? "Activo" : "Inactivo"
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
    }
}