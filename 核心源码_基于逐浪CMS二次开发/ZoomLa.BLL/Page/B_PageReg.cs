using System;
using System.Collections.Generic;
using System.Text;
using ZoomLa.Model.Page;
using System.Data;
using ZoomLa.SQLDAL;
using System.Data.SqlClient;
using ZoomLa.Common;
using ZoomLa.SQLDAL.SQL;
using System.Data.Common;
using ZoomLa.BLL.Content;

namespace ZoomLa.BLL.Page
{
    public class B_PageReg
    {

        public string TbName,PK;
        private M_PageReg initMod = new M_PageReg();
        public B_PageReg()
        {
            TbName = initMod.TbName;
            PK = initMod.PK;
        }
        public int Add(M_PageReg m_PageReg)
        {
            string strSQL = "INSERT INTO [" + TbName + "](" + BLLCommon.GetFields(m_PageReg) + ")VALUES(" + BLLCommon.GetParas(m_PageReg) +")";
            return DataConverter.CLng(SqlHelper.ExecuteScalar(CommandType.Text, strSQL, m_PageReg.GetParameters()));
        }
        public DataTable Sel()
        {
            return Sql.Sel(TbName);
        }
        public M_PageReg SelReturnModel(int ID)
        {
            if (ID == -1)
            {
               return SelSysModel();
            }
            using (DbDataReader reader = Sql.SelReturnReader(TbName, PK, ID))
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
        public M_PageReg SelModelByUid(int uid)
        {
            if (uid == -1) {return SelSysModel(); }
            string where = " WHERE UserID="+uid;
            using (DbDataReader reader = Sql.SelReturnReader(TbName,where))
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
        public M_PageReg SelModelByUName(string uname)
        {
            if (string.IsNullOrEmpty(uname)) {return null; }
            string where = " WHERE UserName=" + uname;
            using (DbDataReader reader = Sql.SelReturnReader(TbName, where))
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
        /// <summary>
        /// 公用栏目PageReg信息
        /// </summary>
        private M_PageReg SelSysModel()
        {
            M_PageReg model = new M_PageReg();
            model.ID = -1;
            model.UserID = -1;
            model.UserName = "公用栏目";
            model.CreationTime = DateTime.Now;
            return model;
        }
        public PageSetting SelPage(int cpage, int psize, int status, string uname)
        {
            string where = " 1=1 ";
            List<SqlParameter> sp = new List<SqlParameter>();
            if (status != -100)
            {
                where += " AND [Status]=" + DataConvert.CLng(status);
            }
            if (!string.IsNullOrEmpty(uname))
            {
                where += " AND UserName LIKE @uname"; sp.Add(new SqlParameter("uname", "%" + uname + "%"));
            }
            PageSetting setting = PageSetting.Double(cpage, psize, TbName, "ZL_PageStyle", PK, "A.NodeStyle=B.PageNodeID", where, "A.ID DESC", sp,"A.*,B.PageNodeName");
            DBCenter.SelPage(setting);
            return setting;
        }
        /// <summary>
        /// 获取用户申请的附加字段信息
        /// </summary>
        public DataRow SelUserApplyInfo(M_PageReg regMod)
        {
            if (string.IsNullOrEmpty(regMod.TableName) || regMod.InfoID < 1) { return null; }
            B_ModelField fieldBll = new B_ModelField();
            DataTable dt = fieldBll.SelectTableName(regMod.TableName, "ID=" + regMod.InfoID);
            return dt.Rows.Count > 0 ? dt.Rows[0] : null;
        }
        public bool DelByIDS(string ids)
        {
            SafeSC.CheckIDSEx(ids);
            DBCenter.DelByIDS(TbName, PK, ids);
            return true;
        }
        public bool Del(int ID)
        {
            return Sql.Del(TbName, ID);
        }
        public bool UpdateByID(M_PageReg model)
        {
            return DBCenter.UpdateByID(model,model.ID);
        }
        public bool UpdateByField(string fieldName, string value, string ids)
        {
            if (string.IsNullOrEmpty(ids)) { return true; }
            SafeSC.CheckDataEx(fieldName);
            SafeSC.CheckIDSEx(ids);
            string sql = "Update " + TbName + " Set " + fieldName + " =@value Where [id] in(" + ids + ")";
            SqlParameter[] sp = new SqlParameter[] { new SqlParameter("value", value) };
            SqlHelper.ExecuteNonQuery(CommandType.Text, sql, sp);
            return true;
        }
        //更新模板
        public bool UpTemplata(int id, string tempstr)
        {
            string strSql = "Update ZL_PageReg set Template=@Temp where ID=" + id;
            List<SqlParameter> sp = new List<SqlParameter>() { new SqlParameter("Temp", tempstr) };
            DBCenter.UpdateSQL(TbName, "Template=@Temp", "ID=" + id, sp);
            return true;
        }
    }
}