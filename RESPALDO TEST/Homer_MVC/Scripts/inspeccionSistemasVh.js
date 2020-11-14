var cont = 0;
var clss = $(".msRadio");
var sistemas_cont = clss.length;
var idSetInterval = 0;
var operacionesSinConf = []

function check_style() { 
    $('.i-checks').iCheck({
        checkboxClass: 'icheckbox_square-green',
        radioClass: 'iradio_square-green'
    })
}

check_style()

function inspeccionVhSig(autorz) {
    var idot = $('#id').val()
    var data = {
        idot:idot,
        autorz:autorz
    }
    $.ajax({
        url: '/SignosVitales/signos',
        type:"GET",
        data: data,
        success: function (resp) {
            $('#dv_sistemasVh').html(resp)
            console.log(" _ f _ ")
            if (autorz == 1) {
                /// $("#insp_tab").attr("onclick", "inpeccionVhSig(" + autorz + ")");
                $("#insp_tab").removeAttr('onclick')
                $('[href="#inspeccion"]').show();
                $("#insp_tab").click()
            }
        }
    })
}

function terminar_inspeccion() {
    var data = $('#form_signos').serialize();
    var url_brw = '/SignosVitales/tablas';
    var chckd = 0;
    for (var i in clss) {
        try {
            var idn_c = clss[i].getAttribute('rw_idn')
            if (document.getElementById('sist_a_' + idn_c).checked)
                ++chckd;
            else if (document.getElementById('sist_b_' + idn_c).checked)
                ++chckd;
            else if (document.getElementById('sist_c_' + idn_c).checked)
                ++chckd;
        } catch (e) {
            break;
        }
    }
    if (chckd < sistemas_cont) {
        console.log('Ponga un estado a todos los signos')
        return false
    } else {
        console.log('Ya se llenaron todos los signos')
    }
    $.ajax({
        url:url_brw,
        data:data,
        type:'POST',
        success: function (resp) {
            $('#dv_operacionesVh').html(resp);
            //$('#btn_terminar_inspeccion').remove()
            $('#btn_terminar_inspeccion').hide()
            CargarDatosPorOrdenId();

        }
    })
            
}

function cargarTablas() {
    var url_brw = '/SignosVitales/tablasVw';
    var data = {
        ot_id: $('#id').val()
    }
    $.ajax({
        url: url_brw,
        data: data,
        type: 'POST',
        success: function (resp) {
            $('#dv_operacionesVh').html(resp);
        }
    })
}

function elim_op(idn,opc_slc) {
    var idot = $('#id').val()
    var data = {
        oprid: idn,
        idot: idot
    }
    $.ajax({
        url: '/SignosVitales/eliminarOperacion',
        type: "POST",
        data:data,
        success: function (resp) {
            console.log(resp)
            if (resp.resl == 1) {
                $("#opr_" + opc_slc + "_" + idn).remove()
                operacionesSlc('C')
                operacionesSlc('B')
                CargarDatosPorOrdenId()
                OperacionesSinConfirmacion()
            }
        }
    })
}

function addFill(opc_slc) {
    var data = {
        ot_id: $('#id').val(),
        op_id: $('#operaciones' + opc_slc + ' option:selected').val(),
        autorz: $('#autorz').val(),
        estadoOp: opc_slc
    }
    $.ajax({
        url: "/SignosVitales/filaVw",
        type: "POST",
        data: data,
        success: function (resp) {
            console.log(resp)
            if (resp.trim() != "") {
                $("#tb_" + opc_slc + " tbody").append(resp)
                operacionesSlc('C')
                operacionesSlc('B')
                check_style()
                OperacionesSinConfirmacion()
                if ($('#autorz').val().trim().toLowerCase() == 'true')
                    CargarDatosPorOrdenId()
            }
        }
    })
} 

function operacionesSlc(slc_opc) {
   
    var data = {
        idot: $('#id').val()
    }
    $.ajax({
        url: '/SignosVitales/operacionesSlc',
        type: 'GET',
        data: data,
        success: function (resp) {
            $('#operaciones' + slc_opc).empty()
            $('#operaciones' + slc_opc).html(resp)
        }
    })
}


