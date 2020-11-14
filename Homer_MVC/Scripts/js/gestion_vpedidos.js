//Variables Globales --Alex
var precioRPT = 0; //total de accesorios --Alex
var costosAdd = 0; //Total de costos adicionales --Alex
var r = 0; // contador de accesorios --Alex
var fpagos = 0; // Contador formas de pago
var re = 0; // contador de retomas --Alex
var cot = 0;
var cos = 0; // contador costos adicionales
var valor_total_global = 0;
var pago_agregado = 0; // este valor es la suma de los pago agregados
var valorFaltante = 0;
var valorFaltante2 = 0;

$(document).ready(function() {
    //$('.js-source-states').select2();
    $("#btnGuardarSimulacion").show();
    $("#btnGuardarSimulacion2").show();
    $("form select").each(function(i) {
        this.addEventListener("invalid",
            function(e) {
                var _s2Id = `s2id_${e.target.id}`; //s2 autosuggest html ul li element id
                const _posS2 = $(`#${_s2Id}`).position();
                //get the current position of respective select2
                $(`#${_s2Id} ul`).addClass("_invalid"); //add this class with border:1px solid red;
                //this will reposition the hidden select2 just behind the actual select2 autosuggest field with z-index = -1
                $(`#${e.target.id}`).attr("style",
                    `display:block !important;position:absolute;z-index:-1;top:${0 - $(`#${_s2Id}`).outerHeight() + 30
                    }px;left:${15 - ($(`#${_s2Id}`).width() / 10)}px;`);
                /*
                //Adjust the left and top position accordingly
                */
                //remove invalid class after 3 seconds
                setTimeout(function() {
                        $(`#${_s2Id} ul`).removeClass("_invalid");
                    },
                    3000);
                return true;
            },
            false);
    });

    $(".js-source-states").select2({
        includeSelectAllOption: true,
        enableCaseInsensitiveFiltering: true,
    });

});
//laura actualizacion
$('a[data-toggle="tab"]').on("shown.bs.tab",
    function(e) {
        $('a[data-toggle="tab"]').removeClass("btn-primary");
        $('a[data-toggle="tab"]').addClass("btn-default");
        $(this).removeClass("btn-default");
        $(this).addClass("btn-primary");
    });

$("#cerrarModal").click(function() {
    $("#modalPedido").hide();
});

$(function() {

    //carga inical de formas de pago y bancos --Alex
    traer_datos();

    $("form").keypress(function(e) {
        if (e.which == 13) {
            return false;
        }
    });
    $("#flotaNumero").hide();


    if (!$("#txtObligacion_retoma").prop("checked")) {
        $("#txtValorObligacion").val("");
        $("#div_obligacion").show();
    }
    setTimeout(function() {
            $("#mensaje").fadeOut(1500);
        },
        3000);
    setTimeout(function() {
            $("#mensaje_error").fadeOut(1500);
        },
        3000);

    if ($(".suma").val() == null || $(".suma").val() == "") {
        $(".suma").val(0);
    }

    buscarDatos();
});

//Cargar los datos de formas de pago y bancos
function traer_datos() {
    $.ajax({
        url: "/vpedidos/BuscarPagos",
        data: {
        },
        type: "post",
        cache: false,
        success: function(data) {
            console.log("data");
            console.log(data);

            //Se agregan las formas de pago --Alex
            var option = '<option value=""></option>';
            for (let j = 0; j < data.pagos.length; j++) {
                option += `<option value="${data.pagos[j].id}">${data.pagos[j].descripcion}</option>`;
            }
            $("#slcCondicion").html(option);

            //Se agregan los bancos --Alex
            var optionBanco = '<option value="">Seleccione</option>';
            for (let k = 0; k < data.bancos.length; k++) {
                optionBanco += `<option value="${data.bancos[k].id}">${data.bancos[k].financiera_nombre}</option>`;
            }
            $("#slcBanco").html(optionBanco);

            //Se agregan los bancos obligatorios
            $("#parametros").val(data.parametro);

            $(".fecha_formapago").datetimepicker({
                minDate: `-${new Date()}`,
                format: "YYYY/MM/DD",
            });
        },
        complete: function(data) {
            $("#slcCondicion").select2();
            $("#slcBanco").select2();
        }
    });
}

function buscarDatos() {
    $.ajax({
        url: "/vpedidos/BuscarDatos",
        data: {
        },
        type: "post",
        cache: false,
        success: function(data) {
            // console.log("Esta es la información de la tabla paginada " + data[0].facturado);
            $("#tablaPaginada").find("tbody").empty();
            for (let i = 0; i < data.length; i++) {
                $("#tablaPaginada").find("tbody").append(
                    `<tr><td align="right">${data[i].numero}</td><td align="left">${data[i].bodega
                    }</td><td align="left">${data[i].asesor}</td><td align="left">${data[i].modelo
                    }</td><td align="right">${data[i].doc_tercero}</td><td align="left">${data[i].fecha
                    }</td><td align="left">${data[i].fuente}</td><td align="left">${data[i].subfuente
                    }</td><td align="right">${data[i].planmayor}</td><td align="left">${
                    data[i].fecha_asignacion_planmayor}</td><td align="left">${data[i].facturado
                    }</td><td align="right">${data[i].numfactura
                    }</td><td align="left">${data[i].anulado
                    }</td><td  align="center"><button class="btn btn-info btn-xs" onclick="valida('${data[i].id
                    }')">&nbsp;&nbsp;Ver&nbsp;&nbsp;</button>&nbsp;&nbsp<button class="btn btn-primary btn-xs" onclick="seguimiento('${
                    data[i].id
                    }')">&nbsp;&nbsp;Seguimiento&nbsp;&nbsp;</button></td></tr>`);
            }
        },
        complete: function(data) {
            $("#tablaPaginada").dataTable({
                dom: "<''<'col-md-6'l><'col-md-6'f>>t<'col-md-4'i><'col-md-8 text-right'pB>",
                "lengthMenu": [[10, 25, 50, -1], [10, 25, 50, "All"]],
                buttons: []
            });
        }
    });
}

//se traen los modelos segun la marca seleccionada --Alex
$("#marcvh_id").change(function() {
    var esNuevo = true;
    if (!$("#nuevo").is(":checked")) {
        esNuevo = false;
    }
    $.ajax({
        url: "/cotizacion/BuscarModelosPorMarca",
        data: {
            idMarca: $("#marcvh_id").val(),
            esNuevo: esNuevo
        },
        type: "post",
        cache: false,
        success: function(data) {
            //$('#modelo').empty().select2();
            //$('#id_anio_modelo').empty().select2();
            //$('#colVh_id').empty().select2();

            $("#modelo").empty();
            $("#id_anio_modelo").empty();
            $("#colVh_id").empty();
            //$('#Color_Deseado').empty().select2();
            //$('#color_opcional').empty().select2();

            $("#modelo").append($("<option>",
                {
                    value: "",
                    text: "Seleccione"
                }));
            for (let i = 0; i < data.length; i++) {
                $("#modelo").append($("<option>",
                    {
                        value: data[i].modvh_codigo,
                        text: data[i].modvh_nombre
                    }));
            }
            //$('#modelo').select2();
            //$('#id_anio_modelo').select2();
        }
    });
});

//se traen los años segun el modelo seleccionado --Alex
$("#modelo").change(function () {
   
    $("#descripcion").val("");
    $("#valor").val("");
    var esNuevo = true;
    if (!$("#nuevo").is(":checked")) {
        esNuevo = false;
    }

    $.ajax({
        url: "/cotizacion/BuscarAniosModelos",
        data: {
            codigoModelo: $("#modelo").val(),
            esNuevo: esNuevo
        },
        type: "post",
        cache: false,
        success: function(data) {
           
            $("#id_anio_modelo").empty().select2();
            $("#id_anio_modelo").append($("<option>",
                {
                    value: "",
                    text: "Seleccione"
                }));
            for (let i = 0; i < data.length; i++) {
                $("#id_anio_modelo").append($("<option>",
                    {
                        value: data[i].codigo,
                        text: data[i].anios
                    }));
            }
            //$('#id_anio_modelo').select2();
        }
    });
});

