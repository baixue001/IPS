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

namespace ZoomLa.BLL.Other
{
    public class B_User_Bank
    {
        private M_User_Bank initMod = new M_User_Bank();
        private string TbName, PK;
        public B_User_Bank()
        {
            TbName = initMod.TbName;
            PK = initMod.PK;
        }
        public bool Del(int ID)
        {
            return DBCenter.Del(TbName, PK, ID);
        }

        public void DelByIDS(string ids)
        {
            SafeSC.CheckIDSEx(ids);
            DBCenter.DelByIDS(TbName, PK, ids);
        }
        public int Insert(M_User_Bank model)
        {
            return DBCenter.Insert(model);
        }
        public DataTable Sel(int userid = 0, string stype = "", string skey = "")
        {
            string where = "1=1";
            List<SqlParameter> sp = new List<SqlParameter>();
            if (userid > 0) { where += " AND A.UserID=" + userid; }
            if (!string.IsNullOrEmpty(stype)&&!string.IsNullOrEmpty(skey))
            {
                sp.Add(new SqlParameter("skey", "%" + skey + "%"));
                switch (stype)
                {
                    case "uname":
                        where += " AND A.PeopleName LIKE @skey";
                        break;
                    case "bname":
                        where += " AND A.BankName LIKE @skey";
                        break;
                    case "bnum":
                        where += " AND A.CardNum LIKE @skey";
                        break;
                }
            }
            return DBCenter.JoinQuery("A.*,B.UserName",TbName,"ZL_User","A.UserID=B.UserID",where,"AddTime DESC",sp.ToArray());
        }
        public PageSetting SelPage(int cpage, int psize, int userid = 0)
        {
            string where = "1=1";
            if (userid > 0) { where += " AND UserID=" + userid; }
            PageSetting setting = PageSetting.Single(cpage, psize, TbName, PK, where);
            DBCenter.SelPage(setting);
            return setting;
        }
        public M_User_Bank SelReturnModel(int ID)
        {
            if (ID < 1) { return null; }
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
        public M_User_Bank SelModelByUid(int uid, string bankName = "")
        {
            string where = "UserID=" + uid;
            List<SqlParameter> sp = new List<SqlParameter>();
            if (!string.IsNullOrEmpty(bankName))
            {
                sp.Add(new SqlParameter("bankName", bankName.Trim()));
                where += " AND BankName=@bankName";
            }
            using (DbDataReader reader = DBCenter.SelReturnReader(TbName,where,"ID DESC",sp))
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
        public bool UpdateByID(M_User_Bank model)
        {
            return DBCenter.UpdateByID(model, model.ID);
        }
    }
}
