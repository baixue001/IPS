using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using ZoomLa.SQLDAL;
using System.Data.SqlClient;
using ZoomLa.Model.PlanSql;
using ZoomLa.SQLDAL.SQL;
using System.Data.Common;

namespace ZoomLa.BLL.PlanSql
{
    public class B_PlanSql
    {
        public string strTableName,PK;
        private M_PlanSql initMod = new M_PlanSql();
        public B_PlanSql()
        {
            strTableName = initMod.TbName;
            PK = initMod.PK;
        }
        /// <summary>
        /// 根据ID查询一条记录
        /// </summary>
        public DataTable Sel(int ID)
        {
            return Sql.Sel(strTableName, PK, ID);
        }
        public DataTable SelByPlanID(int planid)
        {
            string sql = "SELECT * FROM " + strTableName + " WHERE PlanID="+planid+" AND statu>0";
            return SqlHelper.ExecuteTable(sql);
        }

        /// <summary>
        /// 根据ID查询一条记录
        /// </summary>
        public M_PlanSql SelReturnModel(int ID)
        {
            using (DbDataReader reader = Sql.SelReturnReader(strTableName, PK, ID))
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
        /// 查询所有记录
        /// </summary>
        public DataTable Sel()
        {
            return Sql.Sel(strTableName);
        }
        public PageSetting SelPage(int cpage, int psize)
        {
            PageSetting setting = PageSetting.Single(cpage, psize, strTableName, PK, "");
            DBCenter.SelPage(setting);
            return setting;
        }
        /// <summary>
        /// 根据ID更新
        /// </summary>
        public bool UpdateByID(M_PlanSql model)
        {
            return DBCenter.UpdateByID(model, model.ID);
        }


        public bool Del(int ID)
        {
            return Sql.Del(strTableName, ID);
        }

        public int insert(M_PlanSql model)
        {
            return DBCenter.Insert(model);
        }
    }
}