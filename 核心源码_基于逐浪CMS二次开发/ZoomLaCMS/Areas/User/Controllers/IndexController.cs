using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net.Mail;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ZoomLa.AppCode.Verify;
using ZoomLa.BLL;
using ZoomLa.BLL.API;
using ZoomLa.BLL.Content;
using ZoomLa.BLL.Helper;
using ZoomLa.BLL.Helper.Encry;
using ZoomLa.BLL.Other;
using ZoomLa.BLL.User;
using ZoomLa.Common;
using ZoomLa.Components;
using ZoomLa.Model;
using ZoomLa.Safe;
using ZoomLa.SQLDAL;
using ZoomLaCMS.Areas.User.Models;
using ZoomLaCMS.Ctrl;

namespace ZoomLaCMS.Areas.User.Controllers
{
    [Area("User")]
    public class IndexController : Ctrl_User
    {
        //  /User/Index/Action
        //B_User_Plat upBll = new B_User_Plat();
        //B_Plat_Comp compBll = new B_Plat_Comp();
        //B_User_InviteCode utBll = new B_User_InviteCode();
        //B_MailManage mailBll = new B_MailManage();
        B_Safe_Mobile mobileBll = new B_Safe_Mobile();
        B_Group gpBll = new B_Group();
        B_Model modBll = new B_Model();
        B_ModelField fieldBll = new B_ModelField();
        //手机注册,已验证过的手机号,注册完成或关闭浏览器后清除
        private string RegisterMobile
        {
            get { return HttpContext.Session.GetString("Register_Mobile_Checked"); }
            set
            {
                HttpContext.Session.SetString("Register_Mobile_Checked",value);
            }
        }
        [Authorize(Policy = "User")]
        [Route("/User/")]
        [Route("/User/Index")]
        [Route("/Office/")]
        public IActionResult Index()
        {
            if (!buser.CheckUserStatus(mu, ref err)) { return WriteErr(err); }
            M_Uinfo basemu = buser.GetUserBaseByuserid(mu.UserID);
            //--------------------------------------------------
            B_Search shBll = new B_Search();
            DataTable dt = shBll.SelByUserGroup(mu.GroupID);
            string userapptlp = "<div class='col-xl-2 col-lg-2 col-md-2 col-sm-4 col-4 @mobile user_menuBox'><div class='user_menu'><a target='@target' href='@fileUrl'>@ico<br/>@name</a></div></div>";
            string onthertlp = "<li><a target='@target' href='@fileUrl'>@ico<span>@name</span></a></li>";
            string userhtml = "";
            string ontherhtml = "";
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                DataRow dr = dt.Rows[i];
                string fileUrl = DataConvert.CStr(dr["fileUrl"]).ToLower();
                if (fileUrl.Contains(".aspx")) { continue; }
                if (fileUrl.Contains("/office") || fileUrl.Contains("markdown") || fileUrl.Contains("/design/")||fileUrl.Contains("userday")) { continue; }
                string targetlink = GetLinkTarget(dr["OpenType"].ToString());
                string mobileStr = DataConvert.CLng(dr["mobile"]) == 1 ? "" : "d-none d-sm-block";
                if (DataConverter.CLng(dr["EliteLevel"]) == 1)//抽出推荐应用
                {
                    userhtml += ReplaceData(userapptlp, dr).Replace("@target", targetlink).Replace("@mobile", mobileStr);
                }
                else
                {
                    ontherhtml += ReplaceData(onthertlp, dr).Replace("@target", targetlink);
                }
            }
            ViewBag.userhtml = MvcHtmlString.Create(userhtml);
            ViewBag.ontherhtml = MvcHtmlString.Create(ontherhtml);
            //---------------------
            ViewBag.mu = mu;
            ViewBag.basemu = basemu;
            return View(mu);
        }
        [Route("/User/Default")]
        public IActionResult Default() { return RedirectToAction("Index", "Index"); }
        [Route("/User/Login")]
        public IActionResult Login()
        {
            if (!mu.IsNull) { return RedirectToAction("Index", "Index"); }
            return View(mu);
        }
        [Route("/User/Register")]
        public IActionResult Register()
        {
            if (!SiteConfig.UserConfig.EnableUserReg) { return WriteErr("未开放注册,请和网站管理员联系!"); }
            VM_Register model = new VM_Register(HttpContext);
            if (SiteConfig.UserConfig.MobileReg == true && string.IsNullOrEmpty(RegisterMobile))
            {
                return View("Register_Mobile");
            }
            model.Mobile = RegisterMobile;
            return View(model);
        }
        [HttpPost]
        public string Register_MobileCheck()
        {
            M_APIResult retMod = new M_APIResult(Failed);
            string mobile = RequestEx["mobile"];
            string code = RequestEx["code"];
            CommonReturn ret = B_Safe_Mobile.CheckVaildCode(mobile, code, "register");
            if (ret.isok)
            {
                RegisterMobile = mobile;
                retMod.retcode = M_APIResult.Success;
            }
            else
            {
                retMod.retmsg = ret.err;
            }
            return retMod.ToString();
        }
        [HttpPost]
        public string Register_API()
        {
            M_APIResult retMod = new M_APIResult(M_APIResult.Success);
            string action = GetParam("action");
            string value = GetParam("value").Replace(" ", "");
            string result = "";
            switch (action)
            {
                case "uname":
                    if (!CheckUserName(value, ref result))
                    {
                        retMod.retcode = M_APIResult.Failed;
                        retMod.retmsg = result;
                    }
                    break;
                case "mobile"://手机号不可重复
                    {
                        string mobile = GetParam("mobile");
                        if (string.IsNullOrEmpty(mobile))
                        {
                            retMod.retmsg = "手机号码不能为空";
                        }
                        else if (!RegexHelper.IsMobilPhone(mobile))
                        {
                            retMod.retmsg = "手机号码格式不正确";
                        }
                        else
                        {
                            bool flag = DBCenter.IsExist("ZL_UserBase", "Mobile=@mobile",
                                              new List<SqlParameter>() { new SqlParameter("mobile", mobile) });
                            if (flag) { retMod.retmsg = "手机号码已存在"; }
                            else { retMod.retcode = M_APIResult.Success; }
                        }
                    }
                    break;
                case "puser"://推荐人为空则不检测
                    if (!string.IsNullOrEmpty(value) && CheckParentUser(value).IsNull)
                    {
                        retMod.retcode = M_APIResult.Failed;
                        retMod.retmsg = "推荐人不存在";
                    }
                    break;
                case "email":
                    if (!CheckEmail(value, ref result))
                    {
                        retMod.retcode = M_APIResult.Failed;
                        retMod.retmsg = result;
                    }
                    break;
                case "birth":
                    if (!CheckBirthDay(value, ref result))
                    {
                        retMod.retcode = M_APIResult.Failed;
                        retMod.retmsg = result;
                    }
                    break;
                case "GetModelFied":
                    return GetUserGorupModel(value);
                default:
                    retMod.retmsg = "[" + action + "]接口不存在";
                    break;
            }
            return retMod.ToString();
        }
        public string GetUserGorupModel(string value)
        {
            int gid = DataConverter.CLng(value);
            int UserModelID = DataConverter.CLng(gpBll.GetGroupModel(gid));
            ///UserModelID!=0说明绑定了户模型，用要从模型中读取字段，没有绑定就不需要读取字段
            //if (UserModelID != 0)
            //{
            //    return fieldBll.InputallHtml(UserModelID, 0, new ModelConfig()
            //    {
            //        Source = ModelConfig.SType.UserBase
            //    });
            //}
            return "";
        }
        [HttpPost]
        public IActionResult Register_Submit()
        {
            B_UserBaseField bmf = new B_UserBaseField();
            string siteurls = SiteConfig.SiteInfo.SiteUrl.TrimEnd('/');
            if (!SiteConfig.UserConfig.EnableUserReg) { return WriteErr("服务器已关闭用户注册"); }
            //-----------------------------------
            M_UserInfo info = new M_UserInfo();
            info.UserBase = new M_Uinfo();
            info.UserName = GetParam("TxtUserName").Replace(" ", "");
            info.UserPwd = RequestEx["TxtPassword"];
            info.Question = RequestEx["Question_DP"];
            info.Answer = RequestEx["TxtAnswer"];
            info.Email = DataConvert.CStr(RequestEx["TxtEmail"]).Replace(" ", "");
            info.CheckNum = function.GetRandomString(10);
            info.GroupID = DataConverter.CLng(RequestEx["UserGroup"]);
            if (!SiteConfig.UserConfig.Reg_SelGroup)//不允许用户手动选择会员组
            {
                info.GroupID = gpBll.DefaultGroupID();
            }
            info.RegisterIP =IPScaner.GetUserIP(HttpContext);
            info.LastLoginIP = info.RegisterIP;
            //info.Purse = SiteConfig.UserConfig.PresentMoney;//注册赠送的余额,积分等
            //info.UserPoint = SiteConfig.UserConfig.PresentPoint;
            //info.UserExp = DataConverter.CLng(SiteConfig.UserConfig.PresentExp);

            info.TrueName = RequestEx["TxtTrueName"];
            info.UserPwd = StringHelper.MD5(info.UserPwd); ;
            //-----------------------------------------------------
            //会员基本信息
            info.UserBase.Address = RequestEx["TxtAddress"];
            info.UserBase.BirthDay = RequestEx["TxtBirthday"];
            info.UserFace = RequestEx["TxtUserFace"];
            info.UserBase.Fax = RequestEx["TxtFax"];
            info.UserBase.HomePage = RequestEx["TxtHomepage"];
            info.UserBase.HomePhone = RequestEx["TxtHomePhone"];
            info.UserBase.IDCard = RequestEx["TxtIDCard"];
            info.UserBase.Mobile = RequestEx["TxtMobile"];
            info.UserBase.OfficePhone = RequestEx["TxtOfficePhone"];
            info.UserBase.Privating = DataConvert.CLng(RequestEx["DropPrivacy"]);
            info.UserBase.PHS = RequestEx["TxtPHS"];
            info.UserBase.QQ = RequestEx["TxtQQ"];
            info.UserBase.Sign = RequestEx["TxtSign"];
            info.UserBase.UserSex = DataConverter.CBool(RequestEx["DropSex"]);
            info.UserBase.ZipCode = RequestEx["TxtZipCode"];
            info.UserBase.HoneyName = "";
            info.UserBase.CardType = "";
            info.UserBase.Province = GetParam("selprovince");
            info.UserBase.City = GetParam("selcity");
            info.UserBase.County = GetParam("selcoutry");
            if (!string.IsNullOrEmpty(RegisterMobile)) { info.UserBase.Mobile = RegisterMobile; RegisterMobile = null; }
            //-----------------------------------------------------
            #region 信息检测
            string err = "";
            if (SiteConfig.UserConfig.EnableCheckCodeOfReg)
            {
                if (!VerifyHelper.Check(RequestEx["VCode_hid"]))
                {
                    return WriteErr("您输入的验证码和系统产生的不一致，请重新输入", "javascript:history.go(-1);");
                }
            }
            if (!CheckUserName(info.UserName, ref err)) { return WriteErr(err); }
            else if (!CheckUserInfo(info.UserBase)) { return WriteErr(err); }
            else if (SiteConfig.UserConfig.Reg_AnswerMust && string.IsNullOrEmpty(info.Answer)) { return WriteErr("问题答案不能为空");  }
            else if (!CheckEmail(info.Email, ref err)) { return WriteErr("问题答案不能为空");  }
            #endregion
            //推荐人处理
            {
                //支持使用用户名和用户ID
                info.ParentUserID = CheckParentUser(RequestEx["TxtParentUser"]).UserID.ToString();
                if (DataConvert.CLng(info.ParentUserID) < 1 && !string.IsNullOrEmpty(RequestEx["ParentUser_Hid"]))
                {
                    info.ParentUserID = CheckParentUser(RequestEx["ParentUser_Hid"]).UserID.ToString();
                }
            }
            //用于初始状态
            if (SiteConfig.UserConfig.EmailCheckReg) { info.Status = 4; }//邮件认证
            else if (SiteConfig.UserConfig.AdminCheckReg) { info.Status = 2; } //管理员认证
            else if (SiteConfig.UserConfig.EmailCheckReg && SiteConfig.UserConfig.AdminCheckReg) { info.Status = 3; } //邮件认证及管理员认证
            else if (!SiteConfig.UserConfig.UserValidateType) { info.Status = 5; }
            else { info.Status = 0; }
            //自定义字段信息
            DataTable table;
            try
            {
                table = Call.GetDTFromMVC(bmf.Select_All(), Request);
            }
            catch (Exception e)
            {
                return WriteErr(e.Message);
            }
            string[] strArray2 = string.IsNullOrEmpty(SiteConfig.UserConfig.RegFieldsMustFill) ? new string[0] : SiteConfig.UserConfig.RegFieldsMustFill.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string str2 in strArray2)
            {
                if (string.IsNullOrEmpty(GetParam("txt_" + str2)))
                {
                    DataTable tbles = bmf.SelByFieldName(str2);
                }
            }
            //------------添加新用户