//se traen los valores correspondientes al valor unitario, iva e ipoconsumo del modelo-año seleccionado --Alex
$("#id_anio_modelo").change(function() {
    var esNuevo = true;
    if (!$("#nuevo").is(":checked")) {
        esNuevo = false;
    }
    $.ajax({
        url: "/vpedidos/BuscarPreciosPorAnio",
        data: {
            codigoModelo: $("#modelo").val(),
            anioModelo: $("#id_anio_modelo").val(),
            esNuevo: esNuevo
        },
        type: "post",
        cache: false,
        success: function(data) {
            if (data.esNuevo == true) {
                //agregamos los valores correspondientes al valor unitario, iva e ipoconsumo del modelo y año seleccionados --Alex
                $("#valor_unitario").val(addComas(data.valor));
                $("#porcentaje_iva").val(data.codigo_iva);
                $("#porcentaje_impoconsumo").val(data.porcentaje_impoconsumo);

                //calculamos el valor total --Alex
                calcular_valor_total();
                //$('#valor_unitario').trigger('change');
            }
        }
    });
});

// Validamos si los descuentos superan el 100%
$("#pordscto").change(function() {
    const max = parseInt(this.max);
    const valor = parseInt(this.value);
    if (valor > max) {
        $("#modal_porcentajes").modal("show");
        this.value = max;

    }
});
$("#porcentaje_impoconsumo").change(function() {
    const max = parseInt(this.max);
    const valor = parseInt(this.value);
    if (valor > max) {
        $("#modal_porcentajes").modal("show");
        this.value = max;
    }
});
$("#porcentaje_iva").change(function() {
    const max = parseInt(this.max);
    const valor = parseInt(this.value);
    if (valor > max) {
        $("#modal_porcentajes").modal("show");
        this.value = max;
    }
});
$("#obsequioporcen").change(function() {
    const max = parseInt(100); //valor del 100% del descuento
    const valor = parseInt(this.value);
    if (valor > max) {
        $("#modal_porcentajes").modal("show");
        this.value = max;
    }
});

//funcion que limpia los campos del formulario segun el tipo de formulario
function limpiar_valores(formulario) {
    //se limpian todos los select
    $(`.${formulario}_select`).each(function(index) {
        $(this).select2("val", "");
    });

    //se limpian todos los inputs
    $(`.${formulario}_input`).each(function(index) {
        $(this).val("");
    });

    //se limpian todos los otros campos
    $(`.${formulario}_other`).each(function(index) {
        $(this).html("");
    });
}

function calcular_valor_total() {
 
    //validamos si existen valores para matricula, %obsequio, poliza, soat, %descuento, iva, ipoconsumo y valor carroceria  --Alex

    var valor_unitario =
        $("#valor_unitario").val() == ""
            ? 0
            : parseInt(quitCommas($("#valor_unitario")
                .val())); //si el valor es vacio lo inicalizamos en 0 si no en lo que este en el campo --Alex
    var matricula =
        $("#valormatricula").val() == ""
            ? 0
            : parseInt(quitCommas($("#valormatricula")
                .val())); //si el valor es vacio lo inicalizamos en 0 si no en lo que este en el campo --Alex
    var poliza =
        $("#valorPoliza").val() == ""
            ? 0
            : parseInt(quitCommas($("#valorPoliza")
                .val())); //si el valor es vacio lo inicalizamos en 0 si no en lo que este en el campo --Alex
    var soat = $("#valorsoat").val() == ""
        ? 0
        : parseInt(quitCommas($("#valorsoat")
            .val())); //si el valor es vacio lo inicalizamos en 0 si no en lo que este en el campo --Alex
    var carroceria =
        $("#vrcarroceria").val() == ""
            ? 0
            : parseInt(quitCommas($("#vrcarroceria")
                .val())); //si el valor es vacio lo inicalizamos en 0 si no en lo que este en el campo --Alex

    var porcentaje_descuento =
        $("#pordscto").val() == ""
            ? 0
            : parseInt($("#pordscto")
                .val()); //si el valor es vacio lo inicalizamos en 0 si no en lo que este en el campo --Alex
    var porcentale_obsequio =
        $("#obsequioporcen").val() == ""
            ? 0
            : parseInt($("#obsequioporcen")
                .val()); //si el valor es vacio lo inicalizamos en 0 si no en lo que este en el campo --Alex
    var porcentaje_iva =
        $("#porcentaje_iva").val() == ""
            ? 0
            : parseInt($("#porcentaje_iva")
                .val()); //si el valor es vacio lo inicalizamos en 0 si no en lo que este en el campo --Alex
    var porcentaje_impoconsumo =
        $("#porcentaje_impoconsumo").val() == ""
            ? 0
            : $("#porcentaje_impoconsumo")
            .val(); //si el valor es vacio lo inicalizamos en 0 si no en lo que este en el campo --Alex

    //hacemos los calculos correspondientes a los % --Alex
    var descuento = parseFloat(((valor_unitario * porcentaje_descuento) / 100).toFixed(2)); //valor con 2 decimales
    var valor_unitario_descuento = parseFloat(valor_unitario - descuento); // valor unitario sin el descuento
    var iva = parseFloat(((valor_unitario_descuento * porcentaje_iva) / 100).toFixed(2)); //valor con 2 decimales
    var ipoconsumo =
        parseFloat(((valor_unitario_descuento * porcentaje_impoconsumo) / 100).toFixed(2)); //valor con 2 decimales
    matricula =
        porcentale_obsequio != 0
        ? parseFloat((matricula - ((matricula * porcentale_obsequio) / 100)).toFixed(2))
        : parseInt(matricula); // si el valor del obsequito es !=0 se realiza el descuento si no la matricula permanece igual --Alex
    $("#valor_impuesto").val(addCommas(ipoconsumo));
    //calculamos valor total --Alex
    var valor_total = (valor_unitario - descuento) + matricula + poliza + soat + iva + ipoconsumo + carroceria;

    //Asignamos los valores calculados --Alex
    
    contarRepuestos();
    contarCostosAdd();
    valor_total_global = parseFloat(((valor_total + precioRPT + costosAdd) - pago_agregado).toFixed(2));
    $("#txt_valor_faltante").html(addCommas(valor_total_global));
    $("#vrtotal").val(addCommas(valor_total.toFixed(2)));
    $("#vrdescuento").val(addCommas(descuento));
    $("#valor_iva").val(addCommas(iva));

    //actualizamos el valor faltante de la forma de pago --Alex
    //setTimeout(function () {
    //    agregarPago(0);
    //}, 1000);
}

