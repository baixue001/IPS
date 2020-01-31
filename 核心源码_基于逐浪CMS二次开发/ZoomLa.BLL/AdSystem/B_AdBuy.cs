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
    public class B_AdBuy
    {
        private M_AdBuy initMod = new M_AdBuy();
        public string TbName = "", PK = "";
        public B_AdBuy()
        {
            TbName = initMod.TbName;
            PK = initMod.PK;
        }
        public DataTable Sel()
        {
            return DBCenter.Sel(TbName, "", PK + " DESC");
        }
        public M_AdBuy SelReturnModel(int ID)
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
        public int Insert(M_AdBuy model)
        {
            return DBCenter.Insert(model);
        }
        public bool UpdateByID(M_AdBuy model)
        {
            return DBCenter.UpdateByID(model, model.ID);
        }
        public void Del(string ids,int uid=-100)
        {
            if (string.IsNullOrEmpty(ids)) { return; }
            SafeSC.CheckIDSEx(ids);
            string where = "ID IN ("+ids+") ";
            if (uid != -100) { where += " AND UID="+uid; }
            DBCenter.DelByWhere(TbName,where);
        }
        public PageSetting SelPage(int cpage, int psize, Com_Filter filter)
        {
            string where = "1=1 ";
            List<SqlParameter> sp = new List<SqlParameter>();
            if (!string.IsNullOrEmpty(filter.uids))
            {
                SafeSC.CheckIDSEx(filter.uids);
                where += " AND A.UID IN (" + filter.uids + ")";
            }
            if (!string.IsNullOrEmpty(filter.status))
            {
                SafeSC.CheckIDSEx(filter.status);
                where += " AND A.State IN (" + filter.status + ")";
            }
            PageSetting setting = PageSetting.Double(cpage, psize, TbName, "ZL_AdZone", "A.ID", "A.ADID=B.ZoneID", where, PK + " DESC", sp);
            DBCenter.SelPage(setting);
            return setting;
        }
        public void Audit(string ids, int audit)
        {
            if (string.IsNullOrEmpty(ids)) { return; }
            SafeSC.CheckIDSEx(ids);
            DBCenter.UpdateSQL(TbName, "Audit=" + audit, "ID IN (" + ids + ")");
        }
        public void Change(string ids, int status,int uid=-100)
        {
            if (string.IsNullOrEmpty(ids)) { return; }
            SafeSC.CheckIDSEx(ids);
            string where = "ID IN (" + ids + ")";
            if (uid != -100) { where += " AND UID=" + uid; }
            DBCenter.UpdateSQL(TbName, "State=" + status, where);
        }
    }
}
