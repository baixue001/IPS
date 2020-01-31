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


    public class B_Message
    {
        public B_Message()
        {
            strTableName = initmod.TbName;
            PK = initmod.PK;
        }
        public string PK, strTableName;
        private M_Message initmod = new M_Message();
        public DataTable Sel()
        {
            return DBCenter.Sel(strTableName);
        }
        public DataTable Sel(string type, string skey)
        {
            string where = " 1=1 ";
            List<SqlParameter> sp = new List<SqlParameter>();
            switch (type)
            {
                case "all":
                    break;
                case "sys":
                    where += " AND MsgType = 1";
                    break;
                case "mb":
                    where += " AND MsgType = 2";
                    break;
                case "code":
                    where += "AND MsgType = 3";
                    break;
            }
            if (!string.IsNullOrEmpty(skey)) { where += " AND Title LIKE @skey"; sp.Add(new SqlParameter("skey", "%" + skey + "%")); }
            return DBCenter.Sel(strTableName, where, " PostDate DESC", sp);
        }
        public PageSetting SelPage(int cpage, int psize, F_Message filter)
        {
            string where = " 1=1 ", suid = "'%," + filter.uid + ",%'";
            List<SqlParameter> sp = new List<SqlParameter>();
            if (filter.msgType != -100) { where += " AND MsgType=" + filter.msgType; }
            if (!string.IsNullOrEmpty(filter.title)) { where += " AND Title LIKE @title"; sp.Add(new SqlParameter("title", "%" + filter.title + "%")); }
            switch (filter.filter)
            {
                case "send"://我的发件箱
                    where += " AND Sender='" + filter.uid + "'"+NormalMail(filter.uid);
                    break;
                case "draft"://我的草稿
                    where += " AND Sender='" + filter.uid + "' AND SaveData=1 ";
                    break;
                case "rece"://我收到的邮件
                    where += " AND (Incept Like " + suid + " OR CCUser Like " + suid + ")" + NormalMail(filter.uid);
                    break;
                case "recycle"://我的已删除邮件
                    where += " AND Sender='" + filter.uid + "' AND Savedata=0 And DelIDS Like " + suid + " And (RealDelIDS Is Null OR RealDelIDS Not Like " + suid + ")";
                    break;
            }
            PageSetting setting = PageSetting.Single(cpage, psize, strTableName, PK, where, "MsgID DESC", sp);
            DBCenter.SelPage(setting);
            return setting;
        }
        public M_Message SelReturnModel(int ID)
        {
            using (DbDataReader reader = DBCenter.SelReturnReader(strTableName, PK, ID))
            {
                if (reader.Read())
                {
                    return initmod.GetModelFromReader(reader);
                }
                else
                {
                    return null;
                }
            }
        }
        public bool Del(int ID)
        {
            return DBCenter.Del(strTableName,PK,ID);
        }
        //增加信息
        public int GetInsert(M_Message model)
        {
            return DBCenter.Insert(model);
        }
        public static int Add(M_Message model) { return new B_Message().GetInsert(model); }
        public void UpdateByID(M_Message model)
        {
            DBCenter.UpdateByID(model, model.MsgID);
        }
        /// <summary>
        /// 放入垃圾箱-1删除,0收件删除,1发件删除,2:标为已读
        /// </summary>
        public bool DropByids(string ids, int flag = 0)
        {
            string sqlStr = "";
            switch (flag)
            {
                case 0:
                    sqlStr = "Update " + strTableName + " Set Status=1,IsDelInBox=1 Where MsgID in({0})";
                    break;
                case 1:
                    sqlStr = "Update " + strTableName + " Set IsDelSendbox=1 Where MsgID in({0})";
                    break;
                case -1:
                    sqlStr = "Update " + strTableName + " Set Status=-1 Where MsgID in({0})";
                    break;
                default:
                    break;
            }
            SafeSC.CheckIDSEx(ids);
            sqlStr = string.Format(sqlStr, ids);
            return SqlHelper.ExecuteSql(sqlStr);
        }
        /// <summary>
        /// 如自己仅是收件或抄送人,则从收件人中移除自己信息
        /// </summary>
        public void RemoveByUserID(string ids, int uid)
        {
            SafeSC.CheckIDSEx(ids);
            string[] idArr = ids.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            M_Message model = new M_Message();
            for (int i = 0; i < idArr.Length; i++)
            {
                int mid = Convert.ToInt32(idArr[i]);
                model = SelReturnModel(mid);
                model.Incept = model.Receipt.Replace("," + uid + ",", ",,").Replace(",,", ",");//从收件人和抄送人中移除自己
                model.CCUser = model.CCUser.Replace("," + uid + ",", ",,").Replace(",,", ",");
                UpdateByID(model);
            }
        }
        /// <summary>
        /// 恢复收件箱,发件箱,草稿等的被删除状态
        /// </summary>
        public bool ReCoverByIDS(string ids, int uid)
        {
            //SafeSC.CheckIDSEx(ids);
            string sql = "Update " + strTableName + " Set IsDelInbox=0 Where MsgID in ({0})";
            string sql2 = "Update " + strTableName + " Set Status=1,IsDelSendBox=0 Where MsgID in({0}) And Sender=" + uid;
            SafeSC.CheckIDSEx(ids);
            sql = string.Format(sql, ids);
            sql2 = string.Format(sql2, ids);
            SqlHelper.ExecuteSql(sql);
            return SqlHelper.ExecuteSql(sql2);
        }
        public static bool IsExit(string msgID)
        {
            string sqlStr = "SELECT count(*) FROM ZL_Message WHERE MsgID=" + msgID;
            return DataConvert.CLng(SqlHelper.ExecuteScalar(CommandType.Text, sqlStr)) > 0;
        }
        /// <summary>
        /// 是否允许其读这条信息
        /// </summary>
        public bool AllowRead(int msgID, string userID)
        {
            bool flag = false;
            string str = "," + userID + ",";
            M_Message model = SelReturnModel(msgID);
            if (model.Sender.Equals(userID) || model.Incept.IndexOf(str) > -1 || model.CCUser.IndexOf(str) > -1)//发件,收件,抄送人可阅读
            {
                flag = true;
            }
            return flag;
        }
        /// <summary>
        /// 获取用户未读的短信信息
        /// </summary>
        public DataTable SelUnreadMsgByUserID(int userID)
        {
            //string uid = "," + userID + ",";
            //dt = SqlHelper.ExecuteTable("Select MsgID From "+strTableName);
            //return dt;
            return null;
        }

        //----------------------New Logical
        /// <summary>
        /// 0:草稿,1:我的发件(未删除),2:我的收件(未删除),3:我的回收站,默认为收件箱
        /// </summary>
        public DataTable SelMyMail(int uid, int flag = 2, string skey = "")
        {
            //Savedata:0:正常,1:草稿
            List<SqlParameter> sp = new List<SqlParameter>();
            string where = "", suid = "'%," + uid + ",%'";
            switch (flag)
            {
                case 0://草稿(草稿删中删除,则删除数据库记录)
                    where += " Sender=" + uid + " And Savedata =1";
                    break;
                case 1://发件箱
                    where += " Sender=" + uid + NormalMail(uid);
                    break;
                case 2://收件箱
                    where += " (Incept Like " + suid + " OR CCUser Like " + suid + ")" + NormalMail(uid);
                    break;
                case 3://回收站
                    where += " Savedata=0 And DelIDS Like " + suid + " And (RealDelIDS Is Null OR RealDelIDS Not Like " + suid + ")";
                    break;
                case 4://我的所有带附件的发件,正常,草稿,回收站(用于邮箱容量统计),彻底删除的不算
                    where += " Sender='" + uid + "' And AttachMent !='' And (RealDelIDS Is Null OR RealDelIDS Not Like " + suid + ")";
                    break;
                default:
                    throw new Exception("错误的参数Flag:" + flag);
            }
            if (!string.IsNullOrEmpty(skey)) { where += " AND Title LIKE @skey"; sp.Add(new SqlParameter("skey", "%" + skey + "%")); }
            return DBCenter.Sel(strTableName, where, "MsgID DESC", sp);
        }
        //非草稿,非回收站,未删除的邮件
        private string NormalMail(int uid)
        {
            string suid = "'%," + uid + ",%'";
            string sql = " And Savedata=0 And (DelIDS Is Null OR DelIDS Not Like " + suid + ") And (RealDelIDS Is Null OR RealDelIDS Not Like " + suid + ")";
            return sql;
        }
        /// <summary>
        /// 恢复收件箱,发件箱
        /// </summary>
        public bool ReFromRecycle(string ids, int uid)
        {
            SafeSC.CheckIDSEx(ids);
            string suid = "'," + uid + ",'";
            string sql = "Update " + strTableName + " Set DelIDS=REPLACE(REPLACE(DelIDS," + suid + ",','),',,',',') Where MsgID in(" + ids + ")";//将用户的ID从DelIDS中移除,并且将,,替换为,保持格式正常
            return SqlHelper.ExecuteSql(sql);
        }
        public bool ReFromDraft(string ids, int uid)
        {
            SafeSC.CheckIDSEx(ids);
            string sql = "Update " + strTableName + " Set SaveData=0 Where MsgID in(" + ids + ") Where Sender=" + uid;
            return SqlHelper.ExecuteSql(sql);
        }
        /// <summary>
        /// 批量删除, 如用户量大，则改为存储过程
        /// </summary>
        public void DelByIDS(string ids, int uid)
        {
            if (string.IsNullOrEmpty(ids) || uid < 1) { return; }
            SafeSC.CheckIDSEx(ids);
            //如果是收件人等,则从记录中将其移除
            string suid = "'," + uid + ",'";
            DBCenter.UpdateSQL(strTableName, "DelIDS = ISNULL(DelIDS,'')+" + suid, "MsgID IN (" + ids + ")");
        }
        public void RealDelByID(int msgid, int uid)
        {
            RealDelByIDS(msgid.ToString(), uid);
        }
        /// <summary>
        /// 彻底删除,草稿(删记录)
        /// </summary>
        public void RealDelByIDS(string ids, int uid)
        {
            SafeSC.CheckIDSEx(ids);
            string suid = "'," + uid + ",'";
            string sql = "Update " + strTableName + " Set RealDelIDS =ISNULL(RealDelIDS,'') +" + suid + " Where " + PK + " in(" + ids + ")";
            SqlHelper.ExecuteSql(sql);
            DelAttachAndRecord(ids, uid);
        }
        /// <summary>
        /// 后台管理员移除邮件
        /// </summary>
        public void DelFromDB(string ids)
        {
            if (string.IsNullOrEmpty(ids)) { return; }
            SafeSC.CheckIDSEx(ids);
            DBCenter.DelByIDS(strTableName, PK, ids);
        }
        //删除数据库记录和附件(用于草稿)
        private void DelAttachAndRecord(string ids, int uid)
        {
            SafeSC.CheckIDSEx(ids);
            string sql = "Select MsgID,AttachMent From " + strTableName + " Where MsgID in(" + ids + ") And SaveData=1 And Sender =" + uid;
            DataTable dt = SqlHelper.ExecuteTable(CommandType.Text, sql);
            string needDel = DelAttach(dt);
            if (!string.IsNullOrEmpty(needDel))
            {
                needDel = needDel.TrimEnd(',');
                string delSql = "Delete From " + strTableName + " Where MsgID in(" + needDel + ")";
                SqlHelper.ExecuteSql(delSql);
            }
        }
        private string DelAttach(DataTable dt)
        {
            string needDel = "";
            foreach (DataRow dr in dt.Rows)
            {
                needDel += dr["MsgID"] + ",";
                if (string.IsNullOrEmpty(dr["AttachMent"].ToString())) continue;
                foreach (string vpath in dr["AttachMent"].ToString().Split(','))
                {
                    SafeSC.DelFile(vpath);
                }
            }
            return needDel;
        }
        //-----------------Tool
        /// <summary>
        /// 获取指定用户,是否有未读的邮件,如果有的话,则提示
        /// </summary>
        public DataTable GetUnReadMail(int uid)
        {
            string u = "," + uid + ",";
            string suid = "'%," + uid + ",%'";
            string sql = "Select MsgID,Title From " + strTableName + " Where (CCUser Like '%" + u + "%' OR Incept Like '%" + u + "%') And ReadUser Not Like " + suid + NormalMail(uid);
            return SqlHelper.ExecuteTable(CommandType.Text, sql);
        }
        /// <summary>
        /// 批量将邮件设为已阅读
        /// </summary>
        public void UnreadToRead(string ids, int uid)
        {
            SafeSC.CheckIDSEx(ids);
            string sql = "Update ZL_Message Set ReadUser=ReadUser+'," + uid + ",' Where msgid in (" + ids + ") And ReadUser not like '%," + uid + ",%'";
            SqlHelper.ExecuteSql(sql);
        }
        //-----后台使用
        /// <summary>
        /// 彻底删除数据库记录与相关附件
        /// </summary>
        public void DelByAdmin(string ids) //删除数据库记录与相关附件
        {
            SafeSC.CheckIDSEx(ids);
            string sql = "Select MsgID,AttachMent From " + strTableName + " Where MsgID in(" + ids + ")";
            DataTable dt = SqlHelper.ExecuteTable(CommandType.Text, sql);
            DelAttach(dt);
            string delSql = "Delete From " + strTableName + " Where MsgID in(" + ids + ")";
            SqlHelper.ExecuteSql(delSql);
        }
    }
    public class F_Message
    {
        //邮件类型
        public int msgType = -100;
        public string title = "";
        public int uid = -100;
        //快速筛选
        public string filter = "";
    }
}