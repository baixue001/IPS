using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using ZoomLa.Common;
using ZoomLa.Components;
using ZoomLa.Model.CreateJS;
using ZoomLa.SQLDAL;
/*
 * 生成发布逻辑类,内容页,列表页,栏目首页,全站首页
 */
namespace ZoomLa.BLL.CreateJS
{
    public class B_Release
    {
        private delegate void ReleaseFunc();
        private const string vpath = "/Config/CHtml.config";
        private const string TbName = "ZL_CommonModel";
        private static int contentCount = 0;
        private static B_Create bc = null;
        //private static ReleaseFunc func = BeginRelease;
        public static List<M_ReleaseResult> ResutList = new List<M_ReleaseResult>();
        public B_Release() { }
        public static List<M_Release> GetModel()
        {
            string ppath = function.VToP(vpath);
            if (!File.Exists(ppath)) { return null; }
            string json = SafeSC.ReadFileStr(vpath);
            if (!string.IsNullOrEmpty(json))
            {
                return JsonConvert.DeserializeObject<List<M_Release>>(json);
            }
            else { return null; }
        }
        public void Insert(M_Release model)
        {
            string ppath = function.VToP(vpath);
            List<M_Release> list = new List<M_Release>();
            if (File.Exists(ppath))
            {
                string json = SafeSC.ReadFileStr(vpath);
                if (!string.IsNullOrEmpty(json))
                {
                    try
                    {
                        list.AddRange(JsonConvert.DeserializeObject<List<M_Release>>(json));
                    }
                    catch (Exception ex) { ZLLog.L("生成时转换模型异常,原因" + ex.Message); }
                }
            }
            list.Add(model);
            string text = JsonConvert.SerializeObject(list);
            File.WriteAllText(ppath, text);
        }
        //异步,后期切换为多线
        public static async void Start(HttpContext ctx)
        {
            bc = new B_Create(ctx);
            await BeginRelease();
        }
        private static Task BeginRelease()
        {
            List<M_Release> list = GetModel();
            if (list == null || list.Count < 1) { AddResult("<i class='fa fa-lightbulb-o'></i> 没有需要生成的内容"); return Task.CompletedTask; }
            File.Delete(function.VToP(vpath));
            foreach (M_Release model in list)
            {
                try
                {
                    #region 生成
                    switch (model.MyRType)
                    {
                        case M_Release.RType.Index://生成全站首页或栏目首页
                            if (string.IsNullOrEmpty(model.NodeIDS))
                            {
                                bc.CreatePageIndex();
                            }
                            //else { bc.CreatePageIndex(DataConverter.CLng(model.NodeIDS)); }
                            break;
                        case M_Release.RType.ALL://生成指定栏目下或全站的内容
                            CreateSiteAll(model.NodeIDS);
                            break;
                        case M_Release.RType.IDRegion:
                            CreateByIDRegion(model);
                            break;
                        case M_Release.RType.Newest://指定数量的最新文章
                            bc.CreateLastInfoRecord(model.Count.ToString());
                            break;
                        case M_Release.RType.DateRegion:
                            CreateByDate(model);
                            break;
                        case M_Release.RType.ByNodeIDS://通过栏目ID发布内容
                            bc.CreateInfoColumn(model.NodeIDS);
                            break;
                        case M_Release.RType.ALLNode://所有栏目的内容页
                            bc.CreateColumnAll();
                            break;
                        case M_Release.RType.NodeIDS://发布选定栏目页
                            bc.CreateColumnByID(model.NodeIDS);
                            break;
                        case M_Release.RType.ALLSPage:
                            bc.CreateSingle();
                            break;
                        case M_Release.RType.SPage://取出指定单页信息,交于节点生成方法生成
                            bc.CreateColumnByID(model.NodeIDS);
                            break;
                        case M_Release.RType.Special://生成专题列表页与所属内容页
                            bc.CreateSpecial(model.NodeIDS);
                            break;
                        case M_Release.RType.Gids://发布选定的多个内容页
                            bc.createann(model.Gids);
                            break;
                        default:
                            ZLLog.L(Model.ZLEnum.Log.labelex, "指定的生成方式不存在");
                            return Task.CompletedTask;
                    }//switch end;
                    #endregion
                }
                catch (Exception ex) { ZLLog.L(Model.ZLEnum.Log.labelex, "BeginRelease--," + JsonConvert.SerializeObject(model) + "原因:" + ex.Message); }
            }
            if (contentCount < 1 && bc.m_CreateCount > 0)
            {
                contentCount = bc.m_CreateCount;
            }
            ResutList.Add(new M_ReleaseResult()
            {
                ResultMsg = "<i class=\"fa fa-check-circle\"></i>生成请求处理完成",
                Status = 99,
                Count = contentCount,
                IsEnd = true
            });
            return Task.CompletedTask;
        }
        #region 生成结果逻辑
        /// <summary>
        /// 处理结果
        /// </summary>
        /// <param name="result">结果字符串</param>
        /// <param name="path">生成的物理路径</param>
        public static void AddResult(string result, string path = "")
        {
            M_ReleaseResult model = new M_ReleaseResult();
            try
            {
                model.VPath = function.PToV(path);
                if (model.VPath.Equals("/")) { model.VPath = ""; }
                model.Url = model.VPath;
                model.ResultMsg = result;
            }
            catch (Exception ex) { model.ResultMsg = ex.Message; }
            ResutList.Add(model);
        }
        #endregion
        //全站生成,或生成指定节点下全站内容页
        private static void CreateSiteAll(string NodeIDS) 
        {
            if (string.IsNullOrEmpty(NodeIDS))
            {
                bc.CreateInfo();
            }
            else
            {
                foreach (string nodeid in NodeIDS.Split(','))
                {
                    int nid = DataConverter.CLng(nodeid);
                    if (nid < 1) { continue; }
                    bc.CreateInfoByNodeID(nid);
                }
            }
        }
        private static void CreateByDate(M_Release model)
        {
            SqlParameter[] sp = new SqlParameter[] { new SqlParameter("stime", model.STime), new SqlParameter("etime", model.ETime) };
            string sql = "SELECT GeneralID,NodeID,ModelID FROM " + TbName + " WHERE (CreateTime BETWEEN @stime and @etime)";
            if (model.NodeID > 0)
            {
                sql += " AND NodeID=" + model.NodeID;
            }
            ZLLog.L(Model.ZLEnum.Log.labelex, sql);
            DataTable dt = SqlHelper.ExecuteTable(CommandType.Text, sql, sp);
            CreateByDT(dt);
        }
        private static void CreateByIDRegion(M_Release model)
        {
            if (model.SGid < 1) return;
            string sql = "SELECT GeneralID,NodeID,ModelID FROM " + TbName + " WHERE GeneralID>=" + model.SGid;
            if (model.EGid > 0)
            {
                sql += " AND GeneralID<=" + model.EGid;
            }
            DataTable dt = SqlHelper.ExecuteTable(CommandType.Text, sql);
            CreateByDT(dt);
        }
        //根据DT生成页面,后期优化为多线程生成
        private static void CreateByDT(DataTable dt) 
        {
            contentCount = dt.Rows.Count;
            foreach (DataRow dr in dt.Rows)
            {
                bc.CreateInfo(DataConverter.CLng(dr["GeneralID"]));
            }
        }
    }
}
