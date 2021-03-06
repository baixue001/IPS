﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using ZoomLa.Common;
using ZoomLa.Model;

using ZoomLa.SQLDAL;
using ZoomLa.SQLDAL.SQL;

namespace ZoomLa.BLL.User
{
    public class B_Com_VisitCount
    {
        private string PK, TbName;
        private M_Com_VisitCount initMod = new M_Com_VisitCount();
        public string[] typeArr = "mbh5,h5,ppt".Split(',');
        public B_Com_VisitCount()
        {
            PK = initMod.PK;
            TbName = initMod.TbName;
        }
        public static int Insert(M_Com_VisitCount model)
        {
            return DBCenter.Insert(model);
        }
        public bool UpdateByID(M_Com_VisitCount model)
        {
            return DBCenter.UpdateByID(model,model.ID);
        }
        public void UpdateNum(int ID)
        {
            DBCenter.UpdateSQL(TbName, "DVisitNum=DVisitNum+1","ID="+ID);
        }
        public bool Del(int ID)
        {
            return DBCenter.DelByIDS(TbName,PK,ID.ToString());
        }
        public M_Com_VisitCount SelReturnModel(int ID)
        {
            using (DbDataReader reader = DBCenter.SelReturnReader(TbName, PK, ID))
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
        public bool SelByIP(string IP)
        {
            return DBCenter.IsExist(TbName,"IP="+IP);
        }
        public DataTable SelDataByIP(string IP)
        {
            return SqlHelper.ExecuteTable("SELECT * FROM " + TbName + " WHERE IP = " + IP);
        }
        public DataTable Sel()
        {
            return DBCenter.Sel(TbName);
        }
        public DataTable CountByIP()
        {
            string sql = "SELECT IP FROM " + TbName + " GROUP BY IP";
            return DBCenter.DB.ExecuteTable(new SqlModel() { sql = sql });
        }
        public int GetAllNum(string field)
        {
            return DBCenter.Count(TbName,"");
        }
        public DataTable SelByField(string field)
        {
            string sql = "SELECT COUNT(" + field + ")AS NUM," + field + " FROM " + TbName + " WHERE " + field + " IS NOT NULL GROUP BY " + field;
            return DBCenter.DB.ExecuteTable(new SqlModel() { sql = sql });
        }
        /// <summary>
        /// 统计月总访问量
        /// </summary>
        /// <param name="year"></param>
        /// <param name="month"></param>
        /// <returns></returns>
        public DataTable CountByMonth(int year, int month)
        {
            string sql = "SELECT COUNT(*) AS Num FROM " + TbName + " WHERE YEAR(CDate)=" + year + " AND MONTH(CDate)=" + month;
            return DBCenter.DB.ExecuteTable(new SqlModel() { sql = sql });
        }
        public DataTable CountByYear(int year)
        {
            string sql = "SELECT COUNT(*) AS Num FROM " + TbName + " WHERE YEAR(CDate)=" + year;
            return DBCenter.DB.ExecuteTable(new SqlModel() { sql = sql });
        }
        public DataTable SelByTime(int type, DateTime time)
        {
            string sql = "";
            SqlParameter[] sp = new SqlParameter[] { new SqlParameter("Year", time.Year), new SqlParameter("Month", time.Month) };
            switch (type)
            {
                case 1://按年统计
                    sql = "SELECT COUNT(YEAR)AS NUM,A.YEAR FROM (SELECT *,YEAR(CDate)AS YEAR FROM ZL_Com_VisitCount )AS A GROUP BY YEAR";
                    break;
                case 2://按月统计
                    sql = "SELECT COUNT(MONTH)AS NUM,A.MONTH FROM (SELECT *,MONTH(CDate)AS MONTH FROM ZL_Com_VisitCount WHERE YEAR(CDate)=@Year)AS A GROUP BY MONTH";
                    break;
                case 3://按日统计
                    sql = "SELECT COUNT(DAY)AS NUM,A.DAY,A.Year,A.Month FROM (SELECT *,YEAR(CDate) AS Year,MONTH(CDate) AS Month,DAY(CDate)AS DAY FROM ZL_Com_VisitCount WHERE (YEAR(CDate)=@Year AND MONTH(CDate)=@Month))AS A GROUP BY Year,Month,DAY";
                    break;
            }
            return DBCenter.DB.ExecuteTable(new SqlModel(sql,sp));
        }
        /// <summary>
        /// 按系统查询
        /// </summary>
        /// <returns></returns>
        public DataTable SelBySys()
        {
            string sql = "SELECT COUNT(OSVersion) AS Num,OSVersion FROM " + TbName + " GROUP BY OSVersion";
            return DBCenter.DB.ExecuteTable(new SqlModel() { sql = sql });
        }
        /// <summary>
        /// 按浏览器管理
        /// </summary>
        /// <returns></returns>
        public DataTable SelByBrowser()
        {
            string sql = "SELECT COUNT(BrowerVersion) AS Num,BrowerVersion FROM " + TbName + " GROUP BY BrowerVersion";
            return DBCenter.DB.ExecuteTable(new SqlModel() { sql = sql });
        }
        public DataTable SelectAll(int year = 0, int month = 0, string username = "", string source = "")
        {
            string wherestr = "1=1";
            if (year > 0)
            { wherestr += " AND YEAR(CDate)=" + year; }
            if (month > 0)
            { wherestr += " AND MONTH(CDate)=" + month; }
            if (!string.IsNullOrEmpty(username))
            { wherestr += " AND B.UserName LIKE @username"; }
            if (!string.IsNullOrEmpty(source))
            { wherestr += " AND A.Source LIKE @source"; }
            SqlParameter[] sp = new SqlParameter[] { new SqlParameter("username", "%" + username + "%"), new SqlParameter("source", "%" + source + "%") };
            string sql = "SELECT A.*,B.UserName FROM " + TbName + " A LEFT JOIN ZL_User B ON A.UserID=B.UserID WHERE " + wherestr + " ORDER BY A.CDate DESC";
            return DBCenter.DB.ExecuteTable(new SqlModel(sql,sp));
        }
        public DataTable SelForSum(string ztype, string infotitle = "")
        {
            string where = "1=1 ";
            List<SqlParameter> sp = new List<SqlParameter>();
            if (!string.IsNullOrEmpty(ztype))
            {
                string typestr = "";
                foreach (string type in ztype.ToLower().Split(','))
                {
                    if (typeArr.Contains(type)) { typestr += "'" + type + "',"; }
                }
                where += " AND ZType IN (" + typestr.Trim(',') + ")";
            }
            if (!string.IsNullOrEmpty(infotitle))
            {
                where += " AND InfoTitle LIKE @infotitle";
                sp.Add(new SqlParameter("infotitle", "%" + infotitle + "%"));
            }
            where += " AND InfoID <> '' AND InfoID IS NOT NULL";
            string sql = "SELECT *,(SELECT COUNT(*) FROM ZL_Com_VisitCount WHERE InfoID=A.InfoID) VisitCount FROM ZL_Com_VisitCount A WHERE ID IN (SELECT MAX(ID) ID FROM ZL_Com_VisitCount WHERE " + where + " GROUP BY InfoID)";
            return DBCenter.DB.ExecuteTable(new SqlModel(sql,sp.ToArray()));
        }

        public DataTable SelByType(string ztype, string infotitle, int uid)
        {
            string where = "1=1 ";
            List<SqlParameter> sp = new List<SqlParameter>();
            if (!string.IsNullOrEmpty(ztype))
            {
                string typestr = "";
                foreach (string type in ztype.ToLower().Split(','))
                {
                    if (typeArr.Contains(type)) { typestr += "'" + type + "',"; }
                }
                where += " AND ZType IN (" + typestr.Trim(',') + ")";
            }
            if (!string.IsNullOrEmpty(infotitle))
            {
                where += " AND InfoTitle LIKE @infotitle";
                sp.Add(new SqlParameter("infotitle", "%" + infotitle + "%"));
            }
            if (uid > 0) { where += " AND UserID = " + uid; }
            return DBCenter.Sel(TbName, where, "CDate DESC,InfoID ASC", sp);
        }
        public DataTable SelByType(string ztype, string infotitle = "")
        {
            return SelByType(ztype, infotitle, 0);
        }
        public PageSetting SelPage(int pageIndex, int pageSize, string ztype, int uid, string infotitle = "", string infoid = "")
        {
            List<SqlParameter> spList = new List<SqlParameter>();
            string where = "1=1", order = "CDate DESC,InfoID ASC";
            if (!string.IsNullOrEmpty(ztype))
            {
                string typestr = "";
                foreach (string type in ztype.ToLower().Split(','))
                {
                    if (typeArr.Contains(type)) { typestr += "'" + type + "',"; }
                }
                where += " AND ZType IN (" + typestr.Trim(',') + ")";
            }
            if (!string.IsNullOrEmpty(infotitle))
            {
                where += " AND InfoTitle LIKE @infotitle";
                spList.Add(new SqlParameter("infotitle", "%" + infotitle + "%"));
            }
            if (!string.IsNullOrEmpty(infoid))
            {
                where += " AND InfoID = @infoid";
                spList.Add(new SqlParameter("infoid", infoid));
            }
            if (uid > 0) { where += " AND UserID = " + uid; }
            PageSetting config = new PageSetting()
            {
                fields = "A.*",
                pk = PK,
                t1 = TbName,
                cpage = pageIndex,
                psize = pageSize,
                where = where,
                order = order,
                sp = spList.ToArray()
            };
            config.dt = DBCenter.SelPage(config);
            return config;
        }
        public DataTable SelByInfoID(string InfoId)
        {
            string where = " InfoID = @InfoID";
            List<SqlParameter> sp = new List<SqlParameter>() { new SqlParameter("InfoID", InfoId) };
            return DBCenter.Sel(TbName, where, "CDate DESC", sp);
        }
        public int GetVisitCount(string infoid, string ztype = "mbh5,h5")
        {
            string where = " Where 1=1";
            List<SqlParameter> sp = new List<SqlParameter>();
            if (!string.IsNullOrEmpty(ztype))
            {
                string typestr = "";
                foreach (string type in ztype.ToLower().Split(','))
                {
                    if (typeArr.Contains(type)) { typestr += "'" + type + "',"; }
                }
                where += " AND ZType IN (" + typestr.Trim(',') + ")";
            }
            where += " AND InfoID=@infoid";
            sp.Add(new SqlParameter("infoid", infoid));
            object obj = SqlHelper.ExecuteScalar(CommandType.Text, "SELECT COUNT(*) AS COUNT FROM " + TbName + where, sp.ToArray());
            return DataConvert.CLng(obj);
        }
    }
}
