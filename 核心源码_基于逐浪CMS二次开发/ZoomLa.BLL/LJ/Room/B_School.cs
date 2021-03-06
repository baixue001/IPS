﻿using System;
using System.Data;
using ZoomLa.Model;
using ZoomLa.Common;
using ZoomLa.SQLDAL;
using System.Data.SqlClient;
using System.Collections.Generic;
using System.Data.Common;
using ZoomLa.SQLDAL.SQL;

namespace ZoomLa.BLL
{


    public class B_School
    {
        public string TbName, PK;
        public M_School initMod = new M_School();
        public DataTable dt = null;
        public B_School()
        {
            TbName = initMod.TbName;
            PK = initMod.PK;
        }
        public DataTable Sel(int ID)
        {
            return Sql.Sel(TbName, PK, ID);
        }
        public M_School SelReturnModel(int ID)
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
            return Sql.Sel(TbName, "", "AddTime DESC");
        }
        public DataTable SelByName(string name)
        {
            SqlParameter[] sp = new SqlParameter[] { new SqlParameter("name", name) };
            string sql = "SELECT * FROM " + TbName + " WHERE SchoolName=@name";
            return SqlHelper.ExecuteTable(sql, sp);
        }
        public M_School SelModelByName(string schoolName)
        {
            SqlParameter[] sp = new SqlParameter[] { new SqlParameter("schoolName", schoolName.Trim()) };
            using (DbDataReader reader = Sql.SelReturnReader(TbName, " WHERE SchoolName=@schoolName", sp))
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
        /// 排序
        /// </summary>
        public DataTable Sel(string strWhere, string strOrderby)
        {
            return Sql.Sel(TbName, strWhere, strOrderby);
        }
        /// <summary>
        /// 根据ID更新
        /// </summary>
        public bool UpdateByID(M_School model)
        {
            return DBCenter.UpdateByID(model,model.ID);
        }
        public bool Del(int ID)
        {
            return Sql.Del(TbName, ID);
        }
        public bool DelByIDS(string ids)
        {
            SafeSC.CheckIDSEx(ids);
            string sql = "DELETE FROM " + TbName + " WHERE ID IN (" + ids + ")";
            return SqlHelper.ExecuteSql(sql);
        }
        public int insert(M_School model)
        {
            return DBCenter.Insert(model);
        }
        public int GetInsert(M_School model)
        {
            return DBCenter.Insert(model);
        }
        public bool GetUpdate(M_School model)
        {
            return DBCenter.UpdateByID(model,model.ID);
        }
        public bool InsertUpdate(M_School model)
        {
            if (model.ID > 0)
                GetUpdate(model);
            else
                GetInsert(model);
            return true;
        }
        public bool GetDelete(int ID)
        {
            return Sql.Del(TbName, PK + "=" + ID);
        }
        public M_School GetSelect(int ID)
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
        public PageSetting SelPage(int cpage, int psize, string name = "", string province = "", string city = "", string county = "")
        {
            string where = " 1=1";
            List<SqlParameter> sp = new List<SqlParameter>();
            if (!string.IsNullOrEmpty(name)) { where += " AND SchoolName @LIKE @name"; sp.Add(new SqlParameter("name", name)); }
            if (!string.IsNullOrEmpty(province)) { where += " AND Province=@province"; sp.Add(new SqlParameter("province", province)); }
            if (!string.IsNullOrEmpty(city)) { where += " AND City=@city"; sp.Add(new SqlParameter("city", city)); }
            if (!string.IsNullOrEmpty(county)) { where += " AND County LIKE @county"; sp.Add(new SqlParameter("county", "%" + county + "%")); }
            PageSetting setting = PageSetting.Single(cpage, psize, TbName, PK, where, "", sp);
            DBCenter.SelPage(setting);
            return setting;
        }
        /// <summary>
        /// 按条件筛选学校
        /// </summary>
        public DataTable SelSchool(string Keyword, string province = "", string city = "", string county = "")
        {
            string sql = "Select * FROM " + TbName + " WHERE 1=1 ";
            if (!string.IsNullOrEmpty(Keyword)) { sql += " AND SchoolName LIKE @Keyword"; }
            if (!string.IsNullOrEmpty(province)) { sql += " AND Province=@province"; }
            if (!string.IsNullOrEmpty(city)) { sql += " AND city=@city"; }
            if (!string.IsNullOrEmpty(county)) { sql += " AND county LIKE @county"; }
            SqlParameter[] sp = new SqlParameter[] {
                new SqlParameter("Keyword", "%"+Keyword+"%"),
                new SqlParameter("province",province),
                new SqlParameter("city",city),
                new SqlParameter("county","%"+county+"%"),
            };
            return SqlHelper.ExecuteTable(CommandType.Text, sql, sp);
        }
        public DataTable Sel_All()
        {
            string strSql = "SELECT * FROM " + TbName + " WHERE 1=1 ORDER BY Addtime DESC";
            return SqlHelper.ExecuteTable(CommandType.Text, strSql);
        }
        public DataTable Select_All()
        {
            return Sql.Sel(TbName, "", "");
        }
        public DataTable SelectSchoolRoom(string country, string province, string schooltype, string visage)
        {
            string strSQL = "SELECT * FROM " + TbName + " WHERE 1=1 ";
            if (!string.IsNullOrEmpty(country))
            {
                strSQL += " and Country=@country ";
            }
            if (!string.IsNullOrEmpty(province))
            {
                strSQL += " and Province=@province";
            }
            if (schooltype != "0")
            {
                strSQL += " and SchoolType=@schooltype";
            }
            if (visage != "0")
            {
                strSQL += " and Visage=@visage";
            }
            SqlParameter[] sp = new SqlParameter[] { new SqlParameter("Country", country), new SqlParameter("Province", province), new SqlParameter("SchoolType", schooltype), new SqlParameter("Visage", visage) };
            strSQL += " ORDER BY AddTime DESC";
            return SqlHelper.ExecuteTable(CommandType.Text, strSQL, sp);
        }
    }
}
