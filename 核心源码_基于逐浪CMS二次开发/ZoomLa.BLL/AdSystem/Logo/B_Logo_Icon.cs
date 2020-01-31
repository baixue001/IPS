using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using ZoomLa.Model.AdSystem;
using ZoomLa.SQLDAL;
using ZoomLa.SQLDAL.SQL;

namespace ZoomLa.BLL.AdSystem
{
    public class B_Logo_Icon
    {
        private M_Logo_Icon initMod = new M_Logo_Icon();
        public string TbName = "", PK = "";
        public string PlugDir = "/Plugins/Third/Logo/";
        public B_Logo_Icon()
        {
            TbName = initMod.TbName;
            PK = initMod.PK;
        }
        public DataTable Sel()
        {
            return DBCenter.Sel(TbName,"",PK+" DESC");
        }
        public M_Logo_Icon SelReturnModel(int ID)
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
        public int Insert(M_Logo_Icon model)
        {
            return DBCenter.Insert(model);
        }
        public bool UpdateByID(M_Logo_Icon model)
        {
            return DBCenter.UpdateByID(model, model.ID);
        }
        public void Del(string ids)
        {
            if (string.IsNullOrEmpty(ids)) { return; }
            SafeSC.CheckIDSEx(ids);
            DBCenter.DelByIDS(TbName, PK, ids);
        }
        /// <summary>
        /// 删除数据库记录与文件
        /// </summary>
        public void RealDel(string ids)
        {
            if (string.IsNullOrEmpty(ids)) { return; }
            SafeSC.CheckIDSEx(ids);
            DataTable dt = DBCenter.Sel(TbName, "ID IN (" + ids + ")");
            foreach (DataRow dr in dt.Rows)
            {
                try
                {
                    SafeSC.DelFile(DataConvert.CStr(dr["VPath"]));
                }
                catch (Exception ex) { ZLLog.L("logo_icon_del:" + ex.Message); }
            }
            Del(ids);
        }
        public PageSetting SelPage(int cpage, int psize)
        {
            string where = "1=1 ";
            List<SqlParameter> sp = new List<SqlParameter>();
            PageSetting setting = PageSetting.Single(cpage, psize, TbName, PK, where, PK + " DESC", sp);
            DBCenter.SelPage(setting);
            return setting;
        }
        //-----------------Tools
        public string GetSvgPath(int id)
        {
            return PlugDir + "assets/icons/" + id + ".svg";
        }
    }
}