function agregarPago() {
    const cifra = parseFloat(quitCommas($("#txtValor").val())) ||
        0; // este es el valor que se ingresa desde la forma de pago
    const formadepago = parseInt($("#slcCondicion").val()) || 0; // forma de pago seleccionada
    var idcredito = 0;
    if (formadepago == 1) {
        idcredito = $("#idcreditoseleccionado").val();
    }
    const parametro = $("#parametros").val(); //bancos que deben ir agregados de forma obligatoria
    const bancoObligatorio = parametro.split(",");
    var esObligatorio = false;

    for (let a = 0; a < bancoObligatorio.length; a++) {
        if (formadepago == bancoObligatorio[a]) {
            esObligatorio = true;
            break;
        } else {
            esObligatorio = false;
            break;
        }
    }
    if (cifra == 0 ||
        formadepago == 0 ||
        $("#txtFecpago").val() == "" ||
        (esObligatorio == true && $(`#slcBanco${x}`).val() == "")) {
        $("#modal_agregarPago").modal("show");
    } else {

        //Se calcula el nuevo valor faltante
        const valorFaltante = parseFloat((quitCommas($("#txt_valor_faltante").html()) - cifra)
            .toFixed(2)); //Valor faltante es lo contenido en el label de la tabla - lo digitado en el formulario --Alex

        //Se valida que no se digite un valor mayor al valor faltante
        //if (valorFaltante < 0) {
        //    swal("Error", "Ha ingresado un valor mayor al faltante...", "error");
        //    $(".confirm").removeAttr("disabled");

        //} else {
        //Se asigna el nuevo valor faltante al label de la tabla y se actualiza el valor agregado en la variable global
        $("#txt_valor_faltante").html(addCommas(valorFaltante));
        pago_agregado += cifra;
        const banco = $("#slcBanco").val() == "" ? "" : $("#slcBanco option:selected").text();
        //if (valorFaltante == 0 && valor_total_global != 0) {
        //Se muestra el botón de guardar y se habilida su funcion submit
        //$("#btnGuardarSimulacion").show();
        //$("#btnGuardarSimulacion").removeAttr("disabled");
        //$("#btnGuardarSimulacion2").show();
        //$("#btnGuardarSimulacion2").removeAttr("disabled");
        //$(":submit").removeAttr("disabled");
        //}
        //else {
        //    $('#btnGuardar').hide();
        //    $('#btnGuardarSimulacion').hide();
        //    $(":submit").prop("disabled", true);
        //}
        $("#tablaPagosBody").append(
            `<tr id="fila_fpago_${fpagos}"><td>${$("#slcCondicion option:selected").text()}</td><td>${
            $("#txtValor").val()}</td><td>${$("#txtFecpago").val()}</td><td>${banco}</td><td>${$("#txtObservacion")
            .val()
            }</td><td><button class="btn btn-danger btn-circle" id="${fpagos
            }" type="button" onclick="eliminarPago(this.id)"><i class="fa fa-remove"></i></button></td><td hidden><input type="text" class="fpago_valores" id="id_fpago_${
            fpagos}" value="${fpagos}|${$("#slcCondicion").val()}|${$("#slcCondicion option:selected").text()}|${cifra
            }|${$("#txtFecpago").val()}|${$("#slcBanco").val()}|${$("#slcBanco option:selected").text()}|${
            $("#txtObservacion").val()}|${idcredito}"></td></tr>`);
        limpiar_valores("fpago");
        fpagos++;
        //}
    }
}

function eliminarPago(id) {
    //Traemos el valor dentro del input que contiene la informacion de la forma de pago
    const forma_pago = $(`#id_fpago_${id}`).val();

    //hacemos un array para acceder a los datos de la forma de pago y seleccionamos el campo el valor de la forma 
    const array_forma_pago = forma_pago.split("|");
    const valor_sumar = parseFloat(quitCommas(array_forma_pago[3]));

    //sumamos el valor a eliminar al valor faltante y se lo restamos al pago agregado
    const valor_faltante = parseFloat(quitCommas($("#txt_valor_faltante").html())) + valor_sumar;
    pago_agregado -= valor_sumar;
    $("#txt_valor_faltante").html(addCommas(valor_faltante));

    //removemos la fila seleccionada
    $(`#fila_fpago_${id}`).remove();

    //ocultamos y desabilitamos en boton de guardar
    //$("#btnGuardar").hide();
    //$("#btnGuardarSimulacion").hide();
    //$(":submit").prop("disabled", true);
}

$("#escanje").change(function() {
    if ($("#escanje").prop("checked")) {
        $("#eschevyplan").bootstrapToggle("off");
        $("#esreposicion").bootstrapToggle("off");
        $("#esLeasing").bootstrapToggle("off");
    }
});

$("#eschevyplan").change(function() {
    if ($("#eschevyplan").prop("checked")) {
        $("#escanje").bootstrapToggle("off");
        $("#esreposicion").bootstrapToggle("off");
        $("#esLeasing").bootstrapToggle("off");
    }
});

$("#esreposicion").change(function() {
    if ($("#esreposicion").prop("checked")) {
        $("#escanje").bootstrapToggle("off");
        $("#eschevyplan").bootstrapToggle("off");
        $("#esLeasing").bootstrapToggle("off");
    }
});

$("#esLeasing").change(function() {
    if ($("#esLeasing").prop("checked")) {
        $("#escanje").bootstrapToggle("off");
        $("#eschevyplan").bootstrapToggle("off");
        $("#esreposicion").bootstrapToggle("off");
    }
});

$("#escanje").change(function() {
    if ($("#escanje").prop("checked")) {
        $("#eschevyplan").bootstrapToggle("off");
        $("#esreposicion").bootstrapToggle("off");
        $("#esLeasing").bootstrapToggle("off");
    }
});

$("#eschevyplan").change(function() {
    if ($("#eschevyplan").prop("checked")) {
        $("#escanje").bootstrapToggle("off");
        $("#esreposicion").bootstrapToggle("off");
        $("#esLeasing").bootstrapToggle("off");
    }
});

$("#esreposicion").change(function() {
    if ($("#esreposicion").prop("checked")) {
        $("#escanje").bootstrapToggle("off");
        $("#eschevyplan").bootstrapToggle("off");
        $("#esLeasing").bootstrapToggle("off");
    }
});

$("#esLeasing").change(function() {
    if ($("#esLeasing").prop("checked")) {
        $("#escanje").bootstrapToggle("off");
        $("#eschevyplan").bootstrapToggle("off");
        $("#esreposicion").bootstrapToggle("off");
    }
});

//================== VALIDACION PLACA ================
$("#rango_placa").change(function() {
    if ($("#rango_placa").val() != "") {
        $("#placapar").bootstrapToggle("off");
        $("#placapar").prop("disabled", true);
        $("#placaimpar").bootstrapToggle("off");
        $("#placaimpar").prop("disabled", true);
    } else {
        if ($("#rango_placa").val() == "") {
            $("#placapar").prop("disabled", false);
            $("#placaimpar").prop("disabled", false);
        }
    }
});

$("#placapar").change(function() {
    if ($("#placapar").prop("checked")) {
        $("#placaimpar").bootstrapToggle("off");
        $("#rango_placa").prop("disabled", true);
    }
    if (!$("#placapar").prop("checked") && !$("#placaimpar").prop("checked")) {
        $("#rango_placa").prop("disabled", false);
    }
});

$("#placaimpar").change(function() {
    if ($("#placaimpar").prop("checked")) {
        $("#placapar").bootstrapToggle("off");
        $("#rango_placa").prop("disabled", true);
    }
    if (!$("#placapar").prop("checked") && !$("#placaimpar").prop("checked")) {
        $("#rango_placa").prop("disabled", false);
    }
});

$("#terminacionplaca").change(function() {
    if (($("#terminacionplaca").val() % 2) == 0) {
        $("#placapar").prop("checked", "checked");
        $("#placaimpar").removeAttr("checked", "checked");
    } else {
        $("#placapar").removeAttr("checked", "checked");
        $("#placaimpar").prop("checked", "checked");
    }
});

$("#placapar").change(function() {
    if ($("#placapar").is(":checked") && ($("#terminacionplaca").val() % 2) != 0) {
        $("#terminacionplaca").val("");
    }

    if ($("#placapar").is(":checked")) {
        $("#placaimpar").removeAttr("checked", "checked");
    }
});

$("#placaimpar").change(function() {
    if ($("#placaimpar").is(":checked") && ($("#terminacionplaca").val() % 2) == 0) {
        $("#terminacionplaca").val("");
    }
    if ($("#placaimpar").is(":checked")) {
        $("#placapar").removeAttr("checked", "checked");
    }
});
//=======================================================

$("#nit").change(function() {
    buscar_tercero();
});

$("#nit2").change(function() {
    $.ajax({
        url: "/vpedidos/BuscarCliente",
        data: {
            cliente: $("#nit2").val()
        },
        type: "post",
        cache: false,
        success: function(data) {
            //$('#nombre2').text(data[0].nombre)
        },
    });
});

