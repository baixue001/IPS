using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using ZoomLa.Model.Shop;
using ZoomLa.SQLDAL;
using ZoomLa.SQLDAL.SQL;

namespace ZoomLa.BLL.Shop
{
    public class B_Shop_DeliveryMan
    {
        private M_Shop_DeliveryMan initMod = new M_Shop_DeliveryMan();
        private string TbName, PK;
        public B_Shop_DeliveryMan()
        {
            TbName = initMod.TbName;
            PK = initMod.PK;
        }
        public bool Del(int ID)
        {
            return DBCenter.Del(TbName, PK, ID);
        }
        public void DelByIDS(string ids, int storeid = -100)
        {
            if (string.IsNullOrEmpty(ids)) return;
            SafeSC.CheckIDSEx(ids);
            string where = "ID IN (" + ids + ") ";
            if (storeid != -100) { where += " AND StoreID=" + storeid; }
            DBCenter.DelByWhere(TbName, where);
        }
        public int Insert(M_Shop_DeliveryMan model)
        {
            return DBCenter.Insert(model);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="userid">用户ID</param>
        /// <param name="skey">店铺名或店铺ID</param>
        /// <returns></returns>
        public DataTable Sel(string skey = "")
        {
            string where = "1=1";
            List<SqlParameter> sp = new List<SqlParameter>();
            if (!string.IsNullOrEmpty(skey))
            {
                where += " AND (B.Title LIKE @skey";
                sp.Add(new SqlParameter("skey", "%" + skey + "%"));
                if (DataConvert.CLng(skey) > 0)
                {
                    where += " OR A.StoreID=" + DataConvert.CLng(skey);
                }
                where += ")";
            }
            return DBCenter.JoinQuery("A.*,B.Title,(SELECT UserName FROM ZL_User WHERE UserID = A.UserID) UserName", TbName, "ZL_CommonModel", "A.StoreID=B.GeneralID", where, "ID DESC", sp.ToArray());
        }
        public DataTable Sel(int userid, string auth = "")
        {
            string where = "UserID=" + userid;
            List<SqlParameter> sp = new List<SqlParameter>();
            if (!string.IsNullOrEmpty(auth)) { where += " AND Auth LIKE @auth"; sp.Add(new SqlParameter("auth", "%" + auth + "%")); }
            return DBCenter.Sel(TbName, where, "", sp);
        }
        public PageSetting SelPage(int cpage, int psize, int storeID = 0)
        {
            string where = "1=1";
            if (storeID > 0) { where += " AND StoreID=" + storeID; }
            PageSetting setting = PageSetting.Double(cpage, psize, TbName, "ZL_User", "A." + PK, "A.UserID=B.UserID", where);
            setting.fields = "A.*,B.UserName,B.HoneyName";
            DBCenter.SelPage(setting);
            return setting;
        }
        public M_Shop_DeliveryMan SelReturnModel(int ID)
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
        public M_Shop_DeliveryMan SelModelByUid(int uid, int storeid)
        {
            using (DbDataReader reader = DBCenter.SelReturnReader(TbName, "UserID=" + uid + " AND StoreID=" + storeid))
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
        public bool UpdateByID(M_Shop_DeliveryMan model)
        {
            return DBCenter.UpdateByID(model, model.ID);
        }
        public string GetAuth(string auth)
        {
            if (string.IsNullOrEmpty(auth)) { return ""; }
            string result = "";
            string[] arr = auth.Split(',');
            foreach(string str in arr)
            {
                switch (str)
                {
                    case "settle":
                        result += "结清";
                        break;
                }
            }
            return result.Trim(',');
        }
    }
}
