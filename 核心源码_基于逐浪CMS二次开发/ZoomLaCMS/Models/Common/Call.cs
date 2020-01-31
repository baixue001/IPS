using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using ZoomLa.BLL;
using ZoomLa.BLL.Content;
using ZoomLa.Common;
using ZoomLa.Components;

namespace ZoomLaCMS
{
    public class Bread
    {
        public Bread() { }
        public Bread(string url, string text) { this.url = url; this.text = text; }
        public Bread(string type)
        {
            switch (type)
            {
                case "{admin}":
                case "{main}":
                    this.url = Call.PathAdmin("Index/WorkTable");
                    this.text = "工作台";
                    break;
                default:
                    break;
            }
        }
        /// <summary>
        /// li中的超链接URL
        /// </summary>
        public string url = "";
        /// <summary>
        /// 链接中的文本
        /// </summary>
        public string text = "";
        public string addon = "";//放置添加按钮等信息
                                 //如设定了html,则优先于前两个
        public string html = "";
    }
    public class Call
    {
        public const string SplitStr = "------asdfasgasgwqtwgsadgavzvxcvadfvzx";
        public const string Boundary = "------asjdfohponzvnzcvapowtunzafadsfwt";
        public static string SiteName { get { return SiteConfig.SiteInfo.SiteName; } }
        /// <summary>
        /// 前台等页面显示的Logo
        /// </summary>
        public static string LogoUrl { get { return SiteConfig.SiteInfo.LogoUrl; } }
        /// <summary>
        /// Call.SetBread(new Bread[] {
        ///new Bread("/{manage}/I/Main","工作台"),
        ///new Bread("/User/","用户配置"),
        ///new Bread() { text = "网站信息配置",addon = "[<a href='javascript:;'>更多配置</a>]" }
        ///})
        /// </summary>
        public static IHtmlContent SetBread(Bread[] breads, string addonHtml = "", string version = "admin_v4")
        {
            string html_wrap = "<nav aria-label=\"breadcrumb\" role=\"navigation\" class=\"fixed-top\"><ol class=\"breadcrumb\">{0}{1}</ol></nav>";
            string html = "";
            // <li class="breadcrumb-item active" aria-current="page">Library</li>
            for (int i = 0; i < breads.Length; i++)
            {
                Bread bread = breads[i];
                bread.url = bread.url.Replace("{manage}", SiteConfig.SiteOption.ManageDir);
                string html_a = "";
                //根据配置,确定面包屑是否要用超链接包裹
                if (!string.IsNullOrEmpty(bread.html))
                {
                    html_a = html;
                }
                else if (string.IsNullOrEmpty(bread.url))
                {
                    html_a = bread.text + bread.addon;
                }
                else
                {
                    html_a = "<a href='" + bread.url + "'>" + bread.text + "</a> " + bread.addon + "";
                }
                //================
                if (i == (breads.Length - 1))
                {
                    html += "<li class=\"breadcrumb-item active\" aria-current=\"page\">" + html_a + "</li>";
                }
                else
                {
                    html += "<li class=\"breadcrumb-item\">" + html_a + "</li>";
                }
            }
            if (addonHtml.Contains("{search}")) { addonHtml = addonHtml.Replace("{search}", Bread_Tlp_Search()); }
            return MvcHtmlString.Create(string.Format(html_wrap, html, addonHtml));
        }
        private static string Bread_Tlp_Search()
        {
            var html = "<div id=\"help\" class=\"pull-right text-center\"><a href=\"javascript::\" id=\"sel_btn\" onclick=\"selbox.toggle();\"><i class=\"zi zi_search\"></i></a></div>"
                     + "<div id=\"sel_box\">"
                     + "<div class=\"input-group\">"
                     + "<input type=\"text\" id=\"skey\" name=\"skey\" class=\"form-control mvcparam\" placeholder=\"请输入关键词\" />"
                     + "<span class=\"input-group-append\">"
                     + "<a class=\"btn btn-outline-secondary\" id=\"skey_btn\" onclick=\"mvcpage.load();\">搜索</a>"
                     + "</span>"
                     + "</div>"
                     + "</div>";
            return html;
        }

