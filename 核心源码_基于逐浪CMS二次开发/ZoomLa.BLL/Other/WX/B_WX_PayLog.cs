using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using ZoomLa.Model.Other;
using ZoomLa.SQLDAL;
using ZoomLa.SQLDAL.SQL;

namespace ZoomLa.BLL.Other
{
    public class B_WX_PayLog
    {
        private M_WX_PayLog initMod = new M_WX_PayLog();
        public string TbName = "", PK = "";
        public B_WX_PayLog()
        {
            TbName = initMod.TbName;
            PK = initMod.PK;
        }
        public DataTable Sel()
        {
            return DBCenter.Sel(TbName);
        }
        //public DataTable Sel(int AppID)
        //{
        //    string where = "1=1";
        //    if (AppID > 0) { where += " AND A.Appid="+AppID; }
        //    return DBCenter.JoinQuery("A.*,B.Alias", TbName, "ZL_WX_APPID", "A.Appid=B.ID", where, "A.ID DESC");
        //}

        public M_WX_PayLog SelReturnModel(int ID)
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
        public int Insert(M_WX_PayLog model)
        {
            return DBCenter.Insert(model);
        }
        public bool UpdateByID(M_WX_PayLog model)
        {
            return DBCenter.UpdateByID(model, model.ID);
        }
        public bool Del(int ID)
        {
            return DBCenter.Del(TbName, PK, ID);
        }
        public PageSetting SelPage(int cpage, int psize, Com_Filter filter)
        {
            string where = "1=1 ";
            if (filter.storeId > 0)
            {
                where += " AND A.Appid=" + filter.storeId;
            }
            string fields = "A.*,(SELECT Alias FROM ZL_WX_APPID WHERE ID=A.AppId) AS Alias";
            PageSetting setting = PageSetting.Single(cpage, psize, TbName, PK, where, "A.ID DESC", null, fields);
            DBCenter.SelPage(setting);
            return setting;
        }
        /// <summary>
        /// 获取支付类型
        /// </summary>
        public string GetPayType(int type)
        {
            return "企业付款";
        }
    }
}
