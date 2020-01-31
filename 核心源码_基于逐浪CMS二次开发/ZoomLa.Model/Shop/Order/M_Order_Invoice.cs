using System;using System.Data.Common;
using System.Collections.Generic;
using System.Data;

using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace ZoomLa.Model.Shop
{
    public class M_Order_Invoice : M_Base
    {
        public string[] InvClassArr = "明细,办公用品,电脑配件,食品,服装服饰,酒水餐饮".Split(',');
        public M_Order_Invoice() {
            InvType = 1;
            InvClass = "明细";
        }
        public int ID { get; set; }
        public int OrderID { get; set; }
        /// <summary>
        /// 发票类型,0=普通发票,1=电子发票,2=增值税发票
        /// </summary>
        public int InvType { get; set; }
        /// <summary>
        /// 发票抬头  个人|公司名称
        /// </summary>
        public string InvHead { get; set; }
        /// <summary>
        /// 发票科目
        /// </summary>
        public string InvClass { get; set; }
        /// <summary>
        /// 发票内容
        /// </summary>
        public string InvContent { get; set; }
        /// <summary>
        /// 纳税人识别号
        /// </summary>
        public string UserCode { get; set; }
        /// <summary>
        /// 发票人手机
        /// </summary>
        public string UserMobile { get; set; }
        /// <summary>
        /// 发票人邮箱
        /// </summary>
        public string UserEmail { get; set; }
        public DateTime CDate { get; set; }
        public int UserID { get; set; }
        public string _tbname = "ZL_Order_Invoice";
        public override string TbName { get { return _tbname; }set { _tbname = value; } }
        public override string PK { get { return "ID"; } }

        public override string[,] FieldList()
        {
            string[,] Tablelist = {
                                {"ID","Int","4"},
                                {"OrderID","Int","4"},
                                {"InvType","Int","4"},
                                {"InvHead","NVarChar","200"},
                                {"InvContent","NVarChar","200"},
                                {"UserCode","NVarChar","100"},
                                {"UserMobile","NVarChar","100"},
                                {"UserEmail","NVarChar","100"},
                                {"CDate","DateTime","8"},
                                {"UserID","Int","4"},
                                {"InvClass","NVarChar","100"}
        };
            return Tablelist;
        }

        public override SqlParameter[] GetParameters()
        {
            M_Order_Invoice model = this;
            SqlParameter[] sp = GetSP();
            if (model.CDate <= DateTime.MinValue) { model.CDate = DateTime.Now; }
            sp[0].Value = model.ID;
            sp[1].Value = model.OrderID;
            sp[2].Value = model.InvType;
            sp[3].Value = model.InvHead;
            sp[4].Value = model.InvContent;
            sp[5].Value = model.UserCode;
            sp[6].Value = model.UserMobile;
            sp[7].Value = model.UserEmail;
            sp[8].Value = model.CDate;
            sp[9].Value = model.UserID;
            sp[10].Value = model.InvClass;
            return sp;
        }
        public M_Order_Invoice GetModelFromReader(DbDataReader rdr)
        {
            M_Order_Invoice model = new M_Order_Invoice();
            model.ID = ConvertToInt(rdr["ID"]);
            model.OrderID = ConvertToInt(rdr["OrderID"]);
            model.InvType = ConvertToInt(rdr["InvType"]);
            model.InvHead = ConverToStr(rdr["InvHead"]);
            model.InvContent = ConverToStr(rdr["InvContent"]);
            model.UserCode = ConverToStr(rdr["UserCode"]);
            model.UserMobile = ConverToStr(rdr["UserMobile"]);
            model.UserEmail = ConverToStr(rdr["UserEmail"]);
            model.CDate = ConvertToDate(rdr["CDate"]);
            model.UserID = ConvertToInt(rdr["UserID"]);
            model.InvClass = ConverToStr(model.InvClass);
            rdr.Close();
            return model;
        }
    }
}
