using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ZoomLa.BLL;
using ZoomLa.BLL.Shop;
using ZoomLa.Common;
using ZoomLa.Model;
using ZoomLa.SQLDAL;
using ZoomLa.SQLDAL.SQL;
using ZoomLaCMS.Ctrl;

namespace ZoomLaCMS.Areas.AdminMVC.Controllers
{
    [Area("Admin")]
    [Authorize(Policy="Admin")]
    public class PayController : Ctrl_Admin
    {
        B_PayPlat platBll = new B_PayPlat();
        B_Payment payBll = new B_Payment();
        B_OrderList orderBll = new B_OrderList();
        public IActionResult PayPlat()
        {
            DataTable dt = platBll.GetPayPlatListAll();
            return View(dt);
        }
        [HttpPost]
        public ContentResult PayPlat_API()
        {
            string result = "";
            string action = GetParam("action");
            int id = DataConverter.CLng(GetParam("ids"));
            switch (action)
            {
                case "move":
                    #region 移动
                    {
                        string direct = GetParam("direct");
                        int curid = DataConvert.CLng(GetParam("curid")), tarid = DataConvert.CLng(GetParam("tarid"));
                        M_PayPlat curMod = platBll.SelReturnModel(curid);
                        M_PayPlat tarMod = platBll.SelReturnModel(tarid);
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
                        platBll.UpdateByID(curMod);
                        platBll.UpdateByID(tarMod);
                    }
                    #endregion
                    break;
                case "enable":
                    {
                        bool status = DataConverter.CBool(GetParam("status"));
                        M_PayPlat platMod = platBll.SelReturnModel(id);
                        if (platMod == null) { result = "支付平台[" + id + "]不存在"; }
                        else
                        {
                            platMod.IsDisabled = !status;
                            platBll.UpdateByID(platMod);
                        }
                    }
                    break;
                case "setdef":
                    {
                        platBll.SetDefault(id);
                    }
                    break;
            }
            return Content(Success.ToString());
        }
        public IActionResult PayPlatAdd()
        {
            if (Mid < 1) { return WriteErr("未指定支付平台"); }
            M_PayPlat info = platBll.SelReturnModel(Mid);
            M_PayPlat.Plat plat = (M_PayPlat.Plat)info.PayClass;
            switch (plat)//是否转到有单独配置的页
            {
                case M_PayPlat.Plat.WXPay:
                    Response.Redirect("PayWeiXin");
                    break;
                case M_PayPlat.Plat.PayPal:
                    Response.Redirect("PaypalManage");
                    break;
                case M_PayPlat.Plat.Alipay_Bank:
                    Response.Redirect("AlipayBank");
                    break;
            }
            return View(info);
        }
        [HttpPost]
        public IActionResult PayPlatAdd_Submit(M_PayPlat model)
        {
            M_PayPlat info = platBll.SelReturnModel(Mid);
            info.PayPlatName = model.PayPlatName;
            info.AccountID = model.AccountID;
            info.APPID = model.APPID;
            info.Secret = model.Secret;
            info.Rate = model.Rate;
            info.MD5Key = model.MD5Key;
            info.Remind = model.Remind;
            info.PublicKey = model.PublicKey;
            info.PrivateKey = model.PrivateKey;
            info.ServerPublicKey = model.ServerPublicKey;
            info.PrivateKey_Pwd = model.PrivateKey_Pwd;
            info.IsDisabled = DataConvert.CLng(GetParam("IsDisabled")) == 1 ? false : true;
            platBll.UpdateByID(info);
            return WriteOK("操作成功", "PayPlat");
        }
        public IActionResult PayPalPlat()
        {
            return View();
        }
        public IActionResult WechatPlat()
        {
            M_PayPlat model = B_PayPlat.GetModelForWx();
            return View(model);
        }
        public void WechatPlat_Submit(M_PayPlat model)
        {
            M_PayPlat info = B_PayPlat.GetModelForWx();
            info.APPID = model.APPID;
            info.Secret = model.Secret;
            info.AccountID = model.AccountID;
            info.MD5Key = model.MD5Key;
            platBll.UpdateByID(info);
            WriteOK("操作成功", "WechatPlat");
        }
        public IActionResult AliH5Plat()
        {
            return View();
        }
        public IActionResult AliBankPlat()
        {
            return View();
        }
        public IActionResult PaymentList()
        {
            //[example] 前端字段排序
            PageSetting setting = payBll.SelPage(CPage, PSize, new F_Payment()
            {
                uids = GetParam("uids"),
                uname = GetParam("uname"),
                orderno = GetParam("orderno"),
                payno = GetParam("payno"),
                orderType = GetParam("orderType"),
                status = DataConvert.CLng(GetParam("status"), -100),
                orderBy = GetParam("orderBy")
            });
            if (Request.IsAjaxRequest())
            {
                return PartialView("Payment_List",setting);
            }
            else
            {
                return View(setting);
            }
        }
        public IActionResult PaymentInfo()
        {
            M_Payment payMod = payBll.SelReturnModel(Mid);
            if (payMod == null) { return WriteErr("支付单不存在");return null; }
          
            M_OrderList orderMod = orderBll.SelModelByOrderNo(payMod.PaymentNum);
            M_UserInfo mu = buser.SelReturnModel(orderMod.Userid);
            M_PayPlat platMod = new M_PayPlat();
            if (payMod.Status == 3 && payMod.PayPlatID > 0)
            {
                platMod = platBll.SelReturnModel(payMod.PayPlatID);
            }
            ViewBag.platMod = platMod;
            ViewBag.orderMod = orderMod;
            ViewBag.mu = mu;
            return View(payMod);
        }
        //强制未成功的支付单生效
        [HttpPost]
        public void Payment_Success()
        {
            //M_Payment payMod = payBll.SelReturnModel(Mid);
            //M_OrderList orderMod = orderBll.SelModelByOrderNo(payMod.PaymentNum);
            //payMod.Status = (int)M_Payment.PayStatus.HasPayed;
            //payMod.CStatus = true;
            //payMod.Remark += "(" + "管理员确认支付,ID:" + adminMod.AdminId + ",登录名:" + adminMod.AdminName + ",真实姓名:" + adminMod.AdminTrueName + ")";
            //payBll.Update(payMod);
            //OrderHelper.FinalStep(payMod, orderMod, new M_Order_PayLog());
            //Response.Redirect(Request.RawUrl);
        }

    }
}
