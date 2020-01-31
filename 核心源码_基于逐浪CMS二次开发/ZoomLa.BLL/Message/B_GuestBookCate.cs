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
    public class B_GuestBookCate
    {
        private string TbName, PK;
        M_GuestBookCate initMod = new M_GuestBookCate();
        public B_GuestBookCate()
        {
            TbName = initMod.TbName;
            PK = initMod.PK;
        }
        public int Insert(M_GuestBookCate model)
        {
            model.OrderID = GetMaxOrderID() + 1;
            return DBCenter.Insert(model);
        }
        public DataTable Sel() { return DBCenter.Sel(TbName, "", PK + " DESC"); }
        public DataTable Cate_Sel(int gtype)
        {
            return DBCenter.Sel(TbName, "GType=" + gtype, PK + " DESC");
        }
        /// <summary>
        /// 抽取用户留言栏目
        /// </summary>
        public DataTable SelByGuest()
        {
            string where = "GType=0";
            return DBCenter.SelWithField(TbName, "A.*,GCount=(SELECT COUNT(*) FROM ZL_GuestBook WHERE Cateid=A.Cateid AND ParentID=0 AND Status<>-1),GCountAll=(SELECT COUNT(*) FROM ZL_GuestBook WHERE Cateid=A.Cateid AND Status<>-1)", where);
        }
        public DataTable Cate_SelByType(M_GuestBookCate.TypeEnum type, int pid = 0)
        {
            string fields = "A.*,(Select Count(*) From " + TbName + " Where ParentID=A.Cateid)ChildCount";
            string where = "GType=" + (int)type;
            where += "And ParentID=" + pid;
            return DBCenter.SelWithField(TbName, fields, where, "OrderID");
        }
        /// <summary>
        /// 用于BarList.aspx,主题数,回贴总数,最新一条贴子
        /// </summary>
        public DataTable GetCateList(int parentID = 0)
        {
            string statuwhere = "Status!=" + ((int)ZoomLa.Model.ZLEnum.ConStatus.Recycle);//非删除状态
            string fields = "A.CateName,A.CateID,A.BarImage,A.ParentID,";
            fields += "B.ID,B.Title,B.CDate,B.R_CDate,B.Status,";
            fields += "(SELECT COUNT(*) From ZL_Guest_Bar Where CateID=A.Cateid And Pid=0 AND " + statuwhere + ") ItemCount,";//主题
            fields += "(Select COUNT(*) From ZL_Guest_Bar Where CateID=a.Cateid And Pid>0 AND " + statuwhere + ") ReCount";//回贴
            string where = "";
            if (parentID > 0) { where += "A.ParentID=" + parentID; }
            string tbview = "";
            switch (DBCenter.DB.DBType)//分组取第一条数据
            {
                case "oracle":
                    tbview = "(SELECT * FROM (SELECT ROW_NUMBER() OVER(PARTITION BY CateID ORDER BY R_CDate DESC) rn,ZL_Guest_BarView.* FROM ZL_Guest_BarView) WHERE rn = 1)";
                    break;
                default:
                    tbview = "(SELECT * FROM ZL_Guest_BarView AS b where ID = (Select Top 1 ID FROM ZL_Guest_BarView Where Pid=0 And Cateid = b.Cateid AND " + statuwhere + " ORDER BY R_CDate DESC))";
                    break;
            }
            return DBCenter.JoinQuery(fields, TbName, tbview, "A.CateID=B.CateID", where, "B.OrderID");
        }
        public M_GuestBookCate SelReturnModel(int ID)
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
        public bool Update(M_GuestBookCate model)
        {
            if (model.CateID > 0)
                UpdateByID(model);
            else
                Insert(model);
            return true;
        }
        public bool UpdateByID(M_GuestBookCate model)
        {
            return DBCenter.UpdateByID(model, model.CateID);
        }
        public bool Del(int ID)
        {
            return DBCenter.Del(TbName, PK, ID);
        }
        public bool DelByCateIDS(string ids)
        {
            SafeSC.CheckIDSEx(ids);
            return DBCenter.DelByIDS(TbName, PK, ids);
        }
        private int GetMaxOrderID()
        {
            return DataConvert.CLng(DBCenter.ExecuteScala(TbName, "MAX(OrderID)", ""));
        }
        public void SwitchOrderID(string id1, string id2)
        {
            int mid1 = Convert.ToInt16(id1.Split(':')[0]);
            int oid1 = Convert.ToInt16(id1.Split(':')[1]);
            int mid2 = Convert.ToInt16(id2.Split(':')[0]);
            int oid2 = Convert.ToInt16(id2.Split(':')[1]);
            DBCenter.UpdateSQL(TbName, "OrderID=" + oid1, "CateID=" + mid1);
            DBCenter.UpdateSQL(TbName, "OrderID=" + oid2, "CateID=" + mid2);
        }
    }
}
