using Homer_MVC.IcebergModel;
using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace Homer_MVC.Controllers
{
    public class EquipamientoVHController : Controller
    {
        private readonly Iceberg_Context context = new Iceberg_Context();

        // GET: EquipamientoVH/Create
        public ActionResult Create(int? menu)
        {
            vequipamiento equipamientoVH = new vequipamiento { estado = true };
            BuscarFavoritos(menu);
            return View(equipamientoVH);
        }

        // POST: EquipamientoVH/Create
        [HttpPost]
        public ActionResult Create(vequipamiento equipamiento, int? menu)
        {
            if (ModelState.IsValid)
            {
                vequipamiento buscarCodigo = context.vequipamiento.FirstOrDefault(x => x.codigo == equipamiento.codigo);
                if (buscarCodigo == null)
                {
                    equipamiento.fec_creacion = DateTime.Now;
                    equipamiento.userid_creacion = Convert.ToInt32(Session["user_usuarioid"]);
                    context.vequipamiento.Add(equipamiento);
                    context.SaveChanges();
                    TempData["mensaje"] = "El equipamiento " + equipamiento.codigo + " se creo correctamente";
                }
                else
                {
                    TempData["mensaje_error"] = "El area " + equipamiento.codigo + " ya existe";
                }
            }

            BuscarFavoritos(menu);
            return View();
        }

        // GET: EquipamientoVH/Edit/5
        public ActionResult Edit(int? id, int? menu)
        {
            //valida si el id es null
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            vequipamiento equipamiento = context.vequipamiento.Find(id);
            if (equipamiento == null)
            {
                return HttpNotFound();
            }
            //consulta el nombre de usuario creacion con el id, lo envia a la vista a traves de ViewBag
            IQueryable<string> result = from a in context.users
                                        join b in context.vequipamiento
                                            on a.user_id equals b.userid_creacion
                                        where b.id == id
                                        select a.user_nombre;

            foreach (string i in result)
            {
                ViewBag.user_nombre_cre = i;
            }
            //consulta el nombre de usuario actualizacion con el id, lo envia a la vista a traves de ViewBag
            IQueryable<string> result1 = from a in context.users
                                         join b in context.icb_area on a.user_id equals b.area_userid_actualizacion
                                         where b.area_id == id
                                         select a.user_nombre;
            foreach (string i in result1)
            {
                ViewBag.user_nombre_act = i;
            }

            BuscarFavoritos(menu);
            return View(equipamiento);
        }

        // POST: EquipamientoVH/Edit/5
        [HttpPost]
        public ActionResult Edit(int? menu, vequipamiento equipamiento)
        {
            if (ModelState.IsValid)
            {
                //consulta si el registro esta en BD teniendo en cuenta el id de consulta
                int nom = (from a in context.vequipamiento
                           where a.codigo == equipamiento.codigo
                           select a.codigo).Count();

                if (nom == 1)
                {
                    equipamiento.fec_actualizacion = DateTime.Now;
                    equipamiento.user_idactualizacion = Convert.ToInt32(Session["user_usuarioid"]);
                    context.Entry(equipamiento).State = EntityState.Modified;
                    context.SaveChanges();
                    TempData["mensaje"] = "La actualización del area fue exitoso!";
                    ConsultaDatosCreacion(equipamiento);
                    BuscarFavoritos(menu);
                    return View(equipamiento);
                }

                TempData["mensaje_error"] = "El registro que ingreso no se encuentra, por favor valide!";
            }

            ConsultaDatosCreacion(equipamiento);
            BuscarFavoritos(menu);
            return View(equipamiento);
        }

        public JsonResult buscarEquipamientos()
        {
            var data = context.vequipamiento.ToList().Select(x => new
            {
                x.id,
                x.codigo,
                x.Descripcion,
                estado = x.estado ? "Activo" : "Inactivo"
            });
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public void ConsultaDatosCreacion(vequipamiento equipamiento)
        {
            //consulta el nombre de usuario creacion con el id, lo envia a la vista a traves de ViewBag
            users creator = context.users.Find(equipamiento.userid_creacion);
            if (creator != null)
            {
                ViewBag.user_nombre_cre = creator.user_nombre + " " + creator.user_apellido;
            }

            users modificator = context.users.Find(equipamiento.user_idactualizacion);
            if (modificator != null)
            {
                ViewBag.user_nombre_act = modificator.user_nombre + " " + modificator.user_apellido;
                ViewBag.user_fec_act = modificator.userfec_actualizacion.ToString();
            }
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