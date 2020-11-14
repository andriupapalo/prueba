$('#btnIniciarInspeccion').click(function () {
    var cantidadItemsRojos = 0;
    $('#Kilometraje').prop('readonly', true)
    $.ajax({
        url: '/InspeccionVh/BuscarListaInspeccion',
        data: {
            id_cita: $('#id_cita').val()
        },
        type: "post",
        cache: false,
        success: function (data) {
            // Si la cita ya tiene una fecha incicial y fiaal, la inspeccion ya esta hecha, por tanto no se atiende nuevamente
            if (data.citaYaAtendida == true) {
                $('#txtAlertaInspeccion').text(data.mensajeAlerta);
                $('#areaAlertaInspeccion').show();
                setTimeout(function () {
                    $("#areaAlertaInspeccion").fadeOut(2500);
                }, 10000);

            } else {

                var conceptos = '';

                for (var i = 0; i < data.buscarConceptos.length; i++) {

                    var categoria = '';
                    for (var j = 0; j < data.buscarConceptos[i].categorias.length; j++) {

                        var items = '';
                        for (var k = 0; k < data.buscarConceptos[i].categorias[j].items.length; k++) {

                            if (data.buscarConceptos[i].categorias[j].items[k].tiporespuesta == 'listaEstados') {

                                var listaColores = '<div class="" data-toggle="buttons"><input type="text" style="display:none;" class="form-control" id="respuestaItem' + data.buscarConceptos[i].categorias[j].items[k].id + '" name="respuestaItem' + data.buscarConceptos[i].categorias[j].items[k].id + '"/>';
                                for (var col = 0; col < data.listaEstados.length; col++) {
                                    listaColores += '<label class="btn" onclick="mostrarDetalle(' + data.buscarConceptos[i].categorias[j].items[k].id + ',' + data.listaEstados[col].id + ',' + '\'' + data.listaEstados[col].descripcion + '\')" style="background-color:' + data.listaEstados[col].color + '">' +
                                                        '<input type="radio" name="itemSelected' + data.buscarConceptos[i].categorias[j].items[k].id + '" id="itemSelected' + data.buscarConceptos[i].categorias[j].items[k].id + data.listaEstados[col].id + '" autocomplete="off">' +
                                                        '<span class="glyphicon glyphicon-ok"></span>' +
                                                    '</label>&nbsp;&nbsp;&nbsp;'
                                }
                                listaColores += '</div>';

                                items += ' <div class="col-sm-12">' +
                                                '<div class="form-group">' +
                                                    '<label class="control-label col-md-4">' + data.buscarConceptos[i].categorias[j].items[k].Descripcion + ':&nbsp;</label>' +
                                                    '<div class="col-md-6">' +

                                                        listaColores +

                                                    '</div>' +
                                                '</div>' +
                                            '</div>';
                            } else if (data.buscarConceptos[i].categorias[j].items[k].tiporespuesta == 'text') {
                                items += ' <div class="col-sm-12">' +
                                                '<div class="form-group">' +
                                                    '<label class="control-label col-md-4">' + data.buscarConceptos[i].categorias[j].items[k].Descripcion + ':&nbsp;</label>' +
                                                    '<div class="col-md-6">' +

                                                        '<input type="text" class="form-control" id="respuestaItem' + data.buscarConceptos[i].categorias[j].items[k].id + '" name="respuestaItem' + data.buscarConceptos[i].categorias[j].items[k].id + '" placeholder="Digite ' + data.buscarConceptos[i].categorias[j].items[k].Descripcion + '"/>' +

                                                    '</div>' +
                                                '</div>' +
                                            '</div>';
                            } else if (data.buscarConceptos[i].categorias[j].items[k].tiporespuesta == 'select') {

                                var opciones = '';
                                for (var opcion = 0; opcion < data.buscarConceptos[i].categorias[j].items[k].respuestasSelect.length; opcion++) {
                                    opciones += '<option value="' + data.buscarConceptos[i].categorias[j].items[k].respuestasSelect[opcion].id + '">' +
                                                        data.buscarConceptos[i].categorias[j].items[k].respuestasSelect[opcion].descripcion
                                    '</option>';
                                }


                                items += ' <div class="col-sm-12">' +
                                                '<div class="form-group">' +
                                                    '<label class="control-label col-md-4">' + data.buscarConceptos[i].categorias[j].items[k].Descripcion + ':&nbsp;</label>' +
                                                    '<div class="col-md-6">' +

                                                        '<select class="form-control" id="respuestaItem' + data.buscarConceptos[i].categorias[j].items[k].id + '" name="respuestaItem' + data.buscarConceptos[i].categorias[j].items[k].id + '" placeholder="Seleccione">' +
                                                            opciones +
                                                        '</select>' +

                                                    '</div>' +
                                                '</div>' +
                                            '</div>';
                            }

                        }

                        categoria += '<div class="hpanel">' +
                                        '<div class="panel-heading hbuilt" style="background-color:#e4e5e7">' +
                                            '<div class="panel-tools">' +
                                                '<a class="showhide"><i class="fa fa-chevron-up"></i></a>' +
                                            '</div>' +
                                            '<i class="fa fa-car"></i>&nbsp;&nbsp;&nbsp;' + data.buscarConceptos[i].categorias[j].descripcion +
                                        '</div>' +

                                            '<div class="panel-body">' +

                                                    items +

                                            '</div>' +
                                       '</div>';

                    }

                    conceptos += '<div class="hpanel">' +
                                    '<div class="panel-heading hbuilt" style="background-color:#e4e5e7">' +
                                        '<div class="panel-tools">' +
                                            '<a class="showhide"><i class="fa fa-chevron-up"></i></a>' +
                                        '</div>' +
                                        '<i class="fa fa-car"></i>&nbsp;&nbsp;&nbsp;' + data.buscarConceptos[i].Descripcion +
                                    '</div>' +

                                    '<div class="panel-body">' +
                                          categoria +
                                    '</div>' +
                                '</div>';
                }
                $('#areaItemsInspeccion').html(conceptos);
                $('#areaTablaTareasAsignadas').show();
            }
        },
        complete: function (data) {

        }
    })
});

