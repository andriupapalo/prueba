using Homer_MVC.IcebergModel;
using System;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace Homer_MVC.Controllers
{
    public class FinancierasController : Controller
    {
        private readonly Iceberg_Context context = new Iceberg_Context();
        CultureInfo miCultura = new CultureInfo("is-IS");

        // GET:
        public ActionResult Crear(int? menu)
        {
            var buscarTerceros = (from terceros in context.icb_terceros
                                  select new
                                  {
                                      id_tercero = terceros.tercero_id,
                                      nombres = "(" + terceros.doc_tercero + ") " + terceros.prinom_tercero + " " +
                                                terceros.segnom_tercero + " " + terceros.apellido_tercero + " " +
                                                terceros.segapellido_tercero + " " + terceros.razon_social
                                  }).ToList();
            ViewBag.Nit = new SelectList(buscarTerceros, "id_tercero", "nombres");
            BuscarFavoritos(menu);
            return View(new icb_unidad_financiera { financiera_estado = true });
        }

        // POST: gen_tercero/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Crear(icb_unidad_financiera financiera, int? menu)
        {
            if (ModelState.IsValid)
            {
                string monrcom2 = Request["valor_comision_monto1"];
                string porcom2 = Request["valor_comision1"];
                string tipcom = Request["tipocomision"];
                //consulta si el registro esta en BD
                int nom = (from a in context.icb_unidad_financiera
                           where a.codigo == financiera.codigo || a.Nit == financiera.Nit
                           select a.codigo).Count();

                if (nom == 0)
                {
                    financiera.financiera_fecela = DateTime.Now;
                    financiera.financiera_usuela = Convert.ToInt32(Session["user_usuarioid"]);
                    financiera.tipocomision = Request["tipocomision"];
                    if (!string.IsNullOrEmpty(monrcom2) && tipcom == "0")
                    {
                        financiera.valor_comision_monto = Convert.ToDecimal(Request["valor_comision_monto1"], miCultura);
                        financiera.valor_comision = null;
                    }

                    if (!string.IsNullOrEmpty(porcom2) && tipcom == "1")
                    {
                        financiera.valor_comision_monto = null;
                        financiera.valor_comision = Convert.ToDecimal(porcom2, new CultureInfo("is-IS"));
                    }
                    //if (!String.IsNullOrEmpty(monrcom2))
                    //{
                    //    financiera.valor_comision_monto = Convert.ToDecimal(Request["valor_comision_monto1"]);
                    //}
                    //if (!String.IsNullOrEmpty(porcom2))
                    //{
                    //    financiera.valor_comision = Convert.ToDecimal(Request["valor_comision1"]);
                    //}

                    context.icb_unidad_financiera.Add(financiera);
                    context.SaveChanges();
                    TempData["mensaje"] = "El registro de la nueva financiera fue exitoso!";
                    var buscarTerceros0 = (from terceros in context.icb_terceros
                                           select new
                                           {
                                               id_tercero = terceros.tercero_id,
                                               nombres = "(" + terceros.doc_tercero + ") " + terceros.prinom_tercero + " " +
                                                         terceros.segnom_tercero + " " + terceros.apellido_tercero + " " +
                                                         terceros.segapellido_tercero + " " + terceros.razon_social
                                           }).ToList();
                    ViewBag.Nit = new SelectList(buscarTerceros0, "id_tercero", "nombres");
                    return RedirectToAction("Crear", new { menu });
                }

                TempData["mensaje_error"] = "El registro que ingreso ya se encuentra, por favor valide!";
            }
            else
            {
                TempData["mensaje_error"] = "Error al registrar los datos ingresados, por valor valide";
            }

            BuscarFavoritos(menu);
            var buscarTerceros = (from terceros in context.icb_terceros
                                  select new
                                  {
                                      id_tercero = terceros.tercero_id,
                                      nombres = "(" + terceros.doc_tercero + ") " + terceros.prinom_tercero + " " +
                                                terceros.segnom_tercero + " " + terceros.apellido_tercero + " " +
                                                terceros.segapellido_tercero + " " + terceros.razon_social
                                  }).ToList();
            ViewBag.Nit = new SelectList(buscarTerceros, "id_tercero", "nombres");

            return View(financiera);
        }

        // GET:
        public ActionResult Editar(int? id, int? menu)
        {
            //valida si el id es null
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            icb_unidad_financiera financiera = context.icb_unidad_financiera.Find(id);
            if (financiera == null)
            {
                return HttpNotFound();
            }
            //consulta el nombre de usuario creacion con el id, lo envia a la vista a traves de ViewBag
            IQueryable<string> result = from a in context.users
                                        join b in context.icb_unidad_financiera on a.user_id equals b.financiera_usuela
                                        where b.financiera_usuela == id
                                        select a.user_nombre;
            foreach (string i in result)
            {
                ViewBag.user_nombre_cre = i;
            }
            //consulta el nombre de usuario actualizacion con el id, lo envia a la vista a traves de ViewBag
            IQueryable<string> result1 = from a in context.users
                                         join b in context.icb_unidad_financiera on a.user_id equals b.financiera_usuario_actualizacion
                                         where b.financiera_id == id
                                         select a.user_nombre;
            foreach (string i in result1)
            {
                ViewBag.user_nombre_act = i;
            }

            BuscarFavoritos(menu);
            var buscarTerceros = (from terceros in context.icb_terceros
                                  select new
                                  {
                                      id_tercero = terceros.tercero_id,
                                      nombres = "(" + terceros.doc_tercero + ") " + terceros.prinom_tercero + " " +
                                                terceros.segnom_tercero + " " + terceros.apellido_tercero + " " +
                                                terceros.segapellido_tercero + " " + terceros.razon_social
                                  }).ToList();
            ViewBag.Nit = new SelectList(buscarTerceros, "id_tercero", "nombres", financiera.Nit);
            ViewBag.Tipo = financiera.tipocomision;
            ViewBag.MonCom = financiera.valor_comision_monto;
            ViewBag.PorCom = financiera.valor_comision;
            //ViewBag.Com = Convert.ToInt32(financiera.valor_comision);
            return View(financiera);
        }

        // POST: 
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Editar(icb_unidad_financiera financiera, int? menu)
        {
            if (ModelState.IsValid)
            {
                string monrcom2 = Request["valor_comision_monto1"];
                string porcom2 = Request["valor_comision1"];
                string tipcom = Request["tipocomision"];
                //consulta si el registro esta en BD teniendo en cuenta el id de consulta
                icb_unidad_financiera nom = (from a in context.icb_unidad_financiera
                                             where a.codigo == financiera.codigo || a.financiera_id == financiera.financiera_id
                                             select a).FirstOrDefault();

                if (nom.financiera_id == financiera.financiera_id)
                {
                    financiera.financiera_fecha_actualizacion = DateTime.Now;
                    financiera.financiera_usuario_actualizacion = Convert.ToInt32(Session["user_usuarioid"]);
                    nom.financiera_fecha_actualizacion = DateTime.Now;
                    nom.financiera_usuario_actualizacion = Convert.ToInt32(Session["user_usuarioid"]);
                    nom.financiera_nombre = financiera.financiera_nombre;
                    nom.Nit = financiera.Nit;
                    if (!string.IsNullOrEmpty(monrcom2) && tipcom == "0")
                    {
                        nom.valor_comision_monto = Convert.ToDecimal(Request["valor_comision_monto1"], miCultura);
                        nom.valor_comision = null;
                    }

                    if (!string.IsNullOrEmpty(porcom2) && tipcom == "1")
                    {
                        nom.valor_comision_monto = null;
                        nom.valor_comision = Convert.ToDecimal(porcom2, new CultureInfo("is-IS"));
                    }

                    nom.tipocomision = Request["tipocomision"];
                    nom.financiera_estado = financiera.financiera_estado;
                    nom.financiera_razon_inantivo = financiera.financiera_razon_inantivo;
                    context.Entry(nom).State = EntityState.Modified;
                    context.SaveChanges();
                    TempData["mensaje"] = "La actualización del registro fue exitoso!";
                }
                else
                {
                    TempData["mensaje_error"] = "El registro que ingreso ya se encuentra, por favor valide";
                }
            }

            BuscarFavoritos(menu);
            var buscarTerceros = (from terceros in context.icb_terceros
                                  select new
                                  {
                                      id_tercero = terceros.tercero_id,
                                      nombres = "(" + terceros.prinom_tercero + " " + terceros.segnom_tercero + " " +
                                                terceros.apellido_tercero + " " + terceros.segapellido_tercero + " " +
                                                terceros.razon_social
                                  }).ToList();
            ViewBag.Nit = new SelectList(buscarTerceros, "id_tercero", "nombres", financiera.Nit);
            ViewBag.Tipo = financiera.tipocomision;
            ViewBag.MonCom = financiera.valor_comision_monto;
            ViewBag.PorCom = financiera.valor_comision;
            return View(financiera);
        }

        public JsonResult AgregarFinanciera
        (
            string codigo,
            string nombre,
            int? nit,
            decimal? valor_comision,
            decimal? valor_comision_monto,
            // bool estado,
            string financiera_razon_inantivo,
            string tipocomision
        )
        {
            bool result = false;
            if (nit > 0)
            {
                icb_unidad_financiera buscarFinanciera =
                    context.icb_unidad_financiera.FirstOrDefault(c => c.Nit == nit && c.codigo == codigo);
                if (buscarFinanciera != null)
                {
                    buscarFinanciera.financiera_fecha_actualizacion = DateTime.Now;
                    buscarFinanciera.financiera_usuario_actualizacion = Convert.ToInt32(Session["user_usuarioid"]);
                    buscarFinanciera.financiera_nombre = nombre;
                    buscarFinanciera.Nit = nit;
                    if (tipocomision == "0")
                    {
                        buscarFinanciera.valor_comision_monto = Convert.ToInt32(valor_comision_monto);
                    }

                    if (tipocomision == "1")
                    {
                        buscarFinanciera.valor_comision = Convert.ToInt32(valor_comision);
                    }
                    //buscarFinanciera.valor_comision = Convert.ToInt32(valor_comision);
                    //buscarFinanciera.valor_comision_monto = Convert.ToInt32(valor_comision_monto);
                    // buscarFinanciera.financiera_estado = estado;
                    buscarFinanciera.financiera_razon_inantivo = financiera_razon_inantivo;
                    buscarFinanciera.tipocomision = tipocomision;
                    //context.Entry(buscarFinanciera).State = EntityState.Modified;
                    //context.SaveChanges();
                    //TempData["mensaje"] = "La actualización del registro fue exitoso!";

                    context.Entry(buscarFinanciera).State = EntityState.Modified;
                    int actualizar = context.SaveChanges();
                    TempData["mensaje"] = "La actualización del registro fue exitoso!";
                    if (actualizar > 0)
                    {
                        result = true;
                        return Json(result, JsonRequestBehavior.AllowGet);
                    }
                }
                else
                {
                    if (tipocomision == "0")
                    {
                        context.icb_unidad_financiera.Add(new icb_unidad_financiera
                        {
                            financiera_nombre = nombre,
                            financiera_usuela = Convert.ToInt32(Session["user_usuarioid"]),
                            financiera_fecela = DateTime.Now,
                            financiera_usuario_actualizacion = Convert.ToInt32(Session["user_usuarioid"]),
                            financiera_fecha_actualizacion = DateTime.Now,
                            //   financiera_estado = estado,
                            financiera_razon_inantivo = financiera_razon_inantivo,
                            // financiera_licencia = 
                            codigo = codigo,
                            Nit = nit
                            //  valor_comision_monto = Convert.ToInt32(valor_comision_monto),
                            // tipocomision = tipocomision,
                        });
                        int guardar = context.SaveChanges();
                        TempData["mensaje"] = "El registro de la nueva financiera fue exitoso!";
                        if (guardar > 0)
                        {
                            result = true;
                            return Json(result, JsonRequestBehavior.AllowGet);
                        }
                    }

                    if (tipocomision == "1")
                    {
                        context.icb_unidad_financiera.Add(new icb_unidad_financiera
                        {
                            financiera_nombre = nombre,
                            financiera_usuela = Convert.ToInt32(Session["user_usuarioid"]),
                            financiera_fecela = DateTime.Now,
                            financiera_usuario_actualizacion = Convert.ToInt32(Session["user_usuarioid"]),
                            financiera_fecha_actualizacion = DateTime.Now,
                            //   financiera_estado = estado,
                            financiera_razon_inantivo = financiera_razon_inantivo,
                            valor_comision = Convert.ToInt32(valor_comision),
                            // financiera_licencia = 
                            codigo = codigo,
                            Nit = nit,
                            tipocomision = tipocomision
                        });
                        int guardar1 = context.SaveChanges();
                        TempData["mensaje"] = "El registro de la nueva financiera fue exitoso!";
                        if (guardar1 > 0)
                        {
                            result = true;
                            return Json(result, JsonRequestBehavior.AllowGet);
                        }
                    }
                }
            }

            result = false;
            return Json(result, JsonRequestBehavior.AllowGet);
        }


        public JsonResult BuscarDatos()
        {
            var data = context.icb_unidad_financiera.ToList().Select(x => new
            {
                id = x.financiera_id,
                x.codigo,
                nombre = x.financiera_nombre,
                nit = x.icb_terceros.doc_tercero,
                comision = x.tipocomision == "1" ? x.valor_comision : x.valor_comision_monto != null ? x.valor_comision_monto.Value : 0,
                estado = x.financiera_estado ? "Activo" : "Inactivo",
                x.tipocomision
            });
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public JsonResult buscarcodigo(string codigo)
        {
            if (string.IsNullOrWhiteSpace(codigo))
            {
                return Json(0);
            }

            int codigoexiste = context.icb_unidad_financiera.Where(d => d.codigo == codigo).Count();
            if (codigoexiste > 0)
            {
                return Json(true);
            }

            return Json(false);
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