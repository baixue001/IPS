using System;
using System.Data;
using ZoomLa.Model;
using ZoomLa.Common;
using ZoomLa.SQLDAL;
using System.Text;
using System.Web;
using System.Data.SqlClient;
using System.Data.Common;
using ZoomLa.SQLDAL.SQL;

namespace ZoomLa.BLL
{


    public class B_OrderBaseField
    {
        private string TbName, PK;
        private M_OrderBaseField initMod = new M_OrderBaseField();
        public B_OrderBaseField()
        {
            TbName = initMod.TbName;
            PK = initMod.PK;
        }
        public DataTable Sel(int ID)
        {
            return Sql.Sel(TbName, PK, ID);
        }
        public M_OrderBaseField SelReturnModel(int ID)
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
        private M_OrderBaseField SelReturnModel(string strWhere)
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
        public bool UpdateByID(M_OrderBaseField model)
        {
            return DBCenter.UpdateByID(model,model.FieldID);
        }
        public bool Del(int ID)
        {
            return Sql.Del(TbName, ID);
        }
        public int insert(M_OrderBaseField model)
        {
            return DBCenter.Insert(model);
        }
        public int GetInsert(M_OrderBaseField orderBaseField)
        {
            return insert(orderBaseField);
        }
        public bool GetUpdate(M_OrderBaseField orderBaseField)
        {
            return UpdateByID(orderBaseField);
        }
        public bool DeleteByGroupID(int OrderBaseFieldID)
        {
            //return dal.DeleteByGroupID(OrderBaseFieldID);
            string sqlStr = "DELETE FROM [dbo].[ZL_OrderBaseField] WHERE [FieldID] = @FieldID";
            SqlParameter[] cmdParams = new SqlParameter[1];
            cmdParams[0] = new SqlParameter("@FieldID", SqlDbType.Int, 4);
            cmdParams[0].Value = OrderBaseFieldID;
            return SqlHelper.ExecuteSql(sqlStr, cmdParams);
        }
        public M_OrderBaseField GetSelect(int OrderBaseFieldID)
        {
            //return dal.GetSelect(OrderBaseFieldID);
            string sqlStr = "SELECT [FieldID],[FieldName],[FieldAlias],[FieldTips],[Description],[IsNotNull],[FieldType],[Content],[OrderId],[ShowList],[ShowWidth],[NoEdit],[Type] FROM [dbo].[ZL_OrderBaseField] WHERE [FieldID] = @FieldID";
            SqlParameter[] cmdParams = new SqlParameter[1];
            cmdParams[0] = new SqlParameter("@FieldID", SqlDbType.Int, 4);
            cmdParams[0].Value = OrderBaseFieldID;
            using (DbDataReader reader = SqlHelper.ExecuteReader(CommandType.Text, sqlStr, cmdParams))
            {
                if (reader.Read())
                {
                    return initMod.GetModelFromReader(reader);
                }
                else
                {
                    return new M_OrderBaseField();
                }
            }
        }
        /// <summary>
        /// 通过类型查询
        /// </summary>
        /// <param name="type">类型：0:为订单,1:购物车</param>
        /// <returns></returns>
        public DataTable Select_Type(int type)
        {
            string sqlStr = "SELECT * FROM [dbo].[ZL_OrderBaseField] WHERE Type=@Type ORDER BY OrderID DESC";
            SqlParameter[] para = new SqlParameter[]{
                new SqlParameter("@Type",type)
            };
            return SqlHelper.ExecuteTable(CommandType.Text, sqlStr, para);
        }
        /// <summary>
        /// 查询上一个字段
        /// </summary>
        /// <param name="CurrentID">当前字段ID</param>
        /// <returns></returns>
        public M_OrderBaseField GetPreField(int CurrentID)
        {
            string strSql = "select top 1 FieldID from ZL_OrderBaseField where OrderId<@CurrentID order by OrderId desc";
            SqlParameter[] cmdParam = new SqlParameter[] {
                new SqlParameter("@CurrentID",SqlDbType.Int,4)
            };
            cmdParam[0].Value = CurrentID;
            int FieldID = DataConverter.CLng(SqlHelper.ExecuteScalar(CommandType.Text, strSql, cmdParam));

            return Get_Select(FieldID);
        }

