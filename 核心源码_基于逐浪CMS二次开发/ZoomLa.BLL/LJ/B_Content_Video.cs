﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using ZoomLa.Model;
using ZoomLa.SQLDAL;
using ZoomLa.SQLDAL.SQL;
using System.Data.Common;

namespace ZoomLa.BLL
{
    public class B_Content_Video:ZL_Bll_InterFace<M_Content_Video>
    {
        public M_Content_Video initMod = new M_Content_Video();
        public string TbName, PK;
        public B_Content_Video()
        {
            TbName = initMod.TbName;
            PK = initMod.PK;
        }
        public int Insert(M_Content_Video model)
        {
            return DBCenter.Insert(model);
        }

        public bool UpdateByID(M_Content_Video model)
        {
            return DBCenter.UpdateByID(model,model.ID);
        }

        public bool Del(int ID)
        {
            return Sql.Del(TbName,ID);
        }

        public M_Content_Video SelReturnModel(int ID)
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
            return Sql.Sel(TbName,"","CDate DESC");
        }
        public PageSetting SelPage(int cpage, int psize)
        {
            PageSetting setting = PageSetting.Single(cpage, psize, TbName, PK, "");
            DBCenter.SelPage(setting);
            return setting;
        }
        //所有视频数据(以文件形式)
        public DataTable SelForFile()
        {
            string sql = "SELECT VName AS Name,Thumbnail AS Path,CDate AS CreateTime,VPath AS FilePath FROM " + TbName;
            return SqlHelper.ExecuteTable(CommandType.Text, sql);
        }
    }
}
