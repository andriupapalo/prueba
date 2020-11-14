
idUrl = 0;
idMenu = 0;
function valida_Sb(id, id_menu) {
    window.location.href = '../vpedidos/Edit?menu='+id_menu+'&id=' + id;
}


function seguimiento_Sb(id, id_menu) {
    window.location.href = '../vpedidos/Seguimiento?menu='+id_menu+'&id=' + id;
}

function facturar_Sb(idPedido, id_menu, bodega, pedido, planmayor, modelo, autorizado) {
    idUrl = idPedido;
    idMenu = id_menu;
    $('#tipo_sb').prop('disabled', false)
    $('#selectBodegas').prop('disabled', false)
    //if (autorizado == "true") {
    //    $('#bodegaOculto').val('')
    //    $('#pedidoOculto').val('')
    //    $('#bodegaOculto').val(bodega)
    //    $('#pedidoOculto').val(pedido)
    //    $('#modalFacturacionVehiculo').modal('show')
    //}else {
        $.ajax({
            url: '/vpedidos/verificarPendientes',
            data: {
                idPedido: idPedido,
            },
            type: "post",
            cache: false,
            success: function (data) {
                if (data.solicitar == false) {
                    $('#bodegaOculto').val('')
                    $('#pedidoOculto').val('')
                    $('#bodegaOculto').val(bodega)
                    $('#pedidoOculto').val(pedido)
                    $('#modalFacturacionVehiculo').modal('show')
         
                    function definirFactura(param){
                        var X ='@ViewBag.facid';
                        window.open('@Url.Action("crearPDFfacturacionrepuestos", "FacturacionRepuestos")?id=' + X+'&&param='+param,'_blank');
                        //window.open.href = '../../FacturacionRepuestos/crearPDFfacturacionrepuestos?id=' + X + '&&param=' + param;
                    }
                }else {
                    swal({
                        title: 'Advertencia',
                        text: 'Este pedido no cumple con los requisitos para ser facturado ¿Desea enviar la solicitud para facturar el vehiculo ' + planmayor + ' - ' +modelo + '?',
                        type: "warning",
                        showCancelButton: true,
                        confirmButtonColor: "#DD6B55",
                        confirmButtonText: "Si, solicitar!",
                        cancelButtonText: "No, cancelar!",
                        closeOnConfirm: false,
                        closeOnCancel: false
                    },
                    function (isConfirm) {
                        if (isConfirm) {
                            if (planmayor == null || planmayor == "") {
                                swal("Este pedido no tiene plan mayor!", "", "warning");
                            }else{
                                $.ajax({
                                    url: '/vpedidos/EnviarSolicitudFacturacion',
                                    data: {
                                        plan_mayor: planmayor,
                                        modelo: modelo,
                                    },
                                    type: "post",
                                    cache: false,
                                    success: function (data) {

                                        if (data == -1) {
                                            swal("", "Error al enviar la notificación", "error");
                                        }
                                        if (data == 0) {
                                            swal("", "No existen usuarios parametrizados para enviar una notificación", "warning");
                                        }
                                        if (data == 1) {
                                            swal("", "Notificación enviada correctamente", "success");
                                        }
                                        if (data == 2) {
                                            swal("", "Ya se envio una notificación anteriormente del vehículo seleccionado", "warning");
                                        }
                                    }
                                })
                            }
                        }
                        else {
                            swal("Cancelado","", "error");
                        }
                    });
                }
            } 
        })
    }


function facturarRepuesto_Sb(id, id_menu, bodega, pedido, cliente) {
    idUrl = id;
    idMenu = id_menu;
    $('#tipoFR').prop('disabled', false)
    $('#selectBodegasFR').prop('disabled', false)
    $('#bodegaOcultoFR').val('')
    $('#pedidoOcultoFR').val('')
    $('#idClienteOculto').val('')

    $('#bodegaOcultoFR').val(bodega)
    $('#pedidoOcultoFR').val(pedido)
    $('#idClienteOculto').val(cliente)
    $('#modalFacturacionRepuestos').modal('show')
}

