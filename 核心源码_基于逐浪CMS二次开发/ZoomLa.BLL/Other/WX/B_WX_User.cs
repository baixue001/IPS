using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using ZoomLa.SQLDAL;
using ZoomLa.Model.Plat;
using System.Data.SqlClient;
using ZoomLa.Model;
using System.Data;
using ZoomLa.SQLDAL.SQL;
using ZoomLa.BLL.User.Addon;
using System.Data.Common;

namespace ZoomLa.BLL
{
    /// <summary>
    /// 已关注我们微信平台的用户信息
    /// 信息通过:ZL_UserApp,wechat与用户绑定
    /// </summary>
    public class B_WX_User : ZL_Bll_InterFace<M_WX_User>
    {
        M_WX_User initMod = new M_WX_User();
        public string TbName, PK;
        public B_WX_User()
        {
            this.TbName = initMod.TbName;
            this.PK = initMod.GetPK();
        }
        public int Insert(M_WX_User model)
        {
            if (string.IsNullOrEmpty(model.OpenID)) { throw new Exception("微信用户的OpenID不能为空"); }
            if (DBCenter.IsExist(TbName, "OpenID=@openid", new List<SqlParameter>() { new SqlParameter("openid", model.OpenID) }))
            {
                return 0;
            }
            return DBCenter.Insert(model);
        }
        public bool UpdateByID(M_WX_User model)
        {
            return DBCenter.UpdateByID(model,model.ID);
        }
        public bool Del(int ID)
        {
            return Sql.Del(TbName, ID);
        }
        /// <summary>
        /// 取消订阅,移除微信用户信息,移除绑定信息
        /// </summary>
        public bool DelByOpenid(string openid)
        {
            List<SqlParameter> sp = new List<SqlParameter> { new SqlParameter("@openid", openid) };
            DBCenter.DelByWhere(TbName, "OpenID=@openid", sp);
            new B_UserAPP().DelByOpenID(openid);
            return true;
        }
        public M_WX_User SelReturnModel(int ID)
        {
            using (DbDataReader reader = Sql.SelReturnReader(TbName, PK, ID))
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
        public M_WX_User SelForOpenid(string openid)
        {
            string sql = "SELECT * FROM " + TbName + " WHERE OpenID=@openid";
            SqlParameter[] sp = new SqlParameter[] { new SqlParameter("@openid", openid) };
            using (DbDataReader reader = SqlHelper.ExecuteReader(CommandType.Text, sql, sp))
            {
                if (reader.Read())
                    return initMod.GetModelFromReader(reader);
                else
                    return null;
            }
        }
        public DataTable Sel()
        {
            return Sql.Sel(TbName, "", "CDate Desc");
        }
        //public DataTable SelByAppId(int appid, string name = "")
        //{
        //    return SelPage(1, int.MaxValue, new Com_Filter()
        //    {
        //        storeId = appid,
        //        uname = name
        //    }).dt;
        //}
        public PageSetting SelPage(int cpage, int psize, Com_Filter filter)
        {
            List<SqlParameter> sp = new List<SqlParameter>();
            string where = "1=1";
            string stable = "(SELECT A.UserID,A.OpenID,B.UserName FROM ZL_UserApp A LEFT JOIN ZL_User B ON A.UserID=B.UserID WHERE A.SourcePlat='wechat')";
            if (filter.storeId > 0)
            {
                where += " AND A.AppID=" + filter.storeId;
            }
            if (!string.IsNullOrEmpty(filter.uname))
            {
                where += " AND A.Name LIKE @name"; sp.Add(new SqlParameter("name", "%" + filter.uname + "%"));
            }

            PageSetting setting = PageSetting.Double(cpage, psize, "ZL_WX_User", stable,"A.ID", "A.OpenID=B.OpenID",where,"ID DESC",sp, "A.*,B.UserID,B.UserName");
            DBCenter.SelPage(setting);
            return setting;
        }
    }
}
