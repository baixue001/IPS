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


    public class B_ServiceSeat : ZoomLa.BLL.ZL_Bll_InterFace<M_ServiceSeat>
    {
        private string strTableName, PK;
        private M_ServiceSeat initMod = new M_ServiceSeat();
        public B_ServiceSeat()
        {
            strTableName = initMod.TbName;
            PK = initMod.PK;
        }
        public DataTable Sel()
        {
            return Sql.Sel(strTableName);
        }
        public DataTable SelForChat()
        {
            string fields = "A.*,A.S_Name AS HoneyName,A.S_AdminID AS UserID,B.UserName";
            return DBCenter.JoinQuery(fields, strTableName, "ZL_User", "S_AdminID=B.UserID", "", "S_Default DESC");
        }
        public PageSetting SelPage(int cpage, int psize)
        {
            PageSetting setting = PageSetting.Single(cpage, psize, strTableName, PK, "");
            DBCenter.SelPage(setting);
            return setting;
        }
        public DataTable Sel(int ID)
        {
            return Sql.Sel(strTableName, PK, ID);
        }
        public M_ServiceSeat SelReturnModel(int ID)
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
        public bool UpdateByID(M_ServiceSeat model)
        {
            return DBCenter.UpdateByID(model, model.S_ID);
        }
        public bool Del(int ID)
        {
            return Sql.Del(strTableName, ID);
        }
        public void DelByIDS(string ids)
        {
            SafeSC.CheckIDSEx(ids);
            string sql = "DELETE FROM " + strTableName + " WHERE S_ID IN (" + ids + ")";
            SqlHelper.ExecuteSql(sql);
        }
        public int Insert(M_ServiceSeat model)
        {
            return DBCenter.Insert(model);
        }
        public M_ServiceSeat GetSelectByaid(int adminid)
        {
            string sqlStr = "SELECT * FROM ZL_ServiceSeat WHERE S_AdminID=" + adminid;
            using (DbDataReader reader = SqlHelper.ExecuteReader(CommandType.Text, sqlStr))
            {
                if (reader.Read())
                {
                    return initMod.GetModelFromReader(reader);
                }
                else
                {
                    return new M_ServiceSeat();
                }
            }
        }
        public void UpdateDefault(int id)
        {
            DBCenter.UpdateSQL(strTableName, "S_Default=0", "S_Default=1");
            DBCenter.UpdateSQL(strTableName, "S_Default=1", PK + "=" + id);
        }
        //-------------------Tools
        public bool IsSeat(int uid)
        {
            return DBCenter.IsExist(strTableName, "S_AdminID=" + uid);
        }
    }
}
