using System;
using System.Data;
using System.Configuration;
using System.Web;
using ZoomLa.Model;
using ZoomLa.Common;
using System.Net.Mail;
using System.Collections.Generic;
using ZoomLa.Components;
using ZoomLa.SQLDAL;
using System.Data.SqlClient;
using System.Data.Common;
using ZoomLa.SQLDAL.SQL;

namespace ZoomLa.BLL
{


    public class B_MailTemp
    {
        public B_MailTemp()
        {
            strTableName = initMod.TbName;
            PK = initMod.PK;
        }
        private string PK, strTableName;
        private M_MailTemp initMod = new M_MailTemp();
        public M_MailTemp SelReturnModel(int ID)
        {
            using (DbDataReader reader = DBCenter.SelReturnReader(strTableName, PK, ID))
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
        private M_MailTemp SelReturnModel(string strWhere)
        {
            using (DbDataReader reader = DBCenter.SelReturnReader(strTableName, strWhere))
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
            return DBCenter.Sel(strTableName);
        }
        public DataTable SelectAll(string name = "")
        {

            string wherestr = "";
            SqlParameter[] sp = new SqlParameter[] { new SqlParameter("@tempname", "%" + name + "%") };
            if (!string.IsNullOrEmpty(name)) { wherestr += " AND TempName LIKE @tempname"; }
            string sql = "SELECT * FROM " + strTableName + " WHERE 1=1" + wherestr + " ORDER BY CreateTime DESC";
            return DBCenter.DB.ExecuteTable(new SqlModel(sql, sp));
        }
        public PageSetting SelPage(int cpage, int psize, string name = "")
        {
            string where = " 1=1";
            List<SqlParameter> sp = new List<SqlParameter>();
            if (!string.IsNullOrEmpty(name)) { where += " AND TempName LIKE @tempname"; sp.Add(new SqlParameter("tempname", "%" + name + "%")); }
            PageSetting setting = PageSetting.Single(cpage, psize, strTableName, PK, where, "CreateTime DESC", sp);
            DBCenter.SelPage(setting);
            return setting;
        }
        public bool UpdateByID(M_MailTemp model)
        {
         return DBCenter.UpdateByID(model,model.ID);
        }
        public bool Del(int ID)
        {
            return DBCenter.Del(strTableName,PK,ID);
        }
        public bool DelByIDS(string ids)
        {
            SafeSC.CheckIDSEx(ids);
            return DBCenter.DelByIDS(strTableName,PK,ids);
        }
        public int insert(M_MailTemp model)
        {
           return DBCenter.Insert(model);
        }
    }

}

