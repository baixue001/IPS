using System;
using System.Data;
using ZoomLa.Model;
using ZoomLa.BLL;
using ZoomLa.Common;
using ZoomLa.SQLDAL;
using System.Collections;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data.Common;
using ZoomLa.SQLDAL.SQL;
/// <summary>
/// B_ADZone 的摘要说明
/// </summary>
public class B_ADZone
{
    public string TbName, PK;
    public M_Adzone initMod = new M_Adzone();
    public B_ADZone()
    {
        TbName = initMod.TbName;
        PK = initMod.PK;
    }
    public M_Adzone SelReturnModel(int ID)
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
    private M_Adzone SelReturnModel(string strWhere)
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
    public bool UpdateByID(M_Adzone model)
    {
        return DBCenter.UpdateByID(model,model.ZoneID);
    }
    public bool Del(int ID)
    {
        return Sql.Del(TbName, ID);
    }
    public int Insert(M_Adzone model)
    {
        return DBCenter.Insert(model);
    }
    public string GetJSFileName()
    {
        return DateTime.Now.ToString("yyyyMM/") + B_ADZone.ADZone_MaxID() + ".js";
    }
    //-------------------------------------------------------------
    public DataTable Sel()
    {
        return Sql.Sel(TbName, "", PK + " DESC");
    }
    public DataTable Sel(int zoneType, string skey)
    {
        return Sel(new Com_Filter() { type = zoneType.ToString(), skey = skey });
    }
    public DataTable Sel(Com_Filter filter)
    {
        //int zoneType, string skey
        List<SqlParameter> sp = new List<SqlParameter>();
        string where = "1=1 ";
        if (!string.IsNullOrEmpty(filter.type) && filter.type != "-100") { where += " AND ZoneType =" + Convert.ToInt32(filter.type); }
        if (!string.IsNullOrEmpty(filter.skey)) { where += " AND ZoneName LIKE @skey"; sp.Add(new SqlParameter("skey", "%" + filter.skey + "%")); }
        if (filter.addon == "sale") { where += " AND Sales=1"; }

        return DBCenter.Sel(TbName, where, "ZoneID DESC", sp);
    }
    //-------------------------------------------------------------
    /// <summary>
    /// 添加版位,并生成JS
    /// </summary>
    public static bool ADZone_Add(M_Adzone model)
    {
        model.ZoneID = DBCenter.Insert(model);
        CreateJS(model.ZoneID.ToString());
        return true;
    }
    /// <summary>
    /// 修改版位,并生成JS
    /// </summary>
    public static bool ADZone_Update(M_Adzone model)
    {
        DBCenter.UpdateByID(model, model.ZoneID);
        CreateJS(model.ZoneID.ToString());
        return true;
    }
    /// <summary>
    /// 添加版位信息时,创建对应的JS文件
    /// </summary>
    public static void CreateJS(string ids)
    {
        if (string.IsNullOrEmpty(ids)) { return; }
        B_ADZone zoneBll = new B_ADZone();
        B_ADZoneJs jsBll = new B_ADZoneJs();

        string[] idArr = ids.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
        for (int i = 0; i < idArr.Length; i++)
        {
            M_Adzone zoneMod = zoneBll.SelReturnModel(Convert.ToInt32(idArr[i]));
            DataTable dt = B_Advertisement.GetADList(zoneMod.ZoneID);
            //if (dt.Rows.Count > 0)
            //{
               jsBll.CreateJS(zoneMod, dt);
            //}
        }
    }
    /// <summary>
    /// 激活版位
    /// </summary>
    /// <param name="strZoneId"></param>
    /// <returns></returns>
    public static bool ADZone_Active(int ID)
    {
        return BatchActive(ID.ToString());
    }
    /// <summary>
    /// 批量激活
    /// </summary>
    /// <param name="ZoneIds"></param>
    /// <returns></returns>
    public static bool BatchActive(string ids)
    {
        SafeSC.CheckIDSEx(ids);
        return (SqlHelper.ExecuteNonQuery(CommandType.Text, "Update ZL_AdZone Set Active=1 Where ZoneID in (" + ids + ")") > 0);
    }
    public static bool ADZone_Pause(string ID)
    {
        return BatchPause(ID);
    }
    /// <summary>
    /// 批量暂停
    /// </summary>
    public static bool BatchPause(string ZoneIDs)
    {
        SafeSC.CheckIDSEx(ZoneIDs);
        return (SqlHelper.ExecuteNonQuery(CommandType.Text, "Update ZL_AdZone Set Active=0 Where ZoneID in (" + ZoneIDs + ")") > 0);
    }
    /// <summary>
    /// 获取某个广告所属第一个版位
    /// </summary>
    public static M_Adzone getAdzones(int adverseId)
    {
        M_Adzone model=new M_Adzone();
        string sql = "select * from ZL_AdZone where ZoneID=(select top 1 ZoneID  from  ZL_Zone_Advertisement where ADID="+ adverseId + " order by ZoneID desc)";
        using (DbDataReader drd = SqlHelper.ExecuteReader(CommandType.Text, sql))
        {
            if (drd.Read())
            {
                return model.GetModelFromReader(drd);
            }
            else
            {
                return new M_Adzone(true);
            }
        }
    }
    /// <summary>
    /// 获取最大的版位ID
    /// </summary>
    /// <returns></returns>
    public static int ADZone_MaxID()
    {
        string sql = "select max(zoneid) from ZL_AdZone";
        int i;
        try
        {
            i = DataConverter.CLng(SqlHelper.ExecuteScalar(CommandType.Text, sql, null)) + 1;
        }
        catch
        {
            i = 0;
        }
        return i;
    }
    /// <summary>
    /// 删除版位
    /// </summary>
    /// <param name="strZoneId"></param>
    /// <returns></returns>
    public static bool ADZone_Remove(string ID)
    {
        M_Adzone model = new M_Adzone();
        return Sql.Del(model.TbName, model.PK + "=" +  Convert.ToInt32(ID));
    }
    /// <summary>
    /// 批量删除
    /// </summary>
    /// <param name="ZoneIds"></param>
    /// <returns></returns>
    public static bool BatchRemove(string ZoneIds)
    {
        SafeSC.CheckIDSEx(ZoneIds);
        return (SqlHelper.ExecuteNonQuery(CommandType.Text, "Delete from ZL_AdZone Where ZoneID in (" + ZoneIds + ")") > 0);
    }
    /// <summary>
    /// 清空版位
    /// </summary>
    /// <param name="adzoneid"></param>
    public static bool ADZone_Clear(int adzoneid)
    {
        string sql = "delete from ZL_Zone_Advertisement where ZoneID=@zoneid";
        SqlParameter[] parameter = new SqlParameter[1];
        parameter[0] = new SqlParameter("@zoneid", SqlDbType.Int);
        parameter[0].Value = adzoneid;
        return SqlHelper.ExecuteSql(sql, parameter);
    }
    /// <summary>
    /// 统计某个版位的广告数
    /// </summary>
    /// <param name="strZoneId"></param>
    /// <returns></returns>
    public static int ADZone_Count(int strZoneId)
    {
        SqlParameter[] sp = new SqlParameter[] { new SqlParameter("strZoneId", strZoneId) };
        string str = "select count(*) from ZL_Zone_Advertisement where ZoneID=@strZoneId";
        return DataConvert.CLng(SqlHelper.ExecuteScalar(CommandType.Text, str, sp));
    }
    /// <summary>
    /// 删除某个广告所在的版位
    /// </summary>
    /// <param name="ADID"></param>
    /// <returns></returns>
    public static bool Delete_ADZone_Ad(string ADID)
    {
        DBCenter.DelByWhere("ZL_Zone_Advertisement", "ADID=" + DataConvert.CLng(ADID));
        return true;
    }
}
