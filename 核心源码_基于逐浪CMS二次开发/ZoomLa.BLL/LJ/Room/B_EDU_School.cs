﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using ZoomLa.Model.Room;
using ZoomLa.SQLDAL;
using ZoomLa.SQLDAL.SQL;

namespace ZoomLa.BLL.Room
{
    public class B_EDU_School : ZL_Bll_InterFace<M_EDU_School>
    {
        public string TbName, PK;
        public M_EDU_School initMod = new M_EDU_School();
        public DataTable dt = null;
        public B_EDU_School()
        {
            TbName = initMod.TbName;
            PK = initMod.PK;
        }
        public int Insert(M_EDU_School model)
        {
            return DBCenter.Insert(model);
        }

        public bool UpdateByID(M_EDU_School model)
        {
            return DBCenter.UpdateByID(model,model.ID);
        }
        public DataTable SelByUid(int uid)
        {
            string sql = "SELECT * FROM " + TbName + " WHERE UserID=" + uid;
            return SqlHelper.ExecuteTable(CommandType.Text, sql);
        }
        /// <summary>
        /// 获取指定用户,和其所属的班级的共享课程表
        /// </summary>
        public DataTable SelByUidAndClass(int uid)
        {
            //获取自己所属于哪些班级
            B_ClassRoom classBll = new B_ClassRoom();
            DataTable classDT = classBll.SelByUid(uid);
            string where = "";
            for (int i = 0; i < classDT.Rows.Count; i++)
            {
                int rid = Convert.ToInt32(classDT.Rows[i]["RoomID"]);
                if (i == 0) { where += " CHARINDEX('," + rid + ",',','+A.ClassIDS+',')>0 "; }
                else { where += " OR CHARINDEX('," + rid + ",',','+A.ClassIDS+',')>0 "; }
            }
            if (!string.IsNullOrEmpty(where))
            {
                where = " OR(A.ClassIDS IS NOT NULL AND (" + where + "))";
            }
            return SqlHelper.JoinQuery("A.*,B.Salt AS Alias,B.UserName", TbName, "ZL_User", "A.UserID=B.UserID", "A.UserID=" + uid + where, "A.CDate DESC");
        }

        public bool Del(int ID)
        {
            return Sql.Del(TbName, ID);
        }

        public M_EDU_School SelMyConfig()
        {
            using (DbDataReader reader = Sql.SelReturnReader(TbName, " WHERE 1=1"))
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
        public M_EDU_School SelReturnModel(int ID)
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
            return Sql.Sel(TbName);
        }
        public PageSetting SelPage(int cpage, int psize)
        {
            PageSetting setting = PageSetting.Single(cpage, psize, TbName, PK, "");
            DBCenter.SelPage(setting);
            return setting;
        }
    }
}
