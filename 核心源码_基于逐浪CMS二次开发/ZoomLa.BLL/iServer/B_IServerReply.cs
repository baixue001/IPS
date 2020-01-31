using System;
using System.Data;
using System.Configuration;
using System.Web;
using ZoomLa.Model;
using ZoomLa.Common;
using System.Net.Mail;
using System.Collections.Generic;
using ZoomLa.Components;
using System.Data.SqlClient;
using ZoomLa.SQLDAL;
using ZoomLa.SQLDAL.SQL;
using System.Data.Common;

namespace ZoomLa.BLL
{


    public class B_IServerReply
    {
        public B_IServerReply()
        {
            TbName = initMod.TbName;
            PK = initMod.PK;
        }
        public string PK, TbName;
        private M_IServerReply initMod = new M_IServerReply();

        public M_IServerReply SelReturnModel(int ID)
        {
            using (DbDataReader reader = DBCenter.SelReturnReader(TbName, PK, ID))
            {
                if (reader.Read())
                {
                    return initMod.GetModelFromReader(reader);
                }
                else
                {
                    return null;
                }
            }
        }
        //添加回复信息
        public bool Add(M_IServerReply model)
        {
            return DBCenter.Insert(model) > 0;
        }
        //根据问题Id查询回复
        public DataTable SeachById(int QuestionId)
        {
            return DBCenter.Sel(TbName, "QuestionId=" + QuestionId, PK + " ASC");
        }
        public PageSetting SelPage(int cpage, int psize, int QuestionId = -100)
        {
            string where = " 1=1";
            if (QuestionId != -100) { where += " AND QuestionId = " + QuestionId; }
            PageSetting setting = PageSetting.Single(cpage, psize, TbName, PK, where, "ReplyTime ASC");
            DBCenter.SelPage(setting);
            return setting;
        }
        //编辑回复内容
        public bool UpdateByID(M_IServerReply model)
        {
            return DBCenter.UpdateByID(model, model.Id);
        }
        public void DelByIDS(string ids)
        {
            if (string.IsNullOrEmpty(ids)) { return; }
            SafeSC.CheckIDSEx(ids);
            DBCenter.DelByIDS(TbName, PK, ids);
        }
        /// <summary>
        /// 通过提问者查询
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public DataTable GetSelOUserid(int userId, int num)
        {
            string sqlStr = "SELECT top " + num + " b.*,a.* FROM ZL_IServerReply as a,ZL_IServer as b WHERE a.QuestionId =b.QuestionId AND b.userId =@userId Order by ReplyTime DESC";
            SqlParameter[] cmdParams = new SqlParameter[] {
                new SqlParameter("@userId", SqlDbType.Int),
            };
            cmdParams[0].Value = userId;

            return SqlHelper.ExecuteTable(CommandType.Text, sqlStr, cmdParams);
        }

        /// <summary>
        /// 更新是否已读的状态
        /// </summary>
        /// <param name="state">是否已读</param>
        /// <param name="questionId">问题ID</param>
        /// <returns></returns>
        public bool GetUpdataState(int state, int questionId)
        {
            string sqlStr = "Update ZL_IServerReply set isRead=@isRead where QuestionId=@QuestionId";
            SqlParameter[] para = new SqlParameter[]{
                new SqlParameter("@isRead",state),
                new SqlParameter("@QuestionId",questionId)
            };
            return SqlHelper.ExecuteSql(sqlStr, para);
        }

        /// <summary>
        /// 获取未读数量
        /// </summary>
        /// <param name="isread"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public DataTable GetReadMess(int isread, int userId)
        {
            string sqlStr = "SELECT b.*,a.* FROM ZL_IServerReply as a,ZL_IServer as b WHERE a.questionid=b.questionid AND b.userId=" + userId + "AND a.isRead=" + isread;
            return SqlHelper.ExecuteTable(CommandType.Text, sqlStr);
        }
    }
}
