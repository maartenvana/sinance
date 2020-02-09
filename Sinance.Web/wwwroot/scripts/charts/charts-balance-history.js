function CreateBalanceHistoryGraph(elementId, data) {
    $(elementId).highcharts({
        chart: {
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
            formatter: ChartCurrencyFormatter
        },
        plotOptions: {
            area: {
                fillColor: {
                    linearGradient: { x1: 0, y1: 0, x2: 0, y2: 1 },
                    stops: [
                        [0, Highcharts.getOptions().colors[0]],
                        [1, Highcharts.Color(Highcharts.getOptions().colors[0]).setOpacity(0).get("rgba")]
                    ]
                },
                marker: {
                    radius: 2
                },
                lineWidth: 1,
                states: {
                    hover: {
                        lineWidth: 1
                    }
                },
                threshold: null
            }
        },
        series: [
            {
                type: "area",
                name: "euro",
                data: data
            }
        ]
    });
}