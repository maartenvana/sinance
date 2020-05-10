function CreateProfitPerYearGraph(elementId, series) {
    $(elementId).highcharts({
        chart: {
            type: "column",
            zoomType: "x"
        },
        title: {
            floating: true,
            text: ""
        },
        yAxis: {
            title: {
                text: ""
            }
        },
        xAxis: {
            categories: ["Jan", "Feb", "Maa", "Apr", "Mei", "Jun", "Jul", "Aug", "Sep", "Okt", "Nov", "Dec"]
        },
        credits: {
            enabled: false
        },
        plotOptions: {
            column: {
                depth: 25
            }
        },
        legend: {
            enabled: false
        },
        tooltip: {
            shared: true,
            formatter: function () {
                var tooltipContent = this.x + "<br>";

                for (var i = 0; i < this.points.length; i++) {
                    tooltipContent += this.points[i].series.name + " <b>€ " + Highcharts.numberFormat(this.points[i].y, 2) + "</b><br>";
                }

                return tooltipContent;
            },
        },
        series
    });
}