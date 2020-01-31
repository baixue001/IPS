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
    public class B_Order_InvTlp
    {
        private M_Order_Invoice initMod = new M_Order_Invoice();
        public string TbName = "", PK = "";
        public int MaxCount = 6;
        public B_Order_InvTlp()
        {
            TbName = "ZL_Order_InvTlp";
            PK = initMod.PK;
        }
        public DataTable Sel()
        {
            return DBCenter.Sel(TbName, "", PK + " DESC");
        }
        public M_Order_Invoice SelReturnModel(int ID)
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
        public int Insert(M_Order_Invoice model)
        {
            model.TbName = TbName;
            return DBCenter.Insert(model);
        }
        public bool UpdateByID(M_Order_Invoice model)
        {
            model.TbName = TbName;
            return DBCenter.UpdateByID(model, model.ID);
        }
        public void Del(string ids,int uid=0)
        {
            if (string.IsNullOrEmpty(ids)) { return; }
            SafeSC.CheckIDSEx(ids);
            string where = "ID IN ("+ids+")";
            if (uid > 0) { where += " AND UserID="+uid; }
            DBCenter.DelByWhere(TbName,where);
        }
        public PageSetting SelPage(int cpage, int psize,int uid)
        {
            string where = "1=1 ";
            where += " AND UserID="+uid;
            List<SqlParameter> sp = new List<SqlParameter>();
            PageSetting setting = PageSetting.Single(cpage, psize, TbName, PK, where, PK + " DESC", sp);
            DBCenter.SelPage(setting);
            return setting;
        }
        //-------------
        public void Sync(M_Order_Invoice invMod)
        {
            if (invMod.UserID < 1 || string.IsNullOrEmpty(invMod.InvHead)) { return; }
            //数量超过5则不处理
            int count = DBCenter.Count(TbName, "UserID=" + invMod.UserID);
            if (count >= MaxCount) { return; }
            //head已存在则不处理(不更新)
            invMod.InvHead = invMod.InvHead.Replace(" ","");
            bool exist = DBCenter.IsExist(TbName, "InvHead=@head AND UserID=" + invMod.UserID,
                new List<SqlParameter>() { new SqlParameter("head", invMod.InvHead) });
            if (exist) { return; }
            //同步写入
            Insert(invMod);
        }
    }
}
