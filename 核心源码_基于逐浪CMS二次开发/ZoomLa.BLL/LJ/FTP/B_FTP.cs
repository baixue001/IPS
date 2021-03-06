﻿using System;
using System.Collections.Generic;
using System.Text;
using ZoomLa.SQLDAL;
using ZoomLa.Model.FTP;
using System.Data;
using System.Data.SqlClient;
using ZoomLa.SQLDAL.SQL;
using System.Data.Common;

namespace ZoomLa.BLL.FTP
{
    public class B_FTP
    {
        public B_FTP()
        {
            strTableName = initmod.TbName;
            PK = initmod.PK;
        }
        public string PK, strTableName;
        private M_FtpConfig initmod = new M_FtpConfig();
        /// <summary>
        /// 插入配置信息
        /// </summary>
        public int AddFTPFile(M_FtpConfig model)
        {
           return DBCenter.Insert(model);
        }

        /// <summary>
        /// 查询配置记录
        /// </summary>
        /// <returns></returns>
        public DataTable SelectFtp_All()
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
        /// 根据ID查询配置记录
        /// </summary>
        public M_FtpConfig SeleteIDByAll(int id)
        {
            string sqlStr = "select * from ZL_FTPConfig where ID=" + id;
            using (DbDataReader reader = SqlHelper.ExecuteReader(CommandType.Text, sqlStr))
            {
                if (reader.Read())
                {
                    M_FtpConfig mf =GetFtpFile(reader);
                    return mf;
                }
                else
                {
                    return new M_FtpConfig();
                }
            }
        }
        private M_FtpConfig GetFtpFile(DbDataReader reader)
        {
            M_FtpConfig mf = new M_FtpConfig();
            mf.ID = Convert.ToInt32(reader["ID"]);
            mf.FtpServer = reader["FtpServer"].ToString();
            mf.FtpPort = reader["FtpPort"].ToString();
            mf.FtpUsername = reader["FtpUsername"].ToString();
            mf.FtpPassword = reader["FtpPassword"].ToString();
            mf.OutTime = reader["OutTime"].ToString();
            mf.SavePath = reader["SavePath"].ToString();
            mf.Alias = reader["Alias"].ToString();
            mf.Url = reader["Url"].ToString();
            reader.Close();
            reader.Dispose();
            return mf;
        }
        /// <summary>
        /// 修改配置信息
        /// </summary>
        public bool UpdateFtpFile(int id, M_FtpConfig model)
        {
           return DBCenter.UpdateByID(model,model.ID);
        }

        /// <summary>
        /// 删除配置信息
        /// </summary>
        public bool DeleteFtpFile(int id)
        {
            return Sql.Del(strTableName, id);
        }
        public DataTable SelByAlias(string alias) 
        {
            SqlParameter[] sp = new SqlParameter[] {new SqlParameter("Alias",alias) };
            string sql = "Select * From "+strTableName+" Where Alias =@Alias";
            return SqlHelper.ExecuteTable(CommandType.Text,sql,sp);
        }
        public M_FtpConfig SelFirstModel()
        {
            string sqlStr = "select Top 1 * from ZL_FTPConfig";
            using (DbDataReader reader = SqlHelper.ExecuteReader(CommandType.Text, sqlStr))
            {
                if (reader.Read())
                {
                    M_FtpConfig mf = GetFtpFile(reader);
                    return mf;
                }
                else
                {
                    return new M_FtpConfig();
                }
            }
        }
    }
}