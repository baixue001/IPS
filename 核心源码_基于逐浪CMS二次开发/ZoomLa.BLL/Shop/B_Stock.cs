using System;
using System.Data;
using ZoomLa.Model;
using ZoomLa.Common;
using ZoomLa.SQLDAL;
using System.Data.SqlClient;
using ZoomLa.SQLDAL.SQL;
using System.Collections.Generic;
using System.Data.Common;
namespace ZoomLa.BLL
{



    public class B_Stock
    {
        private string TbName, PK;
        private M_Stock initMod = new M_Stock();
        public B_Stock()
        {
            TbName = initMod.TbName;
            PK = initMod.PK;
        }
        public M_Stock SelReturnModel(int ID)
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
            return DBCenter.Sel(TbName, "", PK + " DESC");
        }
        public PageSetting SelPage(int cpage, int psize,F_Stock filter)
        {
            string where = "1=1 ";
            //if (storeid != -100) { where += " AND A.StoreID=" + storeid; }
            List<SqlParameter> sp = new List<SqlParameter>();
            if (filter.ProID != 0 && filter.ProID != -100) { where += " AND A.ProID=" + filter.ProID; }
            if (filter.StoreID != -100) { where += " AND A.StoreID=" + filter.StoreID; }
            if (filter.StockType != -100) { where += " AND A.StockType=" +filter.StockType; }
            if (!string.IsNullOrEmpty(filter.ProName))
            {
                where += " AND B.ProName LIKE @proname";
                sp.Add(new SqlParameter("proname", "%" + filter.ProName + "%"));
            }
            if (!string.IsNullOrEmpty(filter.AddUser))
            {
                where += " AND A.AddUser LIKE @adduser";
                sp.Add(new SqlParameter("adduser", "%" + filter.AddUser + "%"));
            }
            if (!string.IsNullOrEmpty(filter.SDate))
            {
                where += " AND A.AddTime>=@sdate";
                sp.Add(new SqlParameter("sdate", Convert.ToDateTime(filter.SDate).ToString("yyyy/MM/dd 00:00:00")));
            }
            if (!string.IsNullOrEmpty(filter.EDate))
            {
                where += " AND A.AddTime<=@edate";
                sp.Add(new SqlParameter("edate", Convert.ToDateTime(filter.EDate).ToString("yyyy/MM/dd 23:59:59")));
            }
            if (filter.NodeID != 0)
            {
                where += " AND B.NodeID="+filter.NodeID;
            }
            //PageSetting setting = PageSetting.Single(cpage, psize, TbName, PK, where);
            PageSetting setting = PageSetting.Double(cpage, psize, TbName, "ZL_Commodities", "A.ID", "A.ProID=B.ID", where, "A.ID DESC", sp, "A.*,B.ProName,B.NodeID");
            DBCenter.SelPage(setting);
            return setting;
        }
        public bool UpdateByID(M_Stock model)
        {
            return DBCenter.UpdateByID(model,model.id);
        }
        public bool Del(int ID)
        {
            return Sql.Del(TbName, ID);
        }
        public void DelByIDS(string ids)
        {
            if (string.IsNullOrEmpty(ids)) { return; }
            SafeSC.CheckIDSEx(ids); DBCenter.DelByIDS(TbName, PK, ids);
        }
        private int insert(M_Stock model)
        {
            return DBCenter.Insert(model);
        }
        public DataTable SelectTypeByID(int typename)
        {
            string strSql = "select * from ZL_Stock where stocktype=" + typename + " order by(ID) desc";
            return SqlHelper.ExecuteTable(CommandType.Text, strSql, null);
        }
        public static string GetCode(string action)
        {
            switch (action)
            {
                case "RK":
                    break;
                case "CK":
                    break;
                default:
                    throw new Exception("无效的操作符");
            }
            return action + DateTime.Now.ToString("yyyyMMddHHmmssffffff");
        }
        /// <summary>
        /// 插入数据的同时修改商品的库存信息
        /// </summary>
        public bool AddStock(M_Stock model)
        {
            string sqlStrs = "";
            if (model.stocktype == 0)
            {
                sqlStrs = "Update ZL_Commodities set Stock=Stock+" + model.Pronum + " where ID=" + model.proid;
            }
            else
            {
                sqlStrs = "Update ZL_Commodities set Stock=Stock-" + model.Pronum + " where ID=" + model.proid;

            }
            SqlHelper.ExecuteSql(sqlStrs);
            return insert(model) > 0;
        }
    }
    public class F_Stock
    {
        public int StoreID = -100;
        public int StockType = -100;
        public int ProID = -100;
        public int NodeID = 0;
        public string SDate = "";
        public string EDate = "";
        public string ProName = "";
        public string AddUser = "";
    }
}