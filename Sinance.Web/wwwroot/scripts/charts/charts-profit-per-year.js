function CreateProfitPerYearGraph(elementId, data) {
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
            formatter: ChartCurrencyFormatter
        },
        series: [
            {
                data: data
            }
        ]
    });
}