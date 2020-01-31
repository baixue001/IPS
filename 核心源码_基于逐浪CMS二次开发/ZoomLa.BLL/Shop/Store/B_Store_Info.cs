using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using ZoomLa.BLL.Shop;
using ZoomLa.Model;
using ZoomLa.Model.Content;
using ZoomLa.Model.Shop;
using ZoomLa.SQLDAL;
using ZoomLa.SQLDAL.SQL;

namespace ZoomLa.BLL.Shop
{
    public class B_Store_Info
    {
        private M_Store_Info initMod = new M_Store_Info();
        private string TbName, PK;
        public B_Store_Info()
        {
            TbName = initMod.TbName;
            PK = initMod.PK;
        }
        public PageSetting SelPage(int cpage, int psize, F_StoreInfo filter)
        {
            string fields = "A.GeneralID,A.ItemID,A.Title,A.Status,A.CreateTime,B.*";
            string where = "A.TableName='ZL_Store_Reg' ";
            List<SqlParameter> sp = new List<SqlParameter>();
            if (filter.sid != -100) { where += " AND A.GeneralID=" + filter.sid; }
            if (filter.uid != -100) { where += " AND B.UserID=" + filter.uid; }
            if (filter.status != -100) { where += " AND A.Status=" + filter.status; }

            if (!string.IsNullOrEmpty(filter.uname)) { where += " AND B.UserName=@uname"; sp.Add(new SqlParameter("uname", filter.uname)); }
            PageSetting setting = PageSetting.Double(cpage, psize, initMod.TbName, initMod.STbName, "A.GeneralID", "A.ItemID=B.ID", where, "A.GeneralID DESC", sp, fields);
            DBCenter.SelPage(setting);
            return setting;
        }
        public M_Store_Info SelReturnModel(int id)
        {
            DataTable dt = SelPage(1, 1, new F_StoreInfo() { sid = id }).dt;
            if (dt.Rows.Count < 1) { return null; }
            else { return initMod.GetModelFromReader(dt.Rows[0]); }
        }
        public M_Store_Info SelModelByUser(int uid)
        {
            DataTable dt = SelPage(1, 1, new F_StoreInfo() { uid = uid }).dt;
            if (dt.Rows.Count < 1) { return null; }
            else { return initMod.GetModelFromReader(dt.Rows[0]); }
        }
        public void Del(int sid)
        {
            int itemID = DataConvert.CLng(DBCenter.ExecuteScala(initMod.TbName, "ItemID", "GeneralID=" + sid));
            DBCenter.Del(initMod.TbName, "GeneralID", sid);
            DBCenter.Del(initMod.STbName, "ID", itemID);
        }
        //public bool UpdateByID(M_Shop_DeliveryMan model)
        //{
        //    return DBCenter.UpdateByID(model, model.ID);
        //}
        //----------------------------------
        /// <summary>
        /// 填充dt,用于写入数据库附表
        /// </summary>
        public DataTable FillDT(M_CommonData storeMod, DataTable table)
        {
            {
                DataRow dr = table.NewRow();
                dr[0] = "UserID";
                dr[1] = "int";
                dr[2] = storeMod.SuccessfulUserID;
                table.Rows.Add(dr);
            }
            {
                DataRow dr = table.NewRow();
                dr[0] = "UserName";
                dr[1] = "TextType";
                dr[2] = storeMod.Inputer;
                table.Rows.Add(dr);
            }
            {
                DataRow dr = table.NewRow();
                dr[0] = "StoreModelID";
                dr[1] = "int";
                dr[2] = storeMod.ModelID;
                table.Rows.Add(dr);
            }
            {
                DataRow dr = table.NewRow();
                dr[0] = "StoreName";
                dr[1] = "TextType";
                dr[2] = storeMod.Title;
                table.Rows.Add(dr);
            }
            return table;
        }
        public static CommonReturn CheckStore(int uid)
        {
            B_Store_Info storeBll = new B_Store_Info();
            if (uid < 1) { return CommonReturn.Failed("用户未登录"); }
            M_Store_Info storeMod = storeBll.SelModelByUser(uid);
            string err = "";
            if (storeMod == null || storeMod.ID < 1) { err = "店铺信息不存在"; }
            else if (storeMod.Status != (int)ZLEnum.ConStatus.Audited) { err = "店铺没有经过审核"; }
            if (!string.IsNullOrEmpty(err))
            {
                return CommonReturn.Failed(err);
            }
            else {return CommonReturn.Success(storeMod); }
        }
    }
    public class F_StoreInfo
    {
        /// <summary>
        /// 店铺ID
        /// </summary>
        public int sid = -100;
        public int uid = -100;
        public string uname = "";
        public int status = -100;
    }
}
