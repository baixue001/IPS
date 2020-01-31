using System;using System.Data.Common;
using System.Data.SqlClient;
using System.Data;
namespace ZoomLa.Model
{
    public class M_CartPro : M_Base
    {
        public int ID { get; set; }
        //[disuse]
        private int CartID { get; set; }
        public int ProID { get; set; }
        public int Pronum { get; set; }
        public int UserID { get; set; }
        /// <summary>
        /// 购买人名称,实际UserID等看订单表中
        /// </summary>
        public string Username { get; set; }
        public string Proname { get; set; }
        public string Danwei { get; set; }
        /// <summary>
        /// 实际购买时的价格
        /// </summary>
        public double Shijia { get; set; }
        /// <summary>
        /// 金额小计(优惠后)
        /// </summary>
        public double AllMoney { get; set; }
        /// <summary>
        /// 生成购物车记录的时间
        /// </summary>
        public DateTime Addtime
        {
            get;
            set;
        }
        /// <summary>
        /// 附加信息,用于存购买时用户信息
        /// </summary>
        public string Additional { get; set; }
        /// <summary>
        /// 店铺ID
        /// </summary>
        public int StoreID { get; set; }
        /// <summary>
        /// 附加虚拟币信息
        /// </summary>
        public string AllMoney_Json { get; set; }
        /// <summary>
        /// 多价格编号,或IDC期限,用于标识商品价格
        /// </summary>
        public string code { get; set; }
        /// <summary>
        /// 商品定制信息，本次该商品的特殊标识（可存）
        /// </summary>
        public string Attribute { get; set; }
        public string Remark { get; set; }
        //-------------------------new
        public double AllMoney_Init { get; set; }
        public double AllMoney_Arrive { get; set; }
        public string ArriveRemark { get; set; }
        /// <summary>
        /// 暂用于存商品.[Class]
        /// </summary>
        public string PClass { get; set; }
        /// <summary>
        /// 商品类型==商品的分类
        /// </summary>
        public int ProClass { get; set; }
        public int Orderlistid { get; set; }
        //-------------------------unique
        /// <summary>
        /// 备份商品购买时的数据(json)
        /// </summary>
        public string ProInfo { get; set; }
        /// <summary>
        /// 附加状态,是否已返修,退货,评价 repair,return,comment
        /// </summary>
        public string AddStatus { get; set; }
        /// <summary>
        /// 是否已被晒单等
        /// </summary>
        public string cartStatus { get; set; }
        /// <summary>
        /// 暂只用于IDC
        /// </summary>
        public DateTime EndTime
        {
            get;
            set;
        }
        //-------------------------disuse
        public string FarePrice { get; set; }
        public M_CartPro()
        {
            this.PClass = "0";
            this.ProClass = 1;
            this.Addtime = DateTime.Now;
            this.EndTime = DateTime.Now.AddYears(10);
            this.UserID = 0;
            this.Username = "";
        }
        public override string TbName { get { return "ZL_CartPro"; } }
        public override string[,] FieldList()
        {
            string[,] Tablelist = {
                                  {"ID","Int","4"},
                                  {"Orderlistid","Int","4"},
                                  {"CartID","Int","4"},
                                  {"ProID","Int","4"}, 
                                  {"Pronum","Int","4"},
                                  {"Username","NVarChar","255"},
                                  {"Proname","NVarChar","255"},
                                  {"Danwei","NVarChar","50"},
                                  {"Shijia","Money","8"}, 
                                  {"AllMoney","Money","8"},
                                  {"Addtime","DateTime","8"},
                                  {"PClass","NVarChar","255"},
                                  {"ProClass","Int","4"}, 
                                  {"Attribute","NVarChar","1000"},
                                  {"FarePrice","NVarChar","400"},
                                  {"EndTime","DateTime","8"},
                                  {"Additional","NText","4000"},
                                  {"StoreID","Int","4"},
                                  {"AllMoney_Json","NVarChar","500"},
                                  {"AddStatus","NVarChar","1000"},
                                  {"code","VarChar","500"},
                                  {"ProInfo","NText","20000"},
                                  {"Remark","NVarChar","4000"},
                                  {"UserID","Int","4"}
                                 };
            return Tablelist;
        }
        public override SqlParameter[] GetParameters()
        {
            M_CartPro model = this;
            if (model.Addtime <= DateTime.MinValue) { model.Addtime = DateTime.Now; }
            if (model.EndTime <= DateTime.MinValue) { model.EndTime = DateTime.Now; }
            SqlParameter[] sp = GetSP();
            sp[0].Value = model.ID;
            sp[1].Value = model.Orderlistid;
            sp[2].Value = model.CartID;
            sp[3].Value = model.ProID;
            sp[4].Value = model.Pronum;
            sp[5].Value = model.Username;
            sp[6].Value = model.Proname;
            sp[7].Value = model.Danwei;
            sp[8].Value = model.Shijia;
            sp[9].Value = model.AllMoney;
            sp[10].Value = model.Addtime;
            sp[11].Value = model.PClass;
            sp[12].Value = model.ProClass;
            sp[13].Value = model.Attribute;
            sp[14].Value = model.FarePrice;
            sp[15].Value = model.EndTime;
            sp[16].Value = model.Additional;
            sp[17].Value = model.StoreID;
            sp[18].Value = model.AllMoney_Json;
            sp[19].Value = model.AddStatus;
            sp[20].Value = model.code;
            sp[21].Value = model.ProInfo;
            sp[22].Value = model.Remark;
            sp[23].Value = model.UserID;
            return sp;
        }
        public M_CartPro GetModelFromReader(DataRow rdr)
        {
            M_CartPro model = new M_CartPro();
            model.ID = Convert.ToInt32(rdr["ID"]);
            model.Orderlistid = ConvertToInt(rdr["Orderlistid"]);
            model.CartID = Convert.ToInt32(rdr["CartID"]);
            model.ProID = Convert.ToInt32(rdr["ProID"]);
            model.Pronum = ConvertToInt(rdr["Pronum"]);
            model.Username = ConverToStr(rdr["Username"]);
            model.Proname = ConverToStr(rdr["Proname"]);
            model.Danwei = ConverToStr(rdr["Danwei"]);
            model.Shijia = ConverToDouble(rdr["Shijia"]);
            model.AllMoney = ConverToDouble(rdr["AllMoney"]);
            model.Addtime = Convert.ToDateTime(rdr["Addtime"]);
            model.PClass = ConverToStr(rdr["PClass"]);
            model.ProClass = ConvertToInt(rdr["ProClass"]);
            model.Attribute = ConverToStr(rdr["Attribute"]);
            model.FarePrice = ConverToStr(rdr["FarePrice"]);
            model.EndTime = ConvertToDate(rdr["EndTime"]);
            model.Additional = ConverToStr(rdr["Additional"]);
            model.StoreID = ConvertToInt(rdr["StoreID"]);
            model.AllMoney_Json = ConverToStr(rdr["AllMoney_Json"]);
            model.AddStatus = ConverToStr(rdr["AddStatus"]);
            model.code = ConverToStr(rdr["code"]);
            model.ProInfo = ConverToStr(rdr["ProInfo"]);
            model.Remark = ConverToStr(rdr["Remark"]);
            model.UserID = ConvertToInt(rdr["UserID"]);
            return model;
        }
        public M_CartPro GetModelFromReader(DbDataReader rdr)
        {
            M_CartPro model = new M_CartPro();
            model.ID = Convert.ToInt32(rdr["ID"]);
            model.Orderlistid = Convert.ToInt32(rdr["Orderlistid"]);
            model.CartID = Convert.ToInt32(rdr["CartID"]);
            model.ProID = Convert.ToInt32(rdr["ProID"]);
            model.Pronum = Convert.ToInt32(rdr["Pronum"]);
            model.Username = rdr["Username"].ToString();
            model.Proname = ConverToStr(rdr["Proname"]);
            model.Danwei = ConverToStr(rdr["Danwei"]);
            model.Shijia = Convert.ToDouble(rdr["Shijia"]);
            model.AllMoney = Convert.ToDouble(rdr["AllMoney"]);
            model.Addtime = Convert.ToDateTime(rdr["Addtime"]);
            model.PClass = ConverToStr(rdr["PClass"]);
            model.ProClass = ConvertToInt(rdr["ProClass"]);
            model.Attribute = rdr["Attribute"].ToString();
            model.FarePrice = rdr["FarePrice"].ToString();
            model.EndTime = ConvertToDate(rdr["EndTime"]);
            model.Additional = ConverToStr(rdr["Additional"]);
            model.StoreID = ConvertToInt(rdr["StoreID"]);
            model.AllMoney_Json = ConverToStr(rdr["AllMoney_Json"]);
            model.AddStatus = ConverToStr(rdr["AddStatus"]);
            model.code = ConverToStr(rdr["code"]);
            model.ProInfo = ConverToStr(rdr["ProInfo"]);
            model.Remark = ConverToStr(rdr["Remark"]);
            model.UserID = ConvertToInt(rdr["UserID"]);
            rdr.Dispose();
            return model;
        }
    }
}