$("#nit3").change(function() {
    $.ajax({
        url: "/vpedidos/BuscarCliente",
        data: {
            cliente: $("#nit3").val()
        },
        type: "post",
        cache: false,
        success: function(data) {
            //$('#nombre3').text(data[0].nombre)
        },
    });
});

$("#nit4").change(function() {
    $.ajax({
        url: "/vpedidos/BuscarCliente",
        data: {
            cliente: $("#nit4").val()
        },
        type: "post",
        cache: false,
        success: function(data) {
            //$('#nombre4').text(data[0].nombre)
        },
    });
});

// Funcion que calcula el valor de los accesorios agregados
function contarRepuestos() {
    precioRPT = 0;
    $(".total_accesorios").each(function(index) {
        const valor_array = this.value.split("|");
        if (valor_array[1] == "false") {
            precioRPT += parseFloat(valor_array[0]);
        }

    });
}

function contarCostosAdd() {
    costosAdd = 0;
    $(".total_costos").each(function(index) {
        const valor_array = this.value.split("|");
        if (valor_array[1] == "false") {
            costosAdd += parseFloat(valor_array[0]);
        }

    });
}

$("#idcotizacion").change(function () {
    $("#nit_asegurado").val("").select2();
    $("#nit2").val("").select2();
    $("#nit3").val("").select2();
    $("#nit4").val("").select2();
    $("#nit5").val("").select2();

    const btnEliminar = `<button class="btn btn-danger btn-circle" id="${$(`#selectRepuestos${r}`).val()
        }" type="button" onclick="eliminarAccesorio(this.id)"><i class="fa fa-remove"></i></button>`;

    $("#tablaVehiculos").find("tbody").empty();

    $("#tablaVehiculos").find("tbody")
        .append(
            '<tr><td align="center"><input class="vhseleccion" type="radio" name="vhseleccion" value="Ninguno" /></td>' +
            '<td align="center" colspan="4">Ninguno</td>' +
            "</tr>");

    $.ajax({
        url: "/vpedidos/BuscarCotizacion",
        data: {
            cotizacion: $("#idcotizacion").val(),
        },
        type: "post",
        cache: false,

        success: function(data) {
          
            console.log("entro");
            for (var i = 0; i < data.vh.length; i++) {
                $("#tablaVehiculos").find("tbody").append(
                    `<tr><td align="center"><input class="vhseleccion" type="radio" name="vhseleccion" value="${
                    data.vh[i].idmodelo
                    }" id="vhseleccion" checked="checked"></td><td align="center">${data.vh[i].modelo
                    }</td><td align="center">${data.vh[i].anio
                    }</td><td align="center">$${addCommas(data.vh[i].precio)}</td><td align="center">${data.vh[i].color
                    }</td></tr>`);
            }

            console.log(data.cot[0].id_tercero);
            $("#nit").val(data.cot[0].id_tercero);
            $("#mns_clienteNoExiste").html(
                `<i class="fa fa-warning"></i>&nbsp;Cliente desactualizado, debe ser actualizado antes de continuar. <a class="text-warning" href="@Url.Action("update","icb_terceros")?menu=@ViewBag.id_menu&&id${
                data.cot[0].id_tercero}" target="_blank"><u>CLICK AQUÍ</u></a>`);
            $("#vendedor").val(data.cot[0].asesor).trigger("change");

            $("#nit").select2();
            //$('#vendedor').select2();
            $("#bnombreAsesor").select2();


            if ($("#idcotizacion").val() != "") {
                $("#div_nit").hide();
            } else {
                $("#div_nit").show();
            }
          
            $("#lista_repuestos").val(data.rpt.length);
            $("#tablaRepuestos").find("tbody").empty();

            for (var i = 0; i < data.rpt.length; i++) {
                
                r = i + 1;
                var calcularValorTotal = Math.round(data.rpt[i].cantidad * data.rpt[i].valor);

                $("#tablaRepuestos").append(
                    `<tr id="fila_accesorio_${r}"><td>${data.rpt[i].nombre}</td><td>No</td><td>${
                    addCommas(data.rpt[i].valor)}</td><td>0%</td><td>${addCommas(data.rpt[i].cantidad)
                    }</td><td><input type="hidden" class="total_accesorios" name="totalRespuesto${r
                    }" id="totalRespuesto${r}" value="${calcularValorTotal
                    }|false"/>$ ${addCommas(calcularValorTotal)
                    }</td><td><button class="btn btn-danger btn-circle" type="button" onclick="eliminarAccesorio(${r
                    })"><i class="fa fa-remove"></i></button></td><td hidden><input type="hidden" class="accesorios_valores" id="id_accesorio_${
                    r}" value="${r}|${data.rpt[i].codigo}|${data.rpt[i].valor}|false|${calcularValorTotal}|0|${
                    data.rpt[i].cantidad}"></td></tr>`);
            }

            agregarRetoma();
            for (var i = 0; i < data.ret.length; i++) {
                $(".placa_retoma").val(data.ret[0].placa_retoma);
                $("#placa_retoma1").val(data.ret[0].placa_retoma);
                $(".kl_retoma").val(data.ret[0].kl_retoma);
                $("#txtModelo_retoma1").val(data.ret[0].modelo_retoma);
                $(".valor_retoma").val(data.ret[0].valor_retoma);
                $("#txtObligacion_retoma").val(data.ret[0].obligaciones);
                $("#txtValorObligacion").val(data.ret[0].valor_obligacion);
                agregarRetoma();
            }
            buscar_tercero();
        },
        complete: function(data) {
            //$('#tablaVehiculos').dataTable({
            //    dom: "<'row'<'col-sm-4'l><'col-sm-4 text-center'B><'col-sm-4'f>>t<'col-sm-4'i>p",
            //    "lengthMenu": [[10, 25, 50, -1], [10, 25, 50, "All"]],
            //    buttons: [
            //       { extend: 'excel', className: 'btn-sm', text: 'Exportar a Excel' },
            //       { extend: 'pdf', title: 'ExampleFile', className: 'btn-sm', text: 'PDF' },
            //       { extend: 'print', className: 'btn-sm', text: 'Imprimir' }
            //    ]
            //});
        }
    });
});

$("#usado").click(function() {
    $("#marcvh_id").val($("#marcvh_id option:first").val()).select2();
    $("#modelo").empty().select2();
    $("#id_anio_modelo").empty().select2();
    $("#colVh_id").empty().select2();
    //$('#Color_Deseado').empty().select2();
    //$('#color_opcional').empty().select2();
    $(".marca").show();
});

$("#nuevo").click(function() {
    $(".marca").hide();
});


$("#codflota").change(function() {

    $.ajax({
        url: "/vpedidos/BuscarFlota",
        data: {
            flota: $("#codflota").val(),
        },
        type: "post",
        cache: false,
        success: function(data) {
            console.log(data);
            //$('#nitFlota').text(data[0].nit_flota + ' - ' + data[0].nombre);
            $("#flota").empty().select2();
            if ($("#codflota").val() != "") {
                $("#flotaNumero").show();
            }
            if ($("#codflota").val() == "") {
                $("#flota").val("").select2();
                $("#flotaNumero").hide();
            }

            $("#flota").append($("<option>",
                {
                    value: "",
                    text: "Seleccione"
                }));

            for (let i = 0; i < data.length; i++) {
                $("#flota").append($("<option>",
                    {
                        value: data[i].id,
                        text: data[i].nombre
                    }));
            }
            $("#flota").select2();
        },
        complete: function(data) {
            if ($("#codflota").val() == "") {
                $("#flota").val("").select2();
                $("#flotaNumero").hide();
            }
        }
    });
});

$("#flota").change(function() {
    if ($("#flota").val() == "") {
        $("#codflota").val("").select2();
        $("#flotaNumero").hide();
    }
});

