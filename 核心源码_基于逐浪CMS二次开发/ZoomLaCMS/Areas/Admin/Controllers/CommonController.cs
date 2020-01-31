using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using ZoomLa.BLL;
using ZoomLa.BLL.Content;
using ZoomLa.Model;
using ZoomLa.Model.Content;
using ZoomLa.SQLDAL;
using ZoomLaCMS.Ctrl;
using ZoomLaCMS.Models.Field;

namespace ZoomLaCMS.Areas.AdminMVC.Controllers
{
    [Area("Admin")]
    [Authorize(Policy="Admin")]
    public class CommonController : Ctrl_Admin
    {
        // GET: /Admin/Common/
        B_Content conBll = new B_Content();
        B_UserBaseField ubBll = new B_UserBaseField();
        public IActionResult UserBaseField(int uid,int mode)
        {
            //@Html.Partial("Fields/Index_User_AddContent", new VM_FieldModel(Model.ModelID, Model.NodeID, new ModelConfig() { Source = ModelConfig.SType.UserContent }, Model.GeneralID))
            DataTable valueDT = DBCenter.SelTop(1, "UserID", "*", "ZL_UserBase", "UserID=" + uid, "");
            ModelConfig modcfg = new ModelConfig() { Source = ModelConfig.SType.Admin, ValueDT = valueDT };
            modcfg.Mode = (ModelConfig.SMode)mode;
            VM_FieldModel model = new VM_FieldModel(ubBll.Select_All(), modcfg);
            return View("Fields/Index_User_AddContent", model);
        }
        //用于ShowContent
        public IActionResult ContentField(int gid)
        {
            int mode = DataConvert.CLng(Request.Form["mode"]);
            M_CommonData mc = conBll.SelReturnModel(gid);
            DataTable contentDT = conBll.GetContentByItems(mc.TableName, mc.GeneralID);
            VM_FieldModel model = new VM_FieldModel(mc.ModelID, mc.NodeID, new ModelConfig() { Source = ModelConfig.SType.Admin, Mode = (ModelConfig.SMode)mode }, mc.GeneralID, contentDT.Rows[0]);
            //@Html.Partial("Fields/Index_Admin_AddContent", new VM_FieldModel(Model.ModelID, Model.NodeID, )
            return View("Fields/Index_Admin_AddContent", model);
        }
    }
}
