using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ZoomLa.BLL;
using ZoomLa.BLL.API;
using ZoomLa.SQLDAL;
using ZoomLa.SQLDAL.SQL;
using ZoomLaCMS.Ctrl;
using ZoomLa.Common;
using ZoomLa.BLL.Helper;
using System.IO;
using ZoomLa.Safe;
using ZoomLa.Components.Comp;

namespace ZoomLaCMS.Areas.Admin.Controllers.Extend
{
    [Area("Admin")]
    [Authorize(Policy = "Admin")]
    [Route("[area]/Extend/[controller]/[action]")]
    public class DBToolController : Ctrl_Admin
    {
        public string Spwd { get { return HttpContext.Session.GetString("SPWD"); } set { HttpContext.Session.SetString("SPWD", value); } }
        public string TbName { get { return RequestEx["tbname"]; } }
        B_Admin badmin = new B_Admin();
        public IActionResult Default()
        {
            if (!adminMod.IsSuperAdmin()) { return WriteErr("无权访问该功能"); }
            if (Request.IsAjax())
            {
                M_APIResult retMod = new M_APIResult(M_APIResult.Failed);
                string action = GetParam("action");
                string tbname = GetParam("tbname").Trim();
                string fdname = GetParam("fdname").Trim();
                try
                {
                    switch (action)
                    {
                        case "field_update":
                            {
                                int len = DataConvert.CLng(GetParam("len"));
                                string type = GetParam("type").Replace(" ", "").ToLower();
                                if (string.IsNullOrEmpty(type) || len < 1) { retMod.retmsg = "字段类型或数值不正确"; }
                                else
                                {
                                    string sql = "ALTER TABLE [" + tbname + "] ALTER Column [" + fdname + "][" + type + "]";
                                    string[] ignore = new string[] { "int", "money", "decimal", "float", "datetime", "date", "timestamp" };
                                    if (!ignore.Contains(type)) { sql += "(" + len + ")"; }
                                    DBCenter.DB.ExecuteNonQuery(new SqlModel(sql, null));
                                    retMod.retcode = M_APIResult.Success;
                                }
                            }
                            break;
                        default:
                            {
                                retMod.retmsg = "[" + action + "]未命中";
                            }
                            break;
                    }
                }
                catch (Exception ex) { retMod.retmsg = ex.Message; }
                return Content(retMod.ToString());
            }
            else { return View(); }
        }
        [HttpPost]
        public IActionResult Default_Result()
        {
            string SQL = EncryptHelper.Base64Decrypt(Request.Form["sql"]);
            DataTable dt = null;
            try
            {
                if (!string.IsNullOrEmpty(SQL)) { dt = DBCenter.ExecuteTable(SQL); }
                else { throw new Exception("未指定需要查询的参数"); }
            }
            catch (Exception ex)
            {
                dt = null;
                ViewBag.err = ex.Message;
            }
            return PartialView();
        }
        [HttpPost]
        public IActionResult Default_Field()
        {
            DataTable dt = new DataTable();
            if (!string.IsNullOrEmpty(TbName)) { SafeSC.CheckDataEx(TbName); dt = DBHelper.Table_FieldList(TbName); }
            return PartialView(dt);
        }
        public IActionResult ViewList()
        {
            DataTable dt = DBCenter.DB.View_List();
            return View(dt);
        }
        private static ZipConfig cfg = new ZipConfig();
        //数据与全站备份文件存放位置
        private string backupPath = "/temp/";
        private string dbPath { get { return backupPath + "database/"; } }
        private string sitePath { get { return backupPath + "site/"; } }
        public IActionResult DBBackUP()
        {
            if (adminMod.AdminId < 1) { return WriteErr("仅超管可访问该页面"); }
            if (!Directory.Exists(function.VToP(dbPath))) { SafeSC.CreateDir(dbPath); }
            if (Request.IsAjax())
            {
                //返回进度
                return Content(cfg.Result.P_Percent);
            }
            DataTable dt = FileSystemObject.GetDirectoryInfos(function.VToP(dbPath), FsoMethod.File);
            if (dt != null && dt.Rows.Count > 0)
            {
                dt.DefaultView.Sort = "createTime DESC";
            }
            ViewBag.dt = dt;
            return View();
        }
        public IActionResult DB_Backup()
        {
            //string dbPath = !this.chBackup.Checked ? backPath : AppDomain.CurrentDomain.BaseDirectory;
            string dbname = DBHelper.GetAttrByStr(DBCenter.DB.ConnectionString, "Initial Catalog");
            string DatabasePath = GetParam("DatabasePath").Split('.')[0];
            if (SafeC.FileNameCheck(DatabasePath)) { return WriteErr("文件名不规范"); }
            string savePath = "";
            if (DBHelper.IsLocalDB(HttpContext.Connection.LocalIpAddress.ToString(), System.Environment.MachineName))
            {
                savePath =function.VToP(dbPath + GetParam("DatabasePath") + ".bak");
            }
            else
            {
                savePath = "D:\\backup\\" + DatabasePath + ".bak";
            }
            string sql = "backup database [" + dbname + "] to  disk='" + savePath + "'  with  init ";
            if (!Directory.Exists(function.VToP(dbPath))){SafeSC.CreateDir(dbPath);}
            SqlHelper.ExecuteSql(sql);
            return WriteOK("数据库备份成功", "DBBackUP");
        }
        public IActionResult DB_ReStore()
        {
            if (adminMod.AdminId < 1) { return WriteErr("仅超管可访问该页面"); }
            string dbname = DBHelper.GetAttrByStr(DBCenter.DB.ConnectionString, "Initial Catalog");
            string bakPath = function.VToP(dbPath + GetParam("fname") + ".bak");
            string sql = "use master restore database " + dbname + " from disk='" + bakPath + "' with replace";
            try
            {
                SqlHelper.ExecuteSql(sql);
                return WriteOK("数据库还原成功");
            }
            catch (Exception ex)
            {
                return WriteErr(ex.Message);
            }
        }
        public IActionResult SiteBackUP()
        {
            if (adminMod.AdminId < 1) { return WriteErr("仅超管可访问该页面"); }
            if (!Directory.Exists(dbPath)) SafeSC.CreateDir(dbPath);
            return View();
        }
        public IActionResult Site_Backup()
        {
            //string rarName = SiteText.Value.Trim();
            //if (string.IsNullOrEmpty(rarName) || rarName.Contains(".")) function.WriteErrMsg("不能为空,或包含特殊字符");
            ////ZipConfig cfg = ZipHelper.ZipTaskList.FirstOrDefault(p => p.Flow.Equals("BackupAllSite"));
            ////if (cfg != null) { ZipHelper.ZipTaskList.Remove(cfg); }
            //cfg = new ZipConfig()
            //{
            //    ZipSrc = "/",
            //    ZipSave = "/temp/" + rarName + ".zip",
            //    IsAsync = true,
            //    //Flow = "BackupAllSite"
            //};
            //if (!Zip_UploadFiles.Checked)
            //{
            //    cfg.IgnoreDirs.Add("/UploadFiles/");
            //    cfg.IgnoreDirs.Add(SiteConfig.SiteOption.UploadDir);
            //}
            //ZipHelper.Zip(cfg);
            //function.Script(this, "beginCheck('getProgress');");
            return View();
        }
    }
}