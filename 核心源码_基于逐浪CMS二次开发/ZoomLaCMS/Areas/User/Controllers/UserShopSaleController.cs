using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ZoomLa.BLL.Shop;
using ZoomLa.Common;
using ZoomLa.Model.Shop;
using ZoomLa.SQLDAL;
using ZoomLaCMS.Ctrl;

namespace ZoomLaCMS.Areas.User.Controllers
{
    [Area("User")]
    [Authorize(Policy = "User")]
    public class UserShopSaleController : Ctrl_User
    {
        private string ActionPath = "/User/UserShopSale/";
        F_Order_Sale filter = new F_Order_Sale();
        private F_Order_Sale GetFilter()
        {
            F_Order_Sale filter = new F_Order_Sale();
            M_Store_Info storeMod = storeBll.SelModelByUser(mu.UserID);
            filter.stime = RequestEx["stime"];
            filter.etime = RequestEx["etime"];
            if (string.IsNullOrEmpty(filter.stime)) { filter.stime = DateTime.Now.AddMonths(-1).ToString("yyyy/MM/dd"); }
            if (string.IsNullOrEmpty(filter.etime)) { filter.etime = DateTime.Now.ToString("yyyy/MM/dd"); }
            filter.storeIds = storeMod.ID.ToString();
            filter.fast = RequestEx["fast"];

            ViewBag.storeMod = storeMod;
            return filter;
        }
        B_Store_Info storeBll = new B_Store_Info();
        
        public IActionResult Index()
        {
            F_Order_Sale filter = GetFilter();
            filter.action = ActionPath + "Index";
            return View(filter);
        }
        
        public IActionResult SaleByProduct()
        {
            F_Order_Sale filter = GetFilter();
            filter.action = ActionPath + "SaleByProduct";
            return View(filter);
        }
        
        public IActionResult SaleByClass()
        {
            F_Order_Sale filter = GetFilter();
            filter.action = ActionPath + "SaleByClass";
            return View(filter);
        }
        
        public IActionResult SaleByDay()
        {
            F_Order_Sale filter = GetFilter();
            filter.action = ActionPath + "SaleByDay";
            return View(filter);
        }
        
        public IActionResult SaleByUser()
        {
            F_Order_Sale filter = GetFilter();
            filter.action = ActionPath + "SaleByUser";
            return View(filter);
        }
    }
}