$("#cargomatricula").change(function() {
    if ($("#cargomatricula").val() == 1) {
        $("#divValorMatricula").show();
        $("#divPorcObsequio").hide();
        $("#valormatricula").val($("#matriculaOculto").val());
        $("#obsequioporcen").val(0);

    }

    if ($("#cargomatricula").val() == 2) {
        $("#divValorMatricula").show();
        $("#divPorcObsequio").show();
        $("#valormatricula").val(0);
    }

    if ($("#cargomatricula").val() == 3) {
        $("#divValorMatricula").hide();
        $("#divPorcObsequio").hide();
        $("#valormatricula").val(0);
    }
    calcular_valor_total();
});

function buscar_tercero() {
    if (!$("#nit").val()) {
        $("#mns_clienteNoExiste").show();
        $("#enviarSeleccionx").prop("disabled", true);
        $("#enviarSeleccionx").hide();
        $("#nombre").text("");
        $("#celular").text("");
        $("#correo").text("");
        $("#telefono").text("");
        $("#direccion").text("");
        $("#ciudad").text("");
    } else {
        $("#mns_clienteNoExiste").hide();
        $.ajax({
            url: "/vpedidos/BuscarCliente",
            data: {
                cliente: $("#nit").val()
            },
            type: "post",
            cache: false,
            success: function(data) {
                console.log(data);
                if (data.cliente == true) {
                    $("#enviarSeleccionx").prop("disabled", false);
                    $("#enviarSeleccionx").show();
                    $("#idTercero").val(data.info[0].idTercero);
                    $("#nombre").text(data.info[0].nombre);
                    $("#celular").text(data.info[0].celular);
                    $("#correo").text(data.info[0].correo);
                    $("#telefono").text(data.info[0].telefono);
                    $("#direccion").text(data.info[0].direccion);
                    $("#ciudad").text(data.info[0].ciudad);
                } else {
                    $("#mns_clienteNoExiste").html(
                        `<i class="fa fa-warning"></i>&nbsp;El tercero seleccionado no es cliente, debe ser actualizado antes de continuar. <a class="text-warning" onclick="linkTerceros(${
                        $("#nit").val()})" target="_blank"><u>CLICK AQUÍ</u></a>`);
                    $("#mns_clienteNoExiste").show();
                    $("#enviarSeleccionx").prop("disabled", true);
                    $("#enviarSeleccionx").hide();
                    $("#nombre").text("");
                    $("#celular").text("");
                    $("#correo").text("");
                    $("#telefono").text("");
                    $("#direccion").text("");
                    $("#ciudad").text("");
                }
            },
        });
    }
}

//const number = document.querySelector('.number');
//number.addEventListener('keyup', (e) => {
//    console.log(number);
//    const element = e.target;
//    const value = element.value;
//    element.value = formatNumber(value);
//});

var numero_miles = "";

function formatNumber(n) {
    const array_number = n.split(",");
    var complemento = "";
    n = String(array_number[0]).replace(/\D/g, "");
    if (array_number.length > 1) {
        complemento = `,${array_number[1]}`;
    }

    return n === "" ? n : String(n).replace(/\B(?=(\d{3})+(?!\d))/g, ".") + complemento;
}

function miles(id) {
    numero_miles = formatNumber($(`#${id}`).val());
    $(`#${id}`).val(numero_miles);
    console.log($(`#${id}`).val);
}

function seleccionCredito(id, valor, banco) {

    $("#slcCondicion").val("1").select2();
    $("#slcBanco").val(banco).select2();
    $("#txtValor").val(valor);
    $("#idcreditoseleccionado").val(id);
}

function enviarSeleccion() {
    const documento = $("#nit").val();
    const modelo = $("input:radio[name=vhseleccion]:checked").val();
    const cotizacion = $("#idcotizacion").val();
    const nombre = $("#nombreAsesor").val();
    const asesor = $("#vendedor").val();
    const nit = $("#nit").val();

    if ($("#nit").val() == null ||
        $("#nit").val() == "" ||
        ($("#vendedor").val() == "" && ($("#nombreAsesor").val() == "" || $("#nombreAsesor").val() == undefined))) {
        swal("Los campos con (*) son obligatorios", "Cédula/Nit y Asesor son obligatorios", "error");
    } else if (($("#idcotizacion").val() != "" || $("#nit").val() != "") && ($("#vendedor").val() != "") ||
        ($("#nombreAsesor").val() != "")) {
        //buscamos que el tercero tenga creditos, si es asi le cargamos la info en la seccion de formas de pago ->
        $.ajax({
            url: "/vpedidos/BuscarCreditos",
            data: {
                idTercero: $("#idTercero").val(),
            },
            type: "post",
            cache: false,
            success: function(data) {
                if (data != null) {
                    $("#tieneCreditos").show("1000");
                    for (let k = 0; k < data.length; k++) {
                        $("#tablatieneCreditos").append(
                            `<tr id="lineaCredito${k
                            }"><td align="center"><input type="radio" name="rbCredito" id="rbCredito" value="${data[k]
                            .Id
                            }" onclick="seleccionCredito('${data[k].Id}','${data[k].vaprobado}','${data[k].financiera_id
                            }')" /></td><td align="right">${data[k].fecha
                            }</td><td align="center"><input type="hidden" name="estadoC${k}" id="estadoC${k}" value="${
                            data[k].descripcion}"/>${data[k].descripcion
                            }</td><td align="center"><input type="hidden" name="valorS${k}" id="valorS${k}" value="${
                            data[k].vsolicitado}"/>$ ${data[k].vsolicitado
                            }</td><td align="center"><input type="hidden" name="valorA${k}" id="valorA${k}" value="${
                            data[k].vaprobado}" />$${data[k].vaprobado}</td></tr>`);
                    }
                }
            }
        });

        //validacion para cargar si tiene o no vehiculo seleccionado
        if (modelo != "Ninguno") {

            $.ajax({
                url: "/vpedidos/BuscarVh",
                data: {
                    modelo: modelo,
                    cotizacion: cotizacion,
                },
                type: "post",
                cache: false,
                success: function(data) {
                    console.log(data);
                    $("#marcvh_id").prop("readonly", true);
                    $("#modelo").prop("readonly", true);
                    $("#id_anio_modelo").prop("readonly", true);
                    //$('#nuevo').val(data[0].nuevo);
                    //$('#usado').val(data[0].usado);
                    $("#marcvh_id").val(data[0].marcvh_id);
                    $("#modelo").val(data[0].modvh_codigo);
                    $("#id_anio_modelo").val(data[0].anomodelo);
                    $("#vrtotal").val((data[0].precio).toFixed(2));
                    $("#Color_Deseado").val(data[0].color);
                    $("#valorPoliza").val(addCommas(data[0].poliza));
                    $("#porcentaje_iva").val(data[0].porcentaje_iva);
                    $("#porcentaje_impoconsumo").val(data[0].impuesto_consumo);
                    $("#vrtotal").val(addComas((data[0].precio).toFixed(2)));
                    $("#valorsoat").val(addComas(data[0].soat));
                    $("#matriculaOculto").val(addComas(data[0].matricula));
                    $("#tipo_vh").text(data[0].tipo_vh);

                    var valorUnitario = Math.round(data[0].precio /
                        (1 + ((data[0].porcentaje_iva + data[0].impuesto_consumo) / 100)));
                    $("#valor_unitario").val(addComas(parseInt(valorUnitario)));

                    $("#marcvh_id").select2();
                    $("#modelo").select2();
                    $("#id_anio_modelo").select2();

                    $("#Color_Deseado").select2();

                    $("#div_vhcotizados").hide();
                    $("#div_datospedido").show();
                    $(".vhcotizado").show();

                    if ($("#pordscto").val() != null || $("#pordscto").val() != "") {
                        var valor_descuento =
                            (parseInt(quitCommas($("#valor_unitario").val())) * $("#pordscto").val()) / 100;
                        $("#vrdescuento").val(addComas(parseInt(valor_descuento)));
                    }

                    if ($("#porcentaje_impoconsumo").val() != null || $("#porcentaje_impoconsumo").val() != "") {
                        var valor_porcentaje = (parseInt(quitCommas($("#valor_unitario").val())) *
                                $("#porcentaje_impoconsumo").val()) /
                            100;
                        $("#valor_impuesto").val(addComas(parseInt(valor_porcentaje)));
                    }

                    if ($("#porcentaje_iva").val() != null || $("#porcentaje_iva").val() != "") {
                        var valor_porcentaje =
                            (parseInt(quitCommas($("#valor_unitario").val())) * $("#porcentaje_iva").val()) / 100;
                        $("#valor_iva").val(addComas(parseInt(valor_porcentaje)));
                    }
                },
            });
        } else {
            r = 0;
            $("#lista_repuestos").val(0);
            $("#tablaRepuestos").find("tbody").empty();
            $("#div_vhcotizados").hide();
            $("#div_datospedido").show();
            $(".vhotro").show();
            $("#valorfaltantetext").html(`Valor Faltante $${0}`);
            agregarRetoma();
        }
        setTimeout(function() {
                agregarAccesorio();
            },
            3000);
    } else {
        $("#obligatorioSeleccion").show();
    }

}


