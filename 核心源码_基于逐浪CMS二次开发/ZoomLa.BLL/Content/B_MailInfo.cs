using System;
using System.Data;
using ZoomLa.Model;
using ZoomLa.Common;
using ZoomLa.SQLDAL;
using System.Data.SqlClient;
using ZoomLa.SQLDAL.SQL;
using System.Data.Common;

namespace ZoomLa.BLL
{

    public class B_MailInfo
    {
        public B_MailInfo()
        {
            PK = initmod.PK;
            strTableName = initmod.TbName;
        }
        private string PK, strTableName;
        private M_MailInfo initmod = new M_MailInfo();
        public DataTable Sel(int ID)
        {
            return Sql.Sel(strTableName, PK, ID);
        }
        public DataTable SelByStatus(int status)
        {
            return DBCenter.Sel(strTableName, "MailStatus=" + status, "MailAddTime desc");
        }
        public DataTable SelByUid(int uid)
        {
            return DBCenter.Sel(strTableName, "UserID=" + uid, PK + " DESC");
        }
        public PageSetting SelPage(int cpage, int psize, int status = -100, int uid = 0)
        {
            string where = " 1=1";
            if (status != -100) { where += " AND MailStatus=" + status; }
            if (uid > 0) { where += " AND UserID=" + uid; }
            PageSetting setting = PageSetting.Single(cpage, psize, strTableName, PK, where);
            DBCenter.SelPage(setting);
            return setting;
        }
        private M_MailInfo SelReturnModel(int ID)
        {
            using (DbDataReader reader = Sql.SelReturnReader(strTableName, PK, ID))
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
            return Sql.Sel(strTableName);
        }
        public bool UpdateByID(M_MailInfo model)
        {
            return DBCenter.UpdateByID(model,model.ID);
        }
        public bool Del(int ID)
        {
            return Sql.Del(strTableName, ID);
        }
        public int insert(M_MailInfo model)
        {
            return DBCenter.Insert(model);
        }
        public bool GetInsert(M_MailInfo model)
        {
            return insert(model) > 0;
        }
        public bool GetUpdate(M_MailInfo model)
        {
            return UpdateByID(model);
        }
        public bool GetDelete(int ID)
        {
            return Del(ID);
        }
        public M_MailInfo GetSelect(int ID)
        {
            return SelReturnModel(ID);
        }
        public DataTable Select_All()
        {
            return Sel();
        }
    }
}