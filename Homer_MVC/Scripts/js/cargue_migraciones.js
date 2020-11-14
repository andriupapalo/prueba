$(function () {
    $('.js-source-states').select2();
    traer_cargues();
});
function traer_columnas() {
    $('#respuesta_superior').hide();
    var migracion = $('#txtMigraciones').val();
    var labelMigracion = $("#txtMigraciones option:selected").text();
    if (migracion != "") {
        $("#archivo_cargue").val("");
        $('#tbColumnas').show();
        $('#seccion_constraint').show();
        $.post('/CargueMigraciones/traerColumnas', { migracion: migracion }).done(function (resp) {
            $('#tbody_columnas').html(resp);
            generar_ejemplo(migracion, labelMigracion);
        }).fail(function (err) {
            console.log('err', err);
            $('#respuesta_superior').html('<div class="alert alert-danger"><p><b>Nota: </b>Se ha generado un error en el envio de datos. </p></div>');
        });
    } else {
        $('#respuesta_superior').show();
        $('#respuesta_superior').html('<div class="alert alert-warning"><p><b>Nota: </b>Por favor complete toda la inforación. </p></div>');
    }
    //console.log(tabla);
    
}
function cargar_archivo() {
    var formData = new FormData();
    var migracion = $('#txtMigraciones').val();
    $('#respuesta_superior').html('<center><img src="/Images/engranaje-eje-paralelo-2.gif" style="width: 4.5rem;" /><br/> Guardando...</center>');
    if ($('#archivo_cargue').prop('files').length > 0) {
        file = $('#archivo_cargue').prop('files')[0];
        formData.append("txtfile", file);
        formData.append("migracion", migracion);
        if (migracion == "") {
            $('#respuesta_superior').show();
            $('#respuesta_superior').html('<div class="alert alert-warning"><p><b>Nota: </b>Por favor complete toda la inforación. </p></div><br>');
        } else {
            $.ajax({
                url: "/CargueMigraciones/GuardarArchivo",
                type: "post",
                dataType: "html",
                data: formData,
                cache: false,
                contentType: false,
                processData: false
            }).done(function (resp) {
                var resp = JSON.parse(resp);
                //console.log(resp,resp.mensaje);
                $('#respuesta_superior').show();
                if (resp.valor == "0") {
                    $('#respuesta_superior').html('<div class="alert alert-' + resp.clase + '"><p><b>Nota: En la fila ' + resp.fila + '</b> ' + resp.mensaje + '</p></div><br>');
                } else {
                    $('#respuesta_superior').html('<div class="alert alert-' + resp.clase + '"><p><b>Nota: </b> ' + resp.mensaje + '</p></div><br>');
                }

                //setTimeout(function () { location.reload(); }, 3000);
            }).fail(function (err) {
                console.log('err', err);
                $('#respuesta_superior').show();
                $('#respuesta_superior').html('<div class="alert alert-danger"><p><b>Nota: </b>Se ha generado un error al enviar los datos. </p></div><br>');
            });
        }

    } else {
        $('#respuesta_superior').show();
        $('#respuesta_superior').html('<div class="alert alert-warning"><p><b>Nota: </b>Por favor seleccione un archivo valido. </p></div><br>');
    }
}
function generar_ejemplo(migracion, labelMigracion) {
    $.post('/CargueMigraciones/generarEjemplo', { migracion: migracion, labelMigracion: labelMigracion }).done(function (resp) {
        $('#btnGenerarEjemplo').attr('href', resp);
        $('#btnGenerarEjemplo').attr('download', labelMigracion);
    }).fail(function (err) {
        console.log('err', err);
        $('#respuesta_superior').show();
        $('#respuesta_superior').html('<div class="alert alert-danger"><p><b>Nota: </b>Se ha generado un error al crear el ejemplo. </p></div><br>');
    });
}
function traer_cargues() {
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
        "order": [[ 3, "desc" ]],
        buttons: [
            //{ extend: 'csv', title: 'ExampleFile', className: 'btn-sm', text: 'Exportar a Excel' }
        ],
        "ajax": {
            "url": "/CargueMigraciones/TraerCargues",
            "type": "POST",
            "datatype": "json",
            "data": {
                filtroGeneral: $('#txtFiltroGeneral').val(),
            }
        },
        "columns": [
            { "data": "nombre_migracion", "name": "nombre_migracion", "autoWidth": true },
            { "data": "tabla_afectada", "name": "tabla_afectada", "autoWidth": true },
            { "data": "nombre_archivo", "name": "nombre_archivo", "autoWidth": true },
            { "data": "fecha_cargue", "name": "fecha_cargue", "autoWidth": true, className: "text-right" },
            {
                "mData": null,
                "bSortable": false,
                "mRender": function (o) {
                    var label = "";
                    if (o.estado == 1) {
                        label = '<span class="badge badge-success">Ejecutada</span>';
                    } else {
                        label = '<span class="badge badge-danger">Fallida</span>';
                    }
                    return label;
                }
            },
            { "data": "error", "name": "error", "autoWidth": true }
        ]
    });

    var data = table.buttons.exportData();
    // Buscar filtros
    $('#botonbuscar').prop('disabled', false);
}
$('#botonbuscar').click(function () {
    $('#botonbuscar').prop('disabled', true);
    traer_cargues();
});
