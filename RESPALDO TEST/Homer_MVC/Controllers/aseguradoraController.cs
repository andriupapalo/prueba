using Homer_MVC.IcebergModel;
using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace Homer_MVC.Controllers
{
    public class aseguradoraController : Controller
    {
        private readonly Iceberg_Context context = new Iceberg_Context();

        // GET: aseguradora
        public ActionResult Create(int? menu)
        {
            var proveedores = (from terceros in context.icb_terceros
                               join proveedor in context.tercero_proveedor
                                   on terceros.tercero_id equals proveedor.tercero_id
                               select new
                               {
                                   terceros.tercero_id,
                                   nombre = terceros.prinom_tercero + " " + terceros.segnom_tercero + " " + terceros.apellido_tercero +
                                            " " + terceros.segapellido_tercero + " " + terceros.razon_social
                               }).ToList();
            ViewBag.idtercero = new SelectList(proveedores, "tercero_id", "nombre");

            ViewBag.documento_id = context.ttipodocaseguradora.OrderBy(x => x.documento).ToList();
            BuscarFavoritos(menu);
            return View(new icb_aseguradoras { estado = true });
        }


        // POST: aseguradora
        [HttpPost]
        public ActionResult Create(icb_aseguradoras modelo, int? menu)
        {
            string documentosSeleccionados = Request["documento_id"];
            if (ModelState.IsValid)
            {
                icb_aseguradoras buscarNombreAseguradora = context.icb_aseguradoras.FirstOrDefault(x => x.nombre == modelo.nombre);
                if (buscarNombreAseguradora != null)
                {
                    TempData["mensaje_error"] =
                        "El nombre de la aseguradora ya esta registrado, por favor verifique...";
                }
                else
                {
                    modelo.userid_creacion = Convert.ToInt32(Session["user_usuarioid"]);
                    modelo.fec_creacion = DateTime.Now;
                    context.icb_aseguradoras.Add(modelo);
                    int guardar = context.SaveChanges();

                    if (guardar > 0)
                    {
                        icb_aseguradoras consultarUltimaAseguradora =
                            context.icb_aseguradoras.OrderByDescending(x => x.aseg_id).FirstOrDefault();
                        if (!string.IsNullOrEmpty(documentosSeleccionados))
                        {
                            string[] documentosId = documentosSeleccionados.Split(',');
                            foreach (string substring in documentosId)
                            {
                                context.tdocaseguradora.Add(new tdocaseguradora
                                {
                                    iddocumento = Convert.ToInt32(substring),
                                    idaseguradora = consultarUltimaAseguradora != null
                                        ? consultarUltimaAseguradora.aseg_id
                                        : 0,
                                    fec_creacion = DateTime.Now,
                                    userid_creacion = Convert.ToInt32(Session["user_usuarioid"])
                                });
                            }

                            int guardarDocumentos = context.SaveChanges();
                            if (guardarDocumentos > 0)
                            {
                                TempData["mensaje"] = "La aseguradora se ha agregado exitosamente";
                            }
                            else
                            {
                                TempData["mensaje_error"] =
                                    "Error de conexion con base de datos, por favor verifique...";
                            }
                        }
                    }
                    else
                    {
                        TempData["mensaje_error"] = "Error de conexion con base de datos, por favor verifique...";
                    }
                }
            }

            var proveedores = (from terceros in context.icb_terceros
                               join proveedor in context.tercero_proveedor
                                   on terceros.tercero_id equals proveedor.tercero_id
                               select new
                               {
                                   terceros.tercero_id,
                                   nombre = terceros.prinom_tercero + " " + terceros.segnom_tercero + " " + terceros.apellido_tercero +
                                            " " + terceros.segapellido_tercero + " " + terceros.razon_social
                               }).ToList();
            ViewBag.idtercero = new SelectList(proveedores, "tercero_id", "nombre", modelo.idtercero);

            ViewBag.documento_id = context.ttipodocaseguradora.OrderBy(x => x.documento).ToList();
            ViewBag.documentosSeleccionados = documentosSeleccionados;
            BuscarFavoritos(menu);
            return View(new icb_aseguradoras { estado = true });
        }


        public ActionResult Edit(int? id, int? menu)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            icb_aseguradoras buscarAseguradora = context.icb_aseguradoras.Find(id);
            if (buscarAseguradora == null)
            {
                return HttpNotFound();
            }

            var proveedores = (from terceros in context.icb_terceros
                               join proveedor in context.tercero_proveedor
                                   on terceros.tercero_id equals proveedor.tercero_id
                               select new
                               {
                                   terceros.tercero_id,
                                   nombre = terceros.prinom_tercero + " " + terceros.segnom_tercero + " " + terceros.apellido_tercero +
                                            " " + terceros.segapellido_tercero + " " + terceros.razon_social
                               }).ToList();
            ViewBag.idtercero = new SelectList(proveedores, "tercero_id", "nombre", buscarAseguradora.idtercero);

            ViewBag.documento_id = context.ttipodocaseguradora.OrderBy(x => x.documento).ToList();

            var buscarDocumentos = from documentos in context.tdocaseguradora
                                   where documentos.idaseguradora == id
                                   select new { documentos.iddocumento };
            string documentosString = "";
            bool primera = true;
            foreach (var item in buscarDocumentos)
            {
                if (primera)
                {
                    documentosString += item.iddocumento;
                    primera = !primera;
                }
                else
                {
                    documentosString += "," + item.iddocumento;
                }
            }

            ViewBag.documentosSeleccionados = documentosString;
            ConsultaDatosCreacion(buscarAseguradora);
            BuscarFavoritos(menu);
            return View(buscarAseguradora);
        }


        [HttpPost]
        public ActionResult Edit(icb_aseguradoras modelo, int? menu)
        {
            string documentosSeleccionados = Request["documento_id"];
            if (ModelState.IsValid)
            {
                icb_aseguradoras buscarNombreAseguradora = context.icb_aseguradoras.FirstOrDefault(x => x.nombre == modelo.nombre);
                if (buscarNombreAseguradora != null)
                {
                    if (buscarNombreAseguradora.aseg_id != modelo.aseg_id)
                    {
                        TempData["mensaje_error"] =
                            "El nombre de la aseguradora ya esta registrado, por favor verifique...";
                    }
                    else
                    {
                        buscarNombreAseguradora.fec_actualizacion = DateTime.Now;
                        buscarNombreAseguradora.user_idactualizacion = Convert.ToInt32(Session["user_usuarioid"]);
                        buscarNombreAseguradora.nombre = modelo.nombre;
                        buscarNombreAseguradora.estado = modelo.estado;
                        buscarNombreAseguradora.razon_inactivo = modelo.razon_inactivo;
                        buscarNombreAseguradora.idtercero = modelo.idtercero;
                        modelo.user_idactualizacion = Convert.ToInt32(Session["user_usuarioid"]);
                        modelo.fec_actualizacion = DateTime.Now;
                        context.Entry(buscarNombreAseguradora).State = EntityState.Modified;
                        int guardar = context.SaveChanges();

                        if (guardar > 0)
                        {
                            const string query = "DELETE FROM [dbo].[tdocaseguradora] WHERE [idaseguradora]={0}";
                            int rows = context.Database.ExecuteSqlCommand(query, modelo.aseg_id);
                            if (!string.IsNullOrEmpty(documentosSeleccionados))
                            {
                                string[] documentosId = documentosSeleccionados.Split(',');
                                foreach (string substring in documentosId)
                                {
                                    context.tdocaseguradora.Add(new tdocaseguradora
                                    {
                                        iddocumento = Convert.ToInt32(substring),
                                        idaseguradora = modelo.aseg_id,
                                        fec_creacion = DateTime.Now,
                                        userid_creacion = Convert.ToInt32(Session["user_usuarioid"])
                                    });
                                }

                                int guardarDocumentos = context.SaveChanges();
                                if (guardarDocumentos > 0)
                                {
                                    TempData["mensaje"] = "La aseguradora se ha agregado exitosamente";
                                }
                                else
                                {
                                    TempData["mensaje_error"] =
                                        "Error de conexion con base de datos, por favor verifique...";
                                }
                            }
                        }
                    }
                }
                else
                {
                    icb_aseguradoras buscarAseguradora = context.icb_aseguradoras.FirstOrDefault(x => x.aseg_id == modelo.aseg_id);
                    buscarAseguradora.fec_actualizacion = DateTime.Now;
                    buscarAseguradora.user_idactualizacion = Convert.ToInt32(Session["user_usuarioid"]);
                    buscarAseguradora.nombre = modelo.nombre;
                    buscarAseguradora.estado = modelo.estado;
                    buscarAseguradora.razon_inactivo = modelo.razon_inactivo;
                    buscarAseguradora.idtercero = modelo.idtercero;
                    modelo.user_idactualizacion = Convert.ToInt32(Session["user_usuarioid"]);
                    modelo.fec_actualizacion = DateTime.Now;
                    context.Entry(buscarAseguradora).State = EntityState.Modified;
                    int guardar = context.SaveChanges();

                    if (guardar > 0)
                    {
                        const string query = "DELETE FROM [dbo].[tdocaseguradora] WHERE [idaseguradora]={0}";
                        int rows = context.Database.ExecuteSqlCommand(query, modelo.aseg_id);
                        if (!string.IsNullOrEmpty(documentosSeleccionados))
                        {
                            string[] documentosId = documentosSeleccionados.Split(',');
                            foreach (string substring in documentosId)
                            {
                                context.tdocaseguradora.Add(new tdocaseguradora
                                {
                                    iddocumento = Convert.ToInt32(substring),
                                    idaseguradora = modelo.aseg_id,
                                    fec_creacion = DateTime.Now,
                                    userid_creacion = Convert.ToInt32(Session["user_usuarioid"])
                                });
                            }

                            int guardarDocumentos = context.SaveChanges();
                            if (guardarDocumentos > 0)
                            {
                                TempData["mensaje"] = "La aseguradora se ha agregado exitosamente";
                            }
                            else
                            {
                                TempData["mensaje_error"] =
                                    "Error de conexion con base de datos, por favor verifique...";
                            }
                        }
                    }
                    else
                    {
                        TempData["mensaje_error"] = "Error de conexion con base de datos, por favor verifique...";
                    }
                }
            }

            var proveedores = (from terceros in context.icb_terceros
                               join proveedor in context.tercero_proveedor
                                   on terceros.tercero_id equals proveedor.tercero_id
                               select new
                               {
                                   terceros.tercero_id,
                                   nombre = terceros.prinom_tercero + " " + terceros.segnom_tercero + " " + terceros.apellido_tercero +
                                            " " + terceros.segapellido_tercero + " " + terceros.razon_social
                               }).ToList();
            ViewBag.idtercero = new SelectList(proveedores, "tercero_id", "nombre", modelo.idtercero);

            ViewBag.documento_id = context.ttipodocaseguradora.OrderBy(x => x.documento).ToList();
            ViewBag.documentosSeleccionados = documentosSeleccionados;
            ConsultaDatosCreacion(modelo);
            BuscarFavoritos(menu);
            return View(modelo);
        }


        public void ConsultaDatosCreacion(icb_aseguradoras aseguradora)
        {
            //consulta el nombre de usuario creacion con el id, lo envia a la vista a traves de ViewBag
            users creator = context.users.Find(aseguradora.userid_creacion);
            if (creator != null)
            {
                ViewBag.user_nombre_cre = creator.user_nombre + " " + creator.user_apellido;
            }

            users modificator = context.users.Find(aseguradora.user_idactualizacion);
            if (modificator != null)
            {
                ViewBag.user_nombre_act = modificator.user_nombre + " " + modificator.user_apellido;
                ViewBag.user_fec_act = modificator.userfec_actualizacion.ToString();
            }
        }


        public JsonResult BuscarAseguradoras()
        {
            var buscarAseguradoras = (from aseguradoras in context.icb_aseguradoras
                                      join proveedores in context.icb_terceros
                                          on aseguradoras.idtercero equals proveedores.tercero_id
                                      select new
                                      {
                                          aseguradoras.aseg_id,
                                          aseguradoras.nombre,
                                          proveedor = proveedores.prinom_tercero + " " + proveedores.segnom_tercero + " " +
                                                      proveedores.apellido_tercero + " " + proveedores.segapellido_tercero + " " +
                                                      proveedores.razon_social,
                                          estado = aseguradoras.estado ? "Activo" : "Inactivo"
                                      }).ToList();

            return Json(buscarAseguradoras, JsonRequestBehavior.AllowGet);
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