using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using ZoomLa.Model;
using ZoomLa.SQLDAL;
using ZoomLa.SQLDAL.SQL;

namespace ZoomLa.BLL
{
    /// <summary>
    /// 流程处理进度,包含会签信息等
    /// </summary>
    public class B_Mis_AppProg
    {
        public string TbName = "";
        public string PK = "";
        private M_Mis_AppProg model = new M_Mis_AppProg();
        public B_Mis_AppProg()
        {
            TbName = model.TbName;
            PK = model.PK;
        }
        //-----------------Insert
        public int Insert(M_Mis_AppProg model)
        {
            return DBCenter.Insert(model);
        }
        //-----------------Retrieve
        public DataTable SelAll()
        {
            DataTable dt = SqlHelper.ExecuteTable(CommandType.Text, "Select * From " + TbName + " Order By CreateDate Desc");
            return dt;
        }
        /// <summary>
        /// 根据申请单ID,查看结果,含用户名
        /// </summary>
        public DataTable SelByAppID(string appID)
        {
            SqlParameter[] sp = new SqlParameter[] { new SqlParameter("appid", appID) };
            return DBCenter.JoinQuery("A.*,B.UserName,B.HoneyName", TbName, "ZL_User", "a.ApproveID=b.UserID", "A.appid=@appid", "A.ID", sp);
        }
        /// <summary>
        /// 根据申请单ID,会员ID查看结果,含用户名 ---By ZC
        /// </summary>
        public DataTable SelByAppID(string appID, string userID)
        {
            SqlParameter[] sp = new SqlParameter[] { new SqlParameter("appID", appID), new SqlParameter("userID", userID) };
            return SqlHelper.ExecuteTable(CommandType.Text, "Select a.*,b.UserName From " + TbName + " as a Left Join ZL_User as b ON a.ApproveID=b.UserID Where AppID=@appID And ApproveID=@userID Order By ID", sp);
        }
        public DataTable SelHasSign(int appID)
        {
            SqlParameter[] sp = new SqlParameter[] { new SqlParameter("appID", appID) };
            return SqlHelper.ExecuteTable(CommandType.Text, "Select a.*,b.VPath From " + TbName + " as a Left Join ZL_OA_Sign as b ON a.SignID=b.ID Where AppID=@appID And a.[Sign] is not null And a.[Sign]!='' Order By ID", sp);
        }
        /// <summary>
        /// 根据appid,stepnum获取表信息,用于会签
        /// </summary>
        public DataTable SelDTByStep(int appID, int stepNum)
        {
            return SqlHelper.ExecuteTable(CommandType.Text, "Select * From " + TbName + " Where AppID=" + appID + " And ProLevel=" + stepNum, null);
        }
        /// <summary>
        /// 用于会签,根据Appid与StepNum获取到目标模型
        /// </summary>
        /// <returns></returns>
        public M_Mis_AppProg SelByStep(int appID, int stepNum)
        {
            return SelReturnModel("Where AppID=" + appID + " And ProLevel=" + stepNum);
        }
        private M_Mis_AppProg SelReturnModel(string strWhere)
        {
            using (DbDataReader reader = Sql.SelReturnReader(TbName, strWhere))
            {
                if (reader.Read())
                {
                    return model.GetModelFromReader(reader);
                }
                else
                {
                    return null;
                }
            }
        }
        #region 会签操作
        public DataTable HQ_SelAll(int appid, int stepnum, string role = "")
        {
            List<SqlParameter> sp = new List<SqlParameter>();
            if (stepnum == 0) { stepnum = 1; }
            string where = "Appid=" + appid + " AND ProLevel=" + stepnum;
            if (!string.IsNullOrEmpty(role)) { where += " AND UserStepRole=@role"; sp.Add(new SqlParameter("role", role)); }
            return DBCenter.Sel(TbName, where, "", sp);
        }
        /// <summary>
        /// 清除会签信息,用于回退时，将回退到的那步之后所有的会签信息清除.
        /// 但仍保留批示信息
        /// </summary>
        public void HQ_Clear(int appID, int stepNum)
        {
            DBCenter.UpdateSQL(TbName, "HQUserID=''", "AppID=" + appID + " AND ProLevel>" + stepNum);
        }
        /// <summary>
        /// 根据Appid找到已会签的用户
        /// </summary>
        public string HQ_SelUserID(int appID, int stepNum)
        {
            DataTable dt = SelDTByStep(appID, stepNum);
            string result = "";
            foreach (DataRow dr in dt.Rows)
            {
                result += dr["HQUserID"].ToString() + ",";
            }
            result = result.TrimEnd(',');
            return result;
        }
        /// <summary>
        /// 指定步骤,指定类型有多少人完成了会签,多少人尚未完成
        /// </summary>
        /// <param name="oaMod">文档模型</param>
        /// <param name="stepMod">步骤模型,用于知道有哪些用户需要会签</param>
        /// <param name="uid">当前会签的用户</param>
        /// <param name="role">需要检测的角色</param>
        public void HQ_GetHasAndNot(M_OA_Document oaMod, M_MisProLevel stepMod, int uid, string role, ref string hashq, ref string nohq)
        {
            DataTable hqdt = HQ_SelAll(oaMod.ID, oaMod.CurStepNum, role);
            string needhq = "";hashq = "";nohq = "";
            switch (role)
            {
                case "refer":
                    needhq = stepMod.ReferUser;
                    break;
                case "ccuser":
                    needhq = stepMod.CCUser;
                    break;
                case "helpuser":
                    needhq = stepMod.HelpUser;
                    break;
                default:
                    throw new Exception("会签检测角色错误");
            }
            //获取还有哪些用户没完成会签,哪些用户已完成会签(ids)
            foreach (string userid in needhq.Split(",".ToCharArray(), StringSplitOptions.RemoveEmptyEntries))
            {
                if (userid.Equals(uid.ToString())) { hashq += uid + ","; }
                else if (hqdt.Select("ApproveID='" + userid + "' AND UserStepRole='" + role + "'").Length > 0) { hashq += userid + ","; }
                else { nohq += userid + ","; }
            }
            hashq = hashq.Trim(','); nohq = nohq.Trim(',');
        }
        /// <summary>
        /// 指定步骤的会签是否已完成,true:已完成
        /// </summary>
        public bool HQ_IsComplete(M_OA_Document oaMod, M_MisProLevel stepMod, int uid, string role = "refer")
        {
            string hashq = "", nohq = "";
            HQ_GetHasAndNot(oaMod, stepMod, uid, role, ref hashq, ref nohq);
            return string.IsNullOrEmpty(nohq);
        }
        ///// <summary>
        ///// 判断会签是否完成
        ///// </summary>
        ///// <param name="appID"></param>
        ///// <param name="userID">当前签字的经办人ID</param>
        ///// <param name="stepMod">当前步骤模型</param>
        //public bool IsHQComplete(int appID, int userID, M_MisProLevel stepMod)
        //{
        //    //B_Group groupBll=new B_Group();
        //    bool flag = true;
        //    string ids = HQ_SelUserID(appID, stepMod.stepNum);
        //    //if (string.IsNullOrEmpty(ids)) return false;//无会签信息,则直接返回
        //    ids += "," + userID;
        //    string[] progArr = ids.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
        //    //if (stepMod.ReferGroup != "")
        //    //    stepMod.ReferUser += groupBll.GetUserIDByGroupIDS(stepMod.ReferGroup);
        //    string[] stepArr = stepMod.ReferUser.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
        //    for (int i = 0; i < stepArr.Length; i++)
        //    {
        //        if (!progArr.Select(p => p).Contains(stepArr[i]))
        //        {
        //            flag = false;
        //            break;
        //        }
        //    }
        //    return flag;
        //}
        //-------------------------------前端ascx使用
        /// <summary>
        /// 获取指定步骤的会签信息
        /// </summary>
        /// <param name="appid">文档ID</param>
        /// <param name="stepnum">步骤序号</param>
        /// <returns>字符串,有需要可以返回数组等</returns>
        public string SelHQInfo(int appid, int stepnum)
        {
            DataTable dt = SelHQDT(appid, stepnum);
            string result = "";
            foreach (DataRow dr in dt.Rows)
            {
                result += dr["UserName"] + ",";
            }
            return result.TrimEnd(',');
        }
        /// <summary>
        /// 获取全部或指定步骤的会签与协办批复信息
        /// </summary>
        /// <param name="appid">文档ID</param>
        /// <param name="stepnum">步骤ID</param>
        public DataTable SelHQDT(int appid, int stepnum, string order = "desc")
        {
            string where = "AppID=" + appid;
            if (stepnum != -100) { where += " AND ProLevel=" + stepnum; }
            switch (order)
            {
                case "asc":
                    order = "A.ID ASC";
                    break;
                case "desc":
                default:
                    order = "A.ID DESC";
                    break;
            }
            return SqlHelper.JoinQuery("A.*,B.HoneyName,B.UserName", TbName, "ZL_User", "A.ApproveID=B.UserID", where, order);
        }
        #endregion
        //指定角色的会签是否已完成
        //public bool HQ_IsComplete(DataTable dt, int uid, string role = "refer")
        //{
        //    return dt.Select("ApproveID=" + uid + " AND UserStepRole='" + role + "'").Length > 0;
        //}




