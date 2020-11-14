$(function () {
    $('.js-source-states').select2();
    traer_migraciones();

});
function abrir_modal(id_modal) {
    $('#modal' + id_modal).modal('show');
    $('body').removeClass("modal-open");
}
function crear_migracion() {
    var orden = 1;
    $('#respuesta_inferior').html('<center><img src="/Images/engranaje-eje-paralelo-2.gif" style="width: 4.5rem;" /><br/> Guardando...</center>');
    var migracion = $('#txtMigraciones').val();
    var oid = $('#txtTablas').val();
    var nombre_tabla = $("#txtTablas option:selected").text();
    var columnas = [];
    var constraints = [];
    $(":checkbox[name=chk_columnas]").each(function () {
        if (this.checked) {
            columnas.push($(this).val()+"|1|"+orden);
            orden++;
        } 
        else{
            columnas.push($(this).val()+"|0|");
        }
    });
    //console.log(columnas);
    $(".td_constraint").each(function () {
        constraints.push($(this).html());
    });
    if (migracion!=""&& oid!=""&&nombre_tabla!=""&&columnas!="[]") {
        var datos = {
            migracion: migracion,
            oid: oid,
            nombre_tabla: nombre_tabla,
            columnas: columnas,
            constraint: constraints
        }
        $.post('/ParametrizacionMigraciones/GuardarMigracion', datos).done(function (resp) {
            $('#respuesta_inferior').show();
            $('#respuesta_inferior').html('<div class="alert alert-' + resp[0] + '"><p><b>Nota: </b>' + resp[1] + '</p></div>');
            setTimeout(function(){location.reload();}, 3000);

        }).fail(function (err) {
            console.log('err', err);
            $('#respuesta_inferior').html('<div class="alert alert-danger"><p><b>Nota: </b>Se ha generado un error al enviar los datos. </p></div>');
        });
    } else {
        $('#respuesta_inferior').show();
        $('#respuesta_inferior').html('<div class="alert alert-warning"><p><b>Nota: </b>Por favor complete toda la inforación. </p></div>');
    }
    
    
    
}
function traer_migraciones() {
    $("#tablaPaginada").dataTable().fnDestroy();
    var general = $('#txtFiltroGeneral').val();

    var table = $('#tablaPaginada').DataTable({
        "processing": true,
        "serverSide": true,
        "filter": true,
        "orderMulti": false,
        "searching": false,
        dom: "<''<'col-md-6'l><'col-md-6'f>>t<'col-md-4'i><'col-md-8 text-right'pB>",
        "lengthMenu": [[50, 100, 200, -1], [50, 100, 200, "All"]],
        buttons: [
            //{ extend: 'csv', title: 'ExampleFile', className: 'btn-sm', text: 'Exportar a Excel' }
        ],
        "ajax": {
            "url": "/ParametrizacionMigraciones/cargarMigraciones",
            "type": "POST",
            "datatype": "json",
            "data": {
                filtroGeneral: $('#txtFiltroGeneral').val(),
            }
        },
        "columns": [
            { "data": "nombre_migracion", "name": "nombre_migracion", "autoWidth": true },
            { "data": "nombre_tabla", "name": "nombre_tabla", "autoWidth": true },
            { "data": "columnas", "name": "columnas", "autoWidth": true, className: "text-right" },
            { "data": "constraints", "name": "constraints", "autoWidth": true, className: "text-right" },
            {
                "mData": null,
                "bSortable": false,
                "mRender": function (o) {
                    var boton = "";
                    boton = '<button class="btn btn-info btn-xs" onclick="traerDetalle('
                                             + '\'' + o.nombre_tabla + '\',\'' + o.nombre_migracion+'\''+ ',\'' + o.id+'\''
                                              + ')">&nbsp;&nbsp;Ver&nbsp;&nbsp;</button>';
                    return boton;
                }
            }
        ]
    });

    var data = table.buttons.exportData();
    // Buscar filtros
    $('#botonbuscar').prop('disabled', false);
}
function traer_columnas(nombre_tabla="",nombre_migracion="" ) {
    $('#respuesta_superior').hide();
    var carga_tabla = 1;
    if (nombre_tabla!="" && nombre_migracion!="") {
        var tabla = nombre_tabla;
        var migracion= nombre_migracion;
        carga_tabla = 0;
    } else{
        var tabla = $("#txtTablas option:selected").text();
        var migracion = $('#txtMigraciones').val();
    }
    if (tabla != "" && migracion!= "") {
        $('#tbColumnas').show();
        $('#seccion_constraint').show();
        $.post('/ParametrizacionMigraciones/traerColumnas', { nomTabla: tabla }).done(function (resp) {
            //console.log(resp[1]);
            if (carga_tabla ==1) {
                $('#tbody_columnas').html(resp[0]);
            }
            $('#txt_columnas_origen_pk').html(resp[1]);
            $('#txt_columnas_origen_uk').html(resp[1]);
            $('#txt_columnas_origen_fk').html(resp[1]);
            $('.check_toggle').bootstrapToggle({
                on: 'Agregar',
                off: 'No agregar'
            });

        }).fail(function (err) {
            console.log('err', err);
        });
    } else {
        $('#respuesta_superior').show();
        $('#respuesta_superior').html('<div class="alert alert-warning"><p><b>Nota: </b>Por favor complete toda la inforación. </p></div>');
    }
    //console.log(tabla);
    
}
function traer_columnas_referencia() {
    var tabla = $("#txtTablasReferencia option:selected").text();
    $.post('/ParametrizacionMigraciones/traerColumnasReferencia', { nomTabla: tabla }).done(function (resp) {
        //console.log(resp);
        $('#txt_columnas_referencia_fk').html(resp);
        $('#txt_columnas_resultado_fk').html(resp);
    }).fail(function (err) {
        console.log('err', err);
    });
}
function agregar_constraint(tipo_constraint) {
    $('#respuesta_inferior').hide();
    var nombre_constraint = $('#txt_nombre_constraint').val();
    var select_constraint = $('#txt_tipo_constraint option:selected').text();
    var columna_origen = $('#txt_columnas_origen_' + tipo_constraint).val();
    var tabla_referencia = "";
    var columna_referencia = "";
    var columna_resultado = "";
    if (tipo_constraint=="fk") {
        tabla_referencia = $("#txtTablasReferencia option:selected").text();
        columna_referencia = $('#txt_columnas_referencia_fk').val();
        columna_resultado = $('#txt_columnas_resultado_fk').val();
    }
    if (nombre_constraint==""||select_constraint==""||columna_origen=="") {
        $('#respuesta_inferior').show();
        $('#respuesta_inferior').html('<div class="alert alert-warning"><p><b>Nota: </b>Por favor llene toda la información. </p></div>');
    } else if (tipo_constraint=="fk" &&(tabla_referencia==""||columna_referencia==""||columna_resultado=="")) {
        $('#respuesta_inferior').show();
        $('#respuesta_inferior').html('<div class="alert alert-warning"><p><b>Nota: </b>Por favor llene toda la información. </p></div>');
    } else {
        var constraint = nombre_constraint + "|" + select_constraint + "|" + columna_origen + "|" + tabla_referencia + "|" + columna_referencia + "|" + columna_resultado;
        var fila = "<tr id=\"tr_" + nombre_constraint + "\">" +
                        "<td>" + nombre_constraint + "</td>" +
                        "<td>" + select_constraint + "</td>" +
                        "<td>" + columna_origen + "</td>" +
                        "<td>" + tabla_referencia + "</td>" +
                        "<td>" + columna_referencia + "</td>" +
                        "<td>" + columna_resultado + "</td>" +
                        "<td><a onclick=\"retirar_constraint('tr_" + nombre_constraint + "')\" class=\"btn btn-danger\" name=\"btnAgregarFK\" id=\"btnAgregarFK\"><i class=\"fa fa-trash\"></i></a></td>" +
                        "<td class=\"td_constraint\" style=\"display:none;\">" + constraint + "</td>" +
                   "</tr>";
        $('#tbody_constraint').append(fila);
        $('#txt_nombre_constraint').val("");
        $("#txt_tipo_constraint").select2("val", "");
        $("#txt_columnas_origen_" + tipo_constraint).select2("val", "");
        $("#txtTablasReferencia").select2("val", "");
        $("#txt_columnas_referencia_fk").select2("val", "");
        $("#txt_columnas_resultado_fk").select2("val", "");
        $('.div_constraint').hide();
    }
    
}
function retirar_constraint(id_tr) {
    $('#' + id_tr).remove();
}
function mostrar_tipo_constraint() {
    var tipo_constraint = $('#txt_tipo_constraint').val();
    $('.div_constraint').hide();
    $('#div_' + tipo_constraint).show();
}
function traerDetalle(nombre_tabla, nombre_migracion,id_migracion) {
    $('#txtMigraciones').val(nombre_migracion)
    $('[href="#crear"]').trigger('click');
    traer_columnas(nombre_tabla, nombre_migracion);
    traer_columnasDetallado(nombre_tabla);
    traer_tablas(nombre_tabla);
    traer_constraints (nombre_tabla)
    
    
}
function traer_constraints (nombre_tabla){
    $.post('/ParametrizacionMigraciones/traerConstraintsDetallado', { nomTabla: nombre_tabla }).done(function (resp) {
        //console.log(resp[1]);
        $('#tbody_constraint').html(resp);
        
    }).fail(function (err) {
        console.log('err', err);
    });
}
function traer_columnasDetallado(nombre_tabla) {
    $('#tbody_columnas').html("");
    $.post('/ParametrizacionMigraciones/traerColumnasDetallado', { nomTabla: nombre_tabla }).done(function (resp) {
        //console.log(resp[1]);
        $('#tbody_columnas').html(resp);
        $('.check_toggle').bootstrapToggle({
            on: 'Agregar',
            off: 'No agregar'
        });
    }).fail(function (err) {
        console.log('err', err);
    });
    //console.log(tabla);
    
}
function traer_tablas(nombre_tabla){
    $.post('/ParametrizacionMigraciones/traerTablasDetallado', { nomTabla: nombre_tabla }).done(function (resp) {
        //console.log(resp);
        $('#txtTablas').html(resp.cuerpo);
        $("#txtTablas").select2("val", resp.val);
    }).fail(function (err) {
        console.log('err', err);
    });
}
$('#botonbuscar').click(function () {
    $('#botonbuscar').prop('disabled', true);
    traer_migraciones();
});
