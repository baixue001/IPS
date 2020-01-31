using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ZoomLa.BLL;
using ZoomLa.BLL.API;
using ZoomLa.BLL.Content;
using ZoomLa.BLL.Helper;
using ZoomLa.BLL.SYS;
using ZoomLa.Common;
using ZoomLa.Components;
using ZoomLa.Model;
using ZoomLa.Safe;
using ZoomLa.SQLDAL;
using ZoomLa.SQLDAL.SQL;
using ZoomLaCMS.Areas.Admin.Models;
using ZoomLaCMS.Ctrl;

namespace ZoomLaCMS.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Policy = "Admin")]
    public class TemplateController : Ctrl_Admin
    {
        B_Label labelBll = new B_Label();
        public string LabelName { get { return GetParam("labelName"); } }
        #region 标签管理
        public IActionResult LabelManage()
        {
            PSize = 20;
            string LabelCate = GetParam("Cate");
            string KeyWord = GetParam("KeyWord");
            //-----------------
            //KeyWord = RequestEx["KeyWord"] ?? "";
            DataTable dt = labelBll.SelAllLabel(LabelCate, KeyWord);
            PageSetting setting = PageHelper.GetPagedTable(dt, CPage, PSize);

            if (Request.IsAjax())
            {
                return PartialView("LabelManage_List", setting);
            }
            else
            {
                DataTable CateTable = labelBll.GetLabelCateListXML();//标签类别
                CateTable.Columns.Add("label");
                DataRow allrow = CateTable.NewRow();
                allrow["name"] = "";
                allrow["label"] = "全部标签";
                CateTable.Rows.InsertAt(allrow, 0);
                ViewBag.CateTable = JsonConvert.SerializeObject(CateTable);
                return View(setting);
            }
        }
        public IActionResult LabelSql()
        {
            VM_Label model = new VM_Label();
            if (!string.IsNullOrEmpty(LabelName))
            {
                model.labelMod = labelBll.GetLabelXML(LabelName);
                if (string.IsNullOrEmpty(model.labelMod.DataSourceType) || model.labelMod.DataSourceType.Equals("{}"))
                { model.labelMod.DataSourceType = "{\"ds_m\":\"main\",\"ds_s\":\"main\",\"tb_m\":\"\",\"tb_s\":\"\"}"; }
                model.labelMod.LabelWhere = ClearTableHolder(model.labelMod.LabelWhere);
                model.labelMod.LabelField = ClearTableHolder(model.labelMod.LabelField);
                model.labelMod.LabelTable = ClearTableHolder(model.labelMod.LabelTable);
                model.labelMod.LabelOrder = ClearTableHolder(model.labelMod.LabelOrder);
            }
            return View(model);
        }
        public IActionResult LabelSql_Save()
        {
            M_APIResult retMod = new M_APIResult(Failed);
            string name = DataConvert.CStr(RequestEx["LabelName_T"]).Replace(" ", "");
            if (string.IsNullOrEmpty(name)) { retMod.retmsg = "标签名不能为空"; return Content(retMod.ToString()); }
            //if (string.IsNullOrEmpty(SqlTable_T.Text)) { Script("alert('查询表不能为空!');");  }
            M_Label labelMod = new M_Label();
            if (!string.IsNullOrEmpty(LabelName)) { labelMod = labelBll.GetLabelXML(LabelName); }
            if (!LabelName.ToLower().Equals(name.ToLower()))
            {
                labelBll.CheckLabelXML(name);
            }
            labelMod.LabelName = name;
            labelMod.LabelType = DataConverter.CLng(RequestEx["LabelType_Rad"]);
            labelMod.LabelCate = RequestEx["LabelCate_T"];
            labelMod.Desc = RequestEx["Desc_T"];
            labelMod.LabelTable = RequestEx["SqlTable_T"];
            string sqlFieldStr = RequestEx["SqlField_T"];
            labelMod.LabelField = string.IsNullOrEmpty(sqlFieldStr) ? "*" : sqlFieldStr;
            labelMod.Param = RequestEx["Param_Hid"];
            labelMod.LabelWhere = RequestEx["Where_T"];
            labelMod.LabelOrder = RequestEx["Order_T"];
            labelMod.LabelCount = DataConvert.CLng(RequestEx["PSize_T"], 10).ToString();
            //不支持跨数据源联结查询
            //JObject jobj = new JObject();
            //jobj.Add("ds_m", D1); jobj.Add("ds_s", D2); jobj.Add("tb_m", T1); jobj.Add("tb_s", T2);
            labelMod.DataSourceType = RequestEx["ds_hid"];
            //---------------------
            labelMod.Content = RequestEx["Content_T"];
            labelMod.FalseContent = RequestEx["FalseContent"];
            labelMod.EmptyContent = RequestEx["EmptyContent"];
            //判断模式
            labelMod.IsOpen = DataConvert.CLng(RequestEx["BoolMode_Chk"]);
            labelMod.setroot = RequestEx["setroot"];
            labelMod.Valueroot = RequestEx["Valueroot"];
            labelMod.Modeltypeinfo = RequestEx["Modeltypeinfo"];
            labelMod.Modelvalue = RequestEx["Modelvalue"];
            if (!string.IsNullOrEmpty(LabelName))
            {
                //如果修改了名称
                if (!labelMod.LabelName.ToLower().Equals(LabelName.ToLower()))
                {
                    labelBll.DelLabelXML(LabelName);
                    labelBll.AddLabelXML(labelMod);
                }
                else
                {
                    labelBll.UpdateLabelXML(labelMod);
                }
            }
            else
            {
                labelMod.LabelNodeID = 0;
                labelBll.AddLabelXML(labelMod);
            }
            retMod.retcode = Success;
            return Content(retMod.ToString());
        }
        private string ClearTableHolder(string text)
        {
            text = text.Replace("{table1}.dbo.", "").Replace("{table2}.dbo.", "");
            return text;
        }
        public IActionResult LabelCallTab()
        {
            M_Label labelMod = new M_Label();
            if (!string.IsNullOrEmpty(LabelName))
            {
                labelMod = labelBll.GetLabelXML(LabelName);
            }
            return View(labelMod);
            // B_ARoleAuth.AuthCheckEx(ZLEnum.Auth.system, "label");
            //CustomLabel_DP.DataSource = labelBll.GetLabelCateListXML();
            //CustomLabel_DP.DataTextField = "name";
            //CustomLabel_DP.DataValueField = "name";
            //CustomLabel_DP.DataBind();
            //CustomLabel_DP.Items.Insert(0, new ListItem("选择标签类型", ""));
            //Field_DP.DataSource = labelBll.GetSourceLabelXML();//LabelType
            //Field_DP.DataTextField = "LabelName";
            //Field_DP.DataValueField = "LabelID";
            //Field_DP.DataBind();
            //Field_DP.Items.Insert(0, new ListItem("选择数据源标签", ""));
            //if (!string.IsNullOrEmpty(LabelName))
            //{
            //    DealInvoke();

            //}
            //return View();
        }
        public IActionResult LabelInsert()
        {
            string LName = HttpUtility.UrlDecode(GetParam("n"));
            if (string.IsNullOrEmpty(LName)) { return WriteErr("未指定标签名"); }
            M_Label labelMod = labelBll.GetLabelXML(LName);
            if (labelMod.IsNull) { return WriteErr("[" + LName + "]不存在"); }
            if (labelMod.LabelType == 3) { labelMod.LabelType = 1; }

            //if (string.IsNullOrEmpty(labelMod.Param)) { function.Script(this, "submitdate();");  }
            //创建table
            StringBuilder builder = new StringBuilder();
            builder.Append("<table class='table table-bordered table-striped'>");
            int ptype = 0;
            string aname = "";
            string avalue = "";
            string aintro = "";
            //分割参数
            string[] pa = labelMod.Param.Split(new char[] { '|' });
            for (int i = 0; i < pa.Length; i++)
            {
                //pageid,默认值,2,参数说明
                ptype = DataConverter.CLng(pa[i].Split(',')[2]);
                if (ptype == 1)
                {
                    aname = pa[i].Split(new char[] { ',' })[0];
                    avalue = pa[i].Split(new char[] { ',' })[1];
                    aintro = pa[i].Split(new char[] { ',' })[3];
                    builder.Append("<tr><td class='text-right td_l'><SPAN sid=\"" + aname + "\" stype=\"0\" title=\"" + aname + "\">" + aintro + "</SPAN>：</td><td class='text-left'>");
                    builder.Append("<input type=\"text\" id=\"" + aname + "\" value=\"" + avalue + "\"/></td></tr>");
                }
                else if (ptype == 2) { }//页面参数不需要处理
                else if (ptype == 3)//单选
                {
                    aname = pa[i].Split(new char[] { ',' })[0];
                    avalue = pa[i].Split(new char[] { ',' })[1];
                    aintro = pa[i].Split(new char[] { ',' })[3];
                    builder.Append("<tr><td class='text-right td_l'><SPAN sid=\"" + aname + "\" stype=\"0\" title=\"" + aname + "\">" + aintro + "</SPAN>：</td><td align=\"left\">");
                    builder.Append("<select id=\"" + aname + "\" style=\"width:156px;\">");
                    string[] item = avalue.Split('$');
                    foreach (string iten in item)
                    {
                        builder.Append("<option value=\"" + iten + "\">" + iten + "</option>");
                    }
                    builder.Append("</select></td></tr>");

                }
                else if (ptype == 4)//多选
                {
                    aname = pa[i].Split(new char[] { ',' })[0];
                    avalue = pa[i].Split(new char[] { ',' })[1];
                    aintro = pa[i].Split(new char[] { ',' })[3];
                    builder.Append("<tr><td class='text-right td_l'><SPAN sid=\"" + aname + "\" stype=\"1\" title=\"" + aname + "\">" + aintro + "</SPAN>：</td><td align=\"left\">");
                    builder.Append("<input id=\"h" + aname + "\" type=\"hidden\" />");
                    builder.Append("<div id=\"d" + aname + "\" style=\"display:block;\">");
                    string[] items = avalue.Split('$');
                    foreach (string itens in items)
                    {
                        builder.Append("<input type=\"checkbox\" name=\"c" + aname + "\" onclick=\"selectchecked(this)\" value=\"" + itens + "\" />" + itens + "</br>");
                    }
                    builder.Append("</div></td></tr>");
                }
            }
            //强设为无参数,前台直接执行
            if (!builder.ToString().Contains("<tr>")) { labelMod.Param = ""; }
            builder.Append("</table>");
            ViewBag.html = builder.ToString();
            //this.labelbody.Text = builder.ToString();
            //this.labelintro.Text = labelMod.Desc;
            return View(labelMod);
        }
        //标签列表管理
        public string Label_API(string action, string name)
        {
            switch (action)
            {
                case "copy":
                    {
                        M_Label newlbl = labelBll.GetLabelXML(name);
                        newlbl.LabelName = newlbl.LabelName + function.GetRandomString(4, 2);
                        newlbl.LabelID = 0;
                        labelBll.AddLabelXML(newlbl);
                    }
                    break;
                case "del":
                    {
                        labelBll.DelLabelXML(name);
                    }
                    break;
                case "down"://下载 
                    {

                    }
                    break;
            }
            return Success.ToString();
        }
        public string LabelSql_API()
        {
            M_APIResult retMod = new M_APIResult(M_APIResult.Success);
            string action = Request.Query["action"];
            try
            {
                switch (action)
                {
                    case "tables"://根据数据库获取表信息
                        {

                        }
                        break;
                    case "fields"://根据当前选中的表,获取字段信息
                        {
                            string dsname = RequestEx["dsname"];
                            string tbname = RequestEx["tbname"];
                            SqlBase db = B_DataSource.GetDSByType(dsname);
                            DataTable dt = db.Field_List(tbname);
                            dt.DefaultView.Sort = "Name";
                            retMod.result = JsonConvert.SerializeObject(dt);
                        }
                        break;
                }
            }
            catch (Exception ex) { retMod.retmsg = ex.Message; retMod.retcode = M_APIResult.Failed; }
            return retMod.ToString();
        }
        #endregion
        /// <summary>
        /// 需要修改的模板html文件,不包含/Template/V3等模板目录==店铺模板/店铺介绍页.html
        /// </summary>
        public string FilePath { get { return HttpUtility.UrlDecode(RequestEx["FilePath"]); } }
        //要修改的模板根目录(只允许修改Template目录下的html文件,为空则使用默认值),
        public string SetTemplate
        {
            get
            {
                string tlpdir = HttpUtility.UrlDecode(RequestEx["SetTemplate"] ?? "");
                if (string.IsNullOrEmpty(tlpdir)) { tlpdir = SiteConfig.SiteOption.TemplateDir; }
                tlpdir = tlpdir.Replace("\\", "/").TrimEnd('/') + "/";
                return tlpdir;
            }
        }
        /// <summary>
        /// 当前文件所在的dir==/template/v4/店铺模板/
        /// </summary>
        public string FileDir
        {
            get
            {
                string tlpdir = SetTemplate;
                if (string.IsNullOrEmpty(Path.GetExtension(FilePath)))//单目录跳转,不带后缀名,不带/
                {
                    tlpdir += FilePath + "/";
                }
                else if (FilePath.Contains("/"))//带目录的路径文件路径,取最后一个/位置之间的字符
                {
                    if (FilePath.LastIndexOf("/") != 0)//避免  /全站首页.html
                    {
                        tlpdir += FilePath.Substring(0, FilePath.LastIndexOf("/")) + "/";
                    }
                }
                else//带后缀名的单文件跳转,不需处理
                {

                }
                tlpdir = tlpdir.Replace("\\", "/").TrimEnd('/') + "/";
                while (tlpdir.Contains("//"))
                {
                    tlpdir = tlpdir.Replace("//", "/");
                }
                return tlpdir;
            }
        }
        //合并的文件根路径(虚拟)
        public string RealVPath
        {
            get
            {
                return SafeSC.PathDeal((SetTemplate.TrimEnd('/') + "/" + FilePath.TrimStart('/')).Replace("//", "/"));
            }
        }
        public IActionResult TemplateManage()
        {
            return View();
        }
        public IActionResult TemplateEdit()
        {
            string[] allowExt = ".html|.htm|.shtml|.txt|.config|.xml|.css|.js".Split('|');
            if (string.IsNullOrEmpty(RealVPath)) { return WriteErr("未指定需要修改的html"); }
            string ext = Path.GetExtension(RealVPath);
            //FolderName_L.Text = FileDir;
            if (string.IsNullOrEmpty(ext))
            {

            }
            else
            {
                if (!allowExt.Contains(Path.GetExtension(RealVPath).ToLower())) { return WriteErr("无权修改除html之外的文件"); }
            }
            ViewBag.FileDir = FileDir;
            ViewBag.RealVPath = RealVPath;
            return View();
        }

        //弹窗选择模板
        public IActionResult TemplateList()
        {
            return View();
        }
        [HttpPost]
        public IActionResult TemplateEdit_Submit()
        {
            if (string.IsNullOrEmpty(GetParam("FileName_T"))) { return WriteErr("未指定文件名称"); }
            string oldfname = Path.GetFileNameWithoutExtension(RealVPath);
            string newfname = GetParam("FileName_T");
            if (SafeSC.FileNameCheck(newfname)) { return WriteErr("文件名或后缀名不符合规范,不可包含特殊字符"); }
            if (string.IsNullOrEmpty(Path.GetExtension(RealVPath)))
            {
                SafeSC.WriteFile(FileDir + newfname + ".html", GetParam("textContent"));
            }
            else
            {
                SafeSC.WriteFile(RealVPath, GetParam("textContent"));
            }
            return WriteOK("保存成功");
        }
        private string CSSBaseDir { get { return SiteConfig.SiteOption.CssDir.TrimEnd('/') + "/"; } }
        public IActionResult CSSManage()
        {
            if (Request.IsAjax())
            {
                M_APIResult ret = new M_APIResult(M_APIResult.Failed);
                B_ARoleAuth.AuthCheckEx(ZLEnum.Auth.system, "tlp");
                try
                {
                    string action = GetParam("action");
                    string vpath = Request.Form["vpath"];
                    if (string.IsNullOrEmpty(vpath)) { throw new Exception("未指定文件"); }
                    switch (action)
                    {
                        case "del":
                            {
                                SafeSC.DelFile(CSSBaseDir + vpath);
                                ret.retcode = M_APIResult.Success;
                            }
                            break;
                        case "copy":
                            {
                                string src = function.VToP(CSSBaseDir + vpath);
                                string tar = function.VToP((CSSBaseDir + vpath).Replace(".", "_" + function.GetRandomString(3) + "."));
                                IOHelper.File_Copy(src, tar);
                                ret.retcode = M_APIResult.Success;
                            }
                            break;
                        case "down":
                            {

                            }
                            break;
                        default:
                            ret.retmsg = "[" + action + "]不存在";
                            break;
                    }
                }
                catch (Exception ex) { ret.retmsg = ex.Message; }
                return Content(ret.ToString());
            }
            ViewBag.BaseDir = CSSBaseDir;
            return View();
        }
        public IActionResult CSSEdit()
        {
            B_Admin badmin = new B_Admin();
            B_ARoleAuth.AuthCheckEx(ZLEnum.Auth.system, "tlp");
            //--------------------------------
            if (!string.IsNullOrEmpty(FilePath))
            {
                string CSSPath = CSSBaseDir + FilePath;
                string fileExt = Path.GetExtension(CSSPath);//.css
                string fileName = Path.GetFileName(CSSPath);//mobile.css

                if (fileExt.ToLower().Equals(".css"))
                {
                    //TxtFilename.Attributes.Add("disabled", "disabled");
                    //TxtFilename.Value = fileName.Split('.')[0];
                }
                else { return WriteErr("无权修改.css以外的文件！"); }
                ViewBag.fcontent = SafeSC.ReadFileStr(CSSPath);
                ViewBag.fname = fileName;
            }
            ViewBag.FilePath = FilePath;
            return View();
        }
        public IActionResult CSSEdit_Submit()
        {
            string CSSPath = "";
            if (!string.IsNullOrEmpty(FilePath))
            {
                CSSPath = CSSBaseDir + FilePath.Split('.')[0] + ".css";
            }
            else
            {
                string fname = GetParam("TxtFilename").Replace(" ", "").Split('.')[0];
                if (SafeC.FileNameCheck(fname)) { return WriteErr("文件名不符合规则"); }
                CSSPath = CSSBaseDir + fname + ".css";
            }
            SafeSC.WriteFile(CSSPath, GetParam("textContent"));
            return WriteOK("操作成功", "CSSManage");
        }
    }
}