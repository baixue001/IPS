using System;
using System.Collections.Generic;
using System.Text;
using ZoomLa.Model;
using System.Data;
using ZoomLa.SQLDAL;
using ZoomLa.Common;
using System.Data.SqlClient;
using ZoomLa.SQLDAL.SQL;
using System.Data.Common;

namespace ZoomLa.BLL
{
    public class B_Mail_BookRead
    {
        public B_Mail_BookRead()
        {
            strTableName = initmod.TbName;
            PK = initmod.PK;
        }
        private string PK, strTableName;
        private M_Mail_BookRead initmod = new M_Mail_BookRead();
        public M_Mail_BookRead GetSelectById(int ID)
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
        public M_Mail_BookRead SelByEMail(string email)
        {
            SqlParameter[] sp = new SqlParameter[] { new SqlParameter("@email", email) };
            using (DbDataReader reader = DBCenter.SelReturnReader(strTableName, "EMail=@email", sp))
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
        public M_Mail_BookRead SelByCode(string code)
        {
            List<SqlParameter> sp = new List<SqlParameter>() { new SqlParameter("@code", code) };
            using (DbDataReader reader = DBCenter.SelReturnReader(strTableName,"AuthCode=@code","", sp))
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
        public DataTable Sel()
        {
            return DBCenter.Sel(strTableName);
        }
        public PageSetting SelPage(int cpage, int psize, int audit = -100)
        {
            string where = " 1=1";
            if (audit != -100) { where += " AND isAudit=" + audit; }
            PageSetting setting = PageSetting.Single(cpage, psize, strTableName, "A." + PK, where, "CDate DESC");
            setting.fields = "A.*,B.UserName";
            setting.t2 = "ZL_User";
            setting.on = "A.UserID=B.UserID";
            DBCenter.SelPage(setting);
            return setting;
        }
        public DataTable SelectAll(int audit = -10)
        {
            string wherestr = "";
            if (audit > -10) { wherestr += " AND isAudit=" + audit; }
            string sql = "SELECT A.*,B.UserName FROM " + strTableName + " A LEFT JOIN ZL_User B ON A.UserID=B.UserID WHERE 1=1 " + wherestr + " ORDER BY CDate DESC";
            return DBCenter.DB.ExecuteTable(new SqlModel(sql,null));
        }
        public bool GetUpdata(M_Mail_BookRead model)
        {
            return DBCenter.UpdateByID(model,model.ID);
        }
        public bool GetDelete(int ID)
        {
            return DBCenter.Del(strTableName,PK,ID);
        }
        public bool DelByIDS(string ids)
        {
            SafeSC.CheckIDSEx(ids);
            return DBCenter.DelByIDS(strTableName,PK,ids);
        }
        public int GetInsert(M_Mail_BookRead model)
        {
            return DBCenter.Insert(model);
        }
        /// <summary>
        /// 更新终止订阅值:0为不中止,1为终止
        /// </summary>
        /// <param name="id"></param>
        /// <param name="IsNotRead"></param>
        /// <returns></returns>
        public bool GetUpdate(int id, int IsNotRead)
        {
            return DBCenter.UpdateSQL("ZL_BookRead", "isNotRead="+ IsNotRead, "ID="+id);
        }
        /// <summary>
        /// 更新审核状态,0为未审核，1为审核
        /// </summary>
        /// <param name="id"></param>
        /// <param name="isAudit"></param>
        /// <returns></returns>
        public bool GetUpdateByid(int id, int isAudit)
        {
            return DBCenter.UpdateSQL("ZL_BookRead", "isAudit=" + isAudit, "ID=" + id);
        }
        public void UpdateStatus(string cmd, string ids)
        {
            if (string.IsNullOrEmpty(ids)) { return; }
            int status = 1;
            switch (cmd)
            {
                case "stop":
                    status = 0;
                    break;
                case "back":
                    status = -1;
                    break;
                case "normal":
                    status = 1;
                    break;
            }
            SafeSC.CheckIDSEx(ids);
            DBCenter.UpdateSQL(strTableName, "isAudit=" + status, PK + " IN(" + ids + ")");
        }
    }
}