function tracking_Sb(id, id_menu, planmayor, apertura) {
    idUrl = id;
    idMenu = id_menu;
    apertura = (apertura == undefined) ? 0 : apertura;
    $('#tablaTracking').find('tbody').empty();

    $('#planValue').val(planmayor);

    $.ajax({
        url: '/vpedidos/buscarTracking',
        data: {
            planmayor: planmayor,
        },
        type: "post",
        cache: false,
        success: function (data) {
            //console.log(data)
            $('#tablaTracking').find('tbody').empty();
            if (data.info == true) {
                if (apertura == 1) {
                    $('#modalTracking').modal('show');
                }
                for (var i = 0; i < data.data2.length; i++) {
                    $('#tablaTracking').find('tbody').append('<tr>'
                        +'<td align="right">'+ data.data2[i].codigo + '</td>'
                        +'<td align="left">' + data.data2[i].nombre + '</td>'
                        +'<td align="left">' + data.data2[i].bodega + '</td>'
                        +'<td align="left">' + data.data2[i].fecha + '</td>'
                        +'<td align="left">' + data.data2[i].usuario + '</td>'
                        +'<td align="center">'
                            +'<button type="button" class="btn btn-info btn-sm" id="ver_' + data.data2[i].codigo + '" name="ver_' + data.data2[i].codigo + '" onclick="verArchivo(' + '\''  +data.data2[i].cargado + '\')"><i class="fa fa-eye"></i>&nbsp;&nbsp;&nbsp;ver</button>'
                        +'</td>'
                        +'</tr>');
                }
            }
        },
        complete: function (data) {
            console.log(data)
            
            for (var i = 0; i < data.responseJSON.data2.length; i++) {
                if (data.responseJSON.data2[i].cargado == 0) {
                    $('#ver_'+ data.responseJSON.data2[i].codigo).hide();
                }else {
                    $('#ver_'+ data.responseJSON.data2[i].codigo).show();
                }
            }
        }
    })
}

function verifica_Sb(id, id_menu) {
    window.location.href = '../vpedidos/Verificar?menu=' + id_menu + '&id=' + id;
}

$('#irFacturacionFR').click(function () {
    if ($('#bodegaOcultoFR').val() == $('#selectBodegasFR').val()) {
        window.location.href = '../../FacturacionRepuestos/FacturacionRepuestosBackOffice?id='+idUrl+'&menu='+idMenu+'&backoffice=' + $('#tipoFR').val() + ',' + $('#selectBodegasFR').val() + ',' + $('#pedidoOcultoFR').val() + ',' + $('#idClienteOculto').val();
        $('#msjErrorBodegasFR').hide()
    } else {
        $('#msjErrorBodegasFR').show()
        setTimeout(function () {
            $("#msjErrorBodegasFR").fadeOut(2000);
        }, 8000);
    }
});

$('#irFacturacion').click(function () {
    if ($('#bodegaOculto').val() == $('#selectBodegas').val()) {
        window.location.href = '../../facturacionNuevos/FacturacionNuevosBackoffice?id=' + idUrl + '&menu=' + idMenu + '&backoffice=' + $('#tipo_sb').val() + ',' + $('#selectBodegas').val() + ',' + $('#pedidoOculto').val();
        $('#msjErrorBodegas').hide()
    } else {
        $('#msjErrorBodegas').show()
        setTimeout(function () {
            $("#msjErrorBodegas").fadeOut(2000);
        }, 8000);
    }
});

$('#modalTracking').on('shown.bs.modal', function () {
    if ($('#planValue').val() == null || $('#planValue').val() == "") {
        $('#registroEvento').hide();
    } else {
        setInterval(function () { tracking($('#planValue').val()); }, 3000);
        $('#registroEvento').show();
    }
})

$('#cerrarModal').click(function () {
    $('#modalTracking').hide();
});

$('#registroEvento').click(function () {
    registrarEvento_Sb($('#planValue').val());
});

$('#tipo_sb').change(function () {
    $.ajax({
        url: '/gestionVhNuevo/BuscarBodegasPorDocumento',
        data: {
            id: $('#tipo_sb').val()
        },
        type: "post",
        cache: false,
        success: function (data) {
            $('#selectBodegas').empty();
            $('#selectBodegas').append($('<option>', {
                value: '',
                text: ''
            }));
            for (var i = 0; i < data.buscarBodega.length; i++) {
                $('#selectBodegas').append($('<option>', {
                    value: data.buscarBodega[i].id,
                    text: data.buscarBodega[i].bodccs_nombre
                }));
            }
        }
    })
})

$('#tipoFR').change(function () {
    $.ajax({
        url: '/gestionVhNuevo/BuscarBodegasPorDocumento',
        data: {
            id: $('#tipoFR').val()
        },
        type: "post",
        cache: false,
        success: function (data) {
            $('#selectBodegasFR').empty();
            $('#selectBodegasFR').append($('<option>', {
                value: '',
                text: ''
            }));
            for (var i = 0; i < data.buscarBodega.length; i++) {
                $('#selectBodegasFR').append($('<option>', {
                    value: data.buscarBodega[i].id,
                    text: data.buscarBodega[i].bodccs_nombre
                }));
            }
        }
    })
})

function registrarEvento_Sb(planmayor) {
    window.location.href = '../EventoVehiculo/Registrar?id=' + idUrl + '&menu=' + idMenu + '&planmayor=' + planmayor;
}
