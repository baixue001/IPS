using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ZoomLa.Common;
using ZoomLaCMS.Ctrl;

namespace ZoomLaCMS.Areas.User.Controllers
{
    public class CommonController : Ctrl_User
    {
        [Area("User")]
        [Authorize(Policy = "User")]
        public IActionResult SearchResult()
        {
            string Skey = (RequestEx["key"] ?? "").Trim();
            DataTable dt = new DataTable();
            if (string.IsNullOrEmpty(Skey)) { return View(dt); }
            DataSet ds = new DataSet();
            ds.ReadXml(function.VToP("/Config/UserMap.config"));
            dt = ds.Tables[0];
            dt.DefaultView.RowFilter = "Title Like '%" + Skey + "%'";
            dt = dt.DefaultView.ToTable();
            dt = DecreateDT(dt, Skey);
            return View(dt);
        }
        private DataTable DecreateDT(DataTable dt, string key)
        {
            dt.Columns.Add(new DataColumn("Index", typeof(int)));
            dt.Columns.Add(new DataColumn("DTitle", typeof(string)));
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                dt.Rows[i]["Index"] = (i + 1);
                dt.Rows[i]["Url"] = dt.Rows[i]["Url"].ToString().Replace("","");
                string title = dt.Rows[i]["Title"].ToString();
                dt.Rows[i]["DTitle"] = title.Replace(key, "<span style='color:red;'>" + key + "</span>");
            }
            return dt;
        }
    }
}
