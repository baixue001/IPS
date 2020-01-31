using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using ZoomLa.Model.Common;
using ZoomLa.SQLDAL;
using ZoomLa.SQLDAL.SQL;

namespace ZoomLa.BLL.Common
{
    public class B_Com_File
    {
        private M_Com_File initMod = new M_Com_File();
        public string TbName = "", PK = "";
        public B_Com_File()
        {
            TbName = initMod.TbName;
            PK = initMod.PK;
        }
        public DataTable Sel()
        {
            return DBCenter.Sel(TbName);
        }
        public M_Com_File SelReturnModel(int ID)
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
        public M_Com_File SelModelByGuid(string guid)
        {
            if (string.IsNullOrEmpty(guid)) { return null; }
            List<SqlParameter> sp = new List<SqlParameter>() { new SqlParameter("guid",guid) };
            using (DbDataReader reader = DBCenter.SelReturnReader(TbName,"FileGuid=@guid","",sp))
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
        public int Insert(M_Com_File model)
        {
            return DBCenter.Insert(model);
        }
        public bool UpdateByID(M_Com_File model)
        {
            return DBCenter.UpdateByID(model, model.ID);
        }
        public bool Del(int ID)
        {
            return DBCenter.Del(TbName, PK, ID);
        }
    }
}
