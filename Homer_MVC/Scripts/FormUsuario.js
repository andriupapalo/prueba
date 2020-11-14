
function buscarAjaxCliente() {
    $.ajax({
        url: '/tercero_cliente/BuscarCliente',
        data: { text: $("#txBusqueda").val() },
        type: "post",
        cache: false,
        success: function (retorno) {
            $("#contenido").html(retorno);

        },
        error: function () {
            $('#div-mensaje-buscar').html('<div class="alert alert-danger" style="text-align: left;"><button type="button" class="close" data-dismiss="alert" arial-label="close"><span aria-hidden="true">&times;</span></button><p><i style="font-size: 15pt;" class="fa fa-warning"></i>  Se ha producido un error, intente mas tarde</p></div>')
        }
    })
}

function buscarAjaxTercero() {
    $.ajax({
        url: '/icb_terceros/BuscarTerceros',
        data: { text: $("#txBusqueda").val() },
        type: "post",
        cache: false,
        success: function (retorno) {
            $("#contenido").html(retorno);

        },
        error: function () {
            $('#div-mensaje-buscar').html('<div class="alert alert-danger" style="text-align: left;"><button type="button" class="close" data-dismiss="alert" arial-label="close"><span aria-hidden="true">&times;</span></button><p><i style="font-size: 15pt;" class="fa fa-warning"></i>  Se ha producido un error, intente mas tarde</p></div>')
        }
    })
}


function buscarAjaxuser() {
    $.ajax({
        url: '/users/BuscarUsuario',
        data: { text: $("#txBusqueda").val() },
        type: "post",
        cache: false,
        success: function (retorno) {
            $("#contenido").html(retorno); 
            
        },
        error: function () {
            $('#div-mensaje-buscar').html('<div class="alert alert-danger" style="text-align: left;"><button type="button" class="close" data-dismiss="alert" arial-label="close"><span aria-hidden="true">&times;</span></button><p><i style="font-size: 15pt;" class="fa fa-warning"></i>  Se ha producido un error, intente mas tarde</p></div>')
        }
    })
}

function buscarAjaxacteco_tercero() {
    $.ajax({
        url: '/acteco_tercero/Buscaracteco_tercero',
        data: { text: $("#txBusqueda").val() },
        type: "post",
        cache: false,
        success: function (retorno) {
            $("#contenido").html(retorno);

        },
        error: function () {
            $('#div-mensaje-buscar').html('<div class="alert alert-danger" style="text-align: left;"><button type="button" class="close" data-dismiss="alert" arial-label="close"><span aria-hidden="true">&times;</span></button><p><i style="font-size: 15pt;" class="fa fa-warning"></i>  Se ha producido un error, intente mas tarde</p></div>')
        }
    })
}

function buscarAjaxfpago() {
    $.ajax({
        url: '/fpago_tercero/Buscarfpago_tercero',
        data: { text: $("#txBusqueda").val() },
        type: "post",
        cache: false,
        success: function (retorno) {
            $("#contenido").html(retorno);

        },
        error: function () {
            $('#div-mensaje-buscar').html('<div class="alert alert-danger" style="text-align: left;"><button type="button" class="close" data-dismiss="alert" arial-label="close"><span aria-hidden="true">&times;</span></button><p><i style="font-size: 15pt;" class="fa fa-warning"></i>  Se ha producido un error, intente mas tarde</p></div>')
        }
    })
}

function buscarAjaxtp_impuesto() {
    $.ajax({
        url: '/tpimpu_tercero/Buscartpimpu_tercero',
        data: { text: $("#txBusqueda").val() },
        type: "post",
        cache: false,
        success: function (retorno) {
            $("#contenido").html(retorno);

        },
        error: function () {
            $('#div-mensaje-buscar').html('<div class="alert alert-danger" style="text-align: left;"><button type="button" class="close" data-dismiss="alert" arial-label="close"><span aria-hidden="true">&times;</span></button><p><i style="font-size: 15pt;" class="fa fa-warning"></i>  Se ha producido un error, intente mas tarde</p></div>')
        }
    })
}

