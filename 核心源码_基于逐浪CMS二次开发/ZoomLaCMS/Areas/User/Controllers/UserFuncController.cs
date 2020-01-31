using System;
using System.Data;
using System.Drawing;
using System.IO;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ZoomLa.BLL;
using ZoomLa.BLL.AdSystem;
using ZoomLa.BLL.API;
using ZoomLa.BLL.Client;
using ZoomLa.BLL.Content;
using ZoomLa.BLL.Helper;
using ZoomLa.BLL.User;
using ZoomLa.Common;
using ZoomLa.Components;
using ZoomLa.Model;
using ZoomLa.Model.AdSystem;
using ZoomLa.Model.Client;
using ZoomLa.Model.Content;
using ZoomLa.SQLDAL;
using ZoomLa.SQLDAL.SQL;
using ZoomLaCMS.Ctrl;

namespace ZoomLaCMS.Areas.User.Controllers
{
    [Area("User")]
    [Authorize(Policy = "User")]
    public class UserFuncController : Ctrl_User
    {
        B_User_InviteCode utBll = new B_User_InviteCode();
        B_User_Signin sinBll = new B_User_Signin();
        B_ADZone adzBll = new B_ADZone();
        B_AdBuy adbBll = new B_AdBuy();
        B_ChatMsg msgbll = new B_ChatMsg();
        B_Group gpBll = new B_Group();
        B_Temp tpBll = new B_Temp();
        B_Model modBll = new B_Model();
        B_CRMS_Client clientBll = new B_CRMS_Client();
        public IActionResult InviteCode()
        {
            PageSetting setting = utBll.Code_SelPage(CPage, PSize, mu.UserID, RequestEx["skey"]);
            if (Request.IsAjaxRequest()) { return PartialView("InviteCode_List", setting); }
            int maxcount = SiteConfig.UserConfig.InviteCodeCount;
            ViewBag.maxcount = maxcount;
            ViewBag.codecount = utBll.Code_Sel(mu.UserID).Rows.Count;
            return View(setting);
        }
        //根据配置生成指定数量的邀请码
        public IActionResult InviteCode_Add()
        {
            int maxcount = SiteConfig.UserConfig.InviteCodeCount;
            if (maxcount < 1) { return WriteErr("未开启邀请码功能");  }
            int count = maxcount - utBll.Code_Sel(buser.GetLogin().UserID).Rows.Count;
            if (count > 0)
            {
                utBll.Code_Create(count, mu);
                return WriteOK("生成完成", "InviteCode"); 
            }
            else
            {
                return WriteErr("生成取消 ,因为你已经有了" + maxcount + "个未使用的邀请码!", "/User/UserFunc/InviteCode");
            }
        }
        public int Code_Del(string ids)
        {
            utBll.DelByIDS(ids, mu.UserID);
            return 1;
        }
        public int Code_Create()
        {
            int maxcount = SiteConfig.UserConfig.InviteCodeCount;
            int count = maxcount - utBll.Code_Sel(mu.UserID).Rows.Count;
            if (count > 0)
            {
                utBll.Code_Create(count, buser.GetLogin());
                return -1;
            }
            else
            {
                return maxcount;
            }
        }
        public IActionResult SetSecondPwd()
        {
            mu = buser.GetLogin(false);
            ViewBag.isfirst = string.IsNullOrEmpty(mu.PayPassWord);
            return View();
        }
        [HttpPost]
        public IActionResult SecondPwd_Set()
        {
            mu = buser.GetLogin(false);
            string action =GetParam("action");
            string pwd = (RequestEx["pwd_t"] ?? "").Trim(' ');
            string oldpwd = (RequestEx["oldpwd_t"] ?? "").Trim(' ');
            if (action.Equals("update"))
            {
                if (mu.PayPassWord != StringHelper.MD5(oldpwd))
                {
                    return WriteErr("原密码错误,请重新输入！"); 
                }
            }
            if (string.IsNullOrEmpty(pwd)) { return WriteErr("二级密码不能为空"); }
            if (StringHelper.MD5(pwd).Equals(mu.PayPassWord)) { return WriteErr("新密码和原密码不能相同");  }
            mu.PayPassWord = StringHelper.MD5(pwd);
            buser.UpDateUser(mu);
            return WriteOK("操作成功", "/User/Info/Index");
        }
        #region 客户管理
        public IActionResult ConstPassen()
        {
            int type = DataConverter.CLng(RequestEx["type"]);
            string group = RequestEx["group"];
            //PageSetting setting = basBll.SelByType_SPage(CPage, PSize, mu.UserID, type, group);
            PageSetting setting = clientBll.SelPage(CPage, PSize, new F_CRMS_Client()
            {
                ctype = RequestEx["ctype"],
                uids = mu.UserID.ToString(),
            });
            if (Request.IsAjax()) { return PartialView("ConstPassen_List",setting); }
            else { return View(setting); }
        }
        public IActionResult AddConstPassen()
        {
            M_CRMS_Client clientMod = clientBll.SelReturnModel(Mid);
            if (clientMod == null) { clientMod = new M_CRMS_Client(); }
            return View(clientMod);
        }
        
