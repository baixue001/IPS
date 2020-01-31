using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Text.RegularExpressions;
using ZoomLa.BLL.Helper;
using ZoomLa.Common;
using ZoomLa.Model;
using ZoomLa.Model.Content;
using ZoomLa.Safe;
using ZoomLa.SQLDAL;
using ZoomLa.SQLDAL.SQL;

namespace ZoomLa.BLL.Content
{
    public class B_Node : ZoomLa.BLL.ZL_Bll_InterFace<M_Node>
    {
        private string TbName, PK;
        private M_Node initMod = new M_Node();
        private string defOrder = "OrderID ASC,NodeID ASC";
        public B_Node()
        {
            TbName = initMod.TbName;
            PK = initMod.PK;
        }
        #region 兼容区
        public DataTable GetNodeListContainXML(int pid)
        {
            return SelByPid(pid, true);
        }
        public void UpdateNodeXMLAll()
        {

        }
        public void UpNode()
        {

        }
        /// <summary>
        /// 检测是否存在该子节点
        /// </summary>
        /// <param name="main">父节点ID</param>
        /// <param name="child">子节点ID</param>
        /// <returns></returns>
        public bool checkChild(int main, int child)
        {
            if (main == 0) return false;
            if (main == child) return true;
            DataTable dt = SelByPid(main, true);
            return dt.Select("NodeID=" + child).Length > 0;
        }
        #endregion
        #region 整理区
        public int AddNode(M_Node model)
        {
            return Insert(model);
        }
        #endregion
        #region Tools
        /// <summary>
        /// 获取指定节点的起始父节点,并生成节点树,不包含0
        /// 注意节点不要存在循环(如NodeID=ParentID,或圆形)
        /// </summary>
        public int SelFirstNodeID(int nodeid, ref string nodeTree)
        {
            //int firstNodeID = 0;
            //if (nodeid < 1) { return firstNodeID; }
            //string sql = "with f as(SELECT * FROM ZL_Node WHERE NodeID=" + nodeid + " UNION ALL SELECT A.* FROM ZL_Node A, f WHERE a.NodeID=f.ParentID) SELECT * FROM ZL_Node WHERE NodeID IN(SELECT NodeID FROM f)";
            ////string oracle = "SELECT * FROM ZL_Node START WITH NODEID =" + nodeid + " CONNECT BY PRIOR ParentID = NodeID";
            //string oracle = "SELECT * FROM ZL_Node Where NodeID=" + nodeid;//[need deal]
            //DataTable dt = DBCenter.ExecuteTable(DBCenter.GetSqlByDB(sql, oracle));
            //if (dt.Rows.Count < 1) { return firstNodeID; }
            ////顺序无法确定,首位可能是第一,也可能是最后???
            //foreach (DataRow dr in dt.Rows)
            //{
            //    if (DataConvert.CLng(dr["ParentID"]) == 0) { firstNodeID = Convert.ToInt32(dr["NodeID"]); }
            //    nodeTree += dr["NodeID"] + ",";
            //}
            //nodeTree = nodeTree.Trim(',');
            //return firstNodeID;
            int count = 0;
            if (nodeid < 1) { return 0; }
            while (nodeid != 0 && count < 10)
            {
                DataTable dt = DBCenter.SelWithField("ZL_Node", "NodeID,ParentID", "NodeID=" + nodeid);
                if (dt.Rows.Count > 0)
                {
                    nodeTree += nodeid + ",";
                    nodeid = DataConvert.CLng(dt.Rows[0]["ParentID"]);
                }
                else
                {
                    nodeid = 0;
                }
            }
            nodeTree = nodeTree.Trim(',');
            return nodeid;
        }
        public int SelFirstNodeID(int nodeid)
        {
            string nodeTree = "";
            return SelFirstNodeID(nodeid, ref nodeTree);
        }
        /// <summary>
        /// 创建下拉选单,根据需要,在外层套上请选择或全部选择
        /// </summary>
        public string CreateDP(DataTable dt, int depth = 0, int pid = 0)
        {
            string hasChild = "<option value='{0}'>{2}|-{1}</option>";
            string noChild = "<option value='{0}'>{2}|-{1}</option>";
            string result = "", pre = "", span = "&nbsp&nbsp";
            DataRow[] dr = dt.Select("ParentID='" + pid + "'");
            depth++;
            for (int i = 1; i < depth; i++)
            {
                pre = span + pre;
            }
            for (int i = 0; i < dr.Length; i++)
            {
                if (dt.Select("ParentID='" + Convert.ToInt32(dr[i]["NodeID"]) + "'").Length > 0)
                {
                    result += string.Format(hasChild, dr[i]["NodeID"], dr[i]["NodeName"], pre);
                    result += CreateDP(dt, depth, Convert.ToInt32(dr[i]["NodeID"]));
                }
                else
                {
                    result += string.Format(noChild, dr[i]["NodeID"], dr[i]["NodeName"], pre);
                }
            }
            return result;
        }
        /// <summary>
        /// 批量修改,ListBox,需要特殊处理,按子父级关系输出table
        /// </summary>
        /// <returns></returns>
        public DataTable CreateForListBox(int pid = 0)
        {
            DataTable dt = SelByPid(pid, true);
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                DataRow dr = dt.Rows[i];
                int level = Convert.ToInt32(dr["Depth"]);
                dr["NodeName"] = GetLevelStr("　", "|-", level) + dr["NodeName"];
            }
            //重新整理一次,再输出,dt是已排序好的数据
            DataTable result = dt.Clone();
            AddToDT(dt, result, pid);
            return result;
        }
        /// <summary>
        /// 以层级关系,将其加入至需要返回的表
        /// </summary>
        private void AddToDT(DataTable dt, DataTable result, int pid)
        {
            DataRow[] drs = dt.Select("ParentID=" + pid);
            for (int i = 0; i < drs.Length; i++)
            {
                result.ImportRow(drs[i]);
                AddToDT(dt, result, Convert.ToInt32(drs[i]["NodeID"]));
            }
        }
        /// <summary>
        /// 将重写路径转换为aspx虚拟路径
        /// </summary>
        /// <returns></returns>
        public static string ConvertUrl(string url)
        {
            if (string.IsNullOrEmpty(url)) { return ""; }
            //不支持/Item/11路径
            url = url.Replace(" ", "").ToLower();
            string dir = "~/BU/Front/";
            string realurl = url;//要返回的url
            if (url.StartsWith("/class_"))
            {
                MatchCollection matchs = Regex.Matches(url, "(?<=(_))[.\\s\\S]*?(?=(/|))");
                int id = DataConverter.CLng(matchs[0].Value);
                int cpage = DataConverter.CLng(matchs.Count > 1 ? matchs[1].Value : "1");
                if (url.IndexOf("/nodepage") > 0)
                {
                    realurl = dir + "NodePage?ID=" + id + "&CPage=" + cpage;
                }
                else if (url.IndexOf("default") > 0)
                {
                    realurl = dir + "ColumnList?ID=" + id + "&CPage=" + cpage;
                }
                else if (url.IndexOf("nodenews") > 0)
                {
                    realurl = dir + "NodeNews?ID=" + id + "&CPage=" + cpage;
                }
                else if (url.IndexOf("nodehot") > 0)
                {
                    realurl = dir + "NodeHot?ID=" + id + "&CPage=" + cpage;
                }
                else if (url.IndexOf("nodeelite") > 0)
                {
                    realurl = dir + "NodeElite?ID=" + id + "&CPage=" + cpage;
                }
            }
            else if (url.StartsWith("/item/"))
            {
                // /Item/10   /Item/10_2   
                url = Regex.Split(url, Regex.Escape("/item/"))[1].Split('.')[0];//10||10_2-->10||10_2
                if (url.Contains("_"))
                {
                    realurl = dir + "Content?ID=" + url.Split('_')[0] + "&CPage=" + url.Split('_')[1];
                }
                else
                {
                    realurl = dir + "Content?ID=" + url;
                }
            }
            return realurl;
        }
        #endregion
        public DataTable Sel()
        {
            return DBCenter.Sel(TbName, "", defOrder);
        }
        public PageSetting SelPage(int cpage, int psize, F_Node filter)
        {
            List<SqlParameter> sp = new List<SqlParameter>();
            string where = " 1=1 ";
            if (filter.status != -100)
            {
                where += " AND ZStatus IN (" + filter.status + ")";
            }
            if (!string.IsNullOrEmpty(filter.skey))
            {
                where += " AND NodeName LIKE @skey";
            }
            PageSetting setting = PageSetting.Single(cpage, psize, TbName, PK, where, defOrder, sp);
            DBCenter.SelPage(setting);
            return setting;
        }
        //public DataTable Sel(int pid = -1, string nodeName = "")
        //{
        //    string field = " A.*,(SELECT COUNT(GeneralID) FROM ZL_CommonModel WHERE NodeID=A.NodeID AND Status=99) ItemCount,(SELECT Count(NodeID) From " + TbName + " WHERE A.NodeID=ParentID) ChildCount ";
        //    string where = "";
        //    List<SqlParameter> sp = new List<SqlParameter>();
        //    if (!string.IsNullOrEmpty(nodeName)) { where += "NodeName LIKE @nodename"; sp.Add(new SqlParameter("nodename", "%" + nodeName + "%")); }
        //    DataTable dt = DBCenter.SelWithField(TbName, field, where, "CDate DESC", sp);
        //    dt.Columns["ItemCount"].ReadOnly = false;//使用了聚合,需要取消只读
        //    CountDT(dt, null);
        //    DataTable result = null;
        //    if (pid == -1)//非显示全部,不用排序
        //    {
        //        result = dt.Clone();
        //        AddToDT(dt, result, 0);
        //    }
        //    else
        //    {
        //        result = dt;
        //        result.DefaultView.RowFilter = "ParentID=" + pid;
        //        result = result.DefaultView.ToTable();
        //    }
        //    return result;
        //}
        //根据节点父id得到子节点ids
        public string GetChilds(int pid)
        {
            DataTable dt = SelByPid(pid, true);
            string nodeids = pid.ToString();
            foreach (DataRow dr in dt.Rows)
            {
                nodeids += "," + dr["NodeID"];
            }
            return nodeids;
        }
        /// <summary>
        ///获取子节点列表,不包含父节点信息
        /// </summary>
        /// <param name="pid">父节点ID</param>
        /// <param name="isall">true所有子节点信息,false:仅下一级</param>
        public DataTable SelByPid(int pid, bool isall = false)
        {
            DataTable dt = new DataTable();
            if (isall)
            {
                //string sql = "with Tree as(SELECT *,cast(1 as int) as [level] FROM ZL_Node WHERE ParentID=" + pid + " UNION ALL SELECT a.*,b.[level]+1 FROM ZL_Node a JOIN Tree b on a.ParentID=b.NodeID) SELECT *,(SELECT Count(NodeID) From ZL_Node WHERE A.NodeID=ParentID) ChildCount FROM Tree AS A ORDER BY OrderID,NodeID ASC";
                //DataTable dt = Sel();
                //string nids = "";
                //RecursionByPid(dt, pid, ref nids);
                //nids = StrHelper.PureIDSForDB(nids);
                //dt.DefaultView.RowFilter = "NodeID IN(" + nids + ")";
                //return dt.DefaultView.ToTable();
                //WITH Tree AS(
                //SELECT * FROM ZL_Node WHERE ParentID=0 UNION ALL SELECT A.* FROM ZL_Node A JOIN Tree B on A.ParentID=B.NodeID) 
                //SELECT * FROM Tree ORDER BY OrderID,NodeID ASC
                string sql = "with Tree as(SELECT * FROM ZL_Node WHERE ParentID=" + pid + " UNION ALL SELECT a.* FROM ZL_Node a JOIN Tree b on a.ParentID=b.NodeID) SELECT *,(SELECT Count(NodeID) From ZL_Node WHERE A.NodeID=ParentID AND ZStatus=99) ChildCount FROM Tree AS A ORDER BY OrderID,NodeID ASC";
                //获取父节点下子级后,再去除父级
                string oracle = "SELECT * FROM(SELECT * FROM ZL_Node START WITH NODEID IN(" + pid + ") CONNECT BY nocycle prior NodeID=ParentID )WHERE NodeID!=" + pid;
                dt = DBCenter.ExecuteTable(DBCenter.GetSqlByDB(sql, oracle));
            }
            else
            {
                string fields = "A.*,(SELECT Count(NodeID) From ZL_Node WHERE A.NodeID=ParentID) ChildCount ";
                //string sql = "SELECT " + fields + " FROM " + TbName + " A WHERE ParentID=" + pid+" ORDER BY OrderID,NodeID ASC";
                dt = DBCenter.SelWithField(TbName, fields, "ParentID=" + pid, defOrder);
            }
            dt.DefaultView.RowFilter = "ZStatus IS NULL OR ZStatus NOT IN ('" + (int)ZLEnum.ConStatus.Recycle + "')";
            dt = dt.DefaultView.ToTable();
            return dt;
        }
        //public DataTable SelByPid(int pid, bool isall = false)
        //{
        //    DataTable dt = new DataTable();
        //    dt = DBCenter.ExecuteTable("SELECT * FROM ZL_Node WHERE ParentID=" + pid);
        //    dt.DefaultView.RowFilter = "ZStatus IS NULL OR ZStatus NOT IN ('" + (int)ZLEnum.ConStatus.Recycle + "')";
        //    dt = dt.DefaultView.ToTable();
        //    return dt;
        //}
        public DataTable SelByIDS(string nodeids, string nodeType = "")
        {
            SafeSC.CheckIDSEx(nodeids);
            string where = "NodeID IN (" + nodeids + ")";
            if (!string.IsNullOrEmpty(nodeType)) { SafeSC.CheckIDSEx(nodeType); where += " AND NodeType IN (" + nodeType + ")"; }
            return DBCenter.Sel(TbName, where, defOrder);
        }
        public DataTable SelByType(string nodeType)
        {
            SafeSC.CheckIDSEx(nodeType);
            string where = "NodeType IN (" + nodeType + ")";
            return DBCenter.Sel(TbName, where, defOrder);
        }
        /// <summary>
        /// 用于节点管理--显示全部
        /// (仅统计内容方面的节点,所以数量会与ZL_CommonModel表中的总数有差异)
        /// </summary>
        /// <returns></returns>
        public DataTable SelForShowAll(int pid, bool isall = false, int zstatus = 0)
        {
            string fields = "A.NodeID,A.NodeName,A.NodeType,A.NodeDir,A.Depth,A.ContentModel,A.ParentID,A.ZStatus,";
            //模板
            fields += "A.ListTemplateFile,A.LastinfoTemplate,A.HotinfoTemplate,A.ProposeTemplate,A.IndexTemplate,";
            //兼容
            //fields += "A.NodeBySite,isShow=0,isOpen=0,";
            fields += "A.NodeBySite,0 AS isShow,0 AS isOpen,";
            fields += "(SELECT COUNT(GeneralID) FROM ZL_CommonModel WHERE NodeID=A.NodeID AND Status=99) ItemCount,(SELECT Count(NodeID) From " + TbName + " WHERE A.NodeID=ParentID) ChildCount ";
            DataTable dt = DBCenter.SelWithField(TbName, fields, "", defOrder);
            dt.Columns["ItemCount"].ReadOnly = false;//使用了聚合,需要取消只读
            CountDT(dt, null);
            DataTable result = null;
            if (isall)//非显示全部,不用排序
            {
                result = dt.Clone();
                AddToDT(dt, result, 0);
            }
            else
            {
                result = dt;
                result.DefaultView.RowFilter = "ParentID=" + pid;
                result = result.DefaultView.ToTable();
            }
            switch (zstatus)
            {
                case (int)ZLEnum.ConStatus.Recycle:
                    result.DefaultView.RowFilter = "ZStatus IN ('" + (int)ZLEnum.ConStatus.Recycle + "')";
                    result = result.DefaultView.ToTable();
                    break;
                default://99,0(old)
                    result.DefaultView.RowFilter = "ZStatus IS NULL OR ZStatus NOT IN ('" + (int)ZLEnum.ConStatus.Recycle + "')";
                    result = result.DefaultView.ToTable();
                    break;
            }
            return result;
        }
        public DataTable SelectNodeHtmlXML()
        {
            return SelByPid(0, true);
        }
        public void UpdateNode(M_Node model)
        {
            UpdateByID(model);
        }
        #region 删除操作
        /// <summary>
        /// 删除所有节点及节点模板
        /// </summary>
        public void DeleteAll()
        {
            DBCenter.DB.Table_Clear("ZL_Node_ModelTemplate");
            DBCenter.DB.Table_Clear(TbName);
        }
        /// <summary>
        /// 更改节点数据
        /// </summary>
        public void InitNode(string tablename, string fieldname)
        {
            DBCenter.DB.Table_Clear(TbName);
        }
        /// <summary>
        /// 删除节点，并删除该节点的子节点   
        /// </summary>
        public void DelNode(string nids)
        {
            if (string.IsNullOrEmpty(nids)) { return; }
            SafeSC.CheckIDSEx(nids);
            DBCenter.DelByWhere(TbName, "NodeID IN(" + nids + ")");
        }
        public void DelToRecycle(int nid)
        {
            DelToRecycle(nid.ToString());
        }
        public void DelToRecycle(string ids)
        {
            if (string.IsNullOrEmpty(ids)) { return; }
            SafeSC.CheckIDSEx(ids);
            int status = (int)ZLEnum.ConStatus.Recycle;
            DBCenter.UpdateSQL(TbName, "ZStatus=" + status, "NodeID IN (" + ids + ")");
        }
        public void Reconvery(string nids)
        {
            if (string.IsNullOrEmpty(nids)) { return; }
            SafeSC.CheckIDSEx(nids);
            DBCenter.UpdateSQL(TbName, "ZStatus=99", "NodeID IN (" + nids + ")");
        }
        #endregion
        #region 排序
        /// <summary>
        /// 获取父节点下的第一个节点的ID
        /// </summary>
        public int GetFirstNode(int ParentID)
        {
            return DataConvert.CLng(DBCenter.ExecuteScala(TbName, "NodeID", "ParentID=" + ParentID + " AND NodeType=1", defOrder));
        }
        public int GetMaxOrder(int ParentID)
        {
            return DataConvert.CLng(DBCenter.ExecuteScala(TbName, "MAX(OrderID)", "ParentID=" + ParentID));
        }
        public int GetMinOrder(int ParentID)
        {
            return DataConvert.CLng(DBCenter.ExecuteScala(TbName, "MIN(OrderID)", "ParentID=" + ParentID));
        }
        private int dal_GetPreNode(int ParentID, int CurrentID)
        {
            return DataConvert.CLng(DBCenter.ExecuteScala(TbName, "NodeID", "ParentID=" + ParentID + " and OrderID<" + CurrentID, defOrder));
        }
        public M_Node GetPreNode(int ParentID, int CurrentID)
        {
            int PID = this.dal_GetPreNode(ParentID, CurrentID);
            return this.SelReturnModel(PID);
        }
        /// <summary>
        /// 获取下一个节点模型,如果不存在,则返回自身
        /// </summary>
        public M_Node GetNextNode(int ParentID, int CurrentID)
        {
            int NextID = CurrentID;
            DataTable dt = DBCenter.SelTop(1, PK, "NodeID", TbName, "ParentID=" + ParentID + " AND OrderID>" + CurrentID, "OrderId");
            if (dt.Rows.Count > 0) { NextID = DataConvert.CLng(dt.Rows); }
            return this.SelReturnModel(NextID);
        }
        #endregion
        public M_Node GetNodeXML(int ID)
        {
            return SelReturnModel(ID);
        }
        public M_Node SelReturnModel(int ID)
        {
            if (ID < 1)
            {
                M_Node nodeMod = new M_Node(true);
                nodeMod.NodeID = 0;
                nodeMod.NodeName = "根节点";
                return nodeMod;
            }
            using (DbDataReader reader = DBCenter.SelReturnReader(TbName, PK, ID))
            {
                if (reader.Read())
                {
                    return initMod.GetModelFromReader(reader);
                }
                else
                {
                    return new M_Node(true);
                }
            }
        }
        public M_Node SelModelByName(string name)
        {
            SqlParameter[] sp = new SqlParameter[] { new SqlParameter("name", name) };
            return SelModelByWhere("NodeName=@name", sp);
        }
        public M_Node SelModelByWhere(string where, SqlParameter[] sp)
        {
            using (DbDataReader reader = DBCenter.SelReturnReader(TbName, where, sp))
            {
                if (reader.Read())
                {
                    return initMod.GetModelFromReader(reader);
                }
                else
                {
                    return new M_Node(true);
                }
            }
        }
        public DataTable GetNodeChildList(int parentID)
        {
            //取消对child的代码维护
            string where = " ZStatus IN (0,99)";
            if (parentID >= 0)
            {
                where += " AND ParentID=" + parentID;
            }
            return DBCenter.SelWithField(TbName, "A.*,(SELECT Count(NodeID) From " + TbName + " WHERE A.NodeID=ParentID) ChildCount", where, defOrder);
        }
        /// <summary>
        /// 从1开始的深度
        /// </summary>
        public int GetDepth(int ParentID)
        {
            if (ParentID == 0) { return 0; }
            string tree = "";
            SelFirstNodeID(ParentID, ref tree);
            return tree.Split(',').Length;
        }
        public int GetNodeListCount(int ParentID)
        {
            return DBCenter.Count(TbName, "ParentID=" + ParentID);
        }
        /// <summary>
        /// 获取选定的单页节点信息
        /// </summary>
        public DataTable GetCreateSingleByID(string nids)
        {
            if (string.IsNullOrEmpty(nids)) return null;
            SafeSC.CheckIDSEx(nids);
            return DBCenter.Sel(TbName, "NodeType=2 and NodeID in (" + nids + ")", "OrderID");
        }
        //返回父节点下的子节点
        public DataTable GetNodeListContainByParentID(int ParentID)
        {
            return DBCenter.Sel(TbName, "ParentID=" + ParentID + " AND NodeListType<>6", "OrderID");
        }
        public DataTable SelNodeByModel(int modelID)
        {
            return SelNodeByModel(modelID.ToString());
        }
        public DataTable SelNodeByModel(string ids)
        {
            SafeSC.CheckIDSEx(ids);
            string where = DBCenter.GetSqlByDB("','+ContentModel+','", "','||ContentModel||','");
            string[] idarr = ids.Split(",".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < idarr.Length; i++)
            {
                where += " LIKE '%," + idarr[i] + ",%' OR";
            }
            where = where.TrimEnd("OR".ToCharArray());
            return DBCenter.Sel(TbName, where);
        }
        /// <summary>
        /// 移动节点
        /// </summary>
        public void UpDNode(int NodeID, int main)
        {
            DBCenter.UpdateSQL(TbName, "ParentID=" + NodeID, "ParentID=" + main);
        }
        /// <summary>
        /// 检测节点名称与栏目名是否存在,用于节点修改
        /// </summary>
        /// <returns>不规范,则为false</returns>
        public bool CheckCanSave(M_Node nodeMod)
        {
            if (StrHelper.StrNullCheck(nodeMod.NodeName, nodeMod.NodeDir)) { return false; }
            List<SqlParameter> sp = new List<SqlParameter>() { new SqlParameter("NodeName", nodeMod.NodeName), new SqlParameter("NodeDir", nodeMod.NodeDir) };
            string where = "NodeID!=" + nodeMod.NodeID + " AND ParentID=" + nodeMod.ParentID + " AND (NodeName=@NodeName OR NodeDir=@NodeDir)";
            return DBCenter.Count(TbName, where, sp) == 0;
        }
        public bool CheckNodeName(M_Node nodeMod)
        {
            if (StrHelper.StrNullCheck(nodeMod.NodeName)) { return false; }
            List<SqlParameter> sp = new List<SqlParameter>() { new SqlParameter("NodeName", nodeMod.NodeName) };
            string where = "NodeID!=" + nodeMod.NodeID + " AND ParentID=" + nodeMod.ParentID + " AND NodeName=@NodeName";
            return DBCenter.Count(TbName, where, sp) == 0;
        }
        public bool CheckNodeDir(M_Node nodeMod)
        {
            if (StrHelper.StrNullCheck(nodeMod.NodeDir)) { return false; }
            List<SqlParameter> sp = new List<SqlParameter>() { new SqlParameter("NodeName", nodeMod.NodeName), new SqlParameter("NodeDir", nodeMod.NodeDir) };
            string where = "NodeID!=" + nodeMod.NodeID + " AND ParentID=" + nodeMod.ParentID + " AND NodeDir=@NodeDir";
            return DBCenter.Count(TbName, where, sp) == 0;
        }
    
