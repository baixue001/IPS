using System;using System.Data.Common;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace ZoomLa.Model
{
    public class M_Mis_AppProg:M_Base
    {
        public int ID { get; set; }
        public int AppID { get; set; }
        public int ProID { get; set; }
        /// <summary>
        /// 当前处于流程的第几级
        /// </summary>
        public int ProLevel { get; set; }
        public string ProLevelName { get; set; }
        /// <summary>
        /// 审批人(UserID)
        /// </summary>
        public int ApproveID { get; set; }
        /// <summary>
        /// ResultText
        /// </summary>
        public int Result { get; set; }
        /// <summary>
        /// 对Result的解析
        /// </summary>
        public string ResultText 
        {
            get 
            {
               return GetResult(Result); 
            }
        }
        public string GetResult(int rs) 
        {
            string r = "";
            switch (rs)
            {
                case -1:
                    r = "不同意";
                    break;
                case 0:
                    r = "协办人审批";
                    break;
                case 2:
                    r = "回退";
                    break;
                case 99:
                    r = "同意";
                    break;
                case 98:
                    r = "同意";
                    break;
                default:
                    r = "";
                    break;
            }
            return r;
        }
        public string Remind { get; set; }
        public DateTime CreateTime { get; set; }
        /// <summary>
        /// 系统生成的备注信息,记录回退等操作
        /// </summary>
        public string Remind2 { get; set; }
        /// <summary>
        /// 已会签人员(仅主办人才会在此)
        /// </summary>
        public string HQUserID { get; set; }
        /// <summary>
        /// 签章,格式见M_OA_Document
        /// </summary>
        public string Sign { get; set; }
        /// <summary>
        /// 签章ID
        /// </summary>
        public string SignID { get; set; }
        /// <summary>
        /// 预留
        /// </summary>
        public int Status{ get; set; }
        /// <summary>
        /// ccuser|refer|helpuser
        /// </summary>
        public string UserStepRole {get;set; }
        public M_Mis_AppProg()
        {

        }
        public override string TbName {  get { return "ZL_Mis_AppProg"; } }
        public override string[,] FieldList()
        {
            string[,] Tablelist = {
        		        		{"ID","Int","4"},
        		        		{"AppID","Int","4"},
        		        		{"ProID","Int","4"},
                                {"ProLevel","Int","4"},
                                {"ProLevelName","NVarChar","50"},
        		        		{"ApproveID","Int","4"},
        		        		{"Result","Int","4"},
        		        		{"Remind","NVarChar","255"},
        		        		{"CreateTime","DateTime","8"},
                                {"Remind2","NVarChar","100"},
                                {"HQUserID","NVarChar","300"},
                                {"Sign","NVarChar","50"},
                                {"SignID","NVarChar","50"},
                                {"Status","Int","4"},
                                {"UserStepRole","NVarChar","100"}
        };
            return Tablelist;
        }
        public override SqlParameter[] GetParameters()
        {
            SqlParameter[] sp = GetSP();
            M_Mis_AppProg model=this;
            if (model.CreateTime <= DateTime.MinValue) {model.CreateTime=DateTime.Now; }
            sp[0].Value = model.ID;
            sp[1].Value = model.AppID;
            sp[2].Value = model.ProID;
            sp[3].Value = model.ProLevel;
            sp[4].Value = model.ProLevelName;
            sp[5].Value = model.ApproveID;
            sp[6].Value = model.Result;
            sp[7].Value = model.Remind;
            sp[8].Value = model.CreateTime;
            sp[9].Value = model.Remind2;
            sp[10].Value = model.HQUserID;
            sp[11].Value = model.Sign;
            sp[12].Value = model.SignID;
            sp[13].Value = model.Status;
            sp[14].Value = model.UserStepRole;
            return sp;
        }
        public M_Mis_AppProg GetModelFromReader(DbDataReader rdr)
        {
            M_Mis_AppProg model = new M_Mis_AppProg();
            model.ID = Convert.ToInt32(rdr["ID"]);
            model.AppID = Convert.ToInt32(rdr["AppID"]);
            model.ProID = Convert.ToInt32(rdr["ProID"]);
            model.ProLevel = Convert.ToInt32(rdr["ProLevel"]);
            model.ProLevelName = ConverToStr(rdr["ProLevelName"]);
            model.ApproveID = Convert.ToInt32(rdr["ApproveID"]);
            model.Result = Convert.ToInt32(rdr["Result"]);
            model.Remind = ConverToStr(rdr["Remind"]);
            model.CreateTime = ConvertToDate(rdr["CreateTime"]);
            model.Remind2 = ConverToStr(rdr["Remind2"]);
            model.HQUserID = ConverToStr(rdr["HQUserID"]);
            model.Sign = ConverToStr(rdr["Sign"]);
            model.SignID = ConverToStr(rdr["SignID"]);
            model.Status = ConvertToInt(rdr["Status"]);
            model.UserStepRole = ConverToStr(rdr["UserStepRole"]);
            rdr.Dispose();
            return model;
        }
    }
}