        public static DataTable GetDTFromMVC(DataTable dt, HttpRequest req, bool flag = true)
        {
            B_ModelField mfieldBll = new B_ModelField();
            dt.DefaultView.RowFilter = "IsCopy <>-1";
            dt = dt.DefaultView.ToTable();
            DataTable table = new DataTable();
            table.Columns.Add(new DataColumn("FieldName", typeof(string)));
            table.Columns.Add(new DataColumn("FieldType", typeof(string)));
            table.Columns.Add(new DataColumn("FieldValue", typeof(string)));
            table.Columns.Add(new DataColumn("FieldAlias", typeof(string)));
            foreach (DataRow dr in dt.Rows)
            {
                DataRow fdr = table.NewRow();
                try
                {
                    fdr[0] = dr["FieldName"].ToString();
                    fdr[1] = dr["FieldType"].ToString();
                    fdr[3] = DataConverter.CStr(dr["FieldAlias"]);
                    if (dr["FieldType"].ToString() == "DoubleDateType")
                    {
                        DateTime ti1 = DataConverter.CDate(req.Form["txt" + dr["FieldName"].ToString()]);
                        DateTime ti2 = DataConverter.CDate(req.Form["txt_" + dr["FieldName"].ToString()]);
                        fdr[2] = ti1.ToShortDateString() + "|" + ti2.ToShortDateString();
                    }
                    else
                    {
                        if (DataConverter.CBool(dr["IsNotNull"].ToString()) && flag)
                        {
                            if (string.IsNullOrEmpty(req.Form["txt_" + dr["FieldName"].ToString()]))
                            {
                                throw new Exception("[" + dr["FieldAlias"] + "]不能为空!");
                            }
                        }
                        string[] Sett = dr["Content"].ToString().Split(new char[] { ',' });
                        switch (dr["FieldType"].ToString())
                        {
                            case "FileType"://其与MultiPicType一样，都是读页面上的隐藏控件,而非<select>中的值,存在一个隐藏字段中
                                #region
                                //ChkFileSize=0,FileSizeField=,MaxFileSize=1024,UploadFileExt=rar|zip|docx|pdf:::是否保存文件大小,保存在大小的字段名,文件上传限制,上传后缀名限制
                                bool chksize = DataConverter.CBool(Sett[0].Split(new char[] { '=' })[1]);
                                string sizefield = Sett[1].Split(new char[] { '=' })[1];
                                //if (chksize && sizefield != "")
                                //{
                                //    fdr[0] = sizefield;
                                //    fdr[1] = "FileSize";
                                //    fdr[2] = req.Form["txt_" + sizefield];
                                //    fdr[3] = dr["FieldAlias"];
                                //}
                                //信息由JS控制放置入txt_中
                                fdr[2] = req.Form["txt_" + dr["FieldName"]];
                                #endregion
                                break;
                            case "MultiPicType":
                                #region
                                //ChkThumb=0,ThumbField=,Warter=0,MaxPicSize=1024,PicFileExt=jpg|png|gif|bmp|jpeg:::是否保存缩略图地址,缩略点路径,水印,大小,后缀名限制
                                bool chkthumb = DataConverter.CBool(Sett[0].Split(new char[] { '=' })[1]);
                                string ThumbField = Sett[1].Split(new char[] { '=' })[1];
                                if (chkthumb && ThumbField != "")//存储缩图
                                {
                                    DataRow row2 = table.NewRow();
                                    if (DataConverter.CBool(dr["IsNotNull"].ToString()) && string.IsNullOrEmpty(req.Form["txt_" + ThumbField]))
                                    {
                                        throw new Exception(dr["FieldAlias"].ToString() + "的缩略图不能为空!");
                                    }
                                    row2[0] = ThumbField;
                                    row2[1] = "ThumbField";
                                    row2[2] = req.Form["txt_" + ThumbField];
                                    row2[3] = dr["FieldAlias"];
                                    table.Rows.Add(row2);
                                }
                                {
                                    fdr[0] = dr["FieldName"];
                                    fdr[1] = "MultiPicType";
                                    fdr[2] = req.Form["txt_" + dr["FieldName"]];
                                    fdr[3] = dr["FieldAlias"];
                                }
                                #endregion
                                break;
                            case "TableField"://库选字段
                                string tableval = req.Form["txt_" + dr["FieldName"].ToString()];
                                if (!string.IsNullOrEmpty(tableval))
                                {
                                    string[] temparr = tableval.Split(',');
                                    tableval = string.Join("$", temparr);
                                }
                                fdr[2] = tableval;
                                break;
                            case "NumType":
                                #region
                                double value = DataConverter.CDouble(string.IsNullOrEmpty(GetParam(req,"txt_" + dr["FieldName"])) ? "0" : GetParam(req, "txt_" + dr["FieldName"]));//避免空字符串报错
                                                                                                                                                                          //TitleSize=35,NumberType=1,DefaultValue=50,NumSearchType=0,NumRangType=1,NumSearchRang=0-100,NumLenght=0
                                string[] conArr = dr["Content"].ToString().Split(',');
                                string NumLenght = "f" + conArr[6].Split('=')[1];//小数点位数
                                if (conArr[4].Split('=')[1].Equals("1"))//指定了上限与下限检测,则检测
                                {
                                    string range = conArr[5].Split('=')[1];
                                    if (!string.IsNullOrEmpty(range) && range.Split('-').Length == 2)//范围不为空，并正确填了值，才做判断
                                    {
                                        double minValue = Convert.ToDouble(range.Split('-')[0]);
                                        double maxValue = Convert.ToDouble(range.Split('-')[1]);
                                        if (value < minValue || value > maxValue)
                                        {
                                            throw new Exception(dr["FieldAlias"] + "：的值不正确,必须大于" + minValue + ",并且小于" + maxValue);
                                        }
                                    }
                                }
                                fdr[2] = value.ToString(NumLenght);
                                #endregion
                                break;
                            case "Images"://组图
                                fdr[2] = (GetParam(req, "txt_" + dr["FieldName"].ToString()) ?? "").Trim(',');
                                break;
                            case "CameraType":
                                fdr[2] = req.Form[dr["FieldName"] + "_hid"].ToString();
                                break;
                            case "SqlType"://文件入库,图片入库,,,图片与文件base64存数据库，一个单独字段标识其
                                {
                                    fdr[2] = req.Form["FIT_" + dr["FieldName"]];
                                }
                                break;
                            case "SqlFile":
                                {
                                    fdr[2] = req.Form["FIT_" + dr["FieldName"]];
                                }
                                break;
                            case "MapType":
                                {
                                    fdr[2] = req.Form["txt_" + dr["FieldName"].ToString()];
                                }
                                break;
                            case "autothumb":
                                {
                                    FieldModel fieldMod = new FieldModel(dr["Content"].ToString());
                                    string thumb = req.Form["txt_" + dr["FieldName"].ToString()];
                                    string topimg = req.Form["ThumImg_Hid"];
                                    //如thumb为空,但topimg不为空,则生成缩图
                                    fdr[0] = dr["FieldName"].ToString();
                                    fdr[1] = "auththumb";
                                    fdr[2] = "";
                                }
                                break;
                            case "api_qq_mvs":
                                {
                                    fdr[2] = req.Form["UpMvs_" + dr["FieldName"].ToString()];
                                }
                                break;
                            case "PicType":
                                {
                                    string fvalue = DataConverter.CStr(req.Form[dr["FieldName"] + "_t"]);
                                    fdr[2] = fvalue.Replace(SiteConfig.SiteOption.UploadDir, "");
                                }
                                break;
                            default:
                                #region
                                {
                                    string fvalue = req.Form["txt_" + dr["FieldName"].ToString()];
                                    switch (dr["FieldType"].ToString())
                                    {
                                        //case "Random":
                                        //    fvalue = req.Form["txt_" + dr["FieldName"].ToString()];
                                        //    break;
                                        case "MapType":
                                            //fvalue =;
                                            break;
                                        case "Charts":
                                            fvalue = "0";
                                            break;
                                        case "SqlType":
                                            fvalue = "";
                                            break;
                                        case "SqlFile":
                                            fvalue = "";
                                            break;
                                    }
                                    fdr[2] = fvalue;
                                }
                                #endregion
                                break;
                        }//for end;
                    }//else end;
                }
                catch (Exception ex)
                {
                    if (!ex.Message.Contains("正在中止线程"))
                    {
                        throw new Exception("[" + dr["FieldAlias"] + "][" + dr["FieldName"] + "(" + dr["FieldType"] + ")],原因:" + ex.Message);
                    }
                }
                fdr[2] = StringHelper.Strip_Script(DataConverter.CStr(fdr[2]));
                table.Rows.Add(fdr);
            }//for end;
            return table;
        }
        public static string GetParam(HttpRequest Request, string name)
        {
            string value = "";
            Microsoft.Extensions.Primitives.StringValues temp = new Microsoft.Extensions.Primitives.StringValues();
            if (Request.Form.TryGetValue(name, out temp)) { value = temp.ToString(); }
            return value;
        }
        public static string PathAdmin(string path)
        {
            path = path.TrimStart('/');
            if (path.Contains("")) { return CustomerPageAction.customPath2 + path; }
            else { return "/Admin/" + path; }
        }
        //[c]
        public static string GetLabel(string ntext)
        {
            return "";
            //return new B_CreateHtml().CreateHtml(ntext);
        }
        public static IHtmlContent MVCLabel(string ntext)
        {
            return MvcHtmlString.Create("");
            //return MvcHtmlString.Create(new B_CreateHtml().CreateHtml(ntext));
        }
        public static IHtmlContent MVCLabel(HttpContext ctx, string ntext)
        {
            return MvcHtmlString.Create(new B_CreateHtml(ctx).CreateHtml(ntext));
        }
        public static string ShowASCX(string path,string name="") { return ""; }
        public static string GetHelp(int id)
        {
            //if (SiteConfig.SiteOption.IsOpenHelp == "1")
            //    return "<div id='help' class='pull-right'><a onclick=\"help_show('" + id + "')\" title='帮助'><i class='zi zi_questioncircle'></i></a></div>";
            //else
                return "";
        }

        //----------编辑器
        public static string GetUEditor(string txtid, int status = 1, string theme = "")
        {
            string result = "";
            if (!string.IsNullOrEmpty(theme)) { theme = "theme:'" + theme + "',"; }
            string script = "";
            script += "<script>";
            script += @"UE.getEditor('" + txtid + "', {" + theme + "{0}});";
            script += @"</script>";
            switch (status)
            {
                case 1:
                    result = script.Replace("{0}", BLLCommon.ueditorMin.ToString());
                    break;
                case 2:
                    result = script.Replace("{0}", BLLCommon.ueditorMid.ToString());
                    break;
                case 3:
                    result = script.Replace("{0}", "");
                    break;
                case 4://用于贴吧
                    result = script.Replace("{0}", BLLCommon.ueditorBar.ToString());
                    break;
                case 5://用于聊天等
                    result = script.Replace("{0}", BLLCommon.ueditorNom.ToString());
                    break;
            }
            return result;
        }
    }
}
