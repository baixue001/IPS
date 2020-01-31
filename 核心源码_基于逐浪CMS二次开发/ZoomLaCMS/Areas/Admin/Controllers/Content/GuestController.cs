using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ZoomLa.BLL;
using ZoomLa.Model;
using ZoomLa.SQLDAL;
using ZoomLa.SQLDAL.SQL;
using ZoomLaCMS.Ctrl;

namespace ZoomLaCMS.Areas.Admin.Controllers.Content
{
    [Area("Admin")]
    [Authorize(Policy = "Admin")]
    [Route("/[area]/Content/Guest/[action]")]
    public class GuestController : Ctrl_Admin
    {
        string viewDir = "/Areas/Admin/Views/Content/Guest/";
        public override ViewResult View(string name, object model = null)
        {
            if (name.EndsWith("cshtml")) { return base.View(name,model); }
            else { return base.View(viewDir + name + ".cshtml", model); }
            
        }
        B_GuestBookCate cateBll = new B_GuestBookCate();
        B_GuestBook guestBll = new B_GuestBook();
        public int CateID { get { return DataConvert.CLng(RequestEx["CateID"]); } }
        public IActionResult Default() { return Redirect("CateList"); }
        //留言栏目
        public IActionResult CateList()
        {
            DataTable dt = cateBll.SelByGuest();
            return View("CateList", dt);
        }
        public IActionResult CateAdd()
        {
            M_GuestBookCate cateMod = cateBll.SelReturnModel(Mid);
            if (cateMod == null)
            {
                cateMod = new M_GuestBookCate();
            }
            return View("CateAdd", cateMod);
        }
        public IActionResult CateAdd_Submit(M_GuestBookCate model)
        {
            M_GuestBookCate cateMod = cateBll.SelReturnModel(Mid);
            if (cateMod == null)
            {
                cateMod = new M_GuestBookCate();
            }
            cateMod.CateName = model.CateName;
            cateMod.Status = model.Status;
            cateMod.IsShowUnaudit =DataConvert.CLng(GetParam("IsShowUnaudit"));
            cateMod.Desc = model.Desc;
            if (cateMod.CateID > 0) { cateBll.UpdateByID(cateMod); }
            else { cateBll.Insert(cateMod); }
            return JavaScript("<script>parent.mybind();</script>");
        }
        public ContentResult Cate_API()
        {
            string action = GetParam("action");
            string ids = GetParam("ids");
            switch (action)
            {
                case "del":
                    cateBll.DelByCateIDS(ids);
                    break;
            }
            return Content(Success.ToString());
        }
        public IActionResult MsgList()
        {
            ViewBag.viewDir = viewDir;
            int Status = DataConvert.CLng(GetParam("status"), 0);
            PageSetting setting = B_GuestBook.SelPage(CPage, PSize, new F_Guest()
            {
                status = Status,
                skey = GetParam("skey"),
                cateIds = CateID.ToString()
            });
            if (Request.IsAjax())
            {
                return PartialView("Msg_List", setting);
            }
            return View("MsgList", setting);
        }
        //单个留言详情,弹窗
        public IActionResult MsgShow()
        {
            M_GuestBook msgMod = guestBll.SelReturnModel(Mid);
            if (msgMod.IsNull) { return WriteErr("指定的留言不存在"); }

            return View("MsgShow",msgMod);
        }
        public IActionResult MsgShow_Submit(M_GuestBook model)
        {
            M_GuestBook msgMod = guestBll.SelReturnModel(Mid);
            if (msgMod.IsNull) { return WriteErr("信息不存在"); }
            msgMod.Title = model.Title;
            msgMod.TContent = model.TContent;
            guestBll.UpdateByID(msgMod);
            return WriteOK("操作成功", "MsgShow?ID=" + Mid);
        }
        public ContentResult Msg_API()
        {
            string action = GetParam("action");
            string ids = GetParam("ids");
            switch (action)
            {
                case "recycle":
                    B_GuestBook.UpdateAudit(ids, -1);
                    break;
                case "recover":
                    B_GuestBook.UpdateAudit(ids, 1);
                    break;
                case "del":
                    B_GuestBook.DelByIDS(ids);
                    break;
                case "audit":
                    B_GuestBook.UpdateAudit(ids, 1);
                    break;
                case "unaudit":
                    B_GuestBook.UpdateAudit(ids, 0);
                    break;
            }
            return Content(Success.ToString());
        }
    }
}