using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using ZoomLa.BLL.Helper;
using ZoomLa.Model.Shop;
using ZoomLa.SQLDAL;
using ZoomLa.SQLDAL.SQL;

namespace ZoomLa.BLL.Shop
{
    public class B_Order_Share:ZL_Bll_InterFace<M_Order_Share>
    {
        public string TbName, TbView = "ZL_Order_ShareView", PK;
        public M_Order_Share initMod = new M_Order_Share();
        public B_Order_Share()
        {
            TbName = initMod.TbName;
            PK = initMod.PK;
        }
        public int Insert(M_Order_Share model)
        {
            return DBCenter.Insert(model);
        }
        public bool Del(int ID)
        {
            return Sql.Del(TbName, ID);
        }
        public bool UpdateByID(M_Order_Share model)
        {
            return DBCenter.UpdateByID(model,model.ID);
        }
        public DataTable Sel()
        {
            return Sql.Sel(TbName, "", "CDATE DESC");
        }
        public PageSetting SelPage(int cpage, int psize, int pid, int proid)
        {
            return SelPage(cpage, psize, new F_Order_Share() { pid = pid, proid = proid });
        }
        public PageSetting SelPage(int cpage, int psize, F_Order_Share filter)
        {
            List<SqlParameter> sp = new List<SqlParameter>() { };
            string where = " 1=1 ";
            where += " AND Pid=" + filter.pid;
            if (filter.pid < 1)//主列表才支持各种筛选条件,子回复只需要检查pid
            {
                if (filter.proid > 0)
                {
                    where += " AND ProID=" + filter.proid;
                }
                if (filter.uid > 0)
                {
                    where += " AND UserID=" + filter.uid;
                }
                if (filter.oid > 0)
                {
                    where += " AND OrderID=" + filter.oid;
                }
                if (filter.storeID > 0)
                {
                    where += " AND ProID IN (SELECT ID FROM ZL_Commodities WHERE UserShopID=" + filter.storeID + ")";
                }
            }
            else //对回复筛选
            {
                if (filter.r_uid > 0)
                {
                    where += " AND UserID="+filter.r_uid;
                }
            }
            PageSetting setting = PageSetting.Single(cpage, psize, TbView, PK, where, "ID DESC", sp);
            DBCenter.SelPage(setting);
            if (filter.pid > 0)
            {
                setting.target = "EGV" + filter.pid;
            }
            return setting;
        }
        /// <summary>
        /// 获取List的主信息
        /// </summary>
        public DataTable SelByProID(int proid)
        {
            string sql = "SELECT * FROM " + TbName + " WHERE ProID=" + proid + " AND Pid=0";
            return SqlHelper.ExecuteTable(sql);
        }
        public M_Order_Share SelReturnModel(int ID)
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
    }
    public class F_Order_Share
    {
        /// <summary>
        /// 筛选指定用户的评论
        /// </summary>
        public int uid = -100;
        /// <summary>
        /// 对回复人筛选
        /// </summary>
        public int r_uid = -100;
        /// <summary>
        /// 对应的商品
        /// </summary>
        public int proid = 0;
        /// <summary>
        /// 所属父评论(pid与proid有一个必须赋值)
        /// </summary>
        public int pid = 0;
        /// <summary>
        /// 限定显示指定订单绑定的评论
        /// </summary>
        public int oid = -100;
        /// <summary>
        /// 按店铺筛选
        /// </summary>
        public int storeID = -100;
    }
}
