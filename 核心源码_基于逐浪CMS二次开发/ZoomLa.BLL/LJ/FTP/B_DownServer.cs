using System;
using System.Data;
using System.Configuration;
using System.Web;
using ZoomLa.Model;
using ZoomLa.Components;
using ZoomLa.Common;
using System.Data.SqlClient;
using ZoomLa.SQLDAL;
using System.Data.Common;
using ZoomLa.SQLDAL.SQL;

namespace ZoomLa.BLL
{


    public class B_DownServer
    {
        private string TbName, PK;
        private M_DownServer initMod = new M_DownServer();
        public B_DownServer() 
        {
            TbName = initMod.TbName;
            PK = initMod.PK;
        }
        public DataTable Sel(int ID)
        {
            return Sql.Sel(TbName, PK, ID);
        }
        public M_DownServer SelReturnModel(int ID)
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
        public DataTable Sel()
        {
            return Sql.Sel(TbName);
        }
        public PageSetting SelPage(int cpage, int psize)
        {
            PageSetting setting = PageSetting.Single(cpage, psize, TbName, PK, "");
            DBCenter.SelPage(setting);
            return setting;
        }
        public bool UpdateByID(M_DownServer model)
        {
            return DBCenter.UpdateByID(model,model.ServerID);
        }
        public bool Del(int ID)
        {
            return Sql.Del(TbName, PK + "=" + ID);
        }
        public void DelByIDS(string ids)
        {
            SafeSC.CheckIDSEx(ids);
            DBCenter.DelByIDS(TbName, PK, ids);
        }
        public int insert(M_DownServer model)
        {
            return DBCenter.Insert(model);
        }
    }
}
