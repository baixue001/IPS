using System;using System.Data.Common;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace ZoomLa.Model
{
    public class M_OA_UserConfig:M_Base
    {
        string _leftChk, _mainChk, _popChk;
        public int ID { get; set; }
        public int UserID { get; set; }
        /// <summary>
        /// 左边栏
        /// </summary>
        public string LeftChk
        {
            get { _leftChk = string.IsNullOrEmpty(_leftChk) ? "" : "," + (_leftChk.Trim(',')) + ","; return _leftChk; }
            set { _leftChk = value; }
        }
        /// <summary>
        /// 主界面
        /// </summary>
        public string MainChk
        {
            get { _mainChk = string.IsNullOrEmpty(_mainChk) ? "" : "," + (_mainChk.Trim(',')) + ","; return _mainChk; }
            set { _mainChk = value; }
        }
        /// <summary>
        /// 弹窗
        /// </summary>
        public string PopChk
        {
            get { _popChk = string.IsNullOrEmpty(_popChk) ? "" : "," + (_popChk.Trim(',')) + ","; return _popChk; }
            set { _popChk = value; }
        }
        public int Status { get; set; }
        public string Remind { get; set; }
        public bool IsNull { get; set; }
        public bool HasAuth(string s1,string s2) 
        {
            return s1.IndexOf("," + s2 + ",") > -1;
        }
        public M_OA_UserConfig()
        {

        }
        public override string TbName { get { return "ZL_OA_UserConfig"; }}
        public override string[,] FieldList()
        {
            string[,] Tablelist = {
        	            {"ID","Int","4"},            
                        {"UserID","Int","4"},            
                        {"LeftChk","NVarChar","300"},            
                        {"MainChk","NVarChar","300"},            
                        {"PopChk","NVarChar","300"},            
                        {"Status","Int","4"},            
                        {"Remind","NVarChar","100"}            
              
        };
            return Tablelist;
        }
        public override SqlParameter[] GetParameters()
        {
            var model = this;
            SqlParameter[] sp = GetSP();
            sp[0].Value = model.ID;
            sp[1].Value = model.UserID;
            sp[2].Value = model.LeftChk;
            sp[3].Value = model.MainChk;
            sp[4].Value = model.PopChk;
            sp[5].Value = model.Status;
            sp[6].Value = model.Remind;
            return sp;
        }
        public M_OA_UserConfig GetModelFromReader(DbDataReader rdr)
        {
            M_OA_UserConfig model = new M_OA_UserConfig();
            model.ID = Convert.ToInt32(rdr["ID"]);
            model.UserID = Convert.ToInt32(rdr["UserID"]);
            model.LeftChk = rdr["LeftChk"].ToString();
            model.MainChk = rdr["MainChk"].ToString();
            model.PopChk = rdr["PopChk"].ToString();
            model.Status = Convert.ToInt32(rdr["Status"]);
            model.Remind = rdr["Remind"].ToString();
            rdr.Close();
            rdr.Dispose();
            return model;
        }
    }
}
