using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using ZoomLa.SQLDAL;
using ZoomLa.SQLDAL.SQL;

namespace ZoomLa.BLL.Helper
{
    public class PageHelper
    {
        //----------------Extend
        public static DataTable SelPage(PageSetting config)
        {
            string countsql = "";
            if (!string.IsNullOrEmpty(config.t2))
            {
                config.sql = "SELECT TOP " + config.psize + config.fields + " FROM " + config.t1 + " A LEFT JOIN " + config.t2 + " B ON " + config.on + " WHERE " + config.pk + " Not In(SELECT TOP " + config.cursize + " " + config.pk + " FROM " + config.t1 + " A LEFT JOIN " + config.t2 + " B ON " + config.on + " WHERE " + config.where + config.order + ") AND " + config.where + config.order;
                countsql = "SELECT COUNT(" + config.pk + ") FROM " + config.t1 + " A LEFT JOIN " + config.t2 + " B ON " + config.on + " WHERE " + config.where;
            }
            else
            {
                config.sql = "Select Top " + config.psize + config.fields + " From " + config.t1 + " Where " + config.pk + " Not In(Select Top " + config.cursize + " " + config.pk + " From " + config.t1 + " Where " + config.where + config.order + ") And " + config.where + config.order;
                countsql = "Select COUNT(" + config.pk + ") From " + config.t1 + " Where " + config.where;
            }
            config.itemCount = DataConvert.CLng(SqlHelper.ExecuteScalar(CommandType.Text, countsql, config.sp));
            config.pageCount = GetPageCount(config.itemCount, config.psize);
            config.dt = SqlHelper.ExecuteTable(config.sql, config.sp);
            return config.dt;
        }
        //----------------
        /// <summary>
        /// SelPage(10, 2, out pageCount, "A.ID", "A.*,B.UserFace", "ZL_Plat_Blog", "ZL_User_PlatView", "A.CreateUser=B.UserID", "A.ReplyUserID=0","Order BY A.CreateTime",null);
        /// 双表联接查询,返回数据(执行两次SQL)
        /// </summary>
        /// <param name="curPage">从1开始</param>
        /// <returns>TotalCount列为总记录数</returns>
        public static DataTable SelPage(int pageSize, int curPage, out int itemCount, string pk, string fields, string t1, string t2, string on, string where, string order, SqlParameter[] sp = null)
        {
            order = " " + order; fields = " " + fields;
            where = string.IsNullOrEmpty(where) ? "1=1" : where;
            pageSize = pageSize > 0 ? pageSize : 10;
            curPage = curPage < 1 ? 1 : curPage; curPage--;
            int curSize = (pageSize * curPage);
            string sql = "SELECT TOP " + pageSize + fields + " FROM " + t1 + " A LEFT JOIN " + t2 + " B ON " + on + " WHERE " + pk + " Not In(SELECT TOP " + curSize + " " + pk + " FROM " + t1 + " A LEFT JOIN " + t2 + " B ON " + on + " WHERE " + where + order + ") AND " + where + order;
            string countsql = "SELECT COUNT(" + pk + ") FROM " + t1 + " A LEFT JOIN " + t2 + " B ON " + on + " WHERE " + where;
            itemCount = DataConvert.CLng(SqlHelper.ExecuteScalar(CommandType.Text, countsql, sp));
            //lastSql = sql;
            //if (sp != null && sp.Length > 0)
            //{
            //    foreach (SqlParameter param in sp)
            //    {
            //        lastSql = lastSql.Replace("@" + param.ParameterName, "'" + param.Value + "'");
            //    }
            //}
            return SqlHelper.ExecuteTable(CommandType.Text, sql, sp);
        }
        /// <summary>
        /// 单表分页查询
        /// </summary>
        /// <returns></returns>
        public static DataTable SelPage(int pageSize, int curPage, out int itemCount, string pk, string fields, string tbname, string where = "", string order = "", SqlParameter[] sp = null)
        {
            order = " " + order; fields = " " + fields;
            where = string.IsNullOrEmpty(where) ? "1=1" : where;
            curPage = curPage < 1 ? 1 : curPage; curPage--;
            int curSize = (pageSize * curPage);
            string sql = "Select Top " + pageSize + fields + " From " + tbname + " Where " + pk + " Not In(Select Top " + curSize + " " + pk + " From " + tbname + " Where " + where + order + ") And " + where + order;
            string countsql = "Select COUNT(" + pk + ") From " + tbname + " Where " + where;
            itemCount = DataConvert.CLng(SqlHelper.ExecuteScalar(CommandType.Text, countsql, sp));
            //lastSql = sql;
            //if (sp != null && sp.Length > 0)
            //{
            //    foreach (SqlParameter param in sp)
            //    {
            //        lastSql = lastSql.Replace("@" + param.ParameterName, "'" + param.Value + "'");
            //    }
            //}
            return SqlHelper.ExecuteTable(CommandType.Text, sql, sp);
        }
        public static PageSetting GetPagedTable(DataTable dt, int cpage, int psize)
        {
            PageSetting setting = new PageSetting();
            setting.dt = dt;
            setting.itemCount = dt.Rows.Count;
            setting.psize = psize;
            setting.pageCount = GetPageCount(setting.itemCount, setting.psize);
            //-------------------------
            if (cpage == 0) return setting;
            DataTable newdt = dt.Copy();
            newdt.Clear();//copy dt的框架

            int rowbegin = (cpage - 1) * psize;
            int rowend = cpage * psize;
            if (rowbegin >= dt.Rows.Count)
            {
                setting.dt = newdt;
                return setting;//源数据记录数小于等于要显示的记录，直接返回dt
            }

            if (rowend > dt.Rows.Count)
                rowend = dt.Rows.Count;
            for (int i = rowbegin; i <= rowend - 1; i++)
            {
                DataRow newdr = newdt.NewRow();
                DataRow dr = dt.Rows[i];
                foreach (DataColumn column in dt.Columns)
                {
                    newdr[column.ColumnName] = dr[column.ColumnName];
                }
                newdt.Rows.Add(newdr);
            }
            setting.dt = newdt;
            return setting;
        }
        //-------------Tools
        public static int GetCPage(int cpage, int min, int max)
        {
            if (cpage > max) { cpage = max; }
            if (cpage < min) { cpage = min; }
            if (cpage < 0) { cpage = min; }
            return cpage;
        }
        /// <summary>
        /// 计算分页数量
        /// </summary>
        /// <param name="itemCount">数据条数</param>
        /// <param name="pageSize">分页大小</param>
        /// <returns>页面数</returns>
        public static int GetPageCount(int itemCount, int pageSize) { return itemCount / pageSize + ((itemCount % pageSize > 0) ? 1 : 0); }
        //-----------------
      
    }
}
