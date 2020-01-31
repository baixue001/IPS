using System;
using System.Data;
using ZoomLa.Model;
using ZoomLa.Common;
using ZoomLa.SQLDAL;
using System.Data.SqlClient;
using System.Collections.Generic;
using ZoomLa.SQLDAL.SQL;
using System.Data.Common;

namespace ZoomLa.BLL
{
    public class B_MisProLevel
    {
        public string strTableName, PK;
        private M_MisProLevel model = new M_MisProLevel();
        public DataTable dt = null;
        public B_MisProLevel()
        {
            strTableName = model.TbName;
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
        public M_MisProLevel SelByProIDAndStepNum(int proID, int StepNum)
        {
            return SelReturnModel("Where ProID=" + proID + " And StepNum=" + StepNum);
        }
        public M_MisProLevel SelNextModel(M_OA_Document oaMod)
        {
            if (oaMod == null) { return null; }
            //全自由流程的该步为空,依情形特殊处理
            if (oaMod.ProID == 0) { return null; }
            return SelReturnModel("Where ProID=" + oaMod.ProID + " And StepNum=" + (oaMod.CurStepNum + 1));
        }
        private M_MisProLevel SelReturnModel(string strWhere)
        {
            using (DbDataReader reader = Sql.SelReturnReader(strTableName, strWhere))
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
        public DataTable SelByProID(int proID, string skey = "")
        {
            SqlParameter[] sp = new SqlParameter[] { new SqlParameter("skey", "%" + skey + "%") };
            string where = "ProID=" + proID;
            if (!string.IsNullOrEmpty(skey))
            {
                where += " AND StepName LIKE @skey";
            }
            return SqlHelper.JoinQuery("A.*,B.ProcedureName", strTableName, "ZL_MisProcedure", "A.ProID=B.ID", where, "ParentID ASC,StepNum ASC", sp);
        }
        /// <summary>
        /// 获取指定步骤之前或之后的步骤信息,用于回退和转交等,为防SQL注入,请勿让用户接触第三个参数
        /// </summary>
        public DataTable SelByStep(int proID, int stepNum, string op = "<=")
        {
            SqlParameter[] sp = new SqlParameter[] { new SqlParameter("proID", proID), new SqlParameter("StepNum", stepNum) };
            return SqlHelper.ExecuteTable(CommandType.Text, "Select * From " + strTableName + " Where ProID=@proID And StepNum" + op + "@StepNum Order By StepNum", sp);
        }
        public PageSetting SelPage(int cpage, int psize, int proid = -100)
        {
            string where = " 1=1";
            string order = "";
            if (proid != -100) { where += " AND ProID=" + proid; order = "StepNum"; }
            PageSetting setting = PageSetting.Single(cpage, psize, strTableName, "A." + PK, where, order);
            setting.fields = "A.*,B.ProcedureName";
            setting.t2 = "ZL_MisProcedure";
            setting.on = "A.ProID=B.ID";
            DBCenter.SelPage(setting);
            return setting;
        }
        public bool UpdateByID(M_MisProLevel model)
        {
            return DBCenter.UpdateByID(model,model.ID);
        }
        public bool Del(int ID)
        {
            return Sql.Del(strTableName, ID);
        }
        public int insert(M_MisProLevel model)
        {
            return DBCenter.Insert(model);
        }
        //--------------
        public DataTable SelByRefer(int userID)
        {
            B_User userBll = new B_User();
            M_UserInfo userModel = new M_UserInfo();
            userModel = userBll.GetUserByUserID(userID);
            SqlParameter[] sp = new SqlParameter[] { new SqlParameter("UserID", "%," + userID + ",%"), new SqlParameter("GroupID", "%," + userModel.GroupID + ",%") };
            return SqlHelper.ExecuteTable(CommandType.Text, "Select * From " + strTableName + " Where ReferUser Like @UserID OR ReferGroup Like @GroupID", sp);
        }
        public DataTable SelByCC(int userID)
        {
            SqlParameter[] sp = new SqlParameter[] { new SqlParameter("UserID", "%," + userID + ",%") };
            return SqlHelper.ExecuteTable(CommandType.Text, "Select * From " + strTableName + " Where CCUser Like @UserID");
        }
        public DataTable SelByEmail(int userID)
        {
            SqlParameter[] sp = new SqlParameter[] { new SqlParameter("UserID", "%," + userID + ",%") };
            return SqlHelper.ExecuteTable(CommandType.Text, "Select * From " + strTableName + " Where EmailAlert Like @UserID");
        }
        public DataTable SelBySMS(int userID)
        {
            SqlParameter[] sp = new SqlParameter[] { new SqlParameter("UserID", "%," + userID + ",%") };
            return SqlHelper.ExecuteTable(CommandType.Text, "Select * From " + strTableName + " Where SMSAlert Like @UserID");
        }
        /// <summary>
        /// 是否最后一步
        /// </summary>
        public bool IsLastStep(M_MisProLevel currentModel)
        {
            bool flag = false;
            DataTable dt = SelByProID(currentModel.ProID);
            model = model.GetModelFromDR(dt.Rows[(dt.Rows.Count - 1)]);
            if (model.stepNum == currentModel.stepNum)
                flag = true;
            return flag;
        }
        //public bool UpdateStep(int proID)
        //{
        //    bool flag = false;
        //    DataTable dt = SelByProID(proID);
        //    for (int i = 0; i < dt.Rows.Count; i++)
        //    {
        //        model = SelReturnModel(DataConvert.CLng(dt.Rows[i]["ID"]));
        //        model.stepNum = i + 1;
        //        if (UpdateByID(model))
        //            flag = true;
        //        else
        //            flag = false;
        //    }
        //    return flag;
        //}
        //判断权限用户和用户组
    }
}
