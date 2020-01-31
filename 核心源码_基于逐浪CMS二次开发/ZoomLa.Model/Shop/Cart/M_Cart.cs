using System;using System.Data.Common;
using System.Data.SqlClient;
using System.Data;
namespace ZoomLa.Model
{
    public class M_Cart:M_Base
    {
        public int ID { get; set; }
        public string Cartid { get; set; }
        public int userid { get; set; }
        public string Username { get; set; }
        public DateTime Addtime { get; set; }
        public int StoreID { get; set; }
        public int ProID { get; set; }
        public string Proname { get; set; }
        public int Pronum { get; set; }
        /// <summary>
        /// 需付金额(实际支付金额)
        /// </summary>
        public double AllMoney { get; set; }
        /// <summary>
        /// 购买时的价格(预定价|会员价|优惠价)
        /// </summary>
        public string FarePrice { get; set; }
        /// <summary>
        /// 附加信息,用于存储联系人,商品信息等
        /// </summary>
        public string Additional { get; set; }
        /// <summary>
        /// 用于支持虚拟币
        /// </summary>
        public string AllMoney_Json { get; set; }
        /// <summary>
        /// 多价格编号,如不为空,则读取多价格信息
        /// </summary>
        public string code { get; set; }
        /// <summary>
        /// 商品属性 name:value|name2:value2(需扩展)
        /// </summary>
        public string ProAttr { get; set; }
        /// <summary>
        /// 存放备注信息
        /// </summary>
        public string Remark { get; set; }
        //-------------------------new
        /// <summary>
        /// 初始未优惠的金额
        /// </summary>
        public double AllMoney_Init { get; set; }
        /// <summary>
        /// 优惠的金额(累计)
        /// </summary>
        public double AllMoney_Arrive { get; set; }
        /// <summary>
        /// 优惠详情记录,以|号隔开
        /// </summary>
        public string ArriveRemark { get; set; }
        /// <summary>
        /// 单位
        /// </summary>
        public string Danwei { get; set; }
        /// <summary>
        /// 购买时的价格(未优惠价)
        /// </summary>
        public double Shijia { get; set; }
        //-------------------------disuse
        public double AllIntegral { get; set; }
        public string Getip { get; set; }
        public M_Cart() 
        {
            Addtime = DateTime.Now;
            AllMoney = 0;
            AllMoney_Init = 0;
            AllMoney_Arrive = 0;
            ArriveRemark = "";
        }
        public override string TbName { get { return "ZL_Cart"; } }
        public override string[,] FieldList()
        {
            string[,] Tablelist = {
                                  {"ID","Int","4"},
                                  {"Cartid","NVarChar","255"},
                                  {"userid","Int","4"},
                                  {"Username","NVarChar","255"}, 
                                  {"Addtime","DateTime","8"},
                                  {"StoreID","Int","4"},
                                  {"ProID","Int","4"},
                                  {"Pronum","Int","4"},
                                  {"AllMoney","Money","8"},
                                  {"FarePrice","NVarChar","400"},
                                  {"Additional","NText","4000"},
                                  {"AllMoney_Json","NVarChar","500"},
                                  {"code","VarChar","500"},
                                  {"Proname","NVarChar","500"},
                                  {"ProAttr","NVarChar","4000"},
                                  {"Remark","NVarChar","4000"}
                                 };
            return Tablelist;
        }
        public override SqlParameter[] GetParameters()
        {
            M_Cart model = this;
            SqlParameter[] sp = GetSP();
            sp[0].Value = model.ID;
            sp[1].Value = model.Cartid;
            sp[2].Value = model.userid;
            sp[3].Value = model.Username;
            sp[4].Value = model.Addtime;
            sp[5].Value = model.StoreID;
            sp[6].Value = model.ProID;
            sp[7].Value = model.Pronum;
            sp[8].Value = model.AllMoney;
            sp[9].Value = model.FarePrice;
            sp[10].Value = model.Additional;
            sp[11].Value = model.AllMoney_Json;
            sp[12].Value = model.code;
            sp[13].Value = model.Proname;
            sp[14].Value = model.ProAttr;
            sp[15].Value = model.Remark;
            return sp;
        }
        public M_Cart GetModelFromReader(DbDataReader rdr)
        {
            M_Cart model = new M_Cart();
            model.ID = Convert.ToInt32(rdr["ID"]);
            model.Cartid = ConverToStr(rdr["Cartid"]);
            model.userid = Convert.ToInt32(rdr["userid"]);
            model.Username = ConverToStr(rdr["Username"]);
            model.Addtime = ConvertToDate(rdr["Addtime"]);
            model.StoreID = Convert.ToInt32(rdr["StoreID"]);
            model.ProID = Convert.ToInt32(rdr["ProID"]);
            model.Pronum = Convert.ToInt32(rdr["Pronum"]);
            model.AllMoney = Convert.ToDouble(rdr["AllMoney"]);
            model.FarePrice = ConverToStr(rdr["FarePrice"]);
            model.Additional = ConverToStr(rdr["Additional"]);
            model.AllMoney_Json = ConverToStr(rdr["AllMoney_Json"]);
            model.code = ConverToStr(rdr["code"]);
            model.Proname = ConverToStr(rdr["Proname"]);
            model.ProAttr = ConverToStr(rdr["ProAttr"]);
            model.Remark = ConverToStr(rdr["Remark"]);
            rdr.Close();
            return model;
        }
        public M_Cart GetModelFromReader(DataRow rdr)
        {
            M_Cart model = new M_Cart();
            model.ID = Convert.ToInt32(rdr["ID"]);
            model.Cartid = ConverToStr(rdr["Cartid"]);
            model.Username = ConverToStr(rdr["Username"]);
            model.Addtime = ConvertToDate(rdr["Addtime"]);
            model.StoreID = Convert.ToInt32(rdr["StoreID"]);
            model.ProID = Convert.ToInt32(rdr["ProID"]);
            model.Pronum = Convert.ToInt32(rdr["Pronum"]);
            model.AllMoney = Convert.ToDouble(rdr["AllMoney"]);
            model.FarePrice = ConverToStr(rdr["FarePrice"]);
            model.Additional = ConverToStr(rdr["Additional"]);
            model.AllMoney_Json = ConverToStr(rdr["AllMoney_Json"]);
            model.code = ConverToStr(rdr["code"]);
            model.Proname = ConverToStr(rdr["Proname"]);
            model.Remark = ConverToStr(rdr["Remark"]);
            try
            {
                //兼容CartPro
                if (rdr.Table.Columns.Contains("shijia")) { model.FarePrice = ConverToStr(rdr["Shijia"]); }
                model.userid = Convert.ToInt32(rdr["userid"]);
                model.ProAttr = ConverToStr(rdr["ProAttr"]);
            }
            catch { }
            return model;
        }
    }
}
