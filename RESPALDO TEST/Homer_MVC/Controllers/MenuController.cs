using Homer_MVC.IcebergModel;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;

namespace Homer_MVC.Controllers
{
    public class MenuController : Controller
    {
        private readonly Iceberg_Context context = new Iceberg_Context();
        public List<Menus> menusUsuarioActual = new List<Menus>();

        public ActionResult _Menu(int? menu)
        {
            if (menu != null && menu != 0)
            {
                int subMenus = 0;
                int menu2 = 0;
                int menu3 = 0;
                int menu4 = 0;
                Menus buscarMenu = context.Menus.FirstOrDefault(x => x.idMenu == menu);
                if (buscarMenu != null) //var menu1 = buscarMenu.idMenu;
                {
                    if (buscarMenu.Menus2 != null)
                    {
                        menu2 = buscarMenu.Menus2.idMenu;
                        subMenus++;
                        if (buscarMenu.Menus2.Menus2 != null)
                        {
                            menu3 = buscarMenu.Menus2.Menus2.idMenu;
                            subMenus++;
                            if (buscarMenu.Menus2.Menus2.Menus2 != null)
                            {
                                menu4 = buscarMenu.Menus2.Menus2.Menus2.idMenu;
                                subMenus++;
                            }
                        }
                    }
                }

                if (subMenus == 1)
                {
                    ViewBag.menuUno = menu2;
                }
                else if (subMenus == 2)
                {
                    ViewBag.menuUno = menu3;
                    ViewBag.menuDos = menu2;
                }
                else if (subMenus == 3)
                {
                    ViewBag.menuUno = menu4;
                    ViewBag.menuDos = menu3;
                    ViewBag.menuTres = menu2;
                }
            }
            //var menuActual = Request.Url.LocalPath;
            //var idMenu1 = 0;
            //var idMenu2 = 0;
            //var idMenu3 = 0;
            //var buscaMenuActual = context.Menus.FirstOrDefault(x => x.url.Contains(menuActual));
            //idMenu1 = buscaMenuActual.padreId != null ? buscaMenuActual.padreId ?? 0 : 0;
            //ViewBag.menuUno = idMenu1;

            //if (idMenu1 != 1)
            //{
            //var buscaPadreMenuActual2 = context.Menus.FirstOrDefault(x => x.idMenu == idMenu1);
            //if (buscaPadreMenuActual2.padreId == null)
            //{
            //    idMenu3 = idMenu1;
            //    ViewBag.menuTres = idMenu3;
            //}
            //else
            //{
            //var buscaPadreMenuActual3 = context.Menus.FirstOrDefault(x => x.idMenu == buscaPadreMenuActual2.padreId);
            //if (buscaPadreMenuActual3 == null)
            //{
            //    //ViewBag.menuTres = buscaPadreMenuActual2.padreId;
            //    ViewBag.menuDos = buscaMenuActual.padreId;
            //}
            //else
            //{
            //    if (buscaPadreMenuActual3.padreId != null)
            //    {
            //        ViewBag.menuTres = buscaPadreMenuActual3.padreId;
            //        //ViewBag.menuDos = buscaPadreMenuActual2.padreId;
            //        ViewBag.menuUno = buscaMenuActual.padreId;
            //    }
            //    else
            //    {
            //        ViewBag.menuTres = buscaPadreMenuActual2.padreId;
            //       // ViewBag.menuDos = buscaMenuActual.padreId;
            //    }
            //}

            //}
            //}


            int idActualUser = Convert.ToInt32(Session["user_usuarioid"]);

            users user = context.users.FirstOrDefault(x => x.user_id == idActualUser);
            if (user != null)
            {
                List<Menus> menuPadres = context.Menus.Where(x => x.padreId == null).ToList();

                List<Menu_rol> menusUsuario = context.Menu_rol.Where(x => x.idperfil == user.rol_id).ToList();
                foreach (Menus menuUno in menuPadres)
                {
                    foreach (Menu_rol mu in menusUsuario)
                    {
                        if (mu.idmenu == menuUno.idMenu)
                        {
                            AgregarMenuUno(menuUno);
                        }
                    }

                    if (menuUno.Menus1.Count > 0)
                    {
                        foreach (Menus menuDos in menuUno.Menus1)
                        {
                            foreach (Menu_rol mu in menusUsuario)
                            {
                                if (mu.idmenu == menuDos.idMenu)
                                {
                                    AgregarMenuDos(menuUno, menuDos);
                                }
                            }

                            if (menuDos.Menus1.Count > 0)
                            {
                                foreach (Menus menuTres in menuDos.Menus1)
                                {
                                    foreach (Menu_rol mu in menusUsuario)
                                    {
                                        if (mu.idmenu == menuTres.idMenu)
                                        {
                                            AgregarMenuTres(menuUno, menuDos, menuTres);
                                        }
                                    }

                                    if (menuTres.Menus1.Count > 0)
                                    {
                                        foreach (Menus menuCuatro in menuTres.Menus1)
                                        {
                                            foreach (Menu_rol mu in menusUsuario)
                                            {
                                                if (mu.idmenu == menuCuatro.idMenu)
                                                {
                                                    AgregarMenuCuatro(menuUno, menuDos, menuTres, menuCuatro);
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            //var menu = db.Menus.Where(x => x.padreId == null).ToList();
            int cuantos = menusUsuarioActual.Count();
            for (int i = 0; i < cuantos; i++)
            {
                menusUsuarioActual[i].Menus1 = menusUsuarioActual[i].Menus1.OrderBy(x => x.idMenu).ToList();
                int cuantos2 = menusUsuarioActual[i].Menus1.Count();

                foreach (Menus menuDos in menusUsuarioActual[i].Menus1)
                {
                    menuDos.Menus1 = menuDos.Menus1.OrderBy(x => x.idMenu).ToList();
                    foreach (Menus menuTres in menuDos.Menus1)
                    {
                        menuTres.Menus1 = menuTres.Menus1.OrderBy(x => x.idMenu).ToList();
                        foreach (Menus menuCuatro in menuTres.Menus1)
                        {
                            menuCuatro.Menus1 = menuCuatro.Menus1.OrderBy(x => x.idMenu).ToList();
                        }
                    }
                }
            }

            ViewBag.menuSeleccionado = menu != null ? menu.ToString() : "939284618945";
            return PartialView("_Menu", menusUsuarioActual);
        }


        public Menus BuscarMenu(int idMenu)
        {
            foreach (Menus menuUno in menusUsuarioActual)
            {
                if (menuUno.idMenu == idMenu)
                {
                    return menuUno;
                }

                if (menuUno.Menus1 != null)
                {
                    if (menuUno.Menus1.Count > 0)
                    {
                        foreach (Menus menuDos in menuUno.Menus1)
                        {
                            if (menuDos.idMenu == idMenu)
                            {
                                return menuDos;
                            }

                            if (menuDos.Menus1 != null)
                            {
                                if (menuDos.Menus1.Count > 0)
                                {
                                    foreach (Menus menuTres in menuDos.Menus1)
                                    {
                                        if (menuTres.idMenu == idMenu)
                                        {
                                            return menuTres;
                                        }

                                        if (menuTres.Menus1 != null)
                                        {
                                            if (menuTres.Menus1.Count > 0)
                                            {
                                                foreach (Menus menuCuatro in menuTres.Menus1)
                                                {
                                                    if (menuCuatro.idMenu == idMenu)
                                                    {
                                                        return menuCuatro;
                                                    }
                                                }
                                            }
                                        }
                                        else
                                        {
                                            menuTres.Menus1 = new List<Menus>();
                                        }
                                    }
                                }
                            }
                            else
                            {
                                menuDos.Menus1 = new List<Menus>();
                            }
                        }
                    }
                }
                else
                {
                    menuUno.Menus1 = new List<Menus>();
                }
            }

            return null;
        }


        public void AgregarMenuUno(Menus menuUno)
        {
            Menus nuevoMenuUno = new Menus
            {
                idMenu = menuUno.idMenu,
                icono = menuUno.icono,
                url = menuUno.url,
                nombreMenu = menuUno.nombreMenu
            };
            menusUsuarioActual.Add(nuevoMenuUno);
        }


        public void AgregarMenuDos(Menus menuUno, Menus menuDos)
        {
            Menus nuevoMenuDos = new Menus
            {
                idMenu = menuDos.idMenu,
                icono = menuDos.icono,
                url = menuDos.url,
                nombreMenu = menuDos.nombreMenu
            };
            Menus buscaMenuUno = BuscarMenu(menuUno.idMenu);
            if (buscaMenuUno != null)
            {
                buscaMenuUno.Menus1.Add(nuevoMenuDos);
            }
            else
            {
                Menus nuevoMenuUno = new Menus
                {
                    idMenu = menuUno.idMenu,
                    icono = menuUno.icono,
                    url = menuUno.url,
                    nombreMenu = menuUno.nombreMenu
                };
                nuevoMenuUno.Menus1 = new List<Menus>
                {
                    nuevoMenuDos
                };
                menusUsuarioActual.Add(nuevoMenuUno);
            }
        }


        public void AgregarMenuTres(Menus menuUno, Menus menuDos, Menus menuTres)
        {
            Menus nuevoMenuDos = new Menus
            {
                idMenu = menuDos.idMenu,
                icono = menuDos.icono,
                url = menuDos.url,
                nombreMenu = menuDos.nombreMenu
            };
            Menus nuevoMenuTres = new Menus
            {
                idMenu = menuTres.idMenu,
                icono = menuTres.icono,
                url = menuTres.url,
                nombreMenu = menuTres.nombreMenu
            };
            Menus buscaMenuDos = BuscarMenu(menuDos.idMenu);
            if (buscaMenuDos != null)
            {
                if (nuevoMenuTres.Menus1 == null)
                {
                    nuevoMenuTres.Menus1 = new List<Menus>();
                }

                buscaMenuDos.Menus1.Add(nuevoMenuTres);
            }
            else
            {
                Menus buscaMenuUno = BuscarMenu(menuUno.idMenu);
                if (buscaMenuUno != null)
                {
                    if (nuevoMenuTres.Menus1 == null)
                    {
                        nuevoMenuTres.Menus1 = new List<Menus>();
                    }

                    if (nuevoMenuDos.Menus1 == null)
                    {
                        nuevoMenuDos.Menus1 = new List<Menus>();
                    }

                    nuevoMenuDos.Menus1.Add(nuevoMenuTres);
                    buscaMenuUno.Menus1.Add(nuevoMenuDos);
                }
                else
                {
                    Menus nuevoMenuUno = new Menus
                    {
                        idMenu = menuUno.idMenu,
                        icono = menuUno.icono,
                        url = menuUno.url,
                        nombreMenu = menuUno.nombreMenu
                    };
                    nuevoMenuTres.Menus1 = new List<Menus>();
                    nuevoMenuDos.Menus1 = new List<Menus>
                    {
                        nuevoMenuTres
                    };
                    nuevoMenuUno.Menus1 = new List<Menus>
                    {
                        nuevoMenuDos
                    };
                    menusUsuarioActual.Add(nuevoMenuUno);
                }
            }
        }


        public void AgregarMenuCuatro(Menus menuUno, Menus menuDos, Menus menuTres, Menus menuCuatro)
        {
            Menus nuevoMenuDos = new Menus
            {
                idMenu = menuDos.idMenu,
                icono = menuDos.icono,
                url = menuDos.url,
                nombreMenu = menuDos.nombreMenu
            };
            Menus nuevoMenuTres = new Menus
            {
                idMenu = menuTres.idMenu,
                icono = menuTres.icono,
                url = menuTres.url,
                nombreMenu = menuTres.nombreMenu
            };
            Menus nuevoMenuCuatro = new Menus
            {
                idMenu = menuCuatro.idMenu,
                icono = menuCuatro.icono,
                url = menuCuatro.url,
                nombreMenu = menuCuatro.nombreMenu
            };
            Menus buscaMenuTres = BuscarMenu(menuTres.idMenu);
            if (buscaMenuTres != null)
            {
                buscaMenuTres.Menus1.Add(nuevoMenuCuatro);
            }
            else
            {
                Menus buscaMenuDos = BuscarMenu(menuDos.idMenu);
                if (buscaMenuDos != null)
                {
                    if (nuevoMenuTres.Menus1 == null)
                    {
                        nuevoMenuTres.Menus1 = new List<Menus>();
                    }

                    nuevoMenuTres.Menus1.Add(nuevoMenuCuatro);
                    buscaMenuDos.Menus1.Add(nuevoMenuTres);
                }
                else
                {
                    Menus buscaMenuUno = BuscarMenu(menuUno.idMenu);
                    if (buscaMenuUno != null)
                    {
                        if (nuevoMenuTres.Menus1 == null)
                        {
                            nuevoMenuTres.Menus1 = new List<Menus>();
                        }

                        if (nuevoMenuDos.Menus1 == null)
                        {
                            nuevoMenuDos.Menus1 = new List<Menus>();
                        }

                        nuevoMenuTres.Menus1.Add(nuevoMenuCuatro);
                        nuevoMenuDos.Menus1.Add(nuevoMenuTres);
                        buscaMenuUno.Menus1.Add(nuevoMenuDos);
                    }
                    else
                    {
                        Menus nuevoMenuUno = new Menus
                        {
                            idMenu = menuUno.idMenu,
                            icono = menuUno.icono,
                            url = menuUno.url,
                            nombreMenu = menuUno.nombreMenu
                        };
                        nuevoMenuTres.Menus1 = new List<Menus>
                        {
                            nuevoMenuCuatro
                        };
                        nuevoMenuDos.Menus1 = new List<Menus>
                        {
                            nuevoMenuTres
                        };
                        nuevoMenuUno.Menus1 = new List<Menus>
                        {
                            nuevoMenuDos
                        };
                        menusUsuarioActual.Add(nuevoMenuUno);
                    }
                }
            }
        }


        public JsonResult AgregarMenuFavoritos(int? id_Menu)
        {
            int usuarioActual = Convert.ToInt32(Session["user_usuarioid"]);

            Menus buscarMenu = context.Menus.FirstOrDefault(x => x.idMenu == id_Menu);
            if (buscarMenu != null)
            {
                if (buscarMenu.url != "#")
                {
                    favoritos buscarFavorito = context.favoritos.FirstOrDefault(x => x.idmenu == id_Menu);
                    if (buscarFavorito == null)
                    {
                        context.favoritos.Add(new favoritos
                        {
                            idmenu = id_Menu ?? 0,
                            cantidad = 1,
                            idusuario = usuarioActual,
                            seleccionado = false
                        });
                    }
                    else
                    {
                        buscarFavorito.cantidad = buscarFavorito.cantidad + 1;
                        context.Entry(buscarFavorito).State = EntityState.Modified;
                    }

                    int guardar = context.SaveChanges();
                    if (guardar > 0)
                    {
                        return Json(buscarMenu.url + "?menu=" + id_Menu, JsonRequestBehavior.AllowGet);
                    }
                }

                return Json("#", JsonRequestBehavior.AllowGet);
            }

            return Json("#", JsonRequestBehavior.AllowGet);
        }


        public JsonResult BuscarFavoritosUsuarioActual()
        {
            int usuarioActual = Convert.ToInt32(Session["user_usuarioid"]);

            var buscarFavoritosMasVistos = (from favoritos in context.favoritos
                                            join menu in context.Menus
                                                on favoritos.idmenu equals menu.idMenu
                                            where favoritos.idusuario == usuarioActual
                                            select new
                                            {
                                                favoritos.seleccionado,
                                                favoritos.cantidad,
                                                menu.nombreMenu,
                                                menu.url,
                                                menu.idMenu
                                            }).Take(2).ToList();

            var buscarFavoritosSeleccionados = (from favoritos in context.favoritos
                                                join menu in context.Menus
                                                    on favoritos.idmenu equals menu.idMenu
                                                where favoritos.idusuario == usuarioActual && favoritos.seleccionado
                                                select new
                                                {
                                                    favoritos.seleccionado,
                                                    favoritos.cantidad,
                                                    menu.nombreMenu,
                                                    menu.url,
                                                    menu.idMenu
                                                }).ToList();

            foreach (var favoritos in buscarFavoritosMasVistos)
            {
                bool existe = false;
                var favoritoAgregar = favoritos;
                foreach (var favoritos2 in buscarFavoritosSeleccionados)
                {
                    if (favoritos.nombreMenu == favoritos2.nombreMenu)
                    {
                        existe = true;
                        break;
                    }
                }

                if (!existe)
                {
                    buscarFavoritosSeleccionados.Add(favoritoAgregar);
                }
            }

            return Json(buscarFavoritosSeleccionados, JsonRequestBehavior.AllowGet);
        }
    }
}