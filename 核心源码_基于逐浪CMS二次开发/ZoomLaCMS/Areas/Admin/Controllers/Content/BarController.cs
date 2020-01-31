using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ZoomLa.BLL;
using ZoomLa.BLL.Helper;
using ZoomLa.BLL.Message;
using ZoomLa.Common;
using ZoomLa.Components;
using ZoomLa.Model;
using ZoomLa.Model.Message;
using ZoomLa.SQLDAL;
using ZoomLa.SQLDAL.SQL;
using ZoomLaCMS.Ctrl;

namespace ZoomLaCMS.Areas.Admin.Controllers.Content
{
    [Area("Admin")]
    [Authorize(Policy="Admin")]
    [Route("[area]/Content/Bar/[action]")]
    public class BarController : Ctrl_Admin
    {
        B_GuestBookCate cateBll = new B_GuestBookCate();
        B_Guest_Bar barBll = new B_Guest_Bar();
        B_Guest_BarAuth authBll = new B_Guest_BarAuth();
        string viewDir = "/Areas/Admin/Views/Content/Bar/";
        public override ViewResult View(string name, object model = null)
        {
            if (name.EndsWith("cshtml")) { return base.View(name, model); }
            else { return base.View(viewDir + name + ".cshtml", model); }

        }
        public int CateID { get { return DataConvert.CLng(RequestEx["CateID"]); } }
        public IActionResult Default() { return Redirect("CateList"); }
        public IActionResult CateList()
        {
            if (Request.IsAjax())
            {
                #region AJAX请求
                string action = Request.Form["action"];
                string value = Request.Form["value"];
                string result = "";
                switch (action)
                {
                    case "GetChild":
                        int id = DataConverter.CLng(value);
                        DataTable dt = cateBll.Cate_SelByType(M_GuestBookCate.TypeEnum.PostBar, id);
                        CateAppend(dt);
                        result = JsonHelper.JsonSerialDataTable(dt);
                        return Content(result);
                    default:
                        return Content("");
                }
                #endregion
            }
            else
            {
                B_ARoleAuth.AuthCheckEx(ZLEnum.Auth.content, "bar");
                //if (DelCateID > 0)
                //{
                //    GuestConfigInfo guestinfo = GuestConfig.GuestOption;
                //    GuestConfig config = new GuestConfig();
                //    BarOption baroption = guestinfo.BarOption.Find(v => v.CateID == DelCateID);
                //    if (baroption != null) { guestinfo.BarOption.Remove(baroption); }
                //    cateBll.Del(DelCateID);
                //    config.Update(guestinfo);
                //}
                DataTable dt = cateBll.Cate_SelByType(M_GuestBookCate.TypeEnum.PostBar, 0);
                CateAppend(dt);
                return View("CateList",dt);
            }
        }
        public IActionResult CateAdd()
        {
            M_GuestBookCate cateMod = cateBll.SelReturnModel(Mid);
            if (cateMod == null)
            {
                cateMod = new M_GuestBookCate();
                cateMod.ParentID = DataConvert.CLng(GetParam("PID"));
            }
            return View("CateAdd", cateMod);
        }
        public IActionResult CateAdd_Submit(M_GuestBookCate model)
        {
            M_GuestBookCate cateMod = new M_GuestBookCate();
            if (CateID > 0)
            {
                cateMod = cateBll.SelReturnModel(Mid);
            }
            cateMod.CateName = model.CateName;
            cateMod.NeedLog = Convert.ToInt32(GetParam("NeedLog"));
            cateMod.PostAuth = Convert.ToInt32(GetParam("PostAuth"));
            cateMod.ZipImgSize = model.ZipImgSize;
            cateMod.BarImage = GetParam("BarImage_t");
            cateMod.GType = 1;
            cateMod.ParentID = DataConvert.CLng(GetParam("selected_Hid"));
            cateMod.BarOwner = GetParam("BarOwner_Hid");
            cateMod.PermiBit = DataConvert.CLng(GetParam("PermiBit")).ToString();
            cateMod.Desc = model.Desc;
            //cateMod.IsPlat = DataConverter.CLng(IsPlat_T.Text);
            cateMod.SendScore = model.SendScore;
            cateMod.ReplyScore = model.ReplyScore;
            cateMod.Status = model.Status;
            cateMod.IsShowUnaudit = model.IsShowUnaudit;
            GuestConfigInfo guestinfo = GuestConfig.GuestOption;
            GuestConfig config = new GuestConfig();
            if (CateID < 1)
            {
                cateMod.CateID = cateBll.Insert(cateMod);
                guestinfo.BarOption.Add(new BarOption()
                {
                    CateID = cateMod.CateID,
                    UserTime = DataConvert.CLng(GetParam("UserTime_T")),
                    SendTime = DataConvert.CLng(GetParam("SendTime_T"))
                });
                config.Update(guestinfo);
                return WriteOK("添加成功!", "CateList");
            }
            else
            {
                cateBll.Update(cateMod);
                BarOption baroption = guestinfo.BarOption.Find(v => v.CateID == CateID);
                if (baroption == null)
                {
                    guestinfo.BarOption.Add(new BarOption()
                    {
                        CateID = cateMod.CateID,
                        UserTime = DataConvert.CLng(GetParam("UserTime_T")),
                        SendTime = DataConvert.CLng(GetParam("SendTime_T"))
                    });
                }
                else
                {
                    baroption.UserTime = DataConvert.CLng(GetParam("UserTime_T"));
                    baroption.SendTime = DataConvert.CLng(GetParam("SendTime_T"));
                }
                config.Update(guestinfo);
                return WriteOK("添加成功!", "CateList");
            }
        }
        public ContentResult Cate_API()
        {
            string action = GetParam("action");
            string ids = GetParam("ids");
            switch (action)
            {
                case "del":
                    cateBll.DelByCateIDS(ids);
                    break;
                case "recommend":
                    {
                        string[] idArr = ids.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                        foreach (var item in ids)
                        {
                            M_GuestBookCate mc = cateBll.SelReturnModel(Convert.ToInt32(item));
                            mc.BarInfo = "Recommend";
                            cateBll.UpdateByID(mc);
                        }
                    }
                    break;
                case "unrecommend":
                    {
                        string[] idArr = ids.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                        foreach (var item in ids)
                        {
                            M_GuestBookCate mc = cateBll.SelReturnModel(Convert.ToInt32(item));
                            mc.BarInfo = "";
                            cateBll.UpdateByID(mc);
                        }
                    }
                    break;
                case "clear":
                    {
                        //清空栏目下所有数据
                        barBll.DelByCid(DataConvert.CLng(ids));
                    }
                    break;
            }
            return Content(Success.ToString());
        }
        public IActionResult BarAuthSet()
        {
            ViewBag.viewDir = viewDir;
            PageSetting setting = buser.SelBarAuth(CPage, PSize, CateID, GetParam("view"));
            if (Request.IsAjax())
            {
                return PartialView("BarAuthSet_List", setting);
            }
            return View("BarAuthSet", setting);
        }
        public IActionResult BarAuthSet_Submit()
        {
            M_Guest_BarAuth authMod = new M_Guest_BarAuth();
            string[] uidArr = DataConvert.CStr(Request.Form["Uid_Hid"]).Split(',');
            foreach (string uid in uidArr)
            {
                authMod = authBll.SelModelByUid(CateID, DataConvert.CLng(uid));
                bool isnew = false;
                if (authMod == null) { isnew = true; authMod = new M_Guest_BarAuth(); authMod.Uid = DataConvert.CLng(uid); authMod.BarID = CateID; }
                authMod.Look = DataConvert.CLng(RequestEx["Look_"+uid]);
                authMod.Send = DataConvert.CLng(RequestEx["Send_" + uid]);
                authMod.Reply = DataConvert.CLng(RequestEx["Reply_" + uid]);
                if (isnew) { authBll.Insert(authMod); }
                else { authBll.UpdateByID(authMod); }

            }
            return WriteOK("操作成功", "BarAuthSet?CateID=" + CateID + "&View=" + GetParam("view"));
            //foreach (GridViewRow row in EGV.Rows)
            //{
            //    int uid = Convert.ToInt32((row.FindControl("Uid_Hid") as HiddenField).Value);
            //    authMod = authBll.SelModelByUid(cateid, uid);
            //    bool isnew = false;
            //    if (authMod == null) { isnew = true; authMod = new M_Guest_BarAuth(); authMod.Uid = uid; authMod.BarID = BarID; }
            //    authMod.Look = (row.FindControl("Look") as HtmlInputCheckBox).Checked ? 1 : 0;
            //    authMod.Send = (row.FindControl("Send") as HtmlInputCheckBox).Checked ? 1 : 0;
            //    authMod.Reply = (row.FindControl("Reply") as HtmlInputCheckBox).Checked ? 1 : 0;
            //    if (isnew) { authBll.Insert(authMod); }
            //    else { authBll.UpdateByID(authMod); }
            //}

        }
        #region tools
        private void CateAppend(DataTable dt)
        {
            dt.Columns.Add(new DataColumn("NLogStr", typeof(string)));
            dt.Columns.Add(new DataColumn("CountStr", typeof(string)));
            //countdt=barBll.SelYTCount(id.ToString());
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                DataRow dr = dt.Rows[i];
                dr["BarInfo"] = GetBarStatus(dr["BarInfo"].ToString());
                dr["NLogStr"] = GetNeedLog(dr["NeedLog"].ToString());
                dr["CountStr"] = GetCount(dr["CateID"].ToString());
            }
            dt.Columns.Remove("desc");
        }
        //总数/今日/昨日
        private string GetCount(string cateid)
        {
            string result = "";
            string[] countArr = barBll.SelYTCount(cateid).Split(':');
            result = "<span>" + countArr[0] + "</span>/<span>" + countArr[1] + "</span>/<span>" + countArr[2] + "</span>";
            return result;
        }
        private string GetBarStatus(string barInfo)
        {
            string strcolor = "black";
            string restr = "普通";
            if (!string.IsNullOrWhiteSpace(barInfo) && barInfo.Contains("Recommend"))
            {
                strcolor = "blue";
                restr = "推荐";
            }
            return "<span style='color:" + strcolor + "'>" + restr + "</span>";
        }
        private string GetNeedLog(string needlog)
        {
            string result = "";
            switch (needlog)
            {
                case "0":
                    result = "允许匿名";
                    break;
                case "1":
                    result = "登录用户";
                    break;
                case "2":
                    result = "指定用户";
                    break;
                default:
                    result = "未知";
                    break;
            }
            return result;
        }
        #endregion
    }
}