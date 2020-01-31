using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using ZoomLa.Components;
using ZoomLa.SQLDAL;

namespace ZoomLa.BLL.Helper
{
    /// <summary>
    /// 用于支持数据表递归层級
    /// *关系字段须为ParentID
    /// </summary>
    public class TBTreeHelper
    {
        public static int SelFirstNodeID(string TbName, string PK, int nodeid, ref string nodeTree)
        {
            int firstNodeID = 0;
            if (nodeid < 1) { return firstNodeID; }
            string sql = "with f as(SELECT * FROM " + TbName + " WHERE " + PK + "=" + nodeid + " UNION ALL SELECT A.* FROM " + TbName + " A, f WHERE a." + PK + "=f.ParentID) SELECT * FROM " + TbName + " WHERE " + PK + " IN(SELECT " + PK + " FROM f)";
            string oracle = "SELECT * FROM " + TbName + " Where " + PK + "=" + nodeid;//[need deal]
            DataTable dt = DBCenter.ExecuteTable(DBCenter.GetSqlByDB(sql, oracle));
            if (dt.Rows.Count < 1) { return firstNodeID; }
            //顺序无法确定,首位可能是第一,也可能是最后???
            foreach (DataRow dr in dt.Rows)
            {
                if (DataConvert.CLng(dr["ParentID"]) == 0) { firstNodeID = Convert.ToInt32(dr[PK]); }
                nodeTree += dr[PK] + ",";
            }
            nodeTree = nodeTree.Trim(',');
            return firstNodeID;
        }
        public static DataTable SelByPid(string TbName, string PK, int pid)
        {
            string sql = "with Tree as(SELECT * FROM {0} WHERE ParentID=" + pid + " UNION ALL SELECT a.* FROM {0} a JOIN Tree b on a.ParentID=b.{1}) SELECT * FROM Tree AS A ORDER {1} ASC";
            //获取父节点下子级后,再去除父级
            string oracle = "SELECT * FROM(SELECT * FROM {0} START WITH {1} IN(" + pid + ") CONNECT BY nocycle prior {1}=ParentID )WHERE {1}!=" + pid;
            sql = string.Format(sql, TbName, PK);
            oracle = string.Format(oracle, TbName, PK);
            DataTable dt = DBCenter.ExecuteTable(DBCenter.GetSqlByDB(sql, oracle));
            return dt;
        }
        /// <summary>
        /// 见NodeTree.ascx
        /// </summary>
        public static string ConverForTree(DataTable dt, TreeConfig config)
        {
            string ulHtml = "<ul class='tvNav'><li><a class='list1' id='a0' href='" + config.url + "' target='" + config.target + "' ><span class='list_span'>" + config.allText + "</span> <i class='zi zi_thlarge'></i></a>{html}</li></ul>";
            config.hasChild = config.hasChild.Replace("{url}", config.url);
            config.noChild = config.noChild.Replace("{url}", config.url);
            return ulHtml.Replace("{html}", ConverForTree_LI(dt, 0, config));
        }
        private static string ConverForTree_LI(DataTable dt, int pid, TreeConfig config)
        {
            string result = "";
            DataRow[] dr = dt.Select("ParentID='" + pid + "'");
            for (int i = 0; i < dr.Length; i++)
            {
                result += "<li>";
                if (dt.Select("ParentID='" + Convert.ToInt32(dr[i]["NodeID"]) + "'").Length > 0)
                {
                    result += string.Format(config.hasChild, dr[i]["NodeID"], dr[i]["NodeName"]);
                    result += "<ul class='tvNav tvNav_ul' style='display:none;'>" + ConverForTree_LI(dt, Convert.ToInt32(dr[i]["NodeID"]), config) + "</ul>";
                }
                else
                {
                    result += string.Format(config.noChild, dr[i]["NodeID"], dr[i]["NodeName"]);
                }
                result += "</li>";
            }
            return result;
        }
        /// <summary>
        /// 1.修改列名为NodeID,NodeName,ParentID字段
        /// 2.<select id='node_dp' name='node_dp'>" + ConverForSelect(dt) + "</select>
        /// </summary>
        /// <returns></returns>
        public static string ConverForSelect(DataTable dt, int depth = 0, int pid = 0)
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
                    result += ConverForSelect(dt, depth, Convert.ToInt32(dr[i]["NodeID"]));
                }
                else
                {
                    result += string.Format(noChild, dr[i]["NodeID"], dr[i]["NodeName"], pre);
                }
            }
            return result;
        }
        /// <summary>
        /// 
        /// LstNodes.DataSource = ConverForListBox(dt,0);
        /// LstNodes.DataTextField = "NodeName";LstNodes.DataValueField = "NodeID";LstNodes.DataBind();
        /// </summary>
        /// <param name="dt">所有元素,带depth层级参数</param>
        /// <returns>按子父级关系输出table</returns>
        public static DataTable ConverForListBox(DataTable dt, int pid = 0)
        {
            //DataTable dt = SelByPid(pid, true);
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
        /// 増加depth列,从1开始
        /// </summary>
        public static void AddDepthColumn(DataTable dt)
        {
            if (!dt.Columns.Contains("depth")) { dt.Columns.Add(new DataColumn("depth", typeof(int))); }
            AddDedpthColumn_Loop(dt);
        }
        private static void AddDedpthColumn_Loop(DataTable dt, int pid=0, int depth=1)
        {
            DataRow[] drs = dt.Select("ParentID=" + pid);
            for (int i = 0; i < drs.Length; i++)
            {
                drs[i]["depth"] = depth;
                AddDedpthColumn_Loop(dt, Convert.ToInt32(drs[i]["NodeID"]), (depth + 1));
            }
        }
        /// <summary>
        /// 以层级关系,将其加入至需要返回的表
        /// </summary>
        private static void AddToDT(DataTable dt, DataTable result, int pid)
        {
            DataRow[] drs = dt.Select("ParentID=" + pid);
            for (int i = 0; i < drs.Length; i++)
            {
                result.ImportRow(drs[i]);
                AddToDT(dt, result, Convert.ToInt32(drs[i]["NodeID"]));
            }
        }
        private static string GetLevelStr(string pre, string preChar, int level)
        {
            string str = "";
            for (int i = 1; i < level; i++)
            {
                str += pre;
            }
            return (str += preChar);
        }
    }
    public class TreeConfig
    {
        public string allText = "全部内容";
        public string target = "main_right";
        public string url = "/" + SiteConfig.SiteOption.ManageDir + "/Content/ContentManage?NodeID={0}";
        /// <summary>
        /// {0}=NodeID,{1}=NodeName
        /// </summary>
        public string hasChild = "<a href='{url}' target='main_right' id='a{0}' class='list1'><span class='list_span'>{1}</span> <i class='zi zi_forDown' title='浏览父节点'></i></a>";
        public string noChild = "<a href='{url}' target='main_right' onclick='NodeTree.activeSelf(this);'>{1}</a>";
    }
}