        public IActionResult Client_Add(M_CRMS_Client model)
        {
            model.ClientType = StrHelper.IdsFormat(RequestEx["ClientType"]);
            model.ID = Mid;
            M_ModelInfo modMod = modBll.SelReturnModel(48);
            //DataTable fieldDT = fieldBll.SelByModelID(modMod.ModelID, false);
            //DataTable table = new Call().GetDTFromMVC(fieldDT, Request);
            model.ModelID = modMod.ModelID;
            model.ModelTable = modMod.TableName;
            model.CUserID = mu.UserID;
            model.LinkIds = RequestEx["LinkIds"];
            if (model.ID < 1)
            {
                model.ID = clientBll.Insert(model, new DataTable());
            }
            else
            {
                M_CRMS_Client clientMod = clientBll.SelReturnModel(Mid);
                model.ItemID = clientMod.ItemID;
                model.CDate = clientMod.CDate;
                model.CUserID = clientMod.CUserID;
                model.Flow = clientMod.Flow;
                clientBll.UpdateByID(model, new DataTable());
            }
            return WriteOK("操作成功", "ConstPassen");
        }
        public int Client_Del(string ids)
        {
            clientBll.Del(ids);
            return Success;
        }
        #endregion

        public IActionResult UserSignin()
        {
            ViewBag.issign = sinBll.IsSignToday(mu.UserID);
            ViewBag.signdays = sinBll.GetHasSignDays(mu.UserID);
            return View();
        }
        //用户签到
        [HttpPost]
        public string UserSign_Add()
        {
            M_APIResult retMod = new M_APIResult(Failed);      
            M_User_Signin sinMod = new M_User_Signin();    
            if (!sinBll.IsSignToday(mu.UserID))
            {
                sinMod.CreateTime = DateTime.Now;
                sinMod.UserID = mu.UserID;
                sinMod.Status = 1;
                sinMod.Remind = mu.UserName + "签到";
                sinBll.Insert(sinMod);
                retMod.retcode = M_APIResult.Success;
            }
            else
            {
                retMod.retmsg = "你已经签过到了";
            }
            return retMod.ToString();
        }
        #region 广告申请
        public IActionResult AdPlan()
        {
            PageSetting setting = adbBll.SelPage(CPage, PSize, new Com_Filter()
            {
                uids = mu.UserID.ToString(),
                status = "0,99"
            });
            if (Request.IsAjaxRequest()) { return PartialView("AdPlan_List", setting); }
            return View(setting);
        }
        public IActionResult AdPlanAdd()
        {
            M_AdBuy adbMod = new M_AdBuy();
            if (Mid > 0)
            {
                adbMod= adbBll.SelReturnModel(Mid);
                if (adbMod == null|| adbMod.State != 0) { return WriteErr("申请不存在"); }
                if (adbMod.UID != mu.UserID) { return WriteErr("你无权修改该申请"); }
                if (adbMod.Audit) { return WriteErr("已审核的申请不可修改"); }
            }
            return View(adbMod);
        }
        public IActionResult AdPlan_Add()
        {
            M_AdBuy adMod = adbBll.SelReturnModel(Mid);
            if (adMod == null) { adMod = new M_AdBuy(); }
            //实为版位ID ZoneID
            adMod.ADID = DataConverter.CLng(RequestEx["ADID"]);
            if (adMod.ADID < 1) { return WriteErr("版位不正确","AdPlanAdd"); }
            adMod.Ptime = DataConverter.CDate(RequestEx["PTime"]);
            adMod.ShowTime = DataConverter.CLng(RequestEx["ShowTime"]);
            adMod.Scale = DataConverter.CLng(RequestEx["scale"]);
            adMod.Price = DataConverter.CDecimal(RequestEx["price"]);
            adMod.Content = RequestEx["content"];
            adMod.Files = RequestEx["Files_t"];
            if (adMod.Price <= 0) { return WriteErr("金额不正确"); }
            if (adMod.ShowTime <= 0) { return WriteErr("天数不正确"); }
            if (adMod.ID > 0)
            {
                adbBll.UpdateByID(adMod);
                return WriteOK("修改成功!!", "AdPlan");
            }
            else
            {
                adMod.UID = mu.UserID;
                adMod.Time = DateTime.Now;
                adbBll.Insert(adMod);
                return WriteOK("恭喜，您的广告计划提交成功，请尽快付款完成购买!!", "AdPlan");
            }
        }
        public int AdPlan_Del(string ids)
        {
            //删除入回收站
            adbBll.Change(ids,(int)ZLEnum.ConStatus.Recycle,mu.UserID);
            //adbBll.Del(ids, mu.UserID);
            return Success;
        }
        #endregion
        public IActionResult TalkLog()
        {
            string reuser = RequestEx["reuser"];
            int reuid = buser.GetUserByName(reuser).UserID;
            PageSetting setting = msgbll.SelPage(CPage, PSize, mu.UserID, reuid, RequestEx["sdate"], RequestEx["edate"]);
            if (Request.IsAjaxRequest()) { return PartialView("TalkLog_List", setting); }
            return View(setting);
        }
        public IActionResult TalkLog_Down()
        {
            DataTable dt = msgbll.SelByWhere(mu.UserID, buser.GetUserByName(RequestEx["reuser"]).UserID, RequestEx["sdate"], RequestEx["edate"]);
            if (dt.Rows.Count < 1) { return WriteErr("没有聊天记录,无法导出"); }
            StringBuilder sb = new StringBuilder();
            foreach (DataRow dr in dt.Rows)
            {
                sb.Append(dr["UserName"] + dr["CDate"].ToString() + ":\r\n");
                sb.Append(dr["Content"].ToString() + "\r\n");
                sb.Append("---------------------------------------------------------------\r\n");
            }
            string vpath = "/Temp/ChatHis/";
            string filename = function.GetRandomString(8) + ".txt";
            SafeSC.WriteFile(vpath + filename, sb.ToString());
            //SafeSC.DownFile(vpath + filename);
            SafeSC.DelFile(vpath + filename);
           // Response.End();
           return View();
        }
        public IActionResult PromotUnion()
        {
            return View();
        }
        public IActionResult Watermark()
        {
            ViewBag.username = mu.UserName;
            return View();
        }
        public IActionResult HtmlToJPG()
        {
            //IEBrowHelper ieHelp = new IEBrowHelper();
            //string vpath = ZLHelper.GetUploadDir_User(mu, "UserFunc");
            //Bitmap m_Bitmap = ieHelp.GetWebSiteThumbnail(Request.Url.Scheme + "://" + Request.Url.Authority + "/BU/Cpic" + Request.Url.Query, 1024, 723, 1024, 723);
            //MemoryStream ms = new MemoryStream();
            //m_Bitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Png);//JPG、GIF、PNG等均可 
            //byte[] buff = ms.ToArray();
            //string fname = function.GetRandomString(3) + ".jpg";
            //SafeSC.SaveFile(vpath, fname, buff);
            //ViewBag.imgsrc = vpath + fname;
            //ViewBag.title = RequestEx["Name"] + "的证书_" + Call.SiteName;
            //ViewBag.imgalt = RequestEx["Name"] + "的证书";
            return View();
        }
        //public void DownFile(string url)
        //{
        //    SafeSC.DownFile(url);
        //}
        //用户可自由变换所属会员组,仅VIP组可访问该页面(前端判断)
        public IActionResult ChangeGroup()
        {
            M_Group gpMod = gpBll.SelReturnModel(mu.GroupID);
            M_Temp tpMod = tpBll.SelModelByUid(mu.UserID, 13);
            if (gpMod.VIPGroup != 1 && tpMod == null) { return WriteErr("你所在的会员组无权使用该功能页");  }
            ViewBag.gpMod = gpMod;
            ViewBag.gpdt = gpBll.Sel();
            return View();
        }
        [HttpPost]
        public IActionResult Group_Change()
        {
            M_Temp tpMod = tpBll.SelModelByUid(mu.UserID, 13);
            M_Group gpMod = gpBll.SelReturnModel(mu.GroupID);
            string action = RequestEx["action_hid"];
            if (gpMod.VIPGroup != 1 && tpMod == null) { return WriteErr("你所在的会员组无权使用该功能页", "ChangeGroup");  }
            switch (action)
            {
                case "change":
                    int gid = Convert.ToInt32(RequestEx["Group_DP"]);
                    if (tpMod == null)
                    {
                        tpMod = new M_Temp();
                        tpMod.UserID = mu.UserID;
                        tpMod.Str1 = mu.GroupID.ToString();
                        tpMod.UseType = 13;
                        tpBll.Insert(tpMod);
                    }
                    buser.UpdateGroupId(mu.UserID.ToString(), gid);
                    return WriteOK("切换会员组成功", "ChangeGroup");
                case "recover":
                    if (tpMod == null) { return WriteErr("未找到切换前的会员组记录");  }
                    buser.UpdateGroupId(mu.UserID.ToString(), Convert.ToInt32(tpMod.Str1));
                    tpBll.Del(tpMod.ID);
                    return WriteOK("恢复会员组成功", "ChangeGroup");
                default:
                    return WriteErr("未指定操作");
                    
            }
        }
    }
}
