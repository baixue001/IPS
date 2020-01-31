using System;
using System.Data;
using System.Configuration;
using System.Web;
using ZoomLa.Model;
using ZoomLa.Common;
using System.Net.Mail;
using System.Collections.Generic;
using ZoomLa.Components;
using ZoomLa.SQLDAL;
using System.Data.SqlClient;
using System.Data.Common;
using ZoomLa.SQLDAL.SQL;

namespace ZoomLa.BLL
{


    public class B_IServer
    {
        public string PK, strTableName;
        public M_IServer initMod = new M_IServer();
        public B_IServer()
        {
            strTableName = initMod.TbName;
            PK = initMod.PK;
        }
        public int Insert(M_IServer model)
        {
            return DBCenter.Insert(model);
        }
        public M_IServer SelReturnModel(int id)
        {
            using (DbDataReader reader = DBCenter.SelReturnReader(strTableName, PK, id))
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
            return Sql.Sel(strTableName);
        }
        public bool UpdateByID(M_IServer model)
        {
            return DBCenter.UpdateByID(model, model.QuestionId);
        }
        public void UpdateState(int id, int state)
        {
            if (id < 1) { return; }
            string stateStr = GetStatus(state);
            DBCenter.UpdateSQL(strTableName, "State=@state", PK+"=" + id
                , new List<SqlParameter>() { new SqlParameter("state", stateStr) });
        }
        public bool DeleteById(int ID)
        {
            return Sql.Del(strTableName, "QuestionId=" + ID.ToString());
        }
        /// <summary>
        /// 根据用户名查询问题分类（分组查询）
        /// 当userid小于0时，则查询所有
        /// </summary>
        /// <param name="userid">用户id</param>
        /// <returns>return DateTable</returns>
        public DataTable GetSeachUserIdType(int userid)
        {
            string sql = "SELECT Type FROM ZL_IServer where 1=1 ";
            SqlParameter[] cmdParams = new SqlParameter[1];
            cmdParams[0] = new SqlParameter("@UserId", SqlDbType.VarChar);
            if (userid > 0)
            {
                sql += " and UserId=@UserId";
                cmdParams[0].Value = userid;
            }
            sql += " group by Type";
            return SqlHelper.ExecuteTable(CommandType.Text, sql, cmdParams);
        }
        public PageSetting SelPage(int cpage,int psize,F_IServer filter)
        {
            //int cpage, int psize, int qid = -100, int uid = -100, string title = "", string state = "", 
            //string type = "", int orderid = 0, string root = "", string priority = "", DateTime? BetweenSubTime = null, DateTime? ToSubTime = null

            string where = " 1=1";
            List<SqlParameter> sp = new List<SqlParameter>();
            if (filter.qid != -100) { where += " AND  A.QuestionId=" + filter.qid; }
            if (filter.uid != -100) { where += " AND A.UserId=" + filter.uid; }
            if (!string.IsNullOrEmpty(filter.title)) { where += " AND A.Title LIKE @Title"; sp.Add(new SqlParameter("Title", "%" + filter.title + "%")); }
            if (!string.IsNullOrEmpty(filter.state)) { where += " AND A.State = @State"; sp.Add(new SqlParameter("State", filter.state.Trim())); }
            if (!string.IsNullOrEmpty(filter.type)) { where += " AND A.Type = @Type"; sp.Add(new SqlParameter("Type", filter.type)); }
            if (filter.oid > 0) { where += " AND A.OrderType=" + filter.oid; }
            if (!string.IsNullOrEmpty(filter.root)) { where += " AND A.Root = @Root"; sp.Add(new SqlParameter("Root", filter.root)); }
            if (!string.IsNullOrEmpty(filter.priority)) { where += " AND A.Priority=@Priority"; sp.Add(new SqlParameter("Priority", filter.priority)); }
            if (!string.IsNullOrEmpty(filter.ccuser))
            {
                where += " AND (CCUser IS NOT NULL AND CCUser='" + DataConvert.CLng(filter.ccuser) + "')";
            }
            //if (BetweenSubTime != null && ToSubTime != null)
            //{
            //    where += " AND A.SubTime between @BetweenSubTime and @ToSubTime";
            //    sp.Add(new SqlParameter("BetweenSubTime", BetweenSubTime));
            //    sp.Add(new SqlParameter("ToSubTime", ToSubTime));
            //}
            PageSetting setting = PageSetting.Double(cpage, psize, strTableName, "ZL_User", "A." + PK, "A.UserId=B.UserID", where, "A.QuestionID DESC", sp,
                "A.*,(SELECT GroupName FROM ZL_Group WHERE GroupID=B.GroupID) GroupName");
            DBCenter.SelPage(setting);
            return setting;
        }
        /// <summary>
        /// 根据问题类型查询相应的总数
        /// </summary>
        /// <param name="state">状态</param>
        /// <param name="UserId">用户id</param>
        /// <param name="type">问题的类型</param>
        /// <returns></returns>
        public int getiServerNum(string state, int UserId, string type, int orderid)
        {
            List<SqlParameter> list = new List<SqlParameter>();
            string where = "1=1";
            if (!string.IsNullOrEmpty(state))
            {
                where = where + " AND State=@State ";
                list.Add(new SqlParameter("state", state));
            }
            if (!string.IsNullOrEmpty(type))
            {
                where += " AND Type=@type";
                list.Add(new SqlParameter("type", type));
            }
            if (UserId > 0) { where += " AND UserId=" + UserId; }
            if (orderid > 0) { where += " AND OrderType=" + orderid; }
            return DBCenter.Count("ZL_IServer", where, list);
        }
        /// <summary>
        /// 根据问题类型查询相应的前5条数据
        /// </summary>
        /// <param name="state">状态</param>
        /// <param name="UserId">用户id</param>
        /// <param name="orderall">排序字段</param>
        /// <param name="type">问题的类型</param>
        /// <returns></returns>
        public DataTable SeachTop(string state, int userid, string type = "", int orderid = 0)
        {
            string sql = "SELECT TOP 5 * FROM ZL_IServer WHERE 1=1";
            List<SqlParameter> sp = new List<SqlParameter>();
            if (!string.IsNullOrEmpty(state)) { sql += " AND State=@state"; sp.Add(new SqlParameter("state", state)); }
            if (userid > 0) { sql += " AND UserID=" + userid; }
            if (!string.IsNullOrEmpty(type)) { sql += " AND Type=@type"; sp.Add(new SqlParameter("type", type)); }
            if (orderid > 0) { sql += " AND OrderType=" + orderid; }
            sql += " ORDER BY SubTime DESC";

            return SqlHelper.ExecuteTable(CommandType.Text, sql, sp.ToArray());
        }
        #region XML Type
        private string[] _typeArr = null;
        /// <summary>
        /// 兼容之前的代码
        /// </summary>
        public string[] TypeArr
        {
            get
            {
                if (_typeArr != null) { return _typeArr; }
                List<string> typeList = new List<string>();
                DataSet ds = new DataSet();
                ds.ReadXml(function.VToP("/Config/XMLDB/IServer.config"));
                if (ds == null || ds.Tables.Count < 1) { _typeArr = typeList.ToArray(); }
                else
                {
                    foreach (DataRow dr in ds.Tables["type"].Rows)
                    {
                        if (string.IsNullOrEmpty(DataConvert.CStr(dr["name"]))) { continue; }
                        typeList.Add(dr["name"].ToString());
                    }
                    _typeArr = typeList.ToArray();
                }
                return _typeArr;
            }
        }
        public DataTable TypeDT
        {
            get
            {
                DataTable dt = new DataTable();
                dt.Columns.Add(new DataColumn("id", typeof(int)));
                dt.Columns.Add(new DataColumn("name", typeof(string)));
                for (int i = 0; i < TypeArr.Length; i++)
                {
                    DataRow dr = dt.NewRow();
                    dr["id"] = (i + 1);
                    dr["name"] = TypeArr[i];
                    dt.Rows.Add(dr);
                }
                return dt;
            }
        }
        public int GetTypeIndex(string type)
        {
            for (int i = 0; i < TypeArr.Length; i++)
            {
                if (TypeArr[i].Equals(type)) { return i; }
            }
            return 0;
        }
        public string GetStatus(int num)
        {
            if (num == -100) { return ""; }
            foreach (DataRow dr in State_DT.Rows)
            {
                if (DataConvert.CLng(dr["num"]) == num) { return dr["name"].ToString(); }
            }
            return num.ToString();
        }
        public int Status_GetInt(string state)
        {
            if (string.IsNullOrEmpty(state)) { return -100; }
            foreach (DataRow dr in State_DT.Rows)
            {
                if (dr["name"].ToString().Equals(state)) { return DataConvert.CLng(dr["num"]); }
            }
            return -100;
        }
        private DataTable _state_dt = null;
        public DataTable State_DT
        {
            get
            {
                if (_state_dt == null)
                {
                    string[] dtArr = "未解决:0,未解决:1,处理中:2,已解决:3,已锁定:4,已关闭:-1,已删除:-2"
                        .Split(',');
                    DataTable dt = new DataTable();
                    dt.Columns.Add(new DataColumn("name"));
                    dt.Columns.Add(new DataColumn("num"));
                    foreach (string state in dtArr)
                    {
                        DataRow dr = dt.NewRow();
                        dr["name"] = state.Split(':')[0];
                        dr["num"] = state.Split(':')[1];
                        dt.Rows.Add(dr);
                    }
                    _state_dt = dt;
                }
                return _state_dt;
            }
        }
        #endregion
    }
    public class F_IServer
    {
        public int uid = -100;
        public string ccuser = "";
        public string title = "";
        /// <summary>
        /// 问题所处的状态
        /// </summary>
        public string state = "";
        /// <summary>
        /// 问题所属分类
        /// </summary>
        public string type = "";
        /// <summary>
        /// 指定订单的问题
        /// </summary>
        public int oid = 0;
        public int qid = -100;
        /// <summary>
        /// 提交的来源
        /// </summary>
        public string root = "";
        /// <summary>
        /// 需处理的优先级
        /// </summary>
        public string priority = "";
    }
}
