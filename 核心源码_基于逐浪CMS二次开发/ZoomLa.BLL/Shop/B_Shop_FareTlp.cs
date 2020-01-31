using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Text;
using ZoomLa.Model.Shop;
using ZoomLa.Safe;
using ZoomLa.SQLDAL;
using ZoomLa.SQLDAL.SQL;

namespace ZoomLa.BLL.Shop
{
    public class B_Shop_FareTlp : ZL_Bll_InterFace<M_Shop_FareTlp>
    {
        public string TbName, PK;
        public M_Shop_FareTlp initMod = new M_Shop_FareTlp();
        public B_Shop_FareTlp()
        {
            TbName = initMod.TbName;
            PK = initMod.PK;
        }
        public int Insert(M_Shop_FareTlp model)
        {
            return DBCenter.Insert(model);
        }
        public bool UpdateByID(M_Shop_FareTlp model)
        {
            return DBCenter.UpdateByID(model, model.ID);
        }
        public bool Del(int ID)
        {
            return DBCenter.Del(TbName, PK, ID);
        }
        public void DelByIds(string ids)
        {
            SafeSC.CheckIDSEx(ids);
            DBCenter.DelByIDS(TbName, PK, ids);
        }
        public M_Shop_FareTlp SelReturnModel(int ID)
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
        public DataTable U_SelByUid(int uid)
        {
            return DBCenter.Sel(TbName, "UserID=" + uid);
        }
        public DataTable Sel()
        {
            return DBCenter.Sel(TbName);
        }
        public PageSetting SelPage(int cpage, int psize, Com_Filter filter)
        {
            string where = "1=1 ";
            if (!string.IsNullOrEmpty(filter.uids)) { SafeSC.CheckIDSEx(filter.uids); where += " AND UserID IN (" + filter.uids + ")"; }
            PageSetting setting = PageSetting.Single(cpage, psize, TbName, PK, where);
            DBCenter.SelPage(setting);
            return setting;
        }
        public void DelByIDS(string ids, int uid = 0)
        {
            if (string.IsNullOrEmpty(ids)) return;
            SafeSC.CheckIDSEx(ids);
            string where = PK + " IN (" + ids + ") ";
            if (uid > 0) { where += " AND UserID=" + uid; }
            DBCenter.DelByWhere(TbName, where);
        }
    }
}
