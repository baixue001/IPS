using System;
using System.Data;
using ZoomLa.Model;
using ZoomLa.Common;
using ZoomLa.SQLDAL;
using System.Data.SqlClient;
using System.Data.Common;
using ZoomLa.SQLDAL.SQL;

namespace ZoomLa.BLL
{


    public class B_MailIdiograph
    {
        private string TbName, PK;
        private M_MailIdiograph initMod = new M_MailIdiograph();
        public B_MailIdiograph()
        {
            TbName = initMod.TbName;
            PK = initMod.PK;
        }
        public DataTable Sel(int ID)
        {
            return Sql.Sel(TbName, PK, ID);
        }
        public PageSetting SelPage(int cpage, int psize)
        {
            PageSetting setting = PageSetting.Single(cpage, psize, TbName, PK, "");
            DBCenter.SelPage(setting);
            return setting;
        }
        public M_MailIdiograph SelReturnModel(int ID)
        {
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
        private M_MailIdiograph SelReturnModel(string strWhere)
        {
            using (DbDataReader reader = Sql.SelReturnReader(TbName, strWhere))
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
        public DataTable Sel()
        {
            return Sql.Sel(TbName);
        }
        public bool UpdateByID(M_MailIdiograph model)
        {
            return DBCenter.UpdateByID(model,model.ID);
        }
        public bool Del(int ID)
        {
            return Sql.Del(TbName, ID);
        }
        public int insert(M_MailIdiograph model)
        {
            return DBCenter.Insert(model);
        }
        public bool GetInsert(M_MailIdiograph model)
        {
            return DBCenter.Insert(model) > 0;
        }
        public bool GetUpdate(M_MailIdiograph model)
        {
            return DBCenter.UpdateByID(model,model.ID);
        }
        public bool InsertUpdate(M_MailIdiograph model)
        {

            if (model.ID > 0)
                GetUpdate(model);
            else
                GetInsert(model);
            return true;
        }
        public bool GetDelete(int ID)
        {
            return Sql.Del(TbName, PK + "=" + ID);
        }
        public M_MailIdiograph GetSelect(int ID)
        {
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
        public DataTable Select_All(int state=-10)
        {
            string wherestr = "1=1 ";
            if (state > -10)
            {
                wherestr += " AND [State]=" + state;
            }
            return DBCenter.Sel(TbName,wherestr);
        }
        public bool DelByIDS(string ids)
        {
            SafeSC.CheckIDSEx(ids);
            return DBCenter.DelByIDS(TbName,PK,ids);
        }
        public DataTable GetState(string state)
        {
            SqlParameter[] sp = new SqlParameter[] {new SqlParameter("state",state) };
            return SqlHelper.ExecuteTable(CommandType.Text,"SELECT * FROM "+TbName+" WHERE State=@state",sp);
        }
    }
}