using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using ZoomLa.AppCode.Verify;
using ZoomLa.BLL;
using ZoomLa.Common;
using ZoomLa.Components;
using ZoomLa.Extend.Comp;
using ZoomLa.Model;
using ZoomLa.SQLDAL.SQL;
using ZoomLaCMS.AppCode.Verify;
using ZoomLaCMS.Control;
using ZoomLaCMS.Ctrl;

namespace ZoomLaCMS.Controllers
{
    public class CommonController : Ctrl_User
    {
        public IActionResult ValidateCode()
        {
            string key = GetParam("key");
            if (string.IsNullOrEmpty(key)) { return Content(Failed.ToString()); }
            if (Request.IsAjax())
            {
                string action = Request.Form["action"];
                string value = Request.Form["value"];
                string result = "0";
                switch (action)
                {
                    case "checkcode":
                        if (value.ToLower().Equals(CMSCodeVerify.CodeDic[key])) result = "1";
                        break;
                    default:
                        break;
                }
                return Content(result);
            }
            else
            {
                string randomcode = function.GetRandomString(SiteConfig.SiteOption.VerifyLen, SiteConfig.SiteOption.VerifyForm);
                if (CMSCodeVerify.CodeDic.ContainsKey(key)) { CMSCodeVerify.CodeDic[key] = randomcode.ToLower(); }
                else { CMSCodeVerify.CodeDic.Add(key, randomcode.ToLower()); }
                //return File(CreateImage(randomcode).);
                MemoryStream ms = CaptchaCreate.CreateGif(randomcode);
                return File(ms.ToArray(), "image/gif");
            }
        }
        public IActionResult OutToExcel()
        {
            string headStr = HttpUtility.UrlDecode(Request.Form["table_head_hid"]);
            string data = HttpUtility.UrlDecode(Request.Form["table_data_hid"]);
            string[] headArr = JsonConvert.DeserializeObject<string[]>(headStr);
            DataTable dt = JsonConvert.DeserializeObject<DataTable>(data);
            for (int i = 0; i < dt.Columns.Count; i++)
            {
                dt.Columns[i].ColumnName = headArr[i];
            }
            MemoryStream ms = NPOIHelp.Excel_OutByDT(dt);
            return File(ms.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");

        }
        public IActionResult SelUser(C_SelUser filter)
        {
            if (filter == null) { filter = new C_SelUser(); }
            PageSetting setting = null;
            switch (filter.dataMode)
            {
                case "user"://全部用户
                default:
                    {
                        setting = buser.SelPage(filter.cpage, filter.psize, new F_User()
                        {
                            groupIds = filter.groupId.ToString(),
                            uname = filter.skey
                        });
                    }
                    break;
                case "plat"://仅能力中心用户
                    {
                        B_User_Plat upBll = new B_User_Plat();
                        M_User_Plat upMod = upBll.SelReturnModel(mu.UserID);
                        setting = upBll.SelPage(filter.cpage, filter.psize, new F_User()
                        {
                            compIds = upMod.CompID.ToString(),
                            groupIds = filter.groupId.ToString(),
                            uname = filter.skey
                        });
                        setting.dt.Columns["UserFace"].ColumnName = "salt";
                    }
                    break;
            }
            filter.psize = setting.psize;
            filter.r_pcount = setting.pageCount;
            filter.r_itemCount = setting.itemCount;
            filter.r_dt = setting.dt;
            if (Request.IsAjaxRequest())
            {
                return PartialView("SelUser_List", filter);
            }
            else { return View(filter); }

        }
    }
}