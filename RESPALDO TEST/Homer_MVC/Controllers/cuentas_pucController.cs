using Homer_MVC.IcebergModel;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace Homer_MVC.Controllers
{
    public class cuentas_pucController : Controller
    {
        private readonly Iceberg_Context context = new Iceberg_Context();

        public ActionResult Create(int? menu)
        {
            cuenta_puc crearCuenta = new cuenta_puc { cntpuc_estado = true };

            ViewBag.mov_cnt = new SelectList(new List<SelectListItem>
            {
                new SelectListItem {Text = "Credito", Value = "Credito"},
                new SelectListItem {Text = "Debito", Value = "Debito"}
            });
            ViewBag.concepniff = new SelectList(context.tipo_niff, "tipo_niff_id", "tipo_niff_nombre");

            var data = from c in context.cuenta_puc
                       orderby c.cntpuc_numero
                       where c.concepniff == 4 //tipo de cuenta solo niff
                       select new
                       {
                           id = c.cntpuc_id,
                           nombre = c.cntpuc_numero + " - " + c.cntpuc_descp
                       };
            List<SelectListItem> cuentasNiff = new List<SelectListItem>();
            foreach (var item in data)
            {
                cuentasNiff.Add(new SelectListItem
                {
                    Text = item.nombre,
                    Value = item.id.ToString()
                });
            }

            ViewBag.cuentaniff = cuentasNiff;

            IQueryable<icb_modulo_enlaces> enlacesBuscar = context.icb_modulo_enlaces.Where(x => x.enl_modulo == 71);
            string enlaces = "";
            foreach (icb_modulo_enlaces item in enlacesBuscar)
            {
                Menus buscarEnlace = context.Menus.FirstOrDefault(x => x.idMenu == item.id_modulo_destino);
                enlaces += "<li><a href='" + buscarEnlace.url + "'>" + buscarEnlace.nombreMenu + "</a></li>";
            }

            ViewBag.nombreEnlaces = enlaces;
            BuscarFavoritos(menu);
            return View(crearCuenta);
        }

        // POST: cuenta_puc/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(cuenta_puc cuenta_puc, int? menu)
        {
            if (ModelState.IsValid)
            {
                //consulta si el registro esta en BD
                int nom = (from a in context.cuenta_puc
                           where a.cntpuc_numero == cuenta_puc.cntpuc_numero
                           select a.cntpuc_numero).Count();

                if (nom == 0)
                {
                    string primerDigito = cuenta_puc.cntpuc_numero.Substring(0, 1);
                    if (primerDigito == "1" || primerDigito == "2" || primerDigito == "3")
                    {
                        cuenta_puc.balance = true;
                    }
                    else if (primerDigito == "4" || primerDigito == "5" || primerDigito == "6" || primerDigito == "7")
                    {
                        cuenta_puc.estadoresultado = true;
                    }
                    else if (primerDigito == "8" || primerDigito == "9")
                    {
                        cuenta_puc.deorden = true;
                    }

                    if (cuenta_puc.cntpuc_numero.Length == 1)
                    {
                        cuenta_puc.clase = cuenta_puc.cntpuc_numero;
                    }
                    else if (cuenta_puc.cntpuc_numero.Length == 2)
                    {
                        cuenta_puc.clase = cuenta_puc.cntpuc_numero.Substring(0, 1);
                        cuenta_puc.grupo = cuenta_puc.cntpuc_numero;
                    }
                    else if (cuenta_puc.cntpuc_numero.Length == 4)
                    {
                        cuenta_puc.clase = cuenta_puc.cntpuc_numero.Substring(0, 1);
                        cuenta_puc.grupo = cuenta_puc.cntpuc_numero.Substring(0, 2);
                        cuenta_puc.cuenta = cuenta_puc.cntpuc_numero;
                    }
                    else if (cuenta_puc.cntpuc_numero.Length >= 6)
                    {
                        cuenta_puc.clase = cuenta_puc.cntpuc_numero.Substring(0, 1);
                        cuenta_puc.grupo = cuenta_puc.cntpuc_numero.Substring(0, 2);
                        cuenta_puc.cuenta = cuenta_puc.cntpuc_numero.Substring(0, 4);
                        cuenta_puc.subcuenta = cuenta_puc.cntpuc_numero.Substring(0, 6);
                    }

                    cuenta_puc.cntpucfec_creacion = DateTime.Now;
                    cuenta_puc.cntpucuserid_creacion = Convert.ToInt32(Session["user_usuarioid"]);
                    context.cuenta_puc.Add(cuenta_puc);
                    context.SaveChanges();
                    TempData["mensaje"] = "El registro de la nueva cuenta fue exitoso!";
                    //return RedirectToAction("Create");
                }
                else
                {
                    TempData["mensaje_error"] = "El numero de la cuenta que ingreso ya se encuentra, por favor valide!";
                }
            }

            ViewBag.mov_cnt = new SelectList(new List<SelectListItem>
            {
                new SelectListItem {Text = "Credito", Value = "Credito"},
                new SelectListItem {Text = "Debito", Value = "Debito"}
            });
            ViewBag.concepniff =
                new SelectList(context.tipo_niff, "tipo_niff_id", "tipo_niff_nombre", cuenta_puc.concepniff);

            var data = from c in context.cuenta_puc
                       orderby c.cntpuc_numero
                       where c.concepniff == 4 //tipo de cuenta solo niff
                       select new
                       {
                           id = c.cntpuc_id,
                           nombre = c.cntpuc_numero + " - " + c.cntpuc_descp
                       };
            List<SelectListItem> cuentasNiff = new List<SelectListItem>();
            foreach (var item in data)
            {
                cuentasNiff.Add(new SelectListItem
                {
                    Text = item.nombre,
                    Value = item.id.ToString()
                });
            }

            ViewBag.cuentaniff = cuentasNiff;

            IQueryable<icb_modulo_enlaces> enlacesBuscar = context.icb_modulo_enlaces.Where(x => x.enl_modulo == 71);
            string enlaces = "";
            foreach (icb_modulo_enlaces item in enlacesBuscar)
            {
                Menus buscarEnlace = context.Menus.FirstOrDefault(x => x.idMenu == item.id_modulo_destino);
                enlaces += "<li><a href='" + buscarEnlace.url + "'>" + buscarEnlace.nombreMenu + "</a></li>";
            }

            ViewBag.nombreEnlaces = enlaces;
            BuscarFavoritos(menu);
            return View(cuenta_puc);
        }

        // GET: cuenta_puc/Edit/5
        public ActionResult update(int? id, int? menu)
        {
            //valida si el id es null
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            cuenta_puc cuenta_puc = context.cuenta_puc.Find(id);
            if (cuenta_puc == null)
            {
                return HttpNotFound();
            }
            //consulta el nombre de usuario creacion con el id, lo envia a la vista a traves de ViewBag
            users creator = context.users.Find(cuenta_puc.cntpucuserid_creacion);

            ViewBag.user_nombre_cre = creator != null ? creator.user_nombre + " " + creator.user_apellido : "";

            //consulta el nombre de usuario actualizacion con el id, lo envia a la vista a traves de ViewBag
            users modificator = context.users.Find(cuenta_puc.cntpucuserid_actualizacion);
            if (modificator != null)
            {
                ViewBag.user_nombre_act = modificator.user_nombre + " " + modificator.user_apellido;
            }

            ViewBag.mov_cnt = new SelectList(new List<SelectListItem>
            {
                new SelectListItem {Text = "Credito", Value = "Credito"},
                new SelectListItem {Text = "Debito", Value = "Debito"}
            });
            ViewBag.tipoCuenta = cuenta_puc.mov_cnt;
            ViewBag.concepniff =
                new SelectList(context.tipo_niff, "tipo_niff_id", "tipo_niff_nombre", cuenta_puc.concepniff);

            var data = from c in context.cuenta_puc
                       orderby c.cntpuc_numero
                       where c.concepniff == 4 //tipo de cuenta solo niff
                       select new
                       {
                           id = c.cntpuc_id,
                           nombre = c.cntpuc_numero + " - " + c.cntpuc_descp
                       };
            List<SelectListItem> cuentasNiff = new List<SelectListItem>();
            foreach (var item in data)
            {
                cuentasNiff.Add(new SelectListItem
                {
                    Text = item.nombre,
                    Value = item.id.ToString()
                });
            }

            ViewBag.cuentaniff = cuentasNiff;
            ViewBag.cuentaniffSelected = cuenta_puc.cuentaniff;

            IQueryable<icb_modulo_enlaces> enlacesBuscar = context.icb_modulo_enlaces.Where(x => x.enl_modulo == 71);
            string enlaces = "";
            foreach (icb_modulo_enlaces item in enlacesBuscar)
            {
                Menus buscarEnlace = context.Menus.FirstOrDefault(x => x.idMenu == item.id_modulo_destino);
                enlaces += "<li><a href='" + buscarEnlace.url + "'>" + buscarEnlace.nombreMenu + "</a></li>";
            }

            ViewBag.nombreEnlaces = enlaces;
            BuscarFavoritos(menu);
            return View(cuenta_puc);
        }

        // POST: cuenta_puc/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult update(cuenta_puc cuenta_puc, int? menu)
        {
            if (ModelState.IsValid)
            {
                //consulta si el registro esta en BD teniendo en cuenta el id de consulta
                int nom = (from a in context.cuenta_puc
                           where a.cntpuc_numero == cuenta_puc.cntpuc_numero || a.cntpuc_id == cuenta_puc.cntpuc_id
                           select a.cntpuc_numero).Count();

                if (nom == 1)
                {
                    cuenta_puc.cntpucfec_actualizacion = DateTime.Now;
                    cuenta_puc.cntpucuserid_actualizacion = Convert.ToInt32(Session["user_usuarioid"]);

                    context.Entry(cuenta_puc).State = EntityState.Modified;
                    context.SaveChanges();
                    TempData["mensaje"] = "La actualización de la cuenta fue exitosa!";
                    ViewBag.mov_cnt = new SelectList(new List<SelectListItem>
                    {
                        new SelectListItem {Text = "Credito", Value = "Credito"},
                        new SelectListItem {Text = "Debito", Value = "Debito"}
                    });
                    ViewBag.tipoCuenta = cuenta_puc.mov_cnt;
                    ViewBag.concepniff = new SelectList(context.tipo_niff, "tipo_niff_id", "tipo_niff_nombre",
                        cuenta_puc.concepniff);

                    var datas = from c in context.cuenta_puc
                                orderby c.cntpuc_numero
                                where c.concepniff == 4 //tipo de cuenta solo niff
                                select new
                                {
                                    id = c.cntpuc_id,
                                    nombre = c.cntpuc_numero + " - " + c.cntpuc_descp
                                };
                    List<SelectListItem> cuentasNiffs = new List<SelectListItem>();
                    foreach (var item in datas)
                    {
                        cuentasNiffs.Add(new SelectListItem
                        {
                            Text = item.nombre,
                            Value = item.id.ToString()
                        });
                    }

                    ViewBag.cuentaniff = cuentasNiffs;
                    ViewBag.cuentaniffSelected = cuenta_puc.cuentaniff;

                    ConsultaDatosCreacion(cuenta_puc);
                    BuscarFavoritos(menu);
                    return View(cuenta_puc);
                }

                TempData["mensaje_error"] = "El registro que ingreso ya se encuentra, por favor valide!";
            }

            ViewBag.mov_cnt = new SelectList(new List<SelectListItem>
            {
                new SelectListItem {Text = "Credito", Value = "Credito"},
                new SelectListItem {Text = "Debito", Value = "Debito"}
            });
            ViewBag.tipoCuenta = cuenta_puc.mov_cnt;
            ViewBag.concepniff =
                new SelectList(context.tipo_niff, "tipo_niff_id", "tipo_niff_nombre", cuenta_puc.concepniff);

            var data = from c in context.cuenta_puc
                       orderby c.cntpuc_numero
                       where c.concepniff == 4 //tipo de cuenta solo niff
                       select new
                       {
                           id = c.cntpuc_id,
                           nombre = c.cntpuc_numero + " - " + c.cntpuc_descp
                       };
            List<SelectListItem> cuentasNiff = new List<SelectListItem>();
            foreach (var item in data)
            {
                cuentasNiff.Add(new SelectListItem
                {
                    Text = item.nombre,
                    Value = item.id.ToString()
                });
            }

            ViewBag.cuentaniff = cuentasNiff;
            ViewBag.cuentaniffSelected = cuenta_puc.cuentaniff;

            IQueryable<icb_modulo_enlaces> enlacesBuscar = context.icb_modulo_enlaces.Where(x => x.enl_modulo == 71);
            string enlaces = "";
            foreach (icb_modulo_enlaces item in enlacesBuscar)
            {
                Menus buscarEnlace = context.Menus.FirstOrDefault(x => x.idMenu == item.id_modulo_destino);
                enlaces += "<li><a href='" + buscarEnlace.url + "'>" + buscarEnlace.nombreMenu + "</a></li>";
            }

            ViewBag.nombreEnlaces = enlaces;
            ConsultaDatosCreacion(cuenta_puc);
            BuscarFavoritos(menu);
            return View(cuenta_puc);
        }

        public JsonResult BuscarCuentaConMovimiento(int id_cuenta)
        {
            mov_contable cuentasConMovimiento = context.mov_contable.FirstOrDefault(x => x.cuenta == id_cuenta);
            if (cuentasConMovimiento != null)
            {
                return Json(
                    new
                    {
                        cuentaTieneMovimientos = true,
                        errorMessage =
                            "La cuenta tiene movimientos contables asociados, por tanto no se puede desabilitar"
                    }, JsonRequestBehavior.AllowGet);
            }

            return Json(new { cuentaTieneMovimientos = false, errorMessage = "" }, JsonRequestBehavior.AllowGet);
        }

        public void ConsultaDatosCreacion(cuenta_puc cuenta_puc)
        {
            //consulta el nombre de usuario creacion con el id, lo envia a la vista a traves de ViewBag
            users creator = context.users.Find(cuenta_puc.cntpucuserid_creacion);
            ViewBag.user_nombre_cre = creator != null ? creator.user_nombre + " " + creator.user_apellido : "";

            users modificator = context.users.Find(cuenta_puc.cntpucuserid_actualizacion);
            if (modificator != null)
            {
                ViewBag.user_nombre_act = modificator.user_nombre + " " + modificator.user_apellido;
                ViewBag.user_fec_act = modificator.userfec_actualizacion.ToString();
            }
        }

        public JsonResult BuscarCuentasPUCPaginadas()
        {
            var data = context.cuenta_puc.ToList().Select(x => new
            {
                x.cntpuc_id,
                x.cntpuc_numero,
                x.cntpuc_descp,
                x.mov_cnt,
                cntpuc_estado = x.cntpuc_estado ? "Activo" : "Inactivo"
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