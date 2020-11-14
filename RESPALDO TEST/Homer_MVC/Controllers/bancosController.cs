using Homer_MVC.IcebergModel;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace Homer_MVC.Controllers
{
    public class bancosController : Controller
    {
        private readonly Iceberg_Context db = new Iceberg_Context();

        // GET: bancos/Create
        public ActionResult Create(int? menu)
        {
            var buscarTerceros = (from tercero in db.icb_terceros
                                  select new
                                  {
                                      id_tercero = tercero.tercero_id,
                                      nombre = "(" + tercero.doc_tercero + ") " + tercero.prinom_tercero + " " + tercero.segnom_tercero +
                                               " " + tercero.apellido_tercero + " " + tercero.segapellido_tercero + " " +
                                               tercero.razon_social
                                  }).ToList();
            ViewBag.nit = new SelectList(buscarTerceros, "id_tercero", "nombre");
            BuscarFavoritos(menu);
            return View(new bancos { estado = true });
        }

        // POST: bancos/Create
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(bancos bancos, int? menu)
        {
            if (ModelState.IsValid)
            {
                bancos buscarDato = db.bancos.FirstOrDefault(x => x.Descripcion == bancos.Descripcion);
                if (buscarDato == null)
                {
                    bancos.fec_creacion = DateTime.Now;
                    bancos.user_creacion = Convert.ToInt32(Session["user_usuarioid"]);
                    db.bancos.Add(bancos);
                    db.SaveChanges();
                    TempData["mensaje"] = "La creación del registro fue exitoso";
                }
                else
                {
                    TempData["mensaje_error"] = "El registro ingresado ya existe, por favor valide";
                }
            }

            var buscarTerceros = (from tercero in db.icb_terceros
                                  select new
                                  {
                                      id_tercero = tercero.tercero_id,
                                      nombre = "(" + tercero.doc_tercero + ") " + tercero.prinom_tercero + " " + tercero.segnom_tercero +
                                               " " + tercero.apellido_tercero + " " + tercero.segapellido_tercero + " " +
                                               tercero.razon_social
                                  }).ToList();
            ViewBag.nit = new SelectList(buscarTerceros, "id_tercero", "nombre", bancos.nit);
            BuscarFavoritos(menu);
            return View(bancos);
        }

        // GET: bancos/Edit/5
        public ActionResult Edit(int? id, int? menu)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            bancos bancos = db.bancos.Find(id);
            if (bancos == null)
            {
                return HttpNotFound();
            }

            BuscarFavoritos(menu);
            var buscarTerceros = (from tercero in db.icb_terceros
                                  select new
                                  {
                                      id_tercero = tercero.tercero_id,
                                      nombre = "(" + tercero.doc_tercero + ") " + tercero.prinom_tercero + " " + tercero.segnom_tercero +
                                               " " + tercero.apellido_tercero + " " + tercero.segapellido_tercero + " " +
                                               tercero.razon_social
                                  }).ToList();
            ViewBag.nit = new SelectList(buscarTerceros, "id_tercero", "nombre", bancos.nit);
            ConsultaDatosCreacion(bancos.id);
            return View(bancos);
        }

        // POST: bancos/Edit/5
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(bancos bancos, int? menu)
        {
            if (ModelState.IsValid)
            {
                bancos.fec_actualizacion = DateTime.Now;
                bancos.user_actualizacion = Convert.ToInt32(Session["user_usuarioid"]);
                db.Entry(bancos).State = EntityState.Modified;
                db.SaveChanges();
                TempData["mensaje"] = "La actualización del registro fue exitoso";
            }

            BuscarFavoritos(menu);
            var buscarTerceros = (from tercero in db.icb_terceros
                                  select new
                                  {
                                      id_tercero = tercero.tercero_id,
                                      nombre = "(" + tercero.doc_tercero + ") " + tercero.prinom_tercero + " " + tercero.segnom_tercero +
                                               " " + tercero.apellido_tercero + " " + tercero.segapellido_tercero + " " +
                                               tercero.razon_social
                                  }).ToList();
            ViewBag.nit = new SelectList(buscarTerceros, "id_tercero", "nombre", bancos.nit);
            ConsultaDatosCreacion(bancos.id);
            return View(bancos);
        }

        public void ConsultaDatosCreacion(int id)
        {
            //consulta el nombre de usuario creacion con el id, lo envia a la vista a traves de ViewBag
            users creador = (from c in db.bancos
                             join b in db.users on c.user_creacion equals b.user_id
                             where c.id == id
                             select b).FirstOrDefault();

            ViewBag.user_nombre_cre = creador != null ? creador.user_nombre + " " + creador.user_apellido : null;

            //consulta el nombre de usuario actualizacion con el id, lo envia a la vista a traves de ViewBag
            users actualizador = (from c in db.bancos
                                  join b in db.users on c.user_actualizacion equals b.user_id
                                  where c.id == id
                                  select b).FirstOrDefault();

            ViewBag.user_nombre_act =
                actualizador != null ? actualizador.user_nombre + " " + actualizador.user_apellido : null;
        }

        public JsonResult BuscarDatos()
        {
            var data = db.bancos.ToList().Select(x => new
            {
                x.comision,
                x.Descripcion,
                x.id,
                x.nit,
                x.numero_cuenta
            });
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public ActionResult cuentasBancarias(int? menu)
        {
            ViewBag.idBanco = new SelectList(db.bancos, "id", "Descripcion");
            BuscarFavoritos(menu);
            return View();
        }

        [HttpPost]
        public ActionResult cuentasBancarias(cuentasbancarias modelo, int? menu)
        {
            if (ModelState.IsValid)
            {
                cuentasbancarias buscarDato = db.cuentasbancarias.FirstOrDefault(x =>
                    x.idBanco == modelo.idBanco && x.numeroCuenta == modelo.numeroCuenta && x.cuenta == modelo.cuenta);
                if (buscarDato == null)
                {
                    modelo.fec_creacion = DateTime.Now;
                    modelo.userid_creacion = Convert.ToInt32(Session["user_usuarioid"]);
                    db.cuentasbancarias.Add(modelo);
                    db.SaveChanges();
                    TempData["mensaje"] = "La creación del registro fue exitoso";
                }
                else
                {
                    TempData["mensaje_error"] = "El registro ingresado ya existe, por favor valide";
                }
            }

            ViewBag.idBanco = new SelectList(db.bancos, "id", "Descripcion");
            BuscarFavoritos(menu);
            return View();
        }

        public ActionResult formasPago()
        {

            var listaB = (from b in db.bancos
                         select new
                         {
                             b.id,
                             nombre= b.Descripcion+" (" +b.numero_cuenta+")"
                         }).ToList();

            List<SelectListItem> lista = new List<SelectListItem>();
            foreach (var item in listaB)
            {
                lista.Add(new SelectListItem
                {
                    Text = item.nombre,
                    Value = item.id.ToString()
                });
            }
            ViewBag.Banco = lista;

            return View();
        }

        public JsonResult buscarFormasPago()
        {

            var formasPago = (from f in db.formas_pago
                              join b in db.bancos
                              on f.idbanco equals b.id into formas
                              from fd in formas.DefaultIfEmpty()
                              select new
                              {
                                  f.id,
                                  f.formapago,
                                  f.estado,
                                  banco = fd.Descripcion,
                                  cuenta = fd.numero_cuenta,
                                  f.razon_inactivo,
                              }).ToList();

            var data = formasPago.Select(d => new
            {
                d.id,
                d.formapago,
                d.estado,
                d.banco,
                d.cuenta,
                d.razon_inactivo,
            });

            return Json(data, JsonRequestBehavior.AllowGet);

        }

        public JsonResult agregarFormaPago(int? banco, string formapago,bool habilitado, string razon_inactivo)
        {
            var result = 0;
            formas_pago f = new formas_pago();

            f.formapago = formapago;
            f.idbanco = banco;
            f.estado = habilitado;

            if (!string.IsNullOrEmpty(razon_inactivo))
            {
                f.razon_inactivo = razon_inactivo;
            }

            db.formas_pago.Add(f);
            int resultado = db.SaveChanges();

            if (resultado > 0)
            {
                result = 1;
            }
            else
            {
                result = 0;
            }

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        // GET: bancos/Edit/5
        public ActionResult UpdateCuentasBancarias(int? id, int? menu)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            cuentasbancarias cuentasbancarias = db.cuentasbancarias.Find(id);
            if (cuentasbancarias == null)
            {
                return HttpNotFound();
            }

            BuscarFavoritos(menu);
            ViewBag.idBanco = new SelectList(db.bancos, "id", "Descripcion", cuentasbancarias.idBanco);
            ConsultaDatosCreacion(cuentasbancarias.id);
            return View(cuentasbancarias);
        }

        // POST: bancos/Edit/5
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UpdateCuentasBancarias(cuentasbancarias cuentasbancarias, int? menu)
        {
            if (ModelState.IsValid)
            {
                cuentasbancarias.fec_actualizacion = DateTime.Now;
                cuentasbancarias.userid_actualizacion = Convert.ToInt32(Session["user_usuarioid"]);
                db.Entry(cuentasbancarias).State = EntityState.Modified;
                db.SaveChanges();
                TempData["mensaje"] = "La actualización del registro fue exitoso";
            }

            BuscarFavoritos(menu);

            ConsultaDatosCreacion(cuentasbancarias.id);
            ViewBag.idBanco = new SelectList(db.bancos, "id", "Descripcion", cuentasbancarias.idBanco);
            return View(cuentasbancarias);
        }

        public JsonResult buscarCuentasBancarias()
        {
            var data = (from c in db.cuentasbancarias
                        join b in db.bancos
                            on c.idBanco equals b.id
                        select new
                        {
                            c.id,
                            b.Descripcion,
                            c.cuenta,
                            c.numeroCuenta,
                            estado = c.estado ? "Activo" : "Inactivo"
                        }).ToList();
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }

            base.Dispose(disposing);
        }

        public void BuscarFavoritos(int? menu)
        {
            int usuarioActual = Convert.ToInt32(Session["user_usuarioid"]);

            var buscarFavoritosSeleccionados = (from favoritos in db.favoritos
                                                join menu2 in db.Menus
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