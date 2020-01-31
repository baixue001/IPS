using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ZoomLa.BLL;
using ZoomLa.BLL.Content;
using ZoomLa.BLL.Shop;
using ZoomLa.BLL.User;
using ZoomLa.Common;
using ZoomLa.Model;
using ZoomLa.Model.Shop;
using ZoomLa.SQLDAL;
using ZoomLa.SQLDAL.SQL;
using ZoomLaCMS.Areas.Admin.Models;
using ZoomLaCMS.Ctrl;
using ZoomLaCMS.Models.Field;

namespace ZoomLaCMS.Areas.User.Controllers
{
    [Area("User")]
    [Authorize(Policy = "User")]
    public class InfoController : Ctrl_User
    {
        B_Order_InvTlp invtBll = new B_Order_InvTlp();
        B_UserRecei receBll = new B_UserRecei();
        B_Group gpBll = new B_Group();
        B_ModelField fieldBll = new B_ModelField();
        B_UserBaseField ubfbll = new B_UserBaseField();
        B_History hisBll = new B_History();
        //B_UserRecei receBll = new B_UserRecei();
        B_UserBaseField ubBll = new B_UserBaseField();
        //B_Order_InvTlp invtBll = new B_Order_InvTlp();
        #region 用户信息管理
        public IActionResult Index()
        {
            ViewBag.gpMod = gpBll.SelReturnModel(mu.GroupID);
            return View();
        }
        public void UserInfo() { Response.Redirect("Index");  }
        public IActionResult UserBase()
        {
            M_Uinfo basemu = buser.GetUserBaseByuserid(mu.UserID);
            ViewBag.gpMod = gpBll.SelReturnModel(mu.GroupID);
            ViewBag.basemu = basemu;
            DataTable valueDT = DBCenter.SelTop(1, "UserID", "*", "ZL_UserBase", "UserID=" + mu.UserID, "");
            ModelConfig modcfg = new ModelConfig() { Source = ModelConfig.SType.Admin, ValueDT = valueDT };
            VM_FieldModel model = new VM_FieldModel(ubBll.Select_All(), modcfg);
            ViewBag.htmlMod = model;
            return View(mu);
        }
        public IActionResult UserBase_Edit()
        {
            DataTable dt = ubfbll.Select_All();
            DataTable table;
            try
            {
                table = Call.GetDTFromMVC(dt, Request);
            }
            catch (Exception e)
            {
                return WriteErr(e.Message);
            }
            mu.UserFace = HttpUtility.HtmlEncode(RequestEx["UserFace_T"]);
            mu.HoneyName = HttpUtility.HtmlEncode(RequestEx["txtHonName"]);
            mu.CompanyName = HttpUtility.HtmlEncode(RequestEx["CompanyName"]);
            mu.TrueName = HttpUtility.HtmlEncode(RequestEx["tbTrueName"]);
            M_Uinfo binfo = buser.GetUserBaseByuserid(mu.UserID);
            binfo.Address = RequestEx["tbAddress"];
            binfo.BirthDay = RequestEx["tbBirthday"];
            binfo.UserFace = mu.UserFace;
            binfo.Fax = RequestEx["tbFax"];
            binfo.HomePage = RequestEx["tbHomepage"];
            //binfo.ICQ = Server.HtmlEncode(tbICQ.Text.Trim());
            binfo.HomePhone = RequestEx["tbHomePhone"];
            binfo.IDCard = RequestEx["tbIDCard"];
            binfo.Mobile = HttpUtility.HtmlEncode(RequestEx["tbMobile"]);
            binfo.OfficePhone = RequestEx["tbOfficePhone"];
            binfo.Privating = DataConvert.CLng(RequestEx["tbPrivacy"]);
            //binfo.PHS = Server.HtmlEncode(tbPHS.Text.Trim());
            binfo.QQ = RequestEx["tbQQ"];
            binfo.Sign = RequestEx["tbSign"];
            binfo.UC = RequestEx["tbUC"];
            binfo.UserSex = DataConverter.CBool(RequestEx["tbUserSex"]);
            //binfo.Yahoo = Server.HtmlEncode(tbYahoo.Text.Trim());
            binfo.ZipCode = HttpUtility.HtmlEncode(RequestEx["tbZipCode"]);
            binfo.HoneyName = mu.HoneyName;
            binfo.TrueName = mu.TrueName;
            string[] adrestr = GetParam("address").Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            binfo.Province = adrestr[0];
            binfo.City = adrestr[1];
            binfo.County = adrestr[2];
            binfo.Position = HttpUtility.HtmlEncode(RequestEx["Position"]);
            buser.UpDateUser(mu);
            if (binfo.IsNull)
            {
                binfo.UserId = mu.UserID;
                buser.AddBase(binfo);
            }
            else
            {
                buser.UpdateBase(binfo);//更新用户信息 
            }
            if (table.Rows.Count > 0)
            {
                buser.UpdateUserFile(binfo.UserId, table);
            }
            return WriteOK("修改成功", "UserBase");
        }
        public IActionResult DredgeVip() { return View(); }
        public void DardgeVip_Open()
        {
        }
        #endregion
        #region 地址操作
        public IActionResult UserRecei()
        {
            PageSetting setting = receBll.SelByUid_SPage(CPage, PSize, mu.UserID);
            if (Request.IsAjaxRequest()) { return PartialView("UserRecel_List", setting); }
            return View(setting);
        }
        public int Recei_Del(int id)
        {
            receBll.DeleteByGroupID(id);
            return 1;
        }
        public int Recei_SetDef(int id)
        {
            receBll.SetDef(id);
            return 1;
        }
        public IActionResult AddUserAddress()
        {
            M_UserRecei model = new M_UserRecei();
            if (Mid > 0)
            {
                model = receBll.GetSelect(Mid, mu.UserID);
                if (model == null) { return WriteErr("修改的地址不存在"); return Content(""); }
                if (!model.phone.Contains("-")) { model.phone = "-"; }
            }
            return View(model);
        }
        public string Address_Add()
        {
            M_UserRecei model = new M_UserRecei();
            if (Mid > 0)
            {
                model = receBll.GetSelect(Mid, mu.UserID);
            }
            model.UserID = mu.UserID;
            model.Email = mu.Email;
            //model.Provinces = Request.Form["province_dp"] + "|" + Request.Form["city_dp"] + "|" + Request.Form["county_dp"];
            //model.CityCode = Request.Form["province_dp"] + "|" + Request.Form["city_dp"] + "|" + Request.Form["county_dp"];
            //model.CityCode = province_dp.SelectedValue + " " + city_dp.ValueSelectedValue + " " + county_dp.SelectedValue;
            model.Provinces = RequestEx["pro_hid"];
            model.Street = RequestEx["Street_T"];
            model.Zipcode = RequestEx["ZipCode_T"];
            model.ReceivName = RequestEx["ReceName_T"];
            model.MobileNum = RequestEx["MobileNum_T"];
            //model.phone = Request.Form["Area_T"] + "-" + Request.Form["Phone_T"];
            model.isDefault = DataConvert.CLng(RequestEx["Def_chk"]);
            if (Mid > 0)
            { receBll.GetUpdate(model); }
            else
            { model.ID = receBll.GetInsert(model); }
            if (model.isDefault == 1) { receBll.SetDef(model.ID); }
            return Success.ToString();
        }
        #endregion
        #region 发票管理
        public IActionResult Invoice()
        {
            PageSetting setting = invtBll.SelPage(1, invtBll.MaxCount, mu.UserID);
            if (Request.IsAjax())
            {
                return PartialView("Invoice_List", setting);
            }
            return View(setting);
        }
        public IActionResult InvoiceAdd()
        {
            M_Order_Invoice invMod = new M_Order_Invoice();
            if (Mid > 0)
            {
                invMod = invtBll.SelReturnModel(Mid);
                if (invMod.UserID != mu.UserID)
                { return WriteErr("你无权操作该内容", "Invoice"); }
            }
            return View(invMod);
        }
        public IActionResult Invoice_Add(M_Order_Invoice invMod)
        {
            if (Mid < 1)
            {
                invMod.UserID = mu.UserID;
                invtBll.Insert(invMod);
            }
            else
            {
                M_Order_Invoice oldMod = invtBll.SelReturnModel(Mid);
                if (oldMod.UserID != mu.UserID) { return WriteErr("你无权操作该内容", "Invoice"); }
                invMod.ID = oldMod.ID;
                invMod.UserID = oldMod.UserID;
                invMod.UserMobile = oldMod.UserMobile;
                invMod.UserEmail = oldMod.UserEmail;
                invtBll.UpdateByID(invMod);
            }
            return WriteOK("操作成功", "Invoice");
        }
        public string Invoice_API()
        {
            string action = RequestEx["action"];
            switch (action)
            {
                case "del":
                    {
                        invtBll.Del(RequestEx["ids"], mu.UserID);
                    }
                    break;
            }
            return Success.ToString();
        }
        #endregion
        public void Listprofit() { Response.Redirect("ConsumeDetail?SType=1");  }
        public IActionResult ConsumeDetail()
        {
            int type = DataConvert.CLng(GetParam("SType"));
            PageSetting setting = hisBll.SelPage(CPage, PSize, type, mu.UserID,GetParam("skey_t"),GetParam("stime_t"),GetParam("etime_t"));
            if (Request.IsAjax())
            {
                return PartialView("ConsumeDetail_List", setting);
            }
            return View(setting);
        }
        public IActionResult CardView() { return View(); }
        public IActionResult MySubscription()
        {
            PageSetting setting = new PageSetting();
            setting.dt = new DataTable();
            return View(setting);
        }
    }
}