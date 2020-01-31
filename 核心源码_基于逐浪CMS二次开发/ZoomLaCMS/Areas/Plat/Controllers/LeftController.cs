using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ZoomLa.BLL;
using ZoomLa.BLL.Plat;
using ZoomLa.Model;
using ZoomLa.SQLDAL;
using Microsoft.AspNetCore.Http;
using ZoomLaCMS.Ctrl;
using Microsoft.AspNetCore.Authorization;

namespace ZoomLaCMS.Areas.Plat.Controllers
{
    [Area("Plat")]
    [Authorize(Policy = "Plat")]
    public class LeftController : Ctrl_Plat
    {
        B_Search searchBll = new B_Search();
        B_Plat_File fileBll = new B_Plat_File();
        B_User_Plat upBll = new B_User_Plat();
        B_Plat_Group gpBll = new B_Plat_Group();

        public string MyVPath
        {
            get
            {
                string key = "CompDoc_VPath";
                if (string.IsNullOrEmpty(HttpContext.Session.GetString(key)))
                {
                    HttpContext.Session.SetString(key,B_Plat_Common.GetDirPath(upMod,B_Plat_Common.SaveType.Company));
                }
                return HttpContext.Session.GetString(key);
            }
        }
        public PartialViewResult app()
        {
            DataTable dt = new DataTable();
            dt = searchBll.SelByUserGroup(0);
            return PartialView(dt);
        }
        public PartialViewResult doc()
        {
            DataTable dt = new DataTable();
            dt = fileBll.SelByVPath(MyVPath);
            return PartialView(dt);
        }
        public PartialViewResult news()
        {
            string where = "Pid=0 AND Status>0 AND ATUser LIKE '%," + upMod.UserID + ",%'";
            DataTable dt = DBCenter.SelWithField("ZL_Plat_Blog", "TOP 9 ID,MsgContent,CUName,CUser,CDate", where, "ID DESC");
            return PartialView(dt);
        }
        public PartialViewResult star()
        {
            string where = " ColledIDS LIKE '%," + upMod.UserID + ",%' ";
            DataTable dt = DBCenter.SelWithField("ZL_Plat_Blog", "TOP 12 ID,MsgContent,CUName,CUser,CDate", where, "ID DESC");
            return PartialView(dt);
        }
        public PartialViewResult Users()
        {    
            //未关联部门的用户
            DataTable UserDT = upBll.SelByCompany(upMod.CompID);

            DataTable gpdt = DBCenter.SelWithField("ZL_Plat_Group", "ID,GroupName,MemberIDS", "1=0");
            DataTable dt = DBCenter.SelWithField("ZL_Plat_Group", "ID,GroupName,MemberIDS");
            DataRow dr = gpdt.NewRow();
            dr["ID"] = 0;
            dr["GroupName"] = "未关联部门用户";
            gpdt.Rows.Add(dr);
            gpdt.Merge(dt);
            ViewBag.UserDT = UserDT;
            return PartialView(gpdt);
        }
    }
}