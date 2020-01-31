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
   

    public class B_Trademark
    {
        private string TbName, PK;
        private M_Trademark initMod = new M_Trademark();
        public B_Trademark()
        {
            TbName = initMod.TbName;
            PK = initMod.PK;
        }
        public M_Trademark SelReturnModel(int ID)
        {
            if (ID < 1) { return null; }
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
        public DataTable Sel(string skey="")
        {
            string where = " 1=1 ";
            List<SqlParameter> sp = new List<SqlParameter>();
            if (!string.IsNullOrEmpty(skey))
            {
                where += " AND A.TRName LIKE @skey ";
                sp.Add(new SqlParameter("skey","%"+skey+"%"));
            }
            return DBCenter.JoinQuery("A.*,B.ID AS ProID,B.Producername AS ProName", TbName, "ZL_Manufacturers", "A.Producer=B.ID", where, "A.ID DESC",sp.ToArray());
        }
        public PageSetting SelPage(int cpage, int psize)
        {
            PageSetting setting = PageSetting.Single(cpage, psize, TbName, PK, "");
            DBCenter.SelPage(setting);
            return setting;
        }
        public bool UpdateByID(M_Trademark model)
        {
            return DBCenter.UpdateByID(model,model.id);
        }
        public bool Del(int ID)
        {
            return Sql.Del(TbName, ID);
        }
        public int insert(M_Trademark model)
        {
            return DBCenter.Insert(model);
        }
        public bool DeleteByList(string ids)
        {
            SafeSC.CheckIDSEx(ids);
            return DBCenter.DelByIDS(TbName,PK,ids);
        }
        public bool Upotherdata(string str, string id)
        {
            int sid = DataConverter.CLng(id);
            string sqlStr = "";
            SqlParameter[] sp = new SqlParameter[] { new SqlParameter("sid", sid) };
            switch (str)
            {
                case "1":
                    sqlStr = "update ZL_Trademark set Isuse=0 where id=@sid";
                    break;
                case "2":
                    sqlStr = "update ZL_Trademark set Istop=1 where id=@sid";
                    break;
                case "3":
                    sqlStr = "update ZL_Trademark set Isbest=1 where id=@sid";
                    break;
                case "4":
                    sqlStr = "update ZL_Trademark set Isuse=1 where id=@sid";
                    break;
                case "5":
                    sqlStr = "update ZL_Trademark set Istop=0 where id=@sid";
                    break;
                case "6":
                    sqlStr = "update ZL_Trademark set Isbest=0 where id=@sid";
                    break;
                default:
                    sqlStr = "update ZL_Trademark set Producername=Producername where id=@sid";
                    break;
            }
            return SqlHelper.ExecuteSql(sqlStr, sp);
        }
    }
}