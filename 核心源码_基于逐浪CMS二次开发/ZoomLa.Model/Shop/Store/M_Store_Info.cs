using System;using System.Data.Common;
using System.Collections.Generic;
using System.Data;

using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace ZoomLa.Model.Shop
{
    public class M_Store_Info : M_Base
    {
        public M_Store_Info() { }
        public int ID { get; set; }
        /// <summary>
        /// 附表主键ID
        /// </summary>
        public int ItemID { get; set; }
        public string Title { get; set; }
        /// <summary>
        /// 店铺状态
        /// </summary>
        public int Status { get; set; }
        public DateTime CreateTime { get; set; }
        /// <summary>
        /// 店铺的所有者ID
        /// </summary>
        public int UserID { get; set; }
        public string UserName { get; set; }
        /// <summary>
        /// 店铺绑定样式ID
        /// </summary>
        public int StoreStyleID { get; set; }
        public string Pics { get; set; }
        /// <summary>
        /// 店铺绑定模型ID
        /// </summary>
        public int StoreModelID { get; set; }
        public string Synopsis { get; set; }
        public string Content { get; set; }
        /// <summary>
        /// 店铺Logo
        /// </summary>
        public string Logo { get; set; }
        /// <summary>
        /// 联接查询出的数据
        /// </summary>
        public DataRow InfoDR { get; set; }
        public string STbName = "ZL_Store_Reg";
        public override string TbName { get { return "ZL_CommonModel"; } }
        public override string PK { get { return "ID"; } }

        public override string[,] FieldList()
        {
            string[,] Tablelist = {
                                {"ID","Int","4"},
                                {"Name","NVarChar","200"},
                                {"ProIDS","VarChar","8000"},
                                {"AdminID","Int","4"},
                                {"CDate","DateTime","8"},
                                {"Remind","NVarChar","500"}
        };
            return Tablelist;
        }

        public override SqlParameter[] GetParameters()
        {
            M_Store_Info model = this;
            SqlParameter[] sp = GetSP();
            return sp;
        }
        public M_Store_Info GetModelFromReader(DataRow rdr)
        {
            M_Store_Info model = new M_Store_Info();
            model.ID = Convert.ToInt32(rdr["GeneralID"]);
            model.ItemID = Convert.ToInt32(rdr["ItemID"]);
            model.Title = ConverToStr(rdr["Title"]);
            model.Status = ConvertToInt(rdr["Status"]);
            model.CreateTime = ConvertToDate(rdr["CreateTime"]);
            //----附表信息
            model.UserID = ConvertToInt(rdr["UserID"]);
            model.UserName = ConverToStr(rdr["UserName"]);
            if (rdr.Table.Columns.Contains("synopsis"))
            { model.Synopsis = ConverToStr(rdr["synopsis"]); }
            if (rdr.Table.Columns.Contains("Content"))
            { model.Content = ConverToStr(rdr["Content"]); }
            if (rdr.Table.Columns.Contains("pics"))
            { model.Pics = ConverToStr(rdr["pics"]); }
            if (rdr.Table.Columns.Contains("Logo"))
            { model.Logo = ConverToStr(rdr["Logo"]); }
            if (rdr.Table.Columns.Contains("StoreStyleID"))
            { model.StoreStyleID = ConvertToInt(rdr["StoreStyleID"]); }
            if (rdr.Table.Columns.Contains("StoreModelID"))
            { model.StoreModelID = ConvertToInt(rdr["StoreModelID"]); }
            InfoDR = rdr;
            return model;
        }

    }
}
