using System;
using System.Data;
using ZoomLa.Model;
using ZoomLa.Common;
using ZoomLa.SQLDAL;
using System.Collections;
using System.Text;
using System.IO;
using System.Collections.Generic;
using System.Web;
using System.Data.SqlClient;
using System.Data.Common;
using ZoomLa.BLL.Helper;

namespace ZoomLa.BLL
{

    /// <summary>
    /// B_Advertisement 的摘要说明
    /// </summary>
    public class B_Advertisement
    {
        public static string strTableName = "ZL_Advertisement",PK = "ADID";
        public static string BindTbName = "ZL_Zone_Advertisement";//ZoneID,ADID
        private M_Advertisement initMod = new M_Advertisement();
        public DataTable Sel(int ID)
        {
            return Sql.Sel(strTableName, PK, ID);
        }
        public M_Advertisement SelReturnModel(int ID)
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
        public DataTable Sel()
        {
            return Sql.Sel(strTableName);
        }
        public DataTable Sel(string zoneId, int type)
        {
            string where = "1=1 ";
            if (!string.IsNullOrEmpty(zoneId))
            {
                SafeSC.CheckIDSEx(zoneId);
                where += " AND (ADID IN (Select ADID From ZL_Zone_Advertisement Where ZoneID IN (" + zoneId + ")))";
            }
            if (type != -100)
            {
                where += " AND ADType=" + type;
            }
            return DBCenter.Sel(strTableName, where, "Priority DESC,ADID DESC");
        }
        public bool UpdateByID(M_Advertisement model)
        {
            return DBCenter.UpdateByID(model,model.ADID);
        }
        public int Insert(M_Advertisement model)
        {
            return DBCenter.Insert(model);
        }
        public void Del(string ids)
        {
            if (string.IsNullOrEmpty(ids)) { return; }
            DBCenter.DelByIDS(strTableName, PK, ids);
        }
        public void ChangePassed(string ids, int status)
        {
            if (string.IsNullOrEmpty(ids))
            {
                return;
            }
            DBCenter.UpdateSQL(strTableName, "Passed=" + status, PK + " IN (" + ids + ")");
        }
        /// <summary>
        /// 复制一个广告
        /// </summary>
        public bool Copy(int adid)
        {
            M_Advertisement model = new M_Advertisement();
            model = SelReturnModel(adid);
            model.ADName = "复制" + model.ADName;
            return Insert(model) > 0;
        }
        /// <summary>
        /// 获取广告绑定了哪些版位
        /// </summary>
        public string Zone_GetIDS(int adid)
        {
            DataTable dt = DBCenter.Sel(BindTbName, "ADID=" + adid);
            if (dt.Rows.Count < 1) { return ""; }
            string zoneIds = StrHelper.GetIDSFromDT(dt, "ZoneID");
            return zoneIds;
        }


        /// <summary>
        /// 获取通过审核的某版位的广告列表
        /// </summary>
        public static DataTable GetADList(int zoneId)
        {
            string where = "Passed=1 AND ADID IN (SELECT ADID FROM ZL_Zone_Advertisement WHERE ZoneID=" + zoneId + ")";
            DataTable dt = DBCenter.Sel(strTableName, where, "Priority DESC,ADID DESC");
            return dt;
        }
        /// <summary>
        /// 插入关联信息
        /// </summary>
        public bool Zone_Add(string zoneIds, int ADId)
        {
            if (string.IsNullOrEmpty(zoneIds)) { return false; }
            SafeSC.CheckDataEx(zoneIds);
            string[] zoneIdArr = zoneIds.Split(",".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            foreach (string zoneId in zoneIdArr)
            {
                string strinsert = "INSERT " + BindTbName + " VALUES(" + zoneId + "," + ADId + ")";
                SqlHelper.ExecuteNonQuery(CommandType.Text, strinsert);
            }
            return true;
        }
      
        /// <summary>
        /// 是否存在指定版位ID和广告ID的关联信息
        /// </summary>
        /// <param name="ZoneID"></param>
        /// <param name="ADID"></param>
        /// <returns></returns>
        public static bool IsExistZoneAdv(int ZoneID, int ADID)
        {
            return DBCenter.IsExist(strTableName, "ZoneID="+ZoneID+" and ADID="+ ADID);
        }
        ///// <summary>
        /////根据广告类型，查找广告
        ///// </summary>
        ///// <param name="type"></param>
        ///// <returns></returns>
        //public static DataTable GetAdvByType(int type)
        //{
        //    DataTable dt = new DataTable();
        //    string cmd = @"select * from ZL_Advertisement where ADType=@type order by Priority DESC, ADID desc";
        //    SqlParameter[] parameter = new SqlParameter[] { new SqlParameter("@type", SqlDbType.Int) };
        //    parameter[0].Value = type;
        //    dt = SqlHelper.ExecuteTable(CommandType.Text, cmd, parameter);
        //    return dt;
        //}
        /// <summary>
        /// 是据广告的类型,输出内容
        /// </summary>
        public string GetAdContent(M_Advertisement advMod)
        {
            return "";
        }
    }
}