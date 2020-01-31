var canvas = null;
var fabHelper = {
    list: [],
    textUrl: "/Plugins/Third/Logo/CreateImg.aspx?action=text&",
    backGround:"black"
};
//{itemcb:function(){}}
fabHelper.init = function (json,cfg) {
    if (json == "") { return; }
    json = HtmlUtil.removeUrlRoot(json);
    if(!cfg){cfg={};}
    canvas.loadFromJSON(json, function () {
        var list = canvas.getObjects();
        for (var i = 0; i < list.length; i++) {
            var src = fabHelper.tool.getImageUrl(list[i]);
            if (list[i].spriteImages) { list[i].cType = "sprite"; }
            else if (src.toLowerCase().indexOf("createimg.aspx") > -1) { list[i].cType = "text"; }
            else { list[i].cType = "img"; }
            fabHelper.bindEvent(list[i]);
            if (cfg.itemcb) { cfg.itemcb(list[i],i,list); }
        }
        fabHelper.setBackGround(fabHelper.backGround);
    });

}
fabHelper.tool = {};
//从URL中获取model信息
fabHelper.tool.getInfoFromUrl = function (url, name) {
    var str = url.split(name + "=")[1];
    str = str.split("&")[0];
    return JSON.parse(decodeURIComponent(str));
}
//获取图片URL地址
fabHelper.tool.getImageUrl = function (fabObj) {
    if (fabObj._element) { return fabObj._element.src; }
    else { return "";}
}
//计算text位置,使其居中(相对原位置)
fabHelper.tool.calcTextPos = function (textObj, pos) {
    if (textObj.width >= pos.width) {
        textObj.set({ "left": pos.left });
    }
    else if (textObj.width < pos.width) {
        var left = pos.left + ((pos.width - textObj.width) / 2);
        textObj.set({ "left": left });
    }
}
fabHelper.newMod = function (type) {
    switch (type) {
        case "sprite":
            return { width: 250, height: 200, value: "/Plugins/Third/Logo/test2.practice.png" };
        case "text":
        default:
            return { bkcolor: "#000000", color: "#ffffff", text: "", size: 12, family: "", addon: "" };
    }
}
//{type:"",value:"",model:{}}
fabHelper.add = function (cfg) {
    //兼容
    if (typeof (cfg) == "string") { cfg = { "type": cfg, "value": "" }; }
    switch (cfg.type) {
        case "img":
            fabric.Image.fromURL("/UploadFiles/nopic.gif", function (image) {
                callBack(image);
            });
            break;
        case "text":
            var model = fabHelper.newMod();
            model.text = "YOUR TEXT HERE";
            fabric.Image.fromURL(fabHelper.getTextUrl(model), function (image) {
                callBack(image);
            });
            break;
        case "sprite":
            {
                var model = cfg.model ? cfg.model : fabHelper.newMod(cfg.type);
                console.log(model);
                fabric.Sprite.fromURL(model.value, function (sprite) {
                    //canvas.add(sprite);
                    callBack(sprite);
                    setTimeout(function () {
                        sprite.play();
                    }, fabric.util.getRandomInt(1, 10) * 100);
                    (function render() {
                        canvas.renderAll();
                        fabric.util.requestAnimFrame(render);
                    })();
                }, { top: model.top, left: model.left, width: model.width, height: model.height });
            }
            break;
    }
    //---------------------
    function callBack(fabObj) {
        fabObj.cType = cfg.type;
        fabObj.set({ left: (canvas.width - fabObj.width) / 2, top: 50, });
        fabHelper.bindEvent(fabObj);
        canvas.add(fabObj);
    }
}
fabHelper.bindEvent = function (fabObj) {
    fabObj.on("selected", function (e) {
        var fabObj = canvas.getActiveObject();
        $(".item_tr").hide();
        $("." + fabObj.cType + "_tr").show();
        //----------更新控件数据
        var imgUrl = $(fabObj._element.outerHTML).attr("src");
        imgUrl = StrHelper.getUrlVPath(imgUrl);
        switch (fabObj.cType) {
            case "img":
                $("#PicUrl_T").val(imgUrl);
                break;
            case "text":
                {
                    var model = fabHelper.tool.getInfoFromUrl(imgUrl, "model");
                    $("#text_text").val(model.text);
                    $("#text_size").val(model.size);
                    $("#text_color").val(model.color);
                }
                break;
            case "sprite":
                {
                    $("#sp_width").val(fabObj.width);
                    $("#sp_height").val(fabObj.height);
                    $("#sp_value").val(imgUrl);
                }
                break;
        }
    });
}
//更新当前选定的对象
fabHelper.update = function () {
    var fabObj = canvas.getActiveObject();
    if (fabObj == null) { return; }
    switch (fabObj.cType) {
        case "img":
            {
                var oldWid = fabObj.width * fabObj.scaleX;
                fabObj.setSrc($("#PicUrl_T").val(), function (image) {
                    //fabObj.set({ width: 50, height: 50 });//cut
                    image.scale(oldWid / image.width);
                    canvas.renderAll();
                });
            }
            break;
        case "text":
            {
                var model = fabHelper.newMod(fabObj.cType);
                model.text = $("#text_text").val();
                model.color = $("#text_color").val();
                model.size = $("#text_size").val();
                if ($("#font_ul li.active").length > 0) {
                    model.family = $("#font_ul li.active").data("font");
                }
                fabObj.setSrc(fabHelper.getTextUrl(model),
                    function (textObj) {
                        //如果宽度大于原宽,则不做处理
                        if (textObj.pos) {
                            fabHelper.tool.calcTextPos(textObj, textObj.pos);
                        }
                        canvas.renderAll();
                    });
            }
            break;
        case "sprite"://删除后在原位置新建
            {
                var cfg = {
                    model: {
                        left: fabObj.left, top: fabObj.top,
                        width: Convert.ToInt($("#sp_width").val(), 100),
                        height: Convert.ToInt($("#sp_height").val(), 100),
                        value: $("#sp_value").val(),
                    },
                    type: "sprite"
                };
                fabHelper.del();
                fabHelper.add(cfg);
            }
            break;
    }
}
fabHelper.del = function () {
    var fabObj = canvas.getActiveObject();
    if (!fabObj) { return; }
    canvas.remove(fabObj);
}
fabHelper.getTextUrl = function (model) {
    var url = fabHelper.textUrl + "model=" + encodeURIComponent(JSON.stringify(model));
    return url;
}
//设置背景色或背景图片
fabHelper.setBackGround = function (color) {
    canvas.setBackgroundImage(null);
    switch (color) {
        case "transparent":
            fabric.Object.prototype.cornerColor = "#5bc0de";
            canvas.setBackgroundColor({
                source: "/Plugins/Third/Logo/assets/grid.jpg",
                repeat: 'repeat',
            }, canvas.renderAll.bind(canvas));
            break;
        case "black":
            fabric.Object.prototype.cornerColor = "white";
            canvas.setBackgroundColor("#000", canvas.renderAll.bind(canvas));
            break;
        case "white":
            fabric.Object.prototype.cornerColor = "#5bc0de";
            canvas.setBackgroundColor("#fff", canvas.renderAll.bind(canvas));
            break;
        default://背景图片
            console.log(color);
            //canvas.setBackgroundImage(color, function () {
            //    canvas.renderAll(canvas);
            //});
            fabric.Image.fromURL(color, function (img) {
                img.scaleX = canvas.width / img.width;
                img.scaleY = canvas.height / img.height;
                canvas.setBackgroundImage(img, canvas.renderAll.bind(canvas));
            });
            break;
    }
}