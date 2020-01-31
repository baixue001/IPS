using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;
using ZoomLa.BLL;
using ZoomLa.BLL.API;
using ZoomLa.BLL.Content;
using ZoomLa.BLL.CreateJS;
using ZoomLa.BLL.Helper;
using ZoomLa.Common;
using ZoomLa.Components;
using ZoomLa.Model;
using ZoomLa.Model.Content;
using ZoomLa.Model.CreateJS;
using ZoomLa.Safe;
using ZoomLa.SQLDAL;
using ZoomLaCMS.Areas.Admin.Models;
using ZoomLaCMS.Ctrl;

namespace ZoomLaCMS.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Policy = "Admin")]
    public class ContentController : Ctrl_Admin
    {
        public override int Mid { get { return DataConvert.CLng(Request.Query["GeneralID"]); } }
        M_Node nodeMod = new M_Node();
        B_Content contentBll = new B_Content();
        B_Model modelBll = new B_Model();
        B_ModelField fieldBll = new B_ModelField();
        B_Node nodeBll = new B_Node();
        ContentHelper conhelp = new ContentHelper();
        public int NodeID { get { return DataConvert.CLng(Request.Query["NodeID"]); } }
        public int ModelID { get { return DataConvert.CLng(Request.Query["ModelID"]); } }
        #region 内容管理
        public IActionResult AddContent()
        {
            if (ModelID < 1 || NodeID < 1) { WriteErr("参数错误"); return null; }
            VM_Content vm = new VM_Content();
            vm.fieldDT = fieldBll.SelByModelID(ModelID, true);
            vm.nodeMod = nodeBll.SelReturnModel(NodeID);
            vm.modelMod = modelBll.SelReturnModel(ModelID);
            vm.conMod.Status = 99; //adminMod.DefaultStart;
            vm.conMod.Inputer = adminMod.AdminName; //adminMod.UserName;
            return View(vm);
        }
        public IActionResult EditContent()
        {
            VM_Content vm = new VM_Content();
            vm.conMod = contentBll.SelReturnModel(Mid);
            vm.ValueDT = contentBll.GetContent(vm.conMod);
            vm.fieldDT = fieldBll.SelByModelID(vm.conMod.ModelID, true);
            vm.nodeMod = nodeBll.SelReturnModel(vm.conMod.NodeID);
            vm.modelMod = modelBll.SelReturnModel(vm.conMod.ModelID);
            //M_Content_ScheTask exaTask = scheBll.GetModel(vm.conMod.BidType);
            //if (exaTask != null) { vm.ExamineTime = exaTask.ExecuteTime; }
            //M_Content_ScheTask expTask = scheBll.GetModel(vm.conMod.IsBid);
            //if (expTask != null) { vm.ExpiredTime = expTask.ExecuteTime; }
            //---------------
            return View("AddContent", vm);
        }
        public IActionResult ShowContent()
        {
            int id = DataConvert.CLng(Request.Query["ID"]);
            VM_Content vm = new VM_Content();
            vm.conMod = contentBll.SelReturnModel(id);
            vm.ValueDT = contentBll.GetContent(vm.conMod);
            vm.fieldDT = fieldBll.SelByModelID(vm.conMod.ModelID, true);
            vm.nodeMod = nodeBll.SelReturnModel(vm.conMod.NodeID);
            vm.modelMod = modelBll.SelReturnModel(vm.conMod.ModelID);
            //if (!string.IsNullOrEmpty(vm.conMod.SpecialID))
            //{
            //    vm.SpecialDT = spbll.SelByIDS(vm.conMod.SpecialID);
            //    if (vm.SpecialDT == null) { vm.SpecialDT = new DataTable(); }
            //}
            return View("ShowContent", vm);
        }
        private string ShowContentUrl = "/Admin/Content/ShowContent?ID=";
        //
        public IActionResult Content_Add()
        {
            DataTable table = new DataTable();
            M_CommonData CData = FillContentModel(ref table, null, ref err);
            if (!string.IsNullOrEmpty(err)) { return WriteErr(err); }
            CData.GeneralID = contentBll.AddContent(table, CData);
            //IsPushContent(RequestEx["pushcon_hid"], CData, table);
            //IsCreateHtml(CData, table);
            //IsNeedVerBak(CData);
            //IsAutoTask(CData);
            return Redirect(ShowContentUrl + CData.GeneralID);
        }
        //
        public IActionResult Content_AddToNew()
        {
            M_CommonData CData = contentBll.SelReturnModel(Mid);
            DataTable table = new DataTable();
            CData = FillContentModel(ref table, CData, ref err);
            if (!string.IsNullOrEmpty(err)) { WriteErr(err);  }

            CData.GeneralID = contentBll.AddContent(table, CData);
            return Redirect(ShowContentUrl + CData.GeneralID);
        }
        //
        public IActionResult Content_Update()
        {
            M_CommonData CData = contentBll.SelReturnModel(Mid);
            DataTable table = new DataTable();
            CData = FillContentModel(ref table, CData, ref err);
            if (!string.IsNullOrEmpty(err)) { WriteErr(err);  }
            string json=JsonConvert.SerializeObject(table);
            contentBll.UpdateContent(table, CData);
            return Redirect(ShowContentUrl + CData.GeneralID);
        }
        public void Content_Draft()
        {
            DataTable table = new DataTable();
            M_CommonData CData = FillContentModel(ref table, null, ref err);
            if (!string.IsNullOrEmpty(err)) { WriteErr(err);  }
            CData.Status = (int)ZLEnum.ConStatus.Draft;
            CData.GeneralID = contentBll.AddContent(table, CData);
            Response.Redirect(ShowContentUrl + CData.GeneralID);
        }
        public string Content_API()
        {
            string action = Request.Query["action"];
            switch (action)
            {
                case "duptitle":
                    DataTable dt = contentBll.GetByDupTitle(Request.Query["value"]);
                    return Newtonsoft.Json.JsonConvert.SerializeObject(dt);
                default:
                    return "";
            }
        }
        //填充共通部分
        private M_CommonData FillContentModel(ref DataTable table, M_CommonData CData, ref string err)
        {
            if (CData == null) { CData = new M_CommonData(); }
            //if (SiteConfig.SiteOption.FileRj == 1 && contentBll.SelHasTitle(RequestEx["txtTitle"])) { WriteErr(Resources.L.该内容标题已存在 + "!", "javascript:history.go(-1);"); }
            if (CData.GeneralID < 1)
            {
                CData.Inputer = adminMod.AdminName;
                CData.NodeID = NodeID;
                CData.ModelID = ModelID;
                CData.TableName = modelBll.SelReturnModel(ModelID).TableName;
                string parentTree = "";
                CData.FirstNodeID = nodeBll.SelFirstNodeID(CData.NodeID, ref parentTree);
                CData.ParentTree = parentTree;
            }
            DataTable dt = fieldBll.SelByModelID(CData.ModelID, false);
            try
            {
                table = Call.GetDTFromMVC(dt, Request);
            }
            catch (Exception ex)
            {
                err = ex.Message;return null;
            }
            CData.Title = RequestEx["txtTitle"];
            CData.EliteLevel = DataConvert.CLng(RequestEx["EliteLevel"]);
            CData.Hits = DataConvert.CLng(RequestEx["Hits"]);
            CData.TagKey = RequestEx["tabinput"];
            CData.Status = DataConvert.CLng(RequestEx["ddlFlow"]);
            CData.Template = RequestEx["TemplateUrl_hid"];
            CData.CreateTime = DataConverter.CDate(RequestEx["CreateTime"]);
            CData.UpDateTime = DataConverter.CDate(RequestEx["UpDateTime"]);
            CData.SpecialID = "," + string.Join(",", DataConvert.CStr(RequestEx["Spec_Hid"]).Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries)) + ",";
            CData.TitleStyle = RequestEx["ThreadStyle"];
            CData.TopImg = RequestEx["ThumImg_Hid"];//首页图片
            CData.Subtitle = RequestEx["Subtitle"];
            CData.PYtitle = RequestEx["PYtitle"];
            CData.RelatedIDS = RequestEx["RelatedIDS_Hid"];
            CData.IsComm = DataConvert.CLng(RequestEx["IsComm_Radio"]);
            CData.UGroupAuth = RequestEx["UGroupAuth"];
            CData.IsTop = DataConvert.CLng(RequestEx["IsTop"]);
            CData.IsTopTime = DataConvert.CStr(RequestEx["IsTopTime"]);
            #region  关键词
            {
                B_KeyWord keyBll = new B_KeyWord();
                string[] ignores = DataConvert.CStr(RequestEx["Keywords"]).Split(",".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                string[] newKeys = CData.TagKey.Split(",".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                string keys = StrHelper.RemoveRepeat(newKeys, ignores);
                if (!string.IsNullOrEmpty(keys))
                {
                    keyBll.AddKeyWord(keys, 1);
                }
            }
            #endregion
            return CData;
        }
        public IActionResult ContentManage()
        {
            //if (!adminMod.IsSuperAdmin() && NodeID < 1) { WriteErr("没有指定节点ID"); return null; }
            VM_ContentManage vm = FillVMContentManage(ref err);
            if (!string.IsNullOrEmpty(err)) { return WriteErr(err);  }
            if (Request.IsAjax()) { return PartialView("ContentManage_List", vm); }
            else
            {
                return View(vm);
            }
        }
        public IActionResult ContentRecycle()
        {
            VM_ContentManage vm = new VM_ContentManage();
            vm.filter.NodeID = NodeID;
            vm.filter.ModelID = ModelID;
            vm.filter.Status = ((int)ZLEnum.ConStatus.Recycle).ToString();
            vm.filter.KeyWord = HttpUtility.UrlDecode(GetParam("KeyWord"));
            vm.setting = contentBll.SelPage(CPage, PSize, vm.filter);

            if (Request.IsAjax()) { return PartialView("ContentManage_List", vm); }
            else { return View(vm); }
        }
        public ContentResult ContentManage_API()
        {
            string action = GetParam("action");
            string ids = GetParam("ids");
            switch (action)
            {
                case "move":
                    string direct = Request.Form["direct"];
                    int curid = DataConvert.CLng(Request.Form["curid"]), tarid = DataConvert.CLng(Request.Form["tarid"]);

                    M_CommonData curMod = contentBll.GetCommonData(curid);
                    M_CommonData tarMod = contentBll.GetCommonData(tarid);
                    if (curMod.OrderID == tarMod.OrderID)
                    {
                        switch (direct)
                        {
                            case "up":
                                curMod.OrderID++;
                                break;
                            case "down":
                                curMod.OrderID--;
                                break;
                        }
                    }
                    else
                    {
                        int temp = curMod.OrderID;
                        curMod.OrderID = tarMod.OrderID;
                        tarMod.OrderID = temp;
                    }
                    contentBll.UpdateByID(curMod); contentBll.UpdateByID(tarMod);
                    return Content("true");
                case "del":
                    contentBll.DelContent(ids, "");
                    break;
                case "recover":
                    contentBll.Reset(ids);
                    break;
                case "clear":
                    contentBll.DelRecycle();
                    break;
                default:
                    break;
            }
            return Content(Success.ToString());
        }
        public int ContentManage_Elite(string ids, int elite)
        {
            contentBll.UpdateElite(ids, elite);
            return M_APIResult.Success;
        }
        public string ContentManage_Status(string ids, int status)
        {
            //删除权限单独限验证
            if (status == (int)ZLEnum.ConStatus.Recycle) { return "该API不允许删除"; }
            contentBll.UpdateStatus(ids, status);
            return M_APIResult.Success.ToString();
        }
        public string ContentManage_Del(string ids)
        {
            if (string.IsNullOrEmpty(ids)) { return "未指定需要删除的内容"; }
            //用户只能删除自己有管理权限的节点下的内容
          
            bool isSuper = adminMod.IsSuperAdmin();
            foreach (string item in ids.Split(','))
            {
                int id = DataConvert.CLng(item);
                if (id < 1) { continue; }
                M_CommonData conMod = contentBll.GetCommonData(id);
                if (!string.IsNullOrEmpty(conMod.HtmlLink)) { ZoomLa.BLL.SafeSC.DelFile(conMod.HtmlLink); }
                conMod.Status = (int)ZLEnum.ConStatus.Recycle;
                conMod.IsCreate = 0;
                conMod.HtmlLink = "";
                contentBll.UpdateByID(conMod);
            }
            return M_APIResult.Success.ToString();
        }
        //刷新，将内容添加时间和最后更新时间改为当前时间
        public string ContentManage_Refresh(string ids)
        {

            if (string.IsNullOrEmpty(ids)) { return "未指定需要刷新的内容"; }
            foreach (string item in ids.Split(','))
            {
                M_CommonData conMod = contentBll.GetCommonData(DataConverter.CLng(item));
                if (conMod != null)
                {
                    conMod.CreateTime = DateTime.Now;
                    conMod.UpDateTime = DateTime.Now;
                    contentBll.UpdateByID(conMod);
                }
            }
            return M_APIResult.Success.ToString();
        }
        private VM_ContentManage FillVMContentManage(ref string err)
        {
            VM_ContentManage vm = new VM_ContentManage();
            vm.nodeMod = nodeBll.SelReturnModel(NodeID);
            vm.filter.NodeID = vm.NodeID;
            vm.filter.ModelID = ModelID;
            //vm.filter.KeyType = DataConvert.CLng(RequestEx["KeyType"]);
            //vm.filter.KeyWord = HttpUtility.UrlDecode(RequestEx["KeyWord"] ?? "");
            vm.filter.Status = DataConvert.CStr(RequestEx["Status"]);
            vm.filter.special = DataConverter.CLng(RequestEx["special"]);
            vm.filter.orderBy = RequestEx["orderBy"];
            //----------------工作流,其与角色绑定，不分是否超管(需将其改为视图)
            vm.setting = contentBll.SelPage(CPage, PSize, vm.filter);
            vm.Count_WZS = vm.setting.itemCount;
            vm.Count_DJS = DataConvert.CLng(vm.setting.addon);
            return vm;
        }
        #endregion
        #region 生成发布
        public IActionResult CreateHtmlContent()
        {
            if (!B_ARoleAuth.AuthCheckEx(ZLEnum.Auth.content, "create")) { return WriteErr("无权进行该操作"); }
            if (Request.IsAjax())
            {
                VM_Create vm = new VM_Create();
                int nid = Convert.ToInt32(Request.Form["nid"]);
                DataTable dt = nodeBll.GetNodeChildList(nid);
                dt.Columns.Add(new DataColumn("icon", typeof(string)));
                dt.Columns.Add(new DataColumn("oper", typeof(string)));
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    DataRow dr = dt.Rows[i];
                    dt.Rows[i]["icon"] = vm.ShowIcon(dr); //ShowIcon(Convert.ToInt32(dr["NodeID"]), dr["NodeName"].ToString(), dr["NodeDir"].ToString(), Convert.ToInt32(dr["ChildCount"]));
                    dt.Rows[i]["oper"] = vm.GetOper(dr);// GetOper(dr);
                }
                string json = JsonConvert.SerializeObject(dt);
                return Content(json);
            }
            //----------单节点直接生成
            string CType = GetParam("CType");
            if (!string.IsNullOrEmpty(CType) && NodeID > 0)
            {
                M_Release relMod = new M_Release();
                switch (CType)
                {
                    case "node":
                        relMod.MyRType = M_Release.RType.NodeIDS;
                        break;
                    case "content":
                        relMod.MyRType = M_Release.RType.ByNodeIDS;
                        break;
                    case "spage":
                        relMod.MyRType = M_Release.RType.SPage;
                        break;
                    case "spec":
                        relMod.MyRType = M_Release.RType.Special;
                        break;
                }
                relMod.NodeIDS = NodeID.ToString();
                relBll.Insert(relMod);
                return RedirectToAction("CreateHtml");
            }

            return View("Create/CreateHtmlContent");
        }
        B_Release relBll = new B_Release();
        public IActionResult Create_Submit()
        {
            M_Release relMod = new M_Release();
            M_Release.RType rtype = (M_Release.RType)Enum.Parse(typeof(M_Release.RType), GetParam("cmd"));
            //M_Release.RType rtype = (M_Release.RType)1;
            switch (rtype)
            {
                case M_Release.RType.Index://发布站点主页
                    relMod.MyRType = M_Release.RType.Index;
                    break;
                case M_Release.RType.ALL://发布所有内容
                    relMod.MyRType = M_Release.RType.ALL;
                    break;
                case M_Release.RType.IDRegion:// 按ID发布内容
                    relMod.MyRType = M_Release.RType.IDRegion;
                    relMod.SGid = DataConverter.CLng(GetParam("txtIdStart"));
                    relMod.EGid = DataConverter.CLng(GetParam("txtIdEnd"));
                    if (relMod.SGid == 0 && relMod.SGid == relMod.EGid)
                    {
                       return WriteErr("起始ID不能为空");
                    }
                    break;
                case M_Release.RType.Newest:// 发布最新数量的内容
                    relMod.MyRType = M_Release.RType.Newest;
                    relMod.Count = DataConverter.CLng(GetParam("txtNewsCount"));
                    if (relMod.Count < 1) { return WriteErr("指定的数值不正确,最少生成最近1篇"); }
                    break;
                case M_Release.RType.DateRegion: // 按日期发布内容
                    relMod.MyRType = M_Release.RType.DateRegion;
                    relMod.STime = DataConverter.CDate(GetParam("STime_T"));
                    relMod.ETime = DataConverter.CDate(GetParam("ETime_T"));
                    if ((relMod.ETime - relMod.STime).TotalMinutes < 1)
                    {
                        return WriteErr("时间不正确,开始时间必须小于结束时间");
                    }
                    break;
                case M_Release.RType.ByNodeIDS:// 按栏目发布内容
                    {
                        string nids = Request.Form["nodechk"];
                        if (string.IsNullOrEmpty(nids)) { return WriteErr("未选定栏目"); }
                        nids = nids.TrimEnd(',');
                        relMod.MyRType = M_Release.RType.ByNodeIDS;
                        relMod.NodeIDS = nids;
                    }
                    break;
                case M_Release.RType.ALLNode:// 发布所有栏目页
                    relMod.MyRType = M_Release.RType.ALLNode;
                    break;
                case M_Release.RType.NodeIDS:// 发布选定的栏目页
                    {
                        string nids = Request.Form["nodechk"];
                        if (string.IsNullOrEmpty(nids)) { return WriteErr("未选定栏目"); }
                        nids = nids.TrimEnd(',');
                        relMod.MyRType = M_Release.RType.NodeIDS;
                        relMod.NodeIDS = nids;
                    }
                    break;
                case M_Release.RType.ALLSPage: // 发布所有单页
                    relMod.MyRType = M_Release.RType.ALLSPage;
                    break;
                case M_Release.RType.SPage:// 发布选定的单页
                    {
                        string nids = Request.Form["spagechk"];
                        if (string.IsNullOrEmpty(nids)) { return WriteErr("未选定栏目"); }
                        nids = nids.TrimEnd(',');
                        relMod.MyRType = M_Release.RType.SPage;
                        relMod.NodeIDS = nids;
                    }
                    break;
                case M_Release.RType.Special:// 发布选定的专题
                    {
                        string nids = Request.Form["spchk"];
                        if (string.IsNullOrEmpty(nids)) { return WriteErr("未选定专题"); }
                        nids = nids.TrimEnd(',');
                        relMod.MyRType = M_Release.RType.Special;
                        relMod.NodeIDS = nids;
                    }
                    break;
            }
            relBll.Insert(relMod);
            return RedirectToAction("CreateHtml");
        }
        //新建定时发布任务
        //protected void scheSure_Btn_Click(object sender, EventArgs e)
        //{
        //    if (string.IsNullOrEmpty(scheTime_T2.Text) && string.IsNullOrEmpty(scheTime_T.Text)) return;
        //    scheTime_T.Text = scheTime_T.Text.Trim();
        //    DataTable schDT = null;// scheBll.SelByTaskType(M_Content_ScheTask.TaskTypeEnum.Release);
        //    if (schDT != null && schDT.Rows.Count > 0)
        //    {
        //        scheMod = scheMod.GetModelFromDR(schDT.Rows[0]);
        //        scheMod.ExecuteTime = DateTime.Now.ToString("yyyy/MM/dd ") + scheTime_T.Text;
        //        scheBll.Update(scheMod);
        //    }
        //    else
        //    {
        //        scheMod.TaskName = "定时发布首页";
        //        scheMod.TaskContent = "index";
        //        scheMod.TaskType = (int)M_Content_ScheTask.TaskTypeEnum.Release;
        //        scheMod.ExecuteType = (int)M_Content_ScheTask.ExecuteTypeEnum.EveryDay;//暂时无效,原本用来决定是否IsLoop
        //        scheMod.ExecuteTime = DateTime.Now.ToString("yyyy/MM/dd ") + scheTime_T.Text;
        //        scheMod.Status = 0;
        //        scheBll.Add(scheMod);
        //    }
        //    Response.Redirect("SchedTask");
        //}
        public IActionResult CreateHtml()
        {
            if (Request.IsAjax())
            {
                string result = "";
                if (B_Release.ResutList.Count > 1)
                {
                    result = JsonConvert.SerializeObject(B_Release.ResutList);
                    B_Release.ResutList.Clear();
                }
                return Content(result);
            }
            else
            {
                B_Release.Start(HttpContext);
                return View("Create/CreateHtml");
            }
        }
        #endregion
    }
    [Area("Admin")]
    [Route("/Admin/Content/Node/[action]")]
    [Authorize(Policy = "Admin")]
    public class NodeController : Ctrl_Admin
    {
        B_Node nodeBll = new B_Node();

        public int ParentID { get { return DataConverter.CLng(Request.Query["ParentID"]); } }
        public string viewUrl = "~/Areas/Admin/Views/Content/Node/";
        public IActionResult NodeManage()
        {
            return View(viewUrl + "NodeManage.cshtml");
        }
        public IActionResult NodeSearch()
        {
            string skey = HttpUtility.UrlDecode(GetParam("skey"));
            DataTable AllNodeDT = nodeBll.SelByPid(0, true);
            AllNodeDT.Columns.Add(new DataColumn("NodeIDStr", typeof(string)));
            for (int i = 0; i < AllNodeDT.Rows.Count; i++)
            {
                AllNodeDT.Rows[i]["NodeIDStr"] = AllNodeDT.Rows[i]["NodeID"];
            }
            AllNodeDT.DefaultView.RowFilter = "NodeIDStr like '%" + skey + "%' OR NodeName  Like '%" + skey + "%'";
            AllNodeDT.DefaultView.Sort = "NodeID Asc";
            DataTable dt = AllNodeDT.DefaultView.ToTable();
            ViewBag.AllNodeDT = AllNodeDT;
            ViewBag.skey = skey;
            return View(viewUrl+ "NodeSearch.cshtml", dt);
        }
        public IActionResult NodeAdd()
        {
            M_Node nodeMod = new M_Node();
            //-------------------
            if (Mid > 0)
            {
                nodeMod = nodeBll.SelReturnModel(Mid);
            }
            else
            {
                nodeMod.ParentID = ParentID;
            }
            return View(viewUrl + "NodeAdd.cshtml",nodeMod);
        }
        public IActionResult SPageAdd()
        {
            return RedirectToAction("NodeAdd", new { ID = Mid });
        }
        public IActionResult OutLinkAdd()
        {
            return RedirectToAction("NodeAdd", new { ID = Mid });
        }
        public IActionResult NodeAdd_Submit()
        {
            M_Node node = new M_Node();
            if (Mid > 0) { node = nodeBll.SelReturnModel(Mid); }
            else
            {
                node.ParentID = ParentID;
                node.NodeUrl = "";
                node.NodeListUrl = "";
            }
            node = FillNode(node);
            if (node.NodeID > 0)
            {
                nodeBll.UpdateByID(node);
            }
            else
            {
                if (!nodeBll.CheckCanSave(node)) { return WriteErr("节发现同栏目下栏目名或标识名重复,请点击确定重新修改节点"); }
                node.NodeID = nodeBll.Insert(node);
            }
            UpdateTemplateByNode(node);
            return WriteOK("操作成功", "NodeManage");
        }
        public string Node_API()
        {
            string action = Request.Query["action"];
            string ids = RequestEx["ids"];
            switch (action)
            {
                case "del":
                    //nodeBll.DelToRecycle(ids);
                    nodeBll.DelNode(ids);
                    break;
            }
            return Success.ToString();
        }
        private M_Node FillNode(M_Node node)
        {
            //node.NodeListType = DataConverter.CLng(Nodetype);
            //node.Contribute = isSimple_CH.Checked ? 1 : 0;
            //node.SiteContentAudit = DataConverter.CLng(SiteContentAudit_Rad.SelectedValue);
            node.NodeType = 1;
            node.NodeDir = RequestEx["TxtNodeDir"];
            node.NodeName = RequestEx["TxtNodeName"];

            //#region 修改
            node.NodePic = RequestEx["TxtNodePicUrl"];
            node.Tips = RequestEx["TxtTips"];
            node.Description = RequestEx["TxtDescription"];
            node.Meta_Keywords = RequestEx["TxtMetaKeywords"];
            node.Meta_Description = RequestEx["TxtMetaDescription"];
            node.OpenNew = DataConverter.CBool(RequestEx["RBLOpenType"]);
            node.ItemOpenType = DataConverter.CBool(RequestEx["RBLItemOpenType"]);
            node.ItemOpenTypeTrue = RequestEx["UrlFormat_Rad"];
            node.PurviewType = DataConverter.CBool(RequestEx["RBLPurviewType"]);
            node.CommentType = RequestEx["RBLCommentType"];
            node.HitsOfHot = DataConverter.CLng(RequestEx["TxtHitsOfHot"]);
            node.ConsumePoint = DataConverter.CLng(RequestEx["TxtConsumePoint"]);
            node.ConsumeType = DataConverter.CLng(RequestEx["ConsumeType"]);
            node.ConsumeTime = DataConverter.CLng(RequestEx["TxtConsumeTime"]);
            node.ConsumeCount = DataConverter.CLng(RequestEx["TxtConsumeCount"]);
            node.OpenTypeTrue = RequestEx["RBLOpenType"];
            node.AddPoint = DataConverter.CLng(RequestEx["TxtAddPoint"]);
            node.AddMoney = DataConverter.CDouble(RequestEx["TxtAddMoney"]);
            node.ClickTimeout = DataConverter.CLng(RequestEx["ClickTimeout"]);
            node.AddUserExp = DataConverter.CLng(RequestEx["txtAddExp"]);
            node.DeducUserExp = DataConverter.CLng(RequestEx["txtDeducExp"]);
            node.CDate = DataConverter.CDate(RequestEx["CDate_T"]);
            node.CUser = adminMod.AdminId;
            node.CUName = adminMod.AdminName;

            node.ListTemplateFile = RequestEx["ListTemplateFile_hid"];
            node.IndexTemplate = RequestEx["IndexTemplate_hid"];// RequestEx[TxtIndexTemplate.UniqueID];
            node.LastinfoTemplate = RequestEx["LastinfoTemplate_hid"];
            node.ProposeTemplate = RequestEx["ProposeTemplate_hid"];
            node.HotinfoTemplate = RequestEx["HotinfoTemplate_hid"];
            node.ContentModel = DataConvert.CStr(RequestEx["ChkModel"]);

            node.ListPageHtmlEx = DataConverter.CLng(RequestEx["ListPageHtmlEx_Rad"]);
            node.ContentFileEx = DataConverter.CLng(RequestEx["ContentFileEx_Rad"]);
            node.ListPageEx = DataConverter.CLng(RequestEx["ListPageEx_Rad"]);
            node.LastinfoPageEx = DataConverter.CLng(RequestEx["LastinfoPageEx_Rad"]);//LastinfoTemplate.UniqueID
            node.HotinfoPageEx = DataConverter.CLng(RequestEx["HotinfoPageEx"]);
            node.ProposePageEx = DataConverter.CLng(RequestEx["ProposePageEx"]);
            node.ContentPageHtmlRule = DataConverter.CLng(RequestEx["DDLContentRule"]);
            /* node.SafeGuard = SafeGuard.Checked ? 1 : 0;*/
            node.HtmlPosition = DataConverter.CLng(RequestEx["RBLPosition"]);
            return node;
        }
        private void UpdateTemplateByNode(M_Node node)
        {
            string[] ModelArr = node.ContentModel.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            nodeBll.DelModelTemplate(node.NodeID, node.ContentModel);
            for (int i = 0; i < ModelArr.Length; i++)
            {
                if (!string.IsNullOrEmpty(RequestEx["ModelTemplate_" + ModelArr[i]]))
                {
                    //将模型模板设定写入数据库
                    string temp = RequestEx["ModelTemplate_" + ModelArr[i]];
                    if (nodeBll.IsExistTemplate(node.NodeID, DataConverter.CLng(ModelArr[i])))
                    {
                        nodeBll.UpdateModelTemplate(node.NodeID, DataConverter.CLng(ModelArr[i]), temp);
                    }
                    else
                    {
                        nodeBll.AddModelTemplate(node.NodeID, DataConverter.CLng(ModelArr[i]), temp);
                    }
                }
            }
        }
        //批量设置
        public IActionResult BatchNode()
        {
            return View(viewUrl + "BatchNode.cshtml");
        }
        public IActionResult BatchNode_Submit()
        {
            string nodeIds = Request.Form["NodeIDS_DP"];
            if (string.IsNullOrEmpty(nodeIds)) { return WriteErr("未指定需要修正的节点"); }
            string[] nodeArr = nodeIds.Split(",".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < nodeArr.Length; i++)
            {
                int nodeId = DataConvert.CLng(nodeArr[i]);
                if (nodeId < 1) { continue; }
                M_Node nodeOld = nodeBll.SelReturnModel(nodeId);
                M_Node nodeNew = nodeBll.SelReturnModel(nodeId);
                nodeNew = FillNode(nodeNew);
                nodeNew.NodeName = nodeOld.NodeName;
                nodeNew.NodeDir = nodeOld.NodeDir;
                nodeNew.ParentID = nodeOld.ParentID;
                nodeNew.NodeType = nodeOld.NodeType;
                nodeNew.CUser = nodeOld.CUser;
                nodeNew.CUName = nodeOld.CUName;

                nodeBll.UpdateByID(nodeNew);
                UpdateTemplateByNode(nodeNew);
            }
            return WriteOK("操作成功", "BatchNode");
        }
    }

}