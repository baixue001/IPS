using System;using System.Data.Common;

using System.Data.SqlClient;

/// <summary>
/// M_Bank 的摘要说明
/// </summary>

namespace ZoomLa.Model
{

    public class M_User_Bank : M_Base
    {
        public int ID { get; set; }
        /// <summary>
        /// 卡类
        /// </summary>
        public string CardType { get; set; }
        /// <summary>
        /// 收款码
        /// </summary>
        public string QRCode { get; set; }
        /// <summary>
        /// 开户人
        /// </summary>
        public string PeopleName { get; set; }
        /// <summary>
        /// 银行名称
        /// </summary>
        public string BankName { get; set; }
        /// <summary>
        /// 备注信息
        /// </summary>
        public string Remark { get; set; }
        /// <summary>
        /// 银行卡号
        /// </summary>
        public string CardNum { get; set; }
        /// <summary>
        /// 用户ID
        /// </summary>
        public int UserID { get; set; }
        /// <summary>
        /// 添加时间
        /// </summary>
        public DateTime AddTime { get; set; }
        public override string TbName { get { return "ZL_User_Bank"; } }
        public override string PK { get { return "ID"; } }
        public override string[,] FieldList()
        {
            string[,] Tablelist = {
                                {"ID","Int","4"},
                                {"PeopleName","NVarChar","50"},
                                {"BankName","NVarChar","50"},
                                {"Remark","VarChar","200"},
                                {"CardNum","NVarChar","50"},
                                {"UserID","Int","4"},
                                {"AddTime","DateTime","8"},
                                {"CardType","NVarChar","100"},
                                {"QRCode","NVarChar","300"}
        };
            return Tablelist;
        }

        public override SqlParameter[] GetParameters()
        {
            M_User_Bank model = this;
            if (model.AddTime <= DateTime.MinValue) { model.AddTime = DateTime.Now; }
            SqlParameter[] sp = GetSP();
            sp[0].Value = model.ID;
            sp[1].Value = model.PeopleName;
            sp[2].Value = model.BankName;
            sp[3].Value = model.Remark;
            sp[4].Value = model.CardNum;
            sp[5].Value = model.UserID;
            sp[6].Value = model.AddTime;
            sp[7].Value = model.CardType;
            sp[8].Value = model.QRCode;
            return sp;
        }
        public M_User_Bank GetModelFromReader(DbDataReader rdr)
        {
            M_User_Bank model = new M_User_Bank();
            model.ID = ConvertToInt(rdr["ID"]);
            model.PeopleName = ConverToStr(rdr["PeopleName"]);
            model.BankName = ConverToStr(rdr["BankName"]);
            model.Remark = ConverToStr(rdr["Remark"]);
            model.CardNum = ConverToStr(rdr["CardNum"]);
            model.UserID = ConvertToInt(rdr["UserID"]);
            model.AddTime = ConvertToDate(rdr["AddTime"]);
            model.CardType = ConverToStr(rdr["CardType"]);
            model.QRCode = ConverToStr(rdr["QRCode"]);
            rdr.Close();
            return model;
        }
    }
}
