﻿using System;
using System.Data;
using ZoomLa.Model;
using ZoomLa.Common;
using ZoomLa.SQLDAL;
using System.Data.SqlClient;
using ZoomLa.SQLDAL.SQL;
using System.Data.Common;
namespace ZoomLa.BLL
{


    public class B_Subscribe
    {
        public string strTableName,PK;
        private M_Subscribe initMod = new M_Subscribe();
        public B_Subscribe()
        {
            strTableName = initMod.TbName;
            PK = initMod.PK;
        }
        public DataTable Sel(int ID)
        {
            return Sql.Sel(strTableName, PK, ID);
        }
        public M_Subscribe SelReturnModel(int ID)
        {
            using (DbDataReader reader = Sql.SelReturnReader(strTableName, PK, ID))
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
            return Sql.Sel(strTableName);
        }
        public PageSetting SelPage(int cpage, int psize)
        {
            PageSetting setting = PageSetting.Single(cpage, psize, strTableName, PK, "");
            DBCenter.SelPage(setting);
            return setting;
        }
        public bool UpdateByID(M_Subscribe model)
        {
            return DBCenter.UpdateByID(model,model.ID);
        }
        public bool Del(int ID)
        {
            return Sql.Del(strTableName, ID);
        }
        public int insert(M_Subscribe model)
        {
            return DBCenter.Insert(model);
        }
        /// <summary>
        ///添加记录
        /// </summary>
        /// <param name="Subscribe"></param>
        /// <returns></returns>
        public bool GetInsert(M_Subscribe model)
        {
            return insert(model)>0;
        }

        /// <summary>
        ///更新记录
        /// </summary>
        /// <param name="Subscribe"></param>
        /// <returns></returns>
        public bool GetUpdate(M_Subscribe model)
        {
            return UpdateByID(model);
        }

        /// <summary>
        ///不存在则添加否则更新
        /// </summary>
        /// <param name="Subscribe"></param>
        /// <returns></returns>
        public bool InsertUpdate(M_Subscribe model)
        {
            if (model.ID > 0)
                return UpdateByID(model);
            else
                return insert(model)>0;
        }

        /// <summary>
        ///删除记录
        /// </summary>
        /// <param name="Subscribe"></param>
        /// <returns></returns>
        public bool GetDelete(int ID)
        {
            return Sql.Del(strTableName, PK + "=" + ID);
        }


        /// <summary>
        ///查找一条记录
        /// </summary>
        /// <param name="Subscribe"></param>
        /// <returns></returns>
        public M_Subscribe GetSelect(int ID)
        {
            return SelReturnModel(ID);
        }

        /// <summary>
        ///返回所有记录
        /// </summary>
        /// <returns></returns>
        public DataTable Select_All()
        {
            return Sql.Sel(strTableName);
        }

    }
}
