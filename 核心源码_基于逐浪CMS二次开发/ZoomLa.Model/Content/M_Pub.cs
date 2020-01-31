using System;using System.Data.Common;
using System.Data.SqlClient;
using System.Data;
namespace ZoomLa.Model
{
    [Serializable]
    public class M_Pub:M_Base
    {
        #region 定义字段
        public int Pubid { get; set; }
        /// <summary>
        /// 互动名称
        /// </summary>
        public string PubName { get; set; }
        /// <summary>
        /// 信息类别(0-内容 1-商城 2-黄页 3-店铺 4-会员，5-节点,6-首页)
        /// </summary>
        public int PubClass { get; set; }
        /// <summary>
        /// 互动数据表名
        /// </summary>
        public string PubTableName { get; set; }
        /// <summary>
        /// 互动类型(0-评论 1-投票 2-活动 3-留言 4-问券调查 5-通用统计 8-互动表单)
        /// </summary>
        public int PubType { get; set; }
        /// <summary>
        /// 模板地址
        /// </summary>
        public string PubTemplate { get; set; }
        /// <summary>
        /// 调用字符
        /// </summary>
        public string PubLoadstr { get; set; }
        /// <summary>
        /// 模型ID
        /// </summary>
        public int PubModelID { get; set; }
        /// <summary>
        /// 节点ID
        /// </summary>
        public string PubNodeID { get; set; }
        /// <summary>
        /// 互动模板ID
        /// </summary>
        public string PubTemplateID { get; set; }
        /// <summary>
        /// 说明
        /// </summary>
        public string Pubinfo { get; set; }
        /// <summary>
        /// 是否需要审核(0-不需要 1-需要审核)
        /// </summary>
        public int PubIsTrue { get; set; }
        /// <summary>
        /// 是否需要验证码(0-不需要 1-需要)
        /// </summary>
        public int PubCode { get; set; }
        /// <summary>
        /// 是否需要登陆(0-不需要 1-需要)
        /// </summary>
        public int PubLogin { get; set; }
        /// <summary>
        /// 参与人数
        /// </summary>
        public int PubAddnum { get; set; }
        /// <summary>
        /// 绑定互动
        /// </summary>
        public int PubBindPub { get; set; }
        /// <summary>
        /// 投票显示方式(0-传统 1-柱状 2-圆形)
        /// </summary>
        public int PubShowType { get; set; }
        /// <summary>
        /// 提交窗口模板
        /// </summary>
        public string PubInputTM { get; set; }
        /// <summary>
        /// 调用提交窗口代码
        /// </summary>
        public string PubInputLoadStr { get; set; }
        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime PubCreateTime { get; set; }
        /// <summary>
        /// 设置登陆地址
        /// </summary>
        public string PubLoginUrl { get; set; }
        /// <summary>
        /// 是否开启评论
        /// </summary>
        public int PubOpenComment { get; set; }
        /// <summary>
        /// 结束时间
        /// </summary>
        public DateTime PubEndTime { get; set; }
        /// <summary>
        /// 是否被删除 1-删除
        /// </summary>
        public int PubIsDel { get; set; }
        /// <summary>
        /// 保留天数信息
        /// </summary>
        public int Pubkeep { get; set; }
        /// <summary>
        /// 达到提交次数提示
        /// </summary>
        public string Puberrmsg { get; set; }
        /// <summary>
        /// 更新跳转地址
        /// </summary>
        public string PubGourl { get; set; }
        /// <summary>
        /// 0-未公开,公开
        /// </summary>
        public int Public { get; set; }
        /// <summary>
        /// 前台权限, //Look,Edit,Del,Sen:查看，修改，删除,审核
        /// </summary>
        public string PubPermissions{ get;set; }
        /// <summary>
        /// ip,cookie,user
        /// </summary>
        public string VerifyMode { get; set; }
        /// <summary>
        /// 再次提交间隔,秒为单位
        /// </summary>
        public int Interval { get; set; }
        /// <summary>
        /// 用户可提交次数(0为不限)
        /// </summary>
        public int PubIPOneOrMore { get; set; }
        #endregion
        //---------disuse
        /// <summary>
        /// 是否多条(0-无限制 1-每人限一条)
        /// </summary>
        public int PubOneOrMore { get; set; }
        public int PubFlag { get; set; }
        //Disuse,改用于CookieFlag
        private int PubSiteID { get; set; }
        /// <summary>
        /// 0:不进行Cookie校验,>0每个可提交几次,主用于移动端设备防重复提交
        /// 页面：if (!getCookie("pubflag")) {setCookie("pubflag", GetRandomNum(12));}
        /// </summary>
        private int CookieFlag { get { return PubSiteID; } set { PubSiteID = value; } }
        /// <summary>
        /// 重用于校验Cookies,PugFlag
        /// </summary>
        public int PubTimeSlot { get; set; }