function mostrarDetalle(idItem, idRespuesta, descripcionRta) {
    $('#respuestaItem' + idItem + '').val(descripcionRta);

    $.ajax({
        url: '/InspeccionVh/BuscarTareasItem',
        data: {
            id_item: idItem,
            id_color: idRespuesta
        },
        type: "post",
        cache: false,
        success: function (data) {
            if (data.buscarTarea.length > 0) {
                $('#selectTareas').empty();
                $('#selectTareas').append($('<option>', {
                    value: '',
                    text: ''
                }));
                for (var i = 0; i < data.buscarTarea.length; i++) {
                    $('#selectTareas').append($('<option>', {
                        value: data.buscarTarea[i].id,
                        text: data.buscarTarea[i].descripcion
                    }));
                    $('#selectItems').append($('<option>', {
                        value: data.buscarTarea[i].id,
                        text: data.buscarTarea[i].nombre_item
                    }));
                    $('#selectCategorias').append($('<option>', {
                        value: data.buscarTarea[i].id,
                        text: data.buscarTarea[i].nombre_categoria
                    }));
                }
                if (data.requiereFecha == true) {
                    $('#prioridad').val('media');
                    $('#txtFecha').prop('required', true);
                    $('#areaFechaOculta').show();
                } else {
                    $('#prioridad').val('alta');
                    $('#areaFechaOculta').hide();
                    $('#txtFecha').prop('required', false);
                }
                $('#selectTareas').val('').prop('style', 'visible:visible').select2();
                $('#modalTareas').modal('show');
            }
        }
    })
}

$('#btnEnviarMensaje').click(function () {
    $('#enviarMensaje').val(true);
    $('#formularioPrincipal').removeAttr('onsubmit')
    $('#formularioPrincipal').submit();
});

$('#btnEnviarSinMensaje').click(function () {
    $('#enviarMensaje').val(false);
    $('#formularioPrincipal').removeAttr('onsubmit')
    $('#formularioPrincipal').submit();
});

$('#formularioPrincipal').submit(function () {
    if (cantidadItemsRojos > 0) {
        $('#modalEnviarMensaje').modal('show');
    } else {
        $('#enviarMensaje').val(false);
        $('#formularioPrincipal').removeAttr('onsubmit')
        $('#formularioPrincipal').submit();
    }
});

