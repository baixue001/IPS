using System;
using System.Data;
using ZoomLa.Model;
using ZoomLa.Common;
using ZoomLa.SQLDAL;
using System.Data.SqlClient;
using System.Data.Common;
using ZoomLa.SQLDAL.SQL;

namespace ZoomLa.BLL
{


    /// <summary>
    /// B_RoomActiveJoin 的摘要说明
    /// </summary>
    public class B_RoomActiveJoin
    {
        public string TbName, PK;
        public M_RoomActiveJoin initMod = new M_RoomActiveJoin();
        public DataTable dt = null;
        public B_RoomActiveJoin()
        {
            TbName = initMod.TbName;
            PK = initMod.PK;
        }

        /// <summary>
        /// 根据ID查询一条记录
        /// </summary>
        public DataTable Sel(int ID)
        {
            return Sql.Sel(TbName, PK, ID);
        }
        public PageSetting SelPage(int cpage, int psize)
        {
            PageSetting setting = PageSetting.Single(cpage, psize, TbName, PK, "");
            DBCenter.SelPage(setting);
            return setting;
        }

        /// <summary>
        /// 根据ID查询一条记录
        /// </summary>
        public M_RoomActiveJoin SelReturnModel(int ID)
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
        /// 根据条件查询一条记录
        /// </summary>
        public M_RoomActiveJoin SelReturnModel(string strWhere)
        {
            using (DbDataReader reader = Sql.SelReturnReader(TbName, strWhere))
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
            return Sql.Sel(TbName);
        }

        /// <summary>
        /// 排序
        /// </summary>
        public DataTable Sel(string strWhere, string strOrderby)
        {
            return Sql.Sel(TbName, strWhere, strOrderby);
        }

        /// <summary>
        /// 根据ID更新
        /// </summary>
        public bool UpdateByID(M_RoomActiveJoin model)
        {
            return DBCenter.UpdateByID(model,model.ID);
        }

        public bool Del(int ID)
        {
            return Sql.Del(TbName, ID);
        }

        public int insert(M_RoomActiveJoin model)
        {
            return DBCenter.Insert(model);
        }

        /// <summary>
        ///添加记录
        /// </summary>
        /// <param name="RoomActiveJoin"></param>
        /// <returns></returns>
        public bool GetInsert(M_RoomActiveJoin model)
        {
            return DBCenter.Insert(model)> 0;
        }

        /// <summary>
        ///更新记录
        /// </summary>
        /// <param name="RoomActiveJoin"></param>
        /// <returns></returns>
        public bool GetUpdate(M_RoomActiveJoin model)
        {
            return DBCenter.UpdateByID(model,model.ID);
        }

        /// <summary>
        ///不存在则添加否则更新
        /// </summary>
        /// <param name="RoomActiveJoin"></param>
        /// <returns></returns>
        public bool InsertUpdate(M_RoomActiveJoin model)
        {
            if (model.ID > 0)
                GetUpdate(model);
            else
                GetInsert(model);
            return true;
        }

        /// <summary>
        ///删除记录
        /// </summary>
        /// <param name="RoomActiveJoin"></param>
        /// <returns></returns>
        public bool GetDelete(int ID)
        {
            return Sql.Del(TbName, PK + "=" + ID);
        }

        /// <summary>
        ///查找一条记录
        /// </summary>
        /// <param name="RoomActiveJoin"></param>
        /// <returns></returns>
        public M_RoomActiveJoin GetSelect(int ID)
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
        ///返回所有记录
        /// </summary>
        /// <returns></returns>
        public DataTable Select_All()
        {
            return Sql.Sel(TbName, "", "");
        }

        /// <summary>
        ///按更多的条件查找记录
        /// </summary>
        /// <param name="Selectstr"></param>
        /// <param name="strSQL"></param>
        /// <param name="Orderby"></param>
        /// <returns></returns>
        public DataTable Select_ByValue(string Selectstr, string strSQL, string Orderby)
        {
            string strSql = "select ";
            if (!string.IsNullOrEmpty(Selectstr))
            {
                strSql += Selectstr + " from ";
            }
            strSql += "ZL_RoomActiveJoin";
            if (!string.IsNullOrEmpty(strSQL))
            {
                strSql += " where " + strSQL;
            }
            if (!string.IsNullOrEmpty(Orderby))
            {
                strSql += " Order by " + Orderby;
            }
            return SqlHelper.ExecuteTable(CommandType.Text, strSql, null);
        }
        /// <summary>
        /// 根据用户查询数据
        /// </summary>
        /// <param name="uid">用户id</param>
        /// <param name="active">活动id</param>
        /// <returns></returns>
        public DataTable SelByUid(int uid,int active)
        {
            string sql = "SELECT * FROM " + TbName + " WHERE UserID=" + uid + " AND ActiveID="+active;
            return SqlHelper.ExecuteTable(CommandType.Text, sql);
        }

    }
}
