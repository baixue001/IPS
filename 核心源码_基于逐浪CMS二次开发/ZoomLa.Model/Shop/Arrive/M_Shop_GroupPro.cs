using System;using System.Data.Common;
using System.Collections.Generic;

using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace ZoomLa.Model.Shop
{
    public class M_Shop_GroupPro : M_Base
    {
        public M_Shop_GroupPro() { ProIDS = ""; }
        public int ID { get; set; }
        /// <summary>
        /// 组合名称
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 商品IDS(只是做为记录,实际值应该取ParentID=其的值)
        /// </summary>
        public string ProIDS { get; set; }
        public int AdminID { get; set; }
        public int UserID { get; set; }
        public int StoreID { get; set; }
        public DateTime CDate { get; set; }
        public string Remind { get; set; }
        public override string TbName { get { return "ZL_Shop_GroupPro"; } }
        public override string PK { get { return "ID"; } }

        public override string[,] FieldList()
        {
            string[,] Tablelist = {
        		        		{"ID","Int","4"},
        		        		{"Name","NVarChar","200"},
        		        		{"ProIDS","VarChar","8000"},
        		        		{"AdminID","Int","4"},
        		        		{"CDate","DateTime","8"},
        		        		{"Remind","NVarChar","500"},
                                {"UserID","Int","4"},
                                {"StoreID","Int","4"}
        };
            return Tablelist;
        }

        public override SqlParameter[] GetParameters()
        {
            M_Shop_GroupPro model = this;
            if (model.CDate <= DateTime.MinValue) { model.CDate = DateTime.Now; }
            SqlParameter[] sp = GetSP();
            sp[0].Value = model.ID;
            sp[1].Value = model.Name;
            sp[2].Value = PureIdsForDB(model.ProIDS);
            sp[3].Value = model.AdminID;
            sp[4].Value = model.CDate;
            sp[5].Value = model.Remind;
            sp[6].Value = model.UserID;
            sp[7].Value = model.StoreID;
            return sp;
        }
        public M_Shop_GroupPro GetModelFromReader(DbDataReader rdr)
        {
            M_Shop_GroupPro model = new M_Shop_GroupPro();
            model.ID = ConvertToInt(rdr["ID"]);
            model.Name = ConverToStr(rdr["Name"]);
            model.ProIDS = ConverToStr(rdr["ProIDS"]);
            model.AdminID = ConvertToInt(rdr["AdminID"]);
            model.CDate = ConvertToDate(rdr["CDate"]);
            model.Remind = ConverToStr(rdr["Remind"]);
            model.UserID = ConvertToInt(rdr["UserID"]);
            model.StoreID = ConvertToInt(rdr["StoreID"]);
            rdr.Close();
            return model;
        }
    }
}
