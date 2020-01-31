using System;
using System.Collections.Generic;
using System.Text;
using ZoomLa.Model;
using ZoomLa.Components;
using ZoomLa.Common;
using System.Data;
using ZoomLa.SQLDAL;
using System.Data.SqlClient;
using ZoomLa.SQLDAL.SQL;
using System.Data.Common;

namespace ZoomLa.BLL
{
    public class B_GuestBook
    {
        public B_GuestBook()
        {
            strTableName = initMod.TbName;//ZL_GuestCate
            PK = initMod.PK;
        }
        public static string PK, strTableName;
        M_GuestBook initMod = new M_GuestBook();
        public DataTable Sel()
        {
            return DBCenter.Sel(strTableName);
        }
        public static bool DelByIDS(string ids)
        {
            SafeSC.CheckIDSEx(ids);
            string sql = "DELETE FROM ZL_GuestBook WHERE GID IN (" + ids + ")";
            DBCenter.DB.ExecuteNonQuery(new SqlModel(sql, null));
            return true;
        }
        public bool AddTips(M_GuestBook model)
        {
            if (model.GID < 1)
            {
                DBCenter.Insert(model);
            }
            else
            {
                DBCenter.UpdateByID(model, model.GID);
            }
            return true;
        }
        public static bool DelTips(int TID)
        {
            return DBCenter.Del("ZL_GuestBook", "GID", TID);
        }
        public static bool UpdateAudit(string TID, int status)
        {
            SafeSC.CheckIDSEx(TID);
            return DBCenter.UpdateSQL("ZL_GuestBook", "Status=" + status, "GID IN (" + TID + ")");
        }
        public M_GuestBook SelReturnModel(int GID)
        {
            using (DbDataReader reader = DBCenter.SelReturnReader(strTableName, "GID", GID))
            {
                if (reader.Read())
                {
                    return initMod.GetModelFromReader(reader);

                }
                else
                    return new M_GuestBook(true);
            }
        }
        public void UpdateByID(M_GuestBook model)
        {
            DBCenter.UpdateByID(model,model.GID);
        }
        public static DataTable GetTipsAll()
        {
            return SelPage(new F_Guest() { parentId = 0, onlyAudit = true });
        }
        /// <summary>
        /// 某留言及下级帖子总数
        /// </summary>
        public static int GetTipsTotal(int GID)
        {
            return DBCenter.Count("ZL_GuestBook", "(GID=" + GID + " and Status =1) or (ParentID=" + GID + " and Status =1)");
        }
        public static DataTable SelPage(F_Guest filter)
        {
            return SelPage(1, int.MaxValue, filter).dt;
        }
        /// <summary>
        /// 用于留言中心显示留言列表
        /// </summary>
        /// <param name="OnCateStatus"></param>
        /// <param name="userid">指定用户的未审核留言不被筛选</param>
        /// <param name="isShowListByAudit">true:根据留言所属分类的显示模式来筛选</param>
        /// <returns></returns>
        public static PageSetting SelPage(int cpage, int psize, F_Guest filter)
        {
            // int status = -100, int cateid = -100, int pid = -100, string skey = "", int userid = 0, bool isShowListByAudit = false
            string where = " 1=1";
            List<SqlParameter> sp = new List<SqlParameter>();
            if (filter.status != -100) { where += " AND A.Status=" + filter.status; }
            else { where += " AND A.Status<>-1"; }
            if (!string.IsNullOrEmpty(filter.cateIds)) { where += " AND A.CateID IN(" + filter.cateIds + ")"; }
            if (filter.parentId != -100) { where += " AND A.ParentID=" + filter.parentId; }
            //如指定值,则抽取其与其下的回复贴
            if (filter.gid > 0)
            {
                where += "A.Gid=" + filter.gid + " OR (ParentID=" + filter.gid + " AND Status=1)";
            }
            //前台直接输入ID,按ID搜索
            if (DataConvert.CLng(filter.skey) > 0)
            {
                where += " AND A.Gid=" + DataConvert.CLng(filter.skey);
            }
            else if (string.IsNullOrEmpty(filter.skey))
            {
                sp.Add(new SqlParameter("skey", "%" + filter.skey + "%"));
                where += " AND A.Title LIKE @skey";
            }
            //仅显示已审核和自己所发布的贴子,用于留言中心显示留言
            if (filter.onlyAudit)
            {
                where += " AND (A.Status!=0 OR B.IsShowUnaudit=1";
                if (!string.IsNullOrEmpty(filter.uids))
                {
                    SafeSC.CheckIDSEx(filter.uids);
                    where += " OR A.UserID IN (" + filter.uids + ")";
                }
                where += ")";
            }
            PageSetting setting = PageSetting.Double(cpage, psize, strTableName, "ZL_GuestCate", "A.GID", "A.CateID=B.CateID", where, filter.sort, sp);
            setting.fields = "A.*,B.CateName";
            setting.fields += ",(SELECT UserName FROM ZL_User WHERE ZL_User.UserID=A.UserID) UserName";
            setting.fields += ",(SELECT HoneyName FROM ZL_User WHERE ZL_User.UserID=A.UserID) HoneyName";
            setting.fields += ",(SELECT salt FROM ZL_User WHERE ZL_User.UserID=A.UserID) UserFace";
            DBCenter.SelPage(setting);
            return setting;
        }
    }
    public class F_Guest
    {
        public int gid = 0;
        public int parentId = -100;
        public string uids = "";
        public int status = -100;
        /// <summary>
        /// 栏目IDS
        /// </summary>
        public string cateIds = "";
        public string skey = "";
        //是否仅显示已审核的留言
        public bool onlyAudit = false;
        public string sort = "A.GID DESC";
    }
}