function buscarAjaxtp_regimen() {
    $.ajax({
        url: '/tpregimen_tercero/Buscartpregimen_tercero',
        data: { text: $("#txBusqueda").val() },
        type: "post",
        cache: false,
        success: function (retorno) {
            $("#contenido").html(retorno);

        },
        error: function () {
            $('#div-mensaje-buscar').html('<div class="alert alert-danger" style="text-align: left;"><button type="button" class="close" data-dismiss="alert" arial-label="close"><span aria-hidden="true">&times;</span></button><p><i style="font-size: 15pt;" class="fa fa-warning"></i>  Se ha producido un error, intente mas tarde</p></div>')
        }
    })
}

function buscarAjaxtp_suministro() {
    $.ajax({
        url: '/tiposumi_tercero/Buscartiposumi_tercero',
        data: { text: $("#txBusqueda").val() },
        type: "post",
        cache: false,
        success: function (retorno) {
            $("#contenido").html(retorno);

        },
        error: function () {
            $('#div-mensaje-buscar').html('<div class="alert alert-danger" style="text-align: left;"><button type="button" class="close" data-dismiss="alert" arial-label="close"><span aria-hidden="true">&times;</span></button><p><i style="font-size: 15pt;" class="fa fa-warning"></i>  Se ha producido un error, intente mas tarde</p></div>')
        }
    })
}

function buscarAjaxgen_tercero() {
    $.ajax({
        url: '/gen_tercero/Buscargen_tercero',
        data: { text: $("#txBusqueda").val() },
        type: "post",
        cache: false,
        success: function (retorno) {
            $("#contenido").html(retorno);

        },
        error: function () {
            $('#div-mensaje-buscar').html('<div class="alert alert-danger" style="text-align: left;"><button type="button" class="close" data-dismiss="alert" arial-label="close"><span aria-hidden="true">&times;</span></button><p><i style="font-size: 15pt;" class="fa fa-warning"></i>  Se ha producido un error, intente mas tarde</p></div>')
        }
    })
}

function buscarAjaxtp_Dpte() {
    $.ajax({
        url: '/tp_Dpte/Buscartp_Dpte',
        data: { text: $("#txBusqueda").val() },
        type: "post",
        cache: false,
        success: function (retorno) {
            $("#contenido").html(retorno);

        },
        error: function () {
            $('#div-mensaje-buscar').html('<div class="alert alert-danger" style="text-align: left;"><button type="button" class="close" data-dismiss="alert" arial-label="close"><span aria-hidden="true">&times;</span></button><p><i style="font-size: 15pt;" class="fa fa-warning"></i>  Se ha producido un error, intente mas tarde</p></div>')
        }
    })
}

function buscarAjaxtp_hobby() {
    $.ajax({
        url: '/tp_hobby/Buscartp_hobby',
        data: { text: $("#txBusqueda").val() },
        type: "post",
        cache: false,
        success: function (retorno) {
            $("#contenido").html(retorno);

        },
        error: function () {
            $('#div-mensaje-buscar').html('<div class="alert alert-danger" style="text-align: left;"><button type="button" class="close" data-dismiss="alert" arial-label="close"><span aria-hidden="true">&times;</span></button><p><i style="font-size: 15pt;" class="fa fa-warning"></i>  Se ha producido un error, intente mas tarde</p></div>')
        }
    })
}

function buscarAjaxtp_documento() {
    $.ajax({
        url: '/tp_documento/Buscartp_documento',
        data: { text: $("#txBusqueda").val() },
        type: "post",
        cache: false,
        success: function (retorno) {
            $("#contenido").html(retorno);

        },
        error: function () {
            $('#div-mensaje-buscar').html('<div class="alert alert-danger" style="text-align: left;"><button type="button" class="close" data-dismiss="alert" arial-label="close"><span aria-hidden="true">&times;</span></button><p><i style="font-size: 15pt;" class="fa fa-warning"></i>  Se ha producido un error, intente mas tarde</p></div>')
        }
    })
}






