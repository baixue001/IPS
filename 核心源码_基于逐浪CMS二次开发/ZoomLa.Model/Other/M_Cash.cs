﻿using System;using System.Data.Common;
using System.Data.SqlClient;
using System.Data;

/*
 * 用于记录用户提现申请
 */
namespace ZoomLa.Model
{
    public class M_Cash:M_Base
    {
        public int Y_ID
        {
            get;
            set;
        }
        public int UserID
        {
            get;
            set;
        }
        /// <summary>
        /// 存用户名
        /// </summary>
        public string YName
        {
            get;
            set;
        }
        /// <summary>
        /// 提现金额
        /// </summary>
        public double money
        {
            get;
            set;
        }
        /// <summary>
        /// 扣除的手续费(现为额外扣除)
        /// </summary>
        public double WithDrawFee {get;set; }
       /// <summary>
        /// 提现时间
       /// </summary>
        public DateTime sTime
        {
            get;
            set;
        }
        /// <summary>
        /// 银行帐户
        /// </summary>
        public string Account
        {
            get;
            set;
        }
        /// <summary>
        /// 银行名称
        /// </summary>
        public string Bank
        {
            get;
            set;
        }
        /// <summary>
        /// 处理时间
        /// </summary>
        public DateTime eTime
        {
            get;
            set;
        }
        /// <summary>
        /// 状态:0:待处理,99:成功,-1拒绝
        /// </summary>
        public int yState
        {
            get;
            set;
        }
        /// <summary>
        /// 申请类型,0:银行卡提现,1:余额提现
        /// </summary>
        public int Classes
        {
            get;
            set;
        }
        /// <summary>
        /// 处理结果(仅管理员可见)
        /// </summary>
        public string Result { get; set; }
        /// <summary>
        /// 处理结果备注(用户可见)
        /// </summary>
        public string Remark { get; set; }
        /// <summary>
        /// 开户人姓名
        /// </summary>
        public string PeopleName { get; set; }
        /// <summary>
        /// 处理结果凭证(打款记录)(管理员)
        /// </summary>
        public string ResultImg { get; set; }
        /// <summary>
        /// 处理该笔提现的管理员ID
        /// </summary>
        public int AdminID { get; set; }
        /// <summary>
        /// 银行卡类型(银行卡|支付宝|微信|其它)
        /// </summary>
        public string CardType { get; set; }
        /// <summary>
        /// 收款码图片
        /// </summary>
        public string QRcode { get; set; }
        /// <summary>
        /// 用户备注
        /// </summary>
        public string UserRemind { get; set; }
        public M_Cash()
        {
            sTime = DateTime.Now;
            eTime = DateTime.Now;
            yState = 0;
            Classes = 0;
        }
        public override string PK { get { return "Y_ID"; } }
        public override string TbName { get { return "ZL_Cash"; } }
        public override string[,] FieldList()
        {
            string[,] Tablelist = {
                                  {"Y_ID","Int","4"},
                                  {"UserID","Int","4"},
                                  {"YName","NVarChar","100"},
                                  {"money","Money","10"},
                                  {"sTime","DateTime","8"},
                                  {"Account","NVarChar","500"},
                                  {"Bank","NVarChar","50"},
                                  {"eTime","DateTime","8"},
                                  {"yState","Int","4"},
                                  {"Classes","Int","4"},
                                  {"Result","NVarChar","500"},
                                  {"Remark","NVarChar","500"},
                                  {"PeopleName","NVarChar","100"},
                                  {"WithDrawFee","Money","10"},
                                  {"ResultImg","NVarChar","4000"},
                                  {"AdminID","Int","4"},
                                  {"CardType","NVarChar","100"},
                                  {"QRCode","NVarChar","300"},
                                  {"UserRemind","NVarChar","500"},
                                 };
            return Tablelist;
        }
        public override SqlParameter[] GetParameters()
        {
            M_Cash model = this;
            SqlParameter[] sp = GetSP();
            sp[0].Value = model.Y_ID;
            sp[1].Value = model.UserID;
            sp[2].Value = model.YName;
            sp[3].Value = model.money;
            sp[4].Value = model.sTime;
            sp[5].Value = model.Account;
            sp[6].Value = model.Bank;
            sp[7].Value = model.eTime;
            sp[8].Value = model.yState;
            sp[9].Value = model.Classes;
            sp[10].Value = model.Result;
            sp[11].Value = model.Remark;
            sp[12].Value = model.PeopleName;
            sp[13].Value = model.WithDrawFee;
            sp[14].Value = model.ResultImg;
            sp[15].Value = model.AdminID;
            sp[16].Value = model.CardType;
            sp[17].Value = model.QRcode;
            sp[18].Value = model.UserRemind;
            return sp;
        }
        public M_Cash GetModelFromReader(DbDataReader rdr)
        {
            M_Cash model = new M_Cash();
            model.Y_ID = Convert.ToInt32(rdr["Y_ID"]);
            model.UserID = ConvertToInt(rdr["UserID"]);
            model.YName = ConverToStr(rdr["YName"]);
            model.money = ConverToDouble(rdr["money"]);
            model.sTime = ConvertToDate(rdr["sTime"]);
            model.Account = ConverToStr(rdr["Account"]);
            model.Bank = ConverToStr(rdr["Bank"]);
            model.eTime = ConvertToDate(rdr["eTime"]);
            model.yState = ConvertToInt(rdr["yState"]);
            model.Classes = ConvertToInt(rdr["Classes"]);
            model.Result = ConverToStr(rdr["Result"]);
            model.Remark = ConverToStr(rdr["Remark"]);
            model.PeopleName = ConverToStr(rdr["PeopleName"]);
            model.WithDrawFee = ConverToDouble(rdr["WithDrawFee"]);
            model.ResultImg = ConverToStr(rdr["ResultImg"]);
            model.AdminID = ConvertToInt(rdr["AdminID"]);
            model.CardType = ConverToStr(rdr["CardType"]);
            model.QRcode = ConverToStr(rdr["QRcode"]);
            model.UserRemind = ConverToStr(rdr["UserRemind"]);
            rdr.Close();
            return model;
        }
    }
    
}

