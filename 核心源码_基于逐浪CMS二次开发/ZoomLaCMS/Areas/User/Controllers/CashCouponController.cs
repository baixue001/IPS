using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ZoomLa.BLL;
using ZoomLa.Common;
using ZoomLa.Model;
using ZoomLa.SQLDAL;
using ZoomLa.SQLDAL.SQL;
using ZoomLaCMS.Ctrl;

namespace ZoomLaCMS.Areas.User.Controllers
{
    [Area("User")]
    [Authorize(Policy = "User")]
    public class CashCouponController : Ctrl_User
    {
        B_Arrive avBll = new B_Arrive();
        public void Index()
        {
           Response.Redirect("ArriveManage");
        }
        public IActionResult ArriveManage()
        {
            int type = DataConvert.CLng(RequestEx["Type"], -100);
            int state = DataConvert.CLng(RequestEx["State"], 1);
            int storeID = DataConvert.CLng(RequestEx["StoreID"], -100);
            string addon = RequestEx["addon"] ?? "";
            if (state == 1 && string.IsNullOrEmpty(addon)) { addon = "noexp"; }
            //, mu.UserID, type, state, addon
            PageSetting setting = avBll.SelPage(CPage, PSize, new Filter_Arrive()
            {
                uid = mu.UserID,
                type = type,
                state = state,
                addon = addon,
                storeID = storeID
            });
            if (Request.IsAjaxRequest()) { return PartialView("ArriveManage_List",setting); }
            return View(setting);
        }
        //用户领取优惠券
        public IActionResult GetArrive()
        {
            int storeID = DataConvert.CLng(RequestEx["StoreID"], -100);
            PageSetting setting = avBll.U_SelForGet(CPage, PSize, mu.UserID,storeID);
            if (Request.IsAjax()) { return PartialView("GetArrive_List", setting); }
            return View(setting);
        }
        //领取优惠券
        [HttpPost]
        public IActionResult Arrive_Get(string flow)
        {
            avBll.U_GetArrive(mu.UserID, flow);
            string url = string.IsNullOrEmpty(RequestEx["r_hid"]) ? "GetArrive" : RequestEx["r_hid"];
            return WriteOK("优惠券领取成功",url);
        }
        public IActionResult ArriveJihuo()
        {
            return View();
        }
        public IActionResult Arrive_Act()
        {
            string ANo = RequestEx["ANo"];
            string APwd = RequestEx["APwd"];

            //优惠券的实例
            M_Arrive avMod = avBll.SelReturnModel(ANo, APwd);
            if (avMod == null) { return WriteErr("优惠券不存在");  }
            string str = "优惠券激活成功" + "！此优惠券的面值为[" + avMod.Amount + "]";
            return WriteOK(str,"ArriveJiHuo");
        }
    }
}
