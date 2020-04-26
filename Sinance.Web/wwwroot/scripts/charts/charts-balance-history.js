function CreateBalanceHistoryGraph(elementId, series) {
    $(elementId).highcharts({
        chart: {
            type: "area",
            zoomType: "x"
        },
        title: {
            text: ""
        },
        xAxis: {
            type: "datetime"
        },
        yAxis: {
            title: {
                text: ""
            }
        },
        credits: {
            enabled: false
        },
        legend: {
            enabled: false
        },
        tooltip: {
            shared: true,
            formatter: function () {
                var tooltipContent = new Date(this.x).toLocaleDateString() + "<br>";

                for (var i = 0; i < this.points.length; i++) {
                    tooltipContent += this.points[i].series.name + " <b>€ " + Highcharts.numberFormat(this.points[i].y, 2) + "</b><br>";
                }

                if (this.points.length > 1) {
                    tooltipContent += "Total <b>€ " + Highcharts.numberFormat(this.points[0].total, 2) + "</b>";
                }

                return tooltipContent;
            },
        },
        plotOptions: {
            area: {
                stacking: 'normal',
                lineColor: '#666666',
                lineWidth: 1,
                marker: {
                    lineWidth: 1,
                    lineColor: '#666666'
                }
            }
        },
        series
    });
}