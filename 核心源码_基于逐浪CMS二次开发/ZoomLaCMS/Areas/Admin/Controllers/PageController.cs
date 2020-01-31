using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ZoomLa.BLL;
using ZoomLa.BLL.Content;
using ZoomLa.BLL.Page;
using ZoomLa.Common;
using ZoomLa.Components;
using ZoomLa.Model;
using ZoomLa.Model.Content;
using ZoomLa.Model.Page;
using ZoomLa.SQLDAL;
using ZoomLa.SQLDAL.SQL;
using ZoomLaCMS.Ctrl;
using ZoomLaCMS.Models.Common;

namespace ZoomLaCMS.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Policy = "Admin")]
    public class PageController : Ctrl_Admin
    {
        B_Model modBll = new B_Model();
        B_PageReg regBll = new B_PageReg();
        B_PageStyle styleBll = new B_PageStyle();
        B_Content conBll = new B_Content();
        B_PageTemplate tempBll = new B_PageTemplate();
        B_Page pageBll = new B_Page();
        B_ModelField fieldBll = new B_ModelField();
        public int StyleID
        {
            get { if (ViewBag.StyleID == null) { ViewBag.StyleID = DataConvert.CLng(RequestEx["StyleID"]); }; return DataConvert.CLng(ViewBag.StyleID); }
            set { ViewBag.StyleID = value; }
        }
        public int UserID
        {
            get { if (ViewBag.StyleID == null) { ViewBag.StyleID = DataConvert.CLng(RequestEx["UserID"]); }; return DataConvert.CLng(ViewBag.UserID); }
            set { ViewBag.UserID = value; }
        }
        public int RegID
        {
            get { if (ViewBag.RegID == null) { ViewBag.RegID = DataConvert.CLng(RequestEx["RegID"]); }; return DataConvert.CLng(ViewBag.RegID); }
            set { ViewBag.RegID = value; }
        }
        public IActionResult ApplyAudit()
        {
            if (!B_ARoleAuth.AuthCheckEx(ZLEnum.Auth.page, "manage")) { return null; };
            PageSetting setting = regBll.SelPage(CPage, PSize, DataConvert.CLng(RequestEx["status"], -100), RequestEx["skey"]);
            if (Request.IsAjax())
            {
                return PartialView("ApplyAudit_List", setting);
            }
            return View(setting);
        }
        public IActionResult ApplyInfo()
        {
            if (!B_ARoleAuth.AuthCheckEx(ZLEnum.Auth.page, "manage")) { return null; }
            M_PageReg regMod = regBll.SelReturnModel(Mid);
            ViewBag.valuedr = regBll.SelUserApplyInfo(regMod);
            DataTable styleDt = styleBll.Sel();
            styleDt.Columns["PageNodeid"].ColumnName = "TemplateID";
            styleDt.Columns["TemplateIndex"].ColumnName = "TemplateUrl";
            styleDt.Columns["TemplateIndexPic"].ColumnName = "TemplatePic";
            styleDt.Columns["PageNodeName"].ColumnName = "rname";
            ViewBag.styleDt = styleDt;
            ViewBag.applyModDT = modBll.GetListPage();
            return View(regMod);
        }
        public IActionResult PageStyle()
        {
            if (!B_ARoleAuth.AuthCheckEx(ZLEnum.Auth.page, "manage")) { return null; }
            PageSetting setting = styleBll.SelPage(CPage, PSize);
            if (Request.IsAjax())
            {
                return PartialView("PageStyle_List", setting);
            }
            return View(setting);
        }
        public IActionResult PageStyleAdd()
        {
            if (!B_ARoleAuth.AuthCheckEx(ZLEnum.Auth.page, "manage")) { return null; }
            M_PageStyle styleMod = styleBll.SelReturnModel(Mid);
            if (styleMod == null) { styleMod = new M_PageStyle(); }
            return View(styleMod);
        }
        public IActionResult PageContent()
        {
            if (!B_ARoleAuth.AuthCheckEx(ZLEnum.Auth.page, "manage")) { return null; }
            int nodeID = DataConvert.CLng(RequestEx["TemplateID"]);
            PageSetting setting = conBll.Page_Sel(CPage, PSize, nodeID, DataConvert.CStr(RequestEx["status"]), RequestEx["inputer_t"], RequestEx["title_t"]);
            if (Request.IsAjax())
            {
                return PartialView("PageContent_List", setting);
            }
            return View(setting);
        }
        public IActionResult EditContent()
        {
            if (!B_ARoleAuth.AuthCheckEx(ZLEnum.Auth.page, "manage")) { return null; }
            M_CommonData Cdata = conBll.GetCommonData(Mid);
            if (Cdata == null) { return WriteErr("黄页内容不存在"); }
            M_ModelInfo model = modBll.SelReturnModel(Cdata.ModelID);
            M_Templata nodeMod = tempBll.SelReturnModel(Cdata.NodeID);
            ViewBag.nodeMod = nodeMod;
            return View(Cdata);
        }
        [HttpPost]
        public IActionResult Content_Edit()
        {
            M_CommonData CData = conBll.SelReturnModel(Mid);
            Call commonCall = new Call();
            DataTable dt = fieldBll.SelByModelID(CData.ModelID, false);
            DataTable table;
            try
            {
                table = Call.GetDTFromMVC(dt, Request);
            }
            catch (Exception e)
            {
               return WriteErr(e.Message);
            }
            CData.Title = Request.Form["Title"];
            CData.UpDateTime = DateTime.Now;
            conBll.UpdateContent(table, CData);
            return WriteOK("修改成功", "PageContent?TemplateID=" + CData.NodeID);
        }
        public IActionResult PageConfig()
        {
            if (!B_ARoleAuth.AuthCheckEx(ZLEnum.Auth.page, "manage")) { return null; };
            return View();
        }
        //根据传入的PageReg中的ID,列出用户所拥有的表(新用户申请通过,默认建立栏目中配置中处理)
        public IActionResult PageTemplate()
        {
            if (!B_ARoleAuth.AuthCheckEx(ZLEnum.Auth.page, "manage")) { return null; }
            if (Mid == 0) { return WriteErr("未指定参数");  }
            //ViewBag.TempList = FileSystemObject.GetDTForTemplate();
            DataTable templist = new DataTable();
            if (Mid > 0)
            {
                M_PageReg regMod = regBll.SelReturnModel(Mid);
                templist = tempBll.Sel(regMod.UserID);
                StyleID = regMod.NodeStyle;
                UserID = regMod.UserID;
                ViewBag.title = regMod.UserName;
            }
            else if (Mid == -1)//公用栏目
            {
                templist = tempBll.Sel(Mid);
                ViewBag.title = "公用栏目";
            }
            //---------------------------------
            templist.DefaultView.RowFilter = "ParentID=0";
            VM_Recursion reMod = new VM_Recursion();
            ViewBag.Mid = Mid;
            reMod.alldt = templist;
            reMod.dt = templist.DefaultView.ToTable();
            ViewBag.reMod = reMod;
            return View();
        }
        public IActionResult PageTemplateAdd()
        {
            M_PageReg regMod = new M_PageReg();
            M_Templata tempMod = new M_Templata();
            regMod = regBll.SelReturnModel(RegID);
            if (Mid > 0)
            {
                tempMod = tempBll.SelReturnModel(Mid);
                regMod = regBll.SelModelByUid(tempMod.UserID);
                RegID = regMod.ID;
            }
            //用户所属RegID,公有为-1
            if (regMod == null) { return WriteErr("RegID参数错误");}
            DataTable templist = tempBll.Sel(regMod.UserID);
            DataTable styleDT = styleBll.Sel();
            B_Page pageBll = new B_Page(tempMod.Modelinfo);
            pageBll.moddt = modBll.SelByType("4");
            ViewBag.pageBll = pageBll;
            ViewBag.styleList = MVCHelper.ToSelectList(styleDT, "PageNodeName", "PageNodeName", tempMod.UserGroup);
            ViewBag.templist = templist;
            return View(tempMod);
        }
        public IActionResult SetPageOrder()
        {
            return View(tempBll.SelByStyleAndPid(-100, Mid));
        }
        public IActionResult PageConfig_Update()
        {
            SiteConfig.SiteOption.RegPageStart = !string.IsNullOrEmpty(Request.Form["RegPageStart_Chk"]);
            SiteConfig.YPage.IsAudit = !string.IsNullOrEmpty(Request.Form["IsAudit_Chk"]);
            SiteConfig.YPage.UserCanNode = !string.IsNullOrEmpty(Request.Form["UserCanNode_Chk"]);
            SiteConfig.Update();
            return WriteOK("配置保存成功", "PageConfig");
        }
        #region PageTemplate Logical
        public IActionResult PageTemplate_Add()
        {
            M_Templata tempMod = new M_Templata();
            M_PageReg regMod = new M_PageReg();
            if (Mid > 0) { tempMod = tempBll.SelReturnModel(Mid); regMod = regBll.SelModelByUid(tempMod.UserID); }
            if (RegID != 0) { regMod = regBll.SelReturnModel(RegID); }
            tempMod.TemplateName = Request.Form["TemplateName"];
            tempMod.TemplateUrl = Request.Form["TemplateUrl_hid"];
            tempMod.TemplateType = DataConvert.CLng(Request.Form["TemplateType"]);
            tempMod.OpenType = Request.Form["OpenType"];
            tempMod.UserGroup = Request.Form["UserGroup"];
            tempMod.Addtime = DataConverter.CDate(Request.Form["addtime"]);
            tempMod.IsTrue = DataConverter.CLng(Request.Form["IsTrue"]);
            tempMod.OrderID = DataConverter.CLng(Request.Form["OrderID"]);
            tempMod.Identifiers = Request.Form["Identifiers"];
            tempMod.NodeFileEx = Request.Form["NodeFileEx"];
            tempMod.ContentFileEx = Request.Form["ContentFileEx"];
            tempMod.Nodeimgurl = Request.Form["Nodeimgurl"];
            tempMod.Nodeimgtext = Request.Form["Nodeimgtext"];
            tempMod.Pagecontent = Request.Form["Pagecontent"];
            tempMod.PageMetakeyword = Request.Form["PageMetakeyword"];
            tempMod.PageMetakeyinfo = Request.Form["PageMetakeyinfo"];
            tempMod.Linkurl = Request.Form["linkurl.Text"];
            tempMod.Linkimg = Request.Form["linkimg.Text"];
            tempMod.Linktxt = Request.Form["linktxt.Text"];
            tempMod.Modelinfo = pageBll.GetSubmitModelChk(Request);
            if (tempMod.TemplateID > 0)
            {
                tempBll.UpdateByID(tempMod);
            }
            else
            {
                tempMod.ParentID = DataConvert.CLng(RequestEx["ParentID"]);
                tempMod.UserID = regMod.UserID;
                tempMod.Username = regMod.UserName;
                tempBll.insert(tempMod);
            }
            return WriteOK("操作成功", "PageTemplate?ID=" + regMod.ID);
        }
        public int Node_Del(string ids)
        {
            tempBll.DelByIDS(ids);
            return Success;
        }
        #endregion
        #region Order Logical
        public void SetPageOrder_Batch()
        {
            if (!string.IsNullOrEmpty(Request.Form["PageValue"]))
            {
                string[] idsarr = RequestEx["PageValue"].Split(',');
                for (int i = 0; i < idsarr.Length; i++)
                {
                    int orderid = DataConverter.CLng(Request.Form["OrderField" + idsarr[i]]);
                    M_Templata tempmod = tempBll.SelReturnModel(DataConverter.CLng(idsarr[i]));
                    tempmod.OrderID = orderid;
                    tempBll.UpdateByID(tempmod);
                }
            }
            Response.Redirect("SetPageOrder?id=" + RequestEx["ID"]);
        }
        public void SetPageOrder_UpMove()
        {
            int tempid = DataConverter.CLng(RequestEx["tempid"]);
            MovePage(tempid, true);
            Response.Redirect("SetPageOrder?id=" + RequestEx["ID"]);
        }
        public void SetPageOrder_DownMove()
        {
            int tempid = DataConverter.CLng(RequestEx["tempid"]);
            MovePage(tempid, false);
            Response.Redirect("SetPageOrder?id=" + RequestEx["ID"]);
        }
        private void MovePage(int id, bool isup)
        {
            string[] SpecValues = RequestEx["PageValue"].Split(',');
            M_Templata tempmod = tempBll.SelReturnModel(id);
            for (int i = 0; i < SpecValues.Length; i++)
            {
                if (SpecValues[i].Equals(id.ToString()))
                {
                    if ((isup && i - 1 < 0) || (!isup && i + 1 >= SpecValues.Length)) { break; }//上移下移判断
                    int index = isup ? i - 1 : i + 1;
                    M_Templata targetmod = tempBll.SelReturnModel(DataConverter.CLng(SpecValues[index]));
                    int nodeorder = DataConverter.CLng(Request.Form["OrderField" + SpecValues[index]]);
                    targetmod.OrderID = tempmod.OrderID;
                    tempmod.OrderID = nodeorder;
                    tempBll.UpdateByID(tempmod);
                    tempBll.UpdateByID(targetmod);
                    break;
                }
            }
        }
        #endregion
        #region Content Logical
        public int Content_Audit(string ids)
        {
            if (!string.IsNullOrEmpty(ids))
            {
                conBll.SetAuditByIDS(ids, (int)ZLEnum.ConStatus.Audited);
            }
            return Success;
        }
        public int Content_UnAudit(string ids)
        {
            if (!string.IsNullOrEmpty(ids))
            {
                conBll.SetAuditByIDS(ids, (int)ZLEnum.ConStatus.UnAudit);
            }
            return Success;
        }
        public int Content_Del(string ids)
        {
            if (!string.IsNullOrEmpty(ids))
            {
                conBll.SetAuditByIDS(ids, (int)ZLEnum.ConStatus.Recycle);
            }
            return Success;
        }
        public int Content_RealDel(string ids)
        {
            conBll.DelByIDS(ids);
            return Success;
        }
        public int Content_Recovery(string ids)
        {
            conBll.RecByIDS(ids);
            return Success;
        }
        public int Content_Clear()
        {
            conBll.Page_ClearRecycle();
            return Success;
        }
        #endregion
        #region Style Logical
        public IActionResult Style_Add()
        {
            M_PageStyle styleMod = new M_PageStyle();
            if (Mid > 0)
            {
                styleMod = styleBll.SelReturnModel(Mid);
            }
            styleMod.PageNodeName = Request.Form["PageNodeName"];
            styleMod.StylePath = Request.Form["StylePath"];
            styleMod.Orderid = DataConverter.CLng(Request.Form["orderid"]);
            styleMod.TemplateIndex = Request.Form["TemplateIndex_hid"];
            styleMod.TemplateIndexPic = Request.Form["TemplateIndexPic_t"];
            if (styleMod.PageNodeid > 0)
            {
                styleBll.UpdateByID(styleMod);
            }
            else
            {
                styleBll.insert(styleMod);
            }
            return WriteOK("操作成功", "PageStyle");
        }
        public int Style_Del(string ids)
        {
            styleBll.DelByIDS(ids);
            return Success;
        }
        #endregion
        #region Apply Logical
        public int Page_Audit(string ids)
        {
            regBll.UpdateByField("[Status]", ((int)ZLEnum.ConStatus.Audited).ToString(), ids);
            return Success;
        }
        public int Page_UnAudit(string ids)
        {
            regBll.UpdateByField("[Status]", ((int)ZLEnum.ConStatus.UnAudit).ToString(), ids);
            return Success;
        }
        public int Page_Recom(string ids)
        {
            regBll.UpdateByField("[Recommendation]", "1", ids);
            return Success;
        }
        public int Page_UnRecom(string ids)
        {
            regBll.UpdateByField("[Recommendation]", "0", ids);
            return Success;
        }
        public int Page_Del(string ids)
        {
            if (!string.IsNullOrEmpty(ids))
            {
                regBll.DelByIDS(ids);
            }
            return Success;
        }
        public IActionResult Apply_Update()
        {
            M_PageReg regMod = regBll.SelReturnModel(Mid);
            M_UserInfo mu = buser.SelReturnModel(regMod.UserID);
            DataTable dt = fieldBll.GetModelFieldList(regMod.ModelID);
            DataTable table = Call.GetDTFromMVC(dt, Request);
            regMod.CompanyName = Request.Form["CompanyName"];
            regMod.PageTitle = mu.UserName + "的黄页信息";
            regMod.PageInfo = Request.Form["PageInfo"];
            regMod.LOGO = Request.Form["LOGO_t"];
            regMod.NodeStyle = DataConverter.CLng(Request.Form["TempleID_Hid"]);//样式与首页模板??,首页模板可动态以styleMod中为准
            regMod.Template = Request.Form["TempleUrl_Hid"];
            //regMod.Status =//状态不变更
            //regMod.ModelID = applyMod.ModelID;
            //regMod.TableName = applyMod.TableName;
            conBll.Page_Update(table, regMod);
            return WriteOK("修改提交成功", "ApplyAudit");
        }
        #endregion
    }
}