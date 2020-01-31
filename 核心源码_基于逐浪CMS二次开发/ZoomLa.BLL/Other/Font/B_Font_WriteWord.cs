using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Text;
using ZoomLa.Common;
using ZoomLa.Model;
using ZoomLa.Model.Font;
using ZoomLa.SQLDAL;
using ZoomLa.SQLDAL.SQL;

namespace ZoomLa.BLL.Font
{
    public class B_Font_WriteWord
    {
        private M_Font_WriteWord initMod = new M_Font_WriteWord();
        public string TbName = "", PK = "";
        public B_Font_WriteWord()
        {
            TbName = initMod.TbName;
            PK = initMod.PK;
        }
        public DataTable Sel()
        {
            return DBCenter.Sel(TbName);
        }
        public M_Font_WriteWord SelReturnModel(int ID)
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
        public int Insert(M_Font_WriteWord model)
        {
            return DBCenter.Insert(model);
        }
        public bool UpdateByID(M_Font_WriteWord model)
        {
            return DBCenter.UpdateByID(model, model.ID);
        }
        public void Del(string ids)
        {
            DBCenter.DelByIDS(TbName, PK, ids);
        }
        public DataTable SelPage(int cpage, int psize, int wid, int status)
        {
            string where = "1=1";
            where += " AND WriteID=" + wid;
            if (status != -100) { where += " AND ZStatus=" + status; }
            PageSetting setting = PageSetting.Single(cpage, psize, TbName, PK, where, PK + " DESC");
            return DBCenter.SelPage(setting);
        }
        //--------------------------Tools
        public int SelNextWord(int writeid)
        {
            string where = "WriteID=" + writeid + " AND ZStatus=" + (int)ZLEnum.ConStatus.UnAudit;
            return DataConvert.CLng(DBCenter.ExecuteScala(TbName, "ID", where, PK + " ASC"));
        }
    }
}
