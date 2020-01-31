using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using ZoomLa.SQLDAL;
using ZoomLa.Model.Plat;
using System.Data.SqlClient;
using ZoomLa.Model;
using ZoomLa.SQLDAL.SQL;
using System.Data.Common;

namespace ZoomLa.BLL
{
    public class B_WX_APPID : ZL_Bll_InterFace<M_WX_APPID>
    {
        private string TbName, PK;
        private M_WX_APPID initMod = new M_WX_APPID();
        public B_WX_APPID()
        {
            TbName = initMod.TbName;
            PK = initMod.PK;
        }
        public int Insert(M_WX_APPID model)
        {
            return DBCenter.Insert(model);
        }

        public bool UpdateByID(M_WX_APPID model)
        {
            return DBCenter.UpdateByID(model,model.ID);
        }
        public bool Del(int ID)
        {
            return Sql.Del(TbName, ID);
        }
        public bool DelByIds(string ids)
        {
            SafeSC.CheckIDSEx(ids);
            string sql = "DELETE FROM " + TbName + " WHERE ID IN (" + ids + ")";
            return SqlHelper.ExecuteSql(sql);
        }
        public M_WX_APPID SelReturnModel(int ID)
        {
            if (ID < 1) { return null; }
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
        public M_WX_APPID GetAppByWxNo(string wxno)
        {
            string sql = "SELECT * FROM " + TbName + " WHERE (WxNo=@wxno OR OrginID=@wxno)";
            SqlParameter[] sp = new SqlParameter[] { new SqlParameter("@wxno", wxno) };
            using (DbDataReader rdr = SqlHelper.ExecuteReader(CommandType.Text, sql, sp))
            {
                if (rdr.Read())
                {
                    return initMod.GetModelFromReader(rdr);
                }
                else { return null; }
            }
        }
        public M_WX_APPID GetDefault()
        {
            string sql = "SELECT TOP 1 * FROM " + TbName + " ORDER BY IsDefault DESC,ID ASC";
            using (DbDataReader rdr = SqlHelper.ExecuteReader(CommandType.Text, sql))
            {
                if (rdr.Read())
                {
                    return initMod.GetModelFromReader(rdr);
                }
                else { return null; }
            }
        }
        public bool SetDefault(int id)
        {
            string cleansql = "UPDATE " + TbName + " SET IsDefault=0";
            string sql = "UPDATE " + TbName + " SET IsDefault=1 WHERE ID=" + id;
            SqlHelper.ExecuteSql(cleansql);
            return SqlHelper.ExecuteSql(sql);
        }
        public DataTable Sel()
        {
            return Sql.Sel(TbName, "", "CDate Desc");
        }
        public PageSetting SelPage(int cpage, int psize)
        {
            PageSetting setting = PageSetting.Single(cpage, psize, TbName, PK, "", "CDate Desc");
            DBCenter.SelPage(setting);
            return setting;
        }
    }
}
