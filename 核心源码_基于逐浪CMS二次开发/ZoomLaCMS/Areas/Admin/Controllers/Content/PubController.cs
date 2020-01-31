using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ZoomLa.BLL;
using ZoomLa.BLL.Content;
using ZoomLa.BLL.Helper;
using ZoomLa.Common;
using ZoomLa.Components;
using ZoomLa.Model;
using ZoomLa.SQLDAL;
using ZoomLa.SQLDAL.SQL;
using ZoomLaCMS.Ctrl;

namespace ZoomLaCMS.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Policy = "Admin")]
    public class PubController : Ctrl_Admin
    {
        B_Pub pubBll = new B_Pub();
        B_Model modBll = new B_Model();
        B_ModelField fieldBll = new B_ModelField();
        public IActionResult PubManage()
        {
            PageSetting setting = pubBll.SelPage(CPage, PSize, new F_Pub()
            {

            });
            if (Request.IsAjaxRequest())
            {
                return PartialView("PubManage_List", setting);
            }
            return View(setting);
        }
        public ContentResult Pub_API()
        {
            string action = RequestEx["action"];
            string ids = RequestEx["ids"];
            switch (action)
            {
                case "del":
                    {
                        pubBll.DelByIDS(ids);
                    }
                    break;
                case "copy":
                    {
                        int id = DataConvert.CLng(ids);
                        if (id < 1) { return Content(Failed.ToString()); }
                        M_Pub pubMod = pubBll.SelReturnModel(id);
                        pubMod.Pubid = 0;
                        pubMod.PubCreateTime = DateTime.Now;
                        pubMod.PubName = pubMod.PubName + "_copy";
                        pubBll.insert(pubMod);
                    }
                    break;
                case "modelname"://模型名ajax检测
                    {
                        string name = DataConverter.CStr(Request.Form["value"]);
                        string result = DBHelper.Table_IsExist(B_Pub.PREFIX + name).ToString().ToLower();
                        return Content(result);
                        //return "<font color=blue>数据表已存在! 可重复使用!</font>";
                        //return "<font color=green>数据表不存在，系统将自动创建!</font>";
                    }
                case "recover":
                    {
                        pubBll.RecyleByIDS(ids, 1);
                    }
                    break;
            }
            return Content(Success.ToString());
        }
        public void Pubinfo()
        {
            Response.Redirect("PubAdd?id=" + Mid);
        }
        public IActionResult PubAdd()
        {
            ViewBag.modelDT = modBll.GetListPub();
            M_Pub pubMod = new M_Pub();
            if (Mid > 0)
            {
                pubMod = pubBll.SelReturnModel(Mid);
                pubMod.PubTableName = pubMod.PubTableName;
            }
            else
            {

            }
            return View(pubMod);
        }
        [HttpPost]
        public IActionResult PubAdd_Submit()
        {
            M_Pub pubMod = new M_Pub();
            if (Mid > 0) { pubMod = pubBll.SelReturnModel(Mid); }
            pubMod.PubName = GetParam("PubName");
            bool addtrue = true;
            #region 验证模块
            if (Mid < 1)
            {
                pubMod.PubLoadstr = GetParam("PubLoadstr");
                pubMod.PubInputLoadStr = GetParam("PubInputLoadStr");
                pubMod.PubTableName = B_Pub.PREFIX + GetParam("PubTableName");
                DataTable tempinfo = pubBll.SelByName(pubMod.PubName);
                if (tempinfo.Rows.Count > 0)
                {
                    addtrue = false;
                    return WriteErr("已存在此互动模块!请更换模块名称再试!!", "javascript:history.back(-2);");
                }
                DataTable PubInputLoadStrtable = pubBll.SelBy("", "PubInputLoadStr.Text", Mid.ToString());
                if (PubInputLoadStrtable.Rows.Count > 0)
                {
                    addtrue = false;
                    return WriteErr("已经存在此提交标签!");
                }
                DataTable PubLoadstrtable = pubBll.SelBy(pubMod.PubLoadstr, "", Mid.ToString());
                if (PubLoadstrtable.Rows.Count > 0)
                {
                    addtrue = false;
                    return WriteErr("已经存在此互动标签!");
                }
            }
            #endregion
            if (addtrue)
            {
                pubMod.PubClass = DataConverter.CLng(GetParam("PubClass"));
                pubMod.PubCode = DataConvert.CLng(GetParam("PubCode"));
                //界面处理显示结束时间,如果是最大则不显示
                if (!string.IsNullOrEmpty(GetParam("PubEndTime")))
                {
                    pubMod.PubEndTime = DataConverter.CDate(GetParam("PubEndTime").Replace("/", "-"));
                }
                else
                {
                    pubMod.PubEndTime = DateTime.MaxValue;
                }
                pubMod.PubType = DataConverter.CLng(GetParam("PubType"));
                pubMod.PubIsTrue = DataConvert.CLng(GetParam("PubIsTrue"));
                pubMod.PubLogin = DataConvert.CLng(GetParam("PubLogin"));
                //pubMod.PubLoginUrl = PubLoginUrl.Text;
                pubMod.PubOpenComment = DataConvert.CLng(GetParam("PubOpenComment"));
                pubMod.PubShowType = DataConverter.CLng(GetParam("PubShowType"));
                pubMod.Pubkeep = DataConverter.CLng(GetParam("Pubkeep"));
                pubMod.Puberrmsg = Request.Form["Puberrmsg"];
                pubMod.PubPermissions = GetParam("PubPermissions");
                pubMod.PubGourl = GetParam("PubGourl");
                pubMod.Public = DataConvert.CLng(GetParam("RaPublic"));
                pubMod.VerifyMode = Request.Form["verify_chk"];
                pubMod.PubIPOneOrMore = DataConverter.CLng(GetParam("PubIPOneOrMore"));
                pubMod.Interval = DataConverter.CLng(GetParam("Interval_T"));
                pubMod.Pubinfo = GetParam("Pubinfo");
                string PubInputTMurl = "";
                string Tempinputstr = @"";
                //模型ID
                if (pubMod.Pubid == 0)
                {
                    string saveDir = function.VToP(SiteConfig.SiteOption.TemplateDir + "/互动模板/");
                    string saveName = "";

                    pubMod.PubModelID = pubBll.CreateModelInfo(pubMod);//建立数据表与模型Field,Model
                    int Pubinsertid = pubBll.GetInsert(pubMod);

                    #region 创建互动引用模板
                    string PubTemplateurl = "";//模板路径
                    string Tmpstr = "";//引用标签
                    M_Label lab = new M_Label();
                    B_Label blab = new B_Label();
                    lab.LabelAddUser = adminMod.AdminId;

                    string pubcontdid = "@ID";
                    lab.Param = "ID,0,2,";

                    //创建标签
                    switch (pubMod.PubType)
                    {
                        case 0://评论
                               //创建源标签
                            lab.LabelName = pubMod.PubName + "调用标签";
                            lab.LabelCate = "互动标签";
                            lab.LabelType = 4;
                            lab.LabelTable = "" + pubMod.PubTableName + " left join ZL_Pub on " + pubMod.PubTableName + ".Pubupid=ZL_Pub.Pubid";
                            lab.LabelField = "" + pubMod.PubTableName + ".*,ZL_Pub.*";
                            lab.LabelWhere = "" + pubMod.PubTableName + ".Pubupid=" + Pubinsertid.ToString() + " And " + pubMod.PubTableName + ".PubContentid=" + pubcontdid + " And " + pubMod.PubTableName + ".Pubstart=1";
                            lab.LabelOrder = "" + pubMod.PubTableName + ".ID DESC";
                            lab.LabelCount = "10";
                            lab.Content = "{Repeate}\n用户名:{Field=\"PubUserName\"/}<br />\n评论说明:{Field=\"PubContent\"/}<br />\n用户IP:{Field=\"PubIP\"/}<br />\n评论时间:{Field=\"PubAddTime\"/}<br />{/Repeate}<br />\n{ZL.Page/}";
                            lab.Desc = pubMod.PubTableName + "分页标签";
                            lab.LabelNodeID = 0;
                            blab.AddLabelXML(lab);
                            Tmpstr = "{ZL.Label id=\"" + pubMod.PubName + "调用标签\"/}\n{Pub." + pubMod.PubInputLoadStr + "/}";
                            saveName = "默认评论" + pubMod.PubName + "模板.html";
                            break;
                        case 1://投票
                               //创建标签
                            lab.LabelName = pubMod.PubName + "调用标签";
                            lab.LabelCate = "互动标签";
                            lab.LabelType = 4;
                            lab.LabelTable = "" + pubMod.PubTableName + " left join ZL_Pub on " + pubMod.PubTableName + ".Pubupid=ZL_Pub.Pubid";
                            lab.LabelField = "" + pubMod.PubTableName + ".*,ZL_Pub.*";
                            lab.LabelWhere = "" + pubMod.PubTableName + ".Pubupid=" + Pubinsertid.ToString() + " And " + pubMod.PubTableName + ".PubContentid=" + pubcontdid + " And " + pubMod.PubTableName + ".Pubstart=1";
                            lab.LabelOrder = "" + pubMod.PubTableName + ".ID DESC";
                            lab.LabelCount = "10";
                            lab.Content = "{Repeate}\n用户名:{Field=\"PubUserName\"/}<br />\n投票说明:{Field=\"PubContent\"/}<br />\n用户IP:{Field=\"PubIP\"/}<br />\n投票时间:{Field=\"PubAddTime\"/}<br />{/Repeate}<br />\n{ZL.Page/}";
                            lab.Desc = pubMod.PubTableName + "分页标签";
                            lab.LabelNodeID = 0;
                            blab.AddLabelXML(lab);
                            Tmpstr = "{ZL.Label id=\"" + pubMod.PubName + "调用标签\"/}\n{Pub." + pubMod.PubInputLoadStr + "/}";
                            saveName = "默认投票" + pubMod.PubName + "模板.html";
                            break;
                        case 2://活动
                            lab.LabelName = pubMod.PubName + "调用标签";
                            lab.LabelCate = "互动标签";
                            lab.LabelType = 4;
                            lab.LabelTable = "" + pubMod.PubTableName + " left join ZL_Pub on " + pubMod.PubTableName + ".Pubupid=ZL_Pub.Pubid";
                            lab.LabelField = "" + pubMod.PubTableName + ".*,ZL_Pub.*";
                            lab.LabelWhere = "" + pubMod.PubTableName + ".Pubupid=" + Pubinsertid.ToString() + " And " + pubMod.PubTableName + ".PubContentid=" + pubcontdid + " And " + pubMod.PubTableName + ".Pubstart=1";
                            lab.LabelOrder = "" + pubMod.PubTableName + ".ID DESC";
                            lab.LabelCount = "10";
                            lab.Content = "<a href=PubAction?menu=hd&act=add&Pubid=" + Pubinsertid.ToString() + ">发起活动</a>\n{Repeate}\n用户名:{Field=\"PubUserName\"/}<br />\n活动内容:{Field=\"PubContent\"/}<br />\n用户IP:{Field=\"PubIP\"/}<br />\n提交时间:{Field=\"PubAddTime\"/}\n{/Repeate}<br />\n{ZL.Page/}";
                            lab.Desc = pubMod.PubTableName + "分页标签";
                            lab.LabelNodeID = 0;
                            blab.AddLabelXML(lab);
                            Tmpstr = "{ZL.Label id=\"" + pubMod.PubName + "调用标签\"/}\n{Pub." + pubMod.PubInputLoadStr + "/}";
                            saveName = "默认活动" + pubMod.PubName + "模板.html";
                            break;
                        case 3://留言
                            lab.LabelName = pubMod.PubName + "调用标签";
                            lab.LabelCate = "互动标签";
                            lab.LabelType = 4;
                            lab.LabelTable = "" + pubMod.PubTableName + " left join ZL_Pub on " + pubMod.PubTableName + ".Pubupid=ZL_Pub.Pubid";
                            lab.LabelField = "" + pubMod.PubTableName + ".*,ZL_Pub.*";
                            lab.LabelWhere = "" + pubMod.PubTableName + ".Pubupid=" + Pubinsertid.ToString() + " And " + pubMod.PubTableName + ".PubContentid=" + pubcontdid + " And " + pubMod.PubTableName + ".Pubstart=1";
                            lab.LabelOrder = "" + pubMod.PubTableName + ".ID DESC";
                            lab.LabelCount = "10";
                            lab.Content = "{Repeate}\n用户名:{Field=\"PubUserName\"/}<br />\n留言内容:{Field=\"PubContent\"/}<br />\n用户IP:{Field=\"PubIP\"/}<br />\n提交时间:{Field=\"PubAddTime\"/}\n{/Repeate}<br />\n{ZL.Page/}";
                            lab.Desc = pubMod.PubTableName + "分页标签";
                            lab.LabelNodeID = 0;
                            blab.AddLabelXML(lab);
                            Tmpstr = "{ZL.Label id=\"" + pubMod.PubName + "调用标签\"/}\n{Pub." + pubMod.PubInputLoadStr + "/}";
                            saveName = "默认留言" + pubMod.PubName + "模板.html";
                            break;
                        case 4://问券
                            lab.LabelName = pubMod.PubName + "调用标签";
                            lab.LabelCate = "互动标签";
                            lab.LabelType = 4;
                            lab.LabelTable = "" + pubMod.PubTableName + " left join ZL_Pub on " + pubMod.PubTableName + ".Pubupid=ZL_Pub.Pubid";
                            lab.LabelField = "" + pubMod.PubTableName + ".*,ZL_Pub.*";
                            lab.LabelWhere = "" + pubMod.PubTableName + ".Pubupid=" + Pubinsertid.ToString() + " And " + pubMod.PubTableName + ".PubContentid=" + pubcontdid + " And " + pubMod.PubTableName + ".Pubstart=1";
                            lab.LabelOrder = "" + pubMod.PubTableName + ".ID DESC";
                            lab.LabelCount = "10";
                            lab.Content = "{Repeate}\n用户名:{Field=\"PubUserName\"/}<br />\n问券内容:{Field=\"PubContent\"/}<br />\n用户IP:{Field=\"PubIP\"/}<br />\n提交时间:{Field=\"PubAddTime\"/}\n{/Repeate}<br />\n{ZL.Page/}<br />";
                            lab.Desc = pubMod.PubTableName + "分页标签";
                            lab.LabelNodeID = 0;
                            blab.AddLabelXML(lab);
                            Tmpstr = "{ZL.Label id=\"" + pubMod.PubName + "调用标签\"/}\n{Pub." + pubMod.PubInputLoadStr + "/}";
                            saveName = "默认问券" + pubMod.PubName + "模板.html";
                            break;
                        case 5://统计
                            lab.LabelName = pubMod.PubName + "调用标签";
                            lab.LabelCate = "互动标签";
                            lab.LabelType = 2;
                            lab.LabelTable = pubMod.PubTableName + " left join ZL_Pub on " + pubMod.PubTableName + ".Pubupid=ZL_Pub.Pubid";
                            lab.LabelField = pubMod.PubTableName + ".*,ZL_Pub.*";
                            lab.LabelWhere = pubMod.PubTableName + ".Pubupid=" + Pubinsertid.ToString() + " And " + pubMod.PubTableName + ".PubContentid=" + pubcontdid + " and Parentid=0  And " + pubMod.PubTableName + ".Pubstart=1";
                            lab.LabelOrder = pubMod.PubTableName + ".ID DESC";
                            lab.LabelCount = "10";
                            lab.Content = "点击数:{Field=\"Pubnum\"/}";
                            lab.Desc = pubMod.PubTableName + "动态标签";
                            lab.LabelNodeID = 0;
                            blab.AddLabelXML(lab);
                            Tmpstr = "{ZL.Label id=\"" + pubMod.PubName + "调用标签\"/}\n{Pub." + pubMod.PubInputLoadStr + "/}";
                            saveName = "默认统计" + pubMod.PubName + "模板.html";
                            break;
                        case 6://竞标
                        case 7:
                        case 8:
                            break;
                        default:
                            return WriteErr("类型错误,该类型不存在!!!");
                            break;
                    }
                    if (!string.IsNullOrEmpty(saveName)) { FileSystemObject.WriteFile(saveDir + saveName, Tmpstr); }
                    PubTemplateurl = "/互动模板/" + saveName;
                    #endregion
                    #region 创建互动提交模板
                    switch (pubMod.PubType)
                    {
                        case 0://评论
                            Tempinputstr = @"<form name=""form{PubID/}"" method=""post"" action=""/PubAction"">
<input type=""hidden"" name=""PubID"" id=""PubID"" value=""{PubID/}"" />
<input type=""hidden"" name=""PubContentid"" id=""PubContentid"" value=""{PubContentid/}"" />
<input type=""hidden"" name=""PubInputer"" id=""PubInputer"" value=""{PubInputer/}"" />
<div class=""form-group"">
<label for=""PubTitle"">评论标题：</label>
<input type=""text"" class=""form-control"" id=""PubTitle"" name=""PubTitle"" />{PubCode/}
</div>
<div class=""form-group"">
<label for=""PubContent"">评论内容：</label>
<textarea class=""form-control"" name=""PubContent"" cols=""50"" rows=""10"" id=""PubContent""></textarea>
</div>
<div class=""form-group text-center"">
<button type=""submit"" class=""btn btn-default"">提交</button>
<button type=""reset"" class=""btn btn-default"">重置</button>
</div>
</form>";
                            Tempinputstr = Tempinputstr.Replace(@"{PubID/}", Pubinsertid.ToString());
                            saveName = "默认评论" + pubMod.PubName + "提交模板.html";
                            break;
                        case 1://投票
                            Tempinputstr = @"<form name=""form{PubID/}"" method=""post"" action=""/PubAction"">
<input type=""hidden"" name=""PubID"" id=""PubID"" value=""{PubID/}"" />
<input type=""hidden"" name=""PubContentid"" id=""PubContentid"" value=""{PubContentid/}"" />
<input type=""hidden"" name=""PubInputer"" id=""PubInputer"" value=""{PubInputer/}"" />
<div class=""form-group"">
<label for=""PubTitle"">投票标题：</label>
<input type=""text"" class=""form-control"" id=""PubTitle"" name=""PubTitle"" />
{PubCode/}
</div>
<div class=""form-group"">
<label for=""PubContent"">投票内容：</label>
<textarea class=""form-control"" name=""PubContent"" cols=""50"" rows=""10"" id=""PubContent""></textarea>
</div>
<div class=""form-group text-center"">
<button type=""submit"" class=""btn btn-default"">提交</button>
<button type=""reset"" class=""btn btn-default"">重置</button>
</div>
</form>";
                            Tempinputstr = Tempinputstr.Replace(@"{PubID/}", Pubinsertid.ToString());
                            saveName = "默认投票" + pubMod.PubName + "提交模板.html";
                            break;
                        case 2://活动
                            Tempinputstr = @"<form name=""form{PubID/}"" method=""post"" action=""/PubAction"">
<input type=""hidden"" name=""PubID"" id=""PubID"" value=""{PubID/}"" />
<input type=""hidden"" name=""PubContentid"" id=""PubContentid"" value=""{PubContentid/}"" />
<input type=""hidden"" name=""PubInputer"" id=""PubInputer"" value=""{PubInputer/}"" />
<div class=""form-group"">
<label for=""PubTitle"">活动标题：</label>
<input type=""text"" class=""form-control"" id=""PubTitle"" name=""PubTitle"" />
{PubCode/}
</div>
<div class=""form-group"">
<label for=""PubContent"">活动内容：</label>
<textarea class=""form-control"" name=""PubContent"" cols=""50"" rows=""10"" id=""PubContent""></textarea>
</div>
<div class=""form-group text-center"">
<button type=""submit"" class=""btn btn-default"">提交</button>
<button type=""reset"" class=""btn btn-default"">重置</button>
</div>
</form>";
                            Tempinputstr = Tempinputstr.Replace(@"{PubID/}", Pubinsertid.ToString());
                            saveName = "默认活动" + pubMod.PubName + "提交模板.html";
                            break;
                        case 3://留言
                            Tempinputstr = @"<form name=""form{PubID/}"" method=""post"" action=""/PubAction"">
<input type=""hidden"" name=""PubID"" id=""PubID"" value=""{PubID/}"" />
<input type=""hidden"" name=""PubContentid"" id=""PubContentid"" value=""{PubContentid/}"" />
<input type=""hidden"" name=""PubInputer"" id=""PubInputer"" value=""{PubInputer/}"" />
<div class=""form-group"">
<label for=""PubTitle"">留言标题：</label>
<input type=""text"" class=""form-control"" id=""PubTitle"" name=""PubTitle"" />
{PubCode/}
</div>
<div class=""form-group"">
<label for=""PubContent"">留言内容：</label>
<textarea class=""form-control"" name=""PubContent"" cols=""50"" rows=""10"" id=""PubContent""></textarea>
</div>
<div class=""form-group text-center"">
<button type=""submit"" class=""btn btn-default"">提交</button>
<button type=""reset"" class=""btn btn-default"">重置</button>
</div>
</form>";
                            Tempinputstr = Tempinputstr.Replace(@"{PubID/}", Pubinsertid.ToString());
                            saveName = "默认留言" + pubMod.PubName + "提交模板.html";
                            break;
                        case 4://问券
                            Tempinputstr = @"<form name=""form{PubID/}"" method=""post"" action=""/PubAction"">
<input type=""hidden"" name=""PubID"" id=""PubID"" value=""{PubID/}"" />
<input type=""hidden"" name=""PubContentid"" id=""PubContentid"" value=""{PubContentid/}"" />
<input type=""hidden"" name=""PubInputer"" id=""PubInputer"" value=""{PubInputer/}"" />
<div class=""form-group"">
<label for=""PubTitle"">问券标题：</label>
<input type=""text"" class=""form-control"" id=""PubTitle"" name=""PubTitle"" />
{PubCode/}
</div>
<div class=""form-group"">
<label for=""PubContent"">问券内容：</label>
<textarea class=""form-control"" name=""PubContent"" cols=""50"" rows=""10"" id=""PubContent""></textarea>
</div>
<div class=""form-group text-center"">
<button type=""submit"" class=""btn btn-default"">提交</button>
<button type=""reset"" class=""btn btn-default"">重置</button>
</div>
</form>";
                            Tempinputstr = Tempinputstr.Replace(@"{PubID/}", Pubinsertid.ToString());
                            saveName = "默认问券" + pubMod.PubName + "提交模板.html";
                            break;
                        case 5://统计
                        case 6://竞标
                        default:
                            break;
                    }
                    if (!string.IsNullOrEmpty(saveName))
                    {
                        FileSystemObject.WriteFile(saveDir + saveName, Tempinputstr);
                    }
                    PubInputTMurl = "/互动模板/" + saveName;
                    #endregion
                    pubMod = pubBll.GetSelect(Pubinsertid);
                    pubMod.PubInputTM = PubInputTMurl;//提交窗口模板
                    pubMod.PubTemplate = PubTemplateurl;
                    pubBll.GetUpdate(pubMod);
                }
                else
                {
                    pubMod.PubInputTM = GetParam("PubInputTM_hid");
                    pubMod.PubTemplate = GetParam("PubTemplate_hid");
                    pubBll.GetUpdate(pubMod);
                }
            }
            return WriteOK("操作成功", pubMod.PubType == 8 ? "FormManage" : "PubManage");

        }
        public IActionResult Pubsinfo()
        {
            int pubId = DataConvert.CLng(GetParam("PubID"));
            M_Pub pubMod = pubBll.SelReturnModel(pubId);
            if (pubMod == null) { return WriteErr("互动模块不存在"); }
            if (string.IsNullOrEmpty(pubMod.PubTableName)) { return WriteErr("互动表为空");  }
            ViewBag.pubMod = pubMod;
            ViewBag.fieldDT = GetFieldDT(pubMod.PubModelID);
            PageSetting setting = B_Pub_Info.SelPage(CPage, PSize, new F_PubInfo()
            {
                tbname = pubMod.PubTableName,
                pid = DataConvert.CLng(GetParam("ParentID")),
                status = DataConvert.CLng(GetParam("status"), -100),
                uname = GetParam("uname"),
                skey = GetParam("skey"),
                skey_field = GetParam("skey_dp")
            });
            foreach (DataRow dr in setting.dt.Rows)
            {
                dr["PubIP"] = dr["PubIP"] + "(" + IPScaner.IPLocation(DataConvert.CStr(dr["PubIP"])) + ")";
            }
            if (Request.IsAjaxRequest())
            {
                return PartialView("Pubsinfo_List", setting);
            }
            return View(setting);
        }
        public ContentResult PubInfo_API()
        {
            string action = GetParam("action");
            int pubId = DataConvert.CLng(GetParam("pubid"));
            string ids = GetParam("ids");
            SafeSC.CheckIDSEx(ids);
            M_Pub pubMod = pubBll.SelReturnModel(pubId);
            if (pubMod == null) { throw new Exception("未指定互动模型"); }
            switch (action)
            {
                case "del":
                    DBCenter.DelByIDS(pubMod.PubTableName, "ID", ids);
                    break;
                case "audit":
                    DBCenter.UpdateSQL(pubMod.PubTableName, "PubStart=1", "ID IN (" + ids + ")");
                    break;
                case "cancel":
                    DBCenter.UpdateSQL(pubMod.PubTableName, "PubStart=0", "ID IN (" + ids + ")");
                    break;
            }
            return Content(Success.ToString());
        }


        //避免前端搜索的方式注入,true:存在
        private bool IsExistInFieldDT(DataTable FieldDT, string field)
        {
            DataRow[] drs = FieldDT.Select("FieldName IN ('" + field + "')");
            return drs.Length > 0;
        }
        private DataTable GetFieldDT(int modelId)
        {
            DataTable dt = fieldBll.SelByModelID(modelId, true);
            string[] fieldArr = ("PubUserName:用户名,PubTitle:标题,PubContent:内容,PubAddTime:添加时间,PubIP:IP"
                + ",Pubnum:参与人数").Split(',');
            foreach (string field in fieldArr)
            {
                string name = field.Split(':')[0];
                if (dt.Select("FieldName='" + name + "'").Length > 0) { continue; }
                string alias = field.Split(':')[1];
                DataRow dr = dt.NewRow();
                dr["FieldName"] = name;
                dr["FieldAlias"] = alias;
                dt.Rows.Add(dr);
            }
            return dt;
        }
    }
}