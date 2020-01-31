using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ZoomLaCMS.Ctrl;

namespace ZoomLaCMS.Areas.Admin.Controllers.Extend
{
    [Area("Admin")]
    [Authorize(Policy = "Admin")]
    [Route("[area]/Extend/[controller]/[action]")]
    public class SurveyController : Ctrl_Admin
    {
        public IActionResult SurveyManage()
        {
            return View();
        }
        public ContentResult Survey_API()
        {
            return Content(Success.ToString());
        }
        public IActionResult SurveyAdd()
        {
            return View();
        }
        public IActionResult SurveyAdd_Submit()
        {
            return WriteOK("操作成功", "SurveyManage");
        }
        public IActionResult SurveyItemList()
        {
            return View();
        }
        public IActionResult SurveyItemAdd()
        {
            return View();
        }
        public IActionResult SurveyItemAdd_Submit()
        {
            return WriteOK("添加成功");
        }
        //SurveyResult
        public IActionResult SurveyResultChart()
        {
            return View();
        }
        public IActionResult SurveyResultList()
        {
            return View();
        }
    }
}