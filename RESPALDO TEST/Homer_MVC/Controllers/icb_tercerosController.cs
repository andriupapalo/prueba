using DocumentFormat.OpenXml.Bibliography;
using Homer_MVC.IcebergModel;
using Homer_MVC.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace Homer_MVC.Controllers
{
    public class icb_tercerosController : Controller
    {
        private readonly Iceberg_Context context = new Iceberg_Context();

        public void PanelesActivos(int idTercero)
        {
            List<clasificacionTercero> buscarTiposTercero = context.clasificacionTercero.Where(x => x.tercero_id == idTercero).ToList();
            string idsTipos = "";
            bool inicial = false;
            foreach (clasificacionTercero item in buscarTiposTercero)
            {
                if (!inicial)
                {
                    idsTipos += item.tptercero_id;
                    inicial = true;
                }
                else
                {
                    idsTipos += "," + item.tptercero_id;
                }
            }

            ViewBag.tiposTercero = idsTipos;
        }

        // GET: tercero/Create
        [HttpGet]
        public ActionResult tercero(int? menu, string cedula)
        {
            ViewBag.cltercero_id = new SelectList(context.icb_tp_tercero, "cltercero_id", "cltercero_nombre");
            ViewBag.tpdoc_id = new SelectList(context.tp_documento, "tpdoc_id", "tpdoc_nombre");
            ViewBag.tipo_documento = new SelectList(context.tp_documento, "tpdoc_id", "tpdoc_nombre");
            ViewBag.genero_tercero = new SelectList(context.gen_tercero, "gentercero_id", "gentercero_nombre");
            ViewBag.tpregimen_id = new SelectList(context.tpregimen_tercero, "tpregimen_id", "tpregimen_nombre");
            DbSet<contributario> dataContributario = context.contributario;
            ViewBag.retfuente = new SelectList(dataContributario.OrderBy(x => x.descripcion), "codigo", "descripcion");
            ViewBag.retiva = new SelectList(dataContributario.OrderBy(x => x.descripcion), "codigo", "descripcion");
            ViewBag.retica = new SelectList(dataContributario.OrderBy(x => x.descripcion), "codigo", "descripcion");
            ViewBag.autorretencion = new SelectList(dataContributario.OrderBy(x => x.descripcion), "codigo", "descripcion");
            var actecos = context.acteco_tercero.Select(d => new { id = d.acteco_id, nombre = d.nroacteco_tercero + " - " + d.acteco_nombre }).ToList();
            ViewBag.id_acteco = new SelectList(actecos, "id", "nombre");

            BuscarFavoritos(menu);
            return View(new icb_terceros { doc_tercero = cedula });
        }

        // POST: tercero/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult tercero(icb_terceros terceros, int? menu)
        {
            ViewBag.tipo_documento = new SelectList(context.tp_documento, "tpdoc_id", "tpdoc_nombre");
            ViewBag.tpregimen_id = new SelectList(context.tpregimen_tercero, "tpregimen_id", "tpregimen_nombre",
                terceros.tpregimen_id);
            DbSet<contributario> dataContributario = context.contributario;
            ViewBag.retfuente = new SelectList(dataContributario.OrderBy(x => x.descripcion), "codigo", "descripcion");
            ViewBag.retiva = new SelectList(dataContributario.OrderBy(x => x.descripcion), "codigo", "descripcion");
            ViewBag.retica = new SelectList(dataContributario.OrderBy(x => x.descripcion), "codigo", "descripcion");
            ViewBag.autorretencion =
                new SelectList(dataContributario.OrderBy(x => x.descripcion), "codigo", "descripcion");
            var actecos = context.acteco_tercero.Select(d => new { id = d.acteco_id, nombre = d.nroacteco_tercero + " - " + d.acteco_nombre }).ToList();
            ViewBag.id_acteco = new SelectList(actecos, "id", "nombre", terceros.id_acteco);
            if (ModelState.IsValid)
            {
                if (terceros.tpdoc_id == 1 && string.IsNullOrEmpty(terceros.digito_verificacion))
                {
                    TempData["errorDigitoVerificacion"] = "Digito verificacion requerido!";
                    ViewBag.tpdoc_id =
                        new SelectList(context.tp_documento, "tpdoc_id", "tpdoc_nombre", terceros.tpdoc_id);
                    ViewBag.ciu_id = new SelectList(context.nom_ciudad, "ciu_id", "ciu_nombre", terceros.ciu_id);
                    ViewBag.genero_tercero = new SelectList(context.gen_tercero, "gentercero_id", "gentercero_nombre",
                        terceros.genero_tercero);
                    //       ViewBag.tpregimen_id = new SelectList(context.tpregimen_tercero, "tpregimen_id", "tpregimen_nombre", terceros.tpregimen_id);

                    BuscarFavoritos(menu);
                    return View();
                }

                //consulta si el registro documento esta en BD
                int nom = (from a in context.icb_terceros
                           where a.doc_tercero == terceros.doc_tercero
                                 && a.doc_tercero != null
                           select a.doc_tercero).Count();

                if (nom == 0)
                {
                    terceros.tercerofec_creacion = DateTime.Now;
                    terceros.tercerouserid_creacion = Convert.ToInt32(Session["user_usuarioid"]);
                    terceros.segnom_tercero = terceros.segnom_tercero != null ? terceros.segnom_tercero : "";
                    terceros.segapellido_tercero =
                        terceros.segapellido_tercero != null ? terceros.segapellido_tercero : "";
                    terceros.tpregimen_id = terceros.tpregimen_id;
                    terceros.retfuente = terceros.retfuente;
                    terceros.retiva = terceros.retiva;
                    terceros.retica = terceros.retica;
                    terceros.autorretencion = terceros.autorretencion;
                    terceros.cuenta_banco = terceros.cuenta_banco;
                    context.icb_terceros.Add(terceros);
                    context.SaveChanges();
                    int idtercero = context.icb_terceros.OrderByDescending(x => x.tercero_id).FirstOrDefault()
                        .tercero_id;

                    string listDirecciones = Request["lista_direcciones"];
                    if (!string.IsNullOrEmpty(listDirecciones))
                    {
                        int lista = Convert.ToInt32(listDirecciones);

                        for (int i = 1; i <= lista + 1; i++)
                        {
                            terceros_direcciones direcciones = new terceros_direcciones();

                            if (!string.IsNullOrEmpty(Request["ciudad" + i]))
                            {
                                direcciones.ciudad = Convert.ToInt32(Request["ciudad" + i]);
                                if (!string.IsNullOrEmpty(Request["sector" + i]))
                                {
                                    direcciones.sector = Convert.ToInt32(Request["sector" + i]);
                                }

                                direcciones.direccion = Request["direccion" + i];
                                direcciones.idtercero = idtercero;
                                context.terceros_direcciones.Add(direcciones);
                            }
                        }
                    }

                    try
                    {
                        context.SaveChanges();
                        //update(idtercero, menu);
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
                                TempData["mensaje_error"] = message;
                            }
                        }

                        return View();
                    }

                    TempData["mensaje"] = "El registro del nuevo tercero fue exitoso!";
                    ViewBag.tpdoc_id =
                        new SelectList(context.tp_documento, "tpdoc_id", "tpdoc_nombre", terceros.tpdoc_id);
                    ViewBag.genero_tercero = new SelectList(context.gen_tercero, "gentercero_id", "gentercero_nombre",
                        terceros.genero_tercero);
                    ViewBag.tpregimen_id = new SelectList(context.tpregimen_tercero, "tpregimen_id", "tpregimen_nombre",
                        terceros.tpregimen_id);
                    int tercero_id = context.icb_terceros.OrderByDescending(x => x.tercero_id).FirstOrDefault()
                        .tercero_id;
                    //return RedirectToAction("tercero", new {menu}); // cambio pedido por la ingeniera liliana
                    return RedirectToAction("update",
                        new
                        {
                            id = tercero_id,
                            menu
                        }); // despues volvio a pedir que se enviara a la actualizacion (13-08-2018)
                }

                TempData["mensaje_error"] = "El número de documento que ingreso ya se encuentra, por favor valide!";
            }
            else
            {
                string messages = string.Join("; ", ModelState.Values
                    .SelectMany(x => x.Errors)
                    .Select(x => x.ErrorMessage));
            }

            ViewBag.tpdoc_id = new SelectList(context.tp_documento, "tpdoc_id", "tpdoc_nombre", terceros.tpdoc_id);

            ViewBag.genero_tercero = new SelectList(context.gen_tercero, "gentercero_id", "gentercero_nombre",
                terceros.genero_tercero);
            BuscarFavoritos(menu);
            return View();
        }

        // GET: tercero/Edit/5
        public ActionResult update(int? id, int? menu)
        {
            ViewBag.tipo_documento = new SelectList(context.tp_documento, "tpdoc_id", "tpdoc_nombre");
            //valida si el id es null
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            icb_terceros terceros = context.icb_terceros.Find(id);
            PanelesActivos(id ?? 0);
            ViewBag.rol = Convert.ToInt32(Session["user_rolid"]);
            ViewBag.cltercero_id = new SelectList(context.icb_tp_tercero, "cltercero_id", "cltercero_nombre");
            ViewBag.tpdoc_id = new SelectList(context.tp_documento, "tpdoc_id", "tpdoc_nombre",
                terceros != null ? terceros.tpdoc_id : 0);
            ViewBag.tpregimen_id = new SelectList(context.tpregimen_tercero, "tpregimen_id", "tpregimen_nombre",
                terceros.tpregimen_id);
            var actecos = context.acteco_tercero.Select(d => new { id = d.acteco_id, nombre = d.nroacteco_tercero + " - " + d.acteco_nombre }).ToList();
            ViewBag.id_acteco = new SelectList(actecos, "id", "nombre", terceros.id_acteco);
            DbSet<contributario> dataContributario = context.contributario;
            ViewBag.retfuente = new SelectList(dataContributario.OrderBy(x => x.descripcion), "codigo", "descripcion");
            ViewBag.retiva = new SelectList(dataContributario.OrderBy(x => x.descripcion), "codigo", "descripcion");
            ViewBag.retica = new SelectList(dataContributario.OrderBy(x => x.descripcion), "codigo", "descripcion");
            ViewBag.autorretencion =
                new SelectList(dataContributario.OrderBy(x => x.descripcion), "codigo", "descripcion");
            if (terceros.ciu_id != null)
            {
                int? ciu_id = terceros.ciu_id;
                ViewBag.ciu_id = ciu_id;
                ViewBag.ciu_nombre = context.nom_ciudad.FirstOrDefault(x => x.ciu_id == ciu_id).ciu_nombre;
            }

            if (terceros.pais_id != null)
            {
                int? pais_id = terceros.pais_id;
                ViewBag.pais_id = pais_id;
                ViewBag.pais_nombre = context.nom_pais.FirstOrDefault(x => x.pais_id == pais_id).pais_nombre;
            }

            if (terceros.dpto_id != null)
            {
                int? dpto_id = terceros.dpto_id;
                ViewBag.dpto_id = dpto_id;
                ViewBag.dpto_nombre = context.nom_departamento.FirstOrDefault(x => x.dpto_id == dpto_id).dpto_nombre;
            }

            if (terceros.sector_id != null)
            {
                int? sector_id = terceros.sector_id;
                ViewBag.sector_id = sector_id;
                ViewBag.sector_nombre = context.nom_sector.FirstOrDefault(x => x.sec_id == sector_id).sec_nombre;
            }

            ViewBag.genero_tercero = new SelectList(context.gen_tercero, "gentercero_id", "gentercero_nombre",
                terceros.genero_tercero);
            //var direccion = context.icb_terceros.FirstOrDefault(x => x.direc_tercero == terceros.direc_tercero);
            //ViewBag.direccionR = direccion.direc_tercero != null ? direccion.direc_tercero : "";
            users usuarioCreador = context.users.FirstOrDefault(x => x.user_id == terceros.tercerouserid_creacion);
            ViewBag.user_nombre_cre = usuarioCreador.user_nombre + " " + usuarioCreador.user_apellido;
            users usuarioActualizador =
                context.users.FirstOrDefault(x => x.user_id == terceros.tercerouserid_actualizacion);
            ViewBag.user_nombre_act = usuarioActualizador != null
                ? usuarioActualizador.user_nombre + " " + usuarioActualizador.user_apellido
                : null;
            ViewBag.fechaNac = terceros.fec_nacimiento != null ? terceros.fec_nacimiento.Value.ToShortDateString() : "";
            ViewBag.tipo_contacto = new SelectList(context.tipocontactotercero, "idtipocontacto", "decripcion");
            BuscarFavoritos(menu);
            return View(terceros);
        }

        public void CamposListasDesplegables2(terceroClienteForm cliente)
        {


            ViewBag.edocivil_id = new SelectList(context.estado_civil, "edocivil_id", "edocivil_nombre", cliente.edocivil_id);
            ViewBag.tpocupacion_id = new SelectList(context.tp_ocupacion, "tpocupacion_id", "tpocupacion_nombre", cliente.tpocupacion_id);
            ViewBag.tphobby_id = new SelectList(context.tp_hobby, "tphobby_id", "tphobby_nombre", cliente.tphobby_id);
            ViewBag.tpdpte_id = new SelectList(context.tp_Dpte, "tpdpte_id", "tpdpte_nombre", cliente.tpdpte_id);
            ViewBag.cod_pago_id = new SelectList(context.fpago_tercero, "fpago_id", "fpago_nombre", cliente.cod_pago_id ?? 0);
            var session = Convert.ToInt32(Session["user_usuarioid"]);
            var busqueda = context.users.Where(x => x.user_id == session).FirstOrDefault();
            var busquedas1 = context.rolpermisos.Where(x => x.codigo == "P55").FirstOrDefault();
            var tipoc = new SelectList(context.tipocliente.Where(d => d.nombre.ToUpper() == "NORMAL"), "tipo", "nombre");

            var conteo = context.rolacceso.Where(x => x.idpermiso == busquedas1.id && x.idrol == busqueda.rol_id).Count();
            if (cliente.cltercero_id != null)
            {
                tipoc = new SelectList(context.tipocliente, "tipo", "nombre", cliente.tipo_cliente ?? 0);

            }
            else {
                tipoc = new SelectList(context.tipocliente.Where(d => d.nombre.ToUpper() == "NORMAL"), "tipo", "nombre");

            }
            if  (conteo == 0)
            {
                tipoc = new SelectList(context.tipocliente.Where(d => d.nombre.ToUpper() == "NORMAL"), "tipo", "nombre");
            }
            
            
            ViewBag.tipo_cliente = tipoc;
           
            ViewBag.idsegmentacion = new SelectList(context.segmentacion.Where(x => x.estado).OrderBy(x => x.descripcion), "id", "descripcion", cliente.idsegmentacion);
        }

        // POST: tercero/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult update(icb_terceros terceros, int? menu)
        {
            ViewBag.tipo_documento = new SelectList(context.tp_documento, "tpdoc_id", "tpdoc_nombre");
            ViewBag.tipo_contacto = new SelectList(context.tipocontactotercero, "idtipocontacto", "decripcion");
            ViewBag.tpregimen_id = new SelectList(context.tpregimen_tercero, "tpregimen_id", "tpregimen_nombre", terceros.tpregimen_id);
            var actecos = context.acteco_tercero.Select(d => new { id = d.acteco_id, nombre = d.nroacteco_tercero + " - " + d.acteco_nombre }).ToList();
            ViewBag.id_acteco = new SelectList(actecos, "id", "nombre", terceros.id_acteco);
            DbSet<contributario> dataContributario = context.contributario;
            ViewBag.retfuente = new SelectList(dataContributario.OrderBy(x => x.descripcion), "codigo", "descripcion");
            ViewBag.retiva = new SelectList(dataContributario.OrderBy(x => x.descripcion), "codigo", "descripcion");
            ViewBag.retica = new SelectList(dataContributario.OrderBy(x => x.descripcion), "codigo", "descripcion");
            ViewBag.autorretencion =
                new SelectList(dataContributario.OrderBy(x => x.descripcion), "codigo", "descripcion");

            if (terceros.ciu_id != null)
            {
                int? ciu_id = terceros.ciu_id;
                ViewBag.ciu_id = ciu_id;
                ViewBag.ciu_nombre = context.nom_ciudad.FirstOrDefault(x => x.ciu_id == ciu_id).ciu_nombre;
            }

            if (terceros.pais_id != null)
            {
                int? pais_id = terceros.pais_id;
                ViewBag.pais_id = pais_id;
                ViewBag.pais_nombre = context.nom_pais.FirstOrDefault(x => x.pais_id == pais_id).pais_nombre;
            }

            if (terceros.dpto_id != null)
            {
                int? dpto_id = terceros.dpto_id;
                ViewBag.dpto_id = dpto_id;
                ViewBag.dpto_nombre = context.nom_departamento.FirstOrDefault(x => x.dpto_id == dpto_id).dpto_nombre;
            }

            if (terceros.sector_id != null)
            {
                int? sector_id = terceros.sector_id;
                ViewBag.sector_id = sector_id;
                ViewBag.sector_nombre = context.nom_sector.FirstOrDefault(x => x.sec_id == sector_id).sec_nombre;
            }


            if (ModelState.IsValid)
            {
                int tiene = (from t in context.icb_terceros
                             where t.doc_tercero == null
                                   && t.tercero_id == terceros.tercero_id
                             select t
                    ).Count();
                if (tiene == 1)
                {
                    //consulta si el registro documento esta en BD
                    int nom = (from a in context.icb_terceros
                               where a.doc_tercero == terceros.doc_tercero
                                     && a.doc_tercero != null
                               select a.doc_tercero).Count();

                    if (nom == 0)
                    {
                        terceros.doc_tercero = terceros.doc_tercero;
                        terceros.tercerofec_actualizacion = DateTime.Now;
                        terceros.tercerouserid_actualizacion = Convert.ToInt32(Session["user_usuarioid"]);
                        terceros.segnom_tercero = terceros.segnom_tercero != null ? terceros.segnom_tercero : "";
                        terceros.segapellido_tercero =
                            terceros.segapellido_tercero != null ? terceros.segapellido_tercero : "";
                        terceros.tpregimen_id = terceros.tpregimen_id;
                        terceros.retfuente = terceros.retfuente;
                        terceros.retiva = terceros.retiva;
                        terceros.retica = terceros.retica;
                        terceros.retica = terceros.retica;
                        terceros.cuenta_banco = terceros.cuenta_banco;

                        string listDirecciones = Request["lista_direcciones"];
                        if (!string.IsNullOrEmpty(listDirecciones))
                        {
                            int lista = Convert.ToInt32(listDirecciones);
                            for (int i = 1; i <= lista + 1; i++)
                            {
                                terceros_direcciones direcciones = new terceros_direcciones();
                                if (!string.IsNullOrEmpty(Request["ciudad" + i]))
                                {
                                    direcciones.ciudad = Convert.ToInt32(Request["ciudad" + i]);
                                    if (!string.IsNullOrEmpty(Request["sector" + i]))
                                    {
                                        direcciones.sector = Convert.ToInt32(Request["sector" + i]);
                                    }

                                    direcciones.direccion = Request["direccion" + i];
                                    direcciones.idtercero = terceros.tercero_id;
                                    context.terceros_direcciones.Add(direcciones);
                                }
                            }
                        }

                        context.Entry(terceros).State = EntityState.Modified;
                        int resul = context.SaveChanges();
                        if (resul > 0)
                        {
                            TempData["mensaje"] = "La actualización del tercero fue exitoso!";
                        }

                        ViewBag.tpdoc_id = new SelectList(context.tp_documento, "tpdoc_id", "tpdoc_nombre",
                            terceros.tpdoc_id);
                        ViewBag.ciu_id = new SelectList(context.nom_ciudad, "ciu_id", "ciu_nombre", terceros.ciu_id);
                        ViewBag.genero_tercero = new SelectList(context.gen_tercero, "gentercero_id",
                            "gentercero_nombre", terceros.genero_tercero);
                        PanelesActivos(terceros.tercero_id);
                        users usuarioCreador1 =
                            context.users.FirstOrDefault(x => x.user_id == terceros.tercerouserid_creacion);
                        ViewBag.user_nombre_cre = usuarioCreador1.user_nombre + " " + usuarioCreador1.user_apellido;
                        users usuarioActualizador1 =
                            context.users.FirstOrDefault(x => x.user_id == terceros.tercerouserid_actualizacion);
                        ViewBag.user_nombre_act = usuarioActualizador1 != null
                            ? usuarioActualizador1.user_nombre + " " + usuarioActualizador1.user_apellido
                            : null;
                        ViewBag.fechaNac = terceros.fec_nacimiento != null
                            ? terceros.fec_nacimiento.Value.ToShortDateString()
                            : "";
                        ViewBag.tpregimen_id = new SelectList(context.tpregimen_tercero, "tpregimen_id",
                            "tpregimen_nombre", terceros.tpregimen_id);

                        users usuarioCreador2 =
                            context.users.FirstOrDefault(x => x.user_id == terceros.tercerouserid_creacion);

                        users usuarioActualizador2 =
                            context.users.FirstOrDefault(x => x.user_id == terceros.tercerouserid_actualizacion);

                        BuscarFavoritos(menu);

                        return View(terceros);
                    }

                    TempData["mensaje_error"] = "El número de documento que ingreso ya se encuentra, por favor valide!";
                    ViewBag.tpdoc_id =
                        new SelectList(context.tp_documento, "tpdoc_id", "tpdoc_nombre", terceros.tpdoc_id);
                    ViewBag.ciu_id = new SelectList(context.nom_ciudad, "ciu_id", "ciu_nombre", terceros.ciu_id);
                    ViewBag.genero_tercero = new SelectList(context.gen_tercero, "gentercero_id", "gentercero_nombre",
                        terceros.genero_tercero);
                    ViewBag.tpregimen_id = new SelectList(context.tpregimen_tercero, "tpregimen_id", "tpregimen_nombre",
                        terceros.tpregimen_id);

                    PanelesActivos(terceros.tercero_id);

                    ViewBag.fechaNac = terceros.fec_nacimiento != null
                        ? terceros.fec_nacimiento.Value.ToShortDateString()
                        : "";
                    BuscarFavoritos(menu);
                    return View(terceros);
                }

                {
                    terceros.tercerofec_actualizacion = DateTime.Now;
                    terceros.tercerouserid_actualizacion = Convert.ToInt32(Session["user_usuarioid"]);
                    terceros.segnom_tercero = terceros.segnom_tercero != null ? terceros.segnom_tercero : "";
                    terceros.segapellido_tercero =
                        terceros.segapellido_tercero != null ? terceros.segapellido_tercero : "";
                    terceros.tpregimen_id = terceros.tpregimen_id;
                    terceros.retfuente = terceros.retfuente;
                    terceros.retiva = terceros.retiva;
                    terceros.retica = terceros.retica;
                    terceros.autorretencion = terceros.autorretencion;
                    terceros.cuenta_banco = terceros.cuenta_banco;

                    string listDirecciones = Request["lista_direcciones"];
                    if (!string.IsNullOrEmpty(listDirecciones))
                    {
                        int lista = Convert.ToInt32(listDirecciones);
                        for (int i = 1; i <= lista + 1; i++)
                        {
                            terceros_direcciones direcciones = new terceros_direcciones();
                            if (!string.IsNullOrEmpty(Request["ciudad" + i]))
                            {
                                direcciones.ciudad = Convert.ToInt32(Request["ciudad" + i]);
                                if (!string.IsNullOrEmpty(Request["sector" + i]))
                                {
                                    direcciones.sector = Convert.ToInt32(Request["sector" + i]);
                                }

                                direcciones.direccion = Request["direccion" + i];
                                direcciones.idtercero = terceros.tercero_id;
                                context.terceros_direcciones.Add(direcciones);
                            }
                        }
                    }

                    context.Entry(terceros).State = EntityState.Modified;
                    int resul = context.SaveChanges();
                    if (resul > 0)
                    {
                        TempData["mensaje"] = "La actualización del tercero fue exitoso!";
                    }

                    ViewBag.tpdoc_id =
                        new SelectList(context.tp_documento, "tpdoc_id", "tpdoc_nombre", terceros.tpdoc_id);
                    ViewBag.ciu_id = new SelectList(context.nom_ciudad, "ciu_id", "ciu_nombre", terceros.ciu_id);
                    ViewBag.genero_tercero = new SelectList(context.gen_tercero, "gentercero_id", "gentercero_nombre",
                        terceros.genero_tercero);
                    PanelesActivos(terceros.tercero_id);
                    users usuarioCreador1 =
                        context.users.FirstOrDefault(x => x.user_id == terceros.tercerouserid_creacion);
                    ViewBag.user_nombre_cre = usuarioCreador1.user_nombre + " " + usuarioCreador1.user_apellido;
                    users usuarioActualizador1 =
                        context.users.FirstOrDefault(x => x.user_id == terceros.tercerouserid_actualizacion);
                    ViewBag.user_nombre_act = usuarioActualizador1 != null
                        ? usuarioActualizador1.user_nombre + " " + usuarioActualizador1.user_apellido
                        : null;
                    ViewBag.fechaNac = terceros.fec_nacimiento != null
                        ? terceros.fec_nacimiento.Value.ToShortDateString()
                        : "";

                    users usuarioCreador2 =
                        context.users.FirstOrDefault(x => x.user_id == terceros.tercerouserid_creacion);

                    users usuarioActualizador2 =
                        context.users.FirstOrDefault(x => x.user_id == terceros.tercerouserid_actualizacion);

                    BuscarFavoritos(menu);

                    return View(terceros);
                }
            }

            List<ModelErrorCollection> errors = ModelState.Select(x => x.Value.Errors)
                .Where(y => y.Count > 0)
                .ToList();
            ViewBag.tpdoc_id = new SelectList(context.tp_documento, "tpdoc_id", "tpdoc_nombre", terceros.tpdoc_id);
            ViewBag.ciu_id = new SelectList(context.nom_ciudad, "ciu_id", "ciu_nombre", terceros.ciu_id);
            ViewBag.genero_tercero = new SelectList(context.gen_tercero, "gentercero_id", "gentercero_nombre",
                terceros.genero_tercero);
            PanelesActivos(terceros.tercero_id);
            users usuarioCreador = context.users.FirstOrDefault(x => x.user_id == terceros.tercerouserid_creacion);
            ViewBag.user_nombre_cre = usuarioCreador.user_nombre + " " + usuarioCreador.user_apellido;
            users usuarioActualizador =
                context.users.FirstOrDefault(x => x.user_id == terceros.tercerouserid_actualizacion);
            ViewBag.user_nombre_act = usuarioActualizador != null
                ? usuarioActualizador.user_nombre + " " + usuarioActualizador.user_apellido
                : null;
            ViewBag.fechaNac = terceros.fec_nacimiento != null ? terceros.fec_nacimiento.Value.ToShortDateString() : "";
            BuscarFavoritos(menu);
            return View(terceros);
        }

        [HttpGet]
        public ActionResult updateCliente(int? id, int? menu)
        {
            var ids = Convert.ToString(id);
            if (Session["user_usuarioid"] != null)
            {
                terceroClienteForm cliente = new terceroClienteForm
                {
                    tercero_id = id ?? 0,
                    usuario_modi=Convert.ToInt32(Session["user_usuarioid"]),
                };
                PanelesActivos(id ?? 0);
                tercero_cliente cliente2 = context.tercero_cliente.FirstOrDefault(x => x.tercero_id == id);
                tercero_cliente cliente3 = context.tercero_cliente.FirstOrDefault(x => x.tercero_id == id);
                if (cliente2 != null)
                {
                    ViewBag.lPreciosRepuestos = cliente2.lprecios_repuestos;
                    ViewBag.fec_cupo_limite = cliente2.fec_cupo_limite != null ? cliente2.fec_cupo_limite.Value.ToString("yyyy/MM/dd", new CultureInfo("en-US")): "";

                    var buscarTipoDocumento = context.tp_doc_registros.Select(d => new { d.sw, d.tpdoc_id, nombre = "(" + d.prefijo + ") " + d.tpdoc_nombre, d.tipo }).ToList();
                    users usuarioCreador = context.users.FirstOrDefault(x => x.user_id == cliente2.tercliuserid_creacion);
                    ViewBag.user_nombre_cre = usuarioCreador.user_nombre + " " + usuarioCreador.user_apellido;
                    users usuarioActualizador =context.users.FirstOrDefault(x => x.user_id == cliente.tercliuserid_actualizacion);
                    if (usuarioActualizador != null)
                    {
                        ViewBag.user_nombre_act = usuarioActualizador != null
                            ? usuarioActualizador.user_nombre + " " + usuarioActualizador.user_apellido
                            : null;
                    }

                    cliente.base_retencion = cliente2.base_retencion;
                    cliente.cupocredito = cliente2.cupocredito != null ? cliente2.cupocredito.Value.ToString("N0", new CultureInfo("is-IS")) : "0";
                    cliente.bloqueado = cliente2.bloqueado;
                    cliente.cltercero_id = cliente2.cltercero_id;
                    cliente.cod_pago_id = cliente2.cod_pago_id;
                    cliente.contribucion = cliente2.contribucion;
                    cliente.dia_nofacturad = cliente2.dia_nofacturad;
                    cliente.dia_nofacturah = cliente2.dia_nofacturah;
                    cliente.dscto_mo = cliente2.dscto_mo;
                    cliente.dscto_rep = cliente2.dscto_rep;
                    cliente.edades_hijos = cliente2.edades_hijos;
                    cliente.edocivil_id = cliente2.edocivil_id;
                    cliente.exentoiva = cliente2.exentoiva;
                    cliente.fec_cupo_limite = cliente2.fec_cupo_limite != null ? cliente2.fec_cupo_limite.Value.ToString("yyyy/MM/dd", new CultureInfo("en-US")) : "";
                    cliente.idsegmentacion = cliente2.idsegmentacion;
                    cliente.lprecios_repuestos = cliente2.lprecios_repuestos;
                    cliente.lprecios_vehiculos = cliente2.lprecios_vehiculos;
                    cliente.motivo_bloqueado = cliente2.motivo_bloqueado;
                    cliente.numhijos_tercero = cliente2.numhijos_tercero;
                    cliente.pagina_web = cliente2.pagina_web;
                    cliente.retencion = cliente2.retencion;
                    cliente.telefono = cliente2.telefono;
                    cliente.terclifec_creacion = cliente2.terclifec_creacion;
                    cliente.terclifec_actualizacion = cliente2.terclifec_actualizacion;
                    cliente.tercliid_licencia = cliente2.tercliid_licencia;
                    cliente.tercliuserid_actualizacion = cliente2.tercliuserid_actualizacion;
                    cliente.tercliuserid_creacion = cliente2.tercliuserid_creacion;
                    cliente.tiempo_para_bloqueo = cliente2.tiempo_para_bloqueo;
                    cliente.tipo_cliente = cliente2.tipo_cliente;
                    cliente.tpdpte_id = cliente2.tpdpte_id;
                    cliente.tphobby_id = cliente2.tphobby_id;
                    cliente.tpocupacion_id = cliente2.tpocupacion_id;
                    ViewBag.valSegmentacion = cliente2.idsegmentacion;
                }
                else
                {
                    cliente.cupocredito = "0";
                    cliente.fec_cupo_limite = DateTime.Now.Date.ToString("yyyy/MM/dd", new CultureInfo("en-US"));
                    cliente.tercliuserid_creacion = Convert.ToInt32(Session["user_usuarioid"]);
                }
                //  var tiporegimen = context.icb_terceros.Select(d => new { id = d.tpregimen_id, nombre = d.tpregimen_codigo + " - " + d.tpregimen_nombre }).ToList();
                // ViewBag.tpregimen_id = new SelectList(tiporegimen, "id", "nombre", cliente.tpregimen_id);
                //var actividadeconomica = context.acteco_tercero.Select(d =>
                //    new { id = d.acteco_id, nombre = d.nroacteco_tercero + " - " + d.acteco_nombre }).ToList();
                // ViewBag.actividadEconomica_id = new SelectList(context.acteco_tercero, "acteco_id", "acteco_nombre");

                //ViewBag.edocivil_idx = new SelectList(context.estado_civil, "edocivil_id", "edocivil_nombre",cliente.edocivil_id);


                //var civil = context.estado_civil.Select(x => new { id = x.edocivil_id, nombre = x.edocivil_nombre }).ToList();
                //ViewBag.edocivil_id = new SelectList(civil, "id", "nombre", cliente.edocivil_id);

                //ViewBag.edocivil_id = new SelectList(context.estado_civil, "edocivil_id", "edocivil_nombre", cliente.edocivil_id);
                //ViewBag.tpocupacion_id = new SelectList(context.tp_ocupacion, "tpocupacion_id", "tpocupacion_nombre",cliente.tpocupacion_id);
                //ViewBag.tphobby_id = new SelectList(context.tp_hobby, "tphobby_id", "tphobby_nombre", cliente.tphobby_id);
                //ViewBag.tpdpte_id = new SelectList(context.tp_Dpte, "tpdpte_id", "tpdpte_nombre", cliente.tpdpte_id);
                //ViewBag.cod_pago_id = new SelectList(context.fpago_tercero, "fpago_id", "fpago_nombre",cliente.cod_pago_id ?? 0);
                //ViewBag.tipo_cliente = new SelectList(context.tipocliente, "tipo", "nombre", cliente.tipo_cliente ?? 0);
               CamposListasDesplegables2(cliente);
                ViewBag.idsegmentacion = new SelectList(context.segmentacion.Where(x => x.estado).OrderBy(x => x.descripcion), "id","descripcion", cliente.idsegmentacion);
                MostrarSaldo(id);
                PanelesActivos(id ?? 0);
                BuscarFavoritos(menu);
                return View(cliente);
            }
            else
            {
                TempData["mensaje_error"] = "La sesión ha finalizado";
                return RedirectToAction("Login", "Login");
            }
            
        }

        [HttpPost]
        public ActionResult updateCliente(terceroClienteForm cliente, int? menu)
        {
            if (ModelState.IsValid)
            {
                var idseg = Request["valSegmentacion"].ToString();
                var idsegmencliente =!string.IsNullOrWhiteSpace(idseg)?Convert.ToInt32(idseg):0;
                ViewBag.lPreciosRepuestos = cliente.lprecios_repuestos;
                string Paridsegmentacion = context.icb_sysparameter.FirstOrDefault(x => x.syspar_cod == "P134").syspar_value;
                int idsegmentacion = Paridsegmentacion != null ? Convert.ToInt32(Paridsegmentacion) : 1;
                // var tiporegimen = context.tpregimen_tercero.Select(d => new { id = d.tpregimen_id, nombre = d.tpregimen_codigo + " - " + d.tpregimen_nombre }).ToList();
                // ViewBag.tpregimen_id = new SelectList(tiporegimen, "id", "nombre", cliente.tpregimen_id);

                var actividadeconomica = context.acteco_tercero.Select(d =>
                    new { id = d.acteco_id, nombre = d.nroacteco_tercero + " - " + d.acteco_nombre }).ToList();
                
                icb_terceros tercero = context.icb_terceros.Find(cliente.tercero_id);
                tercero_proveedor terceroproveedor = context.tercero_proveedor.Where(x => x.tercero_id == cliente.tercero_id).FirstOrDefault();
                tercero_cliente buscarCliente = context.tercero_cliente.FirstOrDefault(x => x.tercero_id == cliente.tercero_id);
                if (buscarCliente != null)
                {
                    buscarCliente.terclifec_actualizacion = DateTime.Now;
                    buscarCliente.tercliuserid_actualizacion = Convert.ToInt32(Session["user_usuarioid"]);
                    buscarCliente.numhijos_tercero = cliente.numhijos_tercero;
                    buscarCliente.tphobby_id = cliente.tphobby_id.Value;
                    buscarCliente.tpdpte_id = cliente.tpdpte_id.Value;
                    buscarCliente.tpocupacion_id = cliente.tpocupacion_id.Value;
                    buscarCliente.actividadEconomica_id = buscarCliente.icb_terceros.id_acteco;
                    buscarCliente.bloqueado = cliente.bloqueado;
                    buscarCliente.edocivil_id = cliente.edocivil_id.Value;
                    buscarCliente.exentoiva = cliente.exentoiva;
                    buscarCliente.dia_nofacturad = cliente.dia_nofacturad;
                    buscarCliente.dia_nofacturah = cliente.dia_nofacturah;
                    buscarCliente.dscto_mo = cliente.dscto_mo;
                    buscarCliente.dscto_rep = cliente.dscto_rep;
                    buscarCliente.telefono = cliente.telefono;
                    buscarCliente.pagina_web = cliente.pagina_web;
                    decimal cupox = 0;
                    if (!string.IsNullOrWhiteSpace(cliente.cupocredito))
                    {
                        var convertir = Decimal.TryParse(cliente.cupocredito,NumberStyles.Number, new CultureInfo("is-IS"), out cupox);
                        if (convertir == true)
                        {
                            buscarCliente.cupocredito = cupox;
                        }
                    }
                    buscarCliente.tipo_cliente = cliente.tipo_cliente;
                    buscarCliente.cod_pago_id = cliente.cod_pago_id;
                    // buscarCliente.tpregimen_id = cliente.tpregimen_id;
                    buscarCliente.lprecios_repuestos = cliente.lprecios_repuestos;
                    buscarCliente.lprecios_vehiculos = cliente.lprecios_vehiculos;
                    buscarCliente.edades_hijos = cliente.edades_hijos;
                    buscarCliente.idsegmentacion = idsegmencliente>0? idsegmencliente:1; // esto lo debe actualizar el CRM u aun no está 20/06/2019 por eso se quema mientra tanto;
                    DateTime fechax = DateTime.Now;
                    if (!string.IsNullOrWhiteSpace(cliente.fec_cupo_limite))
                    {
                        var convertir2 = DateTime.TryParse(cliente.fec_cupo_limite,new CultureInfo("is-IS"),DateTimeStyles.None, out fechax);
                        if (convertir2 == true)
                        {
                            buscarCliente.fec_cupo_limite = fechax;
                        }
                    }

                    context.Entry(buscarCliente).State = EntityState.Modified;
                    /*tercero.id_acteco = cliente.actividadEconomica_id;*/
                    context.Entry(tercero).State = EntityState.Modified;
                    if (terceroproveedor != null && buscarCliente.icb_terceros.id_acteco!=null)
                    {
                        terceroproveedor.acteco_id = buscarCliente.icb_terceros.id_acteco.Value;
                        context.Entry(terceroproveedor).State = EntityState.Modified;
                    }
                    int result = context.SaveChanges();

                    /*ViewBag.actividadEconomica_id = new SelectList(actividadeconomica, "id", "nombre",
                        cliente.actividadEconomica_id);*/
                    ViewBag.edocivil_id = new SelectList(context.estado_civil, "edocivil_id", "edocivil_nombre");
                    ViewBag.tpocupacion_id = new SelectList(context.tp_ocupacion, "tpocupacion_id", "tpocupacion_nombre");
                    ViewBag.tphobby_id = new SelectList(context.tp_hobby, "tphobby_id", "tphobby_nombre");
                    ViewBag.tpdpte_id = new SelectList(context.tp_Dpte, "tpdpte_id", "tpdpte_nombre");
                    ViewBag.cod_pago_id = new SelectList(context.fpago_tercero, "fpago_id", "fpago_nombre",cliente.cod_pago_id);
                    
                    //ViewBag.tipo_cliente = new SelectList(context.tipocliente, "tipo", "nombre", cliente.tipo_cliente ?? 0);
                    var session = Convert.ToInt32(Session["user_usuarioid"]);
                    var busqueda=context.users.Where(x => x.user_id == session).FirstOrDefault();
                    var busquedas1 = context.rolpermisos.Where(x => x.codigo == "P55").FirstOrDefault();

                     var conteo = context.rolacceso.Where(x => x.idpermiso == busquedas1.id && x.idrol==busqueda.rol_id).Count();

                    var tipoc = new SelectList(context.tipocliente, "tipo", "nombre", cliente.tipo_cliente ?? 0);
                    if (conteo == 0)
                    {
                        tipoc = new SelectList(context.tipocliente.Where(d => d.nombre.ToUpper() == "NORMAL"), "tipo", "nombre");
                    }


                    ViewBag.tipo_cliente = tipoc;
                                       

                    ViewBag.idsegmentacion = new SelectList(context.segmentacion.Where(x => x.estado).OrderBy(x => x.descripcion), "id","descripcion", cliente.idsegmentacion);

                    PanelesActivos(cliente.tercero_id??0);
                    if (result > 0)
                    {
                        TempData["mensaje"] = "La actualizacion del cliente fue exitosa!";
                    }

                    users usuarioCreador =
                        context.users.FirstOrDefault(x => x.user_id == buscarCliente.tercliuserid_creacion);
                    ViewBag.user_nombre_cre = usuarioCreador.user_nombre + " " + usuarioCreador.user_apellido;
                    users usuarioActualizador =
                        context.users.FirstOrDefault(x => x.user_id == buscarCliente.tercliuserid_actualizacion);
                    ViewBag.user_nombre_act = usuarioActualizador != null
                        ? usuarioActualizador.user_nombre + " " + usuarioActualizador.user_apellido
                        : null;
                    MostrarSaldo(cliente.tercero_id);
                    BuscarFavoritos(menu);
                    return View(cliente);
                }
                else
                {
                    
                    tercero_cliente NuevoCliente = new tercero_cliente();
                    NuevoCliente.tercero_id =  cliente.tercero_id.Value;     
                    NuevoCliente.terclifec_creacion = DateTime.Now;
                    NuevoCliente.tercliuserid_creacion = Convert.ToInt32(Session["user_usuarioid"]);

                    NuevoCliente.numhijos_tercero = cliente.numhijos_tercero;
                    NuevoCliente.tphobby_id = cliente.tphobby_id.Value;
                    NuevoCliente.tpdpte_id = cliente.tpdpte_id.Value;
                    NuevoCliente.tpocupacion_id = cliente.tpocupacion_id.Value;
                    NuevoCliente.telefono = cliente.telefono;
                    NuevoCliente.pagina_web = cliente.pagina_web;
                    //NuevoCliente.actividadEconomica_id = cliente.actividadEconomica_id;
                    NuevoCliente.bloqueado = cliente.bloqueado;
                    NuevoCliente.edocivil_id = cliente.edocivil_id.Value;
                    NuevoCliente.exentoiva = cliente.exentoiva;
                    NuevoCliente.dia_nofacturad = cliente.dia_nofacturad;
                    NuevoCliente.dia_nofacturah = cliente.dia_nofacturah;
                    NuevoCliente.tercli_estado =true;

                    NuevoCliente.dscto_mo = cliente.dscto_mo;
                    NuevoCliente.dscto_rep = cliente.dscto_rep;
                    decimal cupox = 0;
                    if (!string.IsNullOrWhiteSpace(cliente.cupocredito))
                    {
                        var convertir = Decimal.TryParse(cliente.cupocredito, NumberStyles.Number, new CultureInfo("is-IS"), out cupox);
                        if (convertir == true)
                        {
                            NuevoCliente.cupocredito = cupox;
                        }
                    }
                    NuevoCliente.tipo_cliente = cliente.tipo_cliente;
                    NuevoCliente.cod_pago_id = cliente.cod_pago_id;
                    // buscarCliente.tpregimen_id = cliente.tpregimen_id;
                    NuevoCliente.lprecios_repuestos = cliente.lprecios_repuestos;
                    NuevoCliente.lprecios_vehiculos = cliente.lprecios_vehiculos;
                    NuevoCliente.edades_hijos = cliente.edades_hijos;
                    // esto lo debe actualizar el CRM u aun no está 20/06/2019 por eso se quema mientra tanto;
                    NuevoCliente.idsegmentacion = idsegmencliente > 0 ? idsegmencliente : 1;
                    DateTime fechax = DateTime.Now;
                    if (!string.IsNullOrWhiteSpace(cliente.fec_cupo_limite))
                    {
                        var convertir2 = DateTime.TryParse(cliente.fec_cupo_limite, new CultureInfo("is-IS"), DateTimeStyles.None, out fechax);
                        if (convertir2 == true)
                        {
                            NuevoCliente.fec_cupo_limite = fechax;
                        }
                    }

                    context.tercero_cliente.Add(NuevoCliente);
                    context.SaveChanges();
                    
                    if (terceroproveedor != null && NuevoCliente.icb_terceros.id_acteco!=null)
                    {
                        terceroproveedor.acteco_id = NuevoCliente.icb_terceros.id_acteco.Value;
                        context.Entry(terceroproveedor).State = EntityState.Modified;
                    }
                    int result = context.SaveChanges();

                    ViewBag.edocivil_id = new SelectList(context.estado_civil, "edocivil_id", "edocivil_nombre");
                    ViewBag.actividadEconomica_id =new SelectList(context.acteco_tercero, "acteco_id", "acteco_nombre");
                    ViewBag.tpocupacion_id = new SelectList(context.tp_ocupacion, "tpocupacion_id", "tpocupacion_nombre");
                    ViewBag.tphobby_id = new SelectList(context.tp_hobby, "tphobby_id", "tphobby_nombre");
                    ViewBag.tpdpte_id = new SelectList(context.tp_Dpte, "tpdpte_id", "tpdpte_nombre");
                    ViewBag.cod_pago_id = new SelectList(context.fpago_tercero, "fpago_id", "fpago_nombre", cliente.cod_pago_id);
                     
                    ViewBag.tipo_cliente = new SelectList(context.tipocliente, "tipo", "nombre", cliente.tipo_cliente);
                    ViewBag.idsegmentacion = new SelectList(context.segmentacion.Where(x => x.estado).OrderBy(x => x.descripcion), "id", "descripcion", cliente.idsegmentacion);

                    PanelesActivos(cliente.tercero_id??0);
                    if (result > 0)
                    {
                        TempData["mensaje"] = "La actualizacion del cliente fue exitosa!";
                    }

                    
                    users usuarioCreador = context.users.FirstOrDefault(x => x.user_id == cliente.tercliuserid_creacion);
                    if (usuarioCreador == null)
                    {
                        var usuario2 = Convert.ToInt32(Session["user_usuarioid"]);
                        usuarioCreador = context.users.FirstOrDefault(x => x.user_id == usuario2);
                    }
                    ViewBag.user_nombre_cre = usuarioCreador.user_nombre + " " + usuarioCreador.user_apellido;
                    MostrarSaldo(cliente.tercero_id);
                    BuscarFavoritos(menu);
                    return View(cliente);
                }
            }

            //ViewBag.fec_cupo_limite = cliente.fec_cupo_limite != null
            //        ? cliente.fec_cupo_limite.Value.ToString("yyyy/MM/dd", new CultureInfo("en-US"))
            //        : "";
            ViewBag.fec_cupo_limite = cliente.fec_cupo_limite;
            List<ModelErrorCollection> errors = ModelState.Select(x => x.Value.Errors) .Where(y => y.Count > 0).ToList();
            ViewBag.edocivil_id = new SelectList(context.estado_civil, "edocivil_id", "edocivil_nombre",new { cliente.tercero_id});
            ViewBag.actividadEconomica_id = new SelectList(context.acteco_tercero, "acteco_id", "acteco_nombre");
            ViewBag.tpocupacion_id = new SelectList(context.tp_ocupacion, "tpocupacion_id", "tpocupacion_nombre");
            ViewBag.tphobby_id = new SelectList(context.tp_hobby, "tphobby_id", "tphobby_nombre");
            ViewBag.tpdpte_id = new SelectList(context.tp_Dpte, "tpdpte_id", "tpdpte_nombre");
            ViewBag.cod_pago_id = new SelectList(context.fpago_tercero, "fpago_id", "fpago_nombre", cliente.cod_pago_id);
           
            ViewBag.tipo_cliente = new SelectList(context.tipocliente, "tipo", "nombre", cliente.tipo_cliente);
            ViewBag.idsegmentacion = new SelectList(context.segmentacion.Where(x => x.estado).OrderBy(x => x.descripcion), "id","descripcion", cliente.idsegmentacion);
            //ViewBag.tpregimen_id = new SelectList(context.tpregimen_tercero, "tpregimen_id", "tpregimen_nombre", cliente.tpregimen_id);

            PanelesActivos(cliente.tercero_id??0);
            users usuarioCreador1 = context.users.FirstOrDefault(x => x.user_id == cliente.tercliuserid_creacion);
            if (usuarioCreador1 != null)
            {
                ViewBag.user_nombre_cre = usuarioCreador1.user_nombre + " " + usuarioCreador1.user_apellido;
            }

            users usuarioActualizador1 =
                context.users.FirstOrDefault(x => x.user_id == cliente.tercliuserid_actualizacion);
            if (usuarioActualizador1 != null)
            {
                ViewBag.user_nombre_act = usuarioActualizador1 != null
                    ? usuarioActualizador1.user_nombre + " " + usuarioActualizador1.user_apellido
                    : null;
            }
       
            BuscarFavoritos(menu);
            return View(cliente);
            //return RedirectToAction("updateCliente", new { id = cliente.tercero_id});
        }

              
        public void MostrarSaldo(int? id) {
            tercero_cliente cliente = context.tercero_cliente.FirstOrDefault(x => x.tercero_id == id);
            int totalFacturas = 0;
            int saldoCupo = 0;
            icb_sysparameter repuestos = context.icb_sysparameter.Where(x => x.syspar_cod == "P102").FirstOrDefault();
            int rep = repuestos != null ? Convert.ToInt32(repuestos.syspar_value) : 5;
            icb_sysparameter accesorios = context.icb_sysparameter.Where(x => x.syspar_cod == "P103").FirstOrDefault();
            int acc = accesorios != null ? Convert.ToInt32(accesorios.syspar_value) : 17;

            var data = (from factRep in context.encab_documento
                        where (factRep.tipo == rep || factRep.tipo == acc)
                              && factRep.nit == id && (factRep.usa_cupo == true || factRep.detalle_formas_pago_orden.Where(d => d.idformas_pago == 7).Count() > 0)
                        select new
                        {
                            factRep.idencabezado,
                           // costo = factRep.valor_total
                           costo = factRep.valor_cupo - factRep.valor_cupo_aplicado
                        }).ToList();
            //se recorre las facturas del cliente y sus correspondientes recibos de caja para determinar valor real en factura
            for (int i = 0; i < data.Count(); i++)
            {
                totalFacturas += calcularsaldo(data[i].idencabezado, Convert.ToInt32(data[i].costo));
            }
            //se consulta el cupo disponible con el que cuenta el cliente 
            tercero_cliente cupoCliente = context.tercero_cliente
                .Where(x => x.tercero_id == id && x.fec_cupo_limite >= DateTime.Now).FirstOrDefault();
            //se resta el total de facturas actualmente vigentes al cupo disponible del cliente
            if (cupoCliente != null)
            {
                saldoCupo = Convert.ToInt32(cliente.cupocredito) - totalFacturas;
            }

            ViewBag.saldocupo = saldoCupo;

                       
        }
        public int calcularsaldo(long? id, int saldo)
        {
            int totalFactura = 0;
            if (id != null)
            {
                var resultadoFactura = (from ed in context.encab_documento
                                        join cd in context.cruce_documentos
                                            on ed.idencabezado equals cd.id_encab_aplica
                                        join dp in context.documentos_pago
                                            on cd.id_encabezado equals dp.idtencabezado
                                        where cd.id_encab_aplica == id
                                        select new
                                        {
                                            dp.valor
                                        }).ToList();

                for (int i = 0; i < resultadoFactura.Count; i++)
                {
                    totalFactura += Convert.ToInt32(resultadoFactura[i].valor);
                }

                saldo = saldo - totalFactura;
                return saldo;
            }

            return totalFactura;
        }
        public JsonResult AgregarTipoTercero(int idTercero, int idTipo)
        {
            if (idTipo == 1)
            {
                tercero_cliente buscarTerceroCliente = context.tercero_cliente.FirstOrDefault(x => x.tercero_id == idTercero);
                if (buscarTerceroCliente != null)
                {
                    buscarTerceroCliente.tercli_estado = true;
                    context.Entry(buscarTerceroCliente).State = EntityState.Modified;
                    context.SaveChanges();
                }
            }

            if (idTipo == 2)
            {
                tercero_proveedor buscarTerceroProveedor = context.tercero_proveedor.FirstOrDefault(x => x.tercero_id == idTercero);
                if (buscarTerceroProveedor != null)
                {
                    buscarTerceroProveedor.tercpro_estado = true;
                    context.Entry(buscarTerceroProveedor).State = EntityState.Modified;
                    context.SaveChanges();
                }
            }

            if (idTipo == 3)
            {
                tercero_empleado buscarTerceroEmpleado = context.tercero_empleado.FirstOrDefault(x => x.tercero_id == idTercero);
                if (buscarTerceroEmpleado != null)
                {
                    buscarTerceroEmpleado.teremp__estado = true;
                    context.Entry(buscarTerceroEmpleado).State = EntityState.Modified;
                    context.SaveChanges();
                }
            }

            int result = 0;
            clasificacionTercero relacion =
                context.clasificacionTercero.FirstOrDefault(x => x.tercero_id == idTercero && x.tptercero_id == idTipo);
            if (relacion == null)
            {
                context.clasificacionTercero.Add(new clasificacionTercero
                {
                    tercero_id = idTercero,
                    tptercero_id = idTipo
                });
                result = context.SaveChanges();
            }


            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public JsonResult BorrarTipoTercero(int idTercero, int idTipo)
        {
            if (idTipo == 1)
            {
                tercero_cliente buscarTerceroCliente = context.tercero_cliente.FirstOrDefault(x => x.tercero_id == idTercero);
                if (buscarTerceroCliente != null)
                {
                    buscarTerceroCliente.tercli_estado = false;
                    context.Entry(buscarTerceroCliente).State = EntityState.Modified;
                    context.SaveChanges();
                }
            }

            if (idTipo == 2)
            {
                tercero_proveedor buscarTerceroProveedor = context.tercero_proveedor.FirstOrDefault(x => x.tercero_id == idTercero);
                if (buscarTerceroProveedor != null)
                {
                    buscarTerceroProveedor.tercpro_estado = true;
                    context.Entry(buscarTerceroProveedor).State = EntityState.Deleted;
                    context.SaveChanges();
                }
            }

            if (idTipo == 3)
            {
                tercero_empleado buscarTerceroEmpleado = context.tercero_empleado.FirstOrDefault(x => x.tercero_id == idTercero);
                if (buscarTerceroEmpleado != null)
                {
                    buscarTerceroEmpleado.teremp__estado = true;
                    context.Entry(buscarTerceroEmpleado).State = EntityState.Deleted;
                    context.SaveChanges();
                }
            }

            clasificacionTercero result =
                context.clasificacionTercero.FirstOrDefault(x => x.tercero_id == idTercero && x.tptercero_id == idTipo);
            if (result != null)
            {
                context.Entry(result).State = EntityState.Deleted;
                context.SaveChanges();
            }

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        ///     se crea una función de verificar el tipo de documento y el número de documento,
        ///     este es una función muy importante para el aprendizaje de un Jsonresult
        /// </summary>
        /// <param name="doc_tercero"></param>
        /// <param name="tpdoc_id"></param>
        /// <returns></returns>
        public JsonResult frontCheckDocument(string doc_tercero, int tpdoc_id)
        {
            int answer = 1;
            int data = (
                from p in context.icb_terceros
                where p.doc_tercero == doc_tercero && p.tpdoc_id == tpdoc_id
                select new
                {
                    p.tpdoc_id
                }).Count();
            if (data > 0)
            {
                answer = 0;
            }
            return Json(answer);
        }

        public void CamposListasDesplegables(tercero_proveedor modelo)
        {
            ViewBag.bodega = new SelectList(context.bodega_concesionario.OrderBy(x => x.bodccs_nombre), "id",
                "bodccs_nombre");
            ViewBag.sw = new SelectList(context.tp_doc_sw.OrderBy(x => x.Descripcion), "tpdoc_id", "Descripcion");
            ViewBag.tipo_regimenid = new SelectList(context.tpregimen_tercero.OrderBy(x => x.tpregimen_nombre),
                "tpregimen_id", "tpregimen_nombre");
            DbSet<contributario> dataContributario = context.contributario;
            ViewBag.retfuente = new SelectList(dataContributario.OrderBy(x => x.descripcion), "codigo", "descripcion",
                modelo.retfuente);
            ViewBag.retiva = new SelectList(dataContributario.OrderBy(x => x.descripcion), "codigo", "descripcion",
                modelo.retiva);
            ViewBag.retica = new SelectList(dataContributario.OrderBy(x => x.descripcion), "codigo", "descripcion",
                modelo.retica);
            ViewBag.autorretencion = new SelectList(dataContributario.OrderBy(x => x.descripcion), "codigo",
                "descripcion", modelo.autorretencion);
        }

        [HttpGet]
        public ActionResult updateProveedor(int? id, int? menu)
        {
            tercero_proveedor proveedor = context.tercero_proveedor.FirstOrDefault(x => x.tercero_id == id);
            if (proveedor != null)
            {
                var tiporegimen = (from a in context.icb_terceros
                                   join b in context.tpregimen_tercero
                                       on a.tpregimen_id equals b.tpregimen_id
                                   where a.tercero_id == id
                                   select new
                                   {
                                       id = b.tpregimen_id,
                                       nombre = b.tpregimen_nombre
                                   }).ToList();
                ViewBag.tpregimen_id = new SelectList(tiporegimen, "id", "nombre");

                ViewBag.acteco_id = new SelectList(context.acteco_tercero, "acteco_id", "acteco_nombre",
                    proveedor.acteco_id);
                ViewBag.tpsuministro_id = new SelectList(context.tiposumi_tercero, "tpsuministro_id",
                    "tpsuministro_nombre", proveedor.tpsuministro_id);
                //ViewBag.tpregimen_id = new SelectList(context.tpregimen_tercero, "tpregimen_id", "tpregimen_nombre", proveedor.tpregimen_id);
                ViewBag.tpimpu_id = new SelectList(context.tpimpu_tercero, "tpimpu_id", "tpimpu_nombre",
                    proveedor.tpimpu_id);
                ViewBag.fpago_id =
                    new SelectList(context.fpago_tercero, "fpago_id", "fpago_nombre", proveedor.fpago_id);
                ViewBag.tipo_proveedor = new SelectList(context.tipoproveedor, "tipo", "nombre");
                PanelesActivos(id ?? 0);
                users usuarioCreador = context.users.FirstOrDefault(x => x.user_id == proveedor.tercprouserid_creacion);
                ViewBag.user_nombre_cre = usuarioCreador.user_nombre + " " + usuarioCreador.user_apellido;
                users usuarioActualizador =
                    context.users.FirstOrDefault(x => x.user_id == proveedor.tercprouserid_actualizacion);
                ViewBag.user_nombre_act = usuarioActualizador != null
                    ? usuarioActualizador.user_nombre + " " + usuarioActualizador.user_apellido
                    : null;
                BuscarFavoritos(menu);
                return View(proveedor);
            }
            else
            {
                var tiporegimen = (from a in context.icb_terceros
                                   join b in context.tpregimen_tercero
                                       on a.tpregimen_id equals b.tpregimen_id
                                   where a.tercero_id == id
                                   select new
                                   {
                                       id = b.tpregimen_id,
                                       nombre = b.tpregimen_nombre
                                   }).ToList();
                ViewBag.tpregimen_id = new SelectList(tiporegimen, "id", "nombre");
                ViewBag.acteco_id = new SelectList(context.acteco_tercero, "acteco_id", "acteco_nombre");
                ViewBag.tpsuministro_id =
                    new SelectList(context.tiposumi_tercero, "tpsuministro_id", "tpsuministro_nombre");
                //ViewBag.tpregimen_id = new SelectList(context.tpregimen_tercero, "tpregimen_id", "tpregimen_nombre");
                ViewBag.tpimpu_id = new SelectList(context.tpimpu_tercero, "tpimpu_id", "tpimpu_nombre");
                ViewBag.fpago_id = new SelectList(context.fpago_tercero, "fpago_id", "fpago_nombre");
                ViewBag.tipo_proveedor = new SelectList(context.tipoproveedor, "tipo", "nombre");
            }

            tercero_proveedor terceroProveedor = new tercero_proveedor
            {
                tercero_id = id ?? 0
            };
            PanelesActivos(id ?? 0);
            BuscarFavoritos(menu);
            return View(terceroProveedor);
        }

        [HttpPost]
        public ActionResult updateProveedor(tercero_proveedor proveedor, int? menu)
        {
            if (ModelState.IsValid)
            {
                var tiporegimen = (from a in context.icb_terceros
                                   join b in context.tpregimen_tercero
                                       on a.tpregimen_id equals b.tpregimen_id
                                   where a.tercero_id == proveedor.tercero_id
                                   select new
                                   {
                                       id = b.tpregimen_id,
                                       nombre = b.tpregimen_nombre
                                   }).ToList();
                var tiporegime = (from a in context.icb_terceros
                                  join b in context.tpregimen_tercero
                                      on a.tpregimen_id equals b.tpregimen_id
                                  where a.tercero_id == proveedor.tercero_id
                                  select new
                                  {
                                      id = b.tpregimen_id,
                                      nombre = b.tpregimen_nombre
                                  }).FirstOrDefault();
                icb_terceros tercero = context.icb_terceros.Find(proveedor.tercero_id);
                tercero_cliente tercerocliente = context.tercero_cliente.Where(x => x.tercero_id == proveedor.tercero_id).FirstOrDefault();
                tercero_proveedor buscarProveedor =
                    context.tercero_proveedor.FirstOrDefault(x => x.tercero_id == proveedor.tercero_id);
                if (buscarProveedor != null)
                {
                    buscarProveedor.tercprofec_actualizacion = DateTime.Now;
                    buscarProveedor.tercprouserid_actualizacion = Convert.ToInt32(Session["user_usuarioid"]);
                    buscarProveedor.pgweb_tercero = proveedor.pgweb_tercero;
                    buscarProveedor.repreleg_tercero = proveedor.repreleg_tercero;
                    buscarProveedor.rete_tercero = proveedor.rete_tercero;
                    buscarProveedor.contrib_tercero = proveedor.contrib_tercero;
                    buscarProveedor.entidad_tercero = proveedor.entidad_tercero;
                    buscarProveedor.aplica_tercero = proveedor.aplica_tercero;
                    buscarProveedor.tarifaica_tercero = proveedor.tarifaica_tercero;
                    buscarProveedor.acteco_id = proveedor.acteco_id;
                    buscarProveedor.tpsuministro_id = proveedor.tpsuministro_id;
                    //buscarProveedor.tpregimen_id = proveedor.tpregimen_id;
                    buscarProveedor.tpregimen_id = tiporegime.id;
                    buscarProveedor.tpimpu_id = proveedor.tpimpu_id;
                    buscarProveedor.exentoiva = proveedor.exentoiva;
                    buscarProveedor.fpago_id = proveedor.fpago_id;
                    buscarProveedor.tipo_proveedor = proveedor.tipo_proveedor;
                    buscarProveedor.retica = tercero.retica;
                    buscarProveedor.retfuente = tercero.retfuente;
                    buscarProveedor.retiva = tercero.retiva;
                    buscarProveedor.autorretencion = tercero.autorretencion;
                    context.Entry(buscarProveedor).State = EntityState.Modified;
                    tercero.id_acteco = proveedor.acteco_id;
                    tercero.tpregimen_id = tiporegime.id;
                    context.Entry(tercero).State = EntityState.Modified;
                    if (tercerocliente != null)
                    {
                        tercerocliente.actividadEconomica_id = proveedor.acteco_id;
                        context.Entry(tercerocliente).State = EntityState.Modified;
                    }
                    int result = context.SaveChanges();

                    ViewBag.tpregimen_id = new SelectList(tiporegimen, "id", "nombre");

                    ViewBag.acteco_id = new SelectList(context.acteco_tercero, "acteco_id", "acteco_nombre");
                    ViewBag.tpsuministro_id =
                        new SelectList(context.tiposumi_tercero, "tpsuministro_id", "tpsuministro_nombre");
                    // ViewBag.tpregimen_id = new SelectList(context.tpregimen_tercero, "tpregimen_id", "tpregimen_nombre");
                    ViewBag.tpimpu_id = new SelectList(context.tpimpu_tercero, "tpimpu_id", "tpimpu_nombre");
                    ViewBag.fpago_id = new SelectList(context.fpago_tercero, "fpago_id", "fpago_nombre");
                    ViewBag.tipo_proveedor =
                        new SelectList(context.tipoproveedor, "tipo", "nombre", proveedor.tipo_proveedor);
                    PanelesActivos(proveedor.tercero_id);
                    if (result > 0)
                    {
                        TempData["mensaje"] = "La actualizacion del proveedor fue exitosa!";
                    }

                    users usuarioCreador =
                        context.users.FirstOrDefault(x => x.user_id == buscarProveedor.tercprouserid_creacion);
                    ViewBag.user_nombre_cre = usuarioCreador.user_nombre + " " + usuarioCreador.user_apellido;
                    users usuarioActualizador =
                        context.users.FirstOrDefault(x => x.user_id == buscarProveedor.tercprouserid_actualizacion);
                    ViewBag.user_nombre_act = usuarioActualizador != null
                        ? usuarioActualizador.user_nombre + " " + usuarioActualizador.user_apellido
                        : null;
                    BuscarFavoritos(menu);
                    return View(buscarProveedor);
                }

                proveedor.tercprofec_creacion = DateTime.Now;
                proveedor.tercprouserid_creacion = Convert.ToInt32(Session["user_usuarioid"]);
                proveedor.retica = tercero.retica;
                proveedor.retfuente = tercero.retfuente;
                proveedor.retiva = tercero.retiva;
                proveedor.autorretencion = tercero.autorretencion;
                context.tercero_proveedor.Add(proveedor);
                tercero.id_acteco = proveedor.acteco_id;
                tercero.tpregimen_id = tiporegime.id;
                context.Entry(tercero).State = EntityState.Modified;
                if (tercerocliente != null)
                {
                    tercerocliente.actividadEconomica_id = proveedor.acteco_id;
                    context.Entry(tercerocliente).State = EntityState.Modified;
                }
                try
                {
                    int result = context.SaveChanges();


                    ViewBag.tpregimen_id = new SelectList(tiporegimen, "id", "nombre");

                    ViewBag.acteco_id = new SelectList(context.acteco_tercero, "acteco_id", "acteco_nombre");
                    ViewBag.tpsuministro_id =
                        new SelectList(context.tiposumi_tercero, "tpsuministro_id", "tpsuministro_nombre");
                    // ViewBag.tpregimen_id = new SelectList(context.tpregimen_tercero, "tpregimen_id", "tpregimen_nombre");
                    ViewBag.tpimpu_id = new SelectList(context.tpimpu_tercero, "tpimpu_id", "tpimpu_nombre");
                    ViewBag.fpago_id = new SelectList(context.fpago_tercero, "fpago_id", "fpago_nombre");
                    ViewBag.tipo_proveedor = new SelectList(context.tipoproveedor, "tipo", "nombre");

                    PanelesActivos(proveedor.tercero_id);
                    if (result > 0)
                    {
                        TempData["mensaje"] = "La actualizacion del proveedor fue exitosa!";
                    }

                    users usuarioCreador =
                        context.users.FirstOrDefault(x => x.user_id == proveedor.tercprouserid_creacion);
                    ViewBag.user_nombre_cre = usuarioCreador.user_nombre + " " + usuarioCreador.user_apellido;
                    BuscarFavoritos(menu);
                    return View(proveedor);
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
                            TempData["mensaje_error"] = message;
                            raise = new InvalidOperationException(message, raise);
                        }
                    }

                    //throw raise;
                }
            }

            var tiporegimen2 = (from a in context.icb_terceros
                                join b in context.tpregimen_tercero
                                    on a.tpregimen_id equals b.tpregimen_id
                                where a.tercero_id == proveedor.tercero_id
                                select new
                                {
                                    id = b.tpregimen_id,
                                    nombre = b.tpregimen_nombre
                                }).ToList();
            ViewBag.tpregimen_id = new SelectList(tiporegimen2, "id", "nombre");

            ViewBag.acteco_id = new SelectList(context.acteco_tercero, "acteco_id", "acteco_nombre");
            ViewBag.tpsuministro_id =
                new SelectList(context.tiposumi_tercero, "tpsuministro_id", "tpsuministro_nombre");
            //  ViewBag.tpregimen_id = new SelectList(context.tpregimen_tercero, "tpregimen_id", "tpregimen_nombre");
            ViewBag.tpimpu_id = new SelectList(context.tpimpu_tercero, "tpimpu_id", "tpimpu_nombre");
            ViewBag.fpago_id = new SelectList(context.fpago_tercero, "fpago_id", "fpago_nombre");
            ViewBag.tipo_proveedor = new SelectList(context.tipoproveedor, "tipo", "nombre");
            PanelesActivos(proveedor.tercero_id);
            users usuarioCreador1 = context.users.FirstOrDefault(x => x.user_id == proveedor.tercprouserid_creacion);
            if (usuarioCreador1 != null)
            {
                ViewBag.user_nombre_cre = usuarioCreador1.user_nombre + " " + usuarioCreador1.user_apellido;
            }

            users usuarioActualizador1 =
                context.users.FirstOrDefault(x => x.user_id == proveedor.tercprouserid_actualizacion);
            if (usuarioActualizador1 != null)
            {
                ViewBag.user_nombre_act = usuarioActualizador1 != null
                    ? usuarioActualizador1.user_nombre + " " + usuarioActualizador1.user_apellido
                    : null;
            }

            BuscarFavoritos(menu);
            return View(proveedor);
        }

        [HttpGet]
        public ActionResult updateEmpleado(int? id, int? menu)
        {
            ViewBag.teremp_cargo = new SelectList(context.icb_cargo, "cargo_id", "cargo_nombre");
            ViewBag.teremp_departamento = new SelectList(context.departamento_gerencial, "dpto_id", "dpto_nombre");

            PanelesActivos(id ?? 0);
            BuscarFavoritos(menu);

            tercero_empleado buscarEmpleado = context.tercero_empleado.FirstOrDefault(x => x.tercero_id == id);
            //ViewBag.fechaContratacion = buscarEmpleado.fecha_contratacion != null ? buscarEmpleado.fecha_contratacion.Value.ToShortDateString() : "";
            if (buscarEmpleado != null)
            {
                ViewBag.fecha_contratacion = buscarEmpleado.fecha_contratacion != null
                    ? buscarEmpleado.fecha_contratacion.Value.ToString("yyyy/MM/dd HH:mm", new CultureInfo("en-US"))
                    : "";
            }
            //ConsultaDatosCreacion(buscarEmpleado.teremp_userid_creacion, buscarEmpleado.teremp_userid_actualizacion ?? 0); ERROR AQUÍ

            if (buscarEmpleado == null)
            {
                modelo_terceroempledo buscarempleado2 = new modelo_terceroempledo
                {
                    tercero_id = Convert.ToInt32(id),
                     teremp_estado=true,                     
                };
                return View(buscarempleado2);
            }
            else
            {
                modelo_terceroempledo buscarempleado2 = new modelo_terceroempledo
                {
                    tercero_id = buscarEmpleado.tercero_id,
                    emp_tercero_id = buscarEmpleado.emp_tercero_id,
                    fecha_contratacion = buscarEmpleado.fecha_contratacion != null
                        ? buscarEmpleado.fecha_contratacion.Value.ToString("yyyy/MM/dd", new CultureInfo("en-US")): "",
                    teremp_estado = buscarEmpleado.teremp__estado,
                    teremp_cargo = buscarEmpleado.teremp_cargo,
                    razoninactivo_id = buscarEmpleado.razoninactivo_id
                };
                return View(buscarempleado2);
            }
        }

        [HttpPost]
        public ActionResult updateEmpleado(modelo_terceroempledo empleado, int? menu)
        {
            if (ModelState.IsValid)
            {
                tercero_empleado buscarEmpleado = context.tercero_empleado.FirstOrDefault(x => x.tercero_id == empleado.tercero_id);
                if (buscarEmpleado == null)
                {
                    tercero_empleado nuevoempleado = new tercero_empleado
                    {
                        teremp_fec_creacion = DateTime.Now,
                        teremp_userid_creacion = Convert.ToInt32(Session["user_usuarioid"]),
                        tercero_id = empleado.tercero_id.Value,
                        fecha_contratacion = Convert.ToDateTime(empleado.fecha_contratacion),
                        teremp_cargo = empleado.teremp_cargo,
                        razoninactivo_id = empleado.razoninactivo_id,
                        teremp__estado=empleado.teremp_estado,
                    };

                    context.tercero_empleado.Add(nuevoempleado);
                    int guardar = context.SaveChanges();
                    if (guardar > 0)
                    {
                        TempData["mensaje"] = "La actualizacion del empleado fue exitosa!";
                    }
                    else
                    {
                        TempData["mensaje_error"] = "Error de conexion con base de datos, por favor verifique...!";
                    }
                }
                else
                {
                    empleado.teremp_userid_actualizacion = Convert.ToInt32(Session["user_usuarioid"]);
                    empleado.teremp_fec_actualizacion = DateTime.Now.ToString();
                    buscarEmpleado.teremp_userid_actualizacion = Convert.ToInt32(Session["user_usuarioid"]);
                    buscarEmpleado.teremp_fec_actualizacion = DateTime.Now;
                    buscarEmpleado.teremp_cargo = empleado.teremp_cargo;
                    //buscarEmpleado.teremp_departamento = empleado.teremp_departamento;
                    //buscarEmpleado.fecha_contratacion = Convert.ToString(empleado.fecha_contratacion) != null ? empleado.fecha_contratacion.Value.ToString("yyyy/MM/dd HH:mm", new CultureInfo("en-US")) : "";
                    buscarEmpleado.fecha_contratacion =
                        Convert.ToDateTime(empleado
                            .fecha_contratacion); //.Value.ToString("yyyy/MM/dd HH:mm", new CultureInfo("en-US"));

                    context.Entry(buscarEmpleado).State = EntityState.Modified;
                    int guardar = context.SaveChanges();
                    if (guardar > 0)
                    {
                        TempData["mensaje"] = "La actualizacion del empleado fue exitosa!";
                    }
                    else
                    {
                        TempData["mensaje_error"] = "Error de conexion con base de datos, por favor verifique...!";
                    }
                }
            }

            ViewBag.teremp_cargo = new SelectList(context.icb_cargo, "cargo_id", "cargo_nombre");
            ViewBag.teremp_departamento = new SelectList(context.departamento_gerencial, "dpto_id", "dpto_nombre");
            ViewBag.fecha_contratacion = !string.IsNullOrWhiteSpace(empleado.fecha_contratacion)
                ? empleado.fecha_contratacion
                : "";
            PanelesActivos(empleado.tercero_id.Value);
            BuscarFavoritos(menu);
            //ConsultaDatosCreacion(empleado);
            //var buscarEmpleadoA = context.tercero_empleado.FirstOrDefault(x => x.tercero_id == empleado.tercero_id);
            //ConsultaDatosCreacion(buscarEmpleadoA.teremp_userid_creacion, buscarEmpleadoA.teremp_userid_actualizacion ?? 0);
            return View(empleado);
        }

        public void ConsultaDatosCreacion(int idA, int? idC)
        {
            //consulta el nombre de usuario creacion con el id, lo envia a la vista a traves de ViewBag
            users creador = (from c in context.users
                             join b in context.tercero_empleado on c.user_id equals b.teremp_userid_creacion
                             where b.emp_tercero_id == idA
                             select c).FirstOrDefault();

            ViewBag.user_nombre_cre = creador != null ? creador.user_nombre + " " + creador.user_apellido : null;

            //consulta el nombre de usuario actualizacion con el id, lo envia a la vista a traves de ViewBag
            users actualizador = (from c in context.users
                                  join b in context.tercero_empleado on c.user_id equals b.teremp_userid_actualizacion
                                  where b.emp_tercero_id == idC
                                  select c).FirstOrDefault();

            ViewBag.user_nombre_act =
                actualizador != null ? actualizador.user_nombre + " " + actualizador.user_apellido : null;
        }

        public ActionResult clientesActivos(int? menu)
        {
            BuscarFavoritos(menu);
            return View();
        }

        public JsonResult bloquearClientes()
        {
            var data = (from c in context.tercero_cliente
                        join v in context.vw_doccartera
                            on c.tercero_id equals v.nit
                        where v.saldo > 0
                        select new
                        {
                            c.tercero_id,
                            c.tiempo_para_bloqueo,
                            v.fecha
                        }).ToList();

            foreach (var item in data)
            {
                if (item.fecha.AddDays(item.tiempo_para_bloqueo).CompareTo(DateTime.Now) > 0)
                {
                    tercero_cliente cliente = context.tercero_cliente.FirstOrDefault(x => item.tercero_id == x.tercero_id);
                    cliente.bloqueado = true;
                    cliente.motivo_bloqueado = "Bloqueado automaticamente por cartera el dia " + DateTime.Now;
                    context.Entry(cliente).State = EntityState.Modified;
                }
            }

            context.SaveChanges();
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public JsonResult BuscarTercerosPaginados()
        {
            var datos = (from tercero in context.icb_terceros
                         join ciudad in context.nom_ciudad
                             on tercero.ciu_id equals ciudad.ciu_id into ciud
                         from ciudad in ciud.DefaultIfEmpty()
                         select new
                         {
                             tercero.tercero_id,
                             tercero.doc_tercero,
                             nombre_completo = tercero.prinom_tercero == null
                                 ? tercero.razon_social
                                 : tercero.prinom_tercero + " " + tercero.segnom_tercero + " " + tercero.apellido_tercero + " " +
                                   tercero.segapellido_tercero,
                             tercero.tercero_estado,
                             ciudad.ciu_nombre,
                             fechaCreacion = tercero.tercerofec_creacion
                         }).ToList();

            var data = datos.Select(c => new
            {
                c.tercero_id,
                doc_tercero = c.doc_tercero ?? "",
                c.nombre_completo,
                tercero_estado = c.tercero_estado ? "Activo" : "Inactivo",
                ciu_nombre = c.ciu_nombre != null ? c.ciu_nombre : "",
                fechaCreacion = c.fechaCreacion != null
                    ? c.fechaCreacion.ToString("yyyy/MM/dd HH:mm", new CultureInfo("en-US"))
                    : ""
                //  fechaCreacion = tercero.tercerofec_creacion.ToString()  
            }).ToList();
            //try
            //{
            //    var draw = Request.Form.GetValues("draw").FirstOrDefault();
            //    var start = Request.Form.GetValues("start").FirstOrDefault();
            //    var length = Request.Form.GetValues("length").FirstOrDefault();
            //    var search = Request.Form.GetValues("search[value]").FirstOrDefault();
            //    var sortColumn = Request.Form.GetValues("columns[" + Request.Form.GetValues("order[0][column]").FirstOrDefault() + "][name]").FirstOrDefault();
            //    var sortColumnDir = Request.Form.GetValues("order[0][dir]").FirstOrDefault();
            //    search = search.Replace(" ","");
            //    int pageSize = Convert.ToInt32(length);
            //    int skip = Convert.ToInt32(start);

            //    var data = (from tercero in context.icb_terceros
            //                  join ciudad in context.nom_ciudad
            //                  on tercero.ciu_id equals ciudad.ciu_id into ciu
            //                  from ciudad in ciu.DefaultIfEmpty()
            //                  where tercero.doc_tercero.Contains(search) || (tercero.prinom_tercero+tercero.segnom_tercero+tercero.apellido_tercero+tercero.segapellido_tercero).Contains(search)
            //                  || ciudad.ciu_nombre.Contains(search) || (tercero.tercerofec_creacion.Day+"/"+ tercero.tercerofec_creacion.Month+"/"+tercero.tercerofec_creacion.Year).Contains(search)
            //                  select new
            //                  {
            //                      tercero.tercero_id,
            //                      doc_tercero = tercero.doc_tercero ?? "",
            //                      nombre_completo = tercero.prinom_tercero == null ? tercero.razon_social : tercero.prinom_tercero + " " + tercero.segnom_tercero + " " + tercero.apellido_tercero + " " + tercero.segapellido_tercero,
            //                      tercero_estado = tercero.tercero_estado == true ? "Activo" : "Inactivo",
            //                      ciu_nombre = ciudad.ciu_nombre != null ? ciudad.ciu_nombre : "",
            //                      fechaCreacion = tercero.tercerofec_creacion.ToString()
            //                  }).OrderBy(x => x.doc_tercero).Skip(skip).Take(pageSize).ToList();

            //    var totalRecords = (from tercero in context.icb_terceros
            //                join ciudad in context.nom_ciudad
            //                on tercero.ciu_id equals ciudad.ciu_id into ciu
            //                from ciudad in ciu.DefaultIfEmpty()
            //                where tercero.doc_tercero.Contains(search)
            //                select new
            //                {
            //                    tercero.tercero_id
            //                }).Count();

            //    return Json(new { draw = draw, recordsFiltered = totalRecords, recordsTotal = totalRecords, data = data }, JsonRequestBehavior.AllowGet);
            //}
            //catch (Exception ex)
            //{
            //    return Json(new { errorMessage = ex.Message }, JsonRequestBehavior.AllowGet);
            //    // Exception
            //}
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public JsonResult AgregarContactoTercero(int idTercero, string nombre, string direccion, string telefono,
            DateTime? cumpleanos, string correo, int tipo_contacto, int? tipo_documento, string cedula)
        {
            icb_contacto_tercero nuevoContacto = new icb_contacto_tercero
            {
                tercero_id = idTercero,
                con_tercero_nombre = nombre,
                con_tercero_direccion = direccion,
                con_tercero_telefono = telefono,
                con_tercerofec_creacion = DateTime.Now,
                con_tercerouserid_creacion = Convert.ToInt32(Session["user_usuarioid"]),
                mail = correo,
                fecha_cumpleaños = cumpleanos,
                tipocontacto = tipo_contacto,
                cedula = cedula,
                tipo_documento = tipo_documento
            };
            context.icb_contacto_tercero.Add(nuevoContacto);
            int result = context.SaveChanges();
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public JsonResult ActualizarContactos(int idTercero)
        {
            List<icb_contacto_tercero> contactos = context.icb_contacto_tercero.Where(x => x.tercero_id == idTercero).ToList();
            var result = contactos.Select(x => new
            {
                x.con_tercero_id,
                x.con_tercero_direccion,
                x.con_tercero_nombre,
                x.con_tercero_telefono,
                correo = x.mail.ToString(),
                cumpleanos = x.fecha_cumpleaños != null ? x.fecha_cumpleaños.Value.ToShortDateString() : "",
                tipo_contacto = x.tipocontactotercero.decripcion.ToString(),
                x.cedula
            });
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public JsonResult eliminarContacto(int idContacto)
        {
            bool result = false;
            icb_contacto_tercero contactoBusqueda = context.icb_contacto_tercero.FirstOrDefault(x => x.con_tercero_id == idContacto);
            if (contactoBusqueda != null)
            {
                context.icb_contacto_tercero.Attach(contactoBusqueda);
                context.icb_contacto_tercero.Remove(contactoBusqueda);
                result = context.SaveChanges() > 0;
            }

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public JsonResult buscarPais()
        {
            var data = from p in context.nom_pais
                       where p.pais_estado
                       select new
                       {
                           id = p.pais_id,
                           nombre = p.pais_nombre
                       };
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public JsonResult buscarDepartamentos(int? idPais)
        {
            var data = from d in context.nom_departamento
                       where d.pais_id == idPais
                             && d.dpto_estado
                       select new
                       {
                           id = d.dpto_id,
                           nombre = d.dpto_nombre
                       };
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public JsonResult buscarCiudades(int? idDepto)
        {
            var data = (from d in context.nom_ciudad
                        where d.dpto_id == idDepto
                              && d.ciu_estado
                        select new
                        {
                            id = d.ciu_id,
                            nombre = d.ciu_nombre
                        }).OrderBy(x => x.nombre).ToList();

            List<SelectListItem> listaCiudad = new List<SelectListItem>();

            foreach (var item in data)
            {
                listaCiudad.Add(new SelectListItem { Value = item.id.ToString(), Text = item.nombre });
            }

            ViewBag.ciu_id = listaCiudad;

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public JsonResult buscarSector(int idCiudad)
        {
            var data = from d in context.nom_sector
                       where d.ciudad_id == idCiudad
                             && d.sec_estado
                       select new
                       {
                           id = d.sec_id,
                           nombre = d.sec_nombre
                       };

            List<SelectListItem> listaSector = new List<SelectListItem>();

            foreach (var item in data)
            {
                listaSector.Add(new SelectListItem { Value = item.id.ToString(), Text = item.nombre });
            }

            ViewBag.sector_id = listaSector;

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public JsonResult buscarDirecciones(int tercero_id)
        {
            var data = from t in context.terceros_direcciones
                       where t.idtercero == tercero_id
                       select new
                       {
                           t.id,
                           //t.pais,
                           t.sector,
                           //t.departamento,
                           t.ciudad,
                           t.direccion
                       };
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public JsonResult buscarLpreciosVehiculos()
        {
            var data = context.vlistanuevos.GroupBy(x => new { x.lista, x.concepto, x.ano, x.mes })
                .Select(x => new { x.Key.lista, x.Key.concepto, x.Key.ano, x.Key.mes }).ToList();
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public JsonResult BuscarClientesActivos()

        {
            

            var data = (from t in context.icb_terceros
                        join c in context.tercero_cliente
                            on t.tercero_id equals c.tercero_id
                        join tc in context.tipocliente
                            on c.tipo_cliente equals tc.tipo into tc
                        from tcu in tc.DefaultIfEmpty()
                        where  ( t.tercero_estado == true )
                        select new
                        {

                            // t.acteco_tercero
                            asesor = t.asesor_id!=null?t.users.user_nombre + " " + t.users.user_apellido:"",
                            nombre = t.prinom_tercero + " " + t.segnom_tercero + "" + t.apellido_tercero + " " +
                                     t.segapellido_tercero,
                            t.doc_tercero,
                            t.email_tercero,
                            t.celular_tercero,
                            t.telf_tercero,
                            c.cupocredito,
                            c.pagina_web,
                            t.fec_nacimiento,
                            clasificacion = tcu.nombre
                        }).ToList() ;



            var data2 = data.Select(d => new {
                d.asesor,
                d.nombre,
                identificacion = d.doc_tercero != null ? d.doc_tercero : "",
                correo = d.email_tercero != null ? d.email_tercero : "",
                celular = d.celular_tercero != null ? d.celular_tercero : "",
                telefono = d.telf_tercero != null ? d.telf_tercero : "",
                cupocredito = d.cupocredito != null ? d.cupocredito.Value.ToString("N2", new CultureInfo("is-IS")) : "",
                //c.cupocredito,
                ///////////////////////////////////
                pagina_web = !string.IsNullOrWhiteSpace(d.pagina_web)? d.pagina_web : "",
                fecha_nacimiento = d.fec_nacimiento != null ? d.fec_nacimiento.Value.ToString("yyyy/MM/dd", new CultureInfo("en-US")) : "",
                //en-US
                //fecha_nacimiento = t.fec_nacimiento != null ? t.fec_nacimiento.ToString() : "",
                clasificacion = d.clasificacion
            }).ToList();
            return Json(data2, JsonRequestBehavior.AllowGet);
        }

        public int EliminarDireccion(int id)
        {
            terceros_direcciones dato = context.terceros_direcciones.Find(id);
            context.Entry(dato).State = EntityState.Deleted;
            int result = context.SaveChanges();

            return result;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                context.Dispose();
            }

            base.Dispose(disposing);
        }

        public JsonResult PermisoModificarInfoContable()
        {
            int usuario = Convert.ToInt32(Session["user_usuarioid"]);
            //Validamos que el usuario loguado tenga el rol y el permiso para modificar los valores del pedido
            int permiso = (from u in context.users
                           join r in context.rols
                               on u.rol_id equals r.rol_id
                           join ra in context.rolacceso
                               on r.rol_id equals ra.idrol
                           where u.user_id == usuario /*&& u.rol_id == 4*/ && ra.idpermiso == 9
                           select new
                           {
                               u.user_id,
                               u.rol_id,
                               r.rol_nombre,
                               ra.idpermiso
                           }).Count();

            return Json(permiso, JsonRequestBehavior.AllowGet);
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