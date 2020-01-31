using System;
using System.Data;
using ZoomLa.Model;
using ZoomLa.Common;
using ZoomLa.SQLDAL;
using System.Data.SqlClient;
using System.Collections;
using ZoomLa.SQLDAL.SQL;
using System.Collections.Generic;
using System.Data.Common;
using ZoomLa.Model.Content;
using ZoomLa.BLL.Content;

namespace ZoomLa.BLL
{
    public class B_Pub
    {
        public const string PREFIX= "ZL_Pub_";
        private M_Pub initMod = new M_Pub();
        public B_Pub()
        {
            strTableName = initMod.TbName;
            PK = initMod.PK;
        }
        private string PK, strTableName;
        public M_Pub SelReturnModel(int ID)
        {
            using (DbDataReader reader = Sql.SelReturnReader(strTableName, PK, ID))
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
        public M_Pub GetSelect(int PubID)
        {
            return SelReturnModel(PubID);
        }
        //public M_Pub GetSelectNode(string nodeid)
        //{
        //    SqlParameter[] cmdParams = new SqlParameter[1];
        //    cmdParams[0] = new SqlParameter("@nodeid", SqlDbType.NVarChar, 255);
        //    cmdParams[0].Value = nodeid;
        //    string sql = "select * from ZL_pub where pubnodeid like '%,'+@nodeid+',%' or  pubnodeid like @nodeid+',%' or  pubnodeid like '%,'+@nodeid or pubnodeid=@nodeid";
        //    using (DbDataReader reader = SqlHelper.ExecuteReader(CommandType.Text, sql, cmdParams))
        //    {
        //        if (reader.Read())
        //        {
        //            return new M_Pub().GetModelFromReader(reader);
        //        }
        //        else
        //        {
        //            return new M_Pub();
        //        }
        //    }
        //}
        /// <summary>
        /// 根据模型id查询
        /// </summary>
        public M_Pub GetPubModel(int Model)
        {
            using (DbDataReader reader = DBCenter.SelReturnReader("ZL_Pub", "PubModelID=" + Model, "ID DESC"))
            {
                if (reader.Read())
                {
                    return initMod.GetModelFromReader(reader);
                }
                else
                {
                    return new M_Pub();
                }
            }
        }
        private M_Pub SelReturnModel(string strWhere)
        {
            using (DbDataReader reader = Sql.SelReturnReader(strTableName, strWhere))
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
        //------------------------------IU
        public int insert(M_Pub model)
        {
            return DBCenter.Insert(model);
        }
        public int GetInsert(M_Pub model)
        {
            return insert(model);
        }
        public bool UpdateByID(M_Pub model)
        {
            return DBCenter.UpdateByID(model, model.Pubid);
        }
        public bool GetUpdate(M_Pub model)
        {
            return DBCenter.UpdateByID(model, model.Pubid);
        }
        public bool InsertUpdate(M_Pub model)
        {
            if (model.Pubid > 0)
                UpdateByID(model);
            else
                insert(model);
            return true;
        }
        //------------------------------DEL
        public bool Del(int ID)
        {
            DelTableInfo(ID);
            return Sql.Del(strTableName, "PubID=" + ID);
        }
        public bool DelByIDS(string ids)
        {
            SafeSC.CheckIDSEx(ids);
            foreach (string id in ids.Split(','))//删除表
            {
                Del(DataConvert.CLng(id));
            }
            return true;
        }
        public bool RecyleByIDS(string ids, int del = 0)
        {
            if (string.IsNullOrEmpty(ids)) { return false; }
            SafeSC.CheckIDSEx(ids);
            return DBCenter.UpdateSQL(strTableName, "PubIsDel=" + del, "PubID IN (" + ids + ")");
        }
        //清空回收站
        public bool DelAll()
        {
            return DBCenter.DelByWhere(strTableName, " PubIsDel=1");
        }
        //-------------------------------Retrive
        //暂不处理
        public DataTable SelBy(string load, string input = "", string pid = "0")
        {
            string sql = "Select * From " + strTableName + " WHERE 1=1 ";
            if (!string.IsNullOrEmpty(load) && !string.IsNullOrEmpty(input))
            {
                sql += " And (PubLoadstr=@load or PubInputLoadStr=@input)";
            }
            else if (!string.IsNullOrEmpty(load))
            {
                sql += " And PubLoadstr=@load";
            }
            else
            {
                sql += " And PubInputLoadStr=@input";
            }
            if (DataConvert.CLng(pid) > 0)
            {
                sql += " And Pubid<>@pid";
            }
            SqlParameter[] sp = new SqlParameter[] { new SqlParameter("load", load), new SqlParameter("input", input), new SqlParameter("pid", pid) };
            return SqlHelper.ExecuteTable(CommandType.Text, sql, sp);
        }
        /// <summary>
        /// 查找互动Form表单
        /// </summary>
        /// <returns></returns>
        public DataTable SelALLForm()
        {
            return SelPage(1, int.MaxValue, new F_Pub()
            {
                pubType = "8"
            }).dt;
        }
        public DataTable SelByName(string pname)
        {
            return SelPage(1, int.MaxValue, new F_Pub() { pubName = pname }).dt;
        }
        public DataTable SelByType(int type = 1)
        {
            string sql = "SELECT * FROM " + strTableName + " WHERE ";
            switch (type)
            {
                case 1:
                    sql += " PubIsDel=0 And PubType!=8 And (PubSiteID is null or PubSiteID='')";
                    break;
                case 2:
                    sql += " PubIsDel=1";
                    break;
                case 3:
                    sql += "PubNodeID<>'0' and PubNodeID is not null and PubNodeID<>''";
                    break;
                case 4:
                    sql += " PubOpenComment=1";
                    break;
            }
            sql += " ORDER BY PubCreateTime DESC";
            return SqlHelper.ExecuteTable(CommandType.Text, sql);
        }
        //Info
        public DataTable SelByTbName(string tbname, int parentid = 0)
        {
            return B_Pub_Info.SelPage(1, int.MaxValue,
                new F_PubInfo() { tbname = tbname, pid = parentid }).dt;
        }
        private bool DelTableInfo(int id)
        {
            string tbname = DataConvert.CStr(DBCenter.ExecuteScala(strTableName, "PubTableName", "PubID=" + id));
            if (string.IsNullOrEmpty(tbname)) { return false; }
            DBHelper.Table_Del(tbname);
            return true;
        }
        #region Model
        public bool UpdateModel(SqlParameter[] sqlpara, string TableName, string where)
        {
            SafeSC.CheckDataEx(TableName);
            if (sqlpara.Length > 0 && TableName != "" && where != "")
            {
                string filename = "";
                string filevalue = "";
                string sqlstr = "";
                foreach (SqlParameter para in sqlpara)
                {
                    if (filename == "") { filename = para.ParameterName; }
                    else { filename = filename + "," + para.ParameterName; }

                    if (filevalue == "")
                    {
                        filevalue = para.ParameterName + "=@" + para.ParameterName;
                    }
                    else
                    {
                        filevalue = filevalue + "," + para.ParameterName + "=" + "@" + para.ParameterName;
                    }
                }
                sqlstr = "update " + TableName + " set " + filevalue + " where " + where;
                return SqlHelper.ExecuteSql(sqlstr, sqlpara);
            }
            else
            {
                return false;
            }
        }
        public bool InsertModel(SqlParameter[] sqlpara, string TableName)
        {
            SafeSC.CheckDataEx(TableName);
            if (sqlpara.Length > 0 && TableName != "")
            {
                string filename = "";
                string filevalue = "";
                string sqlstr = "";

                foreach (SqlParameter para in sqlpara)
                {
                    if (para.ParameterName != "PubID")
                    {
                        if (filename == "") { filename = para.ParameterName; }
                        else { filename = filename + "," + para.ParameterName; }

                        if (filevalue == "")
                        {
                            filevalue = "@" + para.ParameterName;
                        }
                        else
                        {
                            filevalue = filevalue + ",@" + para.ParameterName;
                        }
                    }
                }
                sqlstr = "Insert into " + TableName + " (" + filename + ") values (" + filevalue + ")";
                return SqlHelper.ExecuteSql(sqlstr, sqlpara);
            }
            else
            {
                return false;
            }
        }
        public bool DeleteModel(string TableName, string where)
        {
            SafeSC.CheckDataEx(TableName);
            string sqlstr = "";
            sqlstr = "delete from " + TableName + " where " + where;
            return SqlHelper.ExecuteSql(sqlstr, null);
        }
        /// <summary>
        /// 创建模型和数据表,并增加字段
        /// </summary>
        public int CreateModelInfo(M_Pub pub, M_ModelInfo modelinfo = null)
        {
            B_Model mll = new B_Model();
            if (modelinfo == null) { modelinfo = new M_ModelInfo(); }
            modelinfo.TableName = pub.PubTableName;
            modelinfo.ModelType = 7;
            modelinfo.MultiFlag = false;
            //---------------------------------
            string sqlStr = "";
            string propert1 = "";
            //bool isext = DBHelper.Table_IsExist(pub.PubTableName);
            switch (pub.PubType)
            {
                case 0://评论
                    modelinfo.ModelName = "评论" + pub.PubName + "的模型";
                    //modelinfo.Description = "";
                    //modelinfo.ItemName = "评论";
                    //modelinfo.ItemUnit = "条";
                    //modelinfo.ItemIcon = "GuestBook.gif";
                    modelinfo.ContentModule = "/互动模板/默认评论" + pub.PubName + "模板.html";
                    sqlStr = "CREATE TABLE [dbo].[" + pub.PubTableName + "] (  [ID] [int] IDENTITY (1, 1) NOT NULL,  [Pubupid] [int] NULL , [PubUserName] [nvarchar] (255) NULL , [PubUserID] [int] NULL , [PubContentid] [int] NULL ,[PubInputer] [nvarchar] (255) NULL ,[Parentid] [int] NULL DEFAULT (0), [PubIP] [nvarchar] (255) NULL , [Pubnum] [int] NULL DEFAULT (0) , [Pubstart] [int] NULL DEFAULT (0) ,  [PubTitle] [nvarchar] (255) NULL ,[PubContent] [ntext] NULL  , [PubAddTime] [datetime] NULL DEFAULT (getdate()),[Optimal] [int] NULL DEFAULT (0),[cookflag] [nvarchar] (500) NULL) ALTER TABLE [" + pub.PubTableName + "] WITH NOCHECK ADD CONSTRAINT [PK_" + pub.PubTableName + "] PRIMARY KEY  NONCLUSTERED ( [ID] )";
                    mll.AddModel(modelinfo, sqlStr);
                    propert1 = @"sp_addextendedproperty N'MS_Description', N'评论ID', N'user', N'dbo', N'table', N'" + pub.PubTableName + "', N'column', N'ID'";
                    Execute(propert1);
                    propert1 = @"sp_addextendedproperty N'MS_Description', N'互动ID', N'user', N'dbo', N'table', N'" + pub.PubTableName + "', N'column', N'Pubupid'";
                    Execute(propert1);
                    propert1 = @"sp_addextendedproperty N'MS_Description', N'回复用户名', N'user', N'dbo', N'table', N'" + pub.PubTableName + "', N'column', N'PubUserName'";
                    Execute(propert1);
                    propert1 = @"sp_addextendedproperty N'MS_Description', N'回复用户ID', N'user', N'dbo', N'table', N'" + pub.PubTableName + "', N'column', N'PubUserID'";
                    Execute(propert1);
                    propert1 = @"sp_addextendedproperty N'MS_Description', N'内容ID', N'user', N'dbo', N'table', N'" + pub.PubTableName + "', N'column', N'PubContentid'";
                    Execute(propert1);
                    propert1 = @"sp_addextendedproperty N'MS_Description', N'所属ID', N'user', N'dbo', N'table', N'" + pub.PubTableName + "', N'column', N'Parentid'";
                    Execute(propert1);
                    propert1 = @"sp_addextendedproperty N'MS_Description', N'IP地址', N'user', N'dbo', N'table', N'" + pub.PubTableName + "', N'column', N'PubIP'";
                    Execute(propert1);
                    propert1 = @"sp_addextendedproperty N'MS_Description', N'参与人数', N'user', N'dbo', N'table', N'" + pub.PubTableName + "', N'column', N'Pubnum'";
                    Execute(propert1);
                    propert1 = @"sp_addextendedproperty N'MS_Description', N'是否审核', N'user', N'dbo', N'table', N'" + pub.PubTableName + "', N'column', N'Pubstart'";
                    Execute(propert1);
                    propert1 = @"sp_addextendedproperty N'MS_Description', N'评论标题', N'user', N'dbo', N'table', N'" + pub.PubTableName + "', N'column', N'PubTitle'";
                    Execute(propert1);
                    propert1 = @"sp_addextendedproperty N'MS_Description', N'评论回复', N'user', N'dbo', N'table', N'" + pub.PubTableName + "', N'column', N'PubContent'";
                    Execute(propert1);
                    propert1 = @"sp_addextendedproperty N'MS_Description', N'评论时间', N'user', N'dbo', N'table', N'" + pub.PubTableName + "', N'column', N'PubAddTime'";
                    Execute(propert1);
                    propert1 = @"sp_addextendedproperty N'MS_Description', N'设置评论最佳回复', N'user', N'dbo', N'table', N'" + pub.PubTableName + "', N'column', N'Optimal'";
                    Execute(propert1);
                    break;
                case 1://投票
                    modelinfo.ModelName = "投票" + pub.PubName + "的模型";

                    modelinfo.ContentModule = "/互动模板/默认投票" + pub.PubName + "模板.html";
                    modelinfo.ModelType = 7;
                    modelinfo.MultiFlag = false;
                    sqlStr = "CREATE TABLE [dbo].[" + pub.PubTableName + "] (  [ID] [int] IDENTITY (1, 1) NOT NULL ,  [Pubupid] [int] NULL , [PubUserName] [nvarchar] (255) NULL , [PubUserID] [int] NULL ,  [PubContentid] [int] NULL ,[PubInputer] [nvarchar] (255) NULL ,[Parentid] [int] NULL DEFAULT (0), [PubIP] [nvarchar] (255) NULL , [Pubnum] [int] NULL DEFAULT (0) , [Pubstart] [int] NULL DEFAULT (0) , [PubTitle] [nvarchar] (255) NULL , [PubContent] [ntext] NULL  , [PubAddTime] [datetime] NULL DEFAULT (getdate()),[Optimal] [int] NULL DEFAULT (0),[cookflag] [nvarchar] (500) NULL ) ALTER TABLE [" + pub.PubTableName + "] WITH NOCHECK ADD CONSTRAINT [PK_" + pub.PubTableName + "] PRIMARY KEY  NONCLUSTERED ( [ID] )";
                    mll.AddModel(modelinfo, sqlStr);
                    propert1 = @"sp_addextendedproperty N'MS_Description', N'投票ID', N'user', N'dbo', N'table', N'" + pub.PubTableName + "', N'column', N'ID'";
                    Execute(propert1);
                    propert1 = @"sp_addextendedproperty N'MS_Description', N'互动ID', N'user', N'dbo', N'table', N'" + pub.PubTableName + "', N'column', N'Pubupid'";
                    Execute(propert1);
                    propert1 = @"sp_addextendedproperty N'MS_Description', N'回复用户名', N'user', N'dbo', N'table', N'" + pub.PubTableName + "', N'column', N'PubUserName'";
                    Execute(propert1);
                    propert1 = @"sp_addextendedproperty N'MS_Description', N'回复用户ID', N'user', N'dbo', N'table', N'" + pub.PubTableName + "', N'column', N'PubUserID'";
                    Execute(propert1);
                    propert1 = @"sp_addextendedproperty N'MS_Description', N'内容ID', N'user', N'dbo', N'table', N'" + pub.PubTableName + "', N'column', N'PubContentid'";
                    Execute(propert1);
                    propert1 = @"sp_addextendedproperty N'MS_Description', N'所属ID', N'user', N'dbo', N'table', N'" + pub.PubTableName + "', N'column', N'Parentid'";
                    Execute(propert1);
                    propert1 = @"sp_addextendedproperty N'MS_Description', N'IP地址', N'user', N'dbo', N'table', N'" + pub.PubTableName + "', N'column', N'PubIP'";
                    Execute(propert1);
                    propert1 = @"sp_addextendedproperty N'MS_Description', N'参与人数', N'user', N'dbo', N'table', N'" + pub.PubTableName + "', N'column', N'Pubnum'";
                    Execute(propert1);
                    propert1 = @"sp_addextendedproperty N'MS_Description', N'是否审核', N'user', N'dbo', N'table', N'" + pub.PubTableName + "', N'column', N'Pubstart'";
                    Execute(propert1);
                    propert1 = @"sp_addextendedproperty N'MS_Description', N'投票标题', N'user', N'dbo', N'table', N'" + pub.PubTableName + "', N'column', N'PubTitle'";
                    Execute(propert1);
                    propert1 = @"sp_addextendedproperty N'MS_Description', N'投票回复', N'user', N'dbo', N'table', N'" + pub.PubTableName + "', N'column', N'PubContent'";
                    Execute(propert1);
                    propert1 = @"sp_addextendedproperty N'MS_Description', N'投票时间', N'user', N'dbo', N'table', N'" + pub.PubTableName + "', N'column', N'PubAddTime'";
                    Execute(propert1);
                    propert1 = @"sp_addextendedproperty N'MS_Description', N'设置投票最佳回复', N'user', N'dbo', N'table', N'" + pub.PubTableName + "', N'column', N'Optimal'";
                    Execute(propert1);
                    break;
                case 2://活动
                    modelinfo.ModelName = "活动" + pub.PubName + "的模型";
                    modelinfo.ContentModule = "/互动模板/默认活动" + pub.PubName + "模板.html";

                    sqlStr = "CREATE TABLE [dbo].[" + pub.PubTableName + "] (  [ID] [int] IDENTITY (1, 1) NOT NULL ,  [Pubupid] [int] NULL , [PubUserName] [nvarchar] (255) NULL , [PubUserID] [int] NULL , [PubContentid] [int] NULL , [PubInputer] [nvarchar] (255) NULL ,[Parentid] [int] NULL DEFAULT (0),[PubIP] [nvarchar] (255) NULL , [Pubnum] [int] NULL DEFAULT (0) , [Pubstart] [int] NULL DEFAULT (0) , [PubTitle] [nvarchar] (255) NULL , [PubContent] [ntext] NULL  , [PubAddTime] [datetime] NULL DEFAULT (getdate()),[Optimal] [int] NULL DEFAULT (0),[cookflag] [nvarchar] (500) NULL) ALTER TABLE [" + pub.PubTableName + "] WITH NOCHECK ADD CONSTRAINT [PK_" + pub.PubTableName + "] PRIMARY KEY  NONCLUSTERED ( [ID] )";
                    mll.AddModel(modelinfo, sqlStr);
                    propert1 = @"sp_addextendedproperty N'MS_Description', N'活动ID', N'user', N'dbo', N'table', N'" + pub.PubTableName + "', N'column', N'ID'";
                    Execute(propert1);
                    propert1 = @"sp_addextendedproperty N'MS_Description', N'互动ID', N'user', N'dbo', N'table', N'" + pub.PubTableName + "', N'column', N'Pubupid'";
                    Execute(propert1);
                    propert1 = @"sp_addextendedproperty N'MS_Description', N'回复用户名', N'user', N'dbo', N'table', N'" + pub.PubTableName + "', N'column', N'PubUserName'";
                    Execute(propert1);
                    propert1 = @"sp_addextendedproperty N'MS_Description', N'回复用户ID', N'user', N'dbo', N'table', N'" + pub.PubTableName + "', N'column', N'PubUserID'";
                    Execute(propert1);
                    propert1 = @"sp_addextendedproperty N'MS_Description', N'内容ID', N'user', N'dbo', N'table', N'" + pub.PubTableName + "', N'column', N'PubContentid'";
                    Execute(propert1);
                    propert1 = @"sp_addextendedproperty N'MS_Description', N'所属ID', N'user', N'dbo', N'table', N'" + pub.PubTableName + "', N'column', N'Parentid'";
                    Execute(propert1);
                    propert1 = @"sp_addextendedproperty N'MS_Description', N'IP地址', N'user', N'dbo', N'table', N'" + pub.PubTableName + "', N'column', N'PubIP'";
                    Execute(propert1);
                    propert1 = @"sp_addextendedproperty N'MS_Description', N'参与人数', N'user', N'dbo', N'table', N'" + pub.PubTableName + "', N'column', N'Pubnum'";
                    Execute(propert1);
                    propert1 = @"sp_addextendedproperty N'MS_Description', N'是否审核', N'user', N'dbo', N'table', N'" + pub.PubTableName + "', N'column', N'Pubstart'";
                    Execute(propert1);
                    propert1 = @"sp_addextendedproperty N'MS_Description', N'活动标题', N'user', N'dbo', N'table', N'" + pub.PubTableName + "', N'column', N'PubTitle'";
                    Execute(propert1);
                    propert1 = @"sp_addextendedproperty N'MS_Description', N'活动回复', N'user', N'dbo', N'table', N'" + pub.PubTableName + "', N'column', N'PubContent'";
                    Execute(propert1);
                    propert1 = @"sp_addextendedproperty N'MS_Description', N'发动时间', N'user', N'dbo', N'table', N'" + pub.PubTableName + "', N'column', N'PubAddTime'";
                    Execute(propert1);
                    propert1 = @"sp_addextendedproperty N'MS_Description', N'设置活动最佳回复', N'user', N'dbo', N'table', N'" + pub.PubTableName + "', N'column', N'Optimal'";
                    Execute(propert1);
                    break;
                case 3://留言

                    modelinfo.ModelName = "留言" + pub.PubName + "的模型";
                    modelinfo.ContentModule = "/互动模板/默认留言" + pub.PubName + "模板.html";


                    sqlStr = "CREATE TABLE [dbo].[" + pub.PubTableName + "] (  [ID] [int] IDENTITY (1, 1) NOT NULL ,  [Pubupid] [int] NULL , [PubUserName] [nvarchar] (255) NULL , [PubUserID] [int] NULL , [PubContentid] [int] NULL , [PubInputer] [nvarchar] (255) NULL ,[Parentid] [int] NULL DEFAULT (0),[PubIP] [nvarchar] (255) NULL , [Pubnum] [int] NULL DEFAULT (0) , [Pubstart] [int] NULL DEFAULT (0) , [PubTitle] [nvarchar] (255) NULL ,[PubContent] [ntext] NULL  , [PubAddTime] [datetime] NULL DEFAULT (getdate()),[Optimal] [int] NULL DEFAULT (0),[cookflag] [nvarchar] (500) NULL) ALTER TABLE [" + pub.PubTableName + "] WITH NOCHECK ADD CONSTRAINT [PK_" + pub.PubTableName + "] PRIMARY KEY  NONCLUSTERED ( [ID] )";
                    mll.AddModel(modelinfo, sqlStr);
                    propert1 = @"sp_addextendedproperty N'MS_Description', N'留言ID', N'user', N'dbo', N'table', N'" + pub.PubTableName + "', N'column', N'ID'";
                    Execute(propert1);
                    propert1 = @"sp_addextendedproperty N'MS_Description', N'互动ID', N'user', N'dbo', N'table', N'" + pub.PubTableName + "', N'column', N'Pubupid'";
                    Execute(propert1);
                    propert1 = @"sp_addextendedproperty N'MS_Description', N'回复用户名', N'user', N'dbo', N'table', N'" + pub.PubTableName + "', N'column', N'PubUserName'";
                    Execute(propert1);
                    propert1 = @"sp_addextendedproperty N'MS_Description', N'回复用户ID', N'user', N'dbo', N'table', N'" + pub.PubTableName + "', N'column', N'PubUserID'";
                    Execute(propert1);
                    propert1 = @"sp_addextendedproperty N'MS_Description', N'内容ID', N'user', N'dbo', N'table', N'" + pub.PubTableName + "', N'column', N'PubContentid'";
                    Execute(propert1);
                    propert1 = @"sp_addextendedproperty N'MS_Description', N'所属ID', N'user', N'dbo', N'table', N'" + pub.PubTableName + "', N'column', N'Parentid'";
                    Execute(propert1);
                    propert1 = @"sp_addextendedproperty N'MS_Description', N'IP地址', N'user', N'dbo', N'table', N'" + pub.PubTableName + "', N'column', N'PubIP'";
                    Execute(propert1);
                    propert1 = @"sp_addextendedproperty N'MS_Description', N'参与人数', N'user', N'dbo', N'table', N'" + pub.PubTableName + "', N'column', N'Pubnum'";
                    Execute(propert1);
                    propert1 = @"sp_addextendedproperty N'MS_Description', N'是否审核', N'user', N'dbo', N'table', N'" + pub.PubTableName + "', N'column', N'Pubstart'";
                    Execute(propert1);
                    propert1 = @"sp_addextendedproperty N'MS_Description', N'留言标题', N'user', N'dbo', N'table', N'" + pub.PubTableName + "', N'column', N'PubTitle'";
                    Execute(propert1);
                    propert1 = @"sp_addextendedproperty N'MS_Description', N'留言回复', N'user', N'dbo', N'table', N'" + pub.PubTableName + "', N'column', N'PubContent'";
                    Execute(propert1);
                    propert1 = @"sp_addextendedproperty N'MS_Description', N'留言时间', N'user', N'dbo', N'table', N'" + pub.PubTableName + "', N'column', N'PubAddTime'";
                    Execute(propert1);
                    propert1 = @"sp_addextendedproperty N'MS_Description', N'设置留言最佳回复', N'user', N'dbo', N'table', N'" + pub.PubTableName + "', N'column', N'Optimal'";
                    Execute(propert1);
                    break;
                case 4://问券
                    modelinfo.ModelName = "问券" + pub.PubName + "的模型";
                    modelinfo.ContentModule = "/互动模板/默认问券" + pub.PubName + "模板.html";
                    sqlStr = "CREATE TABLE [dbo].[" + pub.PubTableName + "] (  [ID] [int] IDENTITY (1, 1) NOT NULL ,  [Pubupid] [int] NULL , [PubUserName] [nvarchar] (255) NULL , [PubUserID] [int] NULL , [PubContentid] [int] NULL ,[PubInputer] [nvarchar] (255) NULL ,[Parentid] [int] NULL DEFAULT (0), [PubIP] [nvarchar] (255) NULL , [Pubnum] [int] NULL DEFAULT (0) , [Pubstart] [int] NULL DEFAULT (0) , [PubTitle] [nvarchar] (255) NULL , [PubContent] [ntext] NULL , [PubAddTime] [datetime] NULL DEFAULT (getdate()),[Optimal] [int] NULL DEFAULT (0),[cookflag] [nvarchar] (500) NULL) ALTER TABLE [" + pub.PubTableName + "] WITH NOCHECK ADD CONSTRAINT [PK_" + pub.PubTableName + "] PRIMARY KEY  NONCLUSTERED ( [ID] )";
                    mll.AddModel(modelinfo, sqlStr);
                    propert1 = @"sp_addextendedproperty N'MS_Description', N'问券ID', N'user', N'dbo', N'table', N'" + pub.PubTableName + "', N'column', N'ID'";
                    Execute(propert1);
                    propert1 = @"sp_addextendedproperty N'MS_Description', N'互动ID', N'user', N'dbo', N'table', N'" + pub.PubTableName + "', N'column', N'Pubupid'";
                    Execute(propert1);
                    propert1 = @"sp_addextendedproperty N'MS_Description', N'回复用户名', N'user', N'dbo', N'table', N'" + pub.PubTableName + "', N'column', N'PubUserName'";
                    Execute(propert1);
                    propert1 = @"sp_addextendedproperty N'MS_Description', N'回复用户ID', N'user', N'dbo', N'table', N'" + pub.PubTableName + "', N'column', N'PubUserID'";
                    Execute(propert1);
                    propert1 = @"sp_addextendedproperty N'MS_Description', N'内容ID', N'user', N'dbo', N'table', N'" + pub.PubTableName + "', N'column', N'PubContentid'";
                    Execute(propert1);
                    propert1 = @"sp_addextendedproperty N'MS_Description', N'所属ID', N'user', N'dbo', N'table', N'" + pub.PubTableName + "', N'column', N'Parentid'";
                    Execute(propert1);
                    propert1 = @"sp_addextendedproperty N'MS_Description', N'IP地址', N'user', N'dbo', N'table', N'" + pub.PubTableName + "', N'column', N'PubIP'";
                    Execute(propert1);
                    propert1 = @"sp_addextendedproperty N'MS_Description', N'参与人数', N'user', N'dbo', N'table', N'" + pub.PubTableName + "', N'column', N'Pubnum'";
                    Execute(propert1);
                    propert1 = @"sp_addextendedproperty N'MS_Description', N'是否审核', N'user', N'dbo', N'table', N'" + pub.PubTableName + "', N'column', N'Pubstart'";
                    Execute(propert1);
                    propert1 = @"sp_addextendedproperty N'MS_Description', N'问券标题', N'user', N'dbo', N'table', N'" + pub.PubTableName + "', N'column', N'PubTitle'";
                    Execute(propert1);
                    propert1 = @"sp_addextendedproperty N'MS_Description', N'问券回复', N'user', N'dbo', N'table', N'" + pub.PubTableName + "', N'column', N'PubContent'";
                    Execute(propert1);
                    propert1 = @"sp_addextendedproperty N'MS_Description', N'问券时间', N'user', N'dbo', N'table', N'" + pub.PubTableName + "', N'column', N'PubAddTime'";
                    Execute(propert1);
                    propert1 = @"sp_addextendedproperty N'MS_Description', N'设置问券最佳回复', N'user', N'dbo', N'table', N'" + pub.PubTableName + "', N'column', N'Optimal'";
                    Execute(propert1);
                    break;
                case 5://统计
                    modelinfo.ModelName = "统计" + pub.PubName + "的模型";
                    modelinfo.ContentModule = "/互动模板/默认统计" + pub.PubName + "模板.html";
                    sqlStr = "CREATE TABLE [dbo].[" + pub.PubTableName + "] (  [ID] [int] IDENTITY (1, 1) NOT NULL ,  [Pubupid] [int] NULL , [PubUserName] [nvarchar] (255) NULL , [PubUserID] [int] NULL , [PubContentid] [int] NULL , [PubInputer] [nvarchar] (255) NULL ,[Parentid] [int] NULL DEFAULT (0),  [PubIP] [nvarchar] (255) NULL , [Pubnum] [int] NULL DEFAULT (0) , [Pubstart] [int] NULL DEFAULT (0) , [PubTitle] [nvarchar] (255) NULL , [PubContent] [ntext] NULL , [PubAddTime] [datetime] NULL DEFAULT (getdate()),[Optimal] [int] NULL DEFAULT (0),[cookflag] [nvarchar] (500) NULL) ALTER TABLE [" + pub.PubTableName + "] WITH NOCHECK ADD CONSTRAINT [PK_" + pub.PubTableName + "] PRIMARY KEY  NONCLUSTERED ( [ID] )";
                    mll.AddModel(modelinfo, sqlStr);
                    propert1 = @"sp_addextendedproperty N'MS_Description', N'统计ID', N'user', N'dbo', N'table', N'" + pub.PubTableName + "', N'column', N'ID'";
                    Execute(propert1);
                    propert1 = @"sp_addextendedproperty N'MS_Description', N'互动ID', N'user', N'dbo', N'table', N'" + pub.PubTableName + "', N'column', N'Pubupid'";
                    Execute(propert1);
                    propert1 = @"sp_addextendedproperty N'MS_Description', N'参与用户名', N'user', N'dbo', N'table', N'" + pub.PubTableName + "', N'column', N'PubUserName'";
                    Execute(propert1);
                    propert1 = @"sp_addextendedproperty N'MS_Description', N'参与用户ID', N'user', N'dbo', N'table', N'" + pub.PubTableName + "', N'column', N'PubUserID'";
                    Execute(propert1);
                    propert1 = @"sp_addextendedproperty N'MS_Description', N'内容ID', N'user', N'dbo', N'table', N'" + pub.PubTableName + "', N'column', N'PubContentid'";
                    Execute(propert1);
                    propert1 = @"sp_addextendedproperty N'MS_Description', N'所属ID', N'user', N'dbo', N'table', N'" + pub.PubTableName + "', N'column', N'Parentid'";
                    Execute(propert1);
                    propert1 = @"sp_addextendedproperty N'MS_Description', N'IP地址', N'user', N'dbo', N'table', N'" + pub.PubTableName + "', N'column', N'PubIP'";
                    Execute(propert1);
                    propert1 = @"sp_addextendedproperty N'MS_Description', N'参与人数', N'user', N'dbo', N'table', N'" + pub.PubTableName + "', N'column', N'Pubnum'";
                    Execute(propert1);
                    propert1 = @"sp_addextendedproperty N'MS_Description', N'是否审核', N'user', N'dbo', N'table', N'" + pub.PubTableName + "', N'column', N'Pubstart'";
                    Execute(propert1);
                    propert1 = @"sp_addextendedproperty N'MS_Description', N'统计标题', N'user', N'dbo', N'table', N'" + pub.PubTableName + "', N'column', N'PubTitle'";
                    Execute(propert1);
                    propert1 = @"sp_addextendedproperty N'MS_Description', N'统计备注', N'user', N'dbo', N'table', N'" + pub.PubTableName + "', N'column', N'PubContent'";
                    Execute(propert1);
                    propert1 = @"sp_addextendedproperty N'MS_Description', N'添加时间', N'user', N'dbo', N'table', N'" + pub.PubTableName + "', N'column', N'PubAddTime'";
                    Execute(propert1);
                    propert1 = @"sp_addextendedproperty N'MS_Description', N'设置统计最佳回复', N'user', N'dbo', N'table', N'" + pub.PubTableName + "', N'column', N'Optimal'";
                    Execute(propert1);
                    break;
                case 6://竞标
                    modelinfo.ModelName = "竞标" + pub.PubName + "的模型";
                    modelinfo.ContentModule = "/互动模板/默认竞标" + pub.PubName + "模板.html";
                    sqlStr = "CREATE TABLE [dbo].[" + pub.PubTableName + "] (  [ID] [int] IDENTITY (1, 1) NOT NULL ,  [Pubupid] [int] NULL , [PubUserName] [nvarchar] (255) NULL , [PubUserID] [int] NULL ,  [PubContentid] [int] NULL ,[PubInputer] [nvarchar] (255) NULL ,[Parentid] [int] NULL DEFAULT (0), [PubIP] [nvarchar] (255) NULL , [Pubnum] [int] NULL DEFAULT (0) , [Pubstart] [int] NULL DEFAULT (0) , [PubTitle] [nvarchar] (255) NULL , [PubContent] [ntext] NULL  , [PubAddTime] [datetime] NULL DEFAULT (getdate()),[Optimal] [int] NULL DEFAULT (0),[cookflag] [nvarchar] (500) NULL ) ALTER TABLE [" + pub.PubTableName + "] WITH NOCHECK ADD CONSTRAINT [PK_" + pub.PubTableName + "] PRIMARY KEY  NONCLUSTERED ( [ID] )";
                    mll.AddModel(modelinfo, sqlStr);
                    propert1 = @"sp_addextendedproperty N'MS_Description', N'竞标ID', N'user', N'dbo', N'table', N'" + pub.PubTableName + "', N'column', N'ID'";
                    Execute(propert1);
                    propert1 = @"sp_addextendedproperty N'MS_Description', N'互动ID', N'user', N'dbo', N'table', N'" + pub.PubTableName + "', N'column', N'Pubupid'";
                    Execute(propert1);
                    propert1 = @"sp_addextendedproperty N'MS_Description', N'回复用户名', N'user', N'dbo', N'table', N'" + pub.PubTableName + "', N'column', N'PubUserName'";
                    Execute(propert1);
                    propert1 = @"sp_addextendedproperty N'MS_Description', N'回复用户ID', N'user', N'dbo', N'table', N'" + pub.PubTableName + "', N'column', N'PubUserID'";
                    Execute(propert1);
                    propert1 = @"sp_addextendedproperty N'MS_Description', N'内容ID', N'user', N'dbo', N'table', N'" + pub.PubTableName + "', N'column', N'PubContentid'";
                    Execute(propert1);
                    propert1 = @"sp_addextendedproperty N'MS_Description', N'所属ID', N'user', N'dbo', N'table', N'" + pub.PubTableName + "', N'column', N'Parentid'";
                    Execute(propert1);
                    propert1 = @"sp_addextendedproperty N'MS_Description', N'IP地址', N'user', N'dbo', N'table', N'" + pub.PubTableName + "', N'column', N'PubIP'";
                    Execute(propert1);
                    propert1 = @"sp_addextendedproperty N'MS_Description', N'参与人数', N'user', N'dbo', N'table', N'" + pub.PubTableName + "', N'column', N'Pubnum'";
                    Execute(propert1);
                    propert1 = @"sp_addextendedproperty N'MS_Description', N'是否审核', N'user', N'dbo', N'table', N'" + pub.PubTableName + "', N'column', N'Pubstart'";
                    Execute(propert1);
                    propert1 = @"sp_addextendedproperty N'MS_Description', N'竞标标题', N'user', N'dbo', N'table', N'" + pub.PubTableName + "', N'column', N'PubTitle'";
                    Execute(propert1);
                    propert1 = @"sp_addextendedproperty N'MS_Description', N'竞标回复', N'user', N'dbo', N'table', N'" + pub.PubTableName + "', N'column', N'PubContent'";
                    Execute(propert1);
                    propert1 = @"sp_addextendedproperty N'MS_Description', N'竞标时间', N'user', N'dbo', N'table', N'" + pub.PubTableName + "', N'column', N'PubAddTime'";
                    Execute(propert1);
                    propert1 = @"sp_addextendedproperty N'MS_Description', N'设置竞标最佳回复', N'user', N'dbo', N'table', N'" + pub.PubTableName + "', N'column', N'Optimal'";
                    Execute(propert1);
                    break;
                case 7://评星
                    modelinfo.ModelName = "评星" + pub.PubName + "的模型";
                    modelinfo.ContentModule = "/互动模板/评星" + pub.PubName + "模板.html";
                    sqlStr = "CREATE TABLE [dbo].[" + pub.PubTableName + "] (  [ID] [int] IDENTITY (1, 1) NOT NULL ,  [Pubupid] [int] NULL , [PubUserName] [nvarchar] (255) NULL , [PubUserID] [int] NULL ,  [PubContentid] [int] NULL ,[PubInputer] [nvarchar] (255) NULL ,[Parentid] [int] NULL DEFAULT (0), [PubIP] [nvarchar] (255) NULL , [Pubnum] [int] NULL DEFAULT (0) , [Pubstart] [int] NULL DEFAULT (0) , [PubTitle] [nvarchar] (255) NULL , [PubContent] [ntext] NULL  , [PubAddTime] [datetime] NULL DEFAULT (getdate()),[Optimal] [int] NULL DEFAULT (0),[Type] [int] NULL DEFAULT (1),[cookflag] [nvarchar] (500) NULL ) ALTER TABLE [" + pub.PubTableName + "] WITH NOCHECK ADD CONSTRAINT [PK_" + pub.PubTableName + "] PRIMARY KEY  NONCLUSTERED ( [ID] )";
                    mll.AddModel(modelinfo, sqlStr);
                    break;
                case 8://互动表单
                    sqlStr = "CREATE TABLE [dbo].[" + pub.PubTableName + "] ([ID] [int] IDENTITY (1, 1) NOT NULL ,  [Pubupid] [int] NULL , [PubUserName] [nvarchar] (255) NULL , [PubUserID] [int] NULL ,  [PubContentid] [int] NULL ,[PubInputer] [nvarchar] (255) NULL ,[Parentid] [int] NULL DEFAULT (0), [PubIP] [nvarchar] (255) NULL , [Pubnum] [int] NULL DEFAULT (0) , [Pubstart] [int] NULL DEFAULT (0) , [PubTitle] [nvarchar] (255) NULL , [PubContent] [ntext] NULL  , [PubAddTime] [datetime] NULL DEFAULT (getdate()),[Optimal] [int] NULL DEFAULT (0),[cookflag] [nvarchar] (500) NULL ) ALTER TABLE [" + pub.PubTableName + "] WITH NOCHECK ADD CONSTRAINT [PK_" + pub.PubTableName + "] PRIMARY KEY  NONCLUSTERED ( [ID] )";
                    Execute(sqlStr);
                    break;
                default:
                    mll.AddModel(modelinfo, "");
                    break;
            }
            return modelinfo.ModelID;
        }
        #endregion
        #region Tools
        //兼容,后期移除[181207]
        [Obsolete]
        public DataTable SelByPubTbName(string tbname)
        {
            string sql = "Select * From " + strTableName + " Where PubTableName=@pname";
            SqlParameter[] sp = new SqlParameter[] { new SqlParameter("pname", tbname) };
            return SqlHelper.ExecuteTable(CommandType.Text, sql, sp);
        }
        private static void Execute(string sqlStr)
        {
            if (sqlStr != "")
            {
                SqlHelper.ExecuteNonQuery(CommandType.Text, sqlStr, null);
            }
        }
        /// <summary>
        /// 通过存储模型表字段值的table创建插入语句和更新语句的参数数组
        /// </summary>
        /// <param name="DTContent">存储模型表字段值的table</param>
        /// <returns>Sql参数数组</returns>
        private void ContentPara(SqlParameter[] sp, DataTable DTContent)
        {
            int i = 13;
            foreach (DataRow dr in DTContent.Rows)
            {
                sp[i] = GetPara(dr);
                i++;
            }
        }
        /// <summary>
        /// 从存储模型表字段值的table的一行数据创建一个参数元素
        /// </summary>
        /// <param name="dr">存储模型表字段值的table的一行数据</param>
        /// <returns>Sql参数</returns>
        private SqlParameter GetPara(DataRow dr)
        {
            return new SqlParameter("@" + dr["FieldName"], DataConvert.CStr(dr["FieldValue"]));
        }
        #endregion
        //---------------------------------------------New
        public PageSetting SelPage(int cpage, int psize,F_Pub filter)
        {
            string where = "1=1 ";
            List<SqlParameter> sp = new List<SqlParameter>();
            if (!string.IsNullOrEmpty(filter.pubType))
            {
                SafeSC.CheckIDSEx(filter.pubType);
                where += " AND PubType IN (" + filter.pubType + ")";
            }
            if (!string.IsNullOrEmpty(filter.pubName))
            {
                sp.Add(new SqlParameter("pname", filter.pubName));
                where += " AND PubName=@pname";
            }
            if (filter.hasDel == false)
            {
                where += " AND PubIsDel=0";
            }
            if (!string.IsNullOrEmpty(filter.ids))
            {
                SafeSC.CheckIDSEx(filter.ids);
                where += " AND PubID IN (" + filter.ids + ")";
            }
            PageSetting setting = PageSetting.Single(cpage, psize, strTableName, PK, where, "", sp);
            DBCenter.SelPage(setting);
            return setting;
        }
    }
    //互动下的信息
    public class B_Pub_Info
    {
        public DataTable Sel(string tbname)
        {
            return SelPage(1, int.MaxValue, new F_PubInfo() { tbname = tbname }).dt;
        }
        public static PageSetting SelPage(int cpage, int psize, F_PubInfo filter)
        {
            string where = "1=1 ";
            List<SqlParameter> sp = new List<SqlParameter>();
            if (string.IsNullOrEmpty(filter.tbname)) { throw new Exception("未指定信息表名"); }
            filter.tbname = filter.tbname.Replace(" ","");
            SafeSC.CheckDataEx(filter.tbname);
            if (!string.IsNullOrEmpty(filter.uids))
            {
                SafeSC.CheckIDSEx(filter.uids);
                where += " AND PubUserID IN (" + filter.uids + ")";
            }
            if (filter.pid != -100) { where += " AND ParentID=" + filter.pid; }
            if (!string.IsNullOrEmpty(filter.uname))
            {
                sp.Add(new SqlParameter("uname","%"+filter.uname+"%"));
                where += " AND PubUserName LIKE @uname";
            }
            if (!string.IsNullOrEmpty(filter.skey))
            {
                sp.Add(new SqlParameter("skey", "%" + filter.skey + "%"));
                if (!string.IsNullOrEmpty(filter.skey_field))
                {
                    DataTable dt = DBCenter.Sel(filter.tbname, "1=2");
                    if (!dt.Columns.Contains(filter.skey_field)) { throw new Exception("字段[" + filter.skey_field + "]不存在"); }
                    where += " AND " + filter.skey_field + " LIKE @skey";
                }
                else
                {
                    where += " AND PubTitle LIKE @skey";
                }
            }
            if (filter.status != -100)
            {
                where += " AND PubStart="+filter.status;
            }
            PageSetting setting = PageSetting.Single(cpage, psize, filter.tbname, "ID", where, filter.order,sp);
            DBCenter.SelPage(setting);
            return setting;
        }
    }
    public class F_Pub
    {
        public string pubType = "";
        public string pubName = "";
        public string ids = "";
        public string skey = "";
        //是否包含已删除
        public bool hasDel = false;
    }
    public class F_PubInfo
    {      
        //需要检索的表(必填)
        public string tbname = "";
        // 某贴下的留言内容
        public int pid = -100;
        // 发贴人
        public string uids = "";
        //用户名检索
        public string uname = "";
        //标题搜索
        public string skey = "";
        public int status = -100;
        public string skey_field = "";//指定搜索字段(否则为PubTitle)
        //不开放给前端,只可后端判断
        public string order = "ID DESC";
    }
}