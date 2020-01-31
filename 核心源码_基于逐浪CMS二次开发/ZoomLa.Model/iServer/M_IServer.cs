using System;using System.Data.Common;
using System.Data.SqlClient;
using System.Data;

using System.Web;

namespace ZoomLa.Model
{
    [Serializable]
    public class M_IServer : M_Base
    {
        public M_IServer() { }
        public M_IServer(bool value)
        {
            this.m_IsNull = value;
        }
        #region 属性定义
        public int QuestionId
        {
            get;
            set;
        }
        public int UserId
        {
            get;
            set;
        }
        public string UserName { get; set; }
        /// <summary>
        /// 标题
        /// </summary>
        public string Title
        {
            get;
            set;
        }
        /// <summary>
        /// 内容
        /// </summary>
        public string Content
        {
            get;
            set;
        }

        /// <summary>
        /// 问题优先级
        /// </summary>
        public string Priority
        {
            get;
            set;
        }

        /// <summary>
        /// 问题类型
        /// </summary>
        public string Type
        {
            get;
            set;
        }

        /// <summary>
        /// 已读次数
        /// </summary>
        public int ReadCount
        {
            get;
            set;
        }

        /// <summary>
        /// 提交时间
        /// </summary>
        public DateTime SubTime
        {
            get;
            set;
        }

        /// <summary>
        /// 问题状态
        /// </summary>
        public string State
        {
            get;
            set;
        }

        /// <summary>
        /// 解决时间
        /// </summary>
        public DateTime SolveTime
        {
            get;
            set;
        }

        /// <summary>
        /// 附件地址
        /// </summary>
        public string Path
        {
            get;
            set;
        }
        /// <summary>
        /// 问题来源
        /// </summary>
        public string Root
        {
            get;
            set;
        }
        /// <summary>
        /// 来源附件信息
        /// </summary>
        public string RootInfo
        {
            get;
            set;
        }

        /// <summary>
        /// 是否阅读
        /// </summary>
        private bool m_IsNull = false;
        /// <summary>
        /// 是否为空
        /// </summary>
        public bool IsNull { get { return this.m_IsNull; } }
        public DateTime RequestTime { get; set; }
        /// <summary>
        /// 订单类型(0-非订单)
        /// </summary>
        public int OrderType { get; set; }
        /// <summary>
        /// 后台管理员备注
        /// </summary>
        public string Remind { get; set; }
        /// <summary>
        /// 需要操作的用户IDS,被抄送用户也可回答问题
        /// </summary>
        public string CCUser { get; set; }
        #endregion
        public override string PK { get { return "QuestionId"; } }
        public override string TbName { get { return "ZL_IServer"; } }
        public override string[,] FieldList()
        {
            string[,] Tablelist = {
                                  {"QuestionId","Int","4"},
                                  {"UserId","Int","4"},
                                  {"Title","NVarChar","100"},
                                  {"Content","NText","1000"},
                                  {"Priority","NVarChar","50"},
                                  {"Type","NVarChar","50"},
                                  {"ReadCount","Int","4"},
                                  {"SubTime","DateTime","8"},
                                  {"State","NVarChar","50"},
                                  {"SolveTime","DateTime","8"},
                                  {"path","NVarChar","1000"},
                                  {"Root","NVarChar","50"},
                                  {"RootInfo","NVarChar","50"},
                                  {"RequestTime","DateTime","8"},
                                  {"OrderType","Int","4"},
                                  {"Remind","NVarChar","500"},
                                  {"CCUser","NVarChar","4000"},
                                  {"UserName","NVarChar","100"}
                                 };
            return Tablelist;
        }
        public override SqlParameter[] GetParameters()
        {
            M_IServer model = this;
            if (model.RequestTime <= DateTime.MinValue) model.RequestTime = DateTime.Now.AddDays(1);
            if (model.SubTime <= DateTime.MinValue) model.SubTime = DateTime.Now;
            if (model.SolveTime <= DateTime.MinValue) model.SolveTime = DateTime.Now;
            model.Type = HttpUtility.HtmlEncode(model.Type);
            model.Title = HttpUtility.HtmlEncode(model.Title);
            model.Root = HttpUtility.HtmlEncode(model.Root);
            model.State = HttpUtility.HtmlEncode(model.State);
            model.Priority = HttpUtility.HtmlEncode(model.Priority);
            SqlParameter[] sp = GetSP();
            sp[0].Value = model.QuestionId;
            sp[1].Value = model.UserId;
            sp[2].Value = model.Title;
            sp[3].Value = model.Content;
            sp[4].Value = model.Priority;
            sp[5].Value = model.Type;
            sp[6].Value = model.ReadCount;
            sp[7].Value = model.SubTime;
            sp[8].Value = model.State;
            sp[9].Value = model.SolveTime;
            sp[10].Value = model.Path;
            sp[11].Value = model.Root;
            sp[12].Value = model.RootInfo;
            sp[13].Value = model.RequestTime;
            sp[14].Value = model.OrderType;
            sp[15].Value = model.Remind;
            sp[16].Value = model.CCUser;
            sp[17].Value = model.UserName;
            return sp;
        }
        public M_IServer GetModelFromReader(DbDataReader rdr)
        {
            M_IServer model = new M_IServer();
            model.QuestionId = Convert.ToInt32(rdr["QuestionId"]);
            model.UserId = ConvertToInt(rdr["UserId"]);
            model.Title = ConverToStr(rdr["Title"]);
            model.Content = ConverToStr(rdr["Content"]);
            model.Priority = ConverToStr(rdr["Priority"]);
            model.Type = ConverToStr(rdr["Type"]);
            model.ReadCount = ConvertToInt(rdr["ReadCount"]);
            model.SubTime = ConvertToDate(rdr["SubTime"]);
            model.State = ConverToStr(rdr["State"]);
            model.SolveTime = ConvertToDate(rdr["SolveTime"]);
            model.Path = ConverToStr(rdr["path"]);
            model.Root = ConverToStr(rdr["Root"]);
            model.RootInfo = ConverToStr(rdr["RootInfo"]);
            model.RequestTime = ConvertToDate(rdr["RequestTime"]);
            model.OrderType = ConvertToInt(rdr["OrderType"]);
            model.Remind = ConverToStr(rdr["Remind"]);
            model.CCUser = ConverToStr(rdr["CCUser"]);
            model.UserName = ConverToStr(rdr["UserName"]);
            rdr.Dispose();
            return model;
        }
    }
}