function enviomsm(idins) {
    var data = { idins_op: idins }
    $.ajax({
        url: '/SignosVitales/envioMensajeTexto',
        type: 'GET',
        data:data,
        success: function (resp) {
            console.log(resp)
            $('#msm_envioM' + idins).removeAttr('onclick')
            if (resp.resp == 1) {
                $('#msm_envioM' + idins).removeClass('fa-envelope-o')
                $('#msm_envioM' + idins).addClass('fa-envelope')
            }
        }
    })
}

function mostrar_detalle(idins_op) {
    var data = {
        idins_op: idins_op
    }
    $.ajax({
        url: '/SignosVitales/observacionMensajeTexto',
        type:'GET',
        data:data,
        success:function(resp){
            swal("", resp, "info");
        }
    })
}

function mostrar_detalle3(idins_op) {
    var data = {
        id: idins_op
    }
    $.ajax({
        url: '/OrdenTaller/observacionMensajeTexto',
        type:'GET',
        data:data,
        success:function(resp){
            swal("", resp, "info");
        }
    })
}


function formatoFila(dato) {
    console.log('#opr_C_' + dato.tinsvh_id)
    var opc = [{ 'est': 'danger', 'ico': 'fa fa-times-circle' }, { 'est': 'success', 'ico': 'fa fa-check-circle' }]
    var sz = 'font-size:17pt';
    var i = (dato.mnobra_id == null) ? 0 : 1 ; 
    var td = "<i style='" + sz + "' class='" + opc[i].ico + " text-" + opc[i].est + "'></i><a onclick='mostrar_detalle(" + dato.tinsvh_id + ")'>" +
             "<i style='margin-left:15px;" + sz + "' class='fa fa-file-text text-" + opc[i].est + "'></i></a>";
    $('#opr_C_' + dato.tinsvh_id).addClass(opc[i].est)
    $('#td_insp_' + dato.tinsvh_id).html(td)
}

function deleteInterval() {
    console.log('A eliminar:' + idSetInterval)
    //if (operacionesSinConf.length > 0)
        clearInterval(idSetInterval)
}

function OperacionesSinConfirmacion() {
    var data = { ot_id: $('#id').val() }
    $.ajax({
        url: '/SignosVitales/operacionesSinConfirmacion',
        type: 'GET',
        data: data,
        success: function (resp) {
            deleteInterval()
            operacionesSinConf = resp
            console.log("operacionesSinConf ")
            console.log(operacionesSinConf)
            if (operacionesSinConf.length > 0)
                establecerIntervaloSms()
        }
    })
}

function OperacionesConfirmadas() {
    var data = {
        ot_id: $('#id').val(),
        operaciones: operacionesSinConf.toString()
    }
    console.log(data)
    if (data.operaciones.trim() != '') {
        $.ajax({
            url: '/SignosVitales/operacionesConfirmadas',
            type: 'GET',
            data: data,
            success: function (resp) {
                var opr = resp.length;
                if (opr > 0) {
                    for (var i = 0; i < opr; i++) {
                        console.log(resp[i])
                        formatoFila(resp[i])
                        var pos = operacionesSinConf.indexOf(resp[i].tinsvh_id);
                        operacionesSinConf.splice(pos, 1)
                        if (resp[i].mnobra_id != null)
                            CargarDatosPorOrdenId()
                    }

                    if (operacionesSinConf.length == 0) {
                        deleteInterval()
                        console.log('Se hizo lo que se pudo...')
                    } else {
                        console.log('No se pudo...')
                    }
                    console.log('Probando 123 ...' + operacionesSinConf.length)
                } 
                console.log(operacionesSinConf)
                if (operacionesSinConf.length == 0) {
                    deleteInterval()
                    console.log('Se hizo lo que se pudo...')
                }
            }
        })
    } else {
        deleteInterval()
        console.log('Se hizo lo que se pudo...')
    }
}

function establecerIntervaloSms() {
    console.log('Inicio:' + idSetInterval)

    if (operacionesSinConf.length > 0)
        idSetInterval = setInterval(OperacionesConfirmadas, 10000)

    console.log('Fin:' + idSetInterval)
}


OperacionesSinConfirmacion()






