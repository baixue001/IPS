using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using ZoomLa.Model;

using ZoomLa.SQLDAL;
using ZoomLa.SQLDAL.SQL;

namespace ZoomLa.BLL.User.Addon
{
    public class B_UserAPP
    {
        private string PK, TbName;
        private M_UserAPP initMod = new M_UserAPP();
        public B_UserAPP() 
        {
            PK = initMod.PK;
            TbName = initMod.TbName;
        }
        public int Insert(M_UserAPP model)
        {
            return DBCenter.Insert(model);
        }
        public bool UpdateByID(M_UserAPP model)
        {
            if (string.IsNullOrEmpty(model.SourcePlat)) { throw new Exception("未指定平台"); }
            return DBCenter.UpdateByID(model,model.ID);
        }
        public bool Del(int ID)
        {
            return DBCenter.DelByIDS(TbName,PK,ID.ToString());
        }
        public void DelByUid(int uid, string plat)
        {
            List<SqlParameter> sp = new List<SqlParameter>() { new SqlParameter("plat", plat.Trim()) };
            DBCenter.DelByWhere(TbName, "UserID=" + uid + " AND SourcePlat=@plat", sp);
        }
        public void DelByOpenID(string openid)
        {
            List<SqlParameter> sp = new List<SqlParameter>() { new SqlParameter("openid", openid.Trim()) };
            DBCenter.DelByWhere(TbName, "OpenID=@openid", sp);
        }
        public M_UserAPP SelReturnModel(int ID)
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
        public M_UserAPP SelModelByUid(int uid, string source, string appid = "")
        {
            SqlParameter[] sp = new SqlParameter[] { new SqlParameter("source", source),new SqlParameter("appid",appid) };
            string where = " UserID=" + uid + " AND SourcePlat=@source ";
            if (!string.IsNullOrEmpty(appid)) {where+=" AND AppID=@appid"; }
            using (DbDataReader reader = DBCenter.SelReturnReader(TbName, where, sp))
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
        public M_UserAPP SelModelByOpenID(string openid)
        {
            return SelModelByOpenID(openid,"");
        }
        public M_UserAPP SelModelByOpenID(string openid, string source)
        {
            SqlParameter[] sp = new SqlParameter[] { new SqlParameter("source", source), new SqlParameter("openid", openid) };
            string where = " OpenID=@openid ";
            if (!string.IsNullOrEmpty(source))
            {
                where += " AND SourcePlat=@source";
            }
            using (DbDataReader reader = DBCenter.SelReturnReader(TbName, where, sp))
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
        public DataTable Sel()
        {
            return DBCenter.Sel(TbName);
        }
        public PageSetting SelPage(int cpage, int psize)
        {
            PageSetting setting = PageSetting.Single(cpage, psize,TbName, PK, "");
            DBCenter.SelPage(setting);
            return setting;
        }
        /// <summary>
        /// 更新|新增openid与用户
        /// </summary>
        public M_UserAPP LinkToUser(string openid, string source, int uid)
        {
            M_UserAPP appMod = SelModelByUid(uid, source);
            if (appMod == null)
            {
                appMod = new M_UserAPP();
                appMod.APPID = "";
                appMod.OpenID = openid;
                appMod.UserID = uid;
                appMod.ID = Insert(appMod);
            }
            else { appMod.OpenID = openid; UpdateByID(appMod); }
            return appMod;
        }
    }
}
