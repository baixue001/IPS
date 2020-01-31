using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ZoomLa.BLL.API;
using ZoomLa.BLL;
using ZoomLa.Common;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ZoomLa.Model;
using ZoomLa.BLL.User.Addon;
using System.Web;
using ZoomLa.BLL.CreateJS;
using System.Data.SqlClient;
using System.Data;
using Microsoft.AspNetCore.Mvc;

namespace ZoomLa.Extend
{
    [Route("ZLPlug/[action]")]
    public class ZLPlugController : Controller
    {
        public IActionResult Test()
        {
            return Content("test");
        }
        //[Route("/User/Content/AddContent")]
        //public IActionResult AddContent()
        //{
        //    return Content("AddContent");
        //}
        M_APIResult retMod = new M_APIResult(M_APIResult.Failed);
    }
}