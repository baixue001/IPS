using System;
using System.Data;
using ZoomLa.Model;
using ZoomLa.Common;
using ZoomLa.SQLDAL;
using System.Data.SqlClient;
using System.Data.Common;
namespace ZoomLa.BLL
{
    using SQLDAL.SQL;
    /*
     * 支付币种,用于支付单
     */


    public class B_MoneyManage
    {
        private string TbName, PK;
        private M_MoneyManage initMod = new M_MoneyManage();
        public B_MoneyManage()
        {
            TbName = initMod.TbName;
            PK = initMod.PK;
        }
        public M_MoneyManage SelReturnModel(int ID)
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
        //返回默认汇率
        public M_MoneyManage SelDefModel()
        {
            using (DbDataReader reader = Sql.SelReturnReader(TbName, " WHERE is_flag=1"))
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
            return Sql.Sel(TbName,"","is_flag DESC");
        }
        /// <summary>
        /// 排序
        /// </summary>
        public DataTable Sel(string strWhere, string strOrderby)
        {
            return Sql.Sel(TbName, strWhere, strOrderby);
        }
        public PageSetting SelPage(int cpage, int psize)
        {
            PageSetting setting = PageSetting.Single(cpage, psize, TbName, PK, "");
            DBCenter.SelPage(setting);
            return setting;
        }
        public bool UpdateByID(M_MoneyManage model)
        {
            return DBCenter.UpdateByID(model, model.Flow);
        }
        /// <summary>
        /// 取消所有默认
        /// </summary>
        public void CancelDef()
        {
            string sql = "UPDATE " + TbName + " SET is_flag=0";
            SqlHelper.ExecuteSql(sql);
        }
        public bool GetDelete(int ID)
        {
            return DBCenter.Del(TbName,PK,ID);
        }
        public int insert(M_MoneyManage model)
        {
            return DBCenter.Insert(model);
        }
        public bool DeleteByIds(string ids)
        {
            SafeSC.CheckIDSEx(ids);
            return SqlHelper.ExecuteSql("Delete From " + TbName + " Where Flow IN (" + ids + ")");
        }
    }
}