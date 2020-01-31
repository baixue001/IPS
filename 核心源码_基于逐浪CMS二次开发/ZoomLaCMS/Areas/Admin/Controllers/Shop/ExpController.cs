using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ZoomLa.BLL;
using ZoomLa.BLL.Shop;
using ZoomLa.Model.Shop;
using ZoomLa.SQLDAL.SQL;
using ZoomLaCMS.Ctrl;

namespace ZoomLaCMS.Areas.Admin.Controllers.Shop
{
    [Area("Admin")]
    [Authorize(Policy="Admin")]
    [Route("[area]/Shop/Exp/[action]")]
    public class ExpController : Ctrl_Admin
    {
        B_Shop_FareTlp fareBll = new B_Shop_FareTlp();
        public IActionResult DeliverType()
        {
            PageSetting setting = fareBll.SelPage(CPage, PSize, new Com_Filter() { });
            if (Request.IsAjax())
            {
                return PartialView("DeliverType_List",setting);
            }
            return View(setting);
        }
        public IActionResult DeliverAdd()
        {
            M_Shop_FareTlp fareMod = fareBll.SelReturnModel(Mid);
            if (fareMod == null) { fareMod = new M_Shop_FareTlp(); }
            return View(fareMod);
        }
        public IActionResult DeliverAdd_Submit()
        {
            M_Shop_FareTlp fareMod = new M_Shop_FareTlp();
            if (Mid > 0) { fareMod = fareBll.SelReturnModel(Mid); }
            fareMod.TlpName = GetParam("TlpName_T");
            fareMod.PriceMode = Convert.ToInt32(RequestEx["pricemod_rad"]);
            fareMod.Express = GetParam("Fare_Hid");//运费Json,后期扩展支持地区,重量
                                             //fareMod.EMS = "";
                                             //fareMod.Mail = "";
            fareMod.Remind = GetParam("Remind_T");
            fareMod.Remind2 = GetParam("Remind2_T");
            if (Mid > 0)
            { fareBll.UpdateByID(fareMod); }
            else { fareBll.Insert(fareMod); }
            return WriteOK("操作成功");
        }
        public ContentResult Deliver_API()
        {
            string action = GetParam("action");
            string ids = GetParam("ids");
            switch (action)
            {
                case "del":
                    fareBll.DelByIds(ids);
                    break;
            }
            return Content(Success.ToString());
        }
    }
}