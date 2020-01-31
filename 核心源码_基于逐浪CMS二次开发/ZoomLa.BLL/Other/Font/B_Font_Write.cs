using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using ZoomLa.Common;
using ZoomLa.Model;
using ZoomLa.Model.Font;
using ZoomLa.SQLDAL;

namespace ZoomLa.BLL.Font
{
    public class B_Font_Write
    {
        private M_Font_Write initMod = new M_Font_Write();
        public string TbName = "", PK = "";
        public string Dir { get { return "/CMSPlugins/WebFontPlugin/Views/"; } }
        public B_Font_Write()
        {
            TbName = initMod.TbName;
            PK = initMod.PK;
        }
        public DataTable Sel()
        {
            return DBCenter.Sel(TbName);
        }
        public DataTable Sel(int uid, string skey)
        {
            string where = "1=1 ";
            List<SqlParameter> sp = new List<SqlParameter>();
            if (uid != -100) { where += " AND UserID=" + uid; }
            if (!string.IsNullOrEmpty(skey))
            {
                where += " AND FontName LIKE @skey";
                sp.Add(new SqlParameter("skey", "%" + skey + "%"));
            }
            return DBCenter.Sel(TbName, where, PK + " DESC", sp);
        }
        public M_Font_Write SelReturnModel(int ID)
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
        public int Insert(M_Font_Write model)
        {
            return DBCenter.Insert(model);
        }
        public bool UpdateByID(M_Font_Write model)
        {
            return DBCenter.UpdateByID(model, model.ID);
        }
        public void Del(string ids)
        {
            if (string.IsNullOrEmpty(ids)) { return; }
            SafeSC.CheckIDSEx(ids);
            DBCenter.DelByIDS(TbName, PK, ids);
        }
        //-----------------------Tools
        public bool IsExistFontZip(string dir)
        {
            string vpath = dir + "font.zip";
            return File.Exists(function.VToP(vpath));
        }
    }
}
