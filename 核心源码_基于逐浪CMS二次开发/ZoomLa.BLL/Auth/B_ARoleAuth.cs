using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using ZoomLa.BLL.Helper;
using ZoomLa.Common;
using ZoomLa.Model;
using ZoomLa.Model.Auth;
using ZoomLa.SQLDAL;
using ZoomLa.SQLDAL.SQL;

namespace ZoomLa.BLL
{
    public class B_ARoleAuth : ZoomLa.BLL.ZL_Bll_InterFace<M_ARoleAuth>
    {
        private string PK, TbName;
        private M_ARoleAuth initMod = new M_ARoleAuth();
        public B_ARoleAuth() 
        {
            PK = initMod.PK;
            TbName = initMod.TbName;
        }
        /// <summary>
        /// 添加或更新指定元素(如果角色ID已存在)
        /// </summary>
        public int Insert(M_ARoleAuth model)
        {
            if (model.Rid < 1) { throw new Exception("角色ID不正确"); }
            M_ARoleAuth ridMod = SelModelByRid(model.Rid);
            if (ridMod != null)
            {
                model.ID = ridMod.ID;
                UpdateByID(model);
                return model.ID;
            }
            else
            {
                  return DBCenter.Insert(model);
            }
        }
        public bool UpdateByID(M_ARoleAuth model)
        {
            return DBCenter.UpdateByID(model,model.ID);
        }
        public bool Del(int ID)
        {
            return DBCenter.Del(TbName,PK, ID);
        }
        public M_ARoleAuth SelReturnModel(int ID)
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
        public M_ARoleAuth SelModelByRid(int Rid)
        {
            using (DbDataReader reader = DBCenter.SelReturnReader(TbName, " WHERE Rid=" + Rid))
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
            return DBCenter.Sel(TbName);
        }
        public static bool Check(Model.ZLEnum.Auth authfield, string code)
        {
            //M_AdminInfo adminMod = B_Admin.GetLogin();
            //if (adminMod.IsSuperAdmin()) { return true; }
            //return Check(adminMod.RoleList, authfield, code);
            return true;
        }
        /// <summary>
        /// 权限验证
        /// </summary>
        /// <param name="rids">角色IDS</param>
        /// <param name="auth">需要验证的权限大类</param>
        /// <param name="code">具体权限码</param>
        /// <returns>True拥有权限</returns>
        public static bool Check(string rids, Model.ZLEnum.Auth authfield, string code)
        {
            //rids = rids ?? ""; rids = rids.Trim(',');
            //if (string.IsNullOrEmpty(rids)) return false;
            //if (rids.Split(',').Where(p => p.Equals("1")).ToArray().Length > 0) { return true; }
            ////if (rids.Split(',').Equals(1)) { return true; }//超级管理员
            //SafeSC.CheckIDSEx(rids);//稍后必须设定好前后,避免无效验证
            //SqlParameter[] sp = new SqlParameter[] { new SqlParameter("code", "%" + code + "%") };
            //string field = authfield.ToString();
            //DataTable dt = SqlHelper.ExecuteTable(CommandType.Text, "SELECT ID FROM ZL_ARoleAuth WHERE Rid IN(" + rids + ") AND [" + field + "] Like @code", sp);
            //return (dt.Rows.Count > 0);
            return true;
        }
        public static bool CheckEx(Model.ZLEnum.Auth authfield, string code)
        {
            //M_AdminInfo adminMod = B_Admin.GetLogin();
            //if (adminMod.IsSuperAdmin()) { return true; }
            //bool result = Check(adminMod.RoleList, authfield, code);

            //if (!result)
            //{
            //    throw new Exception("你无权进行此项操作");
            //}
            //return result;
            return true;
        }
        public PageSetting SelPage(int cpage, int psize)
        {
            string where = "1=1 ";
            PageSetting setting = PageSetting.Single(cpage, psize, TbName, PK, where);
            DBCenter.SelPage(setting);
            return setting;
        }
        //------------------
        public static bool AuthCheckEx(Model.ZLEnum.Auth model, string auth)
        {
            //M_AdminInfo adminMod = B_Admin.GetLogin();
            //if (adminMod.IsSuperAdmin()) { return true; }
            //string rids = StrHelper.PureIDSForDB(adminMod.RoleList);
            ////非超管且未指定角色
            //if (string.IsNullOrEmpty(rids.Replace(",", ""))) { function.WriteErrMsg("尚未配置管理员角色"); return false; }
            ////权限检测(后期有需求可缓存)
            //bool r = DBCenter.IsExist("ZL_ARoleAuth", "Rid IN (" + rids + ") AND ([" + model.ToString() + "] IS NOT NULL AND [" + model.ToString() + "] LIKE @auth)",
            //    new List<SqlParameter>() { new SqlParameter("auth", "%" + auth + "%") });
            //if (r == false) { function.WriteErrMsg("你无权访问该页面"); }
            //return r;
            return true;
        }
    }
}