        /// <summary>
        /// 根据审批，获取已被此用户审批过的文档ids字符串
        /// </summary>
        public string GetAppidByApprove(int userID)
        {
            string ids = "";
            DataTable dt = SqlHelper.ExecuteTable(CommandType.Text, "Select AppID From " + TbName + " Where ApproveID = " + userID + " Group By AppID");
            foreach (DataRow dr in dt.Rows)
            {
                ids += dr["AppID"].ToString() + ",";
            }
            ids = ids.TrimEnd(',');
            return ids;
        }
        /// <summary>
        /// 清除签章信息,用于回退
        /// </summary>
        public void ClearSign(int appID, int stepNum)
        {
            SqlHelper.ExecuteNonQuery(CommandType.Text, "Update " + TbName + " Set SignID=null,Sign=null Where AppID=" + appID + " And ProLevel>" + stepNum);
        }
        /// <summary>
        /// 查看用户是否已对流程审批，是则返回true
        /// </summary>
        public bool CheckApproval(int userid, int stepnum, int appid)
        {
            string where = "Appid=" + appid + " AND ApproveID=" + userid + " AND ProLevel=" + stepnum;
            return DBCenter.IsExist(TbName, where);
        }
        /// <summary>
        /// 更新提交时间,用于公文审批等
        /// </summary>
        public bool UpdateDate(int uid, int id, DateTime date)
        {
            string sql = "Update " + TbName + " Set CreateTime='" + date + "' Where ApproveID=" + uid + " And ID=" + id;
            return SqlHelper.ExecuteSql(sql);
        }
    }
}
