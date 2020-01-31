using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using ZoomLa.Model;

namespace ZoomLa.BLL
{
    public class ZLLog
    {
        public static string constr = "";
        static ZLLog() { }
        public static void L(string msg)
        {
            L(ZLEnum.Log.exception, msg);
        }
        public static void L(Model.ZLEnum.Log type, string msg)
        {
            M_Log model = new M_Log() { Message = msg };
            L(type, model);
        }
        /// <summary>
        /// 写入至文本或XML
        /// </summary>
        public static void L(Model.ZLEnum.Log type, M_Log model)
        {
            EmptyDeal(type, model);
            //ILog logs = LogManager.GetLogger(type.ToString());
            //logs.Info(model.ToString());
            ZoomLa.Components.CMSLog.L(type.ToString(), model.ToString());
        }
       
        private static M_Log EmptyDeal(Model.ZLEnum.Log type, M_Log model)
        {
            if (string.IsNullOrEmpty(model.Action)) { model.Action = type.ToString(); }
            if (model.CDate <= DateTime.MinValue) { model.CDate = DateTime.Now; }
            try
            {
                //if (string.IsNullOrEmpty(model.IP)) { model.IP = IPScaner.GetUserIP(); }
                //if (string.IsNullOrEmpty(model.Source)) { model.Source = HttpContext.Current.Request.RawUrl; }
            }
            catch { }
            if (type == Model.ZLEnum.Log.fileup || type == Model.ZLEnum.Log.safe)
            {
                //try
                //{
                //    if (string.IsNullOrEmpty(model.UName)) { M_UserInfo mu = new B_User().GetLogin(); if (mu != null && mu.UserID != 0) { model.UName = mu.UserName; } }
                //    if (string.IsNullOrEmpty(model.AName)) { M_AdminInfo adminMod = B_Admin.GetLogin(); if (adminMod != null && adminMod.AdminId > 0) { model.AName = adminMod.AdminName; } }
                //}
                //catch { }
            }
            return model;
        }
        
        /*---------------------------------------------*/
        //内容管理日志
        public DataTable SelLog(Model.ZLEnum.Log type)
        {
            //string dbfile = GetDBFile(type);
            //try
            //{
            //    return SQLiteHelper.ExecuteTable(dbfile, "SELECT * FROM ZL_ConLog");
            //}
            //catch { return null; }
            switch (type)
            {
                case Model.ZLEnum.Log.alogin:
                    //return SqlHelper.ExecuteTable(CommandType.Text,"SELECT * FROM ZL_Log4");
                    //return SqlCEHelper.ExecuteTable(constr, "SELECT * FROM ZL_Log4 ORDER BY CDate DESC");
                    return null;
                case Model.ZLEnum.Log.content:
                    return null;
                default:
                    return null;
            }
        }
    }
}
