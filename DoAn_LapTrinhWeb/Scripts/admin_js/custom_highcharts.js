$(document).ready(function () {
    $.getJSON("/Dashboard/Getbarcharts", function (data) {
        var Names = []
        var Qts = []
        for (var i = 0; i < data.length; i++) {
            Names.push(data[i].name);
            Qts.push(data[i].count);
        }

        Highcharts.chart('top_10_charts', {
            chart: {
                type: 'line'
            },
            title: {
                text: 'Thống kê top 10'
            },
            //subtitle: {
            //    text: 'Hàng tháng'
            //},
            xAxis: {
                categories: Names
            },
            yAxis: {
                title: {
                    text: 'số lượng: cái'
                }
            },
            plotOptions: {
                line: {
                    dataLabels: {
                        enabled: true
                    },
                    enableMouseTracking: false
                }
            },
            series: [{
                name: 'Sản phẩm bán chạy',
                data: Qts
            }]
        });
    });
});
