using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;
using System.Text;

namespace ZoomLa.Model
{
    public class M_UserExpHis : M_Base
    {
        /// <summary>
        /// 余额,银币,积分(3),点券(4),虚拟币(5),信誉点(6)
        /// </summary>
        public enum SType { Purse = 1, SIcon = 2, Point = 3, UserPoint = 4, DummyPoint = 5, Credit = 6 };
        public int ExpHisID { get; set; }
        public int UserID { get; set; }
        /// <summary>
        /// 备注
        /// </summary>
        public string detail { get; set; }
        /// <summary>
        /// 虚拟币类型
        /// </summary>
        public int ScoreType { get; set; }
        /// <summary>
        /// 金额
        /// </summary>
        public double score { get; set; }
        /// <summary>
        /// 操作前的金额(180416)
        /// </summary>
        public double score_before { get; set; }
        /// <summary>
        /// 操作分类,存中文
        /// </summary>
        public string ZType { get; set; }
        //添加时间
        public DateTime HisTime { get; set; }
        /// <summary>
        /// 操作人ID,AdminID
        /// </summary>
        public int Operator { get; set; }
        /// <summary>
        /// 操作点IP
        /// </summary>
        public string OperatorIP { get; set; }
        /// <summary>
        /// 记录操作的页面URL,便于后期审查
        /// </summary>
        public string Remark { get; set; }
        /// <summary>
        /// 支付凭证图片|附件
        /// </summary>
        public string Attach { get; set; }
        /// <summary>
        /// 存储扩展数据,用于部分项目
        /// </summary>
        public string Extend { get; set; }
        public M_UserExpHis()
        {
            this.HisTime = DateTime.Now;
        }
        public M_UserExpHis(M_UserInfo mu, int sco, string detailStr)
        {
            this.UserID = mu.UserID;
            this.Operator = mu.UserID;
            this.detail = detailStr;
            this.score = 0 - sco;
            this.HisTime = DateTime.Now;
        }
        private string _tbname = "ZL_UserExpHis";
        public override string TbName { get { return _tbname; } set { _tbname = value; } }
        public override string UP_Tables
        {
            get
            {
                return "ZL_UserExpDomP,ZL_UserExpHis,ZL_UserCoinHis,ZL_User_DummyPoint,ZL_User_UserPoint,ZL_User_Credit";
            }
        }
        public override string PK { get { return "ExpHisID"; } }
        public override string[,] FieldList() { return M_UserExpHis.GetFieldList(); }
        public static string[,] GetFieldList()
        {
            string[,] Tablelist = {
                                  {"ExpHisID","Int","4"},
                                  {"UserID","Int","4"},
                                  {"Operator","Int","4"},
                                  {"detail","NVarChar","1000"},
                                  {"score","Money","16"},
                                  {"HisTime","DateTime","8"},
                                  {"OperatorIP","NVarChar","200"},
                                  {"ScoreType","Int","4"},
                                  {"Remark","NVarChar","200"},
                                  {"Attach","NVarChar","4000"},
                                  {"Extend","NVarChar","4000"},
                                  //{"score_before","Money","16"},
                                 };
            return Tablelist;
        }
        public override SqlParameter[] GetParameters()
        {
            M_UserExpHis model = this;
            SqlParameter[] sp = GetSP();
            sp[0].Value = model.ExpHisID;
            sp[1].Value = model.UserID;
            sp[2].Value = model.Operator;
            sp[3].Value = model.detail;
            sp[4].Value = model.score;
            sp[5].Value = model.HisTime;
            sp[6].Value = model.OperatorIP;
            sp[7].Value = model.ScoreType;
            sp[8].Value = model.Remark;
            sp[9].Value = model.Attach;
            sp[10].Value = model.Extend;
            //sp[11].Value = model.score_before;
            return sp;
        }
        public M_UserExpHis GetModelFromReader(DbDataReader rdr)
        {
            M_UserExpHis model = new M_UserExpHis();
            model.ExpHisID = Convert.ToInt32(rdr["ExpHisID"]);
            model.UserID = Convert.ToInt32(rdr["UserID"]);
            model.Operator = ConvertToInt(rdr["Operator"]);
            model.detail = ConverToStr(rdr["detail"]);
            model.score = Convert.ToDouble(rdr["score"]);
            model.HisTime = ConvertToDate(rdr["HisTime"]);
            model.OperatorIP = ConverToStr(rdr["OperatorIP"]);
            model.ScoreType = Convert.ToInt32(rdr["ScoreType"]);
            model.Remark = ConverToStr(rdr["Remark"]);
            model.Attach = ConverToStr(rdr["Attach"]);
            model.Extend = ConverToStr(rdr["Extend"]);
            //model.score_before = ConverToDouble(rdr["score_before"]);
            rdr.Close();
            return model;
        }
    }
}