        /// <summary>
        /// [main]添加
        /// </summary>
        public int Insert(M_Node model)
        {
            model.Depth = (GetDepth(model.ParentID) + 1);
            if (model.OrderID < 1) { model.OrderID = (GetMaxOrder(model.ParentID)) + 1; }
            model.NodeID = DBCenter.Insert(model);
            return model.NodeID;
        }
        public bool UpdateByID(M_Node model)
        {
            if (model.NodeID < 1) { throw new Exception("更新节点失败,NodeID不能为空"); }
            model.Depth = (GetDepth(model.ParentID) + 1);
            return DBCenter.UpdateByID(model, model.NodeID);
        }
        public bool Del(int ID)
        {
            DelNode(ID.ToString());
            return true;
        }
        // Common/NodeList
        public DataTable SelToNodeList()
        {
            return DBCenter.SelWithField(TbName, "A.*,NodeID AS ID,NodeName AS Name", "", defOrder);
        }
        /// <summary>
        /// html文件夹目录
        /// </summary>
        public string GetDir(int nodeid, string strDir)
        {
            M_Node mn = SelReturnModel(nodeid);
            if (nodeid != 0)
            {
                strDir = GetDir(mn.ParentID, "/" + mn.NodeDir) + strDir;
            }
            else
            {
                if (string.IsNullOrEmpty(strDir))
                {
                    strDir = "/";
                }
                else
                {
                    strDir = mn.NodeDir + strDir;
                }
            }
            return strDir.Replace("//", "/");
        }
        public void GetColumnList(int ParentID, DataTable newDT)
        {
            DataTable oldDT = GetNodeChildList(ParentID);

            for (int i = 0; i < oldDT.Rows.Count; i++)
            {
                string depth = "";
                if (oldDT.Rows[i]["NodeType"].ToString() != "5")
                {
                    if (DataConverter.CLng(oldDT.Rows[i]["Depth"].ToString()) > 1)
                    {
                        for (int x = 0; x < DataConverter.CLng(oldDT.Rows[i]["Depth"].ToString()); x++)
                        {
                            depth += "　";
                        }
                        depth += "├";
                    }
                    DataRow dr = newDT.NewRow();
                    dr[0] = oldDT.Rows[i]["NodeID"].ToString();

                    dr[1] = depth + oldDT.Rows[i]["NodeName"].ToString();
                    newDT.Rows.Add(dr);

                    //判断节点下是否还有节点
                    if (GetNodeListCount(DataConverter.CLng(oldDT.Rows[i]["NodeID"].ToString())) > 0)
                    {
                        GetColumnList(DataConverter.CLng(oldDT.Rows[i]["NodeID"].ToString()), newDT);
                    }
                }
            }
        }
        /// <summary>
        /// View:浏览/查看权限
        /// ViewGroup:允许浏览此栏目的会员组(会员组ids)
        /// input:发表权限(会员组ids)
        /// forum:评论权限(1,允许在此栏目发表评论;2,评论需要审核;3,一篇文章只能发表一次评论)
        /// </summary>
        /// <param name="nodejson"></param>
        /// <returns></returns>
        public DataTable ViewAuth_GetDT(string purview = "")
        {
            DataTable CheckDt = new DataTable();
            CheckDt.Columns.Add("View");
            CheckDt.Columns.Add("ViewGroup");
            CheckDt.Columns.Add("input");
            CheckDt.Columns.Add("forum");
            CheckDt.Rows.Add(CheckDt.NewRow());
            if (!string.IsNullOrEmpty(purview))
            {
                CheckDt = ViewAuth_Conver(purview);
            }
            return CheckDt;
        }
        public DataTable ViewAuth_Conver(string purview)
        {
            DataTable dt = null;
            if (string.IsNullOrEmpty(purview)) { return dt; }
            try
            {
                dt = JsonConvert.DeserializeObject<DataTable>(purview);
                string viewGroup = DataConvert.CStr(dt.Rows[0]["ViewGroup"]);
                dt.Rows[0]["View"] = DataConvert.CStr(dt.Rows[0]["View"]);
                dt.Rows[0]["ViewGroup"] = string.IsNullOrEmpty(viewGroup) ? "" : ("," + viewGroup + ",").Replace(",,", ",");
            }
            catch (Exception ex) { ZLLog.L("conver err:" + ex.Message); dt = null; }
            return dt;
        }
        /// <summary>
        /// 内容转移
        /// </summary>
        /// <param name="tnid">目标节点</param>
        /// <param name="snid">来源节点</param>
        public void UnionUp2(int tnid, int snid)
        {
            DBCenter.UpdateSQL("ZL_CommonModel", "Nodeid=" + tnid, "NodeID=" + snid);
        }
        #region 商城|店铺节点操作
        /// <summary>
        /// 返回所有商城节点,用于生成树节点
        /// </summary>
        public DataTable GetAllShopNode(int parentid = 0)
        {
            string where = "NodeType=1 AND (NodeListType=2 OR NodeListType=3)";
            //默认取出全部给予左边栏使用
            if (parentid > 0) { where += " AND ParentID=" + parentid; }
            return DBCenter.Sel(TbName, where, "OrderID");
        }
        /// <summary>
        /// 返回所有店铺节点,用于生成树节点
        /// </summary>
        public DataTable GetAllUserShopNode()
        {
            return DBCenter.Sel(TbName, "NodeType=1 AND NodeListType=5", defOrder);
        }
        public DataTable GetNodeListUserShop(int ParentID)
        {
            return DBCenter.Sel(TbName, "ParentID=" + ParentID + " AND NodeType=1 And NodeListType=5", defOrder);
        }
        public DataTable GetShopNode(string nodeIDS)
        {
            SafeSC.CheckIDSEx(nodeIDS);
            return DBCenter.Sel(TbName, "NodeType=1 AND (NodeListType=2 OR NodeListType=3) AND NodeID not in (" + nodeIDS + ")", "OrderID,NodeId");
        }
        #endregion
        #region  模板操作
        public void AddModelTemplate(int NodeID, int ModelID, string ModelTemplate)
        {
            SqlParameter[] sp = new SqlParameter[] { new SqlParameter("Template", ModelTemplate) };
            DBCenter.Insert("ZL_Node_ModelTemplate", "NodeID,ModelID,Template", NodeID + "," + ModelID + ",@Template", sp);
        }
        public bool IsExistTemplate(int NodeID, int ModelID)
        {
            return DBCenter.IsExist("ZL_Node_ModelTemplate", "NodeID=" + NodeID + " AND ModelID=" + ModelID);
        }
        public void DelModelTemplate(int NodeID, string ModelIDs)
        {
            ModelIDs = StrHelper.PureIDSForDB(ModelIDs);
            if (string.IsNullOrEmpty(ModelIDs)) { return; }
            SafeSC.CheckIDSEx(ModelIDs);
            DBCenter.DelByWhere("ZL_Node_ModelTemplate", "NodeID=" + NodeID + " AND ModelID Not IN (" + ModelIDs + ")");
        }
        public void UpdateModelTemplate(int NodeID, int ModelID, string ModelTemplate)
        {
            List<SqlParameter> sp = new List<SqlParameter>() { new SqlParameter("Template", ModelTemplate) };
            DBCenter.UpdateSQL("ZL_Node_ModelTemplate", "Template=@Template", "NodeID=" + NodeID + " and ModelID=" + ModelID, sp);
        }
        public void DelTemplate(int NodeID)
        {
            DBCenter.Del("ZL_Node_ModelTemplate", "NodeID", NodeID);
        }
        public string GetModelTemplate(int NodeID, int ModelID)
        {
            return DBCenter.ExecuteScala("ZL_Node_ModelTemplate", "Template", "NodeID=" + NodeID + " and ModelID=" + ModelID).ToString();
        }
        #endregion
        //--------------------------------------Tools
        public static string GetNodeType(int type)
        {
            switch (type)
            {
                case 0:
                    return "根节点";
                case 1:
                    return "栏目节点";
                case 2:
                    return "单页节点";
                case 3:
                    return "外部链接";
                default:
                    return "未知栏目类型";
            }
        }
        private string GetLevelStr(string pre, string preChar, int level)
        {
            string str = "";
            for (int i = 1; i < level; i++)
            {
                str += pre;
            }
            return (str += preChar);
        }
        /// <summary>
        /// 统计数量
        /// </summary>
        private void CountDT(DataTable dt, DataRow parentDR)
        {
            int pid = (parentDR == null ? 0 : Convert.ToInt32(parentDR["NodeID"]));
            DataRow[] drs = dt.Select("ParentID=" + pid);
            for (int i = 0; i < drs.Length; i++)
            {
                CountDT(dt, drs[i]);
                if (parentDR != null)
                {
                    parentDR["ItemCount"] = Convert.ToInt32(parentDR["ItemCount"]) + Convert.ToInt32(drs[i]["ItemCount"]);
                }
            }
        }
        /// <summary>
        /// 用于节点层级展示
        /// </summary>
        /// <param name="dt">节点展示dt</param>
        /// <param name="depth">深度,自动计算,第一级传0</param>
        /// <param name="pid">第一级传0</param>
        /// <param name="hasChild">有子节点的菜单模板</param>
        /// <param name="noChild">无子节点的菜单模板</param>
        /// <returns>用于显示的Html</returns>
        public static string GetLI(DataTable dt, string hasChild, string noChild, int depth = 0, int pid = 0)
        {
            if (dt == null || dt.Rows.Count < 1) { return ""; }
            string result = "", pre = "<img src='/Images/TreeLineImages/t.gif' border='0'>", span = "<img src='/Images/TreeLineImages/tree_line4.gif' border='0' width='19' height='20'>";
            DataRow[] dr = dt.Select("ParentID='" + pid + "'");
            depth++;
            for (int i = 1; i < depth; i++)
            {
                pre = span + pre;
            }
            for (int i = 0; i < dr.Length; i++)
            {
                result += "<li>";
                if (dt.Select("ParentID='" + Convert.ToInt32(dr[i]["ID"]) + "'").Length > 0)
                {
                    result += string.Format(hasChild, dr[i]["ID"], dr[i]["Name"], pre);
                    result += "<ul class='tvNav tvNav_ul'>" + GetLI(dt, hasChild, noChild, depth, Convert.ToInt32(dr[i]["ID"])) + "</ul>";
                }
                else
                {
                    result += string.Format(noChild, dr[i]["ID"], dr[i]["Name"], pre);
                }
                result += "</li>";
            }
            return result;
        }
        public static string GetTreeLine(int depth)
        {
            string pre = "<img src='/Images/TreeLineImages/t.gif' border='0'>", span = "<img src='/Images/TreeLineImages/tree_line4.gif' border='0' width='19' height='20'>";
            for (int i = 1; i < depth; i++)
            {
                pre = span + pre;
            }
            return pre;
        }
        /// <summary>
        /// 根据节点的配置,返回页面后缀名
        /// </summary>
        /// <param name="htmlex">nodeinfo.ListPageHtmlEx</param>
        public static string GetFileEx(int htmlex)
        {
            string fileEx = ".html";
            switch (htmlex)
            {
                case 0:
                    fileEx = ".html";
                    break;
                case 1:
                    fileEx = ".htm";
                    break;
                case 2:
                    fileEx = ".shtml";
                    break;
                case 3:
                    fileEx = "";
                    break;
            }
            return fileEx;
        }
        //--------------------------------------
    }
    public class F_Node
    {
        public int status = -100;
        public string skey = "";
    }
}
