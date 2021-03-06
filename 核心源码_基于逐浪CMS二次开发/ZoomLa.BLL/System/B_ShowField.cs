﻿using System;
using System.IO;
using System.Collections;
using System.Data;
using System.Text;
using System.Linq;
using ZoomLa.Common;
using ZoomLa.Components;
using ZoomLa.SQLDAL;
using ZoomLa.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ZoomLa.BLL.Helper;
using System.Text.RegularExpressions;
using System.Web;
using System.Collections.Generic;
using ZoomLa.BLL.Content;

namespace ZoomLa.BLL
{
    public class B_ShowField
    {
        public FieldConfig config = new FieldConfig();
        private B_Admin badmin = new B_Admin();
        private B_Content_WordChain wordBll = new B_Content_WordChain();
        private string ManageDir = SiteConfig.SiteOption.ManageDir;
        private string reqTlp = "<span style=\"color:red;\">*</span>";
        private string updir = (SiteConfig.SiteOption.UploadDir).TrimEnd('/') + "/";
        //------------------------------------
        public string ShowStyleField(FieldModel model)
        {
            config = model.config;
            config.CurFieldName = model.Name;
            return ShowStyleField(model.Alias, model.Name, model.IsNotNull, model.Type, model.Content, model.Description, model.ModelID, model.NodeID, model.dr);
        }
        public string ShowStyleField(string Alias, string Name, bool IsNotNull, string Type, string Content, string Description, int ModelID, int NodeID, DataRow dr, bool ischain = false)
        {
            try
            {
                switch (Type)
                {
                    case "TextType":
                        return this.GetTextType(Alias, Name, IsNotNull, Content, Description, dr);
                    case "MultipleTextType":
                        return this.GetMultipleTextType(Alias, Name, IsNotNull, Content, Description, dr, ischain);
                    case "ListBoxType":
                        return this.GetListBoxType(Alias, Name, IsNotNull, Content, Description, dr);
                    case "DateType":
                        return this.GetDateType(Alias, Name, IsNotNull, Content, Description, dr);
                    case "MultipleHtmlType":
                        return this.GetMultipleHtmlType(Alias, Name, IsNotNull, Content, Description, NodeID, dr, ModelID, ischain);
                    case "PicType":
                        return this.GetPicType(Alias, Name, IsNotNull, Content, Description, ModelID, NodeID, dr);
                    case "SqlType"://图片入库与文件入库合并
                    case "SqlFile":
                        return this.GetSqlFile(Alias, Name, IsNotNull, Content, Description, ModelID, NodeID, dr);
                    case "OptionType":
                        return this.GetRadioType(Alias, Name, IsNotNull, Content, Description, dr);
                    case "FileType":
                        return this.GetFileType(Alias, Name, IsNotNull, Content, Description, ModelID, NodeID, dr);
                    case "SmallFileType":
                        return this.GetSmallFileType(Alias, Name, IsNotNull, Content, Description, ModelID, NodeID, dr);
                    case "PullFileType":
                        return this.GetPullFileType(Alias, Name, IsNotNull, Content, Description, dr);
                    case "NumType":
                        return this.GetNumberType(Alias, Name, IsNotNull, Content, Description, dr);
                    case "MultiPicType":
                        return this.GetMultPicType(Alias, Name, IsNotNull, Content, Description, ModelID, NodeID, dr);
                    case "OperatingType":
                        return this.GetOperatingType(Alias, Name, IsNotNull, Content, Description, dr);
                    case "SuperLinkType":
                        return this.GetSuperLinkType(Alias, Name, IsNotNull, Content, Description, dr);
                    case "GradeOptionType":
                        return this.GetGradeOption(Alias, Name, IsNotNull, Content, Description, dr);
                    case "ColorType"://颜色代码 
                        return this.GetColorType(Alias, Name, IsNotNull, Content, Description, dr);
                    case "DoubleDateType":
                        return this.GetDoubleDateType(Alias, Name, IsNotNull, Description, dr);
                    case "Upload":
                        return this.GetSwf(Alias, Name, IsNotNull, Content, Description, ModelID, NodeID, dr);
                    case "MapType":
                        return this.GetMap(Alias, Name, IsNotNull, Content, Description, ModelID, NodeID, dr);
                    case "Charts":
                        return this.Charts(Alias, Name, IsNotNull, Content, Description, ModelID, NodeID, dr);
                    case "SwfFileUpload":
                        return this.SwfFileUpload(Alias, Name, IsNotNull, Content, Description, ModelID, NodeID, dr);
                    case "RemoteFile":
                        return this.RemoteFile(Alias, Name, IsNotNull, Content, Description, ModelID, NodeID, dr);
                    case "TableField":
                        return this.TableField(Alias, Name, IsNotNull, Content, Description, ModelID, NodeID, dr);
                    case "Random":
                        return this.RandomCode(Alias, Name, IsNotNull, Content, Description, ModelID, NodeID, dr);
                    case "Images":
                        return this.ImagesCode(Alias, Name, IsNotNull, Content, Description, ModelID, NodeID, dr);
                    case "CameraType":
                        return this.CameraCode(Alias, Name, IsNotNull, Content, Description, ModelID, NodeID, dr);
                    case "autothumb":
                        return AutoThumbCode(Alias, Name, IsNotNull, Content, Description, ModelID, NodeID, dr);
                    case "api_qq_mvs":
                        return GetTencentMvs(Alias, Name, IsNotNull, Content, Description, ModelID, NodeID, dr);
                    default:
                        return ShowError(Alias, Name, "字段类型[" + Type + "]不存在");
                }
            }
            catch (Exception ex) { return ShowError(Alias, Name, "字段内容:" + Content + "|||报错:" + ex.Message); }
        }
        // 单行文本
        public string GetTextType(string Alias, string Name, bool IsNotNull, string Content, string Description, DataRow dr)
        {
            if (config.mode == ModelConfig.SMode.PreView) { return PreViewMode(Alias, dr, Name); }
            FieldModel model = new FieldModel(Content);
            model.config = config;
            string def = model.TlpReplace(model.GetValue("DefaultValue"));
            string id = "txt_" + Name;
            string selHtml = "";
            //if (config.source == ModelConfig.SType.Admin) { selHtml = model.GetHtml(id, def); }
            selHtml = model.GetHtml(id, def);
            string required = IsNotNull ? reqTlp : "", disabled = "",
                value = dr == null ? def.Split('|')[0] : dr[Name].ToString();
            string selvideo = "";//选择视频
            if (config.Disable)
            {
                disabled = "disabled='disabled'"; selHtml = "";
            }
            else
            {
                if (model.GetValBool("SelVideo"))
                {
                    selvideo = "<input type=\"button\" onclick=\"ShowSelVideo('" + Name + "');\" class='btn btn-info' value='选择视频' />";
                }
                if (model.GetValBool("SelIcon"))
                {
                    selvideo += "<input type=\"button\" onclick=\"ShowSelIcon('" + Name + "');\" class='btn btn-info' value='选择图标' /><span id='sp_" + Name + "' style='margin-left:5px;font-size:1.5em;'></span>";
                }
            }
            //else if (FieldConfig.SType.OAForm.Equals(config.source)&& !model.AuthCheck(model.GetValue("EditAuth")))
            //{
            //    disabled = "disabled='disabled'"; selHtml = "";
            //}
            StringBuilder builder = new StringBuilder();
            builder.Append("<tr class=\"fd_tr fd_tr_texttype\"><td class=\"fd_td_l\"><span>" + Alias + "：</span></td><td class=\"fd_td_r\">");
            builder.Append("<input " + disabled + " type='" + model.GetValue("isPassword") + "' name='" + id + "' class='form-control m715-50 fd_text' id='" + id + "' size='" + model.GetValue("TitleSize") + "' value='" + value + "'> ");
            builder.Append(required + selHtml + selvideo + Description);
            builder.Append("</td></tr>");
            return builder.ToString();
        }
        // 多行文本不支持Html
        public string GetMultipleTextType(string Alias, string Name, bool IsNotNull, string Content, string Description, DataRow dr, bool ischain = false)
        {
            if (config.mode == ModelConfig.SMode.PreView) { return PreViewMode(Alias, dr, Name); }
            FieldModel model = new FieldModel(Content);
            string required = IsNotNull ? reqTlp : "", value = "", disabled = "", seluser = "";
            if (config.Disable)
            {
                disabled = "disabled='disabled'";
            }
            else
            {
                if (model.GetValBool("SelUser"))//允许选择会员
                {
                    seluser = "<input type=\"button\" onclick=\"ShowSelUser('txt_" + Name + "');\" class='btn btn-info' value='选择用户' />";
                }
                if (model.GetValBool("Down"))//下载字段
                {
                    JObject obj = new JObject();
                    obj.Add(new JProperty("field", "down"));
                    obj.Add(new JProperty("nodeid", 0));
                    obj.Add(new JProperty("inputid", "txt_" + Name));
                    obj.Add(new JProperty("uploaddir", updir));
                    obj.Add(new JProperty("iswater", "0"));
                    string json = JsonConvert.SerializeObject(obj);
                    seluser += "<input type=\"button\" onclick=\"ShowAddDown('txt_" + Name + "');\" class='btn btn-info' value='自定义下载' />";
                    seluser += "<input type='button' onclick='UpFileDiag(" + json + ");' class='btn btn-info' value='上传文件'/>";
                }
            }
            int width = model.GetValInt("width"), height = model.GetValInt("height");
            if (dr != null)
            {
                value = dr[Name].ToString();
            }
            string tlp = "<tr class=\"fd_tr fd_tr_multipletexttype\"><td class=\"fd_td_l\"><span>" + Alias + "：</span></td><td class=\"fd_td_r\">";
            tlp += "<textarea " + disabled + " name=\"txt_" + Name + "\" id=\"txt_" + Name + "\" class=\"form-control m715-50 fd_textarea\"  style='height:" + height + "px;width:" + width + "px;max-width:100%;'>" + value + "</textarea>" + required + " " + Description;
            tlp += seluser;
            tlp += "</td></tr>";
            return tlp;
        }
        // 多行文本支持Html
        public string GetMultipleHtmlType(string Alias, string Name, bool IsNotNull, string Content, string Description, int NodeID, DataRow dr, int ModelID, bool ischain = false)
        {
            Name = Name.ToLower();//ueditor大小写敏感,大写不会影响使用,但holder会为空导致报错
            if (config.mode == ModelConfig.SMode.PreView) { return PreViewMode(Alias, dr, Name); }
            //Width=715,Height=400,IsEditor=2,AllowWord_Chk=True
            //编辑器类型(简洁,简洁增强,标准,完全)
            const string min = "1", minex = "4", mid = "2", copmlete = "3";
            FieldModel model = new FieldModel(Content);
            string content = "", node = "&NodeID=" + NodeID;
            int width = DataConvert.CLng(model.GetValue("Width"));
            int height = DataConvert.CLng(model.GetValue("Height"));
            content = dr == null ? "" : dr[Name].ToString();
            //if (ischain) { content = wordBll.ReturnBack(content); }
            string iseditor = model.GetValue("IsEditor");//编辑器类型
            string exhtml = "";//左边栏,提供对ueditor的扩展支持
            exhtml += "<div class='btn-group-vertical cmdgroup' role='group' data-id='txt_" + Name + "'>"
                   + "<button type='button' class='btn btn-default cmdbtn' data-cmd='disable' title='保护内容'><i class='zi zi_pencilalt'></i></button>"
                   + "<button type='button' class='btn btn-default cmdbtn' data-cmd='enable' title='取消保护' style='display:none;'><i style='color:#ccc;' class='zi zi_pencilalt'></i></button>"
                   + "<button type='button' class='btn btn-default cmdbtn' data-cmd='hide' title='收缩内容'><i class='zi zi_eye'></i></button>"
                   + "<button type='button' class='btn btn-default cmdbtn' data-cmd='show' title='展开内容' style='display:none;'><i class='fa fa-eye-slash'></i></button>"
                   + "<button type='button' class='btn btn-default cmdbtn' data-cmd='clear' title='清空内容'><i class='zi zi_zi_trashalt'></i></button></div>";
            StringBuilder builder = new StringBuilder();
            builder.Append("<tr class=\"fd_tr fd_tr_multiplehtmltype\"><td class=\"fd_td_l\">" + Alias + " " + exhtml + "</td><td class=\"fd_td_r\">");
            //-----------------
            switch (iseditor)
            {
                case min:
                    #region 简洁型编辑器
                    builder.AppendLine("<textarea class='ueditor' cols=\"80\" id=\"txt_" + Name + "\" style=\"width:" + width + "px;height:" + height + "px;\" name=\"txt_" + Name + "\" rows=\"10\">" + HttpUtility.HtmlEncode(content) + "</textarea>");
                    builder.AppendLine("<script id=\"headscript\" type=\"text/javascript\">");
                    builder.AppendLine(@"UE.getEditor('txt_" + Name + "', {" + BLLCommon.ueditorMin + "});");
                    builder.AppendLine(@"</script>");
                    builder.Append("</td></tr>");
                    return builder.ToString();
                #endregion
                case minex:
                    #region 简单型编辑器
                    builder.AppendLine("<textarea class='ueditor' cols=\"80\" id=\"txt_" + Name + "\" style=\"width:" + width + "px;height:" + height + "px;\" name=\"txt_" + Name + "\" rows=\"10\">" + HttpUtility.HtmlEncode(content) + "</textarea>");
                    builder.AppendLine(@"<script type=""text/javascript"">");
                    builder.AppendLine(@"UE.getEditor('txt_" + Name + "', {" + BLLCommon.ueditorMinEx + "" + "});");
                    builder.AppendLine(@"</script>");
                    builder.Append("</td></tr>");
                    return builder.ToString();
                #endregion
                case mid:
                    #region 标准型编辑器
                    builder.AppendLine("<textarea class='ueditor' cols=\"80\" id=\"txt_" + Name + "\" style=\"width:" + width + "px;height:" + height + "px;\" name=\"txt_" + Name + "\" rows=\"10\">" + HttpUtility.HtmlEncode(content) + "</textarea>");
                    builder.AppendLine(@"<script type=""text/javascript"">");
                    builder.AppendLine(@"UE.getEditor('txt_" + Name + "', {" + BLLCommon.ueditorMid + "});");
                    builder.AppendLine(@"</script>");
                    builder.Append("</td></tr>");
                    return builder.ToString();
                #endregion
                case copmlete:
                    #region 增强型编辑器
                    builder.AppendLine("<textarea class='ueditor' cols=\"80\" id=\"txt_" + Name + "\" style=\"width:" + width + "px;height:" + height + "px;\" name=\"txt_" + Name + "\" rows=\"10\">" + HttpUtility.HtmlEncode(content) + "</textarea>");
                    builder.AppendLine(@"<script type=""text/javascript"">");
                    builder.AppendLine(@"UE.getEditor('txt_" + Name + "');");
                    builder.AppendLine(@"</script>");
                    builder.Append("</td></tr>");
                    return builder.ToString();
                #endregion
                default:
                    return "";
            }

        }
        // 数字
        public string GetNumberType(string Alias, string Name, bool IsNotNull, string Content, string Description, DataRow dr)
        {
            if (config.mode == ModelConfig.SMode.PreView) { return PreViewMode(Alias, dr, Name); }
            //TitleSize=35,NumberType=1,DefaultValue=50,NumSearchType=0,NumRangType=1,NumSearchRang=0-100,NumLenght=0
            StringBuilder builder = new StringBuilder();
            builder.Append("<tr class=\"fd_tr fd_tr_numtype\"><td class=\"fd_td_l\"><span>" + Alias + "：</span></td><td class=\"fd_td_r\">");
            FieldModel model = new FieldModel(Content);
            int NumberType = model.GetValInt("NumberType");
            int NumLenght = model.GetValInt("NumLenght");
            int size = model.GetValInt("TitleSize");
            string value = model.GetValue("DefaultValue");
            if (dr != null) { value = DataConvert.CStr(dr[Name]); }
            value = DataConvert.CDouble(value).ToString("F" + NumLenght);
            switch (NumberType)
            {
                case 1://整数
                    builder.Append("<input type=\"txt\" class=\"form-control text_md\" name=\"txt_" + Name + "\" id=\"txt_" + Name + "\" size=\"" + size + "\" value=\"" + value + "\" onkeyup=\"this.value=this.value.replace(/\\D/g,'');\" /> " + Description);
                    break;
                case 2://小数
                    builder.Append("<input type=\"txt\" class=\"form-control text_md\" name=\"txt_" + Name + "\" id=\"txt_" + Name + "\" size=\"" + size + "\" value=\"" + value + "\" onkeyup=\"value=value.replace(/[^\\-?\\d.]/g,'')\" /> " + Description);
                    break;
                case 3://货币
                    builder.Append("<input type=\"txt\" class=\"form-control text_md\" name=\"txt_" + Name + "\" id=\"txt_" + Name + "\" size=\"" + size + "\" value=\"" + value + "\" onkeyup=\"value=value.replace(/[^\\-?\\d.]/g,'')\" />" + Description);
                    break;
            }
            builder.Append("</td></tr>");
            return builder.ToString();
        }
        // 单个图片，带修改
        public string GetPicType(string Alias, string Name, bool IsNotNull, string Content, string Description, int ModelID, int NodeID, DataRow dr)
        {
            if (config.mode == ModelConfig.SMode.PreView) { return PreViewImg(Alias, dr, Name); }
            string isReq = IsNotNull ? reqTlp : "";
            string value = dr == null ? "" : dr[Name].ToString();
            string imgSrc = string.IsNullOrEmpty(value) ? "" : updir + value;
            FieldModel fieldMod = new FieldModel(Content);
            string btns = "<span class='btn btn-group'>";
            btns += "<a href='javascript:;' onclick='GetCutpic(\"" + Name + "\",\"" + function.GetRandomString(5) + "\")' class='btn btn-info'><i class='fa fa-pencil '></i> 图片修改</a>";
            if (fieldMod.GetValBool("SelUPFile"))
            { btns += "<a href='javascript:;' onclick=\"SelectUppic({\'name\':\'" + Name + "\'})\" title='在线选取' class='btn btn-info'><i class='fa fa-picture-o'></i> 在线选取</a>"; }
            btns += "</span>";

            StringBuilder builder = new StringBuilder();
            builder.Append("<tr class=\"fd_tr fd_tr_pictype\"><td class=\"fd_td_l\"><span>" + Alias + "：</span></td><td class=\"fd_td_r\">");
            builder.Append("<div>");
            builder.Append("<input type=\"text\" class=\"form-control m715-50\" value=\"" + value + "\" name=\"txt_" + Name + "\" id=\"txt_" + Name + "\">" + isReq + " " + Description + "");
            builder.Append("<img id='Img_" + Name + "' src='" + imgSrc + "' class='preview_img' width='150' height='100' onclick='PopImage(\"popImg\",\"txt_" + Name + "\",500, 400);' onerror=\"this.src='/UploadFiles/nopic.svg';\" title='点击查看大图' >");
            builder.Append(btns);
            builder.Append("</div>");//专用于添加文章等处
            builder.Append("<div><iframe id=\"Upload_" + Name + "\" src=\"/Common/ContentFile?ModelID=" + ModelID + "&FieldName=" + Name + "&NodeId=" + NodeID + "&S=" + (int)config.source + "\"  style='border:none;width:100%;height:30px;' scrolling=\"no\"></iframe></div>");

            // 添加弹出层
            builder.AppendLine("<div id='popImg' style='display:none;'>");
            builder.AppendLine("<div class='ptitle'><span onclick='javascript:document.getElementById(\"popImg\").style.visibility = \"hidden\";' title='Close' class='ViewPicClo' > &#x00D7; </span>预览图片  </div>");
            builder.AppendLine("<div id='divImage'><img id='imgView' src='#' alt='预览的图片' onload='AutoSetSize(this, 490, 360);' /></div>");
            builder.AppendLine("</div>");
            return builder.ToString();
        }
        // 双时间字段
        public string GetDoubleDateType(string Alias, string Name, bool IsNotNull, string Description, DataRow dr)
        {
            if (config.mode == ModelConfig.SMode.PreView) { return PreViewMode(Alias, dr, Name); }
            string str2 = "";
            StringBuilder builder = new StringBuilder();
            builder.Append("<link rel='stylesheet' type='text/css' href='/App_Themes/AdminDefaultTheme/doutime.css'>");
            string timeNow = DateTime.Now.ToShortDateString().Replace('/', '-');
            if (IsNotNull)
            {
                str2 = "<font color=\"red\">*</font>";
            }
            if (dr == null || (dr != null && dr[Name].ToString().Split('|').Length < 2))
            {
                builder.Append("<tr class='fd_tr fd_tr_doubledatetype'><td class=\"fd_td_l\"><span>" + Alias + "：</span></td><td class=\"fd_td_r\">");
                builder.Append("<div id='hotelDiv'>");
                builder.Append("<input type='text' value='yyyy-mm-dd' id='CheckInDate' name='txt" + Name + "' /> 至 ");
                builder.Append("<input type='text' value='yyyy-mm-dd' id='CheckOutDate' name='txt_" + Name + "'/> </div>");
                builder.Append("<input id='IntervallCheckInAndChekOut' value='1' type='hidden' />");
                builder.Append("<input id='CheckOut' type='hidden' />");
                builder.Append("<script type='text/javascript' src='/JS/systemall.js'></script>");
                builder.Append("<script type='text/javascript' src='/JS/homecn.js'></script>");
                builder.Append("<div id='serverdate' value='" + timeNow + "'></div>");
                builder.Append("<div id='m_contentend'></div>");
                builder.Append("</td></tr>");

                str2 += builder.ToString();
            }
            else
            {
                string[] valu = dr[Name].ToString().Split('|');
                if (valu != null && valu.Length > 1)
                {
                    builder.Append("<tr><td class=\"fd_td_l\"><span>" + Alias + "：</span></td><td>");
                    builder.Append("<div id='hotelDiv'>");
                    builder.Append("<input type='text' value='yyyy-mm-dd ' id='CheckInDate' name='txt" + Name + "' /> 至 ");
                    builder.Append("<input type='text' value='yyyy-mm-dd' id='CheckOutDate' name='txt_" + Name + "'/> </div>");
                    builder.Append("<input id='IntervallCheckInAndChekOut' value='1' type='hidden' />");
                    builder.Append("<input id='CheckOut' type='hidden' value='" + valu[1].Trim().Replace('/', '-') + "' />");
                    builder.Append("<script type='text/javascript' src='/JS/systemall.js'></script>");
                    builder.Append("<script type='text/javascript' src='/JS/homecn.js'></script>");
                    builder.Append("<div id='serverdate' value='" + valu[0].Trim().Replace('/', '-') + "'></div>");
                    builder.Append("<div id='m_contentend'></div>");
                    builder.Append("</td></tr>");
                    str2 += builder.ToString();
                }
            }
            return str2;
        }
        // 日期时间,0:默认空,1:自抽取
        public string GetDateType(string Alias, string Name, bool IsNotNull, string Content, string Description, DataRow dr)
        {
            if (config.mode == ModelConfig.SMode.PreView) { return PreViewMode(Alias, dr, Name); }
            FieldModel model = new FieldModel(Content);
            StringBuilder builder = new StringBuilder();
            string required = IsNotNull ? reqTlp : "",
                value = model.GetValue("Type").Equals("1") ? DateTime.Now.ToString() : "", disabled = "";
            if (config.Disable)
            {
                disabled = "disabled='disabled'";
            }
            //else if (ModelConfig.SType.OAForm.Equals(config.source) && !model.AuthCheck(model.GetValue("EditAuth")))
            //{
            //    disabled = "disabled='disabled'";
            //}
            if (dr != null)
            {
                value = dr[Name].ToString();
            }
            builder.Append("<tr class=\"fd_tr fd_tr_datetype\"><td class=\"fd_td_l\"><span>" + Alias + "：</span></td><td class=\"fd_td_r\">");
            builder.Append("<input " + disabled + " type=\"text\" class=\"form-control m715-50\" name=\"txt_" + Name + "\" size=\"18\" onclick=\"WdatePicker({ dateFmt: 'yyyy-MM-dd HH:mm:ss' });\" value=\"" + value + "\"> " + required + " " + Description + "");
            builder.Append("</td></tr>");
            return builder.ToString();
        }
        // 文件上传
        public string GetFileType(string Alias, string Name, bool IsNotNull, string Content, string Description, int ModelID, int NodeID, DataRow dr)
        {
            if (config.mode == ModelConfig.SMode.PreView) { return PreViewMode(Alias, dr, Name); }
            string str2 = "";
            string[] strArray = Content.Split(new char[] { ',' });
            bool ChkFileSize = DataConverter.CBool(strArray[0].Split(new char[] { '=' })[1]);
            string FileSizeField = strArray[1].Split(new char[] { '=' })[1];
            StringBuilder builder = new StringBuilder();
            string str = "";
            if (ChkFileSize && FileSizeField != "")
            {
                str = "<tr class=\"fd_tr fd_tr_filetype\"><td class=\"fd_td_l\"><span>文件大小</span></td><td class=\"fd_td_r\">";
                if (dr == null)
                {
                    str = str + "<input name=\"txt_" + FileSizeField + "\" id=\"txt_" + FileSizeField + "\" type=\"text\" class=\"inputtext form-control m715-50\" />K</td></tr>";
                }
                else
                {
                    str = str + "<input name=\"txt_" + FileSizeField + "\" id=\"txt_" + FileSizeField + "\" type=\"text\" class=\"inputtext form-xontrol\" value=\"" + dr["" + FileSizeField + ""].ToString() + "\" />K</td></tr>";
                }
            }
            str = str + "<tr class=\"fd_tr fd_tr_filetype\"><td class=\"fd_td_l\"><span>上传文件</span></td><td class=\"fd_td_r\">";
            str = str + "<iframe id=\"Upload_" + Name + "\" src=\"/Common/FileUpload?Showtype=0&ModelID=" + ModelID + "&FieldName=" + Name + "&NodeId=" + NodeID + "\" marginheight=\"0\" marginwidth=\"0\" frameborder=\"0\" width=\"100%\" height=\"30\" scrolling=\"no\"></iframe></td></tr>";
            if (IsNotNull)
            {
                str2 = "<font color=\"red\">*</font>";
            }
            builder.Append("<tr><td class=\"fd_td_l\"><span>" + Alias + "：</span></td><td>");
            builder.Append("<table width=\"100%\" border=\"0\" cellpadding=\"0\" cellspacing=\"0\">");
            builder.Append("    <tr>");
            builder.Append("<td style=\"width: 400px\">");
            if (dr != null)
            {
                if (!string.IsNullOrEmpty(dr[Name].ToString()))
                {
                    builder.Append("<input type=\"hidden\" name=\"txt_" + Name + "\" id=\"txt_" + Name + "\" value=\"" + dr[Name].ToString() + "\">");
                }
                else
                {
                    builder.Append("<input type=\"hidden\" name=\"txt_" + Name + "\" id=\"txt_" + Name + "\">");
                }
            }
            else
            {
                builder.Append("<input type=\"hidden\" name=\"txt_" + Name + "\" id=\"txt_" + Name + "\">");
            }

            builder.Append("<select name=\"sel_" + Name + "\" class=\"form-control m715-50\" id=\"sel_" + Name + "\" style=\"width: 400px; height: 100px\" size=\"2\">");
            if (dr != null)
            {
                if (!string.IsNullOrEmpty(dr[Name].ToString()))
                {
                    string[] strValue = dr[Name].ToString().Split(new char[] { '$' });
                    for (int i = 0; i < strValue.Length; i++)
                    {
                        builder.Append("<option value=\"" + strValue[i] + "\">" + strValue[i] + "</option>");
                    }
                }
            }
            builder.Append("</select> " + str2 + "</td><td>");
            builder.Append("<div class=\"btn-group-vertical\">");
            builder.Append("<input type=\"button\" class=\"button\" onclick=\"SelectFile('sel_" + Name + "','txt_" + Name + "')\" value=\"从上传文件中选择\">");
            builder.Append("<input type=\"button\" class=\"button\" onclick=\"AddSoftUrl('sel_" + Name + "','txt_" + Name + "')\" value=\"添加外部地址\">");
            builder.Append("<input type=\"button\" class=\"button\" value=\"修改当前地址\" onclick=\"return ModifyPhotoUrl('sel_" + Name + "','txt_" + Name + "');\" />");
            builder.Append("<input type=\"button\" class=\"button\" value=\"删除当前地址\" onclick=\"DelPhotoUrl('sel_" + Name + "','txt_" + Name + "');\" />");
            builder.Append("</div>");
            builder.Append("</td></tr></table></td></tr>");

            builder.Append(str);
            return builder.ToString();
        }
        // 在线浏览
        public string GetSwf(string Alias, string Name, bool IsNotNull, string Content, string Description, int ModelID, int NodeID, DataRow dr)
        {
            if (config.mode == ModelConfig.SMode.PreView) { return PreViewMode(Alias, dr, Name); }
            string str2 = "";
            if (IsNotNull)
            {
                str2 = "<font color=\"red\">*</font>";
            }
            StringBuilder builder = new StringBuilder();
            builder.Append("<tr class=\"fd_tr fd_tr_upload\"><td class=\"fd_td_l\"><span>" + Alias + "：</span></td><td class=\"fd_td_r\">");
            builder.Append("<table border='0' width='100%'>");
            builder.Append("<tr>");
            builder.Append("<td>");
            if (dr == null)
            {
                builder.Append("<input type=\"text\" class=\"form-control m715-50\" name=\"txt_" + Name + "\" id=\"txt_" + Name + "\" size=\"35\">" + str2 + " " + Description + "");
                builder.Append("</td></tr>");
                builder.Append("<tr><td><iframe id=\"Upload_" + Name + "\" src=\"/Common/FileUpload?ModelID=" + ModelID + "&FieldName=" + Name + "&NodeId=" + NodeID + "\" marginheight=\"0\" marginwidth=\"0\" frameborder=\"0\" width=\"100%\" height=\"30\" scrolling=\"no\"></iframe></td></tr></table>");
                builder.Append("</td></tr>");
                return builder.ToString();
            }
            builder.Append("<input type=\"text\" class=\"form-control m715-50\" name=\"txt_" + Name + "\" id=\"txt_" + Name + "\" size=\"35\" value=\"" + dr[Name].ToString() + "\">" + str2 + " " + Description + "");
            builder.Append("</td></tr>");
            builder.Append("<tr><td><iframe id=\"Upload_" + Name + "\" src=\"/Common/FileUpload?ModelID=" + ModelID + "&FieldName=" + Name + "&NodeId=" + NodeID + "\" marginheight=\"0\" marginwidth=\"0\" frameborder=\"0\" width=\"100%\" height=\"30\" scrolling=\"no\"></iframe></td></tr></table>");
            builder.Append("</td></tr>");
            return builder.ToString();
        }
        // 多选项
        public string GetListBoxType(string Alias, string Name, bool IsNotNull, string Content, string Description, DataRow dr)
        {
            if (config.mode == ModelConfig.SMode.PreView) { return PreViewMode(Alias, dr, Name); }
            //1=M|L|S(不使用该结构)
            //1=黑色|Black||红色|red 
            //3=2008款|2008款$0||2009款|2009款$0||2010款|2010款$0||2011款|2011款$0||2012款|2012款$0
            StringBuilder builder = new StringBuilder();
            int fieldType = Convert.ToInt32(Content.Substring(0, 1));//checkbox,listbox,多填
            string fieldValue = (dr == null ? "" : dr[Name].ToString());//value是以,号切割的多值
            string fieldContent = Content.Split('=')[1].Split(',')[0];
            string[] options = Regex.Split(fieldContent, Regex.Escape("||"));
            string[] values = fieldValue.Split(",".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            Dictionary<string, string> dic = new Dictionary<string, string>();
            for (int i = 0; i < options.Length; i++)
            {
                string option = options[i];
                if (string.IsNullOrEmpty(option)) { continue; }
                if (option.Contains("$0")) { option = Regex.Split(option, Regex.Escape("$0"))[0]; }
                string key = option.Split('|')[0];
                string value = "";
                if (option.Split('|').Length < 2) { value = key; }
                else { value = option.Split('|')[1]; }
                if (!dic.ContainsKey(key)) { dic.Add(key, value); }
            }
            builder.Append("<tr class=\"fd_tr fd_tr_listboxtype\" ><td class=\"fd_td_l\">" + Alias + "：</td><td class=\"fd_td_r\">");
            switch (fieldType)
            {
                case 1:
                    {
                        string tlp = "<label style='margin-right:5px;margin-bottom:3px;'><input type='checkbox' id='txt_" + Name + "_{2}' name='txt_" + Name + "' value='{0}' {3}><span  style='position:relative;top:3px;'>{1}</span></label>";
                        int index = 0;
                        foreach (var item in dic)
                        {
                            string chkd = (values.Length > 0 && values.Contains(item.Value)) ? " checked='checked' " : "";
                            builder.Append(string.Format(tlp, item.Value, item.Key, index, chkd));
                            index++;
                        }
                    }
                    break;
                case 2:
                    {
                        builder.Append("<select size=\"4\" class=\"form-control m715-50\" name=\"txt_" + Name + "\"  style=\"width:300px;height:126px\" multiple>");
                        string tlp = "<option value='{0}' {2}>{1}</option>";
                        foreach (var item in dic)
                        {
                            string chkd = (values.Length > 0 && values.Contains(item.Value)) ? " selected=\"selected\" " : "";
                            builder.Append(string.Format(tlp, item.Value, item.Key, chkd));
                        }
                        builder.Append("</select>");
                    }
                    break;
                case 3:
                    {
                        builder.Append("<input type='hidden' id='txt_" + Name + "' name='txt_" + Name + "' value='" + fieldValue + "'/>");
                        builder.Append("<iframe frameborder='0' scrolling='auto' width='100%' height='200px' src='/common/MultiField?fieldname=" + Name + "&content=" + fieldContent + "&fieldcontent=" + fieldValue + "'></iframe>");
                        break;
                    }
            }
            builder.Append("</td></tr>");
            return builder.ToString();
        }
        /// 入库图片
        public string GetSqlType(string Alias, string Name, bool IsNotNull, string Content, string Description, int ModelID, int NodeID, DataRow dr)
        {
            if (config.mode == ModelConfig.SMode.PreView) { return PreViewMode(Alias, dr, Name); }
            string str2 = "";
            if (IsNotNull)
            {
                str2 = "<font color=\"red\">*</font>";
            }
            StringBuilder builder = new StringBuilder();
            builder.Append("<tr><td class=\"fd_td_l\"><span>" + Alias + "：</span></td><td>");
            builder.Append("<table border='0' width='100%'>");
            builder.Append("<tr>");
            builder.Append("<td>");
            if (dr == null)
            {
                builder.Append("<input type=\"hidden\" name=\"FIT_" + Name + "\" id=\"FIT_" + Name + "\" ><input type=\"hidden\" name=\"txt_" + Name + "\" id=\"txt_" + Name + "\" >" + str2 + " " + Description + "</td></tr>");
                builder.Append("<tr><td><iframe id=\"Upload_" + Name + "\" src=\"/" + ManageDir + "/Common/FileInData?Showtype=0&ModelID=" + ModelID + "&FieldName=" + Name + "&NodeId=" + NodeID + "\" marginheight=\"0\" marginwidth=\"0\" frameborder=\"0\" width=\"100%\" height=\"30\" scrolling=\"no\"></iframe></td></tr></table>");
                builder.Append("</td></tr>");
                return builder.ToString();
            }

            builder.Append("<input type=\"hidden\" name=\"FIT_" + Name + "\" id=\"FIT_" + Name + "\" value=\"" + dr["FIT_" + Name + ""].ToString() + "\" ><input type=\"hidden\" name=\"txt_" + Name + "\" id=\"txt_" + Name + "\" size=\"35\" value=\"" + dr[Name] + "\">" + str2 + " " + Description + "</td></tr>");
            builder.Append("<tr><td><iframe id=\"Upload_" + Name + "\" src=\"/" + ManageDir + "/Common/FileInData?Showtype=0&ModelID=" + ModelID + "&FieldName=" + Name + "&NodeId=" + NodeID + "\" marginheight=\"0\" marginwidth=\"0\" frameborder=\"0\" width=\"100%\" height=\"30\" scrolling=\"no\"></iframe></td></tr></table>");
            builder.Append("</td></tr>");
            return builder.ToString();
        }
        /// 入库文件
        public string GetSqlFile(string Alias, string Name, bool IsNotNull, string Content, string Description, int ModelID, int NodeID, DataRow dr)
        {
            if (config.mode == ModelConfig.SMode.PreView) { return PreViewMode(Alias, dr, Name); }
            string str2 = "";
            if (IsNotNull)
            {
                str2 = "<font color=\"red\">*</font>";
            }
            StringBuilder builder = new StringBuilder();
            builder.Append("<tr><td class=\"fd_td_l\"><span>" + Alias + "：</span></td><td>");
            builder.Append("<table border='0' width='100%'>");
            builder.Append("<tr>");
            builder.Append("<td>");
            if (dr == null)
            {
                builder.Append("<input type=\"text\" name=\"txt_" + Name + "\" id=\"txt_" + Name + "\" class='form-control text_md' /> <span style='color:green;'>请上传10m以下的文件</span><input type=\"hidden\" name=\"FIT_" + Name + "\" id=\"FIT_" + Name + "\" >" + str2 + " " + Description + "</td></tr>");
                builder.Append("<tr><td><iframe id=\"Upload_" + Name + "\" src=\"/Common/FileUpload?ModelID=" + ModelID + "&FieldName=" + Name + "\" marginheight=\"0\" marginwidth=\"0\" frameborder=\"0\" width=\"100%\" height=\"30\" scrolling=\"no\"></iframe></td></tr></table>");
                builder.Append("</td></tr>");
            }
            else
            {
                builder.Append("<input type=\"text\" class='form-control text_md' name=\"txt_" + Name + "\" id=\"txt_" + Name + "\" size=\"35\" value=\"" + dr[Name].ToString().Split('|')[0] + "\"> <span style='color:green;'>请上传10m以下的文件</span><input type=\"hidden\" name=\"FIT_" + Name + "\" id=\"FIT_" + Name + "\"  value=\"" + dr[Name].ToString() + "\" >" + str2 + " " + Description + "</td></tr>");
                builder.Append("<tr><td><iframe id=\"Upload_" + Name + "\" src=\"/Common/FileUpload?ModelID=" + ModelID + "&FieldName=" + Name + "\" marginheight=\"0\" marginwidth=\"0\" frameborder=\"0\" width=\"100%\" height=\"30\" scrolling=\"no\"></iframe></td></tr></table>");
                builder.Append("</td></tr>");
            }
            return builder.ToString();
        }
        /// <summary>
        /// 腾讯微视频
        /// </summary>
        public string GetTencentMvs(string Alias, string Name, bool IsNotNull, string Content, string Description, int ModelID, int NodeID, DataRow dr)
        {
            if (config.mode == ModelConfig.SMode.PreView) { return PreViewMode(Alias, dr, Name); }
            StringBuilder sb = new StringBuilder();
            sb.Append("<tr><td class=\"fd_td_l\"><span>" + Alias + "：</span></td><td class=\"fd_td_r\">");
            string value = dr == null || dr[Name] == null ? "" : dr[Name].ToString();
            string remotePath = "";
            string url = "";
            bool hasmvs = false;
            if (!string.IsNullOrEmpty(value))
            {
                string[] valueArr = value.Split('|');//远程文件路径|上传后下载地址
                remotePath = valueArr[0];
                url = valueArr[1];
                hasmvs = true;
            }
            sb.Append("<div id='show_" + Name + "' style='" + (hasmvs ? "" : "display:none") + "'><video id='video_" + Name + "' src='" + url + "' style='width:300px;height:200px;' controls='controls'>您的浏览器不支持 video 标签。</video><br/>");
            sb.Append("<span id='mvsurl_" + Name + "'>" + url + "</span>");
            sb.Append("<input type='button' value='重新上传' onclick=\"api_qq_mvs.ShowDiag('" + Name + "','" + remotePath + "')\" class='btn btn-info'/></div>");
            sb.Append("<input id='upbtn_" + Name + "' type='button' value='上传微视频' onclick=\"api_qq_mvs.ShowDiag('" + Name + "')\" class='btn btn-info' style='" + (hasmvs ? "display:none" : "") + "'/>");
            sb.Append("<input type='hidden' name='UpMvs_" + Name + "' id='UpMvs_" + Name + "' value='" + url + "' />");
            return sb.ToString();
        }
        // 单个文件
        public string GetSmallFileType(string Alias, string Name, bool IsNotNull, string Content, string Description, int ModelID, int NodeID, DataRow dr)
        {
            if (config.mode == ModelConfig.SMode.PreView) { return PreViewMode(Alias, dr, Name); }
            string str2 = "";
            if (IsNotNull)
            {
                str2 = "<font color=\"red\">*</font>";
            }
            FieldModel fm = new FieldModel(Content);
            StringBuilder builder = new StringBuilder();
            builder.Append("<tr class=\"fd_tr fd_tr_smallfiletype\"><td class=\"fd_td_l\"><span>" + Alias + "：</span></td><td class=\"fd_td_r\">");
            builder.Append("<table border='0' width='100%'>");
            builder.Append("<tr>");
            builder.Append("<td>");
            if (dr == null)
            {
                builder.Append("<input type=\"text\" class=\"form-control m715-50\" name=\"txt_" + Name + "\" id=\"txt_" + Name + "\" size=\"35\">" + str2 + " " + Description);
                if (fm.GetValue("SelUpfile") == "1")
                {
                    builder.Append("<a href='javascript:;' onclick=\"g({\'name\':\'" + Name + "\'})\" title='在线选取' class='btn btn-info'><i class='fa fa-picture-o'></i> 在线选取</a>");
                }
                builder.Append("</td></tr>");
                JObject obj = new JObject();
                obj.Add(new JProperty("nodeid", NodeID));
                obj.Add(new JProperty("inputid", Name));
                obj.Add(new JProperty("uploaddir", updir));
                string bigfilehtml = "<div><button type='button' class='btn btn-primary' onclick='UpFileDiag(" + JsonConvert.SerializeObject(obj) + ")'>点击上传</button></div>";
                string smallfilehtml = "<iframe id=\"Upload_" + Name + "\" src=\"/Common/FileUpload?Showtype=0&ModelID=" + ModelID + "&FieldName=" + Name + "&NodeId=" + NodeID + "\" marginheight=\"0\" marginwidth=\"0\" frameborder=\"0\" width=\"100%\" height=\"45\" scrolling=\"no\"></iframe>";
                builder.Append("<tr><td>" + (fm.GetValue("isbigfile").Equals("1") ? bigfilehtml : smallfilehtml) + " </td></tr></table>");
                builder.Append("</td></tr>");
                return builder.ToString();
            }
            builder.Append("<input type=\"text\" class=\"form-control m715-50\" style=\"float:left\" name=\"txt_" + Name + "\" id=\"txt_" + Name + "\" size=\"35\" value=\"" + dr[Name].ToString() + "\">" + str2 + " " + Description);
            if (dr[Name].ToString() != "" && dr[Name].ToString() != null)
            {
                builder.Append("<a href=\"" + updir + dr[Name].ToString() + "\" target=\"_blank\" style=\"color:#fff;\" class=\"download\">下载</a>");
            }
            if (fm.GetValue("SelUpfile") == "1")
            {
                builder.Append("<a href='javascript:;' onclick=\"SelectUppic({\'name\':\'" + Name + "\'})\" title='在线选取' class='btn btn-info'><i class='fa fa-picture-o'></i> 在线选取</a>");
            }
            builder.Append("</td></tr>");
            builder.Append("<tr><td><iframe id=\"Upload_" + Name + "\" src=\"/Common/FileUpload?Showtype=0&ModelID=" + ModelID + "&FieldName=" + Name + "&NodeId=" + NodeID + "\" marginheight=\"0\" marginwidth=\"0\" frameborder=\"0\" width=\"100%\" height=\"45\" scrolling=\"no\"></iframe></td></tr></table>");
            builder.Append("</td></tr>");
            return builder.ToString();
        }
        // 下拉文件
        public string GetPullFileType(string Alias, string Name, bool IsNotNull, string Content, string Description, DataRow dr)
        {
            return "";
        }
        // 单选项
        public string GetRadioType(string Alias, string Name, bool IsNotNull, string Content, string Description, DataRow dr)
        {
            //if (!Content.Contains("$")) { return "<tr class=\"fd_tr fd_tr_optiontype\"><td class=\"fd_td_l\"><span>" + Alias + "：</span></td><td class=\"fd_td_r\" style=\"color:red;\">字段格式不正确：" + Content + "</td></tr>"; }
            //1=不分页|0$0||自动分页|1$0||手动分页|2$0,Property=True,Default=
            //1=本站原创|本站原创$0||网络摘抄|网络摘抄$0||网友投稿|网友投稿$0,Property=True,Default=

            if (config.mode == ModelConfig.SMode.PreView) { return PreViewMode(Alias, dr, Name); }
            string disabled = "";
            //name,index,value,str,是否选中
            //string radTlp = "<label><input id='txt_{0}_{1}' type='radio' name='txt_{0}' value='{2}' {4} />{3}</label>";
            if (config.Disable)
            {
                disabled = "disabled='disabled'";
            }
            FieldModel fieldMod = new FieldModel(Content);
            int optype = DataConvert.CLng(Content.Split('=')[0]);
            string id = "txt_" + Name;
            string[] options = Regex.Split(Content.Split(',')[0].Split('=')[1], Regex.Escape("||"));//text|value$0   分隔符为||
            string value = fieldMod.GetValue("Default");
            if (dr != null) { value = dr[Name].ToString(); }
            bool property = fieldMod.GetValBool("Property");
            //-----------------------------------
            string html = "";
            StringBuilder builder = new StringBuilder();
            builder.Append("<tr class=\"fd_tr fd_tr_optiontype\"><td class=\"fd_td_l\"><span>" + Alias + "：</span></td><td class=\"fd_td_r\">");
            switch (optype)
            {
                case 1:
                    #region  select
                    {
                        html = "<select " + disabled + " class=\"form-control m715-50 fd_select\" name=\"" + id + "\">";
                        foreach (string op in options)
                        {
                            string text = op.Split('$')[0].Split('|')[0];
                            string val = op.Split('$')[0].Split('|').Length > 1 ? op.Split('$')[0].Split('|')[1] : op.Split('$')[0].Split('|')[0];
                            html += string.Format("<option value=\"" + val + "\" {0} >" + text + "</option>", (val.Equals(value) ? "selected=\"selected\"" : ""));
                        }
                        html += "</select>";
                    }
                    #endregion
                    break;
                case 2:
                    #region radio
                    {
                        foreach (string op in options)
                        {
                            string text = op.Split('$')[0].Split('|')[0];
                            string val = op.Split('$')[0].Split('|').Length > 1 ? op.Split('$')[0].Split('|')[1] : text;
                            html += string.Format("<label><input type='radio' name='" + id + "' value='" + val + "' {0} />" + text + "</label>", (val.Equals(value) ? "checked=\"checked\"" : ""));
                        }
                    }
                    #endregion
                    break;
            }
            builder.Append(html);
            builder.Append("</td></tr>");
            return builder.ToString();
        }
        // 超链接
        private string GetSuperLinkType(string Alias, string Name, bool IsNotNull, string Content, string Description, DataRow dr)
        {
            if (config.mode == ModelConfig.SMode.PreView) { return PreViewMode(Alias, dr, Name); }
            //TitleSize=50,DefaultValue=sadfsadfsadf
            FieldModel model = new FieldModel(Content);
            int size = model.GetValInt("TitleSize");
            string def = model.GetValue("DefaultValue");
            string required = IsNotNull ? reqTlp : "";
            string val = dr == null ? def : dr[Name].ToString();
            StringBuilder builder = new StringBuilder();
            builder.Append("<tr><td class=\"fd_td_l\"><span>" + Alias + "：</span></td><td>");
            builder.Append("<input type=\"text\" class=\"form-control m715-50\" name=\"txt_" + Name + "\" id=\"txt_" + Name + "\" size=\"" + size + "\" value=\"" + val + "\">" + Description + required);
            builder.Append("</td></tr>");
            return builder.ToString();
        }
        // 运行平台
        private string GetOperatingType(string Alias, string Name, bool IsNotNull, string Content, string Description, DataRow dr)
        {
            if (config.mode == ModelConfig.SMode.PreView) { return PreViewMode(Alias, dr, Name); }
            string[] strArray = Content.Split(new char[] { ',' });
            string[] strArray2 = null;
            string[] strArray3 = null;
            string[] strArray4 = null;

            if (strArray != null && strArray.Length > 0)
            {
                strArray2 = strArray[0].Split(new char[] { '=' });
            }
            if (strArray != null && strArray.Length > 1)
            {
                strArray3 = strArray[1].Split(new char[] { '=' });
            }
            if (strArray != null && strArray.Length > 2)
            {
                strArray4 = strArray[2].Split(new char[] { '=' });
            }
            string str2 = "";
            string str = "";
            string str3 = "";
            string s2 = "";
            string s3 = "";
            string s4 = "";

            if (strArray2 != null && strArray2.Length > 1)
            {
                s2 = strArray2[1];
            }
            if (strArray3 != null && strArray3.Length > 1)
            {
                s3 = strArray3[1];
            }
            if (strArray4 != null && strArray4.Length > 1)
            {
                s4 = strArray4[1];
            }
            string[] strArr = s3.Split(new char[] { '|' });
            for (int i = 0; i < strArr.Length; i++)
            {
                if (string.IsNullOrEmpty(str3))
                {
                    str3 = "<a href=\"javascript:ToSystem('" + strArr[i] + "','txt_" + Name + "')\">" + strArr[i] + "</a>";
                }
                else
                {
                    str3 = "/" + "<a href=\"javascript:ToSystem('" + strArr[i] + "','txt_" + Name + "')\">" + strArr[i] + "</a>";
                }
            }
            if (IsNotNull)
            {
                str2 = "<font color=\"red\">*</font>";
            }
            StringBuilder builder = new StringBuilder();
            builder.Append("<tr class=\"fd_tr fd_tr_operatingtype\"><td class=\"fd_td_l\"><span>" + Alias + "：</span></td><td class=\"fd_td_r\">");
            if (dr == null)
            {
                str = "<input type=\"text\" class=\"form-control m715-50\" name=\"txt_" + Name + "\" id=\"txt_" + Name + "\" size=\"" + s2 + "\" value=\"" + s4 + "\"> " + str2 + " " + Description + "<br/>";
                str = str + str3;
                builder.Append(str);
                builder.Append("</td></tr>");
                return builder.ToString();
            }
            str = "<input type=\"text\" class=\"form-control m715-50\" name=\"txt_" + Name + "\" id=\"txt_" + Name + "\" size=\"" + s2 + "\" value=\"" + HttpUtility.HtmlEncode(dr[Name].ToString()) + "\"> " + str2 + " " + Description + "<br/>";
            str = str + str3;
            builder.Append(str);
            builder.Append("</td></tr>");
            return builder.ToString();
        }
        // 多图片
        private string GetMultPicType(string Alias, string Name, bool IsNotNull, string Content, string Description, int ModelID, int NodeID, DataRow dr)
        {
            if (config.mode == ModelConfig.SMode.PreView) { return PreViewImg_Json(Alias, dr, Name); }
            try
            {
                FieldModel model = new FieldModel(Content);
                var tlp = "<tr><td class=\"tdleft\">" + Alias + "：</td><td><ul data-id='txt_{0}' id=\"ul_{0}\" class=\"preview_img_ul\">{3}</ul>"
                            + "<div class=\"clearfix\"></div>"
                            + "<textarea id=\"txt_{0}\" name=\"txt_{0}\" class=\"form-control m715-50 tarea_l\">{1}</textarea>"
                            + "<div class='zt_op_div'><input type='button' class='btn btn-info' value='批量上传' onclick='UpFileDiag({2});' />"
                            + "<input type='button' class='btn btn-info' value='远程抓取' onclick='UpFileDiag({2}, \"/Plugins/WebUploader/RemoteImg\");'/>"
                            + "<input type='button' class='btn btn-info' value='图片库' onclick='SelectUppic({2});'/>"
                            + "<input type='button' class='btn btn-info' value='组图排序' onclick='SortImg({2})'"
                            + "</div></td></tr>";
                string value = "", lis = "";
                string imgtlp = "<li class='margin_l5'><img src='{0}' class='preview_img'/><div class='file-panel' style='height: 0px;'><span class='cancel' title='删除'>删除</span><span class='editpic' title='编辑'>编辑</span></div></li>";
                if (dr != null && !string.IsNullOrEmpty(dr[Name].ToString()))//有值
                {
                    DataTable imgsData = JsonConvert.DeserializeObject<DataTable>(dr[Name].ToString());
                    foreach (DataRow img in imgsData.Rows)
                    {
                        lis += string.Format(imgtlp, updir + img["url"]);
                    }
                }
                JObject obj = new JObject();
                obj.Add(new JProperty("field", "images"));//标识自己为组图字段
                obj.Add(new JProperty("nodeid", NodeID));
                obj.Add(new JProperty("inputid", Name));
                obj.Add(new JProperty("uploaddir", updir));
                return string.Format(tlp, Name, value, JsonConvert.SerializeObject(obj), lis);
            }
            catch (Exception ex) { return ShowError("组图字段", Name, ex.Message); }
        }
        // 多级选项
        private string GetGradeOption(string Alias, string Name, bool IsNotNull, string Content, string Description, DataRow dr)
        {
            //GradeCate=b,Direction=1(1:竖,0:横)
            if (config.mode == ModelConfig.SMode.PreView) { return PreViewMode(Alias, dr, Name); }
            FieldModel model = new FieldModel(Content);
            string tlp = "<tr class='fd_tr fd_tr_gradeoption'><td class='fd_td_l'>" + Alias + "：</td><td class='fd_td_r'>{0}</td></tr>";
            string html = "", value = "";
            string gradeCate = model.GetValue("GradeCate");
            int direction = model.GetValInt("Direction");
            string height = "40px;";
            //----------------------------------------------------
            if (dr != null && !string.IsNullOrEmpty(dr[Name].ToString())) { value = dr[Name].ToString(); }
            html = "<input type='hidden' name='txt_" + Name + "' id='txt_" + Name + "' value='" + value + "' />";
            switch (gradeCate)
            {
                case "a":
                    if (direction == 1) { height = "125px;"; }
                    html += "<iframe id='Drop_" + Name + "' src='/Common/PPC?FieldName=" + Name + "&FValue=" + value + "&Direction=" + direction + "' style='border:none;width:100%;height:" + height + ";' scrolling='no'></iframe>";
                    break;
                case "b":
                    if (direction == 1) { height = "150px;"; }
                    html += "<iframe id='Drop_" + Name + "' src='/Common/PPC?dptype=1&CateID=" + gradeCate + "&FieldName=" + Name + "&FValue=" + value + "&Direction=" + direction + "' style='border:none;width:100%;height:" + height + ";' scrolling='no'></iframe>";
                    break;
                default:
                    if (direction == 1) { height = "125px;"; }
                    html += "<iframe id='Drop_" + Name + "' src=\"/Common/MultiDropList?CateID=" + gradeCate + "&FieldName=" + Name + "&FValue=" + value + "&Direction=" + direction + "\" style='border:none;width:100%;height:" + height + ";' scrolling=\"no\"></iframe>";
                    break;
            }
            return string.Format(tlp, html);
        }
        // 报表
        public string Charts(string Alias, string Name, bool IsNotNull, string Content, string Description, int ModelID, int NodeID, DataRow dr)
        {
            return "";
        }
        // Google地图
        public string GetMap(string Alias, string Name, bool IsNotNull, string Content, string Description, int ModelID, int NodeID, DataRow dr)
        {
            if (config.mode == ModelConfig.SMode.PreView)
            {
                return PreViewMode(Alias, dr, Name);
            }
            FieldModel model = new FieldModel(Content);
            StringBuilder builder = new StringBuilder();
            builder.Append("<tr class=\"fd_tr fd_tr_MapType\"><td class=\"fd_td_l\"><span>" + Alias + "：</span></td>");
            builder.Append("<td class=\"fd_td_r\" >");//style=\"line-height:100px; height:100px\"
            switch (model.GetValue("source"))
            {
                case "baidu":
                    #region 百度地图
                    {
                        string val = (dr != null ? dr[Name].ToString() : "");
                        //预览
                        string viewurl = "/Common/Label/Map/BaiduMapView?Field=" + Name + "&ispre=1&Type=" + model.GetValue("type");
                        string preViewMap = "<iframe id='" + Name + "_ifr' src='{0}' style='width:500px;height:300px;border:none;' scrolling='no'></iframe>";
                        switch (model.GetValue("type"))
                        {
                            case "full"://完全版
                                builder.Append("<div><input type='hidden' name='txt_" + Name + "' id='txt_" + Name + "' value='" + val + "' /></div>");
                                builder.Append("<div style='margin-top:5px;'>" + string.Format(preViewMap, viewurl) + "</div>");
                                break;
                            case "simp"://简单版
                            default:
                                builder.Append("<div><input type='hidden' name='txt_" + Name + "' id='txt_" + Name + "' value='" + val + "' /></div>");
                                builder.Append("<div style='margin-top:5px;'>" + string.Format(preViewMap, viewurl + "&Point=" + val) + "</div>");
                                break;
                        }
                    }
                    #endregion
                    break;
                case "google":
                    #region Google地图
                    builder.Append("<input type=\"hidden\" id=\"hmap\" name=\"hmap\" />");
                    try
                    {
                        if (dr == null)
                        {
                            builder.Append("<input type=\"button\" value=\"添加地图\" onclick=\"Addmap();\" />");
                            builder.Append("</td>");
                            builder.Append("</td>");
                            builder.Append("<script>function Addmap() {window.open('/" + ManageDir + "/Template/AddMap?Mid=0', 'newWin', 'modal=yes,width=900,height=600,resizable=yes,scrollbars=yes');}</script>");
                        }
                        else
                        {
                            builder.Append("<input type=\"button\" value=\"预览地图\" onclick=\"Addmap();\" />");
                            builder.Append("</td>");
                            builder.Append("</td>");
                            builder.Append("<script>function Addmap() {window.open('/" + ManageDir + "/Template/AddMap?Mid=" + dr[Name].ToString() + "', 'newWin', 'modal=yes,width=900,height=600,resizable=yes,scrollbars=yes');}</script>");
                        }
                    }
                    catch
                    {
                        builder.Append("<script>function Addmap() {window.open('/" + ManageDir + "/Template/AddMap?Mid=0', 'newWin', 'modal=yes,width=900,height=600,resizable=yes,scrollbars=yes');}</script>");
                    }
                    break;
                #endregion
                default:
                    break;
            }
            builder.Append("</td></tr>");
            return builder.ToString();
        }
        // 智能多文件上传
        public string SwfFileUpload(string Alias, string Name, bool IsNotNull, string Content, string Description, int ModelID, int NodeID, DataRow dr)
        {
            //ChkFileSize=1,FileSizeField=ttf,MaxFileSize=,UploadFileExt=zip
            if (config.mode == ModelConfig.SMode.PreView) { return PreViewImg(Alias, dr, Name); }
            FieldModel model = new FieldModel(Content);
            int checkSize = model.GetValInt("ChkFileSize");//1开启检测,显示文本框
            StringBuilder builder = new StringBuilder();
            builder.Append("<tr class='fd_tr fd_tr_swffileupload'><td class='fd_td_l'><span>" + Alias + "：</span></td><td class=\"fd_td_r\">");
            if (dr != null)
            {
                if (!string.IsNullOrEmpty(dr[Name].ToString()))
                {
                    builder.Append("<input type=\"hidden\" name=\"txt_" + Name + "\" id=\"txt_" + Name + "\" value=\"" + dr[Name].ToString() + "\">");
                }
                else
                {
                    builder.Append("<input type=\"hidden\" name=\"txt_" + Name + "\" id=\"txt_" + Name + "\">");
                }
            }
            else
            {
                builder.Append("<input type=\"hidden\" name=\"txt_" + Name + "\" id=\"txt_" + Name + "\">");
            }
            builder.Append("<select name='sel_" + Name + "' class=\"form-control m715-50\" id='sel_" + Name + "' style='height: 100px;display:block;padding-left:5px;' ondblclick=\"ModifyUrl(document.form1.sel_" + Name + ",'sel_" + Name + "','txt_" + Name + "')\" size='2'>");
            if (dr != null && !string.IsNullOrEmpty(dr[Name].ToString()))
            {
                string[] strValue = dr[Name].ToString().Split(new char[] { '$' }, StringSplitOptions.RemoveEmptyEntries);
                for (int i = 0; i < strValue.Length; i++)
                {
                    builder.Append("<option value=\"" + strValue[i] + "\">" + strValue[i] + "</option>");
                }
            }
            JObject jobj = new JObject();
            jobj.Add(new JProperty("nodeid", NodeID));
            jobj.Add(new JProperty("objid", "sel_" + Name));
            jobj.Add(new JProperty("inputid", "txt_" + Name));
            jobj.Add(new JProperty("field", "SwfFileUpload"));
            jobj.Add(new JProperty("uploaddir", updir));
            jobj.Add(new JProperty("iswater", "0"));
            builder.Append("</select>");
            builder.Append("<div class='btn-group' style=\"padding-left:0px;padding-top:5px;\">");
            builder.Append("<input type=\"button\" class=\"btn btn-primary\" value=\"批量上传\" onclick='UpFileDiag(" + JsonConvert.SerializeObject(jobj) + ")' />");
            builder.Append("<input type=\"button\" class=\"btn btn-info\" value=\"选择\" onclick=\"SelectFiles('sel_" + Name + "','txt_" + Name + "',2)\" >");
            builder.Append("<input type=\"button\" class=\"btn btn-info\" value=\"添加\" onclick=\"AddPhotoUrl('sel_" + Name + "','txt_" + Name + "',2)\">");
            builder.Append("<input type=\"button\" class=\"btn btn-info\" value=\"修改\" onclick=\"return ModifyPhotoUrl('sel_" + Name + "','txt_" + Name + "',2);\" />");
            builder.Append("<input type=\"button\" class=\"btn btn-info\" value=\"删除\" onclick=\"DelPhotoUrl('sel_" + Name + "','txt_" + Name + "',2);\" />");
            builder.Append("</div>");
            if (checkSize == 1)
            {
                string value = "", field = model.GetValue("FileSizeField");
                if (dr != null) { value = DataConvert.CStr(dr[field]); }
                builder.Append("<div class='input-group' style='width: 240px;padding-left:5px;margin-top:5px;'>");
                builder.Append("<span class='input-group-addon'>文件大小</span>");
                builder.Append("<input name='txt_" + field + "' type='text' class='form-control' style='width:212px;' value='" + value + "' placeholder='请输入文件大小'>");
                builder.Append("<div>");
            }
            builder.Append("</td></tr>");
            return builder.ToString();
        }
        public string RemoteFile(string Alias, string Name, bool IsNotNull, string Content, string Description, int ModelID, int NodeID, DataRow dr)
        {
            ///因为一样所以用同一个方法
            return SwfFileUpload(Alias, Name, IsNotNull, Content, Description, ModelID, NodeID, dr);
        }
        /// <summary>
        /// 库选字段,返回下拉列表
        /// </summary>
        /// <param name="Alias">字段别名</param>
        /// <param name="Name">字段名称</param>
        /// <param name="IsNotNull">是否必填</param>
        /// <param name="Description">字段描述</param>
        /// <param name="Content">字段设定内容</param>
        /// <returns>下列列表</returns>
        public string TableField(string Alias, string Name, bool IsNotNull, string Content, string Description, int ModelID, int NodeID, DataRow dr)
        {
            if (config.mode == ModelConfig.SMode.PreView) { return PreViewMode(Alias, dr, Name); }
            //ZL_User.UserName,ZL_User.UserID
            try
            {
                FieldModel fieldmod = new FieldModel(Content);
                string tbName = fieldmod.GetValue("TextField").Split('.')[0];
                string textField = fieldmod.GetValue("TextField").Split('.')[1];
                string valField = textField;
                if (!string.IsNullOrEmpty(fieldmod.GetValue("ValueField")))
                { valField = fieldmod.GetValue("ValueField").Split('.')[1]; }
                //string.IsNullOrEmpty(Content.Split(',')[1]) ? textField : Content.Split(',')[1].Split('.')[1];
                string where = fieldmod.GetValue("WhereStr");
                if (!string.IsNullOrEmpty(where)) { where = " AND " + where; }
                string downHtml = "<tr class=\"fd_tr fd_tr_tablefield\"><td class='fd_td_l'>" + Alias + "：</td><td class=\"fd_td_r\">";
                DataTable dt = SqlHelper.ExecuteTable(CommandType.Text, "Select * From " + tbName + " WHERE 1=1 " + where);
                if (fieldmod.GetValue("FieldType").Equals("1"))//单选模式
                {
                    downHtml += "<select class=\"form-control m715-50\" id='" + Name + "' name='txt_" + Name + "'>";
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        downHtml += "<option value='" + dt.Rows[i][valField].ToString() + "'>" + dt.Rows[i][textField] + "</option>";
                    }
                    downHtml += "</select></td>";
                    if (dr != null)
                    {
                        downHtml = downHtml.Replace("value='" + dr[Name] as string + "'", "value='" + dr[Name] as string + "' selected='selected'");
                    }
                }
                else//多选模式
                {
                    downHtml += "<div>";
                    foreach (DataRow valuedr in dt.Rows)
                    {
                        string checkedstr = "";
                        if (dr != null && ("$" + dr[Name].ToString() + "$").Contains("$" + valuedr[valField] + "$"))
                        {
                            checkedstr = "checked='checked'";
                        }
                        downHtml += "<label><input type='checkbox' name='txt_" + Name + "' value='" + valuedr[valField] + "' " + checkedstr + " />" + valuedr[textField] + "</label> ";
                    }
                    downHtml += "</div>";
                }
                return downHtml;
            }
            catch (Exception ex) { return ShowError("库选字段", Name, ex.Message); }
        }
        //随机数字段
        public string RandomCode(string Alias, string Name, bool IsNotNull, string Content, string Description, int ModelID, int NodeID, DataRow dr)
        {
            if (config.mode == ModelConfig.SMode.PreView) { return PreViewMode(Alias, dr, Name); }
            try
            {
                FieldModel model = new FieldModel(Content);
                //Alias,Name
                string tlp = "<tr class=\"fd_tr fd_tr_random\"><td class='fd_td_l'>{0}</td><td class=\"fd_td_r\"><input type='text' id='{1}' name='txt_{1}' value='{2}' class='form-control text_300' /></td></tr>";
                int type = DataConvert.CLng(model.GetValue("type"));
                int len = DataConvert.CLng(model.GetValue("len"));
                string value = dr == null ? function.GetRandomString(len, type) : dr[Name].ToString();
                return string.Format(tlp, Alias, Name, value);
            }
            catch (Exception ex) { return ShowError("随机数", Name, ex.Message); }
        }
        //拍照字段
        public string CameraCode(string Alias, string Name, bool IsNotNull, string Content, string Description, int ModelID, int NodeID, DataRow dr)
        {
            if (config.mode == ModelConfig.SMode.PreView) { return PreViewImg(Alias, dr, Name); }
            try
            {
                FieldModel model = new FieldModel(Content);
                string tlp = "<tr class='fd_tr fd_tr_cameracode'><td class='fd_td_l'>" + Alias + "：</td><td class='fd_td_r'><div style='float:left; border:1px solid #ddd;padding:5px;'><video id='{0}' class='fd_tr_video' autoplay='autoplay' style='width:{1}px;height:{2}px;'></video></div>"
                            + "<div style='float:left; margin-left:5px;'><div style='margin-top:8px;'><button class='fd_td_shoot_btn btn btn-primary' type='button'>在线拍照</button></div><div style='margin-top:10px;'><button class='fd_td_upfile_btn btn btn-primary' type='button'>保存图片</button></div><div style='margin-top:10px;'><button class='fd_td_resetcamera_btn btn btn-primary' type='button'>重置图片</button></div></div>"
                            + "<div style='float:left; margin-left:5px;'><img src='{5}' style='width:{3}px;height:{4}px;'/></div>"
                            + "<input name='{0}_hid' type='hidden' value='{5}'>"
                            + "</td></tr>";
                int cameraWidth = DataConvert.CLng(model.GetValue("cameraWidth"));
                int cameraHeight = DataConvert.CLng(model.GetValue("cameraHeight"));
                int imgWidth = DataConvert.CLng(model.GetValue("imgWidth"));
                int imgHeight = DataConvert.CLng(model.GetValue("imgHeight"));
                string value = dr == null ? "/UploadFiles/nopic.svg" : dr[Name].ToString();
                return string.Format(tlp, Name, cameraWidth, cameraHeight, imgWidth, imgHeight, value);
            }
            catch (Exception ex)
            { return ShowError("拍照字段", Name, ex.Message); }
        }
        //组图字段
        public string ImagesCode(string Alias, string Name, bool IsNotNull, string Content, string Description, int ModelID, int NodeID, DataRow dr)
        {
            if (config.mode == ModelConfig.SMode.PreView) { return PreViewImg_Json(Alias, dr, Name); }
            try
            {
                FieldModel model = new FieldModel(Content);
                string tlp = "<tr class=\"fd_tr fd_tr_image\"><td class=\"fd_td_l\">" + Alias + "：</td><td class=\"fd_td_r\"><ul data-id='txt_{0}' id=\"ul_{0}\" class=\"preview_img_ul\">{3}</ul>"
                            + "<div class=\"clearfix\"></div>"
                            + "<div><textarea id=\"txt_{0}\" name=\"txt_{0}\" class=\"form-control m715-50\" style=\"height: 110px\">{1}</textarea></div>"
                            + "<div class='btn-group' style=\"padding-left:0px;padding-top:2px;\"><input type='button' class='btn btn-primary' value='批量上传' onclick='UpFileDiag({2});' />"
                            + "<input type='button' class='btn btn-info' value='远程抓取' onclick='UpFileDiag({2}, \"/Plugins/WebUploader/RemoteImg\");'/>"
                            + "<input type='button' class='btn btn-info' value='图片库' onclick='SelectUppic({2});'/>"
                            + "<input type='button' class='btn btn-info' value='组图排序' onclick='SortImg({2})'"
                            + "</div></td></tr>";
                string value = "", lis = "";
                string imgtlp = "<li class='margin_l5'><img src='{0}' class='preview_img'/><div class='file-panel' style='height: 0px;'><span class='cancel' title='删除'>删除</span><span class='editpic' title='编辑'>编辑</span></div></li>";
                if (dr != null && !string.IsNullOrEmpty(dr[Name].ToString()))//有值
                {
                    value = dr[Name].ToString().Trim(',');
                    try
                    {
                        value = "[" + value.TrimStart('[').TrimEnd(']').TrimEnd(',') + "]";
                        JArray arr = JsonConvert.DeserializeObject<JArray>(value);
                        foreach (JObject jobj in arr)
                        {
                            lis += string.Format(imgtlp, function.GetImgUrl(jobj["url"]));
                        }
                    }
                    catch (Exception ex) { lis = "<i style='color:red;'>(异常:非标准JSON格式," + ex.Message + ")</i>"; }
                }
                JObject obj = new JObject();
                obj.Add(new JProperty("field", "images"));//标识自己为组图字段
                obj.Add(new JProperty("nodeid", NodeID));
                obj.Add(new JProperty("inputid", Name));
                obj.Add(new JProperty("name", Name));
                obj.Add(new JProperty("uploaddir", updir));
                obj.Add(new JProperty("iswater", model.GetValue("IsWater")));
                return string.Format(tlp, Name, value, JsonConvert.SerializeObject(obj), lis);
            }
            catch (Exception ex) { return ShowError("ImagesCode", Name, ex.Message); }
        }
        //压图传入
        public string AutoThumbCode(string Alias, string Name, bool IsNotNull, string Content, string Description, int ModelID, int NodeID, DataRow dr)
        {
            if (config.mode == ModelConfig.SMode.PreView) { return PreViewImg(Alias, dr, Name); }
            try
            {
                FieldModel model = new FieldModel(Content);
                string tlp = "<tr class=\"fd_tr fd_tr_autothumb\"><td class='fd_td_l'>{0}：</td><td class=\"fd_td_r\"><input type='text' id='{1}' name='txt_{1}' value='{2}' class='form-control m715-50 autothumb_t' /></td></tr>";
                int width = model.GetValInt("width");
                int height = model.GetValInt("height");
                string value = dr == null ? "" : dr[Name].ToString();
                return string.Format(tlp, Alias, Name, value);
            }
            catch (Exception ex) { return ShowError("压图传入", Name, ex.Message); }
        }
        //颜色代码
        private string GetColorType(string Alias, string Name, bool IsNotNull, string Content, string Description, DataRow dr)
        {
            if (config.mode == ModelConfig.SMode.PreView) { return PreViewMode(Alias, dr, Name); }
            string str = "";
            string[] strArray = Content.Split(new char[] { ',' });
            string[] strArray2 = strArray[1].Split(new char[] { '=' });
            if (dr != null)
                str = dr[Name].ToString();
            else
                str = strArray2[1];
            StringBuilder builder = new StringBuilder();
            string str2 = "";
            builder.Append("<tr class=\"fd_tr fd_tr_colortype\"><td class=\"fd_td_l\"><span>" + Alias + "：</span></td><td class=\"fd_td_r\">");
            if (IsNotNull)
            {
                str2 = reqTlp;
            }
            builder.Append("<script type=\"text/javascript\">");
            builder.Append("function SelectColor(t, clientId) {");
            builder.Append("var url = \"/Common/SelectColor?d=f&t=6\"; ");
            builder.Append("var old_color = (document.getElementById(clientId).value.indexOf('#') == 0) ? '&' + document.getElementById(clientId).value.substr(1) : '&' + document.getElementById(clientId).value;");
            builder.Append("if (document.all) {");
            builder.Append("var color = showModalDialog(url + old_color, '',\"dialogWidth:18.5em; dialogHeight:16.0em; status:0\");");
            builder.Append("if (color != null) {");
            builder.Append("document.getElementById(clientId).value = color;");
            builder.Append("} else {");
            builder.Append("document.getElementById(clientId).focus();");
            builder.Append("}");
            builder.Append("} else {");
            builder.Append("var color = window.open(url + '&' + clientId, \"hbcmsPop\", \"top=200,left=200,scrollbars=yes,dialog=yes,modal=no,width=300,height=260,resizable=yes\");");
            builder.Append("}");
            builder.Append("}");
            builder.Append("</script>");
            builder.Append("<input type=\"text\" class=\"form-control m715-50\" id=\"txt_" + Name + "\" name=\"txt_" + Name + "\" maxlength=\"7\"  size=\"7\"  value=\"" + str + "\"> " + str2);
            builder.Append("<span class='fa fa-font'title='颜色选择' style='font-size:20px; cursor:pointer;' onclick=\"SelectColor(this,'txt_" + Name + "');\"></span>" + Description);
            builder.Append("</td></tr>");
            return builder.ToString();
        }
        //------------------------------------PreView
        private string PreViewMode(string alias, DataRow dr, string name)
        {
            string value = dr == null ? "" : DataConvert.CStr(dr[name]);
            string tlp = "<tr classs='fd_tr fd_tr_pretext'><td class='fd_td_l'>" + alias + "</td><td class='fd_td_r'>" + value + "</td></tr>";
            return tlp;
            //str += "<tr><td>" + dt.Rows[i]["FieldAlias"].ToString() + "</td><td>" + contentDT.Rows[0][dt.Rows[i]["FieldName"].ToString()].ToString() + "</td></tr>";
        }
        //用于单张图预览,或以|号隔开的多张图预览,示例:文件地址1|fsxm/jpg/2015/12/225047199594.jpg
        private string PreViewImg(string alias, DataRow dr, string name)
        {
            string value = dr == null ? "" : DataConvert.CStr(dr[name]);
            string tlp = "<tr classs='fd_tr fd_tr_preimg'><td class='fd_td_l'>" + alias + "</td><td class='fd_td_r'>{0}</td></tr>";
            string imgHtml = "";
            if (StrHelper.StrNullCheck(value)) { return string.Format(tlp, ""); }
            string[] imgArr = value.Split("|".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            foreach (string img in imgArr)
            {
                if (!SafeSC.IsImage(img)) { continue; }
                imgHtml += "<img src='" + updir + img + "' class='thumbnail_img' title='" + img + "'/>";
            }
            return string.Format(tlp, imgHtml);
        }
        //Json格式的图片预览,用于智能图组
        private string PreViewImg_Json(string alias, DataRow dr, string name)
        {
            string value = dr == null ? "" : DataConvert.CStr(dr[name]);
            value = value.Trim(',');
            string tlp = "<tr classs='fd_tr fd_tr_preimg'><td class='fd_td_l'>" + alias + "</td><td class='fd_td_r'><ul class=\"preview_img_ul\">{0}</ul></td></tr>";
            string lis = "";
            string imgtlp = "<li class='margin_l5'><img src='{0}' title='{0}' class='thumbnail_img'/></li>";
            if (StrHelper.StrNullCheck(value)) { return string.Format(tlp, ""); }
            try
            {
                value = "[" + value.TrimStart('[').TrimEnd(']').TrimEnd(',') + "]";
                JArray arr = JsonConvert.DeserializeObject<JArray>(value);
                foreach (JObject jobj in arr)
                {
                    lis += string.Format(imgtlp, function.GetImgUrl(jobj["url"]));
                }
            }
            catch (Exception ex) { lis = "<i style='color:red;'>(异常:非标准JSON格式," + ex.Message + ")</i>"; }
            return string.Format(tlp, lis);
        }
        //------------------------------------Tools
        #region old(留下兼容旧版)
        // 标签lable
        public string GetLable(string value, string Alias, string Name, bool IsNotNull, string Content, string Description, DataRow dr)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append("<tr><td class=\"fd_td_l\"><b>" + Alias + "：</b></td><td>");
            if (dr == null)
            {
                builder.Append("</td></tr>");
                return builder.ToString();
            }
            builder.Append("<span>" + value + "</span>");
            builder.Append("</td></tr>");
            return builder.ToString();
        }
        #endregion
        private string SortStr(string str)
        {
            string context = "";
            string[] strx = str.Split(new string[] { "||" }, StringSplitOptions.RemoveEmptyEntries);

            foreach (string s in strx)
            {
                if (s.IndexOf("|") > 0)
                {
                    string[] ss = s.Split(new char[] { '|' });
                    if (ss.Length > 2)
                        context += SortStr(s.Replace("|", "||"));
                    else
                        context += s;
                }
                else
                {
                    context += s + "|" + s;
                }
                context += "||";
            }
            if (context.EndsWith("||"))
            {
                context = context.Remove(context.Length - 2);
            }
            return context;
        }
        private string GetNowuser(string label)
        {
            return "";
        }
        //用于下拉文件
        private string bindTemplatefolder(string pathstr, DataRow dr)
        {
            DataTable tables = FileSystemObject.GetDirectoryAllInfos(pathstr, FsoMethod.File);
            string Rn = "";
            for (int i = 0; i < tables.Rows.Count; i++)
            {
                if (tables.Rows[i]["type"].ToString() != "")
                {
                    string path = tables.Rows[i]["rname"].ToString();

                    string pathdir = (SiteConfig.SiteMapath()).Replace(@"\\", @"\");//path.Replace(@"\\", @"\");

                    path = path.Replace(pathdir, "");
                    path = path.Replace(@"\", @"/");
                    if (path.IndexOf(".svn") == -1)
                    {
                        if (path.Substring(0, 5) != "/style")
                        {
                            Rn += "<option value=\"" + path + "\"> " + path + "</option>";
                        }
                    }
                }
            }
            tables.Dispose();
            return Rn;
        }
        //----------
        private string ShowError(string ftype, string fname, string exmsg)
        {
            return "<tr class='fd_tr'><td class='fd_td_l'>" + fname + "<td><div style='color:red;'>[" + ftype + "]字段,[" + fname + "]出错,原因:" + exmsg + "</div></td></tr>";
        }
    }
}