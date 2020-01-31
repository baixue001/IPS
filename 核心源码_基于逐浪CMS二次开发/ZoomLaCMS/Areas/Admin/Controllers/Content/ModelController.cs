using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ZoomLa.BLL;
using ZoomLa.BLL.Content;
using ZoomLa.Common;
using ZoomLa.Model.Content;
using ZoomLa.SQLDAL;
using ZoomLaCMS.Ctrl;
using ZoomLa.Model;
using ZoomLa.BLL.API;
using ZoomLa.SQLDAL.SQL;
using System.Text;

namespace ZoomLaCMS.Areas.Admin
{
    [Area("Admin")]
    [Route("/Admin/Content/[controller]/[action]")]
    [Authorize(Policy = "Admin")]
    public class ModelController : Ctrl_Admin
    {
        public int ModelID { get { return DataConvert.CLng(Request.Query["ModelID"]); } }
        public int ModelType { get { ViewBag.ModelType= DataConverter.CLng(Request.Query["ModelType"],1);return ViewBag.ModelType; } }
        B_Model bll = new B_Model();
        B_ModelField fieldBll = new B_ModelField();
        public IActionResult Index()
        {
            return View();
        }
        #region 模型
        public IActionResult ModelManage()
        {
            ViewBag.ModelType = ModelType;
            PageSetting setting = bll.SelPage(CPage, PSize, new Com_Filter()
            {
                type = ModelType.ToString(),
                skey = "",
                orderBy = GetParam("orderBy")
            });
            if (Request.IsAjax())
            {
                return PartialView("ModelManage_List", setting);
            }
            return View("ModelManage", setting);
        }
        public IActionResult ModelAdd()
        {
            M_ModelInfo model = new M_ModelInfo();
            if (Mid > 0)
            {
                model = bll.SelReturnModel(Mid);
            }
            else
            {
                model.ModelType = ModelType;
            }
            return View("ModelAdd", model);
        }
        [HttpPost]
        public IActionResult ModelAdd_Submit()
        {
            string modelName = RequestEx["TxtModelName"];
            if (string.IsNullOrEmpty(modelName)) { return WriteErr("模型名称不能为空"); }
            //----------------------------------------
            M_ModelInfo info = new M_ModelInfo();
            if (Mid < 1)
            {
                string modelTbName = (bll.GetTablePrefix(ModelType) + RequestEx["TxtTableName"]).Replace(" ", "");
                info.MultiFlag = true;
                info.TableName = modelTbName;
                if (bll.isExistTableName(modelTbName)) {return WriteErr("数据库表名已存在,请重新输入!"); }
            }
            else
            {
                info = bll.SelReturnModel(Mid);
                //info.TableName = TxtTableName.Text;
            }
            info.ModelName = modelName;
            info.ItemName = RequestEx["TxtItemName"];
            info.ItemUnit = RequestEx["TxtItemUnit"];
            info.ItemIcon = RequestEx["ItemIcon_T"];
            info.Description = RequestEx["TxtDescription"];
            info.ModelType = ModelType;
            info.SysModel = DataConvert.CLng(RequestEx["rblCopy"]);
            info.Islotsize = DataConverter.CLng(RequestEx["rblIslotsize"])==1;
            //info.Thumbnail = FileFactory.Checked ? "1" : "0";
            if (info.ModelID < 1)
            {
                info.FromModel = 0;
                switch (ModelType.ToString())
                {
                    case "3"://用户模型和黄页申请模型
                        bll.AddUserModel(info);
                        break;
                    case "6"://店铺申请模型
                        bll.AddStoreModel(info);
                        break;
                    case "7"://互动模型
                        {
                            B_Pub pll = new B_Pub();
                            M_Pub pubmodel = new M_Pub();
                            pubmodel.PubName = info.ModelName;
                            pubmodel.PubTableName = info.TableName;
                            pubmodel.PubType = DataConverter.CLng(GetParam("PubType"));
                            if (string.IsNullOrEmpty(info.ItemIcon)) { info.ItemIcon = "zi zi_comment"; }
                            pll.CreateModelInfo(pubmodel, info);
                        }
                        break;
                    case "8"://功能模型
                        bll.AddFunModel(info);
                        break;
                    case "12"://OA办公模型
                        bll.AddModel(info);//基于内容模型,增加自定义字段
                        break;
                    default://内容模型、商城模型、黄页内容模型、店铺商品模型
                        if (bll.IsExistModelName(info.ModelName)) { return WriteErr("模型名称[" + info.ModelName + "]已存在"); }
                        //CreateDataLabel(info);
                        bll.AddModel(info);
                        break;
                }
            }
            else
            {
                bll.UpdateByID(info);
            }
            return RedirectToAction("ModelManage");
        }
        public string Model_API()
        {
            string ids = DataConvert.CStr(RequestEx["ids"]);
            string action = Request.Query["action"];
            switch (action)
            {
                case "del":
                    {
                        bll.DelModel(DataConvert.CLng(ids));
                    }
                    break;
                case "copy":
                    {
                        M_ModelInfo model = bll.SelReturnModel(DataConvert.CLng(ids));
                        model.ModelID = 0;
                        model.ModelName = model.ModelName + "_拷贝";
                        model.TableName = model.TableName + "_" + function.GetRandomString(4);
                        bll.AddModel(model);
                        //return WriteOK("拷贝模型成功", "ModelManage?ModelType=" + ModelType);
                    }
                    break;
                case "template"://为模型绑定模板
                    {
                        int id = DataConvert.CLng(Request.Query["id"]);
                        string templateUrl = DataConvert.CStr(RequestEx["url"]);
                        bll.UpdateTemplate(templateUrl,id );
                    }
                    break;
                default:
                    break;
            }
            return Success.ToString();
        }
        #endregion
        #region 字段
        public IActionResult Field()
        {
            //负数为其它固定表的模型
            if (Mid == 0) { return WriteErr("没有指定模块ID"); }
            //this.TxtTemplate_hid.Value = modeli.ContentModule;
            DataTable tablelist = fieldBll.SelByModelID(Mid, true);
            switch (ModelType.ToString())
            {

                case "2"://商城模型
                    tablelist = SysField_Product(tablelist);
                    break;
                case "5"://店铺商品模型
                    tablelist = SysField_Product(tablelist);
                    break;
                case "6"://店铺申请模型  修改
                    tablelist = SysField_ApplyStore(tablelist);
                    break;
                case "7"://互动模型  修改
                    tablelist = SysField_Pub(tablelist);
                    break;
                case "8"://功能模型
                    tablelist = fieldBll.GetModelFieldListall(Mid);
                    break;
                case "9":
                    tablelist = SysField_UserBase();
                    break;
                case "1"://内容模型
                case "4"://黄页内容模型
                case "10"://黄页申请
                case "11"://CRM模型
                case "12"://OA办公模型
                case "13"://项目模型
                    tablelist = SysField_Project(tablelist);
                    break;
                default:
                    break;
            }
            M_ModelInfo modInfo = bll.SelReturnModel(Mid);
            if (ModelType == 9)
            {
                modInfo = new M_ModelInfo()
                {
                    ModelType = 9,
                    TableName = "ZL_UserBase",
                    ModelName = "用户注册字段",
                    ModelID = -1,
                };
            }
            ViewBag.model = modInfo;
            if (Request.IsAjax())
            {
                tablelist.DefaultView.Sort = GetParam("orderby").Replace("_", " ");
                return PartialView("Field_List", tablelist.DefaultView.ToTable());
            }
            else { return View(tablelist); }
        }
        //保存字段排序
        public string Order_Submit()
        {
            M_APIResult retMod=new M_APIResult(M_APIResult.Failed);
            string[] ordervalues =DataConvert.CStr(RequestEx["Order_Hid"]).Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string value in ordervalues)
            {
                if (string.IsNullOrWhiteSpace(value.Split('|')[0])) { continue; }
                int fid = Convert.ToInt32(value.Split('|')[0]);
                int orderid = Convert.ToInt32(value.Split('|')[1]);
                M_ModelField modfield = fieldBll.GetModelByID(Mid.ToString(), fid);
                modfield.OrderID = orderid;
                fieldBll.UpdateOrder(modfield);
            }
            retMod.retcode = M_APIResult.Success;
            return retMod.ToString();
        }
        public IActionResult FieldAdd()
        {
            M_ModelField fieldMod = new M_ModelField();
            M_ModelInfo modInfo = new M_ModelInfo();
            if (Mid > 0)
            {
                fieldMod = fieldBll.SelReturnModel(Mid);
                modInfo = bll.SelReturnModel(fieldMod.ModelID);
            }
            else
            {
                modInfo = bll.SelReturnModel(ModelID);
            }
            if (modInfo == null) { return WriteErr("未指定模型信息"); }
            ViewBag.modInfo = modInfo;
            return View("FieldAdd",fieldMod);
        }
        public IActionResult FieldAdd_Submit()
        {
            bool flag2 = DataConvert.CLng(RequestEx["IsSearchForm"])==1;
            string ftype = RequestEx["type_rad"];
            string defaultValue = "", strContent = "", tempFieldName = "", tempFieldType = "", fieldType = "nvarchar";
            switch (ftype)
            {
                case "TextType": //单行文本
                    strContent = "TitleSize=" + RequestEx["TitleSize"] + ",IsPassword=text" +
                        ",DefaultValue=" + RequestEx["TextType_DefaultValue"] + ",SelVideo=" + DataConvert.CLng(RequestEx["Text_SelVideo_Chk"]) + ",SelIcon=" + DataConvert.CLng(RequestEx["Text_SelIcon_Chk"]);
                    fieldType = "nvarchar";
                    break;
                case "MultipleTextType": //多行文本(不支持Html)
                    strContent = "Width=" + RequestEx["MultipleTextType_Width"] + ",Height=" + RequestEx["MultipleTextType_Height"] + ",SelUser=" + DataConvert.CLng(RequestEx["MText_SelUser_Chk"]) + ",Down=" + DataConvert.CLng(RequestEx["MText_Down_Chk"]);
                    fieldType = "ntext";
                    break;
                case "MultipleHtmlType": //多行文本(支持Html)
                    strContent = "Width=" + RequestEx["MultipleHtmlType_Width"] + ",Height=" + RequestEx["MultipleHtmlType_Height"] + ",IsEditor=" + RequestEx["IsEditor"] + ",AllowWord_Chk=" + DataConvert.CLng(RequestEx["AllowWord_Chk"]) + ",Topimg=" + DataConvert.CLng(RequestEx["Topimg_Chk"]);
                    fieldType = "ntext";
                    break;
                case "OptionType"://单选项
                    strContent = RequestEx["RadioType_Content"].Replace(" ", "").Replace("\n", "").Trim('\r').Replace("\r", "||");
                    strContent = RequestEx["RadioType_Type"] + "=" + strContent + ",Property=" + DataConvert.CLng(RequestEx["RadioType_Property"]) + ",Default=" + DataConvert.CLng(RequestEx["RadioType_Default"]) + "";
                    fieldType = "nvarchar";
                    defaultValue = RequestEx["RadioType_Default"];
                    break;
                case "ListBoxType": //多选项
                    strContent = RequestEx["ListBoxType_Content"].Replace(" ", "").Replace("\n", "").Trim('\r').Replace("\r", "||");
                    strContent = RequestEx["ListBoxType_Type"] + "=" + strContent;
                    fieldType = "ntext";
                    break;
                case "NumType"://数字
                    strContent = "TitleSize=" + RequestEx["NumberType_TitleSize"] + ",NumberType=" + RequestEx["NumberType_Style"] + ",DefaultValue=" + RequestEx["NumberType_DefaultValue"];
                    strContent = strContent + ",NumSearchType=,NumRangType=,NumSearchRang=,NumLenght=" + RequestEx["txtdecimal"];
                    int numstyle = DataConverter.CLng(RequestEx["NumberType_Style"]);
                    if (numstyle == 1) { fieldType = "int"; }
                    else if (numstyle == 2) { fieldType = "float"; }
                    else if (numstyle == 3) { fieldType = "money"; }
                    break;
                case "DateType": //日期时间
                    strContent = ConfigToStr("value|format|sdate|edate".Split('|'), new string[] {
                        RequestEx["date_value_rad"],
                        RequestEx["Date_Format_T"],
                        RequestEx["Date_SDate_T"],
                        RequestEx["Date_EDate_T"]
                    });
                    fieldType = "nvarchar";
                    break;
                case "PicType"://图片
                    strContent = "Water=" + DataConvert.CLng(RequestEx["Pic_Water_Chk"]) + ",MaxPicSize=" + RequestEx["MaxPicSize_T"] + ",PicFileExt=" + RequestEx["PicFileExt_T"] + ",SelUpfile=" + DataConvert.CLng(RequestEx["Pic_SelUpFile_Chk"])+ ",Compress=" + DataConvert.CLng(RequestEx["Pic_Compress_Chk"]);
                    fieldType = "nvarchar";
                    flag2 = false;
                    break;
                case "SqlType"://入库图片
                    strContent = "MaxPicSize=" + RequestEx["TxtMaxPicSize"] + ",FileExtArr=" + RequestEx["TxtPicSqlType"];
                    fieldType = "ntext";
                    tempFieldName = "FIT_" + RequestEx["Name_T"];
                    tempFieldType = "nvarchar";
                    flag2 = false;
                    break;
                case "SqlFile"://入库文件
                    strContent = "MaxPicSize=" + RequestEx["TxtMSqlFileSize"] + ",FileExtArr=" + RequestEx["TxtSqlFiletext"];
                    fieldType = "ntext";
                    tempFieldName = "FIT_" + RequestEx["Name_T"];
                    tempFieldType = "nvarchar";
                    flag2 = false;
                    break;
                case "MultiPicType": //多图片
                    strContent = "ChkThumb=" + DataConvert.CLng(RequestEx["ChkThumb"])+ ",ThumbField=" + RequestEx["TxtThumb"] + ",Warter=" + DataConvert.CLng(RequestEx["Pic_Water_Chk"]) + ",MaxPicSize=" + RequestEx["TxtPicSize"] + ",PicFileExt=" + RequestEx["TextImageType"];
                    //if (ChkThumb.Checked)
                    //{
                    //    tempFieldName = TxtThumb.Text;
                    //    tempFieldType = "nvarchar";
                    //}
                    fieldType = "ntext";
                    flag2 = false;
                    break;
                case "SmallFileType"://文件
                    strContent = "MaxFileSize=" + RequestEx["TxtMaxFileSizes"] + ",UploadFileExt=" + RequestEx["TxtUploadFileTypes"] + ",SelUpfile=" + DataConvert.CLng(RequestEx["rblSelUploadFile"]) + ",isbigfile=" + DataConvert.CLng(RequestEx["isBigFile"]);
                    fieldType = "nvarchar";
                    flag2 = false;
                    break;
                case "PullFileType"://下拉文件
                    strContent = RequestEx["PullFileText"];
                    fieldType = "nvarchar";
                    flag2 = false;
                    break;
                case "FileType"://多文件
                    strContent = "ChkFileSize=" +DataConvert.CLng(RequestEx["ChkFileSize"]) + ",FileSizeField=" + RequestEx["TxtFileSizeField"] + ",MaxFileSize=,UploadFileExt=zip";
                    //if (ChkFileSize.Checked)
                    //{
                    //    tempFieldName = TxtFileSizeField.Text;
                    //    tempFieldType = "nvarchar";
                    //}
                    fieldType = "ntext";
                    flag2 = false;
                    break;
                case "OperatingType": //运行平台
                    strContent = "TitleSize=" + RequestEx["OperatingType_TitleSize"] + ",OperatingList=" + DataConvert.CStr(RequestEx["TxtOperatingOption"]).Replace("\n", "|") + ",DefaultValue=" + RequestEx["OperatingType_DefaultValue"];
                    fieldType = "nvarchar";
                    break;
                case "SuperLinkType"://超链接
                    strContent = "TitleSize=" + RequestEx["SuperLinkType_TitleSize"] + ",DefaultValue=" + RequestEx["SuperLinkType_DefaultValue"];
                    fieldType = "nvarchar";
                    flag2 = false;
                    break;
                case "GradeOptionType":
                    strContent = "GradeCate=" + RequestEx["GradeOptionType_Cate"] + ",Direction=" + RequestEx["GradeOptionType_Direction"];
                    fieldType = "nvarchar";
                    break;
                case "ColorType"://颜色字段
                    strContent = "TitleSize=" + RequestEx["SuperLinkType_TitleSize"] + ",DefaultValue=" + RequestEx["ColorDefault"];
                    fieldType = "nvarchar";
                    flag2 = false;
                    break;
                case "Upload":// 在线浏览
                    strContent = "Warter=" + RequestEx["Pic_Water_Chk"] + ",MaxPicSize=,PicFileExt=";
                    fieldType = "nvarchar";
                    break;
                case "MapType"://地图类型
                    strContent = "source=" + RequestEx["MapSource_DP"] + ",type=" + RequestEx["MapType_Rad"];
                    fieldType = "ntext";
                    break;
                case "SwfFileUpload"://智能多文件上传
                    strContent = "MaxFileSize=" + RequestEx["TxtMaxFileSize1"] + ",UploadFileExt=" + RequestEx["TxtUploadFileType1"];
                    fieldType = "ntext";
                    break;
                case "MobileSMS": //手机短信
                    //strContent = "Width=" + MobileSMSType_Width.Text + ",Height=" + MobileSMSType_Height.Text + "";
                    fieldType = "ntext";
                    break;
                case "Charts":
                    fieldType = "int";
                    break;
                case "TableField"://库选字段
                    strContent = "TextField=" + RequestEx["TableField_Text"] + "," + "ValueField=" + RequestEx["TableField_Value"] + ","
                                + "WhereStr=" + RequestEx["Where_Text"] + "," + "FieldType=" + RequestEx["TableFieldType_Drop"];
                    //strContent = TableField_Text.Text.Trim()+","+TableField_Value.Text.Trim()+","+Where_Text.Text.Trim();
                    fieldType = "nvarchar";
                    break;
                case "Random"://随机数
                    {
                        string len = DataConverter.CLng(RequestEx["Random_Len_T"], 6).ToString();
                        strContent = "Type=" + RequestEx["Random_Type_Rad"] + ",Len=" + len;
                        fieldType = "nvarchar";
                    }
                    break;
                case "Images"://组图
                    {
                        strContent = "IsWater=" +DataConvert.CLng(RequestEx["IsWater_Images"]) + ",type=" + RequestEx["images_type_rad"];
                        strContent += ",maxcount=" + DataConverter.CLng(RequestEx["images_maxcount_t"], 5);
                        fieldType = "ntext";
                    }
                    break;
                case "CameraType"://拍照
                    //{
                    //    strContent = "cameraWidth=" + DataConverter.CLng(RequestEx["CameraWidth_T"]) + "," + "cameraHeight=" + DataConverter.CLng(CameraHeight_T.Text)
                    //                + ",imgWidth=" + DataConverter.CLng(CameraImgWidth_T.Text) + "," + "imgHeight=" + DataConverter.CLng(CameraImgHeight_T.Text);
                    //    fieldType = "nvarchar";
                    //}
                    break;
                default:
                    return WriteErr("保存异常，选定字段类型不匹配!!!");
            }
            M_ModelField fieldMod = new M_ModelField();
            M_ModelInfo modelMod = new M_ModelInfo();
            if (Mid != 0)
            {
                fieldMod = fieldBll.SelReturnModel(Mid);
                modelMod = bll.SelReturnModel(fieldMod.ModelID);
            }
            else
            {
                modelMod = bll.SelReturnModel(ModelID);
                fieldMod.ModelID = ModelID;
                fieldMod.FieldID = 0;
            }
            fieldMod.FieldName = RequestEx["Name_T"];
            fieldMod.FieldAlias = RequestEx["Alias_T"];
            fieldMod.FieldTips = RequestEx["Tips"];
            fieldMod.Description = RequestEx["Description"];
            fieldMod.IsNotNull = DataConvert.CLng(RequestEx["IsNotNull"]) == 1;
            fieldMod.IsSearchForm = flag2;
            fieldMod.FieldType = ftype;
            fieldMod.Content = strContent;
            fieldMod.IsShow = DataConvert.CLng(RequestEx["IsShow"]) == 1;
            fieldMod.IsCopy = DataConvert.CLng(RequestEx["rblCopy"]) == 1 ? 0 : -1;
            fieldMod.Islotsize = DataConvert.CLng(RequestEx["Islotsize"]) == 1;
            fieldMod.IsChain = DataConvert.CLng(RequestEx["IsChain"])==1;
            if (fieldMod.FieldID == 0)
            {
                fieldBll.AddModelField(modelMod, fieldMod);
                if (!string.IsNullOrEmpty(tempFieldName))
                {
                    fieldBll.AddField(modelMod.TableName, tempFieldName, tempFieldType, "");
                }
            }
            else
            {
                fieldBll.Update(fieldMod);
            }
            return WriteOK("操作成功", "Field?ID=" + ModelID + "&ModelType=" + ModelType);
        }
        private string ConfigToStr(string[] names, string[] values)
        {
            string result = "";
            for (int i = 0; i < names.Length; i++)
            {
                result += names[i] + "=" + values[i] + ",";
            }
            return result.TrimEnd(',');
        }
        public string Field_API()
        {
            string ids = DataConvert.CStr(RequestEx["ids"]);
            string action = Request.Query["action"];
            switch (action)
            {
                case "del":
                    {
                        if (!string.IsNullOrEmpty(ids))
                        {
                            fieldBll.DelByFieldID(ids);;
                        }
                    }
                    break;
                case "orderup":
                    {
                        M_ModelField curmod = fieldBll.SelReturnModel(DataConverter.CLng(RequestEx["curid"]));
                        curmod.OrderID = DataConverter.CLng(RequestEx["curorder"]);
                        M_ModelField tagmod = fieldBll.SelReturnModel(DataConverter.CLng(RequestEx["tagid"]));
                        tagmod.OrderID = DataConverter.CLng(RequestEx["tagorder"]);
                        fieldBll.UpdateOrder(curmod, tagmod);
                    }
                    break;
            }
            return Success.ToString();
        }
        private void SysField_AddToDT(DataTable dt, string field)
        {
            if (string.IsNullOrEmpty(field)) {  }
            DataRow dr = dt.NewRow();
            string alias = field.Split(':')[0];
            string name = field.Split(':')[1];
            dr["sys_type"] = 1;
            dr["FieldAlias"] = alias;
            dr["FieldName"] = name;
            dr["FieldType"] = "TextType";
            dr["OrderID"] = 0;
            dt.Rows.Add(dr);
        }
        private DataTable SysField_Product(DataTable dt)
        {
            dt.DefaultView.RowFilter = "sys_type=0";
            dt = dt.DefaultView.ToTable();
            string[] fields = ("商品编号:ProCode,品名:Proname,短名称:ShortProName,简介:Proinfo,商品详情:Procontent,缩略图:Thumbnails,添加时间:AddTime,更新时间:UpdateTime,单位:Prounit"
                + ",商品类型:ProClass,销售状态:Sales,商品属性:Properties,厂商:Producer,商标:Brand,缺货允许购买:Allowed,库存数量:Stock"
                + ",税率:Rate,推荐等级:Dengji,市场价:ShiPrice,零售价:LinPrice,会员价:MemberPrice,促销方案:Preset,优惠率:Recommend"
                + ",模板:ModeTemplate,添加人:AddUser,审核:Istrue,新品:Isnew,热销:Ishot,精品:Isbest,所属根节点:FirstNodeID,赠送积分:PointVal,店铺ID:UserShoID"
                + ",运费:FarePrice,").Split(',');
            foreach (string field in fields)
            {
                SysField_AddToDT(dt, field);
            }
            return dt;
        }
        private DataTable SysField_UserBase()
        {
            B_UserBaseField bbf = new B_UserBaseField();
            DataTable dt = bbf.Select_All();
            dt.Columns.Add(new DataColumn("sys_type",typeof(int)));
            dt.Columns.Add(new DataColumn("rblCopy", typeof(int)));
            dt.Columns.Add(new DataColumn("Islotsize", typeof(int)));
            dt.Columns.Add(new DataColumn("IsChain", typeof(int)));
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                dt.Rows[i]["sys_type"] = 0;
                //dt.Rows[i]["IsShow"] = 1;
                dt.Rows[i]["rblCopy"] = 0;
                dt.Rows[i]["Islotsize"] = 1;
                dt.Rows[i]["IsChain"] =1;
            }
            DataTable sysdt = DBCenter.DB.Field_List("ZL_UserBase");
            string sysStr = "";
            foreach (DataRow dr in sysdt.Rows)
            {
                sysStr += dr["Name"] + ":" + dr["Name"] + ",";
            }
            string[] fields = sysStr.TrimEnd(',').Split(',');
            foreach (string field in fields)
            {
                SysField_AddToDT(dt, field);
            }
            return dt;
        }
        private DataTable SysField_Pub(DataTable dt)
        {
            dt.DefaultView.RowFilter = "sys_type=0";
            dt = dt.DefaultView.ToTable();
            string[] fields = "互动编号:Pubupid,用户名:PubUserName,用户ID:PubUserID,内容ID:PubContentid,录入者:PubInputer,父级ID:Parentid,IP地址:PubIP,互动数量:Pubnum,开始:Pubstart,互动标题:PubTitle,互动内容:PubContent,添加时间:PubAddTime,评价:Optimal".Split(',');
            foreach (string field in fields)
            {
                SysField_AddToDT(dt, field);
            }
            return dt;
        }
        private DataTable SysField_ApplyStore(DataTable dt)
        {
            dt.DefaultView.RowFilter = "sys_type=0";
            dt = dt.DefaultView.ToTable();
            string[] fields = "用户ID:UserID,用户名:UserName,店铺名称:StoreName,店铺信誉:StoreCredit,店铺申请状态:StoreCommendState,店铺状态:StoreState,店铺风格ID:StoreStyleID,店铺模型ID:StoreModelID,店铺风格:StoreStyle,添加时间:AddTime".Split(',');
            foreach (string field in fields)
            {
                SysField_AddToDT(dt, field);
            }
            return dt;
        }
        private DataTable SysField_Project(DataTable dt)
        {
            dt.DefaultView.RowFilter = "sys_type=0";
            dt = dt.DefaultView.ToTable();
            string[] fields = "系统ID:ID,项目名称:Name,所属类型ID:TypeID,用户ID:UserID,甲方信息:UserInfo,项目需求:Requirements,价格:Price,启动时间:BeginTime,项目经理:Leader,技术负责人:WebCoding,审核状态:AuditStatus,项目状态:ProStatus,项目进程:Progress,完成时间:CompletionTime,评分:Rating,项目评价:Evaluation,申请时间:ApplicationTime".Split(',');
            foreach (string field in fields)
            {
                SysField_AddToDT(dt, field);
            }
            return dt;
        }
        #endregion


    }
}