function buscarAjaxnom_pais() {
    $.ajax({
        url: '/nom_pais/Buscarnom_pais',
        data: { text: $("#txBusqueda").val() },
        type: "post",
        cache: false,
        success: function (retorno) {
            $("#contenido").html(retorno);

        },
        error: function () {
            $('#div-mensaje-buscar').html('<div class="alert alert-danger" style="text-align: left;"><button type="button" class="close" data-dismiss="alert" arial-label="close"><span aria-hidden="true">&times;</span></button><p><i style="font-size: 15pt;" class="fa fa-warning"></i>  Se ha producido un error, intente mas tarde</p></div>')
        }
    })
}

function buscarAjaxnom_departamento() {
    $.ajax({
        url: '/nom_departamento/Buscarnom_departamento',
        data: { text: $("#txBusqueda").val() },
        type: "post",
        cache: false,
        success: function (retorno) {
            $("#contenido").html(retorno);

        },
        error: function () {
            $('#div-mensaje-buscar').html('<div class="alert alert-danger" style="text-align: left;"><button type="button" class="close" data-dismiss="alert" arial-label="close"><span aria-hidden="true">&times;</span></button><p><i style="font-size: 15pt;" class="fa fa-warning"></i>  Se ha producido un error, intente mas tarde</p></div>')
        }
    })
}

function buscarAjaxnom_ciudad() {
    $.ajax({
        url: '/nom_ciudad/Buscarnom_ciudad',
        data: { text: $("#txBusqueda").val() },
        type: "post",
        cache: false,
        success: function (retorno) {
            $("#contenido").html(retorno);

        },
        error: function () {
            $('#div-mensaje-buscar').html('<div class="alert alert-danger" style="text-align: left;"><button type="button" class="close" data-dismiss="alert" arial-label="close"><span aria-hidden="true">&times;</span></button><p><i style="font-size: 15pt;" class="fa fa-warning"></i>  Se ha producido un error, intente mas tarde</p></div>')
        }
    })
}

function buscarAjaxrol() {
    $.ajax({
        url: '/rols/BuscarRol',
        data: { text: $("#txBusqueda").val() },
        type: "post",
        cache: false,
        success: function (retorno) {
            $("#contenido").html(retorno);

        },
        error: function () {
            $('#div-mensaje-buscar').html('<div class="alert alert-danger" style="text-align: left;"><button type="button" class="close" data-dismiss="alert" arial-label="close"><span aria-hidden="true">&times;</span></button><p><i style="font-size: 15pt;" class="fa fa-warning"></i>  Se ha producido un error, intente mas tarde</p></div>')
        }
    })
}

function buscarAjaxnom_sector() {
    $.ajax({
        url: '/nom_sector/Buscarnom_sector',
        data: { text: $("#txBusqueda").val() },
        type: "post",
        cache: false,
        success: function (retorno) {
            $("#contenido").html(retorno);

        },
        error: function () {
            $('#div-mensaje-buscar').html('<div class="alert alert-danger" style="text-align: left;"><button type="button" class="close" data-dismiss="alert" arial-label="close"><span aria-hidden="true">&times;</span></button><p><i style="font-size: 15pt;" class="fa fa-warning"></i>  Se ha producido un error, intente mas tarde</p></div>')
        }
    })
}

function buscarAjaxclasif_tercero() {
    $.ajax({
        url: '/clasif_tercero/Buscarclasif_tercero',
        data: { text: $("#txBusqueda").val() },
        type: "post",
        cache: false,
        success: function (retorno) {
            $("#contenido").html(retorno);

        },
        error: function () {
            $('#div-mensaje-buscar').html('<div class="alert alert-danger" style="text-align: left;"><button type="button" class="close" data-dismiss="alert" arial-label="close"><span aria-hidden="true">&times;</span></button><p><i style="font-size: 15pt;" class="fa fa-warning"></i>  Se ha producido un error, intente mas tarde</p></div>')
        }
    })
}

