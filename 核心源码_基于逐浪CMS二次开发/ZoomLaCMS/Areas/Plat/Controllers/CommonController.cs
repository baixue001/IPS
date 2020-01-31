using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using ZoomLa.BLL;
using ZoomLa.BLL.API;
using ZoomLa.BLL.Helper;
using ZoomLa.BLL.Plat;
using ZoomLa.BLL.User;
using ZoomLa.Model;
using ZoomLa.SQLDAL.SQL;
using ZoomLaCMS.Control;
using ZoomLaCMS.Ctrl;

namespace ZoomLaCMS.Areas.Plat.Controllers
{
    [Area("Plat")]
    [Authorize(Policy = "Plat")]
    public class CommonController : Ctrl_Plat
    {
        private static Dictionary<int, Dictionary<string, DateTime>> ReadData = new Dictionary<int, Dictionary<string, DateTime>>();
        B_Blog_Msg msgBll = new B_Blog_Msg();
        B_Plat_Sign signBll = new B_Plat_Sign();
        M_APIResult retMod = new M_APIResult(M_APIResult.Failed);
        //common.ashx
        public ContentResult Common()
        {
            string value = GetParam("value");
            string result = "";
            string action = GetParam("action");
            switch (action.ToLower())
            {
                default:
                    return Content("未匹配接口");
                case "plat_compuser"://获取公司中成员,用于@功能
                    {
                        DataTable dt = upBll.SelByCompWithAT(upMod.CompID);
                        result = JsonHelper.JsonSerialDataTable(dt);
                        return Content(result);
                    }
                case "getuinfo"://获取单个用户信息(只允许获取本公司),返回的信息存入Json,避免重复检测,后期将服务端也缓存化
                    {
                        int uid = Convert.ToInt32(value);
                        M_User_Plat model = upBll.SelReturnModel(uid, upMod.CompID);
                        if (model != null)
                            result = "{\"id\":\"" + model.UserID + "\",\"UserID\":\"" + model.UserID + "\",\"UserName\":\"" + model.TrueName + "\",\"Mobile\":\"" + model.Mobile + "\",\"GroupName\":\"" + model.GroupName.Trim(',') + "\",\"UserFace\":\"" + model.UserFace + "\"}";
                        return Content(result);
                    }
                case "getnotify"://获取提醒
                    {
                        //B_Notify notBll = new B_Notify();
                        //if (B_Notify.NotifyList.Count < 1) { retMod.retmsg = "none"; }
                        //else
                        //{
                        //    notBll.RemoveExpire();//去除超时的
                        //    List<M_Notify> list = notBll.GetNotfiyByUid(mu.UserID);
                        //    DataTable retdt = new DataTable();
                        //    retdt.Columns.Add(new DataColumn("title", typeof(string)));
                        //    retdt.Columns.Add(new DataColumn("content", typeof(string)));
                        //    retdt.Columns.Add(new DataColumn("cuname", typeof(string)));
                        //    if (list.Count > 0)
                        //    {
                        //        foreach (M_Notify model in list)//有多个就发多条
                        //        {
                        //            notBll.AddReader(model, mu.UserID);
                        //            DataRow dr = retdt.NewRow();
                        //            dr["title"] = model.Title;
                        //            dr["content"] = model.Content;
                        //            dr["cuname"] = model.CUName;
                        //            retdt.Rows.Add(dr);
                        //        }
                        //    }
                        //    retMod.retcode = M_APIResult.Success;
                        //    retMod.result = JsonConvert.SerializeObject(retdt);
                        //}
                        return Content(retMod.ToString());
                    }
                case "newblog"://自己公司有无新的信息
                    {
                        result = msgBll.SelByDateForNotify(GetParam("date"), upMod).ToString();
                        return Content(result);
                    }
                case "privatesend"://私信功能,走邮件模块
                    {
                        if (upMod != null)
                        {
                            string msg = GetParam("msg");
                            string receuser = GetParam("receuser");
                            if (!string.IsNullOrWhiteSpace(msg) && !string.IsNullOrWhiteSpace(receuser) && SafeSC.CheckIDS(receuser))
                            {
                                //过滤非用户公司的同事,后期处理
                                M_Message msgMod = new M_Message();
                                B_Message msgBll = new B_Message();
                                msgMod.Incept = receuser;
                                msgMod.Sender = upMod.UserID;
                                msgMod.Title = upMod.TrueName + "的私信";
                                msgMod.PostDate = DateTime.Now;
                                msgMod.Content = msg;
                                msgMod.Savedata = 0;
                                msgMod.Receipt = "";
                                msgMod.CCUser = "";
                                msgMod.Attachment = "";
                                msgBll.GetInsert(msgMod);
                                result = "99";
                                //添加一条新提醒
                                //B_Notify.AddNotify(upMod.UserName, "你收到一封私信", msgMod.Title, msgMod.Incept);
                            }
                            else result = "-1";
                        }
                        else { result = "0"; }//未登录
                        return Content(result);

                    }
                case "addread"://阅读量统计
                    {
                        string ids = GetParam("ids");
                        string paraIds = "";
                        Dictionary<string, DateTime> dataValue;
                        //为True说明ReadData中有该用户浏览记录
                        if (ReadData.TryGetValue(mu.UserID, out dataValue))
                        {
                            //移除超时的记录并剔除IDS中在30s内浏览过的记录ID
                            foreach (string id in ids.Split(",".ToCharArray(), StringSplitOptions.RemoveEmptyEntries))
                            {
                                bool isRead = true;
                                foreach (string key in new List<string>(dataValue.Keys))
                                {
                                    //移除超过30s的记录
                                    if ((DateTime.Now - dataValue[key]).TotalMilliseconds > 30000) { dataValue.Remove(key); continue; }
                                    //30s内浏览过的内容不+阅读量
                                    if (key.Contains("," + id + ",")) { isRead = false; break; }
                                }
                                if (isRead) { paraIds += id + ","; }
                            }
                            paraIds = paraIds.Trim(',');
                            //保存本次浏览记录
                            if (!dataValue.ContainsKey("," + paraIds + ",")) { dataValue.Add("," + paraIds + ",", DateTime.Now); }
                        }
                        else
                        {
                            dataValue = new Dictionary<string, DateTime>();
                            paraIds = ids.Trim(',');
                            dataValue.Add("," + paraIds + ",", DateTime.Now);
                            //保存记录
                            ReadData.Add(mu.UserID, dataValue);
                        }
                        msgBll.AddRead(paraIds);
                        return Content("1");
                    }
            }
        }
        public ContentResult Signin()
        {
            string action = GetParam("action");
            try
            {
                switch (action)
                {
                    case "signinit":
                        {
                            retMod.result = signBll.GetSignType(mu);
                            retMod.addon = signBll.GetToDaySign(mu);
                            retMod.retcode = M_APIResult.Success;
                        }
                        break;
                    case "signin":
                        {
                            retMod.result = signBll.SignIn(mu).ToString();
                            retMod.retcode = M_APIResult.Success;
                        }
                        break;
                    case "signout":
                        {
                            retMod.result = signBll.SignOut(mu).ToString();
                            retMod.retcode = M_APIResult.Success;
                        }
                        break;
                    default:
                        retMod.retmsg = "[" + action + "]接口不存在";
                        break;
                }
            }
            catch (Exception ex) { retMod.retmsg = ex.Message; }
            return Content(retMod.ToString());
        }
        public IActionResult SelUser(C_SelUser filter)
        {
            if (filter == null)
            {
                filter = new C_SelUser();
            }
            filter.dataMode = "plat";
            return RedirectToAction("SelUser", "Common", filter);
        }
        public IActionResult UserDetail()
        {
            if (Mid < 1) { return WriteErr("用户ID传入错误"); }
            M_User_Plat upMod = upBll.SelReturnModel(Mid);
            return View(upMod);
        }
    }
}