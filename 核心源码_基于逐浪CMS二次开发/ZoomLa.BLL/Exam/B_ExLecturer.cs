﻿using System;
using System.Data;
using ZoomLa.Model;
using ZoomLa.Common;
using ZoomLa.SQLDAL;
using System.Data.SqlClient;
using System.Data.Common;
using ZoomLa.SQLDAL.SQL;

namespace ZoomLa.BLL
{


    public class B_ExLecturer
    {
        public B_ExLecturer()
        {
            strTableName = initmod.TbName;
            PK = initmod.PK;
        }
        public string PK, strTableName;
        private M_ExLecturer initmod = new M_ExLecturer();
		/// <summary>
		///添加记录
		/// </summary>
		public int GetInsert(M_ExLecturer model)
        {
           return DBCenter.Insert(model);
        }

		/// <summary>
		///更新记录
		/// </summary>
		/// <param name="ExLecturer"></param>
		/// <returns></returns>
		public bool GetUpdate(M_ExLecturer model)
        {
            return DBCenter.UpdateByID(model,model.ID);
        }

		/// <summary>
		///删除记录
		/// </summary>
		/// <param name="ExLecturer"></param>
		/// <returns></returns>
		public bool DeleteByGroupID(int ExLecturerID)
        {
            return Sql.Del(strTableName, ExLecturerID);
        }

		/// <summary>
		///查找一条记录
		/// </summary>
		/// <param name="ExLecturer"></param>
		/// <returns></returns>
		public M_ExLecturer GetSelect(int ExLecturerID)
        {
            string sqlStr = "SELECT [ID],[TechName],[TechType],[TechSex],[TechTitle],[TechPhone],[CreateTime],[TechSpecialty],[TechHobby],[TechIntrodu],[TechLevel],[TechDepart],[TechClass],[TechRecom],[Popularity],[Awardsinfo],[FileUpload],[AddUser],[Professional] FROM [dbo].[ZL_ExLecturer] WHERE [ID] = @ID";
            SqlParameter[] cmdParams = new SqlParameter[1];
            cmdParams[0] = new SqlParameter("@ID", SqlDbType.Int, 4);
            cmdParams[0].Value = ExLecturerID;
            using (DbDataReader reader = SqlHelper.ExecuteReader(CommandType.Text, sqlStr, cmdParams))
            {
                if (reader.Read())
                {
                    return GetInfoFromReader(reader);
                }
                else
                {
                    return new M_ExLecturer();
                }
            }
        }
        private static M_ExLecturer GetInfoFromReader(DbDataReader rdr)
        {
            M_ExLecturer info = new M_ExLecturer();
            info.ID = DataConverter.CLng(rdr["ID"].ToString());
            info.TechName = rdr["TechName"].ToString();
            info.TechType = rdr["TechType"].ToString();
            info.TechSex = DataConverter.CLng(rdr["TechSex"].ToString());
            info.TechTitle = rdr["TechTitle"].ToString();
            info.TechPhone = rdr["TechPhone"].ToString();
            info.CreateTime = DataConverter.CDate(rdr["CreateTime"].ToString());
            info.TechSpecialty = rdr["TechSpecialty"].ToString();
            info.TechHobby = rdr["TechHobby"].ToString();
            info.TechIntrodu = rdr["TechIntrodu"].ToString();
            info.TechLevel = rdr["TechLevel"].ToString();
            info.TechDepart = DataConverter.CLng(rdr["TechDepart"].ToString());
            info.TechClass = DataConverter.CLng(rdr["TechClass"].ToString());
            info.TechRecom = DataConverter.CLng(rdr["TechRecom"].ToString());
            info.Popularity = DataConverter.CLng(rdr["Popularity"].ToString());
            info.Awardsinfo = rdr["Awardsinfo"].ToString();
            info.FileUpload = rdr["FileUpload"].ToString();
            info.AddUser = DataConverter.CLng(rdr["AddUser"].ToString());
            info.Professional = DataConverter.CLng(rdr["Professional"].ToString());
            rdr.Close();
            rdr.Dispose();
            return info;
        }
		/// <summary>
		///返回所有记录
		/// </summary>
		/// <returns></returns>
		public DataTable Select_All()
        {
            return Sql.Sel(strTableName);
        }
        public PageSetting SelPage(int cpage, int psize)
        {
            PageSetting setting = PageSetting.Single(cpage, psize, strTableName, PK, "");
            DBCenter.SelPage(setting);
            return setting;
        }
    }
}
