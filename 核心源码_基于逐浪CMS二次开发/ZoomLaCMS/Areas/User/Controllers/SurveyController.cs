using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ZoomLa.BLL;
using ZoomLa.BLL.Design;
using ZoomLa.Common;
using ZoomLa.SQLDAL;
using ZoomLa.SQLDAL.SQL;
using ZoomLaCMS.Ctrl;

namespace ZoomLaCMS.Areas.User.Controllers
{
    [Area("User")]
    [Authorize(Policy = "User")]
    public class SurveyController : Ctrl_User
    {
        B_Design_Ask askBll = new B_Design_Ask();
        public void Index()
        {
            Response.Redirect("SurveyAll");
        }
        public IActionResult SurveyAll()
        {
            PageSetting setting = askBll.SelPage(CPage, PSize, new Com_Filter()
            {

            });
            if (Request.IsAjax())
            {
                return PartialView("SurveyAll_List", setting);
            }
            else
            {
                return View(setting);
            }
        }
        public void Survey_Add() { }//目标模块相关方法,必须要实现,
        public int Survey_Del(string ids) { return Success; } 
    }
}
