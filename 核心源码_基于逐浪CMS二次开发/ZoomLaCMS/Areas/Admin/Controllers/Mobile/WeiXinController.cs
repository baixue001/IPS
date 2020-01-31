using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using ZoomLa.BLL;
using ZoomLa.BLL.API;
using ZoomLa.BLL.Helper;
using ZoomLa.BLL.Other;
using ZoomLa.Common;
using ZoomLa.Model;
using ZoomLa.SQLDAL;
using ZoomLa.SQLDAL.SQL;
using ZoomLaCMS.Ctrl;

namespace ZoomLaCMS.Areas.Admin.Controllers.Mobile
{
    [Area("Admin")]
    [Authorize(Policy = "Admin")]
    [Route("[area]/Mobile/WeiXin/[action]")]
    public class WeiXinController : Ctrl_Admin
    {
        WxAPI api = null;
        string viewDir = "~/Areas/Admin/Views/Mobile/WeiXin/";
        public int AppId { get { return DataConvert.CLng(GetParam("appid")); } }
        B_WX_APPID appBll = new B_WX_APPID();
        B_WX_User wxuserBll = new B_WX_User();
        B_WX_PayLog logBll = new B_WX_PayLog();
        B_WX_ReplyMsg rpBll = new B_WX_ReplyMsg();
        public IActionResult Index()
        {
            return View(viewDir + "Index.cshtml");
        }
        public IActionResult WXAPPManage()
        {
            return View(viewDir + "WXAPPManage.cshtml");
        }
        public IActionResult WXAPP_API()
        {
            string action = GetParam("action");
            string ids = GetParam("ids");
            switch (action)
            {
                case "del":
                    appBll.Del(DataConverter.CLng(ids));
                    break;
                case "default":
                    appBll.SetDefault(DataConverter.CLng(ids));
                    break;
            }
            return Content(Success.ToString());
        }
        public IActionResult WelPage()
        {
            //if (!B_ARoleAuth.AuthCheckEx(ZLEnum.Auth.portable, "wechat")) {return WriteErr }
            M_WX_APPID appmod = appBll.SelReturnModel(AppId);
            M_WxImgMsg msgMod = new M_WxImgMsg();
            msgMod.Articles.Add(new M_WXImgItem());
            if (!string.IsNullOrEmpty(appmod.WelStr))
            {
                msgMod = JsonConvert.DeserializeObject<M_WxImgMsg>(appmod.WelStr);
                //try
                //{
                //    Title_T.Text = msgMod.Articles[0].Title;
                //    Content_T.Text = msgMod.Articles[0].Description;
                //    PicUrl_T.Text = msgMod.Articles[0].PicUrl;
                //    Url_T.Text = msgMod.Articles[0].Url;
                //}
                //catch { Content_T.Text = "数据格式错误:" + appmod.WelStr; }
            }
            return View(viewDir + "WelPage.cshtml", msgMod.Articles[0]);
        }
        public IActionResult WelPage_Submit(M_WXImgItem itemMod)
        {
            M_WxImgMsg msgMod = new M_WxImgMsg();
            itemMod.PicUrl = StrHelper.UrlDeal(itemMod.PicUrl);
            itemMod.Url = StrHelper.UrlDeal(itemMod.Url);
            msgMod.Articles.Add(itemMod);
            M_WX_APPID appmod = appBll.SelReturnModel(AppId);
            appmod.WelStr = JsonConvert.SerializeObject(msgMod);
            appBll.UpdateByID(appmod);
            WxAPI.Code_Get(appmod).AppId.WelStr = appmod.WelStr;
            return WriteOK("操作成功", "");
        }
        public IActionResult WxConfig()
        {
            M_WX_APPID wxmod = appBll.SelReturnModel(Mid);
            if (wxmod == null)
            {
                wxmod = new M_WX_APPID();
            }
            return View(viewDir + "WxConfig.cshtml", wxmod);
        }
        public IActionResult WXConfig_Submit(M_WX_APPID model)
        {
            M_WX_APPID wxmod = new M_WX_APPID();
            if (Mid > 0) wxmod = appBll.SelReturnModel(Mid);
            wxmod.Alias = model.Alias;
            wxmod.APPID = model.APPID.Trim();
            wxmod.Secret = model.Secret.Trim();
            wxmod.WxNo = model.WxNo.Trim();
            wxmod.Status = 1;
            wxmod.OrginID = model.OrginID.Trim();
            wxmod.QRCode = GetParam("qrcode_t");
            if (Mid > 0) appBll.UpdateByID(wxmod);
            else { wxmod.ID = appBll.Insert(wxmod); }
            return WriteOK("保存成功!", "WxAppManage");
        }
        //public IActionResult ReplyList()
        //{
        //    return View("WeiXin/ReplyList");
        //}
        //public IActionResult MsgTlpList()
        //{
        //    return View();
        //}
        public IActionResult WxPayConfig()
        {
            if (AppId <= 0) { return WriteErr("没有指定公众号ID"); }
            M_WX_APPID appmod = appBll.SelReturnModel(AppId);
            if (appmod == null) { return WriteErr("公众号不存在"); }
            //APPID_T.Text = appmod.Pay_APPID;
            //AccountID_T.Text = appmod.Pay_AccountID;
            //Key_T.Text = appmod.Pay_Key;
            //SSLPath_T.Text = appmod.Pay_SSLPath;
            //SSLPassword_T.Text = appmod.Pay_SSLPassword;
            //string alias = " [公众号:" + appmod.Alias + "]";
            return View(viewDir + "WxPayConfig.cshtml", appmod);
        }
        public IActionResult WxPayConfig_Submit(M_WX_APPID model)
        {
            M_WX_APPID appmod = appBll.SelReturnModel(AppId);
            appmod.Pay_APPID = model.Pay_APPID.Trim();
            appmod.Pay_AccountID = model.Pay_AccountID.Trim();
            appmod.Pay_Key = model.Pay_Key.Trim();
            appmod.Pay_SSLPath = model.Pay_SSLPath.Trim();
            appmod.Pay_SSLPassword = model.Pay_SSLPassword.Trim();
            appBll.UpdateByID(appmod);
            return WriteOK("操作成功", "WxPayConfig");
        }
        public IActionResult WxUserList()
        {
            ViewBag.viewDir = viewDir;
            PageSetting setting = wxuserBll.SelPage(CPage, PSize, new Com_Filter() { storeId = AppId, skey = GetParam("skey") });
            //if (dt.Rows.Count <= 0)
            //{
            //    dt = GetUserList();//从微信平台获取关注者
            //    dt.DefaultView.RowFilter = "Name LIKE '%" + name + "%'";
            //}
            //dt.DefaultView.Sort = "ID DESC";
            //EGV.DataSource = dt;
            //EGV.DataBind();
            //M_WX_APPID appmod = appBll.SelReturnModel(AppId);
            if (Request.IsAjax())
            {
                return PartialView(viewDir + "WxUser_List", setting);
            }
            return View(viewDir + "WxUserList.cshtml", setting);
        }
        public IActionResult WXUser_API()
        {
            WxAPI api = WxAPI.Code_Get(AppId);
            string action = GetParam("action");
            string result = "";
            switch (action)
            {
                case "update":
                    string openid = GetParam("openid");
                    M_WX_User oldmod = wxuserBll.SelForOpenid(openid);
                    if (oldmod != null && oldmod.ID > 0)
                    {
                        M_WX_User usermod = api.GetWxUserModel(openid);
                        usermod.ID = oldmod.ID;
                        usermod.CDate = DateTime.Now;
                        usermod.AppId = AppId;
                        wxuserBll.UpdateByID(usermod);
                        result = JsonConvert.SerializeObject(usermod);
                    }
                    else
                    {
                        result = "-1";
                    }
                    break;
                default:
                    break;
            }
            return Content(result);
        }
        public IActionResult EditWxMenu()
        {
            try { api = WxAPI.Code_Get(AppId); } catch (Exception ex) { return WriteErr("微信公众号配置不正确," + ex.Message); }
            if (Request.IsAjax())
            {
                M_APIResult result = new M_APIResult();
                result.retcode = M_APIResult.Failed;
                WxAPI api = WxAPI.Code_Get(AppId);
                string action = GetParam("action");
                //result.result = api.AccessToken;
                //RepToClient(result);
                try
                {
                    switch (action)
                    {
                        case "create":
                            string jsondata = "{\"button\":" + Request.Form["menus"] + "}";
                            result.result = api.CreateWxMenu(jsondata);
                            if (!result.result.Contains("errmsg")) { result.retcode = M_APIResult.Success; }
                            else { result.retmsg = result.result; }
                            break;
                        case "get":
                            result.result = api.GetWxMenu();
                            if (!result.result.Contains("errmsg")) { result.retcode = M_APIResult.Success; }
                            else { result.retmsg = result.result; }
                            break;
                        default:
                            result.retmsg = "接口[" + action + "]不存在";
                            break;
                    }
                }
                catch (Exception ex) { result.retmsg = ex.Message; }
                return Content(result.ToString());
            }
            else
            {
                return View(viewDir + "EditWxMenu.cshtml");
            }
        }

