using Homer_MVC.IcebergModel;
using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace Homer_MVC.Controllers
{
    public class empresaController : Controller
    {
        private readonly Iceberg_Context context = new Iceberg_Context();

        // GET: empresa
        public ActionResult Create(int? menu)
        {
            tablaempresa PrimeraEmpresa = context.tablaempresa.FirstOrDefault();


            ListasDesplegables(new tablaempresa());
            BuscarFavoritos(menu);
            return RedirectToAction("update", new { PrimeraEmpresa.id, menu });
            //  return View(new tablaempresa() { estado = true });
        }


        [HttpPost]
        public ActionResult Create(tablaempresa empresa, int? menu)
        {
            if (ModelState.IsValid)
            {
                tablaempresa buscarNombre = context.tablaempresa.FirstOrDefault(x =>
                    x.nombre_empresa == empresa.nombre_empresa || x.nit == empresa.nit);
                if (buscarNombre != null)
                {
                    TempData["mensaje_error"] =
                        "El nombre o nit de la empresa que ingreso ya se encuentra, por favor valide!";
                }
                else
                {
                    empresa.fec_creacion = DateTime.Now;
                    empresa.userid_creacion = Convert.ToInt32(Session["user_usuarioid"]);
                    context.tablaempresa.Add(empresa);
                    int guardar = context.SaveChanges();
                    if (guardar > 0)
                    {
                        TempData["mensaje"] = "El registro de la nueva empresa fue exitoso!";
                        return RedirectToAction("Create", new { menu });
                    }
                }
            }

            ListasDesplegables(empresa);
            BuscarFavoritos(menu);
            return View();
        }


        public ActionResult update(int? id, int? menu)
        {
            //valida si el id es null
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            tablaempresa empresa = context.tablaempresa.Find(id);
            if (empresa == null)
            {
                return HttpNotFound();
            }

            consultaDatosCreacion(empresa);
            ListasDesplegables(empresa);
            BuscarFavoritos(menu);
            return View(empresa);
        }

        [HttpPost]
        public ActionResult update(tablaempresa empresa, int? menu)
        {
            bool guardar = false;
            if (ModelState.IsValid)
            {
                tablaempresa buscarNombre = context.tablaempresa.FirstOrDefault(x =>
                    x.nombre_empresa == empresa.nombre_empresa || x.nit == empresa.nit);
                if (buscarNombre != null)
                {
                    if (buscarNombre.id != empresa.id)
                    {
                        TempData["mensaje_error"] =
                            "El nombre o nit de la empresa que ingreso ya se encuentra, por favor valide!";
                    }
                    else
                    {
                        buscarNombre.fec_actualizacion = DateTime.Now;
                        buscarNombre.user_idactualizacion = Convert.ToInt32(Session["user_usuarioid"]);
                        buscarNombre.nombre_empresa = empresa.nombre_empresa;
                        buscarNombre.nit = empresa.nit;
                        buscarNombre.digitoverificacion = empresa.digitoverificacion;
                        buscarNombre.perfiltributario = empresa.perfiltributario;
                        buscarNombre.razon_inactivo = empresa.razon_inactivo;
                        buscarNombre.estado = empresa.estado;
                        buscarNombre.cuenta_utilidad = empresa.cuenta_utilidad;
                        buscarNombre.cuenta_inicial = empresa.cuenta_inicial;
                        buscarNombre.cuenta_final = empresa.cuenta_final;
                        empresa.fec_actualizacion = DateTime.Now;
                        empresa.user_idactualizacion = Convert.ToInt32(Session["user_usuarioid"]);
                        context.Entry(buscarNombre).State = EntityState.Modified;
                    }
                }
                else
                {
                    empresa.fec_actualizacion = DateTime.Now;
                    empresa.user_idactualizacion = Convert.ToInt32(Session["user_usuarioid"]);
                    context.Entry(empresa).State = EntityState.Modified;
                }

                guardar = context.SaveChanges() > 0;
                if (guardar)
                {
                    TempData["mensaje"] = "El registro de la empresa se actualizo correctamente!";
                }
            }

            consultaDatosCreacion(empresa);
            ListasDesplegables(empresa);
            BuscarFavoritos(menu);
            return View(empresa);
        }


        public void consultaDatosCreacion(tablaempresa modelo)
        {
            //consulta el nombre de usuario creacion con el id, lo envia a la vista a traves de ViewBag
            users creador = (from c in context.users
                             join b in context.tablaempresa on c.user_id equals b.userid_creacion
                             where b.userid_creacion == modelo.userid_creacion
                             select c).FirstOrDefault();
            ViewBag.user_nombre_cre = creador != null ? creador.user_nombre + " " + creador.user_apellido : null;

            //consulta el nombre de usuario actualizacion con el id, lo envia a la vista a traves de ViewBag
            users actualizador = (from c in context.users
                                  join b in context.tablaempresa on c.user_id equals b.user_idactualizacion
                                  where b.user_idactualizacion == modelo.user_idactualizacion
                                  select c).FirstOrDefault();
            ViewBag.user_nombre_act =
                actualizador != null ? actualizador.user_nombre + " " + actualizador.user_apellido : null;
        }


        public JsonResult BuscarEmpresas()
        {
            var buscarEmpresas = (from empresa in context.tablaempresa
                                  select new
                                  {
                                      empresa.id,
                                      empresa.nombre_empresa,
                                      empresa.nit,
                                      empresa.digitoverificacion,
                                      empresa.direccion,
                                      empresa.telefono,
                                      celular = empresa.celular != null ? empresa.celular : "",
                                      estado = empresa.estado ? "Activo" : "Inactivo"
                                  }).ToList();
            return Json(buscarEmpresas, JsonRequestBehavior.AllowGet);
        }


        public void ListasDesplegables(tablaempresa empresa)
        {
            var buscarCuentasPuc = (from cuentas in context.cuenta_puc
                                    select new
                                    {
                                        cuentas.cntpuc_id,
                                        cuenta = "(" + cuentas.cntpuc_numero + ") " + cuentas.cntpuc_descp
                                    }).ToList();
            ViewBag.cuenta_utilidad = new SelectList(buscarCuentasPuc.OrderBy(x => x.cuenta), "cntpuc_id", "cuenta",
                empresa.cuenta_utilidad);
            ViewBag.cuenta_inicial = new SelectList(buscarCuentasPuc, "cntpuc_id", "cuenta", empresa.cuenta_inicial);
            ViewBag.cuenta_final = new SelectList(buscarCuentasPuc, "cntpuc_id", "cuenta", empresa.cuenta_final);
            ViewBag.perfiltributario = new SelectList(context.tpregimen_tercero.OrderBy(x => x.tpregimen_nombre),
                "tpregimen_id", "tpregimen_nombre", empresa.perfiltributario);
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