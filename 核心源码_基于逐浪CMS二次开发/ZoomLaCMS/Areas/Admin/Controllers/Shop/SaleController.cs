using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ZoomLa.BLL.Shop;
using ZoomLa.Common;
using ZoomLaCMS.Ctrl;

namespace ZoomLaCMS.Areas.AdminMVC.Controllers
{
    [Area("Admin")]
    [Authorize(Policy = "Admin")]
    public class SaleController : Ctrl_Admin
    {
        private string ActionPath = "/Admin/Sale/";
        F_Order_Sale filter = new F_Order_Sale();
        private F_Order_Sale GetFilter()
        {
            F_Order_Sale filter = new F_Order_Sale();

            filter.stime = RequestEx["stime"];
            filter.etime = RequestEx["etime"];
            if (string.IsNullOrEmpty(filter.stime)) { filter.stime = DateTime.Now.AddMonths(-1).ToString("yyyy/MM/dd"); }
            if (string.IsNullOrEmpty(filter.etime)) { filter.etime = DateTime.Now.ToString("yyyy/MM/dd"); }
            filter.storeIds = RequestEx["storeIDS"];
            filter.fast = RequestEx["fast"];
            return filter;
        }
        public IActionResult Index()
        {
            F_Order_Sale filter = GetFilter();
            filter.action = ActionPath + "Index";
            return View(filter);
        }
        public IActionResult SaleByDay()
        {
            F_Order_Sale filter = GetFilter();
            filter.action = ActionPath + "SaleByDay";
            return View(filter);
        }
        public IActionResult SaleByProduct()
        {
            //DataTable dt = srpBll.GetSalesByProduct(new F_Shop_SaleReport()
            //{
            //    ProName = RequestEx["proname"],
            //    SDate = RequestEx["sdate"],
            //    EDate = RequestEx["edate"],
            //    NodeIDS = RequestEx["NodeID"]
            //});
            //if (Request.IsAjax())
            //{
            //    return PartialView("SaleByProduct_List", dt);
            //}
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
        public IActionResult SaleByUser()
        {
            F_Order_Sale filter = GetFilter();
            filter.action = ActionPath + "SaleByUser";
            return View(filter);
        }
    }
}
