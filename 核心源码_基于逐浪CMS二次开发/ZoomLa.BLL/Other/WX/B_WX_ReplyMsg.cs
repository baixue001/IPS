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
    /// <summary>
    /// 与按钮等关联的返回消息
    /// </summary>
    public class B_WX_ReplyMsg : ZL_Bll_InterFace<M_WX_ReplyMsg>
    {
        public string TbName, PK;
        public M_WX_ReplyMsg initMod = new M_WX_ReplyMsg();
        public B_WX_ReplyMsg()
        {
            TbName = initMod.TbName;
            PK = initMod.PK;
        }
        public int Insert(M_WX_ReplyMsg model)
        {
            if (model.IsDefault == 1) { DBCenter.UpdateSQL(TbName, "IsDefault=0", "Appid=" + model.AppId + " AND ID!=" + model.ID); }
            return DBCenter.Insert(model);
        }
        public bool UpdateByID(M_WX_ReplyMsg model)
        {
            if (model.IsDefault == 1) { DBCenter.UpdateSQL(TbName, "IsDefault=0", "Appid=" + model.AppId + " AND ID!=" + model.ID); }
            return DBCenter.UpdateByID(model,model.ID);
        }
        public bool Del(int ID)
        {
            return Sql.Del(TbName, ID);
        }
        public M_WX_ReplyMsg SelReturnModel(int ID)
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
        /// <summary>
        /// 根据关键词获取信息
        /// </summary>
        public M_WX_ReplyMsg SelByFilter(int appid, string filter)
        {
            filter = filter.Trim();
            SqlParameter[] sp = new SqlParameter[] { new SqlParameter("filter", filter) };
            using (DbDataReader reader = Sql.SelReturnReader(TbName, " WHERE Filter=@filter AND AppId=" + appid, sp))
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
        /// <summary>
        /// 按关键词获取信息,如未匹配,则返回默认信息,否则为空
        /// </summary>
        public M_WX_ReplyMsg SelByFileAndDef(int appid, string filter)
        {
            M_WX_ReplyMsg model = SelByFilter(appid, filter);
            if (model == null)
            {
                using (DbDataReader reader = Sql.SelReturnReader(TbName, " WHERE IsDefault=1 AND AppId=" + appid, null))
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
            return model;
        }
        public DataTable Sel()
        {
            return Sql.Sel(TbName);
        }
        public DataTable SelByAppID(int appid)
        {
            string sql = "SELECT * FROM " + TbName + " WHERE AppId=" + appid;
            return SqlHelper.ExecuteTable(CommandType.Text, sql);
        }
        public void DelByIDS(string ids)
        {
            SafeSC.CheckIDSEx(ids);
            string sql = "DELETE FROM " + TbName + " WHERE ID IN(" + ids + ")";
            SqlHelper.ExecuteSql(sql);
        }
        public PageSetting SelPage(int cpage, int psize, Com_Filter filter)
        {
            string where = "1=1";
            if (filter.storeId > 0)
            {
                where += " AND AppId="+filter.storeId;
            }

            PageSetting setting = PageSetting.Single(cpage, psize, TbName, PK, where);
            DBCenter.SelPage(setting);
            return setting;
        }
    }
}
