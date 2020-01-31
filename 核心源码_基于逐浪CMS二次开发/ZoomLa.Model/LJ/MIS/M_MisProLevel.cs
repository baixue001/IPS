using System;using System.Data.Common;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using Newtonsoft.Json;
using ZoomLa.Model;

namespace ZoomLa.Model
{
    [Serializable]
    public class M_MisProLevel:M_Base
    {
        public M_MisProLevel()
        {
            this.ReferGroup = "";
            this.CCGroup = "";
            this.EmailAlert = "";
            this.EmailGroup = "";
            this.SmsAlert = "";
            this.SmsGroup = "";
            this.Status = 1;
            this.CanEditField = "*";
            this.CreateTime = DateTime.Now;
            this.PublicAttachOption = 1;
            this.PrivateAttachOption = 1;
            this.CCUser_Allow = 1;
            this.HelpUser_Allow = 0;
            this.stepNum = 1;
            this.ParentID = 0;
            this.StepAuth = new M_OA_StepAuth();
        }
        private string _referUser = "", _ccUser = "", _helpUser = "", _emailAlert = "", _smsAlert = "", _sendMan = "", _referGroup = "", _ccGroup = "", _emailGroup = "", _smsGroup = "";
        public int ID { get; set; }
        /// <summary>
        /// 所属流程ID
        /// </summary>
        public int ProID { get; set; }
        /// <summary>
        /// 用于Free步骤表,记录来源的记录
        /// </summary>
        public int OrginStepID { get; set; }
        /// <summary>
        /// 序号
        /// </summary>
        public int stepNum { get; set; }
        /// <summary>
        /// 步骤名
        /// </summary>
        public string stepName { get; set; }
        /// <summary>
        /// 经办人
        /// </summary>
        public string ReferUser
        {
            get { _referUser =string.IsNullOrEmpty(_referUser)?"": "," + (_referUser.Trim(',')) + ","; return _referUser; }
            set { _referUser = value; }
        }
        /// <summary>
        /// 经办组[disuse]
        /// </summary>
        public string ReferGroup
        {
            get { _referGroup = string.IsNullOrEmpty(_referGroup) ? "" : "," + (_referGroup.Trim(',')) + ","; return _referGroup; }
            set {  _referGroup = value; }
        }
        /// <summary>
        /// 抄送人
        /// </summary>
        public string CCUser
        {
            get { _ccUser = string.IsNullOrEmpty(_ccUser) ? "" : "," + (_ccUser.Trim(',')) + ","; return _ccUser; }
            set { _ccUser = value; }
        }
        /// <summary>
        /// 抄送组[disuse]
        /// </summary>
        public string CCGroup
        {
            get { _ccGroup =string.IsNullOrEmpty(_ccGroup)?"": "," + (_ccGroup.Trim(',')) + ","; return _ccGroup; }
            set { _ccGroup = value; }
        }
        /// <summary>
        /// 辅办人
        /// </summary>
        public string HelpUser
        {
            get { _helpUser = string.IsNullOrEmpty(_helpUser) ? "" : "," + (_helpUser.Trim(',')) + ","; return _helpUser; }
            set { _helpUser = value; }
        }
        /// <summary>
        /// 会签选项0:任意一人即可,1:必须所有人会签
        /// </summary>
        public int HQoption 
        {
            get;set;
        }
        /// <summary>
        /// 强制转交
        /// </summary>
        public int Qzzjoption { get; set; }
        /// <summary>
        /// 回退选项0:不允许,1:允许回到步一步,2:允许回到之前步骤
        /// </summary>
        public int HToption { get; set; }
        /// <summary>
        /// 转交流程时,需email提醒人
        /// </summary>
        public string EmailAlert
        {
            get { _emailAlert = string.IsNullOrEmpty(_emailAlert) ? "" : "," + (_emailAlert.Trim(',')) + ","; return _emailAlert; }
            set { _emailAlert = value; }
        }
        /// <summary>
        /// 转交流程时,需email提醒组
        /// </summary>
        public string EmailGroup
        {
            get { _emailGroup = string.IsNullOrEmpty(_emailGroup) ? "" : "," + (_emailGroup.Trim(',')) + ","; return _emailGroup; }
            set { _emailGroup = value; }
        }
        /// <summary>
        /// 转交流程时,需短信提醒人
        /// </summary>
        public string SmsAlert
        {
            get { _smsAlert = string.IsNullOrEmpty(_smsAlert) ? "" : "," + (_smsAlert.Trim(',')) + ",";return _smsAlert; }
            set { _smsAlert = value; }
        }
        /// <summary>
        /// 转交流程时,需短信提醒组
        /// </summary>
        public string SmsGroup
        {
            get { _smsGroup = string.IsNullOrEmpty(_smsGroup) ? "" : "," + (_smsGroup.Trim(',')) + ",";return _smsGroup; }
            set { _smsGroup = value; }
        }
        /// <summary>
        /// 原预留,现用于自由流程,存储文档ID
        /// </summary>
        public int BackOption { get; set; }
        /// <summary>
        /// 公共附件选项0:不允许,1:允许
        /// </summary>
        public int PublicAttachOption { get; set; }
        /// <summary>
        /// 私人附件选项[预留]
        /// </summary>
        public int PrivateAttachOption { get; set; }
        /// <summary>
        /// 状态[预留]
        /// </summary>
        public int Status { get; set; }
        public DateTime CreateTime { get; set; }
        public string Remind { get; set; }
        /// <summary>
        /// 允许发起人[disuse],改为在流程中设置而不是步骤
        /// </summary>
        public string SendMan
        {
            get { _sendMan = "," + (_sendMan.Trim(',')) + ","; return _sendMan; }
            set { _sendMan = value; }
        }
        /// <summary>
        /// 当前步骤下,允许主办人修改的字段列表[disuse]
        /// </summary>
        public string CanEditField
        {
            get;
            set;
        }
        /// <summary>
        /// 已改为,下一步骤权限,refer:经办人,sender:主办人,all:经办和主办(自由流程为主办人权限)
        /// </summary>
        public string DocAuth { get; set; }
        /// <summary>
        /// 该步骤结束后,可执行的下一步特殊操作,文件呈阅|归档(直接终止流程)|传阅
        /// </summary>
        public string NextOP { get; set; }
        private string referuser_alias = "", _ccuser_alias = "", _helpuser_alias = "";
        /// <summary>
        /// 主办人别名,无则为主办人
        /// </summary>
        public string ReferUser_Alias { get { return string.IsNullOrEmpty(referuser_alias) ? "主办人" : referuser_alias; } set { referuser_alias = value; } }
        public string CCUser_Alias { get { return string.IsNullOrEmpty(_ccuser_alias) ? "协办人" : _ccuser_alias; } set { _ccuser_alias = value; } }
        public string HelpUser_Alias { get { return string.IsNullOrEmpty(_helpuser_alias) ? "辅办人" : _helpuser_alias; } set { _helpuser_alias = value; } }
        /// <summary>
        /// 是否允许协办,1:是
        /// </summary>
        public int CCUser_Allow { get; set; }
        /// <summary>
        /// 是否允许辅办,1:是
        /// </summary>
        public int HelpUser_Allow { get; set; }
        /// <summary>
        /// 权限json数据,避免增加字段(仅用于flowaudit中需要处理的特殊权限,用于主办,辅办,辅办)
        /// (便于扩展权限,与单独进行处理)
        /// </summary>
        public M_OA_StepAuth StepAuth{ get; set; }
        /// <summary>
        /// 所属父步骤,用于创建分支,分支下再根据序号
        /// </summary>
        public int ParentID {get;set; }
        public override string TbName { get { return "ZL_MisProLevel"; } }
        public override string UP_Tables
        {
            get
            {
               return "ZL_MisProLevel,ZL_OA_FreePro";
            }
        }
        public override string[,] FieldList()
        {
            string[,] Tablelist = {
                        {"ID","Int","4"},
        	            {"ProID","Int","4"},            
                        {"stepNum","Int","4"},            
                        {"stepName","NVarChar","50"},            
                        {"ReferUser","VarChar","8000"},
                        {"CCUser","VarChar","8000"}, 
                        {"HQoption","Int","4"},            
                        {"Qzzjoption","Int","4"},            
                        {"HToption","Int","4"},            
                        {"EmailAlert","VarChar","8000"},
                        {"SmsAlert","VarChar","8000"},
                        {"BackOption","Int","4"},            
                        {"PublicAttachOption","Int","4"},            
                        {"PrivateAttachOption","Int","4"},            
                        {"Status","Int","4"},            
                        {"CreateTime","DateTime","8"},            
                        {"Remind","NVarChar","100"} ,
                        {"SendMan","VarChar","8000"},
                        {"ReferGroup","VarChar","8000"},
                        {"CCGroup","VarChar","8000"},
                        {"EmailGroup","VarChar","8000"},
                        {"SmsGroup","VarChar","8000"},
                        {"CanEditField","VarChar","1000"},
                        {"DocAuth","VarChar","500"},
                        {"ReferUser_Alias","NVarChar","50"},
                        {"CCUser_Alias","NVarChar","50"},
                        {"CCUser_Allow","Int","4"},
                        {"NextOP","NVarChar","4000"},
                        {"HelpUser_Alias","NVarChar","50"},
                        {"HelpUser_Allow","Int","4"},
                        {"HelpUser","VarChar","8000"},
                        {"StepAuth","NText","20000"},
                        {"ParentID","Int","4"},
                        {"OrginStepID","Int","4"}
                        
              
        };
            return Tablelist;
        }
        public override SqlParameter[] GetParameters()
        {
            M_MisProLevel model = this;
            if (string.IsNullOrEmpty(model.DocAuth)) { model.DocAuth = "all"; }
            if (model.CreateTime <= DateTime.MinValue) { model.CreateTime = DateTime.Now; }
            if (model.StepAuth == null) { model.StepAuth = new M_OA_StepAuth(); }
            SqlParameter[] sp = GetSP();
            sp[0].Value = model.ID;
            sp[1].Value = model.ProID;
            sp[2].Value = model.stepNum;
            sp[3].Value = model.stepName;
            sp[4].Value = model.ReferUser;
            sp[5].Value = model.CCUser;
            sp[6].Value = model.HQoption;
            sp[7].Value = model.Qzzjoption;
            sp[8].Value = model.HToption;
            sp[9].Value = model.EmailAlert;
            sp[10].Value = model.SmsAlert;
            sp[11].Value = model.BackOption;
            sp[12].Value = model.PublicAttachOption;
            sp[13].Value = model.PrivateAttachOption;
            sp[14].Value = model.Status;
            sp[15].Value = model.CreateTime;
            sp[16].Value = model.Remind;
            sp[17].Value = model.SendMan;
            sp[18].Value = model.ReferGroup;
            sp[19].Value = model.CCGroup;
            sp[20].Value = model.EmailGroup;
            sp[21].Value = model.SmsGroup;
            sp[22].Value = model.CanEditField;
            sp[23].Value = model.DocAuth;
            sp[24].Value = model.ReferUser_Alias;
            sp[25].Value = model.CCUser_Alias;
            sp[26].Value = model.CCUser_Allow;
            sp[27].Value = model.NextOP;
            sp[28].Value = model.HelpUser_Alias;
            sp[29].Value = model.HelpUser_Allow;
            sp[30].Value = model.HelpUser;
            sp[31].Value = JsonConvert.SerializeObject(model.StepAuth);
            sp[32].Value = model.ParentID;
            sp[33].Value = model.OrginStepID;
            return sp;
        }
        public M_MisProLevel GetModelFromReader(DbDataReader rdr)
        {
            M_MisProLevel model = new M_MisProLevel();
            model.ID = Convert.ToInt32(rdr["ID"]);
            model.ProID = Convert.ToInt32(rdr["ProID"]);
            model.stepNum = ConvertToInt(rdr["stepNum"]);
            model.stepName = ConverToStr(rdr["stepName"]);
            model.ReferUser = ConverToStr(rdr["ReferUser"]);
            model.CCUser = ConverToStr(rdr["CCUser"]);
            model.HQoption = ConvertToInt(rdr["HQoption"]);
            model.Qzzjoption = ConvertToInt(rdr["Qzzjoption"]);
            model.HToption = ConvertToInt(rdr["HToption"]);
            model.EmailAlert = ConverToStr(rdr["EmailAlert"]);
            model.SmsAlert = ConverToStr(rdr["SmsAlert"]);
            model.BackOption = ConvertToInt(rdr["BackOption"]);
            model.PublicAttachOption = ConvertToInt(rdr["PublicAttachOption"]);
            model.PrivateAttachOption = ConvertToInt(rdr["PrivateAttachOption"]);
            model.Status = ConvertToInt(rdr["Status"]);
            model.CreateTime = ConvertToDate(rdr["CreateTime"]);
            model.Remind = ConverToStr(rdr["Remind"]);
            model.SendMan = ConverToStr(rdr["SendMan"]);
            model.ReferGroup = ConverToStr(rdr["ReferGroup"]);
            model.CCGroup = ConverToStr(rdr["CCGroup"]);
            model.EmailGroup = ConverToStr(rdr["EmailGroup"]);
            model.SmsGroup = ConverToStr(rdr["SmsGroup"]);
            model.CanEditField = ConverToStr(rdr["CanEditField"]);
            model.DocAuth = ConverToStr(rdr["DocAuth"]);
            model.ReferUser_Alias = ConverToStr(rdr["ReferUser_Alias"]);
            model.CCUser_Alias = ConverToStr(rdr["CCUser_Alias"]);
            model.CCUser_Allow = ConvertToInt(rdr["CCUser_Allow"]);
            model.NextOP = ConverToStr(rdr["NextOP"]);
            model.HelpUser_Alias = ConverToStr(rdr["HelpUser_Alias"]);
            model.HelpUser_Allow = ConvertToInt(rdr["HelpUser_Allow"]);
            model.HelpUser = ConverToStr(rdr["HelpUser"]);
            model.StepAuth = string.IsNullOrEmpty(ConverToStr(rdr["StepAuth"])) ? new M_OA_StepAuth() : JsonConvert.DeserializeObject<M_OA_StepAuth>(ConverToStr(rdr["StepAuth"]));
            model.ParentID=ConvertToInt(rdr["ParentID"]);
            model.OrginStepID=ConvertToInt(rdr["OrginStepID"]);
            rdr.Close();
            return model;
        }
        public M_MisProLevel GetModelFromDR(DataRow rdr)
        {
            M_MisProLevel model = new M_MisProLevel();
            model.ID = Convert.ToInt32(rdr["ID"]);
            model.ProID = Convert.ToInt32(rdr["ProID"]);
            model.stepNum = Convert.ToInt32(rdr["stepNum"]);
            model.stepName = ConverToStr(rdr["stepName"]);
            model.ReferUser = ConverToStr(rdr["ReferUser"]);
            model.CCUser = ConverToStr(rdr["CCUser"]);
            model.HQoption = Convert.ToInt32(rdr["HQoption"]);
            model.Qzzjoption = Convert.ToInt32(rdr["Qzzjoption"]);
            model.HToption = Convert.ToInt32(rdr["HToption"]);
            model.EmailAlert = ConverToStr(rdr["EmailAlert"]);
            model.SmsAlert = ConverToStr(rdr["SmsAlert"]);
            model.BackOption = Convert.ToInt32(rdr["BackOption"]);
            model.PublicAttachOption = Convert.ToInt32(rdr["PublicAttachOption"]);
            model.PrivateAttachOption = Convert.ToInt32(rdr["PrivateAttachOption"]);
            model.Status = Convert.ToInt32(rdr["Status"]);
            model.CreateTime = ConvertToDate(rdr["CreateTime"]);
            model.Remind = ConverToStr(rdr["Remind"]);
            model.SendMan = ConverToStr(rdr["SendMan"]);
            model.ReferGroup = ConverToStr(rdr["ReferGroup"]);
            model.CCGroup = ConverToStr(rdr["CCGroup"]);
            model.EmailGroup = ConverToStr(rdr["EmailGroup"]);
            model.SmsGroup = ConverToStr(rdr["SmsGroup"]);
            model.CanEditField = ConverToStr(rdr["CanEditField"]);
            model.DocAuth = ConverToStr(rdr["DocAuth"]);
            model.ReferUser_Alias = ConverToStr(rdr["ReferUser_Alias"]);
            model.CCUser_Alias = ConverToStr(rdr["CCUser_Alias"]);
            model.CCUser_Allow = ConvertToInt(rdr["CCUser_Allow"]);
            model.NextOP = ConverToStr(rdr["NextOP"]);
            model.HelpUser_Alias = ConverToStr(rdr["HelpUser_Alias"]);
            model.HelpUser_Allow = ConvertToInt(rdr["HelpUser_Allow"]);
            model.HelpUser = ConverToStr(rdr["HelpUser"]);
            model.StepAuth = string.IsNullOrEmpty(ConverToStr(rdr["StepAuth"])) ? new M_OA_StepAuth() : JsonConvert.DeserializeObject<M_OA_StepAuth>(ConverToStr(rdr["StepAuth"]));
            model.ParentID = ConvertToInt(rdr["ParentID"]);
            model.OrginStepID= ConvertToInt(rdr["OrginStepID"]);
            return model;
        }
        /// <summary>
        /// 权限验证,后期移入BLL中
        /// </summary>
        /// <param name="e">需要验证的权限</param>
        /// <param name="userDT">用户信息表</param>
        public bool Auth(AuthEnum e, M_UserInfo mu)
        {
            if (mu.IsNull || mu.UserID < 1) { return false; }
            string uids = "";//, gids = "";
            switch (e)
            {
                case AuthEnum.Refer:
                    uids = this.ReferUser;
                    break;
                case AuthEnum.CCUser:
                    uids = this.CCUser;
                    break;
                case AuthEnum.HelpUser:
                    uids = this.HelpUser;
                    break;
            }
            if (string.IsNullOrEmpty(uids)) {return false; }
            return uids.Contains("," + mu.UserID + ",");
        }
        public enum AuthEnum { Refer, CCUser, HelpUser };
    }
    /// <summary>
    /// 将步骤与权限配置分离
    /// </summary>
    [Serializable]
    public class M_OA_StepAuth
    {
        /// <summary>
        /// 主办人可删除与新增附件
        /// </summary>
        public bool RUser_ManageAttach = false;
        /// <summary>
        /// 主办人是否可修改协办与辅办
        /// </summary>
        public bool RUser_ChangeUser = false;
        /// <summary>
        /// 协办批复权限
        /// </summary>
        public bool CCUser_Write = true;
        /// <summary>
        /// 协办是否也必须会签
        /// </summary>
        public bool CCUser_HQ = false;
        /// <summary>
        /// 辅办批复权限
        /// </summary>
        public bool HUser_Write = true;
    }
}
