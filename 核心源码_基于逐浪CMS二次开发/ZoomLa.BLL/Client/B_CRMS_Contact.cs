using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using ZoomLa.Model.Client;
using ZoomLa.SQLDAL;
using ZoomLa.SQLDAL.SQL;

namespace ZoomLa.BLL.Client
{
    public class B_CRMS_Contact
    {
        private M_CRMS_Contact initMod = new M_CRMS_Contact();
        public string TbName = "", PK = "";
        public B_CRMS_Contact()
        {
            TbName = initMod.TbName;
            PK = initMod.PK;
        }
        public DataTable Sel()
        {
            return DBCenter.Sel(TbName, "", PK + " DESC");
        }
        public M_CRMS_Contact SelReturnModel(int ID)
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
        public int Insert(M_CRMS_Contact model)
        {
            //return Sql.insert(TbName, model.GetParameters(model), BLLCommon.GetParas(model), BLLCommon.GetFields(model));
            return DBCenter.Insert(model);
        }
        public bool UpdateByID(M_CRMS_Contact model)
        {
            return DBCenter.UpdateByID(model, model.ID);
        }
        public void Del(string ids)
        {
            if (string.IsNullOrEmpty(ids)) { return; }
            SafeSC.CheckIDSEx(ids);
            DBCenter.DelByIDS(TbName, PK, ids);
        }
        public PageSetting SelPage(int cpage, int psize,F_CRMS_Contact filter)
        {
            string where = "1=1 ";
            List<SqlParameter> sp = new List<SqlParameter>();
            if (filter.ClientID != -100) { where += " AND A.ClientID="+filter.ClientID; }
            if (!string.IsNullOrEmpty(filter.Name))
            {
                where += " AND A.Name LIKE @name";
                sp.Add(new SqlParameter("name", "%" + filter.Name + "%"));
            }
            PageSetting setting = PageSetting.Double(cpage, psize, TbName, "ZL_CRMS_Client", "A.ID", "A.ClientID=B.ID", where, PK + " DESC", sp, "A.*,B.ClientName");
            DBCenter.SelPage(setting);
            return setting;
        }
    }
    public class F_CRMS_Contact
    {
        public int ClientID = -100;
        public string Name = "";
    }
}
