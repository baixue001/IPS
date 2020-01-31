using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using ZoomLa.Model;
using ZoomLa.SQLDAL;
using System.Data.SqlClient;
using ZoomLa.Common;
using ZoomLa.SQLDAL.SQL;
using System.Data.Common;
using ZoomLa.Components;

namespace ZoomLa.BLL
{
    public class B_Search
    {
        private string PK, strTableName;
        private M_Search initMod = new M_Search();
        public B_Search()
        {
            PK = initMod.PK; strTableName = initMod.TbName;
        }
        public M_Search GetSearchById(int ID)
        {
            using (DbDataReader reader = DBCenter.SelReturnReader(strTableName, PK, ID))
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
        public bool UpdateByID(M_Search model)
        {
            return DBCenter.UpdateByID(model, model.Id);
        }
        public bool GetDelete(int ID)
        {
            return DBCenter.Del(strTableName, PK, ID);
        }
        public int insert(M_Search model)
        {
            return DBCenter.Insert(model);
        }
        public DataTable Sel()
        {
            return DBCenter.Sel(strTableName);
        }
        public DataTable SelectAll(string path = "")
        {
            DataTable dt = DBCenter.Sel(strTableName, "State=1");
            if (!string.IsNullOrEmpty(path))
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    dt.Rows[i]["FileUrl"] = dt.Rows[i]["FileUrl"].ToString().Replace("/manage/", path);
                }
            }
            return dt;
        }
        public DataTable SelByUserGroup(int groupid)
        {
            return SelPage(1, int.MaxValue,
                new F_Search() { state = 1, type = 2, groupId = groupid }).dt;
        }
        public DataTable SelByName(string name,int type)
        {
            return SelPage(1, int.MaxValue, new F_Search() { name = name,type=type }).dt;
        }
        public DataTable SelByEliteLevel(int EliteLevel, int Type)
        {
            return SelPage(1, int.MaxValue, new F_Search() { type = Type, elite = EliteLevel }).dt;
        }
        public DataTable SelByType(int Type, string customPath, int state)
        {
            return SelPage(1, int.MaxValue, new F_Search() { type = Type, state = state, path = customPath }).dt;
        }
        /// <summary>
        /// 筛选获取数据
        /// </summary>
        /// <param name="Type">0:后台,1:会员中心</param>
        /// <param name="customPath">后台路径</param>
        /// <param name="state">0:全部,1:启用,2:禁用</param>
        /// <returns></returns>
        public PageSetting SelPage(int cpage, int psize, F_Search filter)
        {
            List<SqlParameter> sp = new List<SqlParameter>();
            string where = "1=1 ";
            if (!string.IsNullOrEmpty(filter.name))
            {
                sp.Add(new SqlParameter("name", "%" + filter.name + "%"));
                where += " AND Name LIKE @name";
            }
            if (filter.elite != -100)
            {
                where += " AND EliteLevel=" + filter.elite;
            }
            if (filter.type != -100)
            {
                where += " AND Type="+filter.type;
            }
            if (filter.groupId != -100)
            {
                where += " AND (UserGroup='' OR  ','+UserGroup+',' LIKE '%,'+'" + filter.groupId + "'+',%' ) ";
            }
            PageSetting setting = PageSetting.Single(cpage, psize, strTableName, PK, where, "OrderID ASC", sp);
            DBCenter.SelPage(setting);
            if (filter.type == 1)
            {
                for (int i = 0; i < setting.dt.Rows.Count; i++)
                {
                    DataRow dr = setting.dt.Rows[i];
                    dr["FileUrl"] = DataConvert.CStr(dr["FileUrl"]).ToLower().Replace("/manage/", "/"+ SiteConfig.SiteOption.ManageDir + "/");
                }
            }
            return setting;
        }



        //查询最大orderid
        public int SelMaxOrder()
        {
            DataTable dt = Sel();
            int maxOrder = Convert.ToInt32(dt.Rows[dt.Rows.Count - 1]["OrderID"]);
            return maxOrder;
        }
        //查询最小orderid
        public int SelMinOrder()
        {
            DataTable dt = Sel();
            int minOrder = Convert.ToInt32(dt.Rows[0]["OrderID"]);
            return minOrder;
        }
        public int GetNextID(int curID)
        {
            int NextID = 0;
            return NextID;
        }
        public bool UpdateStatusByIDS(string ids, int status)
        {
            if (string.IsNullOrEmpty(ids)) { return false; }
            SafeSC.CheckIDSEx(ids);
            return DBCenter.UpdateSQL(strTableName,"State="+ status,"ID IN ("+ids+")");
        }
        public void UpdateOrder(int mid, int oid)
        {
            DBCenter.UpdateSQL(strTableName, "OrderID=" + oid, PK + "=" + mid);
        }
        public bool DelByIDS(string ids)
        {
            SafeSC.CheckIDSEx(ids);
            return DBCenter.DelByIDS(strTableName, PK, ids);
        }
    }
    public class F_Search
    {
        public string name = "";
        //后台|用户中心类型
        public int type = -100;
        //路径
        public string path = "";
        //是否推荐
        public int elite = -100;
        public int state = -100;
        public int groupId = -100;
    }
}