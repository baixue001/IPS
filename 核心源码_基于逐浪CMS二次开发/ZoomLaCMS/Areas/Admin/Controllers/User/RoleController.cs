using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ZoomLa.BLL;
using ZoomLa.Model;
using ZoomLa.SQLDAL.SQL;
using ZoomLaCMS.Ctrl;

namespace ZoomLaCMS.Areas.Admin.Controllers.User
{
    //用户,管理员,角色&权限配置
    [Area("Admin")]
    [Authorize(Policy = ("Admin"))]
    [Route("[area]/User/Role/[action]")]
    public class RoleController : Ctrl_Admin
    {
        B_Permission urBll = new B_Permission();
        public IActionResult UserRole()
        {
            PageSetting setting = urBll.SelPage(CPage,PSize,new Com_Filter() {

            });
            return View(setting);
        }
        public IActionResult UserRoleAdd()
        {
            M_Permission model = new M_Permission();
            if (Mid > 0) { model = urBll.SelReturnModel(Mid); }
            return View(model);
        }
        public IActionResult UserRoleAdd_Submit(M_Permission model)
        {
            M_Permission urMod = new M_Permission();
            if (Mid > 0) { urMod = urBll.SelReturnModel(Mid); }
            urMod.RoleName = model.RoleName;
            urMod.Info = model.Info;
            if (urMod.ID > 0) { urBll.GetUpdate(urMod); }
            else { urBll.GetInsert(urMod); }
            return WriteOK("操作成功", "UserRole");
        }
        public ContentResult UserRole_API()
        {
            string action = GetParam("action");
            string ids = GetParam("ids");
            switch (action)
            {
                case "del":
                    urBll.DelByIDS(ids);
                    break;
            }
            return Content(Success.ToString());
        }
    }
}