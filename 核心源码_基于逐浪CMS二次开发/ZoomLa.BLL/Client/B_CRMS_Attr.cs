using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using ZoomLa.Model.Client;
using ZoomLa.SQLDAL;

namespace ZoomLa.BLL.Client
{
    public class B_CRMS_Attr
    {
        private M_CRMS_Attr initMod = new M_CRMS_Attr();
        public string TbName = "", PK = "";
        public B_CRMS_Attr()
        {
            TbName = initMod.TbName;
            PK = initMod.PK;
        }
        public DataTable Sel()
        {
            return DBCenter.Sel(TbName, "", PK + " DESC");
        }
        public DataTable Sel(string ztype)
        {
            List<SqlParameter> sp = new List<SqlParameter>() { new SqlParameter("ztype", ztype) };
            string where = "ZType=@ztype";
            return DBCenter.Sel(TbName, where, PK + " DESC",sp);
        }
        public M_CRMS_Attr SelReturnModel(int ID)
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
        public int Insert(M_CRMS_Attr model)
        {
            return DBCenter.Insert(model);
        }
        public bool UpdateByID(M_CRMS_Attr model)
        {
            return DBCenter.UpdateByID(model, model.ID);
        }
        public void Del(string ids)
        {
            if (string.IsNullOrEmpty(ids)) { return; }
            SafeSC.CheckIDSEx(ids);
            DBCenter.DelByIDS(TbName, PK, ids);
        }
    }
}
