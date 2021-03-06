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


    /// <summary>
    /// B_RoomActive 的摘要说明
    /// </summary>
    public class B_RoomActive
    {
        private string TbName, PK;
        private M_RoomActive initMod = new M_RoomActive();
        public B_RoomActive()
        {
            TbName = initMod.TbName;
            PK = initMod.PK;
        }
        public DataTable Sel(int ID)
        {
            return Sql.Sel(TbName, PK, ID);
        }
        public M_RoomActive SelReturnModel(int ID)
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
        private M_RoomActive SelReturnModel(string strWhere)
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
        public bool UpdateByID(M_RoomActive model)
        {
            return DBCenter.UpdateByID(model, model.ActiveUserID);
        }
        public bool Del(int ID)
        {
            return Sql.Del(TbName, ID);
        }
        public int insert(M_RoomActive model)
        {
            return DBCenter.Insert(model);
        }
        public bool GetInsert(M_RoomActive model)
        {
             return DBCenter.Insert(model) > 0;
        }
        public bool GetUpdate(M_RoomActive model)
        {
            return DBCenter.UpdateByID(model,model.AID);
        }
        public bool InsertUpdate(M_RoomActive model)
        {
            if (model.ActiveUserID > 0)
                GetUpdate(model);
            else
                GetInsert(model);
            return true;
        }
        public bool GetDelete(int ID)
        {
            return Sql.Del(TbName, PK + "=" + ID);
        }
        public M_RoomActive GetSelect(int ID)
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
            strSql += "ZL_RoomActive";
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
    }
}
