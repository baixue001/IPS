using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Web;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ZoomLa.BLL;
using ZoomLa.BLL.API;
using ZoomLa.BLL.CreateJS;
using ZoomLa.BLL.Helper;
using ZoomLa.Common;
using ZoomLa.Model;
using ZoomLa.SQLDAL;
using ZoomLaCMS.Ctrl;

namespace ZoomLaCMS.Areas.AdminMVC.Controllers
{
    [Area("Admin")]
    [Authorize(Policy = "Admin")]
    public class ComController : Ctrl_Admin
    {
        M_APIResult retMod = new M_APIResult(-1);
        public ContentResult Logout()
        {
            if (DataConvert.CLng(Request.GetParam("preload")) == 1) { return null; }
            //try
            //{

            //}
            //catch (Exception ex)
            //{
            //    ZLLog.L(ZLEnum.Log.safe, "admin signout err:" + ex.Message);
            //}
            B_Admin.ClearLogin(HttpContext);
            string url = Request.GetParam("ReturnUrl");
            if (string.IsNullOrEmpty(url)) { url = CustomerPageAction.customPath2 + "login"; }
            //回发清除cookies
            //Response.Write("<script></script>");
            return Content("<script>location='" + url + "';</script>");
        }
        [HttpPost]
        public ContentResult SPwdCheck()
        {
            string value = RequestEx["value"];
            if (string.IsNullOrEmpty(value)) { retMod.retmsg = "二级密码不可为空"; }
            else if (!adminMod.RandNumber.Equals(value)) { retMod.retmsg = "二级密码不正确"; }
            else {
                HttpContext.Session.SetString("SPWD", value);
                retMod.retcode = Success;
            }
            return Content(retMod.ToString());
        }
        public IActionResult BootLayout()
        {
            return View();
        }
        #region Import
        public string Mode { get { return RequestEx["mode"]; } }
        public string XMLPath { get { return function.VToP("/Config/Import/" + Mode + ".xml"); } }
        public IActionResult Import()
        {
            if (string.IsNullOrEmpty(Mode)) {  return WriteErr("未指定导出操作"); }
            if (!System.IO.File.Exists(XMLPath)) {  return WriteErr("指定的导入配置不存在"); }
            return View();
        }
        public IActionResult Import_DownTlp()
        {
            Import_Logical.Excel_CreateByXML(XMLPath, "Template");
            return View();
        }
        [HttpPost]
        public IActionResult Import_Submit()
        {
            var file = Request.Form.Files["file_up"];
            if (file == null) { return WriteErr("未指定文件"); }
            if (file.Length < 100) { return WriteErr("文件为空"); }
            string ext = Path.GetExtension(file.FileName).ToLower();
            if (ext != ".xlsx") { return WriteErr("只允许上传xlsx文件");  }
            //-------------------------
            DataTable excelDT = Import_Logical.Excel_ToDT(XMLPath, file.OpenReadStream());
            B_CodeModel codeBll = new B_CodeModel(excelDT.TableName);
            DataTable modelDT = codeBll.SelStruct();
            //将数据直接写入数据库,或将其转换为模型再写入
            foreach (DataRow dr in excelDT.Rows)
            {
                //将第一列作为主键忽略掉
                codeBll.Insert(dr, modelDT.Columns[0].ColumnName);
            }
            return WriteOK("数据导入完成");
        }
        #endregion
        #region Sort
        public string TbName { get { return DataConvert.CStr(RequestEx["tbname"]); } }
        public string Ids { get { return DataConvert.CStr(RequestEx["Ids"]).Trim(','); } }
        private string TableName
        {
            get
            {
                string result = "";
                switch (TbName.ToLower())
                {
                    case "commonmodel":
                        result = "ZL_CommonModel";
                        break;
                    case "product":
                        result = "ZL_Commodities";
                        break;
                    case "modelfield":
                        result = "ZL_ModelField";
                        break;
                }
                return result;

            }
        }
        public IActionResult Sort()
        {
            ViewBag.TbName = TbName;
            DataTable dt = new DataTable();
            string fields = "";
            string where = "";
            switch (TbName.ToLower())
            {
                case "commonmodel":
                    {
                        if (!string.IsNullOrEmpty(Ids))
                        {
                            SafeSC.CheckIDSEx(Ids);
                            where += " GeneralID IN (" + Ids + ")";
                        }
                        fields = "GeneralID AS id,OrderID AS [order],title,Inputer AS remind";
                        dt = DBCenter.SelWithField(TableName, fields, where, "OrderID DESC");
                    }
                    break;
                case "product":
                    {
                        if (!string.IsNullOrEmpty(Ids))
                        {
                            SafeSC.CheckIDSEx(Ids);
                            where += " ID IN (" + Ids + ")";
                        }
                        fields = "id,OrderID AS [order],ProName AS title,ProInfo AS remind";
                        dt = DBCenter.SelWithField(TableName, fields, where, "OrderID DESC");
                    }
                    break;
                case "modelfield"://模型
                    {
                        int modelId = DataConvert.CLng(RequestEx["modelId"]);
                        fields = "FieldID AS id,OrderID as [order],FieldAlias AS title,FieldName as remind";
                        where = "ModelID=" + modelId + " AND sys_type=0";
                        dt = DBCenter.SelWithField(TableName, fields, where, "OrderID ASC");
                    }
                    break;
                    //case "node"://对父栏目下子节点排序
                    //    {
                    //        int pid = DataConvert.CLng(Request["pid"]);
                    //    }
                    //break;
            }
            return View(dt);
        }
        [HttpPost]
        public IActionResult Sort_API()
        {
            //tbname,字段规则
            string orderStr = DataConvert.CStr(RequestEx["orderStr"]).Trim(',');
            if (string.IsNullOrEmpty(orderStr)) { return Content(Failed.ToString()) ; }
            string[] orderArr = orderStr.Split(',');
            foreach (string item in orderArr)
            {
                int id = DataConvert.CLng(item.Split(':')[0]);
                int order = DataConvert.CLng(item.Split(':')[1]);
                switch (TbName.ToLower())
                {
                    case "commonmodel":
                        DBCenter.UpdateSQL(TableName, "OrderID=" + order, "GeneralID=" + id);
                        break;
                    case "product":
                        DBCenter.UpdateSQL(TableName, "OrderID=" + order, "ID=" + id);
                        break;
                    case "modelfield":
                        DBCenter.UpdateSQL(TableName, "OrderID=" + order, "FieldID=" + id);
                        break;
                }
            }
            return Content(Success.ToString());
        }
        #endregion
    }
}
