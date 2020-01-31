using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ZoomLa.BLL;
using ZoomLa.BLL.Message;
using ZoomLa.Common;
using ZoomLa.Components;
using ZoomLa.Model;
using ZoomLa.SQLDAL;
using ZoomLa.SQLDAL.SQL;
using ZoomLaCMS.Ctrl;

namespace ZoomLaCMS.Areas.Admin.Controllers.Content
{
    [Area("Admin")]
    [Authorize(Policy = "Admin")]
    [Route("[area]/Content/Baike/[action]")]
    public class BaikeController : Ctrl_Admin
    {
        B_Baike bkBll = new B_Baike();
        B_BaikeEdit editBll = new B_BaikeEdit();
        B_Group groupBll = new B_Group();
        private int ZStatus { get { return DataConvert.CLng(GetParam("Status")); } }
        private string Flow { get { return GetParam("Flow"); } }
        //---------------------
        string viewDir = "/Areas/Admin/Views/Content/Baike/";
        public override ViewResult View(string name, object model = null)
        {
            if (name.EndsWith("cshtml")) { return base.View(name, model); }
            else { return base.View(viewDir + name + ".cshtml", model); }

        }
        public IActionResult Default()
        {
            return Redirect("BKVersionList");
        }
        public IActionResult BKVersionList()
        {
            ViewBag.viewDir = viewDir;
            PageSetting setting = editBll.SelPage(CPage, PSize, new Com_Filter()
            {
                skey = GetParam("skey"),
                addon = Flow
            });
            ViewBag.viewDir = viewDir;
            if (Request.IsAjax())
            {
                return PartialView(viewDir + "BKVersion_List.cshtml", setting);
            }
            return View("BKVersionList", setting);
        }
        [HttpPost]
        public ContentResult Version_API()
        {
            string action = GetParam("action");
            string ids = GetParam("ids");
            switch (action)
            {
                case "audit":
                    {
                        editBll.BatStatus(ids, 1);
                        foreach (string id in ids.Split(",".ToCharArray(), StringSplitOptions.RemoveEmptyEntries))
                        {
                            editBll.Apply(Convert.ToInt32(id));
                        }
                    }
                    break;
                case "unaudit":
                    {
                        editBll.BatStatus(ids, (int)ZLEnum.ConStatus.UnAudit);
                    }
                    break;
                case "reject":
                    {
                        string msg = GetParam("msg").Trim(',');
                        editBll.BatReject(ids, msg);
                    }
                    break;
                case "del":
                    {
                        editBll.DelByIDS(ids);
                    }
                    break;
            }
            return Content(Success.ToString());
        }
        public IActionResult BKList()
        {
            ViewBag.viewDir = viewDir;
            // string VerStr { get { return DataConvert.CStr(ViewState["VerStr"]); } set { ViewState["VerStr"] = value; } }
            M_Baike bkMod = bkBll.SelModelByFlow(Flow);
            PageSetting setting = editBll.SelPage(CPage, PSize, new Com_Filter()
            {
                addon = Flow
            });
            //VerStr = bkMod.VerStr;
            //EGV.DataSource = editBll.SelBy(-100, Flow, "");
            //EGV.DataBind();
            return View("BKList", setting);
        }
        public IActionResult BK_API()
        {
            string action = GetParam("action");
            string ids = GetParam("ids");
            switch (action)
            {
                case "del":
                    editBll.DelByIDS(ids);
                    break;
                case "apply":
                    editBll.Apply(DataConvert.CLng(ids));
                    break;
                    //-----------
                case "audit":
                    {
                        string[] chkArr = ids.Split(",".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                        for (int i = 0; i < chkArr.Length; i++)
                        {
                            int itemID = Convert.ToInt32(chkArr[i]);
                            //ChangeUserExp(bkBll.SelReturnModel(itemID));
                            bkBll.UpdateStatus(1, itemID);
                        }
                    }
                    break;
                case "unaudit":
                    {
                        //取消审核选定的词条
                        string[] chkArr = ids.Split(",".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                        for (int i = 0; i < chkArr.Length; i++)
                        {
                            int itemID = Convert.ToInt32(chkArr[i]);
                            bkBll.UpdateStatus(0, itemID);
                        }
                    }
                    break;
                case "elite":
                    {
                        //推荐词条
                        string[] chkArr = ids.Split(",".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                        for (int i = 0; i < chkArr.Length; i++)
                        {
                            int itemID = Convert.ToInt32(chkArr[i]);
                            ChangeUserElid(bkBll.SelReturnModel(itemID));
                            bkBll.UpdateElite(1, itemID);
                        }
                    }
                    break;
                case "unelite":
                    {
                        //取消推荐词条
                        string[] chkArr = ids.Split(",".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                        for (int i = 0; i < chkArr.Length; i++)
                        {
                            int itemID = Convert.ToInt32(chkArr[i]);
                            bkBll.UpdateElite(0, itemID);
                        }
                    }
                    break;
            }
            return Content(Success.ToString());
        }
        //给创建用户增加分数
        private void ChangeUserExp(M_Baike model)
        {
            if (model.Status != 1 && GuestConfig.GuestOption.BKOption.CreatePoint > 0)
            {
                M_UserInfo mu = buser.SelReturnModel(model.UserId);
                int point = GuestConfig.GuestOption.BKOption.CreatePoint;
                buser.ChangeVirtualMoney(mu.UserID, new M_UserExpHis()
                {
                    score = point,
                    ScoreType = (int)((M_UserExpHis.SType)(Enum.Parse(typeof(M_UserExpHis.SType), GuestConfig.GuestOption.BKOption.PointType))),
                    detail = mu.UserName + "创建了词条[" + model.Tittle + "],增加奖励:" + point
                });
            }
        }
        //推荐用户增加分数
        private void ChangeUserElid(M_Baike model)
        {
            if (model.Elite != 1 && GuestConfig.GuestOption.BKOption.RemmPoint > 0)
            {
                M_UserInfo mu = buser.SelReturnModel(model.UserId);
                int point = GuestConfig.GuestOption.BKOption.RemmPoint;
                buser.ChangeVirtualMoney(mu.UserID, new M_UserExpHis()
                {
                    score = point,
                    ScoreType = (int)((M_UserExpHis.SType)(Enum.Parse(typeof(M_UserExpHis.SType), GuestConfig.GuestOption.BKOption.PointType))),
                    detail = mu.UserName + "创建的词条[" + model.Tittle + "]被管理员设为推荐,增加奖励:" + point
                });
            }
        }
        public IActionResult Config()
        {
            return View("Config");
        }
        public IActionResult Config_Submit()
        {
            GuestConfigInfo guestinfo = GuestConfig.GuestOption;
            guestinfo.BKOption.selGroup = GetParam("selGroup");
            guestinfo.BKOption.CreateBKGroup = GetParam("CreateGroup");
            guestinfo.BKOption.EditGroup = GetParam("EditGroup");
            guestinfo.BKOption.PointType = GetParam("PointType_R");
            guestinfo.BKOption.CreatePoint = DataConverter.CLng(GetParam("CreatePoint_T"));
            guestinfo.BKOption.EditPoint = DataConverter.CLng(GetParam("EditPoint_T"));
            guestinfo.BKOption.RemmPoint = DataConverter.CLng(GetParam("RemmPoint_T"));
            GuestConfig config = new GuestConfig();
            config.Update(guestinfo);
            return WriteOK("保存百科配置成功!", "Config");
        }
        public IActionResult BkCheck()
        {
            ViewBag.viewDir = viewDir;
            //DataTable dts = bkBll.SelectAll(type, key);
            PageSetting setting = bkBll.SelPage(CPage, PSize, new Com_Filter()
            {
                type = GetParam("type"),
                skey = GetParam("skey")
            });
            return View("BkCheck", setting);
        }
    }
}