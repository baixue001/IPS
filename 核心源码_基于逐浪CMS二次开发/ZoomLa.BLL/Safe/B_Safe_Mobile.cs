using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using ZoomLa.SQLDAL;
using ZoomLa.Model.Plat;
using System.Data.SqlClient;
using ZoomLa.Model;
using ZoomLa.Components;
using ZoomLa.BLL.Helper;
using ZoomLa.SQLDAL.SQL;
using System.Data.Common;
using ZoomLa.Model.Other;
using Microsoft.AspNetCore.Http;

namespace ZoomLa.BLL
{
    /// <summary>
    /// 手机短信验证类
    /// </summary>
    public class B_Safe_Mobile : ZL_Bll_InterFace<M_Safe_Mobile>
    {
        public string TbName, PK;
        public M_Safe_Mobile initMod = new M_Safe_Mobile();
        public B_Safe_Mobile()
        {
            TbName = initMod.TbName;
            PK = initMod.PK;
        }
        public bool Del(int ID)
        {
            return DBCenter.Del(TbName,PK, ID);
        }
        public int Insert(M_Safe_Mobile model)
        {
            //if (string.IsNullOrEmpty(model.IP)) { model.IP = IPScaner.GetUserIP(); }
            return DBCenter.Insert(model);
        }
        public DataTable Sel()
        {
            return DBCenter.Sel(TbName, "", "CDate DESC");
        }
        public PageSetting SelPage(int cpage, int psize)
        {
            PageSetting setting = PageSetting.Single(cpage, psize, TbName, PK, "", "CDate DESC");
            DBCenter.SelPage(setting);
            return setting;
        }
        public M_Safe_Mobile SelReturnModel(int ID)
        {
            using (DbDataReader reader = DBCenter.SelReturnReader(TbName, PK, ID))
            {
                if (reader.Read())
                {
                    return initMod.GetModelFromReader(reader);
                }
                else
                {
                    return null;
                }
            }
        }
        /// <summary>
        /// 根据手机号,查找最后一次发送的验证码记录
        /// </summary>
        public M_Safe_Mobile SelLastModel(string mobile, string source = "")
        {
            if (string.IsNullOrEmpty(mobile)) { return null; }
            List<SqlParameter> sp = new List<SqlParameter>() { new SqlParameter("mobile", mobile.Trim()), new SqlParameter("source", source) };
            string where = "Phone=@mobile";
            if (!string.IsNullOrEmpty(source)) { where += " AND Source=@source"; }
            using (DbDataReader reader = DBCenter.SelReturnReader(TbName, where, "ID DESC", sp))
            {
                if (reader.Read())
                {
                    return initMod.GetModelFromReader(reader);
                }
                else
                {
                    return null;
                }
            }
        }
        public bool UpdateByID(M_Safe_Mobile model)
        {
            return DBCenter.UpdateByID(model, model.ID);
        }
        //----------------------------检测块
        /// <summary>
        /// 发送手机短信检测(为0则不限定)
        /// </summary>
        /// <param name="phone">手机号</param>
        /// <param name="ip">用户ip</param>
        /// <param name="maxphonenum">同一个手机号最大访问次数</param>
        /// <param name="maxipnum">同一个ip最大访问次数</param>
        /// <returns></returns>
        public bool CheckMobile(string phone, string ip)
        {
            SqlParameter[] sp = new SqlParameter[] { new SqlParameter("@phone", phone), new SqlParameter("@ip", ip) };
            string sql = "SELECT COUNT(*) AS NUM FROM " + TbName + " WHERE DATEDIFF(DAY,CDate,'" + DateTime.Now + "')=0 ";
            string wherestr = " AND Phone=@phone ";
            int phonenum = DataConvert.CLng(SqlHelper.ExecuteTable(sql + wherestr, sp).Rows[0]["NUM"]);
            if (SiteConfig.SiteOption.MaxMobileMsg != 0 && phonenum >= SiteConfig.SiteOption.MaxMobileMsg) { return false; }
            wherestr = " AND IP=@ip";
            int ipnum = DataConvert.CLng(SqlHelper.ExecuteTable(sql + wherestr, sp).Rows[0]["NUM"]);
            if (SiteConfig.SiteOption.MaxIpMsg != 0 && ipnum >= SiteConfig.SiteOption.MaxIpMsg) { return false; }
            return true;
        }
        public bool CheckMobile(HttpContext ctx, string phone) { return CheckMobile(phone, IPScaner.GetUserIP(ctx)); }
        /// <summary>
        /// 检测手机验证码,10分钟内有效|存数据库|可换浏览器验证|未验证成功不会取消|必须手机与验证码同时匹配
        /// </summary>
        public static CommonReturn CheckVaildCode(string mobile, string code, string source)
        {
            B_Safe_Mobile mobBll = new B_Safe_Mobile();
            M_Safe_Mobile model = mobBll.SelLastModel(mobile);
            if (string.IsNullOrEmpty(mobile)) { return CommonReturn.Failed("手机号码不能为空"); }
            if (string.IsNullOrEmpty(code)) { return CommonReturn.Failed("验证码不能为空"); }
            if (model == null) { return CommonReturn.Failed("验证码信息不存在"); }
            if (model.ZStatus != 0) { return CommonReturn.Failed("验证码无效"); }
            if ((DateTime.Now - model.CDate > TimeSpan.FromMinutes(10))) { return CommonReturn.Failed("验证码过期"); }
            if (!model.VCode.Equals(code)) { return CommonReturn.Failed("手机验证码不匹配"); }
            DBCenter.UpdateSQL(model.TbName, "ZStatus=99", "ID=" + model.ID);
            return CommonReturn.Success();
        }
    }
}
