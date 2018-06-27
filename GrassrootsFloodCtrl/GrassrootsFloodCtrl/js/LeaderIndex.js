$(function () {
    load();
    //loadflood();
})
function load() {
    $.ajax({
        url: '/api/sumLeader/getLeaderSumForm',
        type: 'get',
        data: { adcd: '330000000000000', year: globalYear },
        dataType: 'json',
        beforeSend: function () {
            $(".selected-box").append("<a style=\" border:0; background:none;\" id=\"staticLoading\" href=\"javascript:void(0);\"><img src=\"/Content/ComprehensiveApp/images/loading.gif\" width=\"22\" height=\"22\" /></a>");
        },
        success: function (result) {
            if (result.code = 200) {
                //行政区划
                $("#adcountry").text(result.administrativeDivision.countryCount);
                $("#adtown").text(result.administrativeDivision.townCount);
                $("#advillage").text(result.administrativeDivision.villageCount);
                //防汛任务
                $("#floodSumCount").text(result.administrativeDivision.villageCount);
                $("#floodheavy").text(result.floodCount);
                $("#floodlight").text(result.administrativeDivision.villageCount - result.floodCount);
                loadflood(result.administrativeDivision.villageCount - result.floodCount, result.floodCount);
                //基层防汛
                $("#jcfxcountry").text(result.persionLiableCount.countryCount);
                $("#jcfxtown").text(result.persionLiableCount.townCount);
                $("#jcfxvillage").text(result.persionLiableCount.villageCount);
                $("#jcfxsum").text(result.persionLiableCount.countryCount + result.persionLiableCount.townCount + result.persionLiableCount.villageCount);
                //形势图
                $("#pic").text(result.villagePicCount);
                //app注册人数
                $("#appuser").text(result.appUserCount);
                //风险隐患
                var pointcount = result.hiddenRiskPoint.dangerousCount + result.hiddenRiskPoint.torrentialFloodCount + result.hiddenRiskPoint.geologyCount;
                pointcount = pointcount + result.hiddenRiskPoint.lowLyingCount + result.hiddenRiskPoint.poolCount + result.hiddenRiskPoint.dikeCount;
                pointcount = pointcount + result.hiddenRiskPoint.seawallCount + result.hiddenRiskPoint.otherCount;
                $("#dangerousCount").text(result.hiddenRiskPoint.dangerousCount);
                $("#torrentialFloodCount").text(result.hiddenRiskPoint.torrentialFloodCount);
                $("#geologyCount").text(result.hiddenRiskPoint.geologyCount);
                $("#lowLyingCount").text(result.hiddenRiskPoint.lowLyingCount);
                $("#poolCount").text(result.hiddenRiskPoint.poolCount);
                $("#dikeCount").text(result.hiddenRiskPoint.dikeCount);
                $("#seawallCount").text(result.hiddenRiskPoint.seawallCount);
                $("#otherCount").text(result.hiddenRiskPoint.otherCount);
                $("#pointpersoncount").text(result.hiddenRiskPoint.pointPersonCount);
                $("#pointcount").text(pointcount);
                //应急响应
                $("#managementCountry").text(result.management.countryCount);
                $("#topost").text(result.management.managementCount);
                $("#duty").text(result.management.dutyCount);
                loadmanagement(result.management.managementCount, result.management.dutyCount);
                //近30天注册情况展示
                loadregistration(result.messageRegistration);
            }
        },
        error: function () {
            $(".selected-box").find("a#staticLoading").remove();
        }
    });
}
//防汛任务
function loadflood(light, heavy) {
    // 基于准备好的dom，初始化echarts实例
    var myChart = echarts.init(document.getElementById('floodTask'));

    var option = {
        color: ['#ef3052','#4c8bea'],
        tooltip: {
            trigger: 'item',
            formatter: "{a} <br/>{b}: {c} ({d}%)"
        },
        legend: {
            orient: 'left',
            align: 'left',
            x: 'right',
            bottom: '40%',
            selectedMode:false,
            data: [{
                name: '防汛任务重',
                textStyle: {
                    fontSize: '14'
                },
                icon: 'image://../css/Leader/images/legend-r.png'//格式为'image://+icon文件地址'，其中image::后的//不能省略
                },{
                    name: '防汛任务轻',
                    textStyle: {
                        fontSize: '14'
                    },
                   icon: 'image://../css/Leader/images/legend-b2.png'//格式为'image://+icon文件地址'，其中image::后的//不能省略
               }]
        },
        series: [
            {
                name: '防汛任务',
                type: 'pie',
                radius: ['50%', '70%'],
                center: ['30%', '50%'],
                avoidLabelOverlap: false,
                label: {
                    normal: {
                        show: false,
                        position: 'center'
                    },
                    emphasis: {
                        show: true,
                        textStyle: {
                            fontSize: '15',
                            fontWeight: 'bold'
                        }
                    }
                },
                labelLine: {
                    normal: {
                        show: false
                    }
                },
                data: [
                    { value: heavy, name: '防汛任务重' },
                    { value: light, name: '防汛任务轻' }
                ]
            }
        ]
    };
    myChart.setOption(option);
    //myChart.on('click', function (params) {
    //    alert(params);
    //});
}
//应急响应
function loadmanagement(sum,duty) {
    // 基于准备好的dom，初始化echarts实例
    var myChart = echarts.init(document.getElementById('management'));

    var option = {
        color: [ '#4dd76a','#4c8bea'],
        tooltip: {
            trigger: 'item',
            formatter: "{a} <br/>{b}: {c} ({d}%)"
        },
        legend: {
            orient: 'left',
            align: 'left',
            selectedMode: false,
            x: 'right',
            bottom: '40%',
            data: [{
                name: '已履职人数',
                textStyle:{
                    fontSize:'14'
                },
                icon: 'image://../css/Leader/images/legend-g.png'//格式为'image://+icon文件地址'，其中image::后的//不能省略
            }, {
                name: '应到岗人数',
                textStyle: {
                    fontSize: '14'
                },
                icon: 'image://../css/Leader/images/legend-b.png'//格式为'image://+icon文件地址'，其中image::后的//不能省略
            }]
        },
        series: [
            {
                name: '应急管理',
                type: 'pie',
                radius: ['50%', '70%'],
                center: ['30%', '50%'],
                avoidLabelOverlap: false,
                label: {
                    normal: {
                        show: false,
                        position: 'center'
                    },
                    emphasis: {
                        show: true,
                        textStyle: {
                            fontSize: '15',
                            fontWeight: 'bold'
                        }
                    }
                },
                labelLine: {
                    normal: {
                        show: false
                    }
                },
                data: [
                    { value: duty, name: '已履职人数' },
                    { value: sum, name: '应到岗人数' }
                ]
            }
        ]
    };
    myChart.setOption(option);
}
//近30天注册情况展示
function loadregistration(data) {
    // 基于准备好的dom，初始化echarts实例
    var myChart = echarts.init(document.getElementById('messageHistogram'));
    var option = {
        color: ['#47c8f2', '#3ce09b'],
        title: {
            text: '近30天责任人动态更新情况',
            textStyle: {
                fontStyle: 'normal',
                fontWeight: 'normal'
            }
        },
        tooltip: {
            trigger: 'axis'
        },
        calculable: true,
        grid: {  
            left: '0%',  
            right: '10',  
            bottom: '40%',  
            containLabel: true  
        },
        xAxis: [
            {
                type: 'category',
                data: data.cityList,
                axisLabel: {
                    interval: 0,//横轴信息全部显示   
                }
            }
        ],
        yAxis: [
            {
                type: 'value'
            }
        ],
        series: [
            {
                name: '责任人总数',
                type: 'bar',
                data: data.allList
            },
            {
                name: '更新人数',
                type: 'bar',
                data: data.regList
            }
        ]
    };
    myChart.setOption(option);
}

//$("#managementCountry").click(function(){
//    window.location.href = "/Supervise/LeaderAppWarnList";
//})