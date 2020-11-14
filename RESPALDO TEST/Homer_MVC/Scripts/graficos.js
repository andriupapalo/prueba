function torta(inicio, fin, bodegas) {
    $.ajax({
        url: '/IndicadoresGraficos/graficoTorta',
        data: {
            fechaInicio: inicio,
            fechaFin: fin,
            bodegas: bodegas
        },
        type: 'POST',
        cache: false,
        success: function (data) {
            var datos = []
            for (var i = 0; i < data.length; i++) {
                datos[i] = {
                    "nombre": data[i].nombreBodega,
                    "valor": parseInt(data[i].valor),
                    "descripcion": '$' + addCommas(parseInt(data[i].valor))
                }
            }
            var chart = AmCharts.makeChart("chartdiv", {
                "type": "pie",
                "theme": "light",
                "dataProvider": datos,
                "valueField": "valor",
                "titleField": "nombre",
                "descriptionField": "descripcion",
                "outlineAlpha": 0.4,
                "depth3D": 15,
                "balloonText": "[[title]]<br><span style='font-size:14px'><b>[[description]]</b> ([[percents]]%)</span>",
                "angle": 30,
                "legend": {
                    "useGraphSettings": false,
                    "valueText": ""
                },
                "export": {
                    "enabled": true
                }


            });

            chart.addListener("clickSlice", function (event, index) {
                var posicion = event.dataItem.index;
                for (var x = 0; x < data.length; x++) {
                    if (x == posicion) {
                        detalleBodega(data[x].id)
                    }
                }


            });
        }
    })


}
function addCommas(nStr) {
    nStr += '';
    x = nStr.split('.');
    x1 = x[0];
    x2 = x.length > 1 ? '.' + x[1] : '';
    var rgx = /(\d+)(\d{3})/;
    while (rgx.test(x1)) {
        x1 = x1.replace(rgx, '$1' + '.' + '$2');
    }
    return x1 + x2;
}
function tortaKardex(anio, mes, bodegas, referencias) {
    $.ajax({
        url: '/IndicadoresGraficos/graficoKardex',
        data: {
            anio: anio,
            mes: mes,
            bodegas: bodegas,
            referencias: referencias
        },
        type: 'post',
        cache: false,
        success: function (data) {
            var datos = [];
            for (var i = 0; i < data.length; i++) {
                datos[i] = {
                    "title": data[i].bodega,
                    "valor": parseInt(data[i].costoStock),
                    "descripcion": '$'+ addCommas(data[i].costoStock)
                }
            }
            var chart = AmCharts.makeChart("torta", {
                "type": "pie",
                "theme": "light",
                "dataProvider": datos,
                "valueField": "valor",
                "titleField": "title",
                "descriptionField": "descripcion",
                "outlineAlpha": 0.4,
                "depth3D": 15,
                "balloonText": "[[title]]<br><span style='font-size:14px'><b>[[description]]</b> ([[percents]]%)</span>",
                "angle": 30,
                "legend": {
                    "useGraphSettings": false,
                    "valueText":""
                },
                "export": {
                    "enabled": false
                }
            });
            chart.addListener("clickSlice", function (event, index) {
                var posicion = event.dataItem.index;
                
                for (var x = 0; x < data.length; x++) {
                    if (x == posicion) {
                        buscarKardex(data[x].id)
                    }
                }
            });
        }
    })
}
function tortaClasificacionABC() {
    $.ajax({
        url: '/IndicadoresGraficos/tortaClasificacionABC',
        data: {},
        type: 'post',
        cache: false,
        success: function (data) {
            var datos = [];
            for (var i = 0; i < data.length; i++) {
                datos[i] = {
                    "title": data[i].id,
                    "valor": parseInt(data[i].cantidad),
                    "descripcion": data[i].cantidad
                }
            }

            var chart = AmCharts.makeChart("tortaClasificacion", {
                "type": "pie",
                "theme": "light",
                "dataProvider": datos,
                "valueField": "valor",
                "titleField": "title",
                "descriptionField": "descripcion",
                "outlineAlpha": 0.4,
                "depth3D": 15,
                "balloonText": "[[title]]<br><span style='font-size:14px'><b>[[description]]</b> ([[percents]]%)</span>",
                "angle": 30,
                "legend": {
                    "useGraphSettings": false,
                    "valueText": ""
                },
                "export": {
                    "enabled": false
                }
            });
            chart.addListener("clickSlice", function (event, index) {
                var posicion = event.dataItem.index;
                for (var x = 0; x < data.length; x++) {
                    if (x == posicion) {
                        buscarClasificados(data[x].id)
                    }
                }
            });
        }
    })
}



