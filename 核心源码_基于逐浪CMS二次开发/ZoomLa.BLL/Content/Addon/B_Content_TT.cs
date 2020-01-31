using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using ZoomLa.Model.Content.Addon;
using ZoomLa.SQLDAL;

namespace ZoomLa.BLL.Content.Addon
{
    public class B_Content_TT
    {
        private M_Content_TT initMod = new M_Content_TT();
        private string TbName, PK;
        public B_Content_TT()
        {
            TbName = initMod.TbName;
            PK = initMod.PK;
        }
        public bool Del(int ID)
        {
            return DBCenter.Del(TbName, PK, ID);
        }
        public int Insert(M_Content_TT model)
        {
            return DBCenter.Insert(model);
        }
        public DataTable Sel(int issave = -100, int status = -100, string skey = "")
        {
            string where = "1=1";
            List<SqlParameter> sp = new List<SqlParameter>();
            if (issave != -100) { where += " AND IsSave=" + issave; }
            if (status != -100) { where += " AND Status=" + status; }
            if (!string.IsNullOrEmpty(skey)) { where += " AND Title LIKE @skey"; sp.Add(new SqlParameter("skey", "%" + skey + "%")); }
            return DBCenter.Sel(TbName, where, "CreateDate DESC", sp);
        }
        public M_Content_TT SelReturnModel(int ID)
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
        public bool UpdateByID(M_Content_TT model)
        {
            return DBCenter.UpdateByID(model, model.ID);
        }
        public bool DelByIDS(string ids)
        {
            SafeSC.CheckIDSEx(ids);
            return DBCenter.DelByIDS(TbName, PK, ids);
        }
        public string GetContentType(string type, int newstype = 0)
        {
            if (string.IsNullOrEmpty(type)) { return ""; }
            string[] types = newstype == 1 ? M_Content_TT.VideoTypeStr.Split('|') : M_Content_TT.NewsTypeStr.Split('|');
            foreach (string str in types)
            {
                if (type.Equals(str.Split(',')[0])) { return str.Split(',')[1]; }
            }
            return "";
        }
    }
}
