using System;using System.Data.Common;
using System.Data.SqlClient;
using System.Data;


namespace ZoomLa.Model
{
    [Serializable]
    public class M_Promotion : M_Base
    {
        public int ID { get; set; }
        /// <summary>
        /// 优惠活动创建人
        /// </summary>
        public int UserID { get; set; }
        /// <summary>
        /// 优惠活动名称
        /// </summary>
        public string PromoName { get; set; }
        /// <summary>
        /// 优惠活动开始时间
        /// </summary>
        public DateTime StartTime { get; set; }
        /// <summary>
        /// 优惠活动结束时间
        /// </summary>
        public DateTime EndTime { get; set; }
        /// <summary>
        /// 参与优惠金额上限
        /// </summary>
        public double MoneyUL { get; set; }
        /// <summary>
        /// 参与优惠金额下限
        /// </summary>
        public double MoneyLL { get; set; }
        /// <summary>
        /// 参与优惠活动商品IDS
        /// </summary>
        public string ProIds { get; set; }
        /// <summary>
        /// 参与活动的会员组
        /// </summary>
        public string UserGroup { get; set; }
        /// <summary>
        /// 优惠方式,特价、减免、打折、赠品等
        /// </summary>
        public int ZType { get; set; }
        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreateTime { get; set; }
        public override string TbName { get { return "ZL_Promotion"; } }
        public override string[,] FieldList()
        {
            string[,] Tablelist = {
                                  {"ID","Int","4"},
                                  {"UserID","Int","4"},
                                  {"PromoName","NVarChar","50"},
                                  {"StartTime","DateTime","8"},
                                  {"EndTime","DateTime","8"},
                                  {"MoneyUL","Money","32"},
                                  {"MoneyLL","Money","32"},
                                  {"ProIds","NVarChar","200"},
                                  {"UserGroup","NVarChar","100"},
                                  {"ZType","Int","4"},
                                  {"CreateTime","DateTime","8"}
                                 };
            return Tablelist;
        }
        public override SqlParameter[] GetParameters()
        {
            M_Promotion model = this;
            if (model.StartTime == DateTime.MinValue) { model.StartTime = DateTime.Now; }
            if (model.EndTime == DateTime.MinValue) { model.EndTime = DateTime.Now; }
            if (model.CreateTime == DateTime.MinValue) { model.CreateTime = DateTime.Now; }
            SqlParameter[] sp = GetSP();
            sp[0].Value = model.ID;
            sp[1].Value = model.UserID;
            sp[2].Value = model.PromoName;
            sp[3].Value = model.StartTime;
            sp[4].Value = model.EndTime;
            sp[5].Value = model.MoneyUL;
            sp[6].Value = model.MoneyLL;
            sp[7].Value = model.ProIds;
            sp[8].Value = model.UserGroup;
            sp[9].Value = model.ZType;
            sp[10].Value = model.CreateTime;
            return sp;
        }

        public M_Promotion GetModelFromReader(DbDataReader rdr)
        {
            M_Promotion model = new M_Promotion();
            model.ID = Convert.ToInt32(rdr["ID"]);
            model.UserID = ConvertToInt(rdr["UserID"]);
            model.PromoName = ConverToStr(rdr["PromoName"]);
            model.StartTime = ConvertToDate(rdr["StartTime"]);
            model.EndTime = ConvertToDate(rdr["EndTime"]);
            model.MoneyUL = ConverToDouble(rdr["MoneyUL"]);
            model.MoneyLL = ConverToDouble(rdr["MoneyLL"]);
            model.ProIds = ConverToStr(rdr["ProIds"]);
            model.UserGroup = ConverToStr(rdr["UserGroup"]);
            model.ZType = ConvertToInt(rdr["ZType"]);
            rdr.Close();
            return model;
        }
    }
}