$(".suma").change(function() {
    ValorTotalCotizacion();
});

$(".resta").change(function() {
    ValorTotalCotizacion();
});

function ValorTotalCotizacion() {
    const valorUnitario = (parseInt(quitCommas($("#valor_unitario").val())));
    const valorDescuento = (parseInt(quitCommas($("#vrdescuento").val()))) || 0;
    const valorIVA = (parseInt(quitCommas($("#valor_iva").val())));
    const valorImpuesto = (parseInt(quitCommas($("#valor_impuesto").val())));

    const valorTotal = (valorUnitario - valorDescuento) + valorIVA + valorImpuesto;
    $("#vrtotal").val(addComas(parseInt(valorTotal)));

}
//function sumar(){
//    var valor_total = parseInt(quitCommas($('#valor_unitario').val())) + parseInt(quitCommas($('#valor_impuesto').val())) + parseInt(quitCommas($('#valor_iva').val()));
//    valor_total = valor_total.toString().replace('.', ',');
//    $('#vrtotal').val(addComas(parseInt(valor_total)));
//}

//function restar(){
//    //var valor_total = quitCommas(parseInt($('#vrtotal').val())) - quitCommas(parseInt($('#vrdescuento').val()));
//    var valor_total = (parseInt(quitCommas($('#valor_unitario').val())) + parseInt(quitCommas($('#valor_impuesto').val())) + parseInt(quitCommas($('#valor_iva').val()))) - parseInt(quitCommas($('#vrdescuento').val())) ;
//    valor_total = valor_total.toString().replace('.', ',');
//    $('#vrtotal').val(addCommas(valor_total))
//}


function agregarAccesorio() {
    r++;
    const optionBanco = "";
    const btnAgregar =
        '<button class="btn btn-success" type="button" onclick="pintarTablaAccesorio()"><i class="fa fa-plus"></i>&nbsp;Agregar</button>';
    $("#repuestos").html(
        `<div class="row"><div class="col-lg-12"><div class="row"><label class="control-label col-md-3">Tipo Accesorio:</label><div class="col-md-3"><input type="radio" name="tipoRepuesto${
        r}" onclick="rbGenericos(${r})" value="generico" id="rbGenerico${r
        }" checked /> Accesorio Generico</div><div class="col-md-3"><input type="radio" name="tipoRepuesto${r
        }" onclick="rbModelos(${r})" value="modelo" id="rbModelo${r
        }" /> Accesorio Por Modelo</div><div class="col-md-3"><input type="checkbox" data-toggle="toggle" name="slcobsequio${
        r}" id="slcobsequio${r
        }" /> Obsequio</div></div></div></div><br /><br /><div class="col-lg-12"><div class="form-group"><div class="row" style="display:none" id="filaVehiculosOculta${
        r
        }"><label class="control-label col-md-3">Vehiculos Seleccionados:&nbsp;<span class="text-danger">*</span></label><div class="col-md-4"><select id="selectModelosCotizados${
        r}" class="form-control" placeholder="Seleccione..." onchange="BuscarAccesoriosModeloEspecifico(${r
        })"></select></div></div><div class="row"><label class="control-label col-md-3">Referencia:&nbsp;<span class="text-danger">*</span></label><div class="col-md-4"><select class="form-control" onchange="costos(${
        r})" name="selectRepuestos${r}" id="selectRepuestos${r
        }" placeholder="Seleccione..."></select></div></div><div class="row"><label class="control-label col-md-3">Precio:&nbsp;<span class="text-danger">*</span></label><div class="col-md-4"><input type="text" class="form-control" name="txtCostoAccesorio${
        r}" id="txtCostoAccesorio${r
        }" onkeyUp = "return miles(this.id)" placeholder="Digite precio" readonly/></div></div><div class="row"><label class="control-label col-md-3">Descuento:&nbsp;<span class="text-danger">*</span></label><div class="col-md-4"><input class="form-control"  name="por_desc${
        r}" id="por_desc${r
        }" placeholder="Digite porcentaje"/></div></div><div class="row"><label class="control-label col-md-3">Cantidad:&nbsp;<span class="text-danger">*</span></label><div class="col-md-4"><input type="text" class="form-control" name="txtCantidadAccesorio${
        r}" id="txtCantidadAccesorio${r
        }" onkeyUp = "return miles(this.id)" placeholder="Digite cantidad"/></div></div><div class="row"><div class="col-md-2 col-md-offset-3"><div class="rpt">${
        btnAgregar}</div></div></div></div></div></div>`);

    $(`#selectRepuestos${r}`).append($("<option>",
        {
            value: "",
            text: ""
        }));
    $(`#selectRepuestos${r}`).val("").select2();
    $(`#slcobsequio${r}`).bootstrapToggle();
    calcular_valor_total();
    rbGenericos(r);
}

//----Funcion que valida que el campo solo tenga permitido el ingreso de numeros
function soloNumeros(e) {
    key = e.keyCode || e.which;
    tecla = String.fromCharCode(key).toLowerCase();
    letras = "1234567890";
    especiales = "8-37-39-46";

    tecla_especial = false;
    for (let i in especiales) {
        if (key == especiales[i]) {
            tecla_especial = true;
            break;
        }
    }
    if (letras.indexOf(tecla) == -1 && !tecla_especial) {
        return false;
    }
}

function pintarTablaAccesorio() {

    const x = r;
    $("#lista_repuestos").val(x);
    var obsequio = "";
    var valorObsequio = "";

    if ($(`#slcobsequio${x}`).prop("checked")) {
        obsequio = "Si";
        valorObsequio = "true";
    } else {
        obsequio = "No";
        valorObsequio = "false";
    }
    //Se capturan los datos desde el formulario --Alex
    if ($(`#selectRepuestos${r}`).val() == "" ||
        $(`#txtCostoAccesorio${r}`).val() == "" ||
        $(`#por_desc${r}`).val() == "" ||
        $(`#txtCantidadAccesorio${r}`).val() == "") {
        swal("Información!", "Los campos marcados con (*) son obligatorios", "warning");
    } else {
        const accesorio = $(`#selectRepuestos${x}`).val();
        const accesorio_label = $(`#selectRepuestos${x} option:selected`).text();
        const valor_unitario = $(`#txtCostoAccesorio${x}`).val() == ""
            ? 0
            : parseFloat(quitCommas($(`#txtCostoAccesorio${x}`).val()));
        const cantidad = $(`#txtCantidadAccesorio${x}`).val() == ""
            ? ""
            : parseInt($(`#txtCantidadAccesorio${x}`).val());
        const porcentaje_descuento = $(`#por_desc${x}`).val() == "" ? 0 : parseInt($(`#por_desc${x}`).val());
        if (accesorio != "" && valor_unitario != 0 && cantidad != "") {
            let valor_total = valor_unitario * cantidad;
            const descuento =
                parseFloat(((valor_total * porcentaje_descuento) / 100).toFixed(2)); //valor con 2 decimales
            valor_total = valor_total - descuento;
            $("#tablaRepuestos").append(
                `<tr id="fila_accesorio_${x}"><td>${accesorio_label}</td><td>${obsequio}</td><td>${addCommas(
                    valor_unitario)
                }</td><td>${porcentaje_descuento}%</td><td>${addCommas(cantidad)
                }</td><td><input type="hidden" class="total_accesorios" name="totalRespuesto${x}" id="totalRespuesto${x
                }" value="${valor_total
                }|${valorObsequio}"/>$ ${addCommas(valor_total)
                }</td><td><button class="btn btn-danger btn-circle" type="button" onclick="eliminarAccesorio(${x
                })"><i class="fa fa-remove"></i></button></td><td hidden><input type="hidden" class="accesorios_valores" id="id_accesorio_${
                x}" value="${x}|${accesorio}|${valor_unitario}|${valorObsequio}|${valor_total}|${porcentaje_descuento
                }|${
                cantidad}"></td></tr>`);
            agregarAccesorio();

        }
    }
}

