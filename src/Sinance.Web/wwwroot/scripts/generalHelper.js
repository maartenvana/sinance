$(function () {
    $.validator.methods.range = function (value, element, param) {
        var globalizedValue = value.replace(",", ".");
        return this.optional(element) || (globalizedValue >= param[0] && globalizedValue <= param[1]);
    }

    $.validator.methods.number = function (value, element) {
        return this.optional(element) || /^-?(?:\d+|\d{1,3}(?:[\s\.,]\d{3})+)(?:[\.,]\d+)?$/.test(value);
    }

    bootbox.setDefaults({
        locale: "nl"
    });
});

function BindDecimalPointConvert() {
    $(".decimalPointConvert").keyup(function (eventObject) {
        if (eventObject.keyCode === 110) {
            var decimalPointConvertBox = $(this);
            decimalPointConvertBox.val(decimalPointConvertBox.val().replace(".", ","));
        }
    });
}
