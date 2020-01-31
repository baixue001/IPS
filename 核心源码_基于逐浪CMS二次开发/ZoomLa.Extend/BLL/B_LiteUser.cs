using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using ZoomLa.BLL;
using ZoomLa.Common;
using ZoomLa.SQLDAL;

namespace ZoomLa.Extend
{
    public class B_LiteUser
    {
        string TbName = "ZL_EX_UserView";
        public M_LiteUser SelModelByPasswd(string uname, string passwd)
        {
            if (string.IsNullOrEmpty(uname) || string.IsNullOrEmpty(passwd)) { return null; }
            passwd = StringHelper.MD5(passwd);
            List<SqlParameter> sp = new List<SqlParameter>() {
                new SqlParameter("uname",uname),
                new SqlParameter("passwd",passwd)
            };
            DataTable dt = DBCenter.Sel(TbName, "UserName=@uname AND UserPwd=@passwd", "", sp);
            if (dt.Rows.Count < 1) { return null; }
            M_LiteUser model = new M_LiteUser().GetModelFromReader(dt.Rows[0]);
            return model;
        }
        public M_LiteUser SelModelByOpenId(string openid)
        {
            if (string.IsNullOrEmpty(openid)) { return null; }
            List<SqlParameter> sp = new List<SqlParameter>() {
                new SqlParameter("openid",openid)
            };
            DataTable dt = DBCenter.Sel(TbName, "openid=@openid", "", sp);
            if (dt.Rows.Count < 1) { return null; }
            M_LiteUser model = new M_LiteUser().GetModelFromReader(dt.Rows[0]);
            return model;
        }
        public M_LiteUser SelReturnModel(int uid)
        {
            DataTable dt = DBCenter.Sel(TbName, "UserID=" + uid, "");
            if (dt.Rows.Count < 1) { return null; }
            M_LiteUser model = new M_LiteUser().GetModelFromReader(dt.Rows[0]);
            return model;
        }
        public bool UpdateByID(M_LiteUser model)
        {
           return  DBCenter.UpdateByID(model, model.userId);
        }
        public DataTable Sel()
        {
            return DBCenter.Sel(TbName, "", "UserID DESC");
        }
        public DataTable Sel(Com_Filter filter)
        {
            string where = "1=1";
            List<SqlParameter> sp = new List<SqlParameter>();
            if (!string.IsNullOrEmpty(filter.uids))
            {
                SafeSC.CheckIDSEx(filter.uids);
                where += " AND UserID IN (" + filter.uids + ")";
            }
            if (filter.storeId > 0)
            {
                where += " AND SiteId=" + filter.storeId;
            }
            DataTable dt = DBCenter.Sel("ZL_Ex_UserView", where, "UserID DESC", sp);
            dt.Columns.Remove("userPwd");
            return dt;
        }
    }
}
//CREATE VIEW[dbo].[ZL_EX_UserView]
//AS
//SELECT A.userId,A.userName,A.userPwd, A.groupId,A.regTime,A.honeyName,[PERMISSIONS] as trueName, salt AS userFace,A.CompanyName AS openid,A.[Status],
//A.ParentUserID AS puid,A.siteId,
//B.[address],B.city,B.userSex,B.mobile,B.cardType,B.idcard,B.Bugle AS age
//FROM ZL_User A
//LEFT JOIN ZL_UserBase B
//ON A.UserID=B.UserID