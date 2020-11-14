using Homer_MVC.IcebergModel;
using Homer_MVC.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Homer_MVC.Controllers
{
    public class NewPerfilTtributarioController : Controller
    {
        private Iceberg_Context context = new Iceberg_Context();


        public void CamposListasDesplegables()
        {
            ViewBag.bodega = new SelectList(context.bodega_concesionario.OrderBy(x => x.bodccs_nombre), "id", "bodccs_nombre");
            ViewBag.sw = new SelectList(context.tp_doc_sw.OrderBy(x => x.Descripcion), "tpdoc_id", "Descripcion");
            ViewBag.tipo_regimenid = new SelectList(context.tpregimen_tercero.OrderBy(x => x.tpregimen_nombre), "tpregimen_id", "tpregimen_nombre");
            ViewBag.conc_regimenid = new SelectList(context.con_ret_del_reg.OrderBy(x => x.descripcion_con), "id_concepto", "descripcion_con");
            //var dataContributario = context.contributario;
            //ViewBag.retfuente = new SelectList(dataContributario.OrderBy(x => x.descripcion), "codigo", "descripcion");
            //ViewBag.retiva = new SelectList(dataContributario.OrderBy(x => x.descripcion), "codigo", "descripcion");
            //ViewBag.retica = new SelectList(dataContributario.OrderBy(x => x.descripcion), "codigo", "descripcion");
            //ViewBag.autorretencion = new SelectList(dataContributario.OrderBy(x => x.descripcion), "codigo", "descripcion");
        }
        // GET: NewPerfilTtributario
      
        public ActionResult Create(int? menu)
        {
            CamposListasDesplegables();
            ModeloNewPerfilTributarios modelo = new ModeloNewPerfilTributarios() { estado = true };
         //   BuscarFavoritos(menu);
            return View(modelo);
        }


        [HttpPost]
        public ActionResult Create(ModeloNewPerfilTributarios modelo, int? menu)
        {
            var bodegasSeleccionadas = Request["bodega"];
            if (ModelState.IsValid)
            {

                if (!string.IsNullOrEmpty(bodegasSeleccionadas))
                {
                    string[] bodegasId = bodegasSeleccionadas.Split(',');
                    List<int> listaBodegas = new List<int>();
                    foreach (var item in bodegasId)
                    {
                        listaBodegas.Add(Convert.ToInt32(item));
                    }
                    var buscarPerfil = (from perfilTibutario in context.parametrizacion_retenciones
                                        join bodega in context.bodega_concesionario
                                        on perfilTibutario.id_Bodega equals bodega.id
                                        where listaBodegas.Contains(perfilTibutario.id_Bodega) 
                                        && perfilTibutario.id_SW == modelo.id_SW 
                                        && perfilTibutario.id_RegimenTributario == modelo.id_RegimenTributario
                                        && perfilTibutario.id_Retencion == modelo.id_Retencion
                                        && perfilTibutario.id_Concepto == modelo.id_Concepto
                                        select new
                                        {
                                            bodega.bodccs_nombre
                                        }).FirstOrDefault();
                    if (buscarPerfil != null)
                    {
                        TempData["mensaje_error"] = "El perfil ya se encuentra registrado para la bodega " + buscarPerfil.bodccs_nombre;
                    }
                    else
                    {
                        foreach (var bodegaId in listaBodegas)
                        {
                            var nuevoperfil = new parametrizacion_retenciones
                            {
                                id_Bodega = bodegaId,
                                id_SW = modelo.id_SW,
                                id_RegimenTributario = modelo.id_RegimenTributario,
                                id_Retencion = modelo.id_Retencion,
                                id_Concepto = modelo.id_Concepto,
                                Porcentaje = modelo.Pordentaje ?? 0,
                                Base = modelo.Base ?? 0,
                                id_licencia = modelo.id_licencia,
                                fec_creacion = DateTime.Now,
                                userid_creacion = Convert.ToInt32(Session["user_usuarioid"]),
                                //autorretencion = modelo.autorretencion,
                                estado = modelo.estado,
                               // razon_inactivo = modelo.razon_inactivo
                            };
                            context.parametrizacion_retenciones.Add(nuevoperfil);


                        }
                        var guardar = context.SaveChanges();
                        if (guardar > 0)
                        {
                            TempData["mensaje"] = "El perfil ha sido creado exitosamente";
                            ViewBag.bodegasSeleccionadas = bodegasSeleccionadas;
                            return RedirectToAction("Create");
                        }
                        else
                        {
                            TempData["mensaje_error"] = "Error de base de datos, por favor revise su conexion";
                        }
                    }
                }
            }
            CamposListasDesplegables();
          //  BuscarFavoritos(menu);
            ViewBag.bodegasSeleccionadas = bodegasSeleccionadas;
            return View(modelo);
        }

    }
}