function eliminarAccesorio(pos) {
    $(`#fila_accesorio_${pos}`).remove();
    calcular_valor_total();
}

function agregarRetoma() {
    const btnEliminar = `<button class="btn btn-danger btn-circle" id="${re
        }" type="button" onclick="eliminarRetoma(this.id)"><i class="fa fa-remove"></i></button>`;

    re++;

    const option = "";
    const optionBanco = "";
    var obligacion = "";
    var valorobligacion = "";
    const x = re - 1;
    const btnAgregar =
        '<button class="btn btn-success" type="button" onclick="agregarRetoma()"><i class="fa fa-plus"></i> Agregar</button>';

    if ($(`#placa_retoma${x}`).val() != "" ||
        $(`#txtModelo_retoma${x}`).val() != "" ||
        $(`#txtValorRetoma${x}`).val() != "" ||
        $(`#anioVehiculoRetoma${x}`).val() != "" ||
        $(`#txtKlretoma${x}`).val() != "" && x != 0) {
        if (re > 1) {


            $("#lista_retomas").val(x);

            if ($("#txtObligacion_retoma").prop("checked")) {
                obligacion = "Si";
                valorobligacion = "true";
            } else {
                obligacion = "No";
                valorobligacion = "false";

            }

            if ($(`#txtKlretoma${x}`).val() != "undefined" && $(`#txtKlretoma${x}`).val() != "") {

                $("#tablaRetomas").append(
                    `<tr id="div${x}"><td align="center"><input type="hidden" name="modelo_retoma${x
                    }" id="modelo_retoma${x
                    }" value="${$(`#txtModelo_retoma${x}`).val()
                    }"/>${$(`#txtModelo_retoma${x}`).val()
                    }</td><td align="center"><input type="hidden" name="placa_retoma${
                    x}" id="placa_retoma${x}" value="${$(`#placa_retoma${x}`).val()}"/>${$(`#placa_retoma${x}`).val()
                    }</td><td align="center"><input type="hidden" value="${$(`#txtKlretoma${x}`).val()
                    }" name="kl_retoma${x
                    }" id="kl_retoma${x}" />${$(`#txtKlretoma${x}`).val()
                    }</td><td align="center"><input type="hidden" value="${$(`#txtValorRetoma${x}`).val()
                    }" name="valor_retoma${x}" id="valor_retoma${x}"  />$${$(`#txtValorRetoma${x}`).val()
                    }</td><td align="center"><input type="hidden" value="${valorobligacion}" name="obligacion_retoma${x
                    }" id="obligacion_retoma${x
                    }" />${obligacion}</td><td align="center"><input type="hidden" value="${$("#txtValorObligacion")
                    .val()
                    }" name="valor_obligacion${x
                    }" id="valor_obligacion${x}" />${$("#txtValorObligacion").val()}</td><td align="center">${
                    btnEliminar
                    }</td></tr>`);
            }
        }
    } else {
        swal("Información!", "Los campos marcados con (*) son obligatorios", "warning");
    }

    $("#divRetomas").html(
        `<div class="row"><div class="col-sm-6"><div class="form-group"><label class="control-label col-md-4">Placa Retoma:<span class="text-danger">&nbsp;*</span></label><div class="col-md-6"><input class="form-control" name="placa_retoma${
        re}" id="placa_retoma${re
        }" value="" placeholder = "Ingrese Placa" type="text"/><label class="text-danger" id="AlertaRetomaNoEncontrada" style="display:none;">Placa no registrada!</label></div><div class="col-md-2"><button type="button" class="btn btn-default" id="${
        re
        }" onclick="buscarRetoma(this.id)"><i class="fa fa-search"></i></button></div></div></div></div><div class="row"><div class="col-sm-6"><div class="form-group"><label class="control-label col-md-4">Vehículo Retoma:&nbsp;<span class="text-danger">&nbsp;*</span></label><div class="col-md-6"><input class="form-control modelo_retoma" name="txtModelo_retoma${
        re}" id="txtModelo_retoma${re
        }" value=""/></div></div></div><div class="col-sm-6"><div class="form-group"><label class="control-label col-md-4">Año Modelo:&nbsp;<span class="text-danger">&nbsp;*</span></label><div class="col-md-6"><input class="form-control placa_retoma" name="anioVehiculoRetoma${
        re}" id="anioVehiculoRetoma${re
        }" value="" /></div></div></div></div><div class="row"><div class="col-sm-6"><div class="form-group"><label class="control-label col-md-4">Valor Retoma:&nbsp;<span class="text-danger">&nbsp;*</span></label><div class="col-md-6"><input class="form-control valor_retoma number" name="txtValorRetoma${
        re}" id="txtValorRetoma${re
        }" value="" onkeyUp = "return miles(this.id)"  /></div></div></div><div class="col-sm-6"><div class="form-group"><label class="control-label col-md-4">Kilometraje:&nbsp;<span class="text-danger">&nbsp;*</span></label><div class="col-md-6"><input class="form-control kl_retoma" name="txtKlretoma${
        re}" id="txtKlretoma${re
        }" value="" onkeyUp = "return miles(this.id)"  /></div></div></div></div><div class="col-sm-6"><div class="form-group"><label class="control-label col-md-4">Obligaciones:&nbsp;</label><div class="col-md-6"><input name="txtObligacion_retoma" type="checkbox" id="txtObligacion_retoma" data-toggle = "toggle" data-size = "mini" value="false" onChange="checkretoma()"/></div></div></div><div class="col-sm-6"><div class="form-group" id="div_obligacion"><label class="control-label col-md-4">Valor Obligación:&nbsp;<span class="text-danger">&nbsp;*</span></label><div class="col-md-6"><input class="form-control number" name="txtValorObligacion" id="txtValorObligacion" value="" onkeyUp = "return miles(this.id)"  /></div></div><div class="col-md-2 re">${
        btnAgregar}</div></div>`
    );
    $("#txtObligacion_retoma").bootstrapToggle("off");
}

function buscarRetoma(id) {
    $.ajax({
        url: "/cotizacion/buscarPlacaRetoma",
        data: {
            placa: $(`#placa_retoma${id}`).val()
        },
        type: "post",
        cache: false,
        success: function(data) {
            console.log(data);
            if (data.encontrado == false) {
                $(`#txtModelo_retoma${id}`).val("");
                $(`#anioVehiculoRetoma${id}`).val("");
                $("#AlertaRetomaNoEncontrada").show();
                setTimeout(function() {
                        $("#AlertaRetomaNoEncontrada").fadeOut(1500);
                    },
                    3000);
            } else {
                $(`#txtModelo_retoma${id}`).val(data.buscarPlaca.modvh_nombre);
                $(`#anioVehiculoRetoma${id}`).val(data.buscarPlaca.anio_vh);
            }
        }
    });
};

