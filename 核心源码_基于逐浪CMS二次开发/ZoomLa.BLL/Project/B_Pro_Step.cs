﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using ZoomLa.Model.Project;
using ZoomLa.SQLDAL;
using ZoomLa.SQLDAL.SQL;

namespace ZoomLa.BLL.Project
{
    public class B_Pro_Step
    {
        private string PK, TbName;
        private M_Pro_Step initMod = new M_Pro_Step();
        public B_Pro_Step() 
        {
            PK = initMod.PK;
            TbName = initMod.TbName;
        }
        public int Insert(M_Pro_Step model)
        {
           return DBCenter.Insert(model);
        }
        public bool UpdateByID(M_Pro_Step model)
        {
            return DBCenter.UpdateByID(model,model.ID);
        }
        public bool Del(int ID)
        {
            return Sql.Del(TbName, ID);
        }
        public M_Pro_Step SelReturnModel(int ID)
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
        public M_Pro_Step SelModelByProID(int proid)
        {
            using (DbDataReader reader = Sql.SelReturnReader(TbName, "ProID", proid))
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
        /// <summary>
        /// 查看发布的项目
        /// </summary>
        /// <param name="uid"></param>
        /// <returns></returns>
        public DataTable SelByUid(int uid) 
        {
            return SqlHelper.ExecuteTable("SELECT * FROM "+TbName+" WHERE CUser="+uid+" ORDER BY CDate DESC");
        }
        /// <summary>
        /// 查看参与的项目
        /// </summary>
        /// <param name="uid"></param>
        /// <returns></returns>
        public DataTable SelByParUser(int uid)
        {
            return SqlHelper.ExecuteTable("SELECT * FROM " + TbName + " WHERE Partic LIKE '," + uid + ",' ORDER BY CDate DESC");
        }
        public int GetOrderID(int proid)
        {
            DataTable dt = SqlHelper.ExecuteTable("SELECT COUNT(ID) FROM " + TbName + " WHERE ProID=" + proid);
            return Convert.ToInt32(dt.Rows[0][0]) + 1;
        }
    }
}