        public M_Pub()
        {
            this.PubAddnum = 0;
            this.PubInputTM = "";
            this.PubNodeID = "";
            this.PubTemplateID = "";
            this.PubIsDel = 0;
            this.Pubkeep = 9999;
            this.Public = 1;
            this.PubCreateTime = DateTime.Now;
            this.PubEndTime = DateTime.Now;
            this.PubOpenComment = 0;
            this.PubIsTrue = 0;
            this.PubIPOneOrMore = 0;
            this.Interval = 0;
            this.Puberrmsg = "该用户提交信息的数量已经达到上限!";
            this.PubCode = 0;
            this.PubLogin = 0;
            this.PubType = 0;
            this.PubClass = 0;
            this.PubPermissions = "";
            this.VerifyMode = "";
        }
        public override string PK { get { return "Pubid"; } }
        public override string TbName { get { return "ZL_Pub"; } }
        public override string[,] FieldList() { return FieldList2(); }
        public static string[,] FieldList2()
        {
            string[,] Tablelist = {
                                  {"Pubid","Int","4"},
                                  {"PubName","NVarChar","255"},
                                  {"PubClass","Int","4"},
                                  {"PubTableName","NVarChar","255"},
                                  {"PubType","Int","4"}, 
                                  {"PubOneOrMore","Int","4"},
                                  {"PubIPOneOrMore","Int","4"},
                                  {"PubTemplate","NVarChar","1000"}, 
                                  {"PubLoadstr","NVarChar","1000"},
                                  {"PubModelID","Int","4"},
                                  {"PubNodeID","NVarChar","1000"},
                                  {"PubTemplateID","NVarChar","1000"}, 
                                  {"Pubinfo","NText","20000"},
                                  {"PubIsTrue","Int","4"},
                                  {"PubCode","Int","4"},
                                  {"PubLogin","Int","4"}, 
                                  {"PubAddnum","Int","4"},
                                  {"PubBindPub","Int","4"},
                                  {"PubShowType","Int","4"}, 
                                  {"PubInputTM","NVarChar","1000"},
                                  {"PubInputLoadStr","NVarChar","255"},
                                  {"PubCreateTime","DateTime","8"},
                                  {"PubLoginUrl","NVarChar","255"},
                                  {"PubOpenComment","Int","4"}, 
                                  {"PubEndTime","DateTime","8"},
                                  {"PubIsDel","Int","4"},
                                  {"PubTimeSlot","Int","4"}, 
                                  {"Pubkeep","Int","4"},
                                  {"Puberrmsg","NVarChar","1000"},
                                  {"PubGourl","NVarChar","1000"},
                                  {"PubSiteID","Int","4"}, 
                                  {"Public","Int","4"},
                                  {"PubPermissions","NVarChar","255"},
                                  {"Interval","NVarChar","20"},
                                  {"PubFlag","Int","4"},
                                  {"VerifyMode","NVarChar","500"}
                                 };
            return Tablelist;
        }
        public override SqlParameter[] GetParameters()
        {
            M_Pub model = this;
            SqlParameter[] sp = GetSP();
            sp[0].Value = model.Pubid;
            sp[1].Value = model.PubName;
            sp[2].Value = model.PubClass;
            sp[3].Value = model.PubTableName;
            sp[4].Value = model.PubType;
            sp[5].Value = model.PubOneOrMore;
            sp[6].Value = model.PubIPOneOrMore;
            sp[7].Value = model.PubTemplate;
            sp[8].Value = model.PubLoadstr;
            sp[9].Value = model.PubModelID;
            sp[10].Value = model.PubNodeID;
            sp[11].Value = model.PubTemplateID;
            sp[12].Value = model.Pubinfo;
            sp[13].Value = model.PubIsTrue;
            sp[14].Value = model.PubCode;
            sp[15].Value = model.PubLogin;
            sp[16].Value = model.PubAddnum;
            sp[17].Value = model.PubBindPub;
            sp[18].Value = model.PubShowType;
            sp[19].Value = model.PubInputTM;
            sp[20].Value = model.PubInputLoadStr;
            sp[21].Value = model.PubCreateTime;
            sp[22].Value = model.PubLoginUrl;
            sp[23].Value = model.PubOpenComment;
            sp[24].Value = model.PubEndTime;
            sp[25].Value = model.PubIsDel;
            sp[26].Value = model.PubTimeSlot;
            sp[27].Value = model.Pubkeep;
            sp[28].Value = model.Puberrmsg;
            sp[29].Value = model.PubGourl;
            sp[30].Value = model.PubSiteID;
            sp[31].Value = model.Public;
            sp[32].Value = model.PubPermissions;
            sp[33].Value = model.Interval;
            sp[34].Value = model.PubFlag;
            sp[35].Value = model.VerifyMode;
            return sp;
        }
        public M_Pub GetModelFromReader(DbDataReader rdr)
        {
            M_Pub model = new M_Pub();
            model.Pubid = Convert.ToInt32(rdr["Pubid"]);
            model.PubName = ConverToStr(rdr["PubName"]);
            model.PubClass = ConvertToInt(rdr["PubClass"]);
            model.PubTableName = ConverToStr(rdr["PubTableName"]);
            model.PubType = ConvertToInt(rdr["PubType"]);
            model.PubOneOrMore = ConvertToInt(rdr["PubOneOrMore"]);
            model.PubIPOneOrMore = ConvertToInt(rdr["PubIPOneOrMore"]);
            model.PubTemplate = ConverToStr(rdr["PubTemplate"]);
            model.PubLoadstr = ConverToStr(rdr["PubLoadstr"]);
            model.PubModelID = ConvertToInt(rdr["PubModelID"]);
            model.PubNodeID = ConverToStr(rdr["PubNodeID"]);
            model.PubTemplateID = rdr["PubTemplateID"].ToString();
            model.Pubinfo = ConverToStr(rdr["Pubinfo"]);
            model.PubIsTrue = ConvertToInt(rdr["PubIsTrue"]);
            model.PubCode = ConvertToInt(rdr["PubCode"]);
            model.PubLogin = ConvertToInt(rdr["PubLogin"]);
            model.PubAddnum = ConvertToInt(rdr["PubAddnum"]);
            model.PubBindPub = ConvertToInt(rdr["PubBindPub"]);
            model.PubShowType = ConvertToInt(rdr["PubShowType"]);
            model.PubInputTM = ConverToStr(rdr["PubInputTM"]);
            model.PubInputLoadStr = ConverToStr(rdr["PubInputLoadStr"]);
            model.PubCreateTime = ConvertToDate(rdr["PubCreateTime"]);
            model.PubLoginUrl =ConverToStr(rdr["PubLoginUrl"]);
            model.PubOpenComment = ConvertToInt(rdr["PubOpenComment"]);
            model.PubEndTime = ConvertToDate(rdr["PubEndTime"]);
            model.PubIsDel = ConvertToInt(rdr["PubIsDel"]);
            model.PubTimeSlot = ConvertToInt(rdr["PubTimeSlot"]);
            model.Pubkeep = ConvertToInt(rdr["Pubkeep"]);
            model.Puberrmsg = ConverToStr(rdr["Puberrmsg"]);
            model.PubGourl = ConverToStr(rdr["PubGourl"]);
            model.PubSiteID = ConvertToInt(rdr["PubSiteID"]);
            model.Public = ConvertToInt(rdr["Public"]);
            model.PubPermissions = ConverToStr(rdr["PubPermissions"]);
            model.Interval = ConvertToInt(rdr["Interval"]);
            model.PubFlag = ConvertToInt(rdr["PubFlag"]);
            model.VerifyMode = ConverToStr(rdr["VerifyMode"]);
            rdr.Close();
            return model;
        }
    }
}