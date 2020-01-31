using System;
using System.Collections.Generic;
using System.Text;
using System.Data;

using ZoomLa.SQLDAL;
using System.Data.SqlClient;
using ZoomLa.SQLDAL.SQL;
using System.Data.Common;
using ZoomLa.Model;

namespace ZoomLa.BLL.User
{
    public class B_ViewHistory
    {
        private M_ViewHistory initMod = new M_ViewHistory();
        public string TbName = "", PK = "";
        public B_ViewHistory()
        {
            TbName = initMod.TbName;
            PK = initMod.PK;
        }
        public DataTable Sel()
        {
            return DBCenter.Sel(TbName);
        }
        public M_ViewHistory SelReturnModel(int ID)
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
        public M_ViewHistory SelReturnModel(int gid, int uid)
        {
            using (DbDataReader reader = DBCenter.SelReturnReader(TbName, "UserID=" + uid + " AND InfoID=" + gid))
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
        public int Insert(M_ViewHistory model)
        {
            return DBCenter.Insert(model);
        }
        public bool UpdateByID(M_ViewHistory model)
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