using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Web;
using ZoomLa.BLL.Helper;
using ZoomLa.Model.Client;
using ZoomLa.SQLDAL;
using ZoomLa.SQLDAL.SQL;

namespace ZoomLa.BLL.Client
{
    public class B_CRMS_Client
    {
        private M_CRMS_Client initMod = new M_CRMS_Client();
        public string TbName = "", PK = "";
        public B_CRMS_Client()
        {
            TbName = initMod.TbName;
            PK = initMod.PK;
        }
        public DataTable Sel()
        {
            return DBCenter.Sel(TbName, "", PK + " DESC");
        }
        public M_CRMS_Client SelReturnModel(int ID)
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
        public M_CRMS_Client SelModelByUid(int uid)
        {
            using (DbDataReader reader = DBCenter.SelReturnReader(TbName, "FUserID=" + uid))
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
        public int Insert(M_CRMS_Client model)
        {
            return DBCenter.Insert(model);
        }
        public int Insert(M_CRMS_Client model, DataTable table)
        {
            int itemid = 0;
            if (!string.IsNullOrEmpty(model.ModelTable) && table.Rows.Count > 0)
            {
                itemid = DBCenter.Insert(model.ModelTable, BLLCommon.GetFields(table), BLLCommon.GetParas(table), BLLCommon.GetParameters(table).ToArray());
            }
            model.ItemID = itemid;
            return DBCenter.Insert(model);
        }
        public bool UpdateByID(M_CRMS_Client model, DataTable table)
        {

            int ItemID = model.ItemID;
            if (table != null && table.Rows.Count > 0)
            {
                List<SqlParameter> splist = new List<SqlParameter>();
                splist.AddRange(BLLCommon.GetParameters(table));
                DBCenter.UpdateSQL(model.ModelTable, BLLCommon.GetFieldAndPara(table), "ID=" + ItemID, splist);
            }
            return DBCenter.UpdateByID(model, model.ID);
        }
        public bool UpdateByID(M_CRMS_Client model)
        {
            return DBCenter.UpdateByID(model, model.ID);
        }
        public void Del(string ids)
        {
            if (string.IsNullOrEmpty(ids)) { return; }
            SafeSC.CheckIDSEx(ids);
            DBCenter.DelByIDS(TbName, PK, ids);
        }
        public PageSetting SelPage(int cpage, int psize, F_CRMS_Client filter)
        {
            List<SqlParameter> sp = new List<SqlParameter>();
            string where = "1=1 ";
            if (!string.IsNullOrEmpty(filter.ctype))
            {
                filter.ctype =HttpUtility.UrlDecode(filter.ctype);
                sp.Add(new SqlParameter("ctype", "%," + filter.ctype + ",%"));
                sp.Add(new SqlParameter("ctype2",filter.ctype));
                where += " AND (ClientType LIKE @ctype OR ClientType=@ctype2)";
            }
            if (!string.IsNullOrEmpty(filter.ignoreCids))
            {
                SafeSC.CheckIDSEx(filter.ignoreCids);
                where += " AND ID NOT IN (" + filter.ignoreCids + ")";
            }
            if (!string.IsNullOrEmpty(filter.uids) && !filter.uids.Equals("0"))
            {
                SafeSC.CheckIDSEx(filter.uids);
                where += " AND CUserID IN (" + filter.uids + ")";
            }
            PageSetting setting = PageSetting.Single(cpage, psize, TbName, PK, where, PK + " DESC", sp);
            DBCenter.SelPage(setting);
            return setting;
        }
    }
    public class F_CRMS_Client
    {
        /// <summary>
        /// 中文名称
        /// </summary>
        public string ctype = "";
        public string ignoreCids = "";
        //用户创建筛选
        public string uids = "";
        //跟进人筛选
        public string fuids = "";
        //管理员创建筛选
        public string adminIds = "";
        public string skey = "";
    }
}