        /// <summary>
        /// 查询下一个字段
        /// </summary>
        /// <param name="CurrentID">当前字段ID</param>
        /// <returns></returns>
        public M_OrderBaseField GetNextField(int CurrentID)
        {
            string strSql = "select Top 1 FieldID from ZL_OrderBaseField where OrderId>@CurrentID order by OrderId Asc";
            SqlParameter[] cmdParam = new SqlParameter[] {
                new SqlParameter("@CurrentID",SqlDbType.Int,4)
            };
            cmdParam[0].Value = CurrentID;
            int FieldID = DataConverter.CLng(SqlHelper.ExecuteScalar(CommandType.Text, strSql, cmdParam));
            return Get_Select(FieldID);
        }
        public int GetMaxID()
        {
            int id = 0;
            DataTable dt = SelectWhere(" 1=1 ", " Max(FieldID) as FieldID ", "");
            if (dt != null && dt.Rows.Count > 0)
            {
                id = DataConverter.CLng(dt.Rows[0]["FieldID"].ToString());
            }
            return id;
        }
        /// <summary>
        /// 通过订单ID查询模型字段数据
        /// </summary>
        /// <param name="orderid">订单ID</param>
        /// <param name="type">类型：0为网站订单模型,1为店铺订单模型</param>
        /// <param name="dt">模型数据</param>
        /// <returns></returns>
        public bool UpdateOrderFile(int orderid, int type, DataTable dt)
        {
            string str = "";
            if (type == 0)  //网站订单模型
            {
                str += "Update ZL_Orderinfo SET " + UpdateSql(dt) + " WHERE id=" + orderid;
            }
            else
            {
                str += "Update ZL_UserOrderinfo SET " + UpdateSql(dt) + " WHERE id=" + orderid;
            }
            SqlParameter[] sqlp = ContentPara(dt);
            new SqlParameter("@id", orderid);
            return SqlHelper.ExecuteSql(str, sqlp);
        }
        public int UpdateModelByOId(DataTable dt, string oid)
        {
            int row = dt.Rows.Count;
            if (dt == null || dt.Rows.Count <= 0)
            {
                return 0;
            }
            SqlParameter[] param = new SqlParameter[row + 1];
            string strsql = "update ZL_OrderInfo set ";
            for (int i = 0; i < row; i++)
            {
                param[i] = GetPara(dt.Rows[i]);
                strsql += dt.Rows[i]["FieldName"].ToString() + "=@" + dt.Rows[i]["FieldName"].ToString() + " ";
                if (i != row - 1) strsql += " , ";
            }
            param[dt.Rows.Count] = new SqlParameter("@OrderNo", SqlDbType.VarChar, 50);
            param[dt.Rows.Count].Value = oid;
            strsql += "where orderno=@OrderNo";
            return SqlHelper.ExecuteNonQuery(CommandType.Text, strsql, param);
        }
        /// <summary>
        ///查找一条记录
        /// </summary>
        /// <param name="FieldID"></param>
        /// <returns></returns>
        public M_OrderBaseField Get_Select(int FieldID)
        {
            string sqlStr = "SELECT [FieldID],[FieldName],[FieldAlias],[FieldTips],[Description],[IsNotNull],[FieldType],[Content],[OrderId],[ShowList],[ShowWidth],[NoEdit],[Type] FROM [dbo].[ZL_OrderBaseField] WHERE [FieldID] = @FieldID";
            SqlParameter[] cmdParams = new SqlParameter[1];
            cmdParams[0] = new SqlParameter("@FieldID", SqlDbType.Int, 4);
            cmdParams[0].Value = FieldID;
            using (DbDataReader reader = SqlHelper.ExecuteReader(CommandType.Text, sqlStr, cmdParams))
            {
                if (reader.Read())
                {
                    return initMod.GetModelFromReader(reader);
                }
                else
                {
                    return new M_OrderBaseField();
                }
            }
        }
        /// <summary>
        /// 查询条件查询字段信息
        /// </summary>
        /// <param name="strSQL">条件</param>
        /// <param name="strSelect">字段</param>
        /// <param name="Orderby">排序</param>
        /// <returns></returns>
        public DataTable SelectWhere(string strSQL, string strSelect, string Orderby)
        {
            string sqlStr = "SELECT " + strSelect + " FROM [dbo].[ZL_OrderBaseField] WHERE " + strSQL;
            if (!string.IsNullOrEmpty(Orderby))
            {
                sqlStr += "ORDER BY" + Orderby;
            }
            return SqlHelper.ExecuteTable(CommandType.Text, sqlStr, null);
        }
        /// <summary>
        /// 通过类型查询
        /// </summary>
        /// <param name="type">类型：0为网站，1为店铺</param>
        /// <returns></returns>
        public DataTable SelectType(int type)
        {
            string sqlStr = "SELECT * FROM [dbo].[ZL_OrderBaseField] WHERE Type=@Type";
            SqlParameter[] para = new SqlParameter[]{
                new SqlParameter("@Type",type)
            };
            return SqlHelper.ExecuteTable(CommandType.Text, sqlStr, para);
        }
        private SqlParameter GetPara(DataRow dr)
        {
            return new SqlParameter("@" + dr["FieldName"], DataConvert.CStr(dr["FieldValue"]));
        }
        private SqlParameter[] ContentPara(DataTable DTContent)
        {
            int count = DTContent.Rows.Count;
            SqlParameter[] sp = new SqlParameter[count];
            int i = 0;
            foreach (DataRow dr in DTContent.Rows)
            {
                sp[i] = GetPara(dr);
                i++;
            }
            return sp;
        }
        private string UpdateSql(DataTable DTContent)
        {
            string strSql = "";
            foreach (DataRow dr in DTContent.Rows)
            {
                if (string.IsNullOrEmpty(strSql))
                    strSql = dr["FieldName"].ToString() + "=@" + dr["FieldName"].ToString();
                else
                    strSql = strSql + "," + dr["FieldName"].ToString() + "=@" + dr["FieldName"].ToString();
            }

            return strSql;
        }
        /// <summary>
        /// 检测字段名是否唯一，唯一True
        /// </summary>
        /// <param name="type"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public bool CheckIsOnly(int type, string name)
        {
            string str = "Select * From " + TbName + " Where Type=" + type + " And FieldName=@FieldName";
            SqlParameter[] sp = new SqlParameter[] { new SqlParameter("FieldName", name) };
            return !(SqlHelper.ExecuteTable(CommandType.Text, str, sp).Rows.Count > 0);
        }
    }
}