$('#TareasformEditar').submit(function () {
    var cantidad = $('#txtCantidadEditar').val().replace(/\D/g, "");
    var costo = $('#txtCostoEditar').val().replace(/\D/g, "");
    var total = parseFloat(cantidad) * parseFloat(costo);

    var remplazarFila = '';
    if ($('#prioridadEditar').val() == 'media') {
        remplazarFila = '<tr id="fila' + $('#selectTareasEditar').val() + '"><td align="left">' + $('#nombre_categoriaEditar').val() + '</td><td align="left"><input type="text" id="prioridad'
           + $('#selectTareasEditar').val() + '" name="prioridad' + $('#selectTareasEditar').val() + '" value="' + $('#prioridadEditar').val() + '" style="display:none;"/><input type="text" id="nombre_item'
           + $('#selectTareasEditar').val() + '" name="nombre_item' + $('#selectTareasEditar').val() + '" value="' + $('#nombre_itemEditar').val() + '" style="display:none;"/><input type="text" id="nombre_categoria'
           + $('#selectTareasEditar').val() + '" name="nombre_categoria' + $('#selectTareasEditar').val() + '" value="' + $('#nombre_categoriaEditar').val() + '" style="display:none;"/><input type="text" id="nombre_tarea'
           + $('#selectTareasEditar').val() + '" name="nombre_tarea' + $('#selectTareasEditar').val() + '" value="' + $("#selectTareasEditar option:selected").text() + '" style="display:none;"/><input type="text" id="id_tarea'
           + $('#selectTareasEditar').val() + '" name="id_tarea' + $('#selectTareasEditar').val() + '" value="' + $('#selectTareasEditar').val() + '" style="display:none;"/>'
                       + $('#nombre_itemEditar').val() + '</td><td align="left">' + $("#selectTareasEditar option:selected").text() + '</td><td align="right"><input type="text" id="cantidad_tarea'
           + $('#selectTareasEditar').val() + '" name="cantidad_tarea' + $('#selectTareasEditar').val() + '" value="' + $('#txtCantidadEditar').val() + '" style="display:none;"/>'
                       + $('#txtCantidadEditar').val() + '</td><td align="right"><input type="text" id="costo_tarea'
           + $('#selectTareasEditar').val() + '" name="costo_tarea' + $('#selectTareasEditar').val() + '" value="' + $('#txtCostoEditar').val() + '" style="display:none;"/>$ '
                       + $('#txtCostoEditar').val() + '</td><td align="right"><input type="text" id="costo_total_tarea'
           + $('#selectTareasEditar').val() + '" name="costo_total_tarea' + $('#selectTareasEditar').val() + '" value="' + addComas(total) + '" style="display:none;"/>$ '
                       + addComas(total) + '</td><td align="center"><input type="text" id="fecha_tarea'
           + $('#selectTareasEditar').val() + '" name="fecha_tarea' + $('#selectTareasEditar').val() + '" value="' + $('#txtFechaEditar').val() + '" style="display:none;"/>'
                       + $('#txtFechaEditar').val() + '</td><td width="10%" align="center"><button class="btn btn-danger btn-xs" onclick="eliminarTarea('
                       + $('#selectTareasEditar').val() + ',' + '\'media' + '\')">&nbsp;&nbsp;<i class="fa fa-times"></i>&nbsp;&nbsp;</button>&nbsp;&nbsp;<button class="btn btn-warning btn-xs" onclick="editarTarea('
                       + $('#selectTareasEditar').val() + ')">&nbsp;&nbsp;<i class="fa fa-edit"></i>&nbsp;&nbsp;</button></td></tr>';
    } else {
        remplazarFila = '<tr id="fila' + $('#selectTareasEditar').val() + '"><td align="left">' + $('#nombre_categoriaEditar').val() + '</td><td align="left"><input type="text" id="prioridad'
           + $('#selectTareasEditar').val() + '" name="prioridad' + $('#selectTareasEditar').val() + '" value="' + $('#prioridadEditar').val() + '" style="display:none;"/><input type="text" id="nombre_item'
           + $('#selectTareasEditar').val() + '" name="nombre_item' + $('#selectTareasEditar').val() + '" value="' + $('#nombre_itemEditar').val() + '" style="display:none;"/><input type="text" id="nombre_categoria'
           + $('#selectTareasEditar').val() + '" name="nombre_categoria' + $('#selectTareasEditar').val() + '" value="' + $('#nombre_categoriaEditar').val() + '" style="display:none;"/><input type="text" id="nombre_tarea'
           + $('#selectTareasEditar').val() + '" name="nombre_tarea' + $('#selectTareasEditar').val() + '" value="' + $("#selectTareasEditar option:selected").text() + '" style="display:none;"/><input type="text" id="id_tarea'
           + $('#selectTareasEditar').val() + '" name="id_tarea' + $('#selectTareasEditar').val() + '" value="' + $('#selectTareasEditar').val() + '" style="display:none;"/>'
                       + $('#nombre_itemEditar').val() + '</td><td align="left">' + $("#selectTareasEditar option:selected").text() + '</td><td align="right"><input type="text" id="cantidad_tarea'
           + $('#selectTareasEditar').val() + '" name="cantidad_tarea' + $('#selectTareasEditar').val() + '" value="' + $('#txtCantidadEditar').val() + '" style="display:none;"/>'
                       + $('#txtCantidadEditar').val() + '</td><td align="right"><input type="text" id="costo_tarea'
           + $('#selectTareasEditar').val() + '" name="costo_tarea' + $('#selectTareasEditar').val() + '" value="' + $('#txtCostoEditar').val() + '" style="display:none;"/>$ '
                       + $('#txtCostoEditar').val() + '</td><td align="right"><input type="text" id="costo_total_tarea'
           + $('#selectTareasEditar').val() + '" name="costo_total_tarea' + $('#selectTareasEditar').val() + '" value="' + addComas(total) + '" style="display:none;"/>$ '
                       + addComas(total) + '</td><td width="10%" align="center"><button class="btn btn-danger btn-xs" onclick="eliminarTarea('
                       + $('#selectTareasEditar').val() + ',' + '\'alta' + '\')">&nbsp;&nbsp;<i class="fa fa-times"></i>&nbsp;&nbsp;</button>&nbsp;&nbsp;<button class="btn btn-warning btn-xs" onclick="editarTarea('
                       + $('#selectTareasEditar').val() + ')">&nbsp;&nbsp;<i class="fa fa-edit"></i>&nbsp;&nbsp;</button></td></tr>';

        cantidadItemsRojos++;
    }

    $('#fila' + $('#selectTareasEditar').val() + '').replaceWith(remplazarFila);
    $('#modalEditarTareas').modal('hide');
});

