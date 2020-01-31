using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ZoomLa.BLL;
using ZoomLa.BLL.Message;
using ZoomLa.SQLDAL;
using ZoomLa.SQLDAL.SQL;
using ZoomLaCMS.Ctrl;

namespace ZoomLaCMS.Areas.User.Controllers
{
    [Area("User")]
    [Authorize(Policy = "User")]
    public class GuestController : Ctrl_User
    {
        B_BaikeEdit editBll = new B_BaikeEdit();
        public void Index()
        {
            Response.Redirect("MyAnswer"); 
        }
        public void Default() { Response.Redirect("MyAnswer");  }
        public IActionResult BaikeContribution()
        {
            PageSetting setting = editBll.SelPage(CPage, PSize,new Com_Filter() {
                uids=mu.UserID.ToString(),
                status=RequestEx["status"]
            });
            return View(setting);
        }
        public PartialViewResult Baike_Data()
        {
            PageSetting setting = editBll.SelPage(CPage, PSize, new Com_Filter()
            {
                uids = mu.UserID.ToString(),
                status = RequestEx["status"]
            });
            return PartialView("BaikeContribution_List", setting);
        }
        public IActionResult AskComment()
        {
            PageSetting setting = new PageSetting();
            setting.dt = new DataTable();
            return View(setting);
        }
        public IActionResult BaikeDraft()
        {
            PageSetting setting = new PageSetting();
            setting.dt = new DataTable();
            return View(setting);
        }
        public IActionResult BaikeFavorite()
        {
            PageSetting setting = new PageSetting();
            setting.dt = new DataTable();
            return View(setting);
        }
        public IActionResult MyAnswer()
        {
            PageSetting setting = new PageSetting();
            setting.dt = new DataTable();
            return View(setting);
        }
        public IActionResult MyApproval()
        {
            PageSetting setting = new PageSetting();
            setting.dt = new DataTable();
            return View(setting);
        }
        public IActionResult MyAsk()
        {
            PageSetting setting = new PageSetting();
            setting.dt = new DataTable();
            return View(setting);
        }
    }
}
