using System;using System.Data.Common;
using System.Collections.Generic;

using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace ZoomLa.Model.MIS
{
    /// <summary>
    /// 文档传阅信息
    /// </summary>
    public class M_OA_DocView : M_Base
    {

        public int ID { get; set; }
        public int DocID { get; set; }
        /// <summary>
        /// 可浏览该文档用户IDS -100为全部
        /// </summary>
        public string ViewUserIDS { get; set; }
        /// <summary>
        /// 可编辑该文档用户[预留]
        /// </summary>
        public string EditUserIDS { get; set; }
        /// <summary>
        /// 状态[预留]
        /// </summary>
        public int ZStatus { get; set; }
        /// <summary>
        /// 创建人ID
        /// </summary>
        public int UserID { get; set; }
        public DateTime CDate { get; set; }
        /// <summary>
        /// 传阅终止日期[预留]
        /// </summary>
        public DateTime EndDate { get; set; }

        public override string TbName { get { return "ZL_OA_DocView"; } }
        public override string PK { get { return "ID"; } }
        public override string[,] FieldList()
        {
            string[,] Tablelist = {
        		        		{"ID","Int","4"},
        		        		{"DocID","Int","4"},
        		        		{"ViewUserIDS","VarChar","8000"},
        		        		{"EditUserIDS","VarChar","8000"},
        		        		{"ZStatus","Int","4"},
        		        		{"UserID","Int","4"},
        		        		{"CDate","DateTime","8"},
        		        		{"EndDate","DateTime","8"}
        };
            return Tablelist;
        }

        public override SqlParameter[] GetParameters()
        {
            M_OA_DocView model = this;
            if (model.CDate <= DateTime.MinValue) { model.CDate = DateTime.Now; }
            if (model.EndDate <= DateTime.MinValue) { model.EndDate = DateTime.MaxValue; }
            SqlParameter[] sp = GetSP();
            sp[0].Value = model.ID;
            sp[1].Value = model.DocID;
            sp[2].Value = model.ViewUserIDS;
            sp[3].Value = model.EditUserIDS;
            sp[4].Value = model.ZStatus;
            sp[5].Value = model.UserID;
            sp[6].Value = model.CDate;
            sp[7].Value = model.EndDate;
            return sp;
        }
        public M_OA_DocView GetModelFromReader(DbDataReader rdr)
        {
            M_OA_DocView model = new M_OA_DocView();
            model.ID = ConvertToInt(rdr["ID"]);
            model.DocID = ConvertToInt(rdr["DocID"]);
            model.ViewUserIDS = ConverToStr(rdr["ViewUserIDS"]);
            model.EditUserIDS = ConverToStr(rdr["EditUserIDS"]);
            model.ZStatus = ConvertToInt(rdr["ZStatus"]);
            model.UserID = ConvertToInt(rdr["UserID"]);
            model.CDate = ConvertToDate(rdr["CDate"]);
            model.EndDate = ConvertToDate(rdr["EndDate"]);
            rdr.Close();
            return model;
        }
    }
}
