﻿function CreateProfitHistoryPerMonthGraph(elementId, series) {
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
            type: "datetime"
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
                var tooltipContent = moment(this.x).format("MMMM YYYY") + "<br>";

                for (var i = 0; i < this.points.length; i++) {
                    tooltipContent += this.points[i].series.name + " <b>€ " + Highcharts.numberFormat(this.points[i].y, 2) + "</b><br>";
                }

                return tooltipContent;
            },
        },
        series
    });
}