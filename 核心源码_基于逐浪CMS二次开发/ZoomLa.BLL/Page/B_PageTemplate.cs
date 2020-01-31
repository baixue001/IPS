using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using ZoomLa.BLL.Helper;
using ZoomLa.Model;
using ZoomLa.SQLDAL;
using ZoomLa.SQLDAL.SQL;

namespace ZoomLa.BLL.Page
{
    public class B_PageTemplate
    {
        private string TbName, PK;
        private M_Templata initMod = new M_Templata();
        public B_PageTemplate()
        {
            TbName = initMod.TbName;
            PK = initMod.PK;
        }
        public int insert(M_Templata model)
        {
            return DBCenter.Insert(model);
        }
        /// <summary>
        /// 获取栏目信息
        /// </summary>
        /// <param name="uid">获取指定用户的栏目信息</param>
        /// <param name="hassys">是否包含公用栏目</param>
        /// <returns></returns>
        public DataTable Sel(int uid = 0, bool hassys = false)
        {
            string where = "";
            string uids = "";
            if (uid != 0) { uids += uid + ","; }
            if (hassys) { uids += "-1,"; }
            if (!string.IsNullOrEmpty(uids)) { where += " UserID IN (" + StrHelper.PureIDSForDB(uids) + ")"; }
            string fields = "*,TemplateID AS ID,TemplateName AS Name";
            fields += ",(SELECT COUNT(GeneralID) FROM ZL_CommonModel WHERE NodeID=A.TemplateID AND TableName LIKE 'ZL_Page_%') AS ArtCount";
            return DBCenter.SelWithField(TbName, fields, where, "OrderID ASC");
        }
        public M_Templata SelReturnModel(int ID)
        {
            using (DbDataReader reader = Sql.SelReturnReader(TbName, PK, ID))
            {
                if (reader.Read())
                {
                    return initMod.GetModelFromReader(reader);
                }
                else
                {
                    return new M_Templata();
                }
            }
        }
        public bool UpdateByID(M_Templata model)
        {
            return DBCenter.UpdateByID(model, model.TemplateID);
        }
        public bool Del(int ID)
        {
            return Sql.Del(TbName, ID);
        }
        public bool DelByIDS(string ids)
        {
            SafeSC.CheckIDSEx(ids);
            string sql = "DELETE FROM " + TbName + " WHERE TemplateID IN (" + ids + ") AND Userid!=0";
            return SqlHelper.ExecuteSql(sql);
        }
        public PageSetting SelPage(int cpage, int psize, int ParentID, int UserID)
        {
            string where = "1=1 ";
            if (ParentID != -100) { where += " AND ParentID=" + ParentID; }
            if (UserID != 0) { where += " AND UserID=" + UserID; }

            List<SqlParameter> sp = new List<SqlParameter>();
            sp.Add(new SqlParameter("ParentID", ParentID));
            sp.Add(new SqlParameter("UserID", UserID));
            PageSetting setting = PageSetting.Single(cpage, psize, "ZL_PageTemplate", "TemplateID", where, "orderid desc", sp);
            DBCenter.SelPage(setting);
            return setting;
        }
        public DataTable SelByStyleAndPid(int sid, int pid)
        {
            string where = "1=1 ";
            if (sid > 0) { where += " AND UserGroup=" + sid; }
            if (pid != -100) { where += " AND ParentID=" + pid; }
            return DBCenter.SelWithField(TbName, "*,TemplateID AS ID,TemplateName AS Name", where, "OrderID ASC");
        }
    }
}