        public IActionResult ReplyList()
        {
            string path = viewDir + "Reply_List.cshtml";
            ViewBag.path = path;
            PageSetting setting = rpBll.SelPage(CPage, PSize, new Com_Filter()
            {
                skey = GetParam("skey"),
                storeId = AppId
            });
            if (Request.IsAjax())
            {
                return PartialView(path, setting);
            }
            else
            {
                return View(viewDir + "ReplyList.cshtml", setting);
            }

        }
        public IActionResult Reply_API()
        {
            switch (action)
            {
                case "del":
                    rpBll.DelByIDS(ids);
                    break;
            }
            return Content(Success.ToString());
        }
        public IActionResult ReplyAdd()
        {

            if (AppId <= 0) {return WriteErr("请先配置公众号信息,再进入该页面,[<a href='WxAppManage.aspx'>前往配置</a>]"); }
            B_ARoleAuth.AuthCheckEx(ZLEnum.Auth.portable, "wechat");
            //M_WX_APPID appmod = appBll.SelReturnModel(AppId);
            //string alias = " [公众号:" + appmod.Alias + "]";
            M_WX_ReplyMsg rpMod = new M_WX_ReplyMsg();
            M_WXImgItem rpItem = new M_WXImgItem();
            if (Mid > 0)
            {
                rpMod = rpBll.SelReturnModel(Mid);
                rpItem = JsonConvert.DeserializeObject<M_WXImgItem>(rpMod.Content);
                //IsDefault_Chk.Checked = rpMod.IsDefault == 1;
            }

            ViewBag.rpItem = rpItem;
            return View(viewDir+ "ReplyAdd.cshtml",rpMod);
        }
        public IActionResult ReplyAdd_Submit()
        {
            M_WX_ReplyMsg rpMod = new M_WX_ReplyMsg();
            if (Mid > 0) { rpMod = rpBll.SelReturnModel(Mid); }
            M_WXImgItem item = new M_WXImgItem() { Title = GetParam("Title_T"), Description = GetParam("Content_T"), PicUrl = GetParam("PicUrl_T"), Url = GetParam("Url_T") };
            rpMod.fiter = GetParam("filter_T");
            rpMod.Content = JsonConvert.SerializeObject(item);
            rpMod.MsgType = item.IsText ? "0" : "1";
            rpMod.AppId = AppId;
            rpMod.MsgType = Request.Form["msgtype_rad"];
            rpMod.IsDefault = DataConvert.CLng(GetParam("IsDefault_Chk"));
            if (Mid > 0)
            {
                rpBll.UpdateByID(rpMod);
            }
            else { rpBll.Insert(rpMod); }
            return WriteOK("ReplyList?appid=" + AppId);
        }
        public IActionResult PayLog()
        {
            ViewBag.viewDir = viewDir;
            PageSetting setting = logBll.SelPage(CPage, PSize, new Com_Filter()
            {
                storeId = AppId,
                skey = GetParam("skey")
            });
            if (Request.IsAjax())
            {
                return PartialView(viewDir + "PayLog_List.cshtml", setting);
            }
            else
            {
                return View(viewDir+"PayLog.cshtml",setting);
            }
        }
    }
}