using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using ZoomLa.Model.Other;
using ZoomLa.SQLDAL;
using ZoomLa.SQLDAL.SQL;

namespace ZoomLa.BLL.Other
{
    public class B_WX_LuckDrawList
    {
        private M_WX_LuckDrawList initMod = new M_WX_LuckDrawList();
        public string TbName = "", PK = "";
        public B_WX_LuckDrawList()
        {
            TbName = initMod.TbName;
            PK = initMod.PK;
        }
        public DataTable Sel()
        {
            return DBCenter.Sel(TbName);
        }
        public DataTable Sel(int luckid,string uname,string prizeName)
        {
            string where = "A.LuckID=" + luckid;
            List<SqlParameter> sp = new List<SqlParameter>();
            if (!string.IsNullOrEmpty(uname))
            {
                where += " AND (A.UserName LIKE @uname OR B.Permissions LIKE @uname)";
                sp.Add(new SqlParameter("uname", "%" + uname + "%"));
            }
            if (!string.IsNullOrEmpty(prizeName))
            {
                where += " AND A.Prize LIKE @prize";
                sp.Add(new SqlParameter("prize", "%" + prizeName + "%"));
            }
            //return DBCenter.Sel(TbName, where, "ID DESC");
            return DBCenter.JoinQuery("A.*,B.HoneyName,B.Permissions",TbName,"ZL_User","A.UserID=B.UserID",where,"A.ID DESC",sp.ToArray());
        }
        public M_WX_LuckDrawList SelReturnModel(int ID)
        {
            if (ID < 1) { return null; }
            using (DbDataReader reader = DBCenter.SelReturnReader(TbName, PK, ID))
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
        public int Insert(M_WX_LuckDrawList model)
        {
            return DBCenter.Insert(model);
        }
        public bool UpdateByID(M_WX_LuckDrawList model)
        {
            return DBCenter.UpdateByID(model, model.ID);
        }
        public void Del(string ids)
        {
            if (string.IsNullOrEmpty(ids)) { return; }
            SafeSC.CheckIDSEx(ids);
            DBCenter.DelByIDS(TbName, PK, ids);
        }
        public PageSetting SelPage(int cpage, int psize)
        {
            string where = "1=1 ";
            List<SqlParameter> sp = new List<SqlParameter>();
            PageSetting setting = PageSetting.Single(cpage, psize, TbName, PK, where, PK + " DESC", sp);
            DBCenter.SelPage(setting);
            return setting;
        }
        //--------------------------
        public bool Exist(int luckid, int uid)
        {
            if (luckid < 1 || uid < 1) { return true; }
            return DBCenter.IsExist(TbName, "UserID=" + uid + " AND LuckID=" + luckid);
        }
        /// <summary>
        /// 已抽奖了几次
        /// </summary>
        public int ExistCount(int luckid, int uid)
        {
            return DataConvert.CLng(DBCenter.ExecuteScala(TbName, "COUNT(ID)", "UserID=" + uid + " AND LuckID=" + luckid));
        }
        /// <summary>
        /// 是否参加该活动并获奖
        /// </summary>
        /// <returns>true:已获取过奖品</returns>
        public bool ExistWithPrize(int luckid,int uid)
        {
            object o=DBCenter.ExecuteScala(TbName,"COUNT(ID)","LuckID="+luckid+" AND UserID="+uid+ " AND (Prize!='未中奖' AND Prize IS NOT NULL)");
            return DataConvert.CLng(o) > 0 ? true : false;
        }
        /// <summary>
        /// 已有多少人参加了活动
        /// </summary>
        /// <param name="luckid"></param>
        /// <returns></returns>
        public int Count(int luckid)
        {
            return DataConvert.CLng(
                 DBCenter.ExecuteTable("SELECT COUNT(USERID) FROM (SELECT UserID FROM ZL_WX_LuckDrawList GROUP BY UserID) AS A").Rows[0][0]
                );
        }
    }
}
