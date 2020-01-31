using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Web;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ZoomLa.BLL;
using ZoomLa.BLL.Content;
using ZoomLa.Common;
using ZoomLa.Model;
using ZoomLa.Model.Content;
using ZoomLa.SQLDAL;
using ZoomLa.SQLDAL.SQL;
using ZoomLaCMS.Ctrl;

namespace ZoomLaCMS.Areas.User.Controllers
{
    [Area("User")]
    [Authorize(Policy = "User")]
    public class PubController : Ctrl_User
    {
        B_Pub pubBll = new B_Pub();
        B_Node nodeBll = new B_Node();
        B_Model modelBll = new B_Model();
        B_Content conBll = new B_Content();
        B_ModelField mfBll = new B_ModelField();
        B_Sensitivity sll = new B_Sensitivity();
        B_UserPromotions ups = new B_UserPromotions();

        public int PubID
        {
            get
            {
                if (ViewBag.PubID == null) { ViewBag.PubID = DataConvert.CLng(RequestEx["pid"]); }
                return DataConvert.CLng(ViewBag.PubID);
            }
            set { ViewBag.PubID = value; }
        }
        public int GID
        {
            get
            {
                if (ViewBag.GID == null) { ViewBag.GID = DataConvert.CLng(RequestEx["ID"]); }
                return DataConvert.CLng(ViewBag.GID);
            }
            set { ViewBag.GID = value; }
        }
        public string PubTable
        {
            get
            {
                if (ViewBag.PubTable == null) { ViewBag.PubTable = RequestEx["PubTable"] ?? ""; }
                return ViewBag.PubTable;
            }
            set { ViewBag.PubTable = value; }
        }
        public void Index()
        {
            Response.Redirect("ManagePub");
        }
        public IActionResult ShowPub()
        {
            GID = DataConverter.CLng(RequestEx["ID"]);
            PubID = DataConverter.CLng(RequestEx["pid"]);
            string menu = RequestEx["menu"] ?? "";
            string Optimal = RequestEx["Optimal"] ?? "";
            M_Pub pubMod = pubBll.SelReturnModel(PubID);
            if (pubMod != null)
            {
                switch (menu)
                {
                    case "setinfo":
                        {
                        }
                        break;
                    case "setdb":
                        {
                            //if (Optimal.Equals("0"))
                            //{
                            //    pubBll.Getdb(pubMod.PubTableName, GID);
                            //}
                            //else if (Optimal.Equals("2"))
                            //{
                            //    pubBll.UpdatePubModelOptimal(pubMod.PubTableName, GID);
                            //}
                        }
                        break;
                    case "setnodb":
                        {
  
                        }
                        break;
                }
            }
            M_CommonData comMod = conBll.GetCommonData(GID);
            int ModelID = comMod.ModelID;
            bool IsLoginUser = comMod.Inputer.Equals(mu.UserName);
            //列表数据
            //PageSetting setting = IsLoginUser ? pubBll.SelPage(CPage, PSize, 0, GID, "", pubMod.PubTableName) : pubBll.SelPage(CPage, PSize, 0, GID, mu.UserName, pubMod.PubTableName);
            PageSetting setting = null;
            //详情数据
            DataRow DataDr = conBll.GetContentByItems(comMod.TableName, comMod.GeneralID).Rows[0];//要显示的数据
            DataTable FieldDt = mfBll.GetModelFieldList(ModelID);//要显示的字段列表
            DataTable newDt = new DataTable();
            newDt.Columns.Add("Title");
            newDt.Columns.Add("Content");
            DataRow dr1 = newDt.NewRow(); dr1["Title"] = "ID"; dr1["Content"] = DataDr["GeneralID"]; newDt.Rows.Add(dr1);
            DataRow dr2 = newDt.NewRow(); dr2["Title"] = "标题"; dr2["Content"] = DataDr["Title"]; newDt.Rows.Add(dr2);
            foreach (DataRow dr in FieldDt.Rows)
            {
                DataRow row = newDt.NewRow();
                row["Title"] = dr["FieldAlias"];
                row["Content"] = DataDr[dr["FieldName"].ToString()];
                newDt.Rows.Add(row);
            }
            ViewBag.Details = newDt;
            ViewBag.IsLoginUser = IsLoginUser;
            PubTable = pubMod.PubTableName;
            return View(setting);
        }
        //未处理
        public IActionResult ManagePub()
        {
            DataTable nodeDT = pubBll.Sel();
            if (nodeDT.Rows.Count < 1) { return WriteErr("互动无节点信息"); }
            if (PubID < 1) { return RedirectToAction("ManagePub", new { pid = nodeDT.Rows[0]["PubID"] }); }
            M_Pub pubMod = pubBll.SelReturnModel(PubID);
            PageSetting setting = B_Pub_Info.SelPage(CPage, PSize, new F_PubInfo()
            {
                uids = mu.UserID.ToString(),
                tbname = pubMod.PubTableName
            });
            ViewBag.nodeDT = nodeDT;
            return View(setting);
        }
        public PartialViewResult Pub_Data()
        {
            M_CommonData comMod = conBll.GetCommonData(GID);
            bool IsLoginUser = comMod.Inputer.Equals(mu.UserName);
            //PageSetting setting = IsLoginUser ? pubBll.SelPage(CPage, PSize, 0, GID, "", PubTable) : pubBll.SelPage(CPage, PSize, 0, GID, mu.UserName, PubTable);
            PageSetting setting = null;
            ViewBag.IsLoaginUser = IsLoginUser;
            return PartialView("ShowPub_List", setting);
        }
        public int Pub_Del(string ids)
        {
            //if (conBll.DelByIDS(ids)) { return 1; } else { return 0; }
            int mid = Convert.ToInt32(ids.Split(':')[0]);
            int pid = Convert.ToInt32(ids.Split(':')[1]);
            M_Pub pinfos = pubBll.GetSelect(pid);
            if (pubBll.DeleteModel(pinfos.PubTableName, "ID='" + mid + "'")) return 1;
            else return 0;
        }
        public IActionResult ShowPubList()
        {
            M_CommonData mc = conBll.GetCommonData(DataConverter.CLng(Mid));
            M_UserPromotions upsinfo = ups.GetSelect(mc.NodeID, mu.GroupID);
            if (upsinfo.pid != 0 && upsinfo.look != 1) { return WriteErr("您所在会员组无查看权限！"); return Content(""); }
            DataTable FieldDt = this.mfBll.GetModelFieldList(mc.ModelID);
            DataRow DataDr = conBll.GetContentByItems(mc.TableName, mc.GeneralID).Rows[0];
            DataTable newDt = new DataTable();
            newDt.Columns.Add("Title");
            newDt.Columns.Add("Content");
            DataRow dr1 = newDt.NewRow(); dr1["Title"] = "ID"; dr1["Content"] = DataDr["GeneralID"]; newDt.Rows.Add(dr1);
            DataRow dr2 = newDt.NewRow(); dr2["Title"] = "标题"; dr2["Content"] = DataDr["Title"]; newDt.Rows.Add(dr2);
            foreach (DataRow dr in FieldDt.Rows)
            {
                DataRow row = newDt.NewRow();
                row["Title"] = dr["FieldAlias"];
                row["Content"] = DataDr[dr["FieldName"].ToString()];
                newDt.Rows.Add(row);
            }
            return View(newDt);
        }
        public IActionResult Pub()
        {
            string skey = RequestEx["skey"];
            PageSetting setting = conBll.SelContent(CPage, PSize, 0, "", mu.UserName, skey);
            if (Request.IsAjaxRequest())
            {
                return PartialView("Pub_List", setting);
            }
            return View(setting);
        }
        public IActionResult Reply()
        {
            int ModelID = 0;
            int PubID = 0;
            string PubTable = "";
            int PubCID = 0;
            string PubInputer = "";
            string title = "";

            string pid = RequestEx["pubid"] ?? "";
            string id = RequestEx["id"] ?? "";
            if (!string.IsNullOrEmpty(pid) && !string.IsNullOrEmpty(id))
            {
                M_Pub mp = pubBll.GetSelect(DataConverter.CLng(pid));
                ModelID = mp.PubModelID;
                PubID = mp.Pubid;
                PubTable = mp.PubTableName;
                PubCID = DataConverter.CLng(id);
         
            }
            TempData["ModelID"] = ModelID;
            TempData["PubID"] = PubID;
            TempData["PubTable"] = PubTable;
            TempData["PubCID"] = PubCID;
            TempData["PubInputer"] = PubInputer;
            ViewBag.title = title;
            return View();
        }
        public IActionResult Reply_Submit()
        {
            int ModelID = DataConverter.CLng(TempData["ModelID"]);
            int PubID = DataConverter.CLng(TempData["PubID"]);
            string PubTable = TempData["PubTable"] as string;
            int PubCID = DataConverter.CLng(TempData["PubCID"]);
            string PubInputer = TempData["PubInputer"] as string;

            DataTable dt = mfBll.GetModelFieldList(ModelID);
            DataTable table = new DataTable();
            table.Columns.Add(new DataColumn("FieldName", typeof(string)));
            table.Columns.Add(new DataColumn("FieldType", typeof(string)));
            table.Columns.Add(new DataColumn("FieldValue", typeof(string)));

            foreach (DataRow dr in dt.Rows)
            {
                if (DataConverter.CBool(dr["IsNotNull"].ToString()))
                {
                    if (string.IsNullOrEmpty(RequestEx["txt_" + dr["FieldName"].ToString()]))
                    {
                        return WriteErr(dr["FieldAlias"].ToString() + "不能为空!"); 
                    }
                }
                if (dr["FieldType"].ToString() == "FileType")
                {
                    string[] Sett = dr["Content"].ToString().Split(new char[] { ',' });
                    bool chksize = DataConverter.CBool(Sett[0].Split(new char[] { '=' })[1]);
                    string sizefield = Sett[1].Split(new char[] { '=' })[1];
                    if (chksize && sizefield != "")
                    {
                        DataRow row2 = table.NewRow();
                        row2[0] = sizefield;
                        row2[1] = "FileSize";
                        row2[2] = RequestEx["txt_" + sizefield];
                        table.Rows.Add(row2);
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
                        DataRow row1 = table.NewRow();
                        row1[0] = sizefield;
                        row1[1] = "ThumbField";
                        row1[2] = RequestEx["txt_" + sizefield];
                        table.Rows.Add(row1);
                    }
                }
                DataRow row = table.NewRow();
                row[0] = dr["FieldName"].ToString();
                string ftype = dr["FieldType"].ToString();
                row[1] = ftype;
                string fvalue = RequestEx["txt_" + dr["FieldName"].ToString()];
                //if (ftype == "TextType" || ftype == "MultipleTextType" || ftype == "MultipleHtmlType")
                //{
                //    fvalue = fvalue;
                //}
                row[2] = fvalue;
                table.Rows.Add(row);
            }
            return WriteOK("回复成功", "ManagePub"); 
        }
        public IActionResult PubManage()
        {
            PageSetting setting = pubBll.SelPage(CPage, PSize,new F_Pub() { });
            if (Request.IsAjaxRequest()) { return PartialView("PubManage_List", setting); }
            return View(setting);
        }
        public IActionResult PubManageSubmit()
        {
            M_Pub pubMod = pubBll.SelReturnModel(PubID);
            if (pubMod.PubPermissions.IndexOf("Look") == -1)
            {
                return WriteErr("后台未赋予查看权限");
            }
            else
            {
                return RedirectToAction("PubInfo",new {pid = pubMod.Pubid });
            }
        }
        public IActionResult PubInfo()
        {
            //M_Pub pubMod = pubBll.SelReturnModel(PubID);
            //PageSetting setting = pubBll.GetComments_SPage(CPage, PSize, pubMod.PubTableName, mu.UserName);
            //if (Request.IsAjaxRequest()) { return PartialView("PubInfo_List", setting); }
            //return View(setting);
            return View();
        }
    }
}
