$(function () {

    $('#datepicker').datepicker();
    $("#datepicker").on("changeDate", function (event) {
        $("#my_hidden_input").val(
                $("#datepicker").datepicker('getFormattedDate')
        )
    });

    $('#datapicker2').datepicker();
    $('#datepicker .input-group.date').datepicker({});
    $('#datepicker .input-daterange').datepicker({});

    $("#demo1").TouchSpin({
        min: 0,
        max: 100,
        step: 0.1,
        decimals: 2,
        boostat: 5,
        maxboostedstep: 10,
    });

    $("#demo2").TouchSpin({
        verticalbuttons: true
    });

    $("#demo3").TouchSpin({
        postfix: '%'
    });

    $("#demo4").TouchSpin({
        postfix: "a button",
        postfix_extraclass: "btn btn-default"
    });

    $(".js-source-states").select2();
    $(".js-source-states-2").select2();

    //turn to inline mode
    $.fn.editable.defaults.mode = 'inline';

    //defaults
    $.fn.editable.defaults.url = '#';

    //editables
    $('#username').editable({
        url: '#',
        type: 'text',
        pk: 1,
        name: 'username',
        title: 'Enter username'
    });

    $('#firstname').editable({
        validate: function (value) {
            if ($.trim(value) == '') return 'This field is required';
        }
    });

    $('#sex').editable({
        prepend: "not selected",
        source: [
            { value: 1, text: 'Male' },
            { value: 2, text: 'Female' }
        ],
        display: function (value, sourceData) {
            var colors = { "": "gray", 1: "green", 2: "blue" },
                    elem = $.grep(sourceData, function (o) { return o.value == value; });

            if (elem.length) {
                $(this).text(elem[0].text).css("color", colors[value]);
            } else {
                $(this).empty();
            }
        }
    });

    $('#dob').editable();

    $('#event').editable({
        placement: 'right',
        combodate: {
            firstItem: 'name'
        }
    });

    $('#comments').editable({
        showbuttons: 'bottom'
    });

    $('#fruits').editable({
        pk: 1,
        limit: 3,
        source: [
            { value: 1, text: 'banana' },
            { value: 2, text: 'peach' },
            { value: 3, text: 'apple' },
            { value: 4, text: 'watermelon' },
            { value: 5, text: 'orange' }
        ]
    });

    $('#user .editable').on('hidden', function (e, reason) {
        if (reason === 'save' || reason === 'nochange') {
            var $next = $(this).closest('tr').next().find('.editable');
            if ($('#autoopen').is(':checked')) {
                setTimeout(function () {
                    $next.editable('show');
                }, 300);
            } else {
                $next.focus();
            }
        }
    });

    // ClockPicker
    $('.clockpicker').clockpicker({ autoclose: true });

    // DateTimePicker
    $('#datetimepicker1').datetimepicker();

});