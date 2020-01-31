using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using ZoomLa.Model.Sys;
using ZoomLa.SQLDAL;

namespace ZoomLa.BLL.Sys
{
    public class B_Sys_CSSManage
    {
        private M_Sys_CSSManage  initMod = new M_Sys_CSSManage();
        public string TbName = "", PK = "";
        public B_Sys_CSSManage()
        {
            TbName = initMod.TbName;
            PK = initMod.PK;
        }
        public DataTable Sel()
        {
            return DBCenter.Sel(TbName);
        }
        public DataTable Sel(string skey)
        {
            List<SqlParameter> sp = new List<SqlParameter>();
            string where = "1=1 ";
            if (!string.IsNullOrEmpty(skey)) { where += " AND Name LIKE @skey"; sp.Add(new SqlParameter("skey", "%" + skey + "%")); }
            return DBCenter.Sel(TbName, where,PK+" DESC", sp);
        }
        public M_Sys_CSSManage SelModelByName(string name)
        {
            string where = "name=@name";List<SqlParameter> sp = new List<SqlParameter>() { new SqlParameter("name", name.Trim()) };
            using (DbDataReader reader = DBCenter.SelReturnReader(TbName, where, sp.ToArray()))
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
        public M_Sys_CSSManage SelReturnModel(int ID)
        {
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
        public int Insert(M_Sys_CSSManage model)
        {
            return DBCenter.Insert(model);
        }
        public bool UpdateByID(M_Sys_CSSManage model)
        {
            return DBCenter.UpdateByID(model, model.ID);
        }
        public void Del(string ids)
        {
            if (string.IsNullOrEmpty(ids)) { return; }
            SafeSC.CheckIDSEx(ids);
            DBCenter.DelByIDS(TbName, PK, ids);
        }
        public string GetLabelQuote(int cssid)
        {
            return "{ZL:GetCSS(" + cssid + ",0)/}"; 
        }
        //支持名称或ID
        public static string SelTopCSS(string name)
        {
            if (string.IsNullOrEmpty(name)) { return ""; }
            string css = "";
            string where = "(ZStatus IS NULL OR ZStatus=0) ";
            if (DataConvert.CLng(name) > 0)
            {
                css = DataConvert.CStr(DBCenter.ExecuteScala("ZL_Sys_CSSManage", "CSS", where + " AND id=" + Convert.ToInt32(name) + ""));
            }
            else
            {
                css = DataConvert.CStr(DBCenter.ExecuteScala("ZL_Sys_CSSManage", "CSS", where + " AND name=@name", "",
      new List<SqlParameter>() { new SqlParameter("name", name.Trim()) }));
            }
            return css;
        }
    }
}
