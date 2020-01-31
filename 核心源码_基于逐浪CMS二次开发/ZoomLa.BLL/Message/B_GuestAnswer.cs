using System;
using System.Collections.Generic;
using System.Text;
using ZoomLa.Model;
using ZoomLa.SQLDAL;
using System.Data.SqlClient;
using System.Data;
using ZoomLa.SQLDAL.SQL;
using System.Data.Common;

namespace ZoomLa.BLL
{
    public class B_GuestAnswer
    {
        private string strTableName, PK;
        private M_GuestAnswer initMod = new M_GuestAnswer();
        public B_GuestAnswer()
        {
            strTableName = initMod.TbName;
            PK = initMod.PK;
        }
        /// <summary>
        /// 通过问题的ID找答案
        /// </summary>
        /// <param name="QuestionID">问题的ID</param>
        /// <returns></returns>
        public DataTable GetAnswersByQid(int Qid)
        {
            string strSql = "select * from ZL_GuestAnswer where QueId=@QueId  order by ID Desc";
            SqlParameter[] sp = new SqlParameter[] { new SqlParameter("QueId", Qid) };
            return DBCenter.DB.ExecuteTable(new SqlModel(strSql, sp));
        }
        //使用当中
        public DataTable Sel(string strWhere, string strOrderby, SqlParameter[] sp)
        {
            List<SqlParameter> list = new List<SqlParameter>();
            if (sp != null) { list.AddRange(sp); }
            return DBCenter.Sel(strTableName, strWhere, strOrderby,list);
        }
        /// <summary>
        /// 查询记录数
        /// </summary>
        /// <returns></returns>
        public int getnum()
        {
            string sql = "SELECT COUNT(*) AS guestCount FROM ZL_GuestAnswer";
            DataTable dt = DBCenter.DB.ExecuteTable(new SqlModel(sql,null));
            return DataConvert.CLng(dt.Rows[0]["guestCount"]);
        }
        /// <summary>
        /// 根据条件查询记录数
        /// </summary>
        /// <returns></returns>
        public int IsExistInt(int status)
        {
            string sql = "SELECT COUNT(*) AS guestCount FROM ZL_GuestAnswer WHERE Status=" + status;
            DataTable dt = DBCenter.DB.ExecuteTable(new SqlModel(sql,null));
            return DataConvert.CLng(dt.Rows[0]["guestCount"]);
        }
        public M_GuestAnswer SelReturnModel(int ID)
        {
            using (DbDataReader reader = DBCenter.SelReturnReader(strTableName, PK, ID))
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
        /// <summary>
        /// 该用户在指定问题下的回答
        /// </summary>
        public M_GuestAnswer SelModelByAsk(int askId, int uid)
        {
            using (DbDataReader reader = DBCenter.SelReturnReader(strTableName, "QueId="+askId+" AND UserId="+uid, "ID DESC",null))
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
        public DataTable Sel()
        {
            return DBCenter.Sel(strTableName);
        }
        public bool UpdateByID(M_GuestAnswer model)
        {
            return DBCenter.UpdateByID(model,model.ID);
        }
        public bool UpdateStatus(int audit, int id)
        {
            return UpdateStatus(audit,id.ToString());
        }
        public bool UpdateStatus(int status, string ids)
        {
            if (!string.IsNullOrEmpty(ids)) { return false; }
            SafeSC.CheckIDSEx(ids);
            return DBCenter.UpdateSQL(strTableName, "Status=" + status, "ID IN (" + ids + ")");
        }
        public bool Del(int ID)
        {
            return DBCenter.Del(strTableName,PK, ID);
        }
        public bool DelByQueID(int qid)
        {
            SqlParameter[] sp = new SqlParameter[] { new SqlParameter("qid", qid) };
            string sqlStr = "Delete From " + strTableName + " Where QueID=@qid";
            DBCenter.DB.ExecuteNonQuery(new SqlModel(sqlStr, sp));
            return true;
        }
        public bool Del(string strWhere)
        {
            return DBCenter.DelByWhere(strTableName, strWhere);
        }
        public int insert(M_GuestAnswer model)
        {
            return DBCenter.Insert(model);
        }


        //----------------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// 根据用户id获得答案数量
        /// </summary>
        /// <param name="uid"></param>
        /// <returns></returns>
        public int GetAnserCount(int uid)
        {
            return DBCenter.Count(strTableName, "UserID=" + uid);
        }
        /// <summary>
        /// 根据问题id获取答案数量
        /// </summary>
        /// <returns></returns>
        public int GetAnserCountByQid(int qid)
        {
            return DBCenter.Count(strTableName, "QueID=" + qid);
        }
        public int GetCountByStatus(int status)
        {
            return DBCenter.Count(strTableName, "Status=" + status);
        }
        /// <summary>
        /// 获取用户回答过的问题IDS
        /// </summary>
        /// <returns></returns>
        public string GetUserAnswerIDS(int userid,int status = -100)
        {
            string sql = "SELECT QueId FROM ZL_GuestAnswer WHERE UserID=" + userid;
            if (status != -100) { sql += " AND status = " + status; }
            sql += " GROUP BY QueId";
            DataTable dt = DBCenter.DB.ExecuteTable(new SqlModel(sql,null));
            string ids = "";
            foreach (DataRow dr in dt.Rows)
            {
                ids += dr["QueId"] + ",";
            }
            return ids.Trim(',');
        }
    }
}

