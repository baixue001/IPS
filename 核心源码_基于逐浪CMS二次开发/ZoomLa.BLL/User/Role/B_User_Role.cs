using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using ZoomLa.BLL.Helper;
using ZoomLa.SQLDAL;

namespace ZoomLa.BLL.User
{
    public class B_User_Role
    {
        private string TbName = "ZL_User", PK = "UserID";
        /// <summary>
        /// 为角色添加新用户
        /// </summary>
        public void AddToRole(string uids, int roleId)
        {
            if (string.IsNullOrEmpty(uids) || roleId < 1) { return; }
            SafeSC.CheckIDSEx(uids);
            string where = "UserID IN (" + uids + ")";
            DBCenter.UpdateSQL(TbName, "UserRole=UserRole+'," + roleId + ",'", where);
        }
        public void RemoveFromRole(string uids, int roleId)
        {
            if (string.IsNullOrEmpty(uids) || roleId < 1) { return; }
            SafeSC.CheckIDSEx(uids);
            string where = "UserID IN (" + uids + ")";
            DataTable userDT = DBCenter.SelWithField(TbName, "UserID,UserRole", where);
            for (int i = 0; i < userDT.Rows.Count; i++)
            {
                DataRow dr = userDT.Rows[i];
                string roles = StrHelper.RemoveToIDS(DataConvert.CStr(dr["UserRole"]), roleId.ToString());
                DBCenter.UpdateSQL(TbName, "UserRole='" + roles + "'", "UserID=" + dr["UserID"]);
            }
        }

    }
}
