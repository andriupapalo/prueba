using Homer_MVC.IcebergModel;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace Homer_MVC.Controllers
{
    public class sitiosOrigenController : Controller
    {
        private readonly Iceberg_Context context = new Iceberg_Context();

        public void CamposListasDesplegables()
        {
            IOrderedQueryable<users> buscarAsesores = context.users.Where(x => x.rol_id == 4).OrderBy(x => x.user_nombre)
                .ThenBy(x => x.user_apellido);
            List<SelectListItem> lista = new List<SelectListItem>();
            foreach (users asesor in buscarAsesores)
            {
                lista.Add(new SelectListItem
                { Value = asesor.user_id.ToString(), Text = asesor.user_nombre + " " + asesor.user_apellido });
            }

            ViewBag.asesores = lista;
            ViewBag.evento_id = new SelectList(context.tp_origen.OrderBy(x => x.tporigen_nombre), "tporigen_id",
                "tporigen_nombre");
        }


        public ActionResult Create(int? menu)
        {
            CamposListasDesplegables();
            BuscarFavoritos(menu);
            return View();
        }


        [HttpPost]
        public ActionResult Create(veventosorigen modelo, int? menu)
        {
            if (ModelState.IsValid)
            {
                // Primero se agrega el evento para despues agregarle los asesores a el respectivo evento
                modelo.fec_creacion = DateTime.Now;
                modelo.userid_creacion = Convert.ToInt32(Session["user_usuarioid"]);
                context.veventosorigen.Add(modelo);
                if (modelo.fechaini <= modelo.fechafin)
                {
                    int guardarEvento = context.SaveChanges();
                    if (guardarEvento > 0)
                    {
                        veventosorigen ultimoEvento = context.veventosorigen.OrderByDescending(x => x.id).FirstOrDefault();
                        if (ultimoEvento != null)
                        {
                            // Se agregan los asesores en caso de que exista por lo menos uno
                            string asesores = Request["asesores"];
                            if (!string.IsNullOrEmpty(asesores))
                            {
                                string[] asesoresId = asesores.Split(',');
                                foreach (string substring in asesoresId)
                                {
                                    context.vorigenasesor.Add(new vorigenasesor
                                    {
                                        evento_id = ultimoEvento.id,
                                        asesor = Convert.ToInt32(substring)
                                    });
                                }

                                int result = context.SaveChanges();
                                if (result > 0)
                                {
                                    TempData["mensaje"] = "El nuevo registro de evento fue agregado exitosamente";
                                    CamposListasDesplegables();
                                    BuscarFavoritos(menu);
                                    return RedirectToAction("Create");
                                }
                            }

                            TempData["mensaje"] = "El nuevo registro de evento fue agregado exitosamente";
                        }
                    }
                }
                else
                {
                    TempData["mensaje_error"] =
                        "La fecha final registrada no puede ser inferior a la fecha inicial, por favor valide";
                }
            }

            CamposListasDesplegables();
            BuscarFavoritos(menu);
            return View(modelo);
        }


        public ActionResult update(int? id, int? menu)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            veventosorigen evento = context.veventosorigen.Find(id);
            if (evento == null)
            {
                return HttpNotFound();
            }

            ConsultaDatosCreacion(evento);
            IOrderedQueryable<users> buscarAsesores = context.users.Where(x => x.rol_id == 4).OrderBy(x => x.user_nombre)
                .ThenBy(x => x.user_apellido);
            List<SelectListItem> lista = new List<SelectListItem>();
            foreach (users asesor in buscarAsesores)
            {
                lista.Add(new SelectListItem
                { Value = asesor.user_id.ToString(), Text = asesor.user_nombre + " " + asesor.user_apellido });
            }

            ViewBag.asesores = lista;
            ViewBag.evento_id = new SelectList(context.tp_origen.OrderBy(x => x.tporigen_nombre), "tporigen_id",
                "tporigen_nombre", evento.evento_id);

            List<vorigenasesor> asesoresAsignados = context.vorigenasesor.Where(x => x.evento_id == id).ToList();
            string idAsesoresAsignados = "";
            int index = 0;
            foreach (vorigenasesor ids in asesoresAsignados)
            {
                if (index == 0)
                {
                    idAsesoresAsignados += ids.asesor;
                    index++;
                }
                else
                {
                    idAsesoresAsignados += "," + ids.asesor;
                }
            }

            ViewBag.asesoresAsignados = idAsesoresAsignados;
            BuscarFavoritos(menu);
            return View(evento);
        }


        [HttpPost]
        public ActionResult update(veventosorigen modelo, int? menu)
        {
            if (ModelState.IsValid)
            {
                int nom = (from a in context.veventosorigen
                           where a.descripcion == modelo.descripcion || a.id == modelo.id
                           select a.descripcion).Count();

                if (nom == 1)
                {
                    if (modelo.fechaini <= modelo.fechafin)
                    {
                        string idsAsesores = Request["asesores"];
                        if (idsAsesores != null)
                        {
                            string[] idAsesor = idsAsesores.Split(',');
                            List<vorigenasesor> asesoresActuales = context.vorigenasesor.Where(x => x.evento_id == modelo.id).ToList();
                            foreach (vorigenasesor id in asesoresActuales)
                            {
                                const string query = "DELETE FROM [dbo].[vorigenasesor] WHERE [evento_id]={0}";
                                int rows = context.Database.ExecuteSqlCommand(query, id.evento_id);
                            }

                            foreach (string id in idAsesor)
                            {
                                context.vorigenasesor.Add(new vorigenasesor
                                { asesor = Convert.ToInt32(id), evento_id = modelo.id });
                            }

                            context.SaveChanges();
                        }

                        modelo.fec_actualizacion = DateTime.Now;
                        modelo.user_idactualizacion = Convert.ToInt32(Session["user_usuarioid"]);
                        context.Entry(modelo).State = EntityState.Modified;
                        context.SaveChanges();
                        TempData["mensaje"] = "La actualización del evento fue exitoso!";
                        ConsultaDatosCreacion(modelo);
                        return RedirectToAction("update", new { modelo.id, menu });
                    }

                    TempData["mensaje_error"] =
                        "La fecha final registrada no puede ser inferior a la fecha inicial, por favor valide";
                }
                else
                {
                    TempData["mensaje_error"] = "El registro que ingreso ya se encuentra, por favor valide!";
                }
            }

            IOrderedQueryable<users> buscarAsesores = context.users.Where(x => x.rol_id == 4).OrderBy(x => x.user_nombre)
                .ThenBy(x => x.user_apellido);
            List<SelectListItem> lista = new List<SelectListItem>();
            foreach (users asesor in buscarAsesores)
            {
                lista.Add(new SelectListItem
                { Value = asesor.user_id.ToString(), Text = asesor.user_nombre + " " + asesor.user_apellido });
            }

            ViewBag.asesores = lista;
            ViewBag.evento_id = new SelectList(context.tp_origen.OrderBy(x => x.tporigen_nombre), "tporigen_id",
                "tporigen_nombre", modelo.evento_id);

            List<vorigenasesor> asesoresAsignados = context.vorigenasesor.Where(x => x.evento_id == modelo.id).ToList();
            string idAsesoresAsignados = "";
            int index = 0;
            foreach (vorigenasesor ids in asesoresAsignados)
            {
                if (index == 0)
                {
                    idAsesoresAsignados += ids.asesor;
                    index++;
                }
                else
                {
                    idAsesoresAsignados += "," + ids.asesor;
                }
            }

            ViewBag.asesoresAsignados = idAsesoresAsignados;
            ConsultaDatosCreacion(modelo);
            BuscarFavoritos(menu);
            return View();
        }


        public void ConsultaDatosCreacion(veventosorigen evento)
        {
            //consulta el nombre de usuario creacion con el id, lo envia a la vista a traves de ViewBag
            users creator = context.users.Find(evento.userid_creacion);
            if (creator != null)
            {
                ViewBag.user_nombre_cre = creator.user_nombre + " " + creator.user_apellido;
            }

            users modificator = context.users.Find(evento.user_idactualizacion);
            if (modificator != null)
            {
                ViewBag.user_nombre_act = modificator.user_nombre + " " + modificator.user_apellido;
                ViewBag.user_fec_act = modificator.userfec_actualizacion.ToString();
            }
        }


        public JsonResult BuscarEventosPaginados()
        {
            var buscarEventos = (from eventos in context.veventosorigen
                                 join origen in context.tp_origen
                                     on eventos.evento_id equals origen.tporigen_id
                                 select new
                                 {
                                     eventos.id,
                                     eventos.descripcion,
                                     origen.tporigen_nombre
                                 }).ToList();
            return Json(buscarEventos, JsonRequestBehavior.AllowGet);
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