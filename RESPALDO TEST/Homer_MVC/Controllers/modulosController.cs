using Homer_MVC.IcebergModel;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Reflection;
using System.Web.Mvc;

namespace Homer_MVC.Controllers
{
    public class modulosController : Controller
    {
        private readonly Iceberg_Context context = new Iceberg_Context();
        private readonly ParametrosBusquedaModel parametrosBusqueda = new ParametrosBusquedaModel();

        // GET: modulos
        public ActionResult Index(int? menu)
        {
            List<Menus> buscarPadres = context.Menus.Where(x => x.padreId == null).OrderBy(x => x.nombreMenu).ToList();
            ViewBag.procesos = buscarPadres;
            BuscarFavoritos(menu);
            return View();
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


        public JsonResult BuscarSubProcesos(int idProceso)
        {
            List<Menus> subProcesos = context.Menus.Where(x => x.padreId == idProceso).OrderBy(x => x.nombreMenu).ToList();
            var result = subProcesos.Select(x => new
            {
                x.idMenu,
                x.nombreMenu
            });

            return Json(result, JsonRequestBehavior.AllowGet);
        }


        public JsonResult BuscarGrupos(int idSubProceso)
        {
            List<Menus> grupos = context.Menus.Where(x => x.padreId == idSubProceso).ToList().OrderBy(x => x.nombreMenu)
                .ToList();
            var result = grupos.Select(x => new
            {
                x.idMenu,
                x.nombreMenu
            });

            return Json(result, JsonRequestBehavior.AllowGet);
        }


        public JsonResult ValidarGrupo(int idGrupo)
        {
            Menus buscaGrupo = context.Menus.FirstOrDefault(x => x.idMenu == idGrupo);
            bool result = false;
            if (buscaGrupo.url == "#")
            {
                result = true;
            }

            return Json(result, JsonRequestBehavior.AllowGet);
        }


        public JsonResult AgregarModulo(string accion, int? procesoId, string procesoNombre, int? subProcesoId,
            string subProcesoNombre, int? grupoId, string grupoNombre, string subGrupoNombre, string tipoModulo)
        {
            int result = -1;
            string urlTipoModulo = "#";
            if (tipoModulo == "vista")
            {
                urlTipoModulo = "../../../../ModuloDesarrollo";
            }

            if (accion == "proceso")
            {
                if (procesoNombre.Trim().Length > 0)
                {
                    Menus buscarNombre = context.Menus.FirstOrDefault(x => x.nombreMenu == procesoNombre);
                    // Significa que el nombre del menu ya existe
                    if (buscarNombre != null)
                    {
                        result = -1;
                    }
                    else
                    {
                        Menus nuevo = new Menus
                        {
                            url = urlTipoModulo,
                            icono = "fa fa-exclamation",
                            nombreMenu = procesoNombre,
                            padreId = null
                        };

                        context.Menus.Add(nuevo);
                        result = context.SaveChanges();
                    }
                }
            }

            if (accion == "subProceso")
            {
                if (subProcesoNombre.Trim().Length > 0 && procesoId != null)
                {
                    Menus buscarNombre = context.Menus.FirstOrDefault(x => x.nombreMenu == subProcesoNombre);
                    if (buscarNombre != null)
                    {
                        result = -1;
                    }
                    else
                    {
                        Menus nuevo = new Menus
                        {
                            url = urlTipoModulo,
                            icono = "fa fa-exclamation",
                            nombreMenu = subProcesoNombre,
                            padreId = procesoId,
                            Menus2 = context.Menus.FirstOrDefault(x => x.idMenu == procesoId)
                        };
                        context.Menus.Add(nuevo);
                        result = context.SaveChanges();
                    }
                }
            }

            if (accion == "grupo")
            {
                if (grupoNombre.Trim().Length > 0 && subProcesoId != null)
                {
                    Menus buscarNombre = context.Menus.FirstOrDefault(x => x.nombreMenu == grupoNombre);
                    if (buscarNombre != null)
                    {
                        result = -1;
                    }
                    else
                    {
                        Menus nuevo = new Menus
                        {
                            url = urlTipoModulo,
                            icono = "fa fa-exclamation",
                            nombreMenu = grupoNombre,
                            padreId = subProcesoId,
                            Menus2 = context.Menus.FirstOrDefault(x => x.idMenu == subProcesoId)
                        };
                        context.Menus.Add(nuevo);
                        result = context.SaveChanges();
                    }
                }
            }

            if (accion == "subGrupo")
            {
                if (subGrupoNombre.Trim().Length > 0 && grupoId != null)
                {
                    Menus buscarNombre = context.Menus.FirstOrDefault(x => x.nombreMenu == subGrupoNombre);
                    if (buscarNombre != null)
                    {
                        result = -1;
                    }
                    else
                    {
                        Menus nuevo = new Menus
                        {
                            url = "../../../../moduloEnDesarrollo",
                            icono = "fa fa-exclamation",
                            nombreMenu = subGrupoNombre,
                            padreId = grupoId,
                            Menus2 = context.Menus.FirstOrDefault(x => x.idMenu == grupoId)
                        };
                        context.Menus.Add(nuevo);
                        result = context.SaveChanges();
                    }
                }
            }

            return Json(result, JsonRequestBehavior.AllowGet);
        }


        public JsonResult BuscarMenusPaginados()
        {
            var data = (from menu in context.Menus
                        join menu2 in context.Menus
                            on menu.padreId equals menu2.idMenu into ps
                        from menu2 in ps.DefaultIfEmpty()
                        select new
                        {
                            menu.idMenu,
                            menu.nombreMenu,
                            menuPadre = menu2.nombreMenu ?? ""
                        }).ToList();
            return Json(data, JsonRequestBehavior.AllowGet);
        }


        [HttpGet]
        public ActionResult Update(int idMenu, int? menuActual)
        {
            menu_busqueda nombreClase = context.menu_busqueda.FirstOrDefault(x => x.menu_busqueda_id_menu == idMenu);
            List<SelectListItem> camposBusqueda = new List<SelectListItem>();

            if (nombreClase != null)
            {
                Type[] types = Assembly.GetExecutingAssembly().GetTypes();

                foreach (Type t in types)
                {
                    if (t.Name == nombreClase.menu_busqueda_vista)
                    {
                        PropertyInfo[] propiedades = t.GetProperties();
                        foreach (PropertyInfo propiedad in propiedades)
                        {
                            menu_busqueda nombreBuscar =
                                context.menu_busqueda.FirstOrDefault(x => x.menu_busqueda_campo == propiedad.Name);
                            if (nombreBuscar == null)
                            {
                                camposBusqueda.Add(new SelectListItem { Value = propiedad.Name, Text = propiedad.Name });
                            }
                        }
                    }
                }

                ViewBag.Vista = nombreClase.menu_busqueda_vista;
            }
            else
            {
                ViewData["VistaAsignada"] = "Error";
            }

            Menus buscaMenu = context.Menus.FirstOrDefault(x => x.idMenu == idMenu);
            IQueryable<Menus> menues = context.Menus.Where(x => x.idMenu != idMenu && x.url == "#");
            List<SelectListItem> menuList = new List<SelectListItem>();
            foreach (Menus item in menues)
            {
                menuList.Add(new SelectListItem { Value = item.idMenu.ToString(), Text = item.nombreMenu });
            }

            ViewBag.listaMenus = new SelectList(menuList);


            IQueryable<Menus> menuesEnlaces = context.Menus.Where(x => x.idMenu != idMenu && x.url != "#");
            ViewBag.listaMenusEnlaces = menuesEnlaces;

            List<icb_modulo_enlaces> enlacesAsignados = context.icb_modulo_enlaces.Where(x => x.enl_modulo == idMenu).ToList();
            string idEnlacesAsignados = "";
            int index = 0;
            foreach (icb_modulo_enlaces ids in enlacesAsignados)
            {
                if (index == 0)
                {
                    idEnlacesAsignados += ids.id_modulo_destino;
                    index++;
                }
                else
                {
                    idEnlacesAsignados += "," + ids.id_modulo_destino;
                }
            }

            ViewBag.enlacesAsignados = idEnlacesAsignados;


            ViewBag.parametros = camposBusqueda;
            List<menu_busqueda> parametrosVista = context.menu_busqueda.Where(x => x.menu_busqueda_id_menu == 106).ToList();
            ViewBag.parametrosBusqueda = parametrosVista;
            BuscarFavoritos(menuActual);
            return View(buscaMenu);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Update(Menus menu, int? menuActual)
        {
            if (ModelState.IsValid)
            {
                string idEnlaces = Request["selectEnlaces"];
                if (idEnlaces != null)
                {
                    string[] idEnlace = idEnlaces.Split(',');
                    List<icb_modulo_enlaces> enlacesActuales = context.icb_modulo_enlaces.Where(x => x.enl_modulo == menu.idMenu).ToList();
                    foreach (icb_modulo_enlaces id in enlacesActuales)
                    {
                        const string query = "DELETE FROM [dbo].[icb_modulo_enlaces] WHERE [enl_id]={0}";
                        int rows = context.Database.ExecuteSqlCommand(query, id.enl_id);
                    }

                    foreach (string id in idEnlace)
                    {
                        context.icb_modulo_enlaces.Add(new icb_modulo_enlaces
                        {
                            enl_usucreacion = Convert.ToInt32(Session["user_usuarioid"]),
                            enl_feccreacion = DateTime.Now,
                            enl_modulo = menu.idMenu,
                            id_modulo_destino = Convert.ToInt32(id)
                        });
                    }

                    context.SaveChanges();
                }


                Menus buscarMenu = context.Menus.FirstOrDefault(x => x.idMenu == menu.idMenu);
                buscarMenu.nombreMenu = menu.nombreMenu;
                buscarMenu.padreId = menu.padreId;
                buscarMenu.Menus2 = context.Menus.FirstOrDefault(x => x.idMenu == menu.padreId);

                context.Entry(buscarMenu).State = EntityState.Modified;
                bool guardar = context.SaveChanges() > 0;
                if (guardar)
                {
                    TempData["mensaje"] = "El modulo se actualizó correctamente";
                }
                else
                {
                    TempData["mensaje_error"] = "Error de conexion, por favor verifique su conexion";
                }
                //var menues = context.Menus.Where(x => x.idMenu != menu.idMenu && x.url == "#");
                //var menuList = new List<SelectListItem>();
                //foreach (var item in menues)
                //{
                //    menuList.Add(new SelectListItem() { Value = item.idMenu.ToString(), Text = item.nombreMenu });
                //}
                //ViewBag.listaMenus = new SelectList(menuList);

                //List<SelectListItem> camposBusqueda = new List<SelectListItem>();
                //var nombreClase = context.menu_busqueda.FirstOrDefault(x => x.menu_busqueda_id_menu == menu.idMenu);
                //if (nombreClase != null)
                //{
                //    Type[] types = Assembly.GetExecutingAssembly().GetTypes();

                //    foreach (Type t in types)
                //    {
                //        if (t.Name == nombreClase.menu_busqueda_vista)
                //        {
                //            var propiedades = t.GetProperties();
                //            foreach (var propiedad in propiedades)
                //            {
                //                var nombreBuscar = context.menu_busqueda.FirstOrDefault(x => x.menu_busqueda_campo == propiedad.Name);
                //                if (nombreBuscar == null)
                //                {
                //                    camposBusqueda.Add(new SelectListItem() { Value = propiedad.Name, Text = propiedad.Name });
                //                }
                //            }
                //        }
                //    }
                //    ViewBag.Vista = nombreClase.menu_busqueda_vista;
                //}
            }

            //ViewBag.parametros = camposBusqueda;
            List<menu_busqueda> parametrosVista = context.menu_busqueda.Where(x => x.menu_busqueda_id_menu == 106).ToList();
            ViewBag.parametrosBusqueda = parametrosVista;

            IQueryable<Menus> menuesEnlaces = context.Menus.Where(x => x.idMenu != menu.idMenu && x.url != "#");
            ViewBag.listaMenusEnlaces = menuesEnlaces;

            IQueryable<Menus> menues = context.Menus.Where(x => x.idMenu != menu.idMenu && x.url == "#");
            List<SelectListItem> menuList = new List<SelectListItem>();
            foreach (Menus item in menues)
            {
                menuList.Add(new SelectListItem { Value = item.idMenu.ToString(), Text = item.nombreMenu });
            }

            ViewBag.listaMenus = new SelectList(menuList);

            BuscarFavoritos(menuActual);
            return View();
        }


        public JsonResult AgregarParametro(int idMenu, string parametro, string nombreParametro, string vista)
        {
            menu_busqueda nuevoParametro = new menu_busqueda
            {
                menu_busqueda_id_menu = idMenu,
                menu_busqueda_campo = parametro,
                menu_busqueda_id_pestana = null,
                menu_busqueda_nombre = nombreParametro,
                menu_busqueda_vista = vista
            };
            context.menu_busqueda.Add(nuevoParametro);
            bool result = context.SaveChanges() > 0;
            menu_busqueda ultimo = context.menu_busqueda.OrderByDescending(x => x.menu_busqueda_id).First();
            var data = new
            {
                ultimo,
                result
            };

            return Json(data, JsonRequestBehavior.AllowGet);
        }


        public JsonResult ActualizarParametro(int idMenu)
        {
            List<menu_busqueda> parametros = context.menu_busqueda.Where(x => x.menu_busqueda_id_menu == idMenu).ToList();
            var result = parametros.Select(x => new
            {
                x.menu_busqueda_id,
                x.menu_busqueda_nombre,
                x.menu_busqueda_campo
            });

            return Json(result, JsonRequestBehavior.AllowGet);
        }


        public JsonResult EliminarParametro(int idMenu)
        {
            bool result = false;
            menu_busqueda menuBusqueda = context.menu_busqueda.FirstOrDefault(x => x.menu_busqueda_id == idMenu);
            if (menuBusqueda != null)
            {
                context.menu_busqueda.Attach(menuBusqueda);
                context.menu_busqueda.Remove(menuBusqueda);
                result = context.SaveChanges() > 0;
            }

            var data = new
            {
                result,
                menuBusqueda
            };
            return Json(data, JsonRequestBehavior.AllowGet);
        }


        public JsonResult AgregarNombreVista(string nombreVista)
        {
            List<SelectListItem> camposBusqueda = new List<SelectListItem>();
            Type[] types = Assembly.GetExecutingAssembly().GetTypes();
            bool result = false;
            foreach (Type t in types)
            {
                if (t.Name == nombreVista)
                {
                    PropertyInfo[] propiedades = t.GetProperties();
                    foreach (PropertyInfo propiedad in propiedades)
                    {
                        //var nombreBuscar = context.menu_busqueda.FirstOrDefault(x => x.menu_busqueda_campo == propiedad.Name);
                        //if (nombreBuscar == null)
                        //{
                        camposBusqueda.Add(new SelectListItem { Value = propiedad.Name, Text = propiedad.Name });
                    }
                    //}
                    result = true;
                }
            }

            var data = new
            {
                result,
                camposBusqueda
            };
            return Json(data, JsonRequestBehavior.AllowGet);
        }
    }
}