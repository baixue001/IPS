using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ZoomLaCMS.Ctrl;

namespace ZoomLaCMS.Areas.Admin.Controllers.Content
{
    [Area("Admin")]
    [Authorize(Policy = "Admin")]
    public class AskController : Ctrl_Admin
    {
        public IActionResult Default()
        {
            return View();
        }
    }
}