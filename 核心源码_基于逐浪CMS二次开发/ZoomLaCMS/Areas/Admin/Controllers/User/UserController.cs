using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ZoomLa.BLL;
using ZoomLa.BLL.User;
using ZoomLa.Common;
using ZoomLa.Model;
using ZoomLa.Safe;
using ZoomLa.SQLDAL;
using ZoomLa.SQLDAL.SQL;
using ZoomLaCMS.Ctrl;

namespace ZoomLaCMS.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Policy = "Admin")]
    public class UserController : Ctrl_Admin
    {
        B_Admin badmin = new B_Admin();
        public IActionResult UserManage()
        {
            F_User filter = new F_User();
            filter.groupIds = GetParam("GroupID");
            filter.uids = GetParam("UserID_T");
            filter.uname = GetParam("UName_T");
            filter.orderBy = GetParam("orderBy");
            PageSetting setting = buser.SelPage(CPage, PSize, filter);
            if (Request.IsAjax()) {
                return PartialView("UserManage_List", setting);
            }
            else {
                return View(setting);
            }
        }
        public IActionResult UserAdd() { return View(); }
        public IActionResult UserAdd_Submit(M_UserInfo input)
        {
            if (buser.IsExist("uname", input.UserName)) { return WriteErr("用户名已存在"); }
           
            CommonReturn ret = buser.CheckRegInfo(input);
            if (!ret.isok) { return WriteErr(ret.err); }
            input.UserFace = GetParam("UserFace_t");
            input.UserPwd = StringHelper.MD5(input.UserPwd);
            input.UserID = buser.Add(input);
            M_Uinfo basemu = new M_Uinfo();
            basemu.UserId = input.UserID;
            basemu.Mobile = input.Remark;
            buser.AddBase(basemu);
            return WriteOK("添加成功","UserAdd");
        }
        public IActionResult UserInfo() {
            M_UserInfo mu = buser.SelReturnModel(Mid);
            if (mu.IsNull) { return WriteErr("用户不存在"); }
            mu.UserBase = buser.GetUserBaseByuserid(mu.UserID);
            return View(mu);
        }
        public IActionResult UserInfo_Submit(M_UserInfo model)
        {
            M_UserInfo info = buser.SelReturnModel(Mid);
            M_Uinfo binfo = buser.GetUserBaseByuserid(info.UserID);
            if (info.IsNull) { return WriteErr("用户不存在"); }
            if (!string.IsNullOrEmpty(GetParam("NewPwd")))
            {
                info.UserPwd = StringHelper.MD5(GetParam("NewPwd"));
            }
            if (!string.IsNullOrEmpty(GetParam("PayPwd")))
            {
                info.PayPassWord = StringHelper.MD5(GetParam("PayPwd"));
            }
            if (string.IsNullOrEmpty(model.Email)) { return WriteErr("邮箱不能为空"); }
            if (!info.Email.Equals(model.Email))
            { 
                if (buser.IsExist("email", model.Email)) { return WriteErr("["+model.Email+"]邮箱已被注册"); }
                info.Email = model.Email;
            }
            info.GroupID = model.GroupID;
            info.ParentUserID = model.ParentUserID;
            info.UserFace = GetParam("UserFace_t");
            info.HoneyName = model.HoneyName;
            info.TrueName = model.TrueName;


            info.ConsumeExp = model.ConsumeExp;
            info.CompanyName = model.CompanyName;
            info.CompanyDescribe = model.CompanyDescribe;
            info.WorkNum = model.WorkNum;
            info.Question = model.Question;
            info.Answer = model.Answer;

            binfo.UserSex = DataConvert.CBool(GetParam("UserSex"));
            binfo.BirthDay = model.UserBase.BirthDay;
            binfo.OfficePhone = model.UserBase.OfficePhone;
            binfo.HomePhone = model.UserBase.HomePhone;
            binfo.Mobile = model.UserBase.Mobile;
            binfo.Fax = model.UserBase.Fax;
            binfo.ZipCode = model.UserBase.ZipCode;
            binfo.IDCard = model.UserBase.IDCard;
            binfo.HomePage = model.UserBase.HomePage;
            binfo.Address = model.UserBase.Address;
            binfo.QQ = model.UserBase.QQ;
            binfo.UserFace = info.UserFace;
            binfo.Sign = model.UserBase.Sign;
            binfo.Province = GetParam("selprovince");
            binfo.City = GetParam("selcity");
            binfo.County = GetParam("selcoutry");

            if (info.UserID.ToString() == info.ParentUserID) { return WriteErr("推荐人不能是用户本身"); }
            buser.UpdateByID(info);
            if (binfo.UserId < 1) { binfo.UserId = info.UserID; buser.AddBase(binfo); }
            else { buser.UpdateBase(binfo); }
            return WriteOK("操作成功","UserInfo?ID="+info.UserID);

            /*
          
            info.VIP = DataConvert.CLng(RequestEx["level_rad"]);
           
            info.CerificateDeadLine = DataConverter.CDate(txtCerificateDeadLine.Text);
            
             
             
             */
        }
        public ContentResult User_API()
        {
            string action = GetParam("action");
            string ids = GetParam("ids");
            switch (action)
            {
                case "del":
                    buser.DelUserById(ids);
                    break;
                case "lock":
                    buser.BatAudit(ids, 1);
                    break;
                case "unlock":
                    buser.BatAudit(ids, 0);
                    break;
                case "approve":
                    buser.BatAudit(ids, 2);
                    break;
                case "unapprove":
                    buser.BatAudit(ids, 3);
                    break;
                default:
                    break;
            }
            return Content(Success.ToString());
        }
        public IActionResult AdminManage()
        {
            F_Admin filter = new F_Admin();
            PageSetting setting = badmin.SelPage(CPage, PSize, filter);
            if (Request.IsAjax()) { return PartialView("AdminManage_List", setting); }
            else { return View(setting); }
        }
        public IActionResult AdminAdd() {
            M_AdminInfo model = B_Admin.GetAdminByID(Mid);
            if (model == null) { model = new M_AdminInfo(); }
            return View(model);
        }
        [HttpPost]
        public IActionResult AdminAdd_Submit(M_AdminInfo input) {
            M_AdminInfo model = new M_AdminInfo();
            if (Mid > 0)
            {
                model = B_Admin.GetAdminByAdminId(Mid);
                string pwd = GetParam("pwd").Replace(" ", "");
                string pwd2 = GetParam("pwd2").Replace(" ", "");
                if (!string.IsNullOrEmpty(pwd))
                {
                    if (pwd.Equals(pwd2)) { return WriteErr("密码与确认密码不匹配"); }
                    if (pwd.Length < 6) { return WriteErr("密码至少6位"); }
                }
                model.AdminPassword = StringHelper.MD5(pwd);
            }
            else
            {
                model.AdminName = input.AdminName.Replace(" ", "");
                model.AdminPassword = input.AdminPassword;
                model.AdminPassword = StringHelper.MD5(model.AdminPassword);
                if (input.AdminPassword.Length < 6) { return WriteErr("密码至少6位"); }
            }
            model.RoleList = GetParam("RoleList");
            model.AdminTrueName = input.AdminTrueName;
            model.RandNumber = input.RandNumber;
            model.DefaultStart = input.DefaultStart;

            model.EnableMultiLogin = DataConvert.CBool(GetParam("EnableMultiLogin"));
            model.EnableModifyPassword = DataConvert.CBool(GetParam("EnableModifyPassword"));
            model.IsLock = DataConvert.CBool(GetParam("IsLock"));
            model.IsTable = DataConvert.CBool(GetParam("IsTable"));
            //admin.PubRole = CheckBox1.Checked ? 1 : 0;
            //admin.StructureID = curmodel_hid.Value;



            if (model.AdminId > 0)
            {
                B_Admin.Update(model);
            }
            else
            {
                //新增
                if (B_Admin.IsExist(model.AdminName)) { return WriteErr("管理员名已存在"); }
                model.AdminId = B_Admin.Add(model);
                if (DataConvert.CBool(GetParam("AddUser_Chk")))
                {
                    M_UserInfo mu = buser.NewUserByAdmin(model);
                }
            }
            return WriteOK("操作成功", "AdminManage");
        }
        [HttpPost]
        public string Admin_API()
        {
            string action = GetParam("action");
            string ids = GetParam("ids");
            if (string.IsNullOrEmpty(ids)) { return Failed.ToString(); }
            ZoomLa.BLL.SafeSC.CheckIDSEx(ids);
            switch (action)
            {
                case "del":
                    foreach (string id in ids.Split(','))
                    {
                        B_Admin.DelAdminById(Convert.ToInt32(id));
                    }
                    break;
                case "lock":
                    {
                        badmin.LockAdmin(ids,true);
                    }
                    break;
                case "unlock":
                    {
                        badmin.LockAdmin(ids, false);
                    }
                    break;
                default:
                    throw new Exception("未指定action");
            }
            return Success.ToString();
        }
        //===========================================================
        public IActionResult PermissionInfo() { return View(); }
        public IActionResult SendMailList() { return View(); }
        public IActionResult SubscriptListManage() { return View(); }
        public IActionResult Jobsconfig() { return View(); }
        public IActionResult ZoneConfig() { return View(); }
        public IActionResult WSApi() { return View(); }
        public IActionResult UserGraph() { return View(); }
    }
}