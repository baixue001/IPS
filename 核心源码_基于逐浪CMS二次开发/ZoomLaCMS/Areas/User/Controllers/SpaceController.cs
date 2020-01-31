using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ZoomLa.BLL;
using ZoomLa.BLL.User;
using ZoomLa.Common;
using ZoomLa.Components;
using ZoomLa.Model;
using ZoomLaCMS.Ctrl;

namespace ZoomLaCMS.Areas.User.Controllers
{
    [Area("User")]
    [Authorize(Policy = "User")]
    public class SpaceController : Ctrl_User
    {

        B_User_BlogStyle bsBll = new B_User_BlogStyle();
        M_User_BlogStyle bsMod = null;
        public int Uid { get { return DataConverter.CLng(RequestEx["id"]); } }
        public void Index()
        {
            Response.Redirect("SpaceManage");
        }
        public IActionResult SpaceManage() {
            B_CreateHtml bll = new B_CreateHtml(HttpContext);
            string errtitle = "<h3 class='panel-title'><span class='zi zi_exclamationCircle'></span> 不正确的访问</h3>";
            int id = DataConverter.CLng(RequestEx["id"]);
            M_UserInfo mu = buser.SelReturnModel(Uid);
            if (mu.IsNull) { return WriteErr(errtitle, "主页ID违规，请使用/User/Space/SpaceManage?id=[uid]方式访问", "");  }
            else if (mu.State != 2) { return WriteErr(errtitle, "未通过认证会员无法开启个人主页! !", "");  }
            else if (mu.PageID < 1) { return WriteErr(errtitle, "用户未指定模板", "");  }
        
            if (bsMod == null) { return WriteErr(errtitle, "指定的风格不存在", "");  }
            string tlp = function.VToP(SiteConfig.SiteOption.TemplateDir + bsMod.UserIndexStyle);
            if (!System.IO.File.Exists(tlp)) { return WriteErr(errtitle, "模板文件[" + bsMod.UserIndexStyle + "]不存在", "");  }
            string html = SafeSC.ReadFileStr(tlp);
            ViewBag.conhtml = bll.CreateHtml(html);
            return View();
        }
    }
}