$('#Tareasform').submit(function () {
    $("#fila" + $('#selectTareas').val() + "").remove();

    var cantidad = $('#txtCantidad').val().replace(/\D/g, "");
    var costo = $('#txtCosto').val().replace(/\D/g, "");
    var total = parseFloat(cantidad) * parseFloat(costo);

    if ($('#prioridad').val() == 'media') {
        $('#tablaTareasPrioridadMedia').find('tbody').append('<tr id="fila' + $('#selectTareas').val() + '">'
                + '<td align="left">'
                    + $('#nombre_categoria').val()
                + '</td>'
                + '<td align="left">'
                    + '<input type="text" id="prioridad' + $('#selectTareas').val() + '" name="prioridad' + $('#selectTareas').val() + '" value="' + $('#prioridad').val() + '" style="display:none;"/>'
                    + '<input type="text" id="nombre_item' + $('#selectTareas').val() + '" name="nombre_item' + $('#selectTareas').val() + '" value="' + $('#nombre_item').val() + '" style="display:none;"/>'
                    + '<input type="text" id="nombre_categoria' + $('#selectTareas').val() + '" name="nombre_categoria' + $('#selectTareas').val() + '" value="' + $('#nombre_categoria').val() + '" style="display:none;"/>'
                    + '<input type="text" id="nombre_tarea' + $('#selectTareas').val() + '" name="nombre_tarea' + $('#selectTareas').val() + '" value="' + $("#selectTareas option:selected").text() + '" style="display:none;"/>'
                    + '<input type="text" id="id_tarea' + $('#selectTareas').val() + '" name="id_tarea' + $('#selectTareas').val() + '" value="' + $('#selectTareas').val() + '" style="display:none;"/>'
                    + $('#nombre_item').val()
                + '</td>'
                + '<td align="left">'
                    + $("#selectTareas option:selected").text()
                + '</td>'
                + '<td align="right">'
                    + '<input type="text" id="cantidad_tarea' + $('#selectTareas').val() + '" name="cantidad_tarea' + $('#selectTareas').val() + '" value="' + $('#txtCantidad').val() + '" style="display:none;"/>'
                    + $('#txtCantidad').val()
                + '</td>'
                + '<td align="right">'
                    + '<input type="text" id="costo_tarea' + $('#selectTareas').val() + '" name="costo_tarea' + $('#selectTareas').val() + '" value="' + $('#txtCosto').val() + '" style="display:none;"/>$ '
                    + $('#txtCosto').val()
                + '</td>'
                + '<td align="right">'
                    + '<input type="text" id="costo_total_tarea' + $('#selectTareas').val() + '" name="costo_total_tarea' + $('#selectTareas').val() + '" value="' + addComas(total) + '" style="display:none;"/>$ '
                    + addComas(total)
                + '</td>'
                + '<td align="center">'
                    + '<input type="text" id="fecha_tarea' + $('#selectTareas').val() + '" name="fecha_tarea' + $('#selectTareas').val() + '" value="' + $('#txtFecha').val() + '" style="display:none;"/>'
                    + $('#txtFecha').val()
                + '</td>'
                + '<td width="10%" align="center">'
                    + '<button type="button" class="btn btn-danger btn-xs" onclick="eliminarTarea(' + $('#selectTareas').val() + ',' + '\'media' + '\')">&nbsp;&nbsp;<i class="fa fa-times"></i>&nbsp;&nbsp;</button>&nbsp;&nbsp;'
                    + '<button type="button" class="btn btn-warning btn-xs" onclick="editarTarea(' + $('#selectTareas').val() + ')">&nbsp;&nbsp;<i class="fa fa-edit"></i>&nbsp;&nbsp;</button>'
                + '</td>'
            + '</tr>');
    } else {
        $('#tablaTareasPrioridadAlta').find('tbody').append('<tr id="fila' + $('#selectTareas').val() + '"><td align="left">' + $('#nombre_categoria').val() + '</td><td align="left"><input type="text" id="prioridad'
            + $('#selectTareas').val() + '" name="prioridad' + $('#selectTareas').val() + '" value="' + $('#prioridad').val() + '" style="display:none;"/><input type="text" id="nombre_item'
            + $('#selectTareas').val() + '" name="nombre_item' + $('#selectTareas').val() + '" value="' + $('#nombre_item').val() + '" style="display:none;"/><input type="text" id="nombre_categoria'
            + $('#selectTareas').val() + '" name="nombre_categoria' + $('#selectTareas').val() + '" value="' + $('#nombre_categoria').val() + '" style="display:none;"/><input type="text" id="nombre_tarea'
            + $('#selectTareas').val() + '" name="nombre_tarea' + $('#selectTareas').val() + '" value="' + $("#selectTareas option:selected").text() + '" style="display:none;"/><input type="text" id="id_tarea'
            + $('#selectTareas').val() + '" name="id_tarea' + $('#selectTareas').val() + '" value="' + $('#selectTareas').val() + '" style="display:none;"/>'
                        + $('#nombre_item').val() + '</td><td align="left">' + $("#selectTareas option:selected").text() + '</td><td align="right"><input type="text" id="cantidad_tarea'
            + $('#selectTareas').val() + '" name="cantidad_tarea' + $('#selectTareas').val() + '" value="' + $('#txtCantidad').val() + '" style="display:none;"/>'
                        + $('#txtCantidad').val() + '</td><td align="right"><input type="text" id="costo_tarea'
            + $('#selectTareas').val() + '" name="costo_tarea' + $('#selectTareas').val() + '" value="' + $('#txtCosto').val() + '" style="display:none;"/>$ '
                        + $('#txtCosto').val() + '</td><td align="right"><input type="text" id="costo_total_tarea'
            + $('#selectTareas').val() + '" name="costo_total_tarea' + $('#selectTareas').val() + '" value="' + addComas(total) + '" style="display:none;"/>$ '
                        + addComas(total) + '</td><td width="10%" align="center"><button type="button" class="btn btn-danger btn-xs" onclick="eliminarTarea('
                        + $('#selectTareas').val() + ',' + '\'alta' + '\')">&nbsp;&nbsp;<i class="fa fa-times"></i>&nbsp;&nbsp;</button>&nbsp;&nbsp;<button type="button" class="btn btn-warning btn-xs" onclick="editarTarea('
                        + $('#selectTareas').val() + ')">&nbsp;&nbsp;<i class="fa fa-edit"></i>&nbsp;&nbsp;</button></td></tr>');

        cantidadItemsRojos++;
    }
    $('#modalTareas').modal('hide');
    //$('#areaTablaTareasAsignadas').show();
    $('#txtCantidad').val('');
    $('#txtCosto').val('');
    $('#txtFecha').val('');
    $('#selectTareas').val('').prop('style', 'display:display').select2();
});

