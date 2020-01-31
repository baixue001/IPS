using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ZoomLa.Model;


namespace ZoomLa.BLL
{
    public class ZLHelper
    {
        /// <summary>
        /// 获取指定用户的上传路径(不适用于百度编辑器)
        /// (/UploadFiles/model/name+id/date/)
        /// </summary>
        /// <param name="model">oa,oa_office,oa_office_word 等命名</param>
        /// <param name="func">功能名称,如果一个模块下有个需要上传的功能,则再加一级用此划分</param>
        public static string GetUploadDir_User(M_UserInfo mu, string model, string func = "", string dateFormat = "yyyyMMdd")
        {
            string dir = "/UploadFiles/";
            if (!string.IsNullOrEmpty(model)) { dir += model + "/"; }
            if (mu == null || mu.IsNull)//匿名用户访问
            {
                dir += "anony/";
            }
            else
            {
                dir += "user/" + mu.UserName + mu.UserID + "/";
            }
            if (!string.IsNullOrEmpty(dateFormat)) { dir += DateTime.Now.ToString(dateFormat) + "/"; }
            if (!string.IsNullOrEmpty(func)) { dir += func.Trim('/') + "/"; }
            return dir;
        }
        /// <summary>
        /// 获取管理员上传目录
        /// (/UploadFiles/model/admin/name+id/date/)
        /// </summary>
        public static string GetUploadDir_Admin(M_AdminInfo adminMod, string model, string func = "", string dateFormat = "yyyyMMdd")
        {
            string dir = "/UploadFiles/";
            if (!string.IsNullOrEmpty(model)) { dir += model + "/"; }
            dir += "admin/";
            dir += adminMod.AdminName + adminMod.AdminId + "/";
            if (!string.IsNullOrEmpty(dateFormat)) { dir += DateTime.Now.ToString(dateFormat) + "/"; }
            if (!string.IsNullOrEmpty(func)) { dir += func.Trim('/') + "/"; }
            return dir;
        }
        /// <summary>
        /// 系统文件目录,与用户无关,默认无日期(声音定位等模块需要)
        /// </summary>
        public static string GetUploadDir_System(string model, string func = "", string dateFormat = "")
        {
            if (string.IsNullOrEmpty(model)) { throw new Exception("模块名称不能为空"); }
            string dir = "/UploadFiles/System/" + model + "/";
            if (!string.IsNullOrEmpty(func)) { dir += func.Trim('/') + "/"; }
            if (!string.IsNullOrEmpty(dateFormat)) { dir += DateTime.Now.ToString(dateFormat) + "/"; }
            return dir;
        }
        /// <summary>
        /// 未登录用户上传的文件
        /// </summary>
        public static string GetUploadDir_Anony(string model, string func = "", string dateFormat = "")
        {
            if (string.IsNullOrEmpty(model)) { throw new Exception("模块名称不能为空"); }
            string dir = "/UploadFiles/Anony/" + model + "/";
            if (!string.IsNullOrEmpty(func)) { dir += func.Trim('/') + "/"; }
            if (!string.IsNullOrEmpty(dateFormat)) { dir += DateTime.Now.ToString(dateFormat) + "/"; }
            return dir;
        }
    }
}