            info.UserID = buser.Add(info);
            info.UserBase.UserId = info.UserID;
            if (info.UserID < 1) { return WriteErr("注册失败"); }
            buser.AddBase(info.UserBase);
            if (table != null && table.Rows.Count > 0) { buser.UpdateUserFile(info.UserID, table); }
            buser.SetLoginState(info);
            //赠送虚拟币
            //if (SiteConfig.UserConfig.PresentMoney > 0)
            //{
            //    buser.AddMoney(info.UserID, SiteConfig.UserConfig.PresentMoney, M_UserExpHis.SType.Purse, "注册赠送");
            //}
            //if (SiteConfig.UserConfig.PresentExp > 0)
            //{
            //    buser.AddMoney(info.UserID, SiteConfig.UserConfig.PresentExp, M_UserExpHis.SType.Point, "注册赠送");
            //}
            //----------------------------------------------------------------------------
            string ReturnUrl = RequestEx["ReturnUrl_Hid"];
            string RegMessage = "";
            string RegRUrl = "";
            bool isok = false;
            //关联绑定微信用户
            //if (!string.IsNullOrEmpty(WXOpenID))
            //{
            //    B_User_Token tokenBll = new B_User_Token();
            //    M_User_Token tokenMod = tokenBll.SelModelByUid(info.UserID);
            //    if (tokenMod == null) { tokenMod = new M_User_Token(); }
            //    tokenMod.uid = info.UserID;
            //    tokenMod.WXOpenID = WXOpenID;
            //    tokenBll.Insert(tokenMod);
            //}
            #region 自定义模型
            int ModelID = DataConverter.CLng(gpBll.GetGroupModel(info.GroupID));
            string usertablename = modBll.SelReturnModel(ModelID).TableName;
            if (ModelID > 0 && usertablename != "" && usertablename != null)
            {
                DataTable groupset = fieldBll.GetModelFieldListall(ModelID);
                DataTable tablereg = new DataTable();
                tablereg.Columns.Add(new DataColumn("FieldName", typeof(string)));
                tablereg.Columns.Add(new DataColumn("FieldType", typeof(string)));
                tablereg.Columns.Add(new DataColumn("FieldValue", typeof(string)));
                if (groupset != null && groupset.Rows.Count > 0)
                {
                    foreach (DataRow dr in groupset.Rows)
                    {
                        if (dr["FieldType"].ToString() == "FileType")
                        {
                            string[] Sett = dr["Content"].ToString().Split(new char[] { ',' });
                            bool chksize = DataConverter.CBool(Sett[0].Split(new char[] { '=' })[1]);
                            string sizefield = Sett[1].Split(new char[] { '=' })[1];
                            if (chksize && sizefield != "")
                            {
                                DataRow row2 = tablereg.NewRow();
                                row2[0] = sizefield;
                                row2[1] = "FileSize";
                                row2[2] = RequestEx["txt_" + sizefield];
                                tablereg.Rows.Add(row2);
                            }
                        }

                        if (dr["FieldType"].ToString() == "MultiPicType")
                        {
                            string[] Sett = dr["Content"].ToString().Split(new char[] { ',' });
                            bool chksize = DataConverter.CBool(Sett[0].Split(new char[] { '=' })[1]);
                            string sizefield = Sett[1].Split(new char[] { '=' })[1];
                            if (chksize && sizefield != "")
                            {
                                if (string.IsNullOrEmpty(RequestEx["txt_" + sizefield]))
                                {
                                  return WriteErr(dr["FieldAlias"].ToString() + "的缩略图不能为空!");
                                }
                                DataRow row1 = tablereg.NewRow();
                                row1[0] = sizefield;
                                row1[1] = "ThumbField";
                                row1[2] = RequestEx["txt_" + sizefield];
                                tablereg.Rows.Add(row1);
                            }
                        }

                        DataRow row = tablereg.NewRow();
                        row[0] = dr["FieldName"].ToString();
                        string ftype = dr["FieldType"].ToString();
                        row[1] = ftype;
                        string fvalue = RequestEx["txt_" + dr["FieldName"].ToString()];
                        if (ftype == "TextType" || ftype == "MultipleTextType" || ftype == "MultipleHtmlType")
                        {
                            if (dr["IsNotNull"].Equals("True") && string.IsNullOrEmpty(fvalue))
                                return WriteErr(dr["FieldAlias"] + ":不能为空!");
                        }
                        row[2] = fvalue;
                        tablereg.Rows.Add(row);
                    }
                    try
                    {
                        if (tablereg.Select("FieldName='UserID'").Length == 0)
                        {
                            DataRow rowsd1 = tablereg.NewRow();
                            rowsd1[0] = "UserID";
                            rowsd1[1] = "int";
                            rowsd1[2] = info.UserID;
                            tablereg.Rows.Add(rowsd1);
                        }
                        else
                        {
                            tablereg.Rows[0]["UserID"] = info.UserID;
                        }
                    }
                    catch (Exception)
                    {

                    }

                    try
                    {
                        if (tablereg.Select("FieldName='UserName'").Length == 0)
                        {
                            DataRow rowsd2 = tablereg.NewRow();
                            rowsd2[0] = "UserName";
                            rowsd2[1] = "TextType";
                            rowsd2[2] = info.UserName;
                            tablereg.Rows.Add(rowsd2);
                        }
                        else
                        {
                            tablereg.Rows[0]["UserName"] = info.UserName;
                        }
                    }
                    catch (Exception) { }
                    try
                    {
                        if (tablereg.Select("FieldName='Styleid'").Length == 0)
                        {
                            DataRow rowsd3 = tablereg.NewRow();
                            rowsd3[0] = "Styleid";
                            rowsd3[1] = "int";
                            rowsd3[2] = 0;
                            tablereg.Rows.Add(rowsd3);
                        }
                        else
                        {
                            tablereg.Rows[0]["UserName"] = 0;
                        }
                    }
                    catch (Exception) { }
                    try
                    {
                        if (tablereg.Select("FieldName='Recycler'").Length == 0)
                        {
                            DataRow rowsd4 = tablereg.NewRow();
                            rowsd4[0] = "Recycler";
                            rowsd4[1] = "int";
                            rowsd4[2] = 0;
                            tablereg.Rows.Add(rowsd4);
                        }
                        else
                        {
                            tablereg.Rows[0]["Recycler"] = 0;
                        }
                    }
                    catch (Exception) { }
                    try
                    {
                        if (tablereg.Select("FieldName='IsCreate'").Length == 0)
                        {
                            DataRow rowsd5 = tablereg.NewRow();
                            rowsd5[0] = "IsCreate";
                            rowsd5[1] = "int";
                            rowsd5[2] = 0;
                            tablereg.Rows.Add(rowsd5);
                        }
                        else
                        {
                            tablereg.Rows[0]["IsCreate"] = 0;
                        }
                    }
                    catch (Exception) { }
                    try
                    {
                        if (tablereg.Select("FieldName='NewTime'").Length == 0)
                        {
                            DataRow rs6 = tablereg.NewRow();
                            rs6[0] = "NewTime";
                            rs6[1] = "DateType";
                            rs6[2] = DateTime.Now;
                            tablereg.Rows.Add(rs6);
                        }
                        else
                        {
                            tablereg.Rows[0]["NewTime"] = DateTime.Now;
                        }
                    }
                    catch (Exception) { }
                }

                if (tablereg != null && tablereg.Rows.Count > 0)
                {
                    buser.InsertModel(tablereg, usertablename);
                }
            }
            #endregion
            if (SiteConfig.UserConfig.EmailCheckReg && !string.IsNullOrEmpty(info.Email))//发送认证邮件，当需要邮件认证时
            {
                //[c]需要新的邮件收发模块
                //MailInfo mailInfo = new MailInfo();
                //mailInfo.IsBodyHtml = true;
                //mailInfo.FromName = SiteConfig.SiteInfo.SiteName;
                //MailAddress address = new MailAddress(info.Email);
                //mailInfo.ToAddress = address;
                //string regurl = siteurls + "/User/Index/RegEmailCheck?UserName=" + HttpUtility.UrlEncode(info.UserName) + "&CheckNum=" + info.CheckNum;
                //string mailcontent = mailBll.SelByType(B_MailManage.MailType.NewUserReg);
                //mailInfo.MailBody = new OrderCommon().TlpDeal(mailcontent, GetRegEmailDt(info.UserName, info.CheckNum, regurl));
                //mailInfo.Subject = SiteConfig.SiteInfo.SiteName + "网站会员注册验证码";
                //if (SendMail.Send(mailInfo) == SendMail.MailState.Ok)
                //{
                //    RegMessage = "注册验证码已成功发送到你的注册邮箱，请到邮箱查收并验证!";
                //    RegMessage = RegMessage + "<a href=\"/\">返回首页</a>";
                //}
                //else
                //{
                //    RegMessage = "注册成功，但发送验证邮件失败，请检查邮件地址是否正确，或与网站管理员联系！";
                //    RegMessage = RegMessage + "<a href=\"/\">返回首页</a>";
                //}
            }
            switch (info.Status)
            {
                case 0:
                    #region 直接注册成功
                    //[c]
                    //if (!string.IsNullOrEmpty(info.Email) && SiteConfig.UserConfig.EmailTell)
                    //{
                    //    MailInfo mailInfo = new MailInfo();
                    //    mailInfo.IsBodyHtml = true;
                    //    mailInfo.FromName = SiteConfig.SiteInfo.SiteName;
                    //    MailAddress address = new MailAddress(info.Email);
                    //    mailInfo.ToAddress = address;
                    //    //SiteConfig.UserConfig.EmailOfRegCheck
                    //    string regurl = siteurls + "/User/Index/RegEmailCheck?UserName=" + HttpUtility.UrlEncode(info.UserName) + "&CheckNum=" + info.CheckNum;
                    //    mailInfo.MailBody = new OrderCommon().TlpDeal(mailBll.SelByType(B_MailManage.MailType.NewUserReg), GetRegEmailDt(info.UserName, info.CheckNum, regurl));
                    //    //mailInfo.MailBody = mailInfo.MailBody.Replace("{$UserName}", info.UserName).Replace("{$UserPwd}", TxtPassword.Text);
                    //    mailInfo.Subject = SiteConfig.SiteInfo.SiteName + "_注册成功提醒";
                    //    if (SendMail.Send(mailInfo) == SendMail.MailState.Ok)
                    //    {
                    //        RegMessage = "注册基本信息已成功发送到你的注册邮箱！";
                    //        RegMessage = RegMessage + "<a href=\"/\">返回首页</a>";
                    //    }
                    //    else
                    //    {
                    //        RegMessage = "注册成功，但发送注册基本信息邮件失败，请检查邮件地址是否正确，或与网站管理员联系！";
                    //        RegMessage = RegMessage + "<a href=\"/\">返回首页</a>";
                    //    }
                    //}
                    //else
                    //{
                        RegMessage = "注册成功！";
                        if (string.IsNullOrEmpty(ReturnUrl))
                        {
                            RegMessage = RegMessage + "<a href=\"/\">返回首页</a>&nbsp;&nbsp;<a href=\"/User/Index\">进入会员中心</a>,5秒后系统自动跳转到会员中心!";
                            RegRUrl = "/User/Index";
                        }
                        else
                        {
                            RegMessage = RegMessage + "<a href=\"/\">返回首页</a>&nbsp;&nbsp;<a href=\"" + ReturnUrl + "\">进入默认页面</a>,5秒后系统自动跳转到默认页面!";
                            RegRUrl = ReturnUrl;
                        }
                        isok = true;
                    //}
                    #endregion
                    break;
                case 2: //等待管理员认证
                    {
                        RegMessage = "注册成功！新注册会员需管理员认证才能有效，请耐心等待！";
                        RegMessage = RegMessage + "若长期没有通过管理员认证,请及时和管理员联系！";
                        RegMessage = RegMessage + "<a href=\"/\">返回首页</a>";
                    }
                    break;
                default:
                    //未开启邮箱验证，则可以登录
                    //if (!SiteConfig.UserConfig.EmailCheckReg) { RegMessage = "注册成功!"; }
                    if (string.IsNullOrEmpty(ReturnUrl))
                    {
                        RegMessage = "注册成功<a href=\"/\">返回首页</a>&nbsp;&nbsp;<a href=\"/User/Index\">进入会员中心</a>,5秒后系统自动跳转到会员中心!";
                        RegRUrl = "default";
                    }
                    else
                    {
                        RegMessage = "<a href=\"/\">返回首页</a>&nbsp;&nbsp;<a href=\"" + ReturnUrl + "\">进入默认页面</a>,5秒后系统自动跳转到默认页面!";
                        RegRUrl = ReturnUrl;
                    }
                    isok = true;
                    break;
            }
            if (SiteConfig.UserConfig.EmailCheckReg)
            {
                RegMessage = "<div class='emptyDiv'><br/>注册成功! &nbsp;&nbsp;<a href='http://mail." + info.Email.Substring(info.Email.LastIndexOf('@') + 1) + "' target='_blank'>立即登录邮箱进行验证>></a><br/></div>";
                isok = false;
            }
            ViewBag.RegMessage = RegMessage;
            ViewBag.RegRUrl = RegRUrl;
            ViewBag.isok = isok;//为true则自动跳转
            ViewBag.pwd = RequestEx["TxtPassword"];
            return View("Register_Finish", info);
        }
        public IActionResult RegEmailCheck()
        {
            string uname = GetParam("UserName");
            string checkNum = GetParam("CheckNum");
            if (string.IsNullOrEmpty(uname)) { return WriteErr("未指定用户名"); }
            if (string.IsNullOrEmpty(checkNum)) { return WriteErr("未指定校验码"); }
            //检测通过,跳转用户中心
            M_UserInfo mu = buser.GetUserByCheckNum(uname, checkNum);
            if (mu.IsNull) { return WriteErr("用户不存在,或已被激活"); }
            mu.Status = 0;
            mu.CheckNum = "";
            buser.UpdateByID(mu);
            buser.SetLoginState(mu);
            //用户成功激活现在引导进入会员中心，就更好了。。
           return WriteOK("用户成功激活现在引导进入会员中心", "/User/Default");
        }
        //替换userapp字符串
        private string ReplaceData(string value, DataRow dr)
        {
            string[] replce = "ico,fileUrl,name".Split(',');
            foreach (string item in replce)
            {
                string temptext = dr[item].ToString();
                if (item.Equals("ico"))
                {//图标替换
                    temptext = StringHelper.GetItemIcon(temptext, "width:50px;height:50px;");

                }
                value = value.Replace("@" + item, temptext);
            }
            return value;
        }
        private string GetLinkTarget(string target)
        {
            switch (target)
            {
                case "1":
                    return "_blank";
                default:
                    return "_self";
            }
        }
        #region Register Logical
        //检测会员名是否有效
        private bool CheckUserName(string uname, ref string err)
        {
            if (string.IsNullOrEmpty(uname)) { return false; }
            uname = uname.Replace(" ", "");
            if (SiteConfig.UserConfig.UserNameLimit > uname.Length || uname.Length > SiteConfig.UserConfig.UserNameMax)
            {
                err = "用户名的长度必须小于" + SiteConfig.UserConfig.UserNameMax + "，并大于" + SiteConfig.UserConfig.UserNameLimit + "!"; return false;
            }
            else if (ZoomLa.BLL.SafeSC.CheckData(uname))
            {
                err = "用户名不能包含特殊字段!"; return false;
            }
            else if (!ZoomLa.BLL.SafeSC.CheckUName(uname))
            {
                err = "用户名不能包含特殊字符!"; return false;
            }
            if (StringHelper.FoundInArr(SiteConfig.UserConfig.UserNameRegDisabled, uname, "|"))
            {
                err = "该用户名禁止注册，请输入不同的用户名!"; return false;
            }
            if (buser.IsExistUName(uname))
            {
                err = "该用户名已被他人占用，请输入不同的用户名"; return false;
            }
            string userregrule = SiteConfig.UserConfig.RegRule;
            if (userregrule != null && userregrule != "")
            {
                if (userregrule.IndexOf(',') > -1)
                {
                    string[] rulearr = userregrule.Split(',');
                    for (int ii = 0; ii < rulearr.Length; ii++)
                    {
                        if (rulearr[ii].ToString() == "1")
                        {
                            string resultString = null;
                            try
                            {
                                resultString = Regex.Match(uname, @"[0-9]*").Value;
                            }
                            catch (ArgumentException)
                            {
                            }
                            if (uname == resultString.Trim())
                            {
                                err = "用户名不允许纯数字"; return false;
                            }
                        }

                        if (rulearr[ii].ToString() == "2")
                        {
                            string resultString = null;
                            try
                            {
                                resultString = Regex.Match(uname, @"[a-zA-Z]*").Value;
                            }
                            catch (ArgumentException)
                            {
                            }

                            if (uname == resultString)
                            {
                                err = "用户名不允许纯英文"; return false;
                            }

                        }

                        if (rulearr[ii].ToString() == "3")
                        {
                            bool foundMatch = false;
                            try
                            {
                                foundMatch = Regex.IsMatch(uname, @"[\u0391-\uFFE5]$");
                            }
                            catch (ArgumentException)
                            {
                            }

                            if (foundMatch)
                            {
                                err = "用户名不允许带有中文"; return false;
                            }
                        }
                    }
                }
                else
                {
                    if (userregrule.ToString() == "1")
                    {
                        string resultString = null;
                        try
                        {
                            resultString = Regex.Match(uname, @"[0-9]*").Value;
                        }
                        catch (ArgumentException)
                        {
                        }
                        if (uname == resultString.Trim())
                        {
                            err = "用户名不允许纯数字"; return false;
                        }
                    }
                    if (userregrule.ToString() == "2")
                    {
                        string resultString = null;
                        try
                        {
                            resultString = Regex.Match(uname, @"[a-zA-Z]*").Value;
                        }
                        catch (ArgumentException)
                        {
                        }

                        if (uname == resultString)
                        {
                            err = "用户名不允许纯英文"; return false;
                        }
                    }
                    if (userregrule.ToString() == "3")
                    {
                        bool foundMatch = false;
                        try
                        {
                            foundMatch = Regex.IsMatch(uname, @"[\u0391-\uFFE5]$");
                        }
                        catch (ArgumentException)
                        {
                        }

                        if (foundMatch)
                        {
                            err = "用户名不允许带有中文"; return false;
                        }
                    }
                }
            }
            return true;
        }
        //用户信息验证,身份证号,生日等
        private bool CheckUserInfo(M_Uinfo basemu)
        {
            if (string.IsNullOrEmpty(basemu.IDCard)) { return true; }
            if (buser.IsExitcard(basemu.IDCard)) {
                //("该身份证号已被注册，请输入不同的身份证号！");
                return false; }
            bool foundMatch = false;
            try
            {
                foundMatch = Regex.IsMatch(basemu.IDCard, @"^\d{17}([0-9]|X)$");
                if (foundMatch)
                {
                    string birth = basemu.IDCard.Substring(6, 8).Insert(6, "-").Insert(4, "-");
                    DateTime time = new DateTime();
                    DateTime newDate = DateTime.Now.AddYears(-120);
                    DateTime now = DateTime.Now;
                    if (DateTime.TryParse(birth, out time) == false)
                    {
                        //("该身份证生日不正确！");
                        return false;
                    }
                    else
                    {
                        DateTime data1 = Convert.ToDateTime(birth);
                        TimeSpan ts = newDate - data1;
                        TimeSpan ts2 = data1 - now;
                        if (ts.Days > 0) {
                            //("您超过了120岁？请核对身份证号。");
                            return false; }
                        else if (ts2.Days > 0) {
                            //("您还没出生吧？请核对身份证号。");
                            return false; }
                    }
                }
                else
                {
                    //("该身份证格式不正确！");
                    return false;
                }
            }
            catch (ArgumentException)
            {
                return true;
            }
            return true;
        }
        //推荐人是否存在支持ID与用户名(暂也支持推荐码)
        private M_UserInfo CheckParentUser(string puname)
        {
            M_UserInfo pmu = new M_UserInfo(true);
            if (string.IsNullOrEmpty(puname)) { return pmu; }
            int puid = DataConvert.CLng(puname);
            //if (puid >= 100001 && puid <= 999999)//只有9级,所以只有首位需去除[delete]
            //{
            //    int depth = Convert.ToInt32(puname.Substring(0, 1));
            //    int uid = Convert.ToInt32(puname.Substring(1, (puname.Length - 1)));
            //    pmu = buser.SelReturnModel(uid);
            //    if (pmu.RoomID == 0) { pmu.RoomID = 1; }
            //    if (pmu.RoomID != depth) { return new M_UserInfo(true); }
            //}
            if (puid > 0)//100001
            {
                pmu = buser.SelReturnModel(puid);
            }
            else
            {
                pmu = buser.GetUserByName(puname);
            }
            return pmu;
        }
        private bool CheckEmail(string email, ref string err)
        {
            //邮箱非必填
            if (!SiteConfig.UserConfig.Reg_EmailMust && string.IsNullOrEmpty(email)) { return true; }
            if (string.IsNullOrEmpty(email)) { err = "邮箱不能为空"; return false; }
            if (!RegexHelper.IsEmail(email)) { err = "邮箱格式不正确"; return false; }
            if (buser.IsExist("ume", email)) { err = "该邮箱已存在"; return false; }
            return true;
        }
        //检测出生日期是否合逻辑
        private bool CheckBirthDay(string value, ref string err)
        {
            DateTime time = DateTime.Now;
            if (string.IsNullOrEmpty(value)) { err = "日期格式为空"; return false; }
            else if (!DateTime.TryParse(value, out time)) { err = "不是有效的日期格式"; return false; }
            else if (time < DateTime.Now.AddYears(-150)) { err = "您超过了150岁?-吉尼斯世界纪录最长寿的人是132岁!"; return false; }
            else if (time > DateTime.Now) { err = "日期大于当前时间"; return false; }
            return true;
        }
        //获取邮件内容模板标签格式
        private DataTable GetRegEmailDt(string username, string checknum, string checkurl)
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("CheckNum");
            dt.Columns.Add("CheckUrl");
            dt.Columns.Add("UserName");
            dt.Rows.Add(dt.NewRow());
            dt.Rows[0]["CheckNum"] = checknum;
            dt.Rows[0]["CheckUrl"] = checkurl;
            dt.Rows[0]["UserName"] = username;
            return dt;
        }
        #endregion
        public IActionResult ASCXHtml(string ascx)
        {
            switch (ascx)
            {
                case "UserShopTop":
                    return PartialView("~/Areas/User/Views/UserShop/UserShopTop.cshtml");
                default:
                    return PartialView("~/Areas/User/Views/Shared/ASCX/" + ascx + ".cshtml");
            }

        }

        public int UserLoginCount
        {
            get
            {
                return DataConvert.CLng(HttpContext.Session.GetInt32("ValidateCount"));
            }
            set
            {
                HttpContext.Session.SetInt32("ValidateCount",value);
            }
        }
        //自有账号登录(跳转页面)
        public string Login_Ajax(string uname, string upwd, string vcode, int regid)
        {
            string id = HttpContext.Session.Id;
            string err = "";
            M_APIResult retMod = new M_APIResult(M_APIResult.Failed);
            string openVCode = SiteConfig.UserConfig.EnableCheckCodeOfLogin;
            if ((openVCode == "0" && UserLoginCount >= 3) || openVCode == "1")
            {
                if (!VerifyHelper.Check(RequestEx["VCode_hid"]))
                {
                    retMod.retmsg = "验证码不正确";
                    return retMod.ToString();
                }
            }
            //upwd = RSAHelper.RsaDecrypt(upwd, SafeSC.ReadFileStr("/config/safe/PrivateKey.config"));
            M_UserInfo mu = LoginByRegID(ref err, uname, upwd, regid);
            if (mu.IsNull) { UserLoginCount++; retMod.retmsg = err; if (openVCode == "0" && UserLoginCount >= 3) { retMod.addon = "showvcode"; } }
            else if (mu.Status != 0) { retMod.retmsg = "你的帐户未通过验证或被锁定，请与网站管理员联系"; }
            else
            {
                UserLoginCount = 0;
                retMod.retcode = M_APIResult.Success;
                buser.SetLoginState(mu, "Month");
            }
            return retMod.ToString();
        }
        //新浪社会化登录(跳转链接)
        public void Login_Sina()
        {
            //B_User_Token tokenBll = new B_User_Token();
            //M_User_Token tokenMod = tokenBll.SelModelByUid(buser.GetLogin().UserID);
            //if (tokenMod != null)//已存有用户信息,则直接登录
            //{
            //    SinaHelper sinaBll = new SinaHelper(tokenMod.SinaToken);
            //    Response.Redirect(sinaBll.GetAuthUrl()); 
            //}
            //else
            //{
            //    SinaHelper sinaBll = new SinaHelper("");
            //    Response.Redirect(sinaBll.GetAuthUrl()); 
            //}
        }
        public void Logout()
        {
            buser.ClearCookie();
            string url = "/User/Login";
            string ReturnUrl = Request.GetParam("ReturnUrl");
            if (!string.IsNullOrEmpty(ReturnUrl))
            {
                //url += "?ReturnUrl=" + ReturnUrl;
                url = ReturnUrl;
            }
            Response.Redirect(url);
            //Response.Write("<script>setTimeout(function(){location='" + url + "';},500);</script>");
        }
        private M_UserInfo LoginByRegID(ref string errmsg, string AdminName, string AdminPass, int RegID)
        {
            AdminName = AdminName.Trim();
            AdminPass = AdminPass.Trim();
            M_UserInfo info = new M_UserInfo(true);
            switch (RegID)
            {
                case 1:
                    errmsg = "邮箱名或密码错误";
                    info = buser.AuthenticateEmail(AdminName, AdminPass);
                    break;
                case 2:
                    errmsg = "用户ID或密码错误";
                    info = buser.AuthenticateID(DataConverter.CLng(AdminName), AdminPass);
                    break;
                case 3:
                    errmsg = "手机号码或密码错误";
                    info = buser.AuthenByMobile(AdminName, AdminPass);
                    break;
                case 4:
                    errmsg = "用户名或密码错误";
                    info = buser.AuthenByUME(AdminName, AdminPass);
                    break;
                default:
                    errmsg = "用户名或密码错误";
                    info = buser.AuthenticateUser(AdminName, AdminPass);
                    break;
            }
            return info;
        }


    }
}