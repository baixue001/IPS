using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ZoomLa.BLL;
using ZoomLa.BLL.Content;
using ZoomLa.Common;
using ZoomLa.Model;
using ZoomLa.SQLDAL;
using ZoomLa.SQLDAL.SQL;
using ZoomLaCMS.Ctrl;

namespace ZoomLaCMS.Areas.Admin.Controllers.Shop
{
    [Area("Admin")]
    [Authorize(Policy = "Admin")]
    [Route("[area]/Shop/Coupon/[action]")]
    public class CouponController : Ctrl_Admin
    {
        B_Content conBll = new B_Content();
        B_Arrive avBll = new B_Arrive();
        public IActionResult ArriveManage()
        {
            PageSetting setting = avBll.SelPage(CPage, PSize, new Filter_Arrive()
            {
                skey = GetParam("skey"),
                type = DataConvert.CLng(GetParam("Type_DP"), -100),
                uid = DataConvert.CLng(GetParam("IsBind_Hid"), -100)
            });
            if (Request.IsAjaxRequest())
            {
                return PartialView("Arrive_List", setting);
            }
            else
            {
                return View(setting);
            }
            //function.Script(this, "showtab('" + IsBind_Hid.Value + "');");
        }
        public ContentResult Arrive_API()
        {
            string action = GetParam("action");
            switch (action)
            {
                case "del":
                    avBll.DelByIDS(ids);
                    break;
                case "active":
                    avBll.ActiveByIDS(ids);
                    break;
            }
            return Content(Success.ToString());
        }
        public IActionResult ArriveAdd()
        {
            M_Arrive avMod = new M_Arrive();
            if (Mid > 0) { avMod = avBll.SelReturnModel(Mid); }
            DataTable storeDT = conBll.Store_Sel("", true);

            if (Mid > 0)
            {
                //switch (avMod.Type)
                //{
                //    case 1:
                //        Amount2_T.Text = avMod.Amount.ToString("F2");
                //        Amount2_Max.Text = avMod.Amount_Max.ToString("F2");
                //        break;
                //    case 2:
                //        break;
                //    default:
                //        Amount_T.Text = avMod.Amount.ToString();
                //        break;
                //}
                //if (avMod.State > 0) { txtState.Text = avMod.State == 1 ? "已激活" : "已使用"; }
                //M_UserInfo info = buser.GetUserByUserID(avMod.UserID);
                //if (!info.IsNull)
                //{
                //    txtUserID.Text = info.UserName;
                //}
                //else
                //{
                //    txtUserID.Text = "未送出";
                //}
            }
            else
            {
                avMod.AgainTime = DateTime.Now;
                avMod.EndTime = DateTime.Now.AddYears(1);
            }
            ViewBag.storeDT = storeDT;
            return View(avMod);
        }
        public IActionResult ArriveAdd_Submit()
        {
            M_Arrive avMod = new M_Arrive();
            if (Mid > 0) { avMod = avBll.SelReturnModel(Mid); }
            avMod.ArriveName = GetParam("txtName");
            avMod.MinAmount = DataConverter.CDouble(GetParam("minAmount_T"));
            avMod.MaxAmount = DataConverter.CDouble(GetParam("maxAmount_T"));
            avMod.Amount_Max = DataConverter.CDouble(GetParam("Amount2_Max"));

            avMod.DateDays = DataConverter.CLng(GetParam("DateDays_T"));
            avMod.AgainTime = DataConverter.CDate(GetParam("AgainTime_T"));
            avMod.EndTime = DataConverter.CDate(GetParam("EndTime_T"));

            avMod.Type = DataConverter.CLng(Request.Form["Type_Rad"]);
            avMod.DateType = DataConverter.CLng(Request.Form["date_rad"]);
            avMod.ProIDS = GetParam("UProIDS_Hid");
            avMod.StoreID = DataConverter.CLng(GetParam("Store_DP"));
            avMod.State = DataConvert.CLng(GetParam("state_chk"));
            avMod.CAdmin = adminMod.AdminId;
            switch (avMod.Type)
            {
                case 1:
                    avMod.Amount = DataConverter.CDouble(GetParam("Amount2_T"));
                    break;
                default:
                    avMod.Amount = DataConverter.CDouble(GetParam("Amount_T"));
                    break;
            }
            //----------------------------------------
            if (avMod.ID < 1)//添加优惠券
            {
                int num = DataConverter.CLng(GetParam("txtCreateNum"));
                CommonReturn retMod = avBll.CreateArrive(avMod, num, GetParam("EcodeType"), GetParam("UserID_Hid"));
                if (retMod.isok) { return WriteOK("批量添加成功!", "ArriveManage?name=" + avMod.ArriveName); }
                else { return WriteErr(retMod.err); }
            }
            else
            {
                avMod.ArriveNO = GetParam("txtNo");
                avMod.ArrivePwd = GetParam("txtPwd");
                avBll.GetUpdate(avMod);
                return WriteOK("操作成功", "ArriveManage");
            }

        }
        public IActionResult PromoList()
        {
            return View();
        }
        public ContentResult Promo_API()
        {
            switch (action)
            {
                case "del":
                    break;
            }
            return Content(Success.ToString());
        }
        public IActionResult PromoAdd()
        {
            return View();
        }
        public IActionResult PromoAdd_Submit()
        {
            return WriteOK("操作成功", "PromoList");
        }
    }
}