$('#selectTareas').change(function () {
    $('#selectItems').val($('#selectTareas').val()).select2();
    $('#selectCategorias').val($('#selectTareas').val()).select2();
    $('#nombre_item').val($("#selectItems option:selected").text());
    $('#nombre_categoria').val($("#selectCategorias option:selected").text());
});

$('#btnAgregarTareaPrioridad').click(function () {
    $.ajax({
        url: '/InspeccionVh/BuscarTodasLasTareas',
        data: {
        },
        type: "post",
        cache: false,
        success: function (data) {
            if (data.length > 0) {
                $('#selectTareas').empty();
                $('#selectItems').empty();
                $('#selectTareas').append($('<option>', {
                    value: '',
                    text: ''
                }));
                for (var i = 0; i < data.length; i++) {
                    $('#selectTareas').append($('<option>', {
                        value: data[i].id,
                        text: data[i].descripcion
                    }));
                    $('#selectItems').append($('<option>', {
                        value: data[i].id,
                        text: data[i].nombre_item
                    }));
                    $('#selectCategorias').append($('<option>', {
                        value: data[i].id,
                        text: data[i].nombre_categoria
                    }));

                }
                $('#prioridad').val('alta');
                $('#areaFechaOculta').hide();
                $('#txtFecha').prop('required', false);

                $('#selectTareas').val('').prop('style', 'visible:visible').select2();
                $('#modalTareas').modal('show');
            }
        }
    })
});

