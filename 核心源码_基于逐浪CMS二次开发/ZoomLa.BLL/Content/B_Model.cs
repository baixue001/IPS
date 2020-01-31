using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using ZoomLa.BLL.Helper;
using ZoomLa.Model;
using ZoomLa.Model.Content;
using ZoomLa.Safe;
using ZoomLa.SQLDAL;
using ZoomLa.SQLDAL.SQL;

namespace ZoomLa.BLL.Content
{
    public class B_Model
    {
        private string TbName, PK;
        private M_ModelInfo initMod = new M_ModelInfo();
        public B_Model()
        {
            TbName = initMod.TbName;
            PK = initMod.PK;
        }
        public M_ModelInfo GetModelById(int id) { return SelReturnModel(id); }
        public M_ModelInfo SelReturnModel(int id)
        {
            if (id < 1) { return new M_ModelInfo(true); }
            using (DbDataReader reader = DBCenter.SelReturnReader(TbName, PK, id))
            {
                if (reader.Read())
                {
                    return initMod.GetModelFromReader(reader);
                }
                else
                {
                    return new M_ModelInfo(true);
                }
            }
        }
        public M_ModelInfo SelModelByTbName(string tbname)
        {
            if (string.IsNullOrEmpty(tbname)) { return null; }
            string where = "TableName=@tbname";
            SqlParameter[] sp = new SqlParameter[] { new SqlParameter("tbname", tbname) };
            using (DbDataReader reader = DBCenter.SelReturnReader(TbName, where, sp))
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
        public PageSetting SelPage(int cpage, int psize, Com_Filter filter)
        {
            List<SqlParameter> sp = new List<SqlParameter>();
            string where = "1=1";
            if (!string.IsNullOrEmpty(filter.type))
            {
                SafeSC.CheckIDSEx(filter.type);
                where += " AND ModelType IN (" + filter.type + ")";
            }
            if (!string.IsNullOrEmpty(filter.skey))
            {
                where += " AND ModelName LIKE @skey";
                sp.Add(new SqlParameter("skey", "%" + filter.skey + "%"));
            }
            string orderBy = StrHelper.SQL_OrderBy(filter.orderBy, "modelid");
            PageSetting setting = PageSetting.Single(cpage, psize, TbName, PK, where, orderBy);
            DBCenter.SelPage(setting);
            return setting;
        }
        DataTable ListModel()
        {
            string strSql = "Select * from ZL_Model Where ModelType=1 or ModelType=5 or ModelType=2 order by ModelID ";
            DataTable dt = DBCenter.ExecuteTable(strSql, null);
            return dt;
        }
        public DataTable GetListSQL()
        {
            return ListModel();
        }
        /// <summary>
        /// 内容模型,用于批量设置处与节点处
        /// </summary>
        /// <returns></returns>
        public DataTable GetList()
        {
            return SelByType("1,2,5,9");
        }
        public DataTable GetListPage()
        {
            string mtype = "'黄页申请'";
            DataTable modeltable = GetModel(mtype, "and TableName like 'ZL_Reg_%'");
            return modeltable;
        }
        public DataTable GetModel(string mtype, string andwhere = "")
        {
            int modelType = GetModelInt(mtype);
            return SqlHelper.ExecuteTable("SELECT * FROM " + TbName + " WHERE ModelType IN(" + modelType + ") " + andwhere);
        }
        /// <summary>
        /// 功能模型
        /// </summary>
        /// <returns></returns>
        public DataTable GetListFuc()
        {
            return SelByType("8");
        }
        /// <summary>
        /// 会员模型
        /// </summary>
        /// <returns></returns>
        public DataTable GetListUser()
        {
            return SelByType("3");
        }
        /// <summary>
        /// 店铺商品模型
        /// </summary>
        /// <returns></returns>
        public DataTable GetListStore()
        {
            return SelByType("6");
        }
        /// <summary>
        /// 读取商城模型列表
        /// </summary>
        /// <returns></returns>
        public DataTable GetListShop()
        {
            return SelByType("2");
        }
        /// <summary>
        /// 互动模型
        /// </summary>
        /// <returns></returns>
        public DataTable GetListPub()
        {
            return SelByType("7");
        }
        /// <summary>
        /// 读取用户商品模型列表
        /// </summary>
        /// <returns></returns>
        public DataTable GetListUserShop()
        {
            return SelByType("5");
        }
        public DataTable SelByType(string ids)
        {
            SafeSC.CheckIDSEx(ids);
            return DBCenter.ExecuteTable("SELECT * FROM " + TbName + " WHERE ModelType IN(" + ids + ") ");
        }

        /// <summary>
        /// 获取项目模型列表
        /// </summary>
        /// <returns></returns>
        public DataTable ListProjectModel()
        {
            return SelByType("8");
        }
        /// <summary>
        /// 商城模型
        /// </summary>
        /// <returns></returns>
        public DataTable GetShopMode()
        {
            return SelByType("2");
        }

        /// <summary>
        /// 为批量导入数据新建模板时动态添内容和创建表
        /// 新建模板添加内容到zl_model/字段加入zl_modelField/根据字段创建内容表
        /// </summary>
        /// <param name="modelName"></param>
        /// <param name="tableName"></param>
        /// <param name="fileNames"></param>
        /// <param name="fieldAliass"></param>
        /// <returns>返回ModelID</returns>
        public object SetModelFieldSingleNewTable(int NodeID, string modelName, string tableName, string fileNames, string fieldAliass)
        {
            return SetModelFieldSingleNewTable(NodeID, modelName, tableName, fileNames, fieldAliass);
        }

        public object SelModelByTbNames()
        {
            throw new NotImplementedException();
        }

        public DataTable ListModel(string p)
        {
            throw new NotImplementedException();
        }
        /****************************************************标记结束**********************************************************/
        public bool UpdateTemplate(string Template, int ModelID)
        {
            Template = Template.Replace(" ", "").Trim('/');
            SqlParameter[] sp = new SqlParameter[] { new SqlParameter("Template", Template) };
            string sql = "UPDATE " + TbName + " SET ContentTemplate=@Template WHERE ModelID=" + ModelID;
            SqlHelper.ExecuteSql(sql, sp);
            return true;
        }

        public DataSet ModelSet(M_ModelInfo modeinfo)
        {
            DataSet newdataset = new DataSet("NewDataSet");
            DataTable tables = new DataTable("Table");
            tables.Columns.Add("ModelID", typeof(int));
            tables.Columns.Add("ModelName", typeof(string));
            tables.Columns.Add("Description", typeof(string));
            tables.Columns.Add("TableName", typeof(string));
            tables.Columns.Add("ItemName", typeof(string));
            tables.Columns.Add("ItemUnit", typeof(string));
            tables.Columns.Add("ItemIcon", typeof(string));
            tables.Columns.Add("ModelType", typeof(string));
            tables.Columns.Add("ContentModule", typeof(string));
            tables.Columns.Add("MultiFlag", typeof(bool));
            tables.Columns.Add("IsNull", typeof(bool));
            tables.Columns.Add("NodeID", typeof(int));
            tables.Columns.Add("SysModel", typeof(int));
            tables.Columns.Add("FromModel", typeof(int));
            tables.Columns.Add("Thumbnail", typeof(string));
            tables.Columns.Add("Islotsize", typeof(bool));
            DataRow modelrow = tables.NewRow();
            modelrow["ModelID"] = modeinfo.ModelID;//模型ID
            modelrow["ModelName"] = modeinfo.ModelName;//模型名称
            modelrow["Description"] = modeinfo.Description;//模型描述
            modelrow["TableName"] = modeinfo.TableName;//模型内容存储表名
            modelrow["ItemName"] = modeinfo.ItemName;//项目名称：如文章、新闻
            modelrow["ItemUnit"] = modeinfo.ItemUnit;//项目单位：如篇、条
            modelrow["ItemIcon"] = modeinfo.ItemIcon;//项目图标
            modelrow["ModelType"] = modeinfo.ModelType;/// 模型类别
            modelrow["ContentModule"] = modeinfo.ContentModule;/// 内容模板
            modelrow["MultiFlag"] = modeinfo.MultiFlag;/// 是否多条记录，只对用户模型有效true时允许一个用户输入多条此模型信息
            modelrow["IsNull"] = modeinfo.IsNull;
            modelrow["NodeID"] = modeinfo.NodeID;
            modelrow["SysModel"] = modeinfo.SysModel;/// 识别系统模型字段，1：系统生成模型，2：用户自定义模型
            modelrow["FromModel"] = modeinfo.FromModel;/// 复制来源模型ID
            modelrow["Thumbnail"] = modeinfo.Thumbnail;
            modelrow["Islotsize"] = modeinfo.Islotsize;
            tables.Rows.Add(modelrow);
            newdataset.Tables.Add(tables);

            DataTable ModelFieldTable = new DataTable("ModelFieldTable");
            ModelFieldTable.Columns.Add("FieldID", typeof(int));
            ModelFieldTable.Columns.Add("ModelId", typeof(int));
            ModelFieldTable.Columns.Add("FieldName", typeof(string));
            ModelFieldTable.Columns.Add("FieldAlias", typeof(string));
            ModelFieldTable.Columns.Add("FieldTips", typeof(string));
            ModelFieldTable.Columns.Add("Description", typeof(string));
            ModelFieldTable.Columns.Add("IsNotNull", typeof(bool));
            ModelFieldTable.Columns.Add("IsSearchForm", typeof(bool));
            ModelFieldTable.Columns.Add("FieldType", typeof(string));
            ModelFieldTable.Columns.Add("OrderId", typeof(int));
            ModelFieldTable.Columns.Add("ShowList", typeof(bool));
            ModelFieldTable.Columns.Add("ShowWidth", typeof(int));
            ModelFieldTable.Columns.Add("IsNull", typeof(bool));
            ModelFieldTable.Columns.Add("IsShow", typeof(bool));
            ModelFieldTable.Columns.Add("IsView", typeof(bool));
            ModelFieldTable.Columns.Add("IsDownField", typeof(int));
            ModelFieldTable.Columns.Add("DownServerID", typeof(int));
            ModelFieldTable.Columns.Add("RestoreField", typeof(int));
            ModelFieldTable.Columns.Add("IsCopy", typeof(int));
            ModelFieldTable.Columns.Add("Sys_type", typeof(bool));
            ModelFieldTable.Columns.Add("Unfurl", typeof(int));
            ModelFieldTable.Columns.Add("Islotsize", typeof(bool));
            ModelFieldTable.Columns.Add("RegShow", typeof(int));

            List<M_ModelField> modelist = modeinfo.ModelField;
            for (int i = 0; i < modelist.Count; i++)
            {
                DataRow rows = ModelFieldTable.NewRow();
                rows["FieldID"] = modelist[i].FieldID;
                rows["ModelId"] = modelist[i].ModelID;
                rows["FieldName"] = modelist[i].FieldName;
                rows["FieldAlias"] = modelist[i].FieldAlias;
                rows["FieldTips"] = modelist[i].FieldTips;
                rows["Description"] = modelist[i].Description;
                rows["IsNotNull"] = modelist[i].IsNotNull;
                rows["IsSearchForm"] = modelist[i].IsSearchForm;
                rows["FieldType"] = modelist[i].FieldType;
                rows["Content"] = modelist[i].Content;
                rows["OrderId"] = modelist[i].OrderID;
                rows["ShowList"] = modelist[i].ShowList;
                rows["ShowWidth"] = modelist[i].ShowWidth;
                rows["IsNull"] = modelist[i].IsNull;
                rows["IsShow"] = modelist[i].IsShow;
                rows["IsView"] = modelist[i].IsView;
                rows["IsDownField"] = modelist[i].IsDownField;
                rows["DownServerID"] = modelist[i].DownServerID;
                rows["RestoreField"] = modelist[i].RestoreField;
                rows["IsCopy"] = modelist[i].IsCopy;
                rows["Sys_type"] = modelist[i].Sys_type;
                //rows["Unfurl"] = modelist[i].Unfurl;
                rows["Islotsize"] = modelist[i].Islotsize;
                //rows["RegShow"] = modelist[i].RegShow;
                ModelFieldTable.Rows.Add(rows);
            }
            newdataset.Tables.Add(ModelFieldTable);
            return newdataset;
        }
        public DataTable ModelFieldSet(int ModelID)
        {
            return new B_ModelField().GetModelFieldListall(ModelID);
        }
        /// <summary>
        /// 读取内容模型列表
        /// </summary>
        /// <returns></returns>
        public DataTable GetListXML()
        {
            return ListModel();
        }
        /// <summary>
        /// 读取会员模型列表
        /// </summary>
        /// <returns></returns>
        /// 
        DataTable ListUserModel()
        {
            string strSql = "Select * from ZL_Model Where TableName like 'ZL_U_%' and ModelType=3";
            DataTable dt = DBCenter.ExecuteTable(strSql, null);
            return dt;
        }
        /// <summary>
        /// 读取用户商城模型列表
        /// </summary>
        /// <returns></returns>
        /// 
        DataTable ListStoreModel()
        {
            string strSql = "Select * from ZL_Model Where ModelType=6";
            DataTable dt = DBCenter.ExecuteTable(strSql, null);
            return dt;
        }
        public DataTable GetListStoreXML()
        {
            //string strSql = "Select * from ZL_Model Where ModelType=6";
            return ListStoreModel();
        }
        /// <summary>
        /// 读取商城模型列表
        /// </summary>
        /// <returns></returns>
        /// 
        DataTable ListShopModel()
        {
            string strSql = "Select * from ZL_Model Where ModelType=2 ";
            DataTable dt = DBCenter.ExecuteTable(strSql, null);
            return dt;
        }
        public DataTable GetListShopXML()
        {
            //string strSql = "Select * from ZL_Model Where ModelType=2 ";
            return ListShopModel();
        }
        /// <summary>
        /// 获取互动模型
        /// </summary>
        /// <returns></returns>
        /// 
        DataTable ListPubModel()
        {
            string strSql = "Select * from ZL_Model Where ModelType=7";
            DataTable dt = DBCenter.ExecuteTable(strSql, null);
            return dt;
        }
        public DataTable GetListPubXML()
        {
            //string strSql = "Select * from ZL_Model Where ModelType=7";
            return ListPubModel();
        }
        /// <summary>
        /// 读取用户商城模型列表
        /// </summary>
        /// <returns></returns>
        /// 
        DataTable ListUserShopModel()
        {
            string strSql = "Select * from ZL_Model Where ModelType=5";
            DataTable dt = DBCenter.ExecuteTable(strSql);
            return dt;
        }
        public DataTable GetListUserShopXML()
        {
            //string strSql = "Select * from ZL_Model Where ModelType=5";
            return ListUserShopModel();
        }
        /// <summary>
        /// 获取黄页模型
        /// </summary>
        /// <returns></returns>
        /// 
        DataTable ListPageModel()
        {
            string strSql = "Select * from ZL_Model Where TableName like 'ZL_Reg_%' and ModelType=3";
            DataTable dt = DBCenter.ExecuteTable(strSql, null);
            return dt;
        }
        public DataTable GetListPageXML()
        {
            //string strSql = "Select * from ZL_Model Where TableName like 'ZL_Reg_%' and ModelType=3";
            return ListPageModel();
        }
        /// <summary>
        /// 获取项目模型列表
        /// </summary>
        /// <returns></returns>
        public DataTable ListProjectModelXML()
        {
            //string strSql = "Select * from ZL_Model Where ModelType=8";
            return ListProjectModel();
        }
        public DataTable Sel() { return DBCenter.Sel(TbName); }
        /// <summary>
        /// 删除所有模型,附加表,模型字段
        /// </summary>
        public void DeleteAll()
        {
            DataTable dt = Sel();
            foreach (DataRow dr in dt.Rows)
            {
                string tbname = dr["TableName"].ToString().Replace(" ", "");
                if (DBHelper.Table_IsExist(tbname))
                {
                    DBCenter.DB.ExecuteNonQuery(new SqlModel("DROP TABLE " + tbname,null));
                }
            }
            DBCenter.DB.ExecuteNonQuery(new SqlModel("TRUNCATE TABLE ZL_ModelField",null));
            DBCenter.DB.ExecuteNonQuery(new SqlModel("TRUNCATE TABLE " + TbName, null));
        }

        
        #region 新方法区
        public bool isExistTableName(string TableName)
        {
            return DBCenter.DB.Table_Exist(TableName);
            //SqlParameter[] sp = new SqlParameter[] { new SqlParameter("TableName", TableName) };
            //return DBCenter.ExecuteTable("SELECT Top 1 ModelID FROM " + TbName + " WHERE TableName=@TableName", new List<SqlParameter>(sp)).Rows.Count > 0;
        }
        /// <summary>
        /// 内容商城模型不允许重名
        /// </summary>
        public bool IsExistModelName(string modelName)
        {
            if (string.IsNullOrEmpty(modelName)) { return false; }
            List<SqlParameter> sp = new List<SqlParameter>() { new SqlParameter("name", modelName) };
            return DBCenter.IsExist(TbName, "ModelName=@name AND ModelType NOT IN (3,8,7,6,12)", sp);
        }
        /// <summary>
        /// 删除模型|对应的数据表|模型字段
        /// </summary>
        public bool DelModel(int id)
        {
            M_ModelInfo model = SelReturnModel(id);
            try { if (DBCenter.DB.Table_Exist(model.TableName)) { DBCenter.DB.Table_Remove(model.TableName); } }
            catch (Exception ex) { ZLLog.L(ZLEnum.Log.exception, "模型表[" + model.TableName + "]删除失败,原因:" + ex.Message); }
            new B_ModelField().DelByModel(id);
            DBCenter.Del(TbName, PK,id);
            return true;
        }
        #endregion
        #region Tools
        public string GetTablePrefix(int ModelType)
        {
            string prefix = "";
            switch (ModelType)
            {
                //内容模型
                case 1:
                    prefix = "ZL_C_";
                    break;
                //商城模型
                case 2:
                    prefix = "ZL_P_";
                    break;
                //用户模型
                case 3:
                    prefix = "ZL_U_";
                    break;
                //黄页内容模型管理
                case 4:
                    prefix = "ZL_Page_";
                    break;
                //店铺模型
                case 5:
                    prefix = "ZL_S_";
                    break;
                //店铺申请设置
                case 6:
                    prefix = "ZL_Store_";
                    break;
                case 7:
                    prefix = "ZL_Pub_";
                    break;
                case 8:
                    prefix = "ZL_C_";
                    break;
                case 10:
                    prefix = "ZL_Reg_";
                    break;
                case 11://CRM模型
                    prefix = "ZL_CRM_";
                    break;
                case 12:
                    prefix = "ZL_OAC_";
                    break;
            }
            return prefix;
        }
        private static void Execute(string sqlStr)
        {
            if (!string.IsNullOrEmpty(sqlStr))
            {
               ExecuteSql(sqlStr);
            }
        }
        /// <summary>
        /// 系统模型分类
        /// </summary>
        public string GetModelType(int type)
        {
            string typename = "";
            switch (type)
            {
                case 1:
                    typename = "内容模型";
                    break;
                case 2:
                    typename = "商品模型";
                    break;
                case 3:///黄页申请模型
                    typename = "用户模型";
                    break;
                case 4:
                    typename = "黄页内容模型";
                    break;
                case 5:
                    typename = "店铺商品模型";
                    break;
                case 6:
                    typename = "店铺申请模型";
                    break;
                case 7:
                    typename = "互动模型";
                    break;
                case 8:
                    typename = "功能模型";
                    break;
                case 9:
                    typename = "用户注册";
                    break;
                case 10://黄页申请模型
                    typename = "黄页申请";
                    break;
                case 11://CRM模型
                    typename = "CRM模型";
                    break;
                case 12:
                    typename = "OA办公模型";
                    break;
            }
            return typename;
        }
        public int GetModelInt(string name)
        {
            name = name.Replace(" ", "").Replace("'", "");
            switch (name)
            {
                case "内容模型":
                    return 1;
                case "商品模型":
                    return 2;
                case "用户模型":
                    return 3;
                case "黄页内容模型":
                    return 4;
                case "店铺商品模型":
                    return 5;
                case "店铺申请模型":
                    return 6;
                case "互动模型":
                    return 7;
                case "功能模型":
                    return 8;
                case "用户注册":
                    return 9;
                case "黄页申请":
                    return 10;
                case "CRM模型":
                    return 11;
                case "OA办公模型":
                    return 12;
                default:
                    throw new Exception("[" + name + "]类型不存在");
            }
        }
        #endregion
        #region 添加模型
        /// <summary>
        /// [main]创建模型,表,字段
        /// </summary>
        public bool AddModel(M_ModelInfo model,string sql="")
        {
            if (string.IsNullOrEmpty(model.ItemIcon)) { model.ItemIcon = "fa fa-file"; }//默认模型图标
            model.TableName = model.TableName.Replace(" ", "");
            if (string.IsNullOrEmpty(model.TableName)) { throw new Exception("模型表名不能为空"); }
            if (DBCenter.DB.Table_Exist(model.TableName)) { throw new Exception("模型表[" + model.TableName + "]已存在,取消创建"); }
            model.ModelID = DBCenter.Insert(model);
            if (!string.IsNullOrEmpty(sql))
            {
                DBCenter.DB.ExecuteNonQuery(new SqlModel(sql,null));
            }
            //如果是内容模型
            if (model.TableName.ToLower().Contains("zl_c_"))
            {
                new B_ModelField().AddFroms(model.ModelID);
            }
            DBCenter.DB.Table_Add(model.TableName, new M_SQL_Field() { fieldName = "ID", fieldType = "int", ispk = true });
            return true;
        }

        /// <summary>
        ///添加互动模型
        /// </summary>
        public int AddPubModel(M_ModelInfo model)//模型尚未插入数据库
        {
            //string strSql = "CREATE TABLE dbo." + model.TableName + " ([ID] [int] IDENTITY (1, 1) PRIMARY Key NOT NULL)";
            //AddModel(model, strSql);
            return model.ModelID;
        }
        public bool AddStoreModel(M_ModelInfo model)
        {
            //string strSql = "CREATE TABLE dbo." + model.TableName + @" ([ID][int] IDENTITY (1, 1)  NOT NULL PRIMARY KEY ,
            //                [UserID][int] NOT NULL,
            //                [UserName][nvarchar] (255) NULL,
            //                [StoreName][varchar] (50) NULL,
            //                [StoreCredit] [int] NULL DEFAULT (0),
            //                [StoreCommendState] [int] NULL DEFAULT (0),
            //                [StoreState] [int] NULL DEFAULT (0),
            //                [StoreStyleID] [int] NULL DEFAULT (0),
            //                [StoreModelID] [int] NULL ,
            //                [StoreStyle] [int] NULL DEFAULT (0),
            //                [AddTime][datetime] NULL DEFAULT (getdate()))";

            //return AddModel(model, strSql);
            return true;
        }
        public bool AddUserModel(M_ModelInfo model)
        {
            //string strSql = "CREATE TABLE dbo." + model.TableName + " ([ID][int] IDENTITY (1, 1)  NOT NULL PRIMARY KEY ,[UserID][int] NOT NULL,[UserName][nvarchar] (255) NULL,[Styleid] [int] NULL DEFAULT (0),[Recycler] [bit] NULL DEFAULT (0),[IsCreate] [bit] NULL DEFAULT (0), [NewTime] [datetime] NULL DEFAULT (getdate()) )";
            //return AddModel(model, strSql);
            return true;
        }
        /// <summary>
        /// 添加功能模型
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        public bool AddFunModel(M_ModelInfo model)
        {
            //string strSql = "CREATE TABLE dbo." + model.TableName + " ([Id] [int] IDENTITY(1,1) NOT NULL PRIMARY KEY,[Uid] [int] NOT NULL,[Title] [nvarchar](50) NULL, [Url] [nvarchar](50) NOT NULL, [PositionTop] [int] NOT NULL, [PositionLeft] [int] NOT NULL, [PositionWidth] [int] NOT NULL, [PositionHeight] [int] NOT NULL, [IsVisble] [int] NOT NULL, [UpdateTime] [datetime] NOT NULL, [PageId] [int] NOT NULL, [MenuID] [int] NOT NULL, [LevelCut] [int] NULL, [VerticalCut] [int] NULL,[status] [int] NULL)";
            //return AddModel(model, strSql);
            return true;
        }
        public bool AddPageModel(M_ModelInfo model)
        {
            //string strSql = "CREATE TABLE dbo." + model.TableName + " ([ID][int] IDENTITY (1, 1)  NOT NULL PRIMARY KEY ,[UserID][int] NOT NULL,[UserName][nvarchar] (255) NULL,[Styleid] [int] NULL DEFAULT (0))";
            //return AddModel(model, strSql);
            return true;
        }
        public bool AddUserModel_Info(M_ModelInfo ModelInfo)
        {
            string strSql1 = "INSERT INTO [ZL_Model] ([ModelName],[Description],[TableName],[ItemName],[ItemUnit],[ItemIcon],[ModelType],[ContentTemplate],[MultiFlag],[NodeID],[SysModel],[FromModel],[Thumbnail],[Islotsize])VALUES(@ModelName,@Description,@TableName,@ItemName,@ItemUnit,@ItemIcon,@ModelType,@ContentTemplate,@MultiFlag,@NodeID,@SysModel,@FromModel,@Thumbnail,@Islotsize)";
            SqlParameter[] cmdParams = ModelInfo.GetParameters();
            ExecuteSql(strSql1, cmdParams);
            string strSql = "CREATE TABLE dbo." + ModelInfo.TableName + " ([ID][int] IDENTITY (1, 1)  NOT NULL PRIMARY KEY ,[UserID][int] NOT NULL,[UserName][nvarchar] (255) NULL,[Styleid] [int] NULL DEFAULT (0),[Recycler] [bit] NULL DEFAULT (0),[IsCreate] [bit] NULL DEFAULT (0), [NewTime] [datetime] NULL DEFAULT (getdate()) )";
            return ExecuteSql(strSql);
        }
        public bool Addinput_Info(M_ModelInfo ModelInfo)
        {
            string strSql1 = "SET IDENTITY_INSERT [ZL_Model] ON;INSERT INTO [ZL_Model] ([ModelID],[ModelName],[Description],[TableName],[ItemName],[ItemUnit],[ItemIcon],[ModelType],[ContentTemplate],[MultiFlag],[NodeID],[SysModel],[FromModel],[Thumbnail],[Islotsize])VALUES(@ModelID,@ModelName,@Description,@TableName,@ItemName,@ItemUnit,@ItemIcon,@ModelType,@ContentTemplate,@MultiFlag,@NodeID,@SysModel,@FromModel,@Thumbnail,@Islotsize);SET IDENTITY_INSERT [ZL_Model] OFF";
            SqlParameter[] cmdParams = ModelInfo.GetParameters();
            ExecuteSql(strSql1, cmdParams);

            string strSql = "CREATE TABLE dbo." + ModelInfo.TableName + " ([ID] [int] IDENTITY (1, 1) PRIMARY Key NOT NULL)";
            return ExecuteSql(strSql);
        }
        /// <summary>
        /// 添加导入模型
        /// </summary>
        public bool Addinput(M_ModelInfo ModelInfo)
        {
            string strSql1 = "SET IDENTITY_INSERT [ZL_Model] ON;INSERT INTO [ZL_Model] ([ModelID],[ModelName],[Description],[TableName],[ItemName],[ItemUnit],[ItemIcon],[ModelType],[ContentTemplate],[MultiFlag],[NodeID],[SysModel],[FromModel],[Thumbnail],[Islotsize])VALUES(@ModelID,@ModelName,@Description,@TableName,@ItemName,@ItemUnit,@ItemIcon,@ModelType,@ContentTemplate,@MultiFlag,@NodeID,@SysModel,@FromModel,@Thumbnail,@Islotsize);SET IDENTITY_INSERT [ZL_Model] OFF";
            SqlParameter[] cmdParams = ModelInfo.GetParameters();
            ExecuteSql(strSql1, cmdParams);

            string strSql = "CREATE TABLE dbo." + ModelInfo.TableName + " ([ID] [int] IDENTITY (1, 1) PRIMARY Key NOT NULL)";
            return ExecuteSql(strSql);
        }
        #endregion
        private static bool ExecuteSql(string sql,SqlParameter[] sp=null)
        {
            DBCenter.DB.ExecuteNonQuery(new SqlModel(sql, sp));
            return true;
        }

        public int Insert(M_ModelInfo model)
        {
            return DBCenter.Insert(model);
        }
        public bool UpdateByID(M_ModelInfo model)
        {
            return DBCenter.UpdateByID(model,model.ModelID);
        }
    }
}
