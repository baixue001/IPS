using System;using System.Data.Common;
using System.Data.SqlClient;
using System.Data;


namespace ZoomLa.Model
{
    [Serializable]
    public class M_IServerReply : M_Base
    {
        public M_IServerReply() { }

        #region 属性定义
        public int Id { get; set; }

        /// <summary>
        ///是否为已读:0为否,1为是
        /// </summary>
        public int IsRead { get; set; }

        /// <summary>
        /// 问题编号
        /// </summary>
        public int QuestionId { get; set; }

        /// <summary>
        /// 用户人用户ID
        /// </summary>
        public int UserId { get; set; }
        public int AdminID { get; set; }
        /// <summary>
        /// 用户|管理员名称
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// 标题
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// 回复时间
        /// </summary>
        public DateTime ReplyTime { get; set; }

        /// <summary>
        /// 内容
        /// </summary>
        public string Content { get; set; }

        /// <summary>
        /// 附件地址
        /// </summary>
        public string Path { get; set; }
        #endregion

        public override string TbName { get { return "ZL_IServerReply"; } }
        public override string[,] FieldList()
        {
            string[,] Tablelist = {
                                  {"Id","Int","4"},
                                  {"QuestionId","Int","4"},
                                  {"UserId","Int","4"},
                                  {"Title","NVarChar","100"},
                                  {"ReplyTime","DateTime","8"},
                                  {"Content","NText","20000"},
                                  {"Path","NVarChar","1000"},
                                  {"isRead","Int","4"},
                                  {"AdminID","Int","4"},
                                  {"UserName","NVarChar","100"}
                                 };
            return Tablelist;
        }
        public override SqlParameter[] GetParameters()
        {
            M_IServerReply model = this;
            if (model.ReplyTime <= DateTime.MinValue) { model.ReplyTime = DateTime.Now; }
            SqlParameter[] sp = GetSP();
            sp[0].Value = model.Id;
            sp[1].Value = model.QuestionId;
            sp[2].Value = model.UserId;
            sp[3].Value = model.Title;
            sp[4].Value = model.ReplyTime;
            sp[5].Value = model.Content;
            sp[6].Value = model.Path;
            sp[7].Value = model.IsRead;
            sp[8].Value = model.AdminID;
            sp[9].Value = model.UserName;
            return sp;
        }

        public M_IServerReply GetModelFromReader(DbDataReader rdr)
        {
            M_IServerReply model = new M_IServerReply();
            model.Id = Convert.ToInt32(rdr["Id"]);
            model.QuestionId = ConvertToInt(rdr["QuestionId"]);
            model.UserId = ConvertToInt(rdr["UserId"]);
            model.Title = ConverToStr(rdr["Title"]);
            model.ReplyTime = ConvertToDate(rdr["ReplyTime"]);
            model.Content = ConverToStr(rdr["Content"]);
            model.Path = ConverToStr(rdr["Path"]);
            model.IsRead = ConvertToInt(rdr["isRead"]);
            model.AdminID = ConvertToInt(rdr["AdminID"]);
            model.UserName = ConverToStr(rdr["UserName"]);
            rdr.Close();
            return model;
        }
    }

}
