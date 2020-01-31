using System;
using System.Data;
using ZoomLa.Model;
using ZoomLa.Common;
using ZoomLa.SQLDAL;
using System.Data.SqlClient;
using System.Collections.Generic;
using ZoomLa.SQLDAL.SQL;
using Newtonsoft.Json;
using System.Data.Common;

/*
 * OA自由流程,后期与B_MisProcedure(固定流程)合并
 * 
 */
namespace ZoomLa.BLL
{
    public class B_OA_FreePro : ZL_Bll_InterFace<M_MisProLevel>
    {
        public string strTableName, PK;
        public DataTable dt = null;
        public M_MisProLevel model = new M_MisProLevel();
        public B_OA_FreePro()
        {
            strTableName = "ZL_OA_FreePro";
            PK = model.PK;
        }
        public M_MisProLevel SelReturnModel(int ID)
        {
            using (DbDataReader reader = Sql.SelReturnReader(strTableName, PK, ID))
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
        /// <summary>
        /// 获取ProID指定步骤的实例化模型
        /// </summary>
        /// <param name="docID">公文ID</param>
        /// <param name="StepNum">需要的步骤</param>
        /// <returns></returns>
        public M_MisProLevel SelByDocIDAndStepNum(int docID, int StepNum)
        {
            M_MisProLevel mod = null;
            if (StepNum == 0) { mod = new M_MisProLevel() { stepNum = 0, stepName = "起草" }; return mod; }
            using (DbDataReader reader = Sql.SelReturnReader(strTableName, "Where BackOption=" + docID + " And StepNum=" + StepNum))
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
        /// <summary>
        /// 获取第一步的模型
        /// </summary>
        public M_MisProLevel SelByDocID(int id)
        {
            using (DbDataReader reader = Sql.SelReturnReader(strTableName, "Where BackOption="+id+" And StepNum=1"))
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
        public DataTable Sel()
        {
            return Sql.Sel(strTableName);
        }
        public DataTable Sel(int docid)
        {
            return DBCenter.Sel(strTableName, "BackOption=" + docid);
        }
        public bool UpdateByID(M_MisProLevel model)
        {
            DupUserDeal(model);
            return DBCenter.UpdateByID(model,model.ID);
        }
        public bool Del(int ID)
        {
            return Sql.Del(strTableName, ID);
        }
        public int Insert(M_MisProLevel model)
        {
            DupUserDeal(model);
            return DBCenter.Insert(model);
        }
        //主办不会是协办和辅办
        private void DupUserDeal(M_MisProLevel model) 
        {
            if (!string.IsNullOrEmpty(model.CCUser))
            {
                model.CCUser = StringHelper.RemoveRepeat(model.CCUser.Split(','), model.ReferUser.Split(','));
            }
            if (!string.IsNullOrEmpty(model.HelpUser)) 
            {
                model.HelpUser=StringHelper.RemoveRepeat(model.HelpUser.Split(','), model.ReferUser.Split(','));
                model.HelpUser=StringHelper.RemoveRepeat(model.HelpUser.Split(','), model.CCUser.Split(','));
            }
        }
        //---------步骤判断
        /// <summary>
        /// 流程是否最后一步(公文,限定和自由判断方式不同)
        /// </summary>
        public bool IsLastStep(M_OA_Document oaMod)
        {
            M_MisProLevel stepMod = GetNextStep(oaMod);
            if (stepMod == null) { return true; }
            else { return false; }
        }
        /// <summary>
        /// 仅用于处理AdminFree,用于第一步起草时调用
        /// 返回流程的下一步骤,如果是最后一步,则返回空值,如果无条件符合的步骤(例如步骤序号断裂等,也返回空值)
        /// </summary>
        public M_MisProLevel GetNextStep(M_OA_Document oaMod)
        {
            DataTable dt = GetNextStepDT(oaMod);
            if (dt == null || dt.Rows.Count < 1) { return null; }
            else { return new M_MisProLevel().GetModelFromDR(dt.Rows[0]); }
        }
        /// <summary>
        /// 获取下一步骤,支持分支
        /// </summary>
        public DataTable GetNextStepDT(M_OA_Document oaMod)
        {
            //ParentID>0,直接分叉流程行进,不必考虑返回等,分叉直至归档,条条支线通归档
            B_MisProLevel stepBll = new B_MisProLevel();
            if (oaMod.ProType != (int)M_MisProcedure.ProTypes.AdminFree) { throw new Exception("流程类型错误"); }
            DataTable dt = stepBll.SelByProID(oaMod.ProID);
            //流程下未定义步骤
            if (dt.Rows.Count < 1) { return null; }
            //获取当前步骤模型
            M_MisProLevel curStepMod = SelByDocIDAndStepNum(oaMod.ID, oaMod.CurStepNum);
            //查看其有无子流程，首步不允许子流程
            if (oaMod.CurStepNum > 0)
            {
                M_MisProLevel orginStepMod = stepBll.SelReturnModel(curStepMod.OrginStepID);
                dt.DefaultView.RowFilter = "ParentID=" + orginStepMod.ID + " AND StepNum=1";//这里读的是free中的ID导致,应该读原中的ID
                if (dt.DefaultView.ToTable().Rows.Count > 0)
                {
                    return dt.DefaultView.ToTable();
                }
            }
            //同层级下是否有步骤序号大于其的
            dt.DefaultView.RowFilter = "ParentID=" + curStepMod.ParentID + " AND StepNum=" + (curStepMod.stepNum + 1);
            if (dt.DefaultView.ToTable().Rows.Count > 0)
            {
                
                return dt.DefaultView.ToTable();
            }
            return null;
        }
        //--------
        /// <summary>
        /// 获取指定步骤之前或之后的步骤信息,用于回退和转交等,为防SQL注入,请勿让用户接触第三个参数
        /// </summary>
        public DataTable SelByStep(int proID, int stepNum, string op = "<=")
        {
            SqlParameter[] sp = new SqlParameter[] { new SqlParameter("proID", proID), new SqlParameter("StepNum", stepNum) };
            return SqlHelper.ExecuteTable(CommandType.Text, "Select * From " + strTableName + " Where BackOption=@proID And StepNum" + op + "@StepNum Order By StepNum", sp);
        }
        /// <summary>
        /// 删除指定步骤,主用于回退等时候
        /// </summary>
        /// <param name="docID">公文ID</param>
        /// <param name="stepNum">步骤号</param>
        /// <param name="op">SQL操作符</param>
        public void DelByStep(int docID, int stepNum, string op = ">")
        {
            SqlParameter[] sp = new SqlParameter[] { new SqlParameter("docID", docID), new SqlParameter("StepNum", stepNum) };
            op = op.Replace(" ", "");
            switch (op)
            {
                case ">":
                case "<":
                case "=":
                case ">=":
                case "<=":
                    break;
                default:
                    throw new Exception("运算符不正确[" + op + "]");
            }
            SqlHelper.ExecuteSql("Delete From " + strTableName + " Where BackOption=@docID And StepNum" + op + "@StepNum", sp);
        }
    }
}