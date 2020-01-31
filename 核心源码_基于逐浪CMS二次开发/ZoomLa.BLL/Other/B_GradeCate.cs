using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Text;
using ZoomLa.Model;
using ZoomLa.SQLDAL;
using ZoomLa.SQLDAL.SQL;

namespace ZoomLa.BLL.Other
{
    public class B_GradeCate
    {
        private string strTableName, PK;
        private M_GradeCate initMod = new M_GradeCate();
        public B_GradeCate()
        {
            strTableName = initMod.TbName;
            PK = initMod.PK;
        }
        public M_GradeCate SelReturnModel(int ID)
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
        /// 添加分级选项分类
        /// </summary>
        /// <param name="Cate">分类实例</param>
        /// <returns>成功状态</returns>
        public bool AddCate(M_GradeCate model)
        {
            if (model.CateID > 0)
               UpdateByID(model);
            else
                insert(model);
            return true;
        }
        /// <summary>
        /// 更新分级选项分类
        /// </summary>
        /// <param name="Cate">分类实例</param>
        /// <returns>成功状态</returns>
        public bool UpdateCate(M_GradeCate model)
        {
            B_GradeOption bll = new B_GradeOption();
            if (model.CateID > 0)
                UpdateByID(model);
            else
                insert(model);
            return true;
        }
        /// <summary>
        /// 删除分类
        /// </summary>
        /// <param name="CateID">分类ID</param>
        /// <returns>成功状态</returns>
        public bool DelCate(int ID)
        {
            return Sql.Del(strTableName, PK + "=" + ID);
        }
        //public static DataTable GetCateList()
        //{
        //    string strSql = "select * from ZL_GradeCate Order by CateID Asc";
        //    return SqlHelper.ExecuteTable(CommandType.Text, strSql, null);
        //}
        /// <summary>
        /// 获取分类实例
        /// </summary>
        /// <param name="CateID">分类ID</param>
        /// <returns></returns>
        public M_GradeCate GetCate(int CateID)
        {
            string strSql = "select * from ZL_GradeCate where CateID=@CateID";
            SqlParameter[] cmdParams = new SqlParameter[] {
                new SqlParameter("@CateID",CateID)
            };
            using (DbDataReader sdr = SqlHelper.ExecuteReader(CommandType.Text, strSql, cmdParams))
            {
                if (sdr.Read())
                {
                    return initMod.GetModelFromReader(sdr);
                }
                else
                {
                    return new M_GradeCate();
                }
            }
        }
        public int insert(M_GradeCate model)
        {
            return DBCenter.Insert(model);
        }
        public bool UpdateByID(M_GradeCate model)
        {
            return DBCenter.UpdateByID(model, model.CateID);
        }
        public DataTable Sel()
        {
            return Sql.Sel(strTableName);
        }
        public bool Del(int ID)
        {
            return Sql.Del(strTableName, ID);
        }
        public void DelByIDS(string ids)
        {
            SafeSC.CheckIDSEx(ids);
            string sql = "DELETE FROM " + strTableName + " WHERE " + PK + " IN(" + ids + ")";
            SqlHelper.ExecuteSql(sql);
        }
        //-------------------
        public PageSetting SelPage(int cpage, int psize, Com_Filter filter)
        {
            string where = "1=1 ";
            PageSetting setting = PageSetting.Single(cpage,psize,strTableName,PK,where,PK+" DESC");
            DBCenter.SelPage(setting);
            return setting;
        }
    }
}
