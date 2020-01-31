using System;using System.Data.Common;
using System.Collections.Generic;

using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace ZoomLa.Model
{
    public class M_User_Level : M_Base
    {
        public M_User_Level()
        {
            ZType = 0;
            ZStatus = 0;
        }
        public int ID { get; set; }
        /// <summary>
        /// 等级名称
        /// </summary>
        public string Alias { get; set; }
        /// <summary>
        /// 等级图片
        /// </summary>
        public string Image { get; set; }
        /// <summary>
        /// 折扣比率
        /// </summary>
        public double DiscountRate { get; set; }
        /// <summary>
        /// 赠送积分比率
        /// </summary>
        public double PointRate { get; set; }
        /// <summary>
        /// 项目灵活使用
        /// </summary>
        public string Addon1 { get; set; }
        /// <summary>
        /// 项目灵活使用
        /// </summary>
        public string Addon2 { get; set; }
        /// <summary>
        /// 所属店铺,为0则全局
        /// </summary>
        public int StoreID { get; set; }
        /// <summary>
        /// 排序
        /// </summary>
        public int OrderID { get; set; }
        /// <summary>
        /// 备注
        /// </summary>
        public string Remark { get; set; }
        public int ZStatus { get; set; }
        public int ZType { get; set; }
        public DateTime CDate { get; set; }
        public override string TbName { get { return "ZL_User_Level"; } }
        public override string PK { get { return "ID"; } }
        public override string[,] FieldList()
        {
            string[,] Tablelist = {
                                {"ID","Int","4"},
                                {"Alias","NVarChar","100"},
                                {"Image","NVarChar","500"},
                                {"DiscountRate","Money","9"},
                                {"PointRate","Money","9"},
                                {"Addon1","NVarChar","100"},
                                {"Addon2","NVarChar","100"},
                                {"StoreID","Int","4"},
                                {"OrderID","Int","4"},
                                {"Remark","NVarChar","500"},
                                {"ZStatus","Int","4"},
                                {"ZType","Int","4"},
                                {"CDate","DateTime","8"}
        };
            return Tablelist;
        }
        public override SqlParameter[] GetParameters()
        {
            M_User_Level model = this;
            SqlParameter[] sp = GetSP();
            if (model.CDate <= DateTime.MinValue) { model.CDate = DateTime.Now; }
            sp[0].Value = model.ID;
            sp[1].Value = model.Alias;
            sp[2].Value = model.Image;
            sp[3].Value = model.DiscountRate;
            sp[4].Value = model.PointRate;
            sp[5].Value = model.Addon1;
            sp[6].Value = model.Addon2;
            sp[7].Value = model.StoreID;
            sp[8].Value = model.OrderID;
            sp[9].Value = model.Remark;
            sp[10].Value = model.ZStatus;
            sp[11].Value = model.ZType;
            sp[12].Value = model.CDate;
            return sp;
        }
        public M_User_Level GetModelFromReader(DbDataReader rdr)
        {
            M_User_Level model = new M_User_Level();
            model.ID = Convert.ToInt32(rdr["ID"]);
            model.Alias = ConverToStr(rdr["Alias"]);
            model.Image = ConverToStr(rdr["Image"]);
            model.DiscountRate = ConverToDouble(rdr["DiscountRate"]);
            model.PointRate = ConverToDouble(rdr["PointRate"]);
            model.Addon1 = ConverToStr(rdr["Addon1"]);
            model.Addon2 = ConverToStr(rdr["Addon2"]);
            model.StoreID = ConvertToInt(rdr["StoreID"]);
            model.OrderID = ConvertToInt(rdr["OrderID"]);
            model.Remark = ConverToStr(rdr["Remark"]);
            model.ZStatus = ConvertToInt(rdr["ZStatus"]);
            model.ZType = ConvertToInt(rdr["ZType"]);
            model.CDate = ConvertToDate(rdr["CDate"]);
            rdr.Close();
            return model;
        }
    }
}
