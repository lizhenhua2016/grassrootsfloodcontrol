var _infowin = null;
var webapps={
	DisplayFeatures:function(jsons){
	//����ʽ
	var lineStyle = {
		color: "#e6000b",
		weight: 3
	};

	//Σ��������ʽ    
	var polygonStyle = {
		color: "#FFFF00",//#DA26BE �߿�ɫ
		weight: 2,
		fill: true,
		fillColor: "#FFFF00",//#DA26BE ���ɫ
		fillOpacity: 0.3
	};

	//��Σ��������ʽ    
	var gpolygonStyle = {
		color: "#FF7F00",//#DA26BE �߿�ɫ
		weight: 2.5,
		fill: true,
		fillColor: "#FF7F00",//#DA26BE ���ɫ
		fillOpacity: 0.5
	};

	//����Σ��������ʽ    
	var jgpolygonStyle = {
		color: "#FF0000",//#DA26BE �߿�ɫ
		weight: 3,
		fill: true,
		fillColor: "#FF0000",//#DA26BE ���ɫ
		fillOpacity: 0.5
	};

	//��ͼ��ע
	for (var i = 0; i < jsons.length; i++) {

		var feaJson = jsons[i];
		var coords = feaJson.coordinates;
		var gcType = feaJson.gcType;
		var layer = null; 
		var zoom = _mapApp.getZoom();
		switch (gcType) {
			case "shd":
				layer = _shd; //ɽ��Σ�յ�
				break;
			case "dzzhd":
				layer = _dzzhd; //�����ֺ���
				break;
			case "weifang":
				layer = _weifang; //Σ������
				break;
			case "dwynyq":
				layer=_dwynyq;//���������׵�
				break;
			case "wdst":
				layer=_wdst;//�ݶ�ɽ��
				break;
			case "dfhtxgxf":
				layer=_dfhtxgxf;//�̷������ն�
				break;
			case "anzidian"://���ֳ���
				layer=_anzidian;
				break;
			case "wzck"://���ʲֿ�
				layer=_wzck;
				break;
		}
		
		var imgType = ".png";
		//if (gcType == "0") {
		//    gcType = "animate1";
		//    imgType = ".gif";
		//} else if (gcType == "1") {
		//    gcType = "animate";
		//    imgType = ".gif";
		//}
		if (feaJson.featureType == "POINT") {
			//����ʽ
			var pointStyle = L.divIcon({
				iconSize: [8, 8],
				iconAnchor: [8, 8],
				className: 'leaflet-divlabel',
				html: "<img src='/Content/ComprehensiveApp/images/" + gcType + imgType + "'" + " style='width:28px;height:18px' align='middle'/>"
			});
			//��
			var x = parseFloat(coords.split(",")[0]);
			var y = parseFloat(coords.split(",")[1]);
			
			var pointFeature = new L.marker([y,x], { icon: pointStyle, riseOnHover: true })
			pointFeature.options.sm_capital = feaJson.featureName;
			pointFeature.options.sm_id = feaJson.featureID;
			pointFeature.options.sm_sid = gcType;
			pointFeature.bindLabel(feaJson.featureName);
			layer.addLayer(pointFeature);

			//if (zoom >= 12)
			//    pointFeature.bindLabel(feaJson.featureName).showLabel();

		} 
		else if (feaJson.featureType == "LINESTRING") {
			//��
			var pointArray = new Array();
			var pointCoords = coords.split(",");
			for (var j = 0; j < pointCoords.length; j++) {
				var x = parseFloat(pointCoords[j].split(" ")[0]);
				var y = parseFloat(pointCoords[j].split(" ")[1]);
				pointArray.push(new L.LatLng(y, x));
			}
			var lineFeature = new L.Polyline(pointArray, lineStyle);
			lineFeature.options.sm_capital = feaJson.featureName;
			lineFeature.options.sm_id = feaJson.featureID;
			lineFeature.options.sm_sid = gcType;
			lineFeature.bindLabel(feaJson.featureName, { direction: 'auto' });
			//if (zoom >= 12)
			//    pointFeature.bindLabel(feaJson.featureName).showLabel();
			layer.addLayer(lineFeature);

		} 
		else if (feaJson.featureType == "MULTILINESTRING") {

			//����
			var lineArray = new Array();
			var lineCoords = coords.split("),(");
			for (var j = 0; j < lineCoords.length; j++) {
				var coord = lineCoords[j];
				var pointArray = new Array();
				var pointCoords = coord.split(",");
				for (var m = 0; m < pointCoords.length; m++) {
					var x = parseFloat(pointCoords[m].split(" ")[0]);
					var y = parseFloat(pointCoords[m].split(" ")[1]);
					pointArray.push(new L.LatLng(y, x));
				}
				var lineFeature = new L.Polyline(pointArray, lineStyle);
				lineFeature.options.sm_capital = feaJson.featureName;
				lineFeature.options.sm_id = feaJson.featureID;
				lineFeature.options.sm_sid = gcType;
				lineFeature.bindLabel(feaJson.featureName, { direction: 'auto' });
				layer.addLayer(lineFeature);
			}
		} 
		else if (feaJson.featureType == "POLYGON") {

			//��
			var pArray = new Array();

			var pointArray = new Array();
			var pointCoords = coords.split(",");

			var x0 = parseFloat(pointCoords[0].split(" ")[0]);
			var y0 = parseFloat(pointCoords[0].split(" ")[1]);
			var startIndex = 0;

			for (var j = 0; j < pointCoords.length; j++) {
				var x = parseFloat(pointCoords[j].split(" ")[0]);
				var y = parseFloat(pointCoords[j].split(" ")[1]);

				pointArray.push(new L.LatLng(y, x));

				if (x == x0 && y == y0 && j > startIndex) {
					pArray.push(pointArray);
					pointArray = new Array();

					startIndex = j + 1;
					if (j + 1 < pointCoords.length) {
						x0 = parseFloat(pointCoords[j + 1].split(" ")[0]);
						y0 = parseFloat(pointCoords[j + 1].split(" ")[1]);
					}
				}
			}
			var polygonFeature;
			if (gcType == "gwxq" || gcType == "dcgwxq")
				polygonFeature = new L.Polygon(pArray, gpolygonStyle);
			else if (gcType == "jgwxq" || gcType == "dcjgwxq")
				polygonFeature = new L.Polygon(pArray, jgpolygonStyle);
			else
				polygonFeature = new L.Polygon(pArray, polygonStyle);

			polygonFeature.options.sm_capital = feaJson.featureName;
			polygonFeature.options.sm_id = feaJson.featureID;
			polygonFeature.options.sm_sid = gcType;
			if (gcType == "yjwxq" || gcType == "gwxq" || gcType == "jgwxq") {
			} else
				polygonFeature.bindLabel(feaJson.featureName);
			layer.addLayer(polygonFeature);
		} else if (feaJson.featureType == "MULTIPOLYGON") {
			//����
			var polygonCoords = coords.split("),(");
			var polygonArray = new Array();
			for (var j = 0; j < polygonCoords.length; j++) {
				var coord = polygonCoords[j];
				//��
				var pArray = new Array();

				var pointArray = new Array();
				var pointCoords = coord.split(",");

				var x0 = parseFloat(pointCoords[0].split(" ")[0]);
				var y0 = parseFloat(pointCoords[0].split(" ")[1]);
				var startIndex = 0;

				for (var m = 0; m < pointCoords.length; m++) {
					var x = parseFloat(pointCoords[m].split(" ")[0]);
					var y = parseFloat(pointCoords[m].split(" ")[1]);

					pointArray.push(new L.LatLng(y, x));

					if (x == x0 && y == y0 && m > startIndex) {
						pArray.push(pointArray);
						pointArray = new Array();
						startIndex = m + 1;
						if (m + 1 < pointCoords.length) {
							x0 = parseFloat(pointCoords[m + 1].split(" ")[0]);
							y0 = parseFloat(pointCoords[m + 1].split(" ")[1]);
						}
					}
				}

				var polygonFeature;
				if (gcType == "gwxq")
					polygonFeature = new L.Polygon(pArray, gpolygonStyle);
				else if (gcType == "jgwxq")
					polygonFeature = new L.Polygon(pArray, jgpolygonStyle);
				else
					polygonFeature = new L.Polygon(pArray, polygonStyle);

				polygonFeature.options.sm_capital = feaJson.featureName;
				polygonFeature.options.sm_id = feaJson.featureID;
				polygonFeature.options.sm_sid = gcType;
				if (gcType == "yjwxq" || gcType == "gwxq" || gcType == "jgwxq") {
				} else
					polygonFeature.bindLabel(feaJson.featureName);
				layer.addLayer(polygonFeature);
			}
		}
	}
  },
  //��ʾ���ڴ�����
  openWindow:function(name, type, pnt){
	switch(type){
		case "dzzhd":
			$("#FrameTuCengInfo .townname").html(name);
			$("#FrameTuCengInfo").show(500);
			$("#FrameTuCengInfo_dzzhd").show();
			$("#FrameTuCengInfo_weifang").hide();
			$("#FrameTuCengInfo_anzidian").hide();
		break;
		case "weifang":
			$("#FrameTuCengInfo .townname").html(name);
			$("#FrameTuCengInfo").show(500);
			$("#FrameTuCengInfo_dzzhd").hide();
			$("#FrameTuCengInfo_anzidian").hide();
			$("#FrameTuCengInfo_weifang").show();
		break;
		case "anzidian":
			$("#FrameTuCengInfo .townname").html(name);
			$("#FrameTuCengInfo").show(500);
			$("#FrameTuCengInfo_anzidian").show();
			$("#FrameTuCengInfo_weifang").hide();
			$("#FrameTuCengInfo_dzzhd").hide();
		break;
	}
  },
  //�򿪴���
  openInfoWin:function(id, name, type, pnt) {
	//�رմ���
	webapps.closeInfoWin();

	var url = "";
	var w = 400;
	var h = 220;
	switch(type){
		case "dzzhd":
			url = "showWindowInfo_dzzhd.html";
		break;
		case "weifang":
			url = "showWindowInfo_weifang.html";
		break;
		case "anzidian":
			url = "showWindowInfo_anzidian.html";
		break;
	}
	var text = "<iframe name='frame' marginwidth='0' marginheight='0' frameborder='0' src='" + url + "' width='100%' height='220px' scrolling='no' style='zIndex:999;'></iframe>";
	_infowin = L.popup({ minWidth: w, minHeight: h,zIndex:999 })
	.setLatLng(pnt)
	.setContent(text)
	.openOn(_mapApp);
},
//�رմ���
closeInfoWin:function() {
	if (_infowin != null) _mapApp.closePopup();
}

}