function checkretoma() {
    if ($("#txtObligacion_retoma").prop("checked")) {
        $("#div_obligacion").show();
    } else {
        $("#txtValorObligacion").val("");
        $("#div_obligacion").hide();
    }
}

function eliminarRetoma(id) {

    $(`#div${id}`).remove();
}

function addCommas(nStr) {
    nStr += "";
    x = nStr.split(".");
    x1 = x[0];
    x2 = x.length > 1 ? `,${x[1]}` : "";
    const rgx = /(\d+)(\d{3})/;
    while (rgx.test(x1)) {
        x1 = x1.replace(rgx, "$1" + "." + "$2");
    }
    return x1 + x2;
}

function quitCommas(nStr) {
    nStr.toString();
    //console.log(nStr);
    var s = nStr.replace(/\./g, "");
    s = s.replace(/\,/g, ".");
    return s;
}

function rbGenericos(id) {

    $.ajax({
        url: "/cotizacion/buscarRepuestosGenericosPedido",
        data: {
        },
        type: "post",
        cache: false,
        success: function(data) {
            //$('#rbGenerico' + id + '').empty();
            //$('#txtCostoAccesorio' + id + '').val('');
            //$('#selectRepuestos'+id+'').append($('<option>', {
            //    value: '',
            //    text: ''
            //}));

            $(`#txtCostoAccesorio${id}`).val("");
            $(`#selectRepuestos${id}`).empty();
            $(`#selectRepuestos${id}`).append($("<option>",
                {
                    value: "",
                    text: "Seleccione"
                }));
            for (let i = 0; i < data.length; i++) {
                $(`#selectRepuestos${id}`).append($("<option>",
                    {
                        value: data[i].ref_codigo,
                        text: `${data[i].ref_codigo} - ${data[i].ref_descripcion} - ${data[i].alias}`
                    }));
            }
            $(`#selectRepuestos${id}`).select2();
        }
    });
}

function rbModelos(id) {
    console.log("hola");
    $.ajax({
        url: "/cotizacion/buscarRepuestosPorModelo",
        data: {
            modelo: $("#modelo").val()
        },
        type: "post",
        cache: false,
        success: function(data) {
            $(`#txtCostoAccesorio${id}`).val("");

            $(`#selectRepuestos${id}`).empty();
            $(`#selectRepuestos${id}`).append($("<option>",
                {
                    value: "",
                    text: "Seleccione"
                }));
            for (let i = 0; i < data.length; i++) {
                $(`#selectRepuestos${id}`).append($("<option>",
                    {
                        value: data[i].referencia,
                        text: `${data[i].descripcion} - ${data[i].alias}`
                    }));
            }
            $(`#selectRepuestos${id}`).select2();
        }
    });
}

function costos(id) {
    $.ajax({
        url: "/cotizacion/buscarPrecioReferencia",
        data: {
            idRepuesto: $(`#selectRepuestos${id}`).val()
        },
        type: "post",
        cache: false,
        success: function(data) {
            $(`#txtCostoAccesorio${id}`).val(addCommas(data.precio1));
        },
        complete: function(data) {
            if (data.responseText != "") {
                console.log("tiene algo");
            } else {
                $(`#txtCostoAccesorio${id}`).val("0");
            }
        }
    });
}


function agregarCostos() {
    cos++;
    $("#lista_costos").val(cos);
    var obsequio = "";
    var valor = "";
    const btnEliminar = `<button class="btn btn-danger btn-circle" id="${cos
        }" type="button" onclick="eliminarCostos(this.id)"><i class="fa fa-remove"></i></button>`;


    if ($("#txtObsequio_costo").prop("checked")) {
        obsequio = "Si";
        valor = true;
    } else {
        obsequio = "No";
        valor = false;
    }
    const x = r;
    var descripcion = $("#txtDescripcion_costo").val();
    var costo = parseFloat(quitCommas($("#txtValor_costo").val()));

    if (descripcion != "" &&
        costo != "" &&
        descripcion != null &&
        costo != null &&
        descripcion != "" &&
        costo != "" &&
        descripcion != undefined &&
        costo != undefined) {

        $("#tablaCostos").append(
            `<tr id="div_cos${cos}"><td>${$("#txtDescripcion_costo").val()}</td><td>${obsequio
            }</td><td><input type="hidden" class="total_costos" name="valor_costo${cos
            }" id="valor_costo${cos}" value="${costo}|${valor}"/>${$("#txtValor_costo").val()}</td><td>${btnEliminar
            }</td><td hidden><input type="text" class="costos_valores" id="id_fpago_${cos}" value="${cos}|${
            $("#txtDescripcion_costo").val()}|${valor}|${costo
            }"></td></tr>`);
        var descripcion = $("#txtDescripcion_costo").val("");
        var costo = $("#txtValor_costo").val("");
    } else {
        cos--;
        swal("Información!", "Los campos marcados con (*) son obligatorios", "warning");
        //agregarCostos();
    }
    calcular_valor_total();
}

function eliminarCostos(id) {
    $(`#div_cos${id}`).remove();
    calcular_valor_total();
}

function validarObligatorios() {
    //se extraen los datos ingresados en formas de pago, accesorios y costo adicionales, se serializan en json y se asignan a una variable que sera enviada por post

    var fpago = "";
    var accesorios = "";
    var costos = "";
    $(".fpago_valores").each(function(index) {
        fpago += this.value + "~";
    });

    $(".accesorios_valores").each(function(index) {
        accesorios += this.value + "~";
    });

    $(".costos_valores").each(function(index) {
        costos += this.value + "~";
    });

    $("#formas_depago_json").val(fpago);
    $("#accesorios_json").val(accesorios);
    $("#costos_json").val(costos);

    ////console.log(fpago_json, accesorios_json, costos_json);
    if ($("#codflota").val() != "") {
        $(".vehiculo").tab("show");
        $("#flota").prop("required", true);
    }
    //if ($("#marcvh_id").val() == "") {
    //    $(".vehiculo").tab("show");
    //    $("#marcvh_id").prop("required", true);
    //}
    //if ($("#modelo").val() == "") {
    //    $(".vehiculo").tab("show");
    //    $("#modelo").prop("required", true);
    //}
    //if ($("#id_anio_modelo").val() == "") {
    //    $(".vehiculo").tab("show");
    //    $("#id_anio_modelo").prop("required", true);
    //}
    //if ($("#servicio").val() == "") {
    //    $(".vehiculo").tab("show");
    //    $("#servicio").prop("required", true);
    //} else {
    //    $("#btnGuardar").prop("disabled", false);

    //    $("#btnGuardar").click();
    //}
    if ($("#servicio").val() != "" ||
        $("#id_anio_modelo").val() != "" ||
        $("#modelo").val() != "" ||
        $("#marcvh_id").val() != "") {
        $("#btnGuardar").prop("disabled", false);
        $("#btnGuardar").click();
    }
}

$("#iddepartamento").change(function() {
    $("#idciudad").empty();
    $("#idciudad").val("").select2();

    $.ajax({
        url: "/cotizacion/buscarCuidadesDepartamento",
        data: {
            id: $("#iddepartamento").val()
        },
        type: "post",
        cache: false,
        success: function(data) {
            console.log(data);
            $("#idciudad").empty();
            $("#idciudad").append($("<option>",
                {
                    value: "",
                    text: ""
                }));
            for (let i = 0; i < data.length; i++) {
                $("#idciudad").append($("<option>",
                    {
                        value: data[i].ciu_id,
                        text: data[i].ciu_nombre
                    }));
            }
            //$('#modvh_codigo').val('').select2();
            $("#idciudad").select2();
        }
    });
});

//Se crea una función que hace una enrutamiento con un evento onclick
function linkTerceros(id_tercero) {
    const menu = $("#menu").val();
    window.open(`../../icb_terceros/update?id=${id_tercero}`, "_blank");
}