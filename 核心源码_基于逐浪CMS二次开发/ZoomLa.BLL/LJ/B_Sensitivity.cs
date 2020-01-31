using System;
using System.Data;
using ZoomLa.Model;
using ZoomLa.Common;
using ZoomLa.SQLDAL;
using System.Data.SqlClient;
using ZoomLa.SQLDAL.SQL;
using System.Collections.Generic;
using System.Data.Common;

namespace ZoomLa.BLL
{


    public class B_Sensitivity
    {
        private string strTableName = "";
        private string PK = "";
        private M_Sensitivity initMod = new M_Sensitivity();
        public B_Sensitivity()
        {
            strTableName = initMod.TbName;
            PK = initMod.PK;
        }
        public DataTable Sel(int ID)
        {
            return Sql.Sel(strTableName, PK, ID);
        }
        public M_Sensitivity SelReturnModel(int ID)
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
        public DataTable Select_All()
        {
            return Sql.Sel(strTableName);
        }
        public PageSetting SelPage(int cpage, int psize)
        {
            PageSetting setting = PageSetting.Single(cpage, psize, strTableName, PK, "");
            DBCenter.SelPage(setting);
            return setting;
        }
        public bool GetUpdate(M_Sensitivity model)
        {
            return DBCenter.UpdateByID(model,model.id);
        }
        public bool UpdateStatus(int id, int istrue)
        {
            SqlParameter[] sp = new SqlParameter[] { new SqlParameter("id", id), new SqlParameter("istrue", istrue) };
            string sqlStr = "Update " + strTableName + " Set istrue=@istrue Where ID=@id";
            return SqlHelper.ExecuteSql(sqlStr, sp);
        }
        public bool DeleteByGroupID(int ID)
        {
            return Sql.Del(strTableName, ID);
        }
        public int insert(M_Sensitivity model)
        {
            return DBCenter.Insert(model);
        }

        public int GetInsert(M_Sensitivity model)
        {
            return insert(model);
        }
        public static string Process(string content)
        {
            if (!string.IsNullOrEmpty(content))
            {
                DataTable dt = SqlHelper.ExecuteTable(CommandType.Text, "SELECT KeyName FROM ZL_Sensitivity WHERE IsTrue=1");
                foreach (DataRow dr in dt.Rows)
                {
                    string key = DataConvert.CStr(dr["keyname"]);
                    if (string.IsNullOrWhiteSpace(key)) continue;
                    content = content.Replace(key, new string('*', key.Length));
                }
            }
            return content;
        }
        /// <summary>
        /// 过滤敏感词汇
        /// </summary>
        public string ProcessSen(string content)
        {
            return Process(content);
        }
        public M_Sensitivity GetSelect(int ID)
        {
            return SelReturnModel(ID);
        }
        public bool IsExist(string key)
        {
            List<SqlParameter> sp = new List<SqlParameter>() { new SqlParameter("key", key) };
            return DBCenter.IsExist(strTableName, " KeyName=@key", sp);
        }
    }
}