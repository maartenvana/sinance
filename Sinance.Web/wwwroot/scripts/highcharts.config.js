$(function() {
    Highcharts.setOptions({
        lang: {
            decimalPoint: ",",
            thousandsSep: "."
        }
    });
});

function ChartCurrencyFormatter() {
        return "€ " + Highcharts.numberFormat(this.y, 2);
}