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
    public class B_Order_Back
    {
        private M_Order_Back initMod = new M_Order_Back();
        public string TbName = "", PK = "";
        public B_Order_Back()
        {
            TbName = initMod.TbName;
            PK = initMod.PK;
        }
        public DataTable Sel()
        {
            return DBCenter.Sel(TbName);
        }
        public M_Order_Back SelReturnModel(int ID)
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
        public int Insert(M_Order_Back model)
        {
            return DBCenter.Insert(model);
        }
        public bool UpdateByID(M_Order_Back model)
        {
            return DBCenter.UpdateByID(model, model.ID);
        }
        public void Del(string ids)
        {
            if (string.IsNullOrEmpty(ids)) { return; }
            SafeSC.CheckIDSEx(ids);
            DBCenter.DelByIDS(TbName, PK, ids);
        }
        public PageSetting SelPage(int cpage, int psize)
        {
            string where = "1=1 ";
            List<SqlParameter> sp = new List<SqlParameter>();
            PageSetting setting = PageSetting.Single(cpage, psize, TbName, PK, where, PK + " DESC", sp);
            DBCenter.SelPage(setting);
            return setting;
        }
        //----------------
        public string GetZStatus(int status)
        {
            switch (status)
            {
                case 1:
                    return "<span style='color:green;'>已同意</sppan>";
                case -1:
                    return "<span style='color:red;'>已拒绝</span>";
                default:
                    return "待处理";
            }
        }
    }
}
