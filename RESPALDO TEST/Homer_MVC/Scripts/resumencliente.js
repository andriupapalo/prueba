 function buscardocumento (documento) {
    $.ajax({
        url: '/crmMenu/BuscarTerceroPorId',
        data: {
            documento: documento
        },
        type: "post",
        cache: false,
        success: function (data) {
            if (data.encontrado == true) {
                if (data.tercero.idtpdoc == 1) {
                    $('.persona_naturalx').hide();
                    $('.persona_juridicax').show();

                }
                else {
                    $('.persona_naturalx').show();
                    $('.persona_juridicax').hide();
                }
                $('#documentoCrm').val(data.tercero.doc_tercero);
                $('#primerNombreCrm').val(data.tercero.prinom_tercero);
                $('#segundoNombreCrm').val(data.tercero.segnom_tercero);
                $('#primerApellidoCrm').val(data.tercero.apellido_tercero);
                $('#segundoApellidoCrm').val(data.tercero.segapellido_tercero);
                $('#razonSocialCrm').val(data.tercero.razon_social);
                $('#edadterceroCrm').val(data.tercero.edad);

                $('#telefonoCrm').val(data.tercero.telf_tercero);
                $('#celularCrm').val(data.tercero.celular_tercero);
                $('#emailCrm').val(data.tercero.email_tercero);
                $('#direccionCrm').val(data.tercero.direc_tercero);
                $('#fechaNacimientoCrm').val(data.tercero.fec_nacimiento);
                $('#contacto_emailsCrm').val(data.tercero.habdtautor_correo);
                $('#contacto_celularsCrm').val(data.tercero.habdtautor_celular);
                $('#contacto_msnmsCrm').val(data.tercero.habdtautor_msm);
                $('#contacto_whatsappsCrm').val(data.tercero.habdtautor_watsap);
                //lleno la tabla de direcciones
                $('#tablaDireccionesTerceroCrm').find('tbody').empty();
                for (var i = 0; i < data.direcciones.length; i++) {
                    $('#tablaDireccionesTerceroCrm').find('tbody').append('<tr><td align="left">'
                                    + data.direcciones[i].ciudad + '</td><td align="left">'
                                    + data.direcciones[i].sector + '</td><td align="left">'
                                    + data.direcciones[i].direccion + '</td></tr>');
                }
                //lleno la tabla de contactos
                $('#tablaContactosTerceroCrm').find('tbody').empty();
                for (var i = 0; i < data.contactox.length; i++) {
                    $('#tablaContactosTerceroCrm').find('tbody').append('<tr><td align="left">'
                                    + data.contactox[i].cedula + '</td><td align="left">'
                                    + data.contactox[i].nombre + '</td><td align="left">'
                                    + data.contactox[i].direccion + '</td><td align="left">'
                                    + data.contactox[i].telefono + '</td><td align="left">'
                                    + data.contactox[i].correo + '</td><td align="left">'
                                    + data.contactox[i].tipo_contacto + '</td><td align="left">'
                                    + data.contactox[i].fechacumpleanos + '</td><td align="left">'
                                    + data.contactox[i].estado_contacto + '</td></tr>');
                }
                // Llenar datos de la tabla de vehiculos cotizados
                $('#tablaVehiculosCotizadosCrm').find('tbody').empty();
                for (var i = 0; i < data.vehiculos.length; i++) {
                    $('#tablaVehiculosCotizadosCrm').find('tbody').append('<tr><td align="center">'
                                    + data.vehiculos[i].cot_feccreacion + '</td><td align="center">'
                                    + data.vehiculos[i].modvh_nombre + '</td><td align="center">'
                                    + data.vehiculos[i].anomodelo + '</td><td align="center">'
                                    + data.vehiculos[i].colvh_nombre + '</td><td align="center"> $'
                                    + addCommas(data.vehiculos[i].precio) + '</td></tr>');
                }

                $('#tablaVehiculosPedidosCrm').find('tbody').empty();
                for (var i = 0; i < data.pedidos.length; i++) {
                    $('#tablaVehiculosPedidosCrm').find('tbody').append('<tr><td align="center">'
                                    + data.pedidos[i].fecha + '</td><td align="center">'
                                    + data.pedidos[i].modvh_nombre + '</td><td align="center">'
                                    + data.pedidos[i].id_anio_modelo + '</td><td align="center">'
                                    + data.pedidos[i].colvh_nombre + '</td><td align="center"> $'
                                    + addCommas(data.pedidos[i].valor_unitario) + '</td></tr>');
                }

                $('#div_datosTercero').show();
            } else {
                $('#div_datosTercero').hide();
            }
        },
        complete: function (data) {
            $('#modalCrm').modal('show');
        }
    })
};

