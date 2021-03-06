﻿using System;
using ZoomLa.Model;
using System.Data;
using ZoomLa.SQLDAL;
using System.Data.SqlClient;
using ZoomLa.SQLDAL.SQL;
using System.Data.Common;
namespace ZoomLa.BLL
{


    public class B_Plan
    {
        public B_Plan() 
        {
            strTableName = initMod.TbName;
            PK = initMod.PK;
        }  
        public string strTableName ,PK;
        private M_Plan initMod = new M_Plan();
       
        /// <summary>
        /// 根据ID查询一条记录
        /// </summary>
        public DataTable Sel(int ID)
        {
            return Sql.Sel(strTableName, PK, ID);
        }
        /// <summary>
        /// 根据ID查询一条记录
        /// </summary>
        public M_Plan SelReturnModel(int ID)
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
        /// <summary>
        /// 查询所有记录
        /// </summary>
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
        /// <summary>
        /// 根据ID更新
        /// </summary>
        public bool UpdateByID(M_Plan model)
        {
            return DBCenter.UpdateByID(model,model.ID);
        }

        public bool Del(int ID)
        {
            return Sql.Del(strTableName, ID);
        }
        /// <summary>
        ///添加记录
        /// </summary>
        /// <param name="Sensitivity"></param>
        /// <returns></returns>

        public int insert(M_Plan model)
        {
            return DBCenter.Insert(model);
        } 
    }
}