function mostrarDetalle(idItem, idRespuesta, descripcionRta) {
    $('#respuestaItem' + idItem + '').val(descripcionRta);

    $.ajax({
        url: '/InspeccionVh/BuscarTareasItem',
        data: {
            id_item: idItem,
            id_color: idRespuesta
        },
        type: "post",
        cache: false,
        success: function (data) {
            if (data.buscarTarea.length > 0) {
                $('#selectTareas').empty();
                $('#selectTareas').append($('<option>', {
                    value: '',
                    text: ''
                }));
                for (var i = 0; i < data.buscarTarea.length; i++) {
                    $('#selectTareas').append($('<option>', {
                        value: data.buscarTarea[i].id,
                        text: data.buscarTarea[i].descripcion
                    }));
                    $('#selectItems').append($('<option>', {
                        value: data.buscarTarea[i].id,
                        text: data.buscarTarea[i].nombre_item
                    }));
                    $('#selectCategorias').append($('<option>', {
                        value: data.buscarTarea[i].id,
                        text: data.buscarTarea[i].nombre_categoria
                    }));
                }
                if (data.requiereFecha == true) {
                    $('#prioridad').val('media');
                    $('#txtFecha').prop('required', true);
                    $('#areaFechaOculta').show();
                } else {
                    $('#prioridad').val('alta');
                    $('#areaFechaOculta').hide();
                    $('#txtFecha').prop('required', false);
                }
                $('#selectTareas').val('').prop('style', 'visible:visible').select2();
                $('#modalTareas').modal('show');
            }
        }
    })
}

function eliminarTarea(id_tarea, prioridad) {
    $("#fila" + id_tarea + "").remove();
    if (prioridad == 'alta') {
        cantidadItemsRojos--;
    }
}

function editarTarea(id_tarea) {
    event.preventDefault()
    var prioridad = $('#prioridad' + id_tarea + '').val();
    var id_tarea = $('#id_tarea' + id_tarea + '').val();
    var nombre_tarea = $('#nombre_tarea' + id_tarea + '').val();
    var nombre_item = $('#nombre_item' + id_tarea + '').val();
    var nombre_categoria = $('#nombre_categoria' + id_tarea + '').val();
    var cantidad_tarea = $('#cantidad_tarea' + id_tarea + '').val();
    var costo_tarea = $('#costo_tarea' + id_tarea + '').val();
    var fecha_tarea = $('#fecha_tarea' + id_tarea + '').val();
    $('#prioridadEditar').val(prioridad);
    $('#nombre_itemEditar').val(nombre_item);
    $('#nombre_categoriaEditar').val(nombre_categoria);
    $('#selectTareasEditar').empty();
    $('#selectTareasEditar').append($('<option>', {
        value: id_tarea,
        text: nombre_tarea
    }));
    $('#selectTareasEditar').val(id_tarea).prop('style', 'visible:visible').select2();
    $('#txtCantidadEditar').val(cantidad_tarea);
    $('#txtCostoEditar').val(costo_tarea);
    if (prioridad == 'media') {
        $('#txtFechaEditar').val(fecha_tarea);
        $('#txtFechaEditar').prop('required', true);
        $('#areaFechaOcultaEditar').show();
    } else {
        $('#txtFechaEditar').prop('required', false);
        $('#areaFechaOcultaEditar').hide();
        $('#txtFechaEditar').val('');
    }
    $('#modalEditarTareas').modal('show');
}