function buscarAjaxestado_civil() {
    $.ajax({
        url: '/estado_civil/Buscarestado_civil',
        data: { text: $("#txBusqueda").val() },
        type: "post",
        cache: false,
        success: function (retorno) {
            $("#contenido").html(retorno);

        },
        error: function () {
            $('#div-mensaje-buscar').html('<div class="alert alert-danger" style="text-align: left;"><button type="button" class="close" data-dismiss="alert" arial-label="close"><span aria-hidden="true">&times;</span></button><p><i style="font-size: 15pt;" class="fa fa-warning"></i>  Se ha producido un error, intente mas tarde</p></div>')
        }
    })
}

function buscarAjaxtp_ocupacion() {
    $.ajax({
        url: '/tp_ocupacion/Buscartp_ocupacion',
        data: { text: $("#txBusqueda").val() },
        type: "post",
        cache: false,
        success: function (retorno) {
            $("#contenido").html(retorno);

        },
        error: function () {
            $('#div-mensaje-buscar').html('<div class="alert alert-danger" style="text-align: left;"><button type="button" class="close" data-dismiss="alert" arial-label="close"><span aria-hidden="true">&times;</span></button><p><i style="font-size: 15pt;" class="fa fa-warning"></i>  Se ha producido un error, intente mas tarde</p></div>')
        }
    })
}

$(function () {

    $('#activaTab-create').click(function (e) {
        e.preventDefault();
        $('#tabs-crear a[href="#buscar"]').tab('show');
    })

})

$(function () {

    $('#activaTab-update').click(function (e) {
        e.preventDefault();
        $('#tabs-update a[href="#buscar"]').tab('show');
    })

})
//$(function () {
//    $('#btn-guardar').click(function () {

//        //var email = document.getElementById('email').value;
//        var numIdentUsuario = $("#numIdentUsuario").val()
//        var nombreUsuario = $("#nombreUsuario").val()
//        var apellidoUsuario = $("#apellidoUsuario").val()
//        var emailUsuario = $("#emailUsuario").val()
//        var userUsuario = $("#userUsuario").val()
//        var passwordUsuario = $("#passwordUsuario").val()
//        var confirPasswordUsuario = $("#confirPasswordUsuario").val()
//        var telefonoUsuario = $("#telefonoUsuario").val()
//        var direccionUsuario = $("#direccionUsuario").val()
//        var idTipoDocumento = $("#idTipoDocumento").val()
//        var idRolUsuario = $("#idRolUsuario").val()
//        var idCiudadUsuario = $("#idCiudadUsuario").val()
//        $.ajax({
//            url: '/Usuario/CrearUsuarios',
//            method: 'POST',
//            data: {
//                numIdentUsuario: numIdentUsuario,
//                nombreUsuario: nombreUsuario,
//                apellidoUsuario: apellidoUsuario,
//                emailUsuario: emailUsuario,
//                userUsuario: userUsuario,
//                passwordUsuario: passwordUsuario,
//                confirPasswordUsuario: confirPasswordUsuario,
//                telefonoUsuario: telefonoUsuario,
//                direccionUsuario: direccionUsuario,
//                idTipoDocumento: idTipoDocumento,
//                idRolUsuario: idRolUsuario,
//                idCiudadUsuario: idCiudadUsuario
//            }
//        }).done(function (res) {
//            if (res === 1) {
//                $('#div-mensaje').html('<div class="alert alert-success" style="text-align: left;"><button type="button" class="close" data-dismiss="alert" arial-label="close"><span aria-hidden="true">&times;</span></button><p><i style="font-size: 15pt;" class="fa fa-check"></i>  El registro se realizo correctamente</p></div>')
//            }else
//                $('#div-mensaje').html('<div class="alert alert-danger" style="text-align: left;"><button type="button" class="close" data-dismiss="alert" arial-label="close"><span aria-hidden="true">&times;</span></button><p><i style="font-size: 15pt;" class="fa fa-warning"></i>  El registro no se realizo correctamente</p></div>')
//        })
//    })

//})


//function actualizarUser(id) {    
//    $.ajax({
//        url: '/Usuario/BuscaIdUser',
//        data: { idUsuario: id },
//        type: "post",
//        cache: false,
//        success: function (res) {
//            alert(res)
//        },
//        error: function (error) {
//            alert("no encuentra datos: " + error)

//        }
//    })
//}