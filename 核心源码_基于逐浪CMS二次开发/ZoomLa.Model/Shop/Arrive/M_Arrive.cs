using System;using System.Data.Common;
using System.Data.SqlClient;
using System.Data;


namespace ZoomLa.Model
{
    [Serializable]
    public class M_Arrive:M_Base
    {
        #region 定义字段
        public int ID { get; set; }
        /// <summary>
        /// 优惠券别名
        /// </summary>
        public string ArriveName { get; set; }
        /// <summary>
        /// 0:预定义生效起始与结束时间
        /// 1:根据用户领取时间进行计算
        /// </summary>
        public int DateType { get; set; }
        /// <summary>
        /// 天数,时间类型为1时生效,根据领取时间生成生效与到期时间
        /// </summary>
        public int DateDays { get; set; }
        /// <summary>
        /// 优惠券缩略图
        /// </summary>
        public string PreviewImg { get; set; }
        /// <summary>
        /// 优惠券详细描述
        /// </summary>
        public string ArriveDesc { get; set; }
        /// <summary>
        /// 创建用户(前端创建)
        /// </summary>
        public int CUser { get; set; }
        /// <summary>
        /// 后台创建,管理员ID
        /// </summary>
        public int CAdmin { get; set; }
        public DateTime CDate { get; set; }
        //------------------------------Item券可用信息
        /// <summary>
        /// 流水号,用于区分匹次
        /// </summary>
        public string Flow { get; set; }
        /// <summary>
        /// 抵用劵编号
        /// </summary>
        public string ArriveNO { get; set; }
        /// <summary>
        /// 抵用劵密码
        /// </summary>
        public string ArrivePwd { get; set; }
        /// <summary>
        /// 生效时间
        /// </summary>
        public DateTime AgainTime { get; set; }
        /// <summary>
        /// 到期时间
        /// </summary>
        public DateTime EndTime { get; set; }
        /// <summary>
        /// 可用于哪些商品,可与金额搭配共用
        /// </summary>
        public string ProIDS { get; set; }
        /// <summary>
        /// 哪些品类可用(暂不开放)
        /// </summary>
        public string ProNodeIDS { get; set; }
        /// <summary>
        /// 所属店铺(与商品可共同生效)
        /// </summary>
        public int StoreID { get; set; }
        /// <summary>
        /// 使用金额最小范围,不设则不限定
        /// </summary>
        public double MinAmount { get; set; }
        /// <summary>
        /// 使用金额最大范围,不设则不限定
        /// </summary>
        public double MaxAmount { get; set; }
        /// <summary>
        /// 优惠金额
        /// </summary>
        public double Amount { get; set; }
        /// <summary>
        /// 最大优惠金额(折扣卡时生效,为0不限定)
        /// </summary>
        public double Amount_Max { get; set; }
        /// <summary>
        /// 0:未激活,1:已激活,10:已使用
        /// </summary>
        public int State { get; set; }
        /// <summary>
        /// 0:优惠券,1:折扣卡,2:商品赠券(选定的商品免费,ProIDS存储)
        /// </summary>
        public int Type { get; set; }
        //------------------------------用户模块
        /// <summary>
        /// 用户领取时间
        /// </summary>
        public DateTime GetTime { get; set; }
        /// <summary>
        /// 使用时间
        /// </summary>
        public DateTime UseTime { get; set; }
        /// <summary>
        /// 所属用户ID
        /// </summary>
        public int UserID { get; set; }
        /// <summary>
        /// 使用备注,记录用途,目标订单号,或唯一标记
        /// </summary>
        public string UseRemind { get; set; }
        #endregion
        public M_Arrive()
        {
            AgainTime = DateTime.Now;
            EndTime = AgainTime.AddYears(1);
            UseTime = DateTime.Now;
            Amount = 1;
            State = 0;
            Type = 0;
            DateType = 0;
            DateDays = 7;
            CDate = DateTime.Now;
        }
        public string _tbname = "ZL_Arrive";
        public override string TbName { get { return _tbname; } set { _tbname = value; } }
        public override string[,] FieldList()
        {
            string[,] Tablelist = {
                                  {"ID","Int","4"},
                                  {"ArriveNO","NVarChar","50"},
                                  {"UserID","Int","4"},
                                  {"MinAmount","Money","16"},
                                  {"MaxAmount","Money","16"}, 
                                  {"Amount","Money","16"}, 
                                  {"AgainTime","DateTime","8"},
                                  {"EndTime","DateTime","8"},
                                  {"State","Int","4"},
                                  {"ArriveName","NVarChar","50"},
                                  {"ArrivePwd","NVarChar","50"}, 
                                  {"UseTime","DateTime","8"},
                                  {"Type","Int","4"},
                                  {"ArriveDesc","NVarChar","500"}, 
                                  {"PreviewImg","NVarChar","500"}, 
                                  {"Flow","NVarChar","100"},
                                  {"UseRemind","NVarChar","2000"},
                                  {"ProIDS","Text","20000"},
                                  {"ProNodeIDS","Text","20000"},
                                  {"StoreID","Int","4"},
                                  {"CUser","Int","4"},
                                  {"CAdmin","Int","4"},
                                  {"AMount_Max","Money","16"},
                                  {"DateType","Int","4"},
                                  {"DateDays","Int","4" },
                                  {"CDate","DateTime","8" }
                                  };
            return Tablelist;
        }
        public override SqlParameter[] GetParameters()
        {
            M_Arrive model = this;
            SqlParameter[] sp = GetSP();
            if (model.CDate <= DateTime.MinValue) { model.CDate = DateTime.Now; }
            sp[0].Value = model.ID;
            sp[1].Value = model.ArriveNO;
            sp[2].Value = model.UserID;
            sp[3].Value = model.MinAmount;
            sp[4].Value = model.MaxAmount;
            sp[5].Value = model.Amount;
            sp[6].Value = model.AgainTime;
            sp[7].Value = model.EndTime;
            sp[8].Value = model.State;
            sp[9].Value = model.ArriveName;
            sp[10].Value = model.ArrivePwd;
            sp[11].Value = model.UseTime;
            sp[12].Value =model.Type;
            sp[13].Value = model.ArriveDesc;
            sp[14].Value = model.PreviewImg;
            sp[15].Value = model.Flow;
            sp[16].Value = model.UseRemind;
            sp[17].Value = model.ProIDS;
            sp[18].Value = model.ProNodeIDS;
            sp[19].Value = model.StoreID;
            sp[20].Value = model.CUser;
            sp[21].Value = model.CAdmin;
            sp[22].Value = model.Amount_Max;
            sp[23].Value = model.DateType;
            sp[24].Value = model.DateDays;
            return sp;
        }
        public M_Arrive GetModelFromReader(DbDataReader rdr)
        {
            M_Arrive model = new M_Arrive();
            model.ID = Convert.ToInt32(rdr["ID"]);
            model.ArriveNO = ConverToStr(rdr["ArriveNO"]);
            model.UserID = Convert.ToInt32(rdr["UserID"]);
            model.MinAmount = ConverToDouble(rdr["MinAmount"]);
            model.MaxAmount = ConverToDouble(rdr["MaxAmount"]);
            model.Amount = Convert.ToInt32(rdr["Amount"]);
            model.AgainTime = ConvertToDate(rdr["AgainTime"]);
            model.EndTime = ConvertToDate(rdr["EndTime"]);
            model.State = Convert.ToInt32(rdr["State"]);
            model.ArriveName = ConverToStr(rdr["ArriveName"]);
            model.ArrivePwd = ConverToStr(rdr["ArrivePwd"]);
            model.UseTime = ConvertToDate(rdr["UseTime"]);
            model.Type = ConvertToInt(rdr["Type"]);
            model.ArriveDesc = ConverToStr(rdr["ArriveDesc"]);
            model.PreviewImg = ConverToStr(rdr["PreviewImg"]);
            model.Flow = ConverToStr(rdr["Flow"]);
            model.UseRemind = ConverToStr(rdr["UseRemind"]);
            model.ProIDS = ConverToStr(rdr["ProIDS"]);
            model.ProNodeIDS = ConverToStr(rdr["ProNodeIDS"]);
            model.StoreID = ConvertToInt(rdr["StoreID"]);
            model.CUser = ConvertToInt(rdr["CUser"]);
            model.CAdmin = ConvertToInt(rdr["CAdmin"]);
            model.Amount_Max = ConverToDouble(rdr["AMount_Max"]);
            model.DateType = ConvertToInt(rdr["DateType"]);
            model.DateDays = ConvertToInt(rdr["DateDays"]);
            model.CDate = ConvertToDate(rdr["CDate"]);
            rdr.Close();
            return model;
        }
        public M_Arrive GetModelFromReader(DataRow rdr)
        {
            M_Arrive model = new M_Arrive();
            model.ID = Convert.ToInt32(rdr["ID"]);
            model.ArriveNO = ConverToStr(rdr["ArriveNO"]);
            model.UserID = Convert.ToInt32(rdr["UserID"]);
            model.MinAmount = ConverToDouble(rdr["MinAmount"]);
            model.MaxAmount = ConverToDouble(rdr["MaxAmount"]);
            model.Amount = Convert.ToInt32(rdr["Amount"]);
            model.AgainTime = ConvertToDate(rdr["AgainTime"]);
            model.EndTime = ConvertToDate(rdr["EndTime"]);
            model.State = Convert.ToInt32(rdr["State"]);
            model.ArriveName = ConverToStr(rdr["ArriveName"]);
            model.ArrivePwd = ConverToStr(rdr["ArrivePwd"]);
            model.UseTime = ConvertToDate(rdr["UseTime"]);
            model.Type = ConvertToInt(rdr["Type"]);
            model.ArriveDesc = ConverToStr(rdr["ArriveDesc"]);
            model.PreviewImg = ConverToStr(rdr["PreviewImg"]);
            model.Flow = ConverToStr(rdr["Flow"]);
            model.UseRemind = ConverToStr(rdr["UseRemind"]);
            model.ProIDS = ConverToStr(rdr["ProIDS"]);
            model.ProNodeIDS = ConverToStr(rdr["ProNodeIDS"]);
            model.StoreID = ConvertToInt(rdr["StoreID"]);
            model.CUser = ConvertToInt(rdr["CUser"]);
            model.CAdmin = ConvertToInt(rdr["CAdmin"]);
            model.Amount_Max = ConverToDouble(rdr["AMount_Max"]);
            model.DateType = ConvertToInt(rdr["DateType"]);
            model.DateDays = ConvertToInt(rdr["DateDays"]);
            model.CDate = ConvertToDate(rdr["CDate"]);
            return model;
        }
    }
    //优惠结果类
    public class M_Arrive_Result
    {
        /// <summary>
        /// 是否可使用该优惠券
        /// </summary>
        public bool enabled = false;
        /// <summary>
        /// 优惠券实际优惠的金额
        /// </summary>
        public double amount = 0;
        /// <summary>
        /// 优惠之后的金额
        /// </summary>
        public double money = 0;
        /// <summary>
        /// 该优惠券不可使用的原因
        /// </summary>
        public string err = "";
        /// <summary>
        /// 优惠券编号
        /// </summary>
        public string flow = "";
    }
}