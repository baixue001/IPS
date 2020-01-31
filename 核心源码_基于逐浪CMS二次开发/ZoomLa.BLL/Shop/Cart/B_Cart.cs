using System;
using System.Data;
using ZoomLa.Model;
using ZoomLa.Common;
using ZoomLa.SQLDAL;
using System.Data.SqlClient;
using System.Text;
using System.Collections.Generic;
using System.Web;
using ZoomLa.SQLDAL.SQL;
using ZoomLa.BLL.Helper;
using System.Data.Common;
using ZoomLa.Model.Shop;
using Microsoft.AspNetCore.Http;
using ZoomLa.BLL.System.Security;

namespace ZoomLa.BLL
{


    public class B_Cart
    {
        private string TbName, PK;
        private M_Cart initMod = new M_Cart();
        public B_Cart() 
        {
            TbName = initMod.TbName;
            PK = initMod.PK;
        }
        public PageSetting SelPage(int cpage, int psize)
        {
            PageSetting setting = PageSetting.Single(cpage, psize, TbName, PK, "");
            DBCenter.SelPage(setting);
            return setting;
        }
        public M_Cart SelReturnModel(int ID)
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
        /// <summary>
        /// 按商品ID+用户|购物车号,返回是否有购物车记录
        /// </summary>
        public M_Cart SelModelByWhere(int uid, int proID)
        {
            string where = " WHERE ProID=" + proID + " AND UserID=" + uid;
            return SelReturnModel(where);
        }
        private M_Cart SelReturnModel(string strWhere)
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
        public DataTable Sel()
        {
            return Sql.Sel(TbName);
        }
        /// <summary>
        /// 以cartid作为身份标识,修改购物车中商品的数量
        /// </summary>
        public void UpdateProNum(string cartid, int uid, int id, int pronum)
        {
            List<SqlParameter> sp = new List<SqlParameter>() { new SqlParameter("cartid", cartid) };
            string where = "ID=" + id;
            if (uid > 0) { where += " AND (CartID=@cartid OR UserID=" + uid + ")"; }
            else { where += " AND CartID=@cartid"; }
            DBCenter.UpdateSQL(TbName, "Pronum=" + pronum, where, sp);
        }
        public bool UpdateByID(M_Cart model)
        {
            return DBCenter.UpdateByID(model,model.ID);
        }
        public bool Update(M_Cart model)
        {
            return DBCenter.UpdateByID(model,model.ID);
        }
        public int insert(M_Cart model)
        {
            return DBCenter.Insert(model);
        }
        public bool Add(M_Cart model)
        {
            insert(model);
            return true;
        }
        public bool Del(int ID)
        {
            return Sql.Del(TbName, ID);
        }
        public bool DeleteByID(int CartId)
        {
            return Sql.Del(TbName, CartId);
        }
        public bool DelByids(string ids)
        {
            if (SafeSC.CheckIDS(ids))
            {
                string sql = "Delete From " + TbName + " Where ID IN (" + ids + ")";
                SqlHelper.ExecuteNonQuery(CommandType.Text, sql);
                return true;
            }
            return false;
        }
        /// <summary>
        /// 返回购物车商品列表
        /// </summary>
        public DataTable GetCarProList(string cartno, int uid)
        {
            SqlParameter[] sp = new SqlParameter[] { new SqlParameter("cartno", cartno) };
            string cartsql = "Select * From ZL_Cart Where (Cartid=@cartno OR UserID=" + uid + ")";
            DataTable dt = SqlHelper.ExecuteTable(CommandType.Text, cartsql, sp);
            if (dt.Rows.Count < 1) { return null; }
            int cartid = Convert.ToInt32(dt.Rows[0]["ID"]);
            return DBCenter.JoinQuery("A.*,B.ProClass", "ZL_CartPro", "ZL_Commodities", "A.ProID=B.ID", "A.CartID=" + cartid);
        }
        //----------------------------新购物流程
        public DataTable SelByCookID(string cartid, int proclass, string ids = "")
        {
            return SelByCartID(cartid, 0, proclass, ids);
        }
        /// <summary>
        /// 筛选用户商品,并更新购物车表信息(CartID为空,则拒绝筛选)
        /// </summary>
        /// <param name="cartid">PC下为Cookies值,APP中为用户ID,用于简化逻辑</param>
        /// <param name="proclass">类别</param>
        public DataTable SelByCartID(string cartid, int uid, int proClass, string ids = "")
        {
            if (string.IsNullOrEmpty(cartid) && uid < 1) { return null; }
            string fields = " A.*";
            fields += ",B.LinPrice,B.PointVal,B.Thumbnails,B.ProClass,B.ProUnit,B.Allowed,B.Stock,B.FarePrice,B.LinPrice_Json,B.ParentID,B.[Class],B.Procontent";
            string where = "";
            SqlParameter[] sp = new SqlParameter[] { new SqlParameter("cartid", cartid) };
            //if (uid > 0) { where = " (A.Cartid=@cartid OR A.UserID=" + uid + ")"; } else { where = " A.Cartid=@cartid"; }
            if (uid > 0) { where = " A.UserID=" + uid; }
            else { where = " A.CartID=@cartid"; }
            //--------------------------------------
            if (!string.IsNullOrEmpty(ids)) { SafeSC.CheckIDSEx(ids); where += " AND A.ID IN (" + ids + ")"; }
            if (proClass != -100) { where += " AND B.ProClass=" + proClass; }
            string sql = "SELECT " + fields + " FROM ZL_Cart A LEFT JOIN ZL_Commodities B ON A.ProID=B.ID WHERE " + where;
            //自营商品,店铺商品
            DataTable dt = SqlHelper.ExecuteTable(sql, sp);
            return dt;
        }
        /// <summary>
        /// 购物车中有则增加数量，否则添加记录
        /// </summary>
        public int AddModel(M_Cart model)
        {
            if (string.IsNullOrEmpty(model.Cartid) && model.userid < 1) { return 0; }
            List<SqlParameter> sp = new List<SqlParameter>() { new SqlParameter("cartid", model.Cartid) };
            string where = "ProID=" + model.ProID;
            if (model.userid > 0) { where += " AND UserID=" + model.userid; }
            else { where += " AND CartID=@cartid"; }
            DataTable dt = DBCenter.SelWithField("ZL_Cart", "ID,Pronum", where,"",sp);
            if (dt.Rows.Count > 0)
            {
                int id = Convert.ToInt32(dt.Rows[0]["ID"]);
                DBCenter.UpdateSQL(TbName, "Pronum=" + (Convert.ToInt32(dt.Rows[0]["Pronum"]) + model.Pronum), "ID=" + id);
                return id;
            }
            else { return insert(model); }
        }
        public bool DelByIDS(string cartid, string uname, string ids)
        {
            SafeSC.CheckIDSEx(ids);
            List<SqlParameter> sp = new List<SqlParameter>() { new SqlParameter("cartid", cartid), new SqlParameter("uname", uname) };
            string where = " ID IN (" + ids + ") ";
            if (!string.IsNullOrEmpty(uname)) { where += " AND (CartID=@cartid OR UserName=@uname)"; }
            else { where += " AND CartID=@cartid"; }
            return DBCenter.DelByWhere(TbName, where, sp);
        }
        public void UpdateByField(int ftype, string value, string ids)
        {
            SafeSC.CheckIDSEx(ids);
            string field = "";
            SqlParameter[] sp = new SqlParameter[] { new SqlParameter("value", value) };
            switch (ftype)
            {
                default:
                    field = "OrderID";
                    break;
            }
            string sql = "Update " + TbName + " Set " + field + " =@value Where ID IN(" + ids + ")";
            SqlHelper.ExecuteNonQuery(CommandType.Text, sql, sp);
        }
        //------------------------用户操作
        public void U_DelByIDS(string ids, int uid)
        {
            if (string.IsNullOrEmpty(ids) || uid < 1) { return; }
            SafeSC.CheckIDSEx(ids);
            if (string.IsNullOrEmpty(ids)) return;
            string sql = "Delete From " + TbName + " Where ID IN (" + ids + ") And UserID=" + uid;
            SqlHelper.ExecuteNonQuery(CommandType.Text, sql);
        }
        public DataTable U_Sel(int uid)
        {
            string field = "A.*,B.LinPrice,B.Thumbnails,B.ProClass,B.ProUnit";
            string where = "A.UserID=" + uid;
            return DBCenter.JoinQuery(field, TbName, "ZL_Commodities", "A.ProID=B.ID", where);
        }
        public void U_SetNum(int uid, int id, int pronum)
        {
            DBCenter.UpdateSQL(TbName, "ProNum=" + pronum, "UserID=" + uid + " AND ID=" + id);
        }
        //------------------------全局逻辑
        /// <summary>
        /// 进入购物车页面时触发,整理用户购物车中的商品
        /// </summary>
        public static void UpdateUidByCartID(string cartid, M_UserInfo mu, bool isDel = true)
        {
            //用户未登录或CartID为空,则不处理逻辑
            if (string.IsNullOrEmpty(cartid) || mu.IsNull) { return; }
            List<SqlParameter> spList = new List<SqlParameter>() { new SqlParameter("cartid", cartid), new SqlParameter("uname", mu.UserName) };
            //更新掉CartID与UserID,使其在未登录购物车中不可见
            DBCenter.UpdateSQL("ZL_Cart", "UserID=" + mu.UserID + ",CartID='',Username=@uname", "Cartid=@cartid AND (UserID=0 OR UserID IS NULL)", spList);
            if (isDel)//避免购物车提交的同时删除,造成商品丢失Bug
            {
                //按ProID分组取最大值,移除重复与小于其的(JD逻辑)
                string delSql = "DELETE FROM ZL_Cart WHERE UserID=" + mu.UserID + " AND ID NOT IN ({0})";
                string selMax = "SELECT MIN(B.ID) FROM "
                         + "(SELECT ProID,max(Pronum) AS Pronum FROM ZL_Cart WHERE UserID=" + mu.UserID + " GROUP BY ProID)T"
                         + " LEFT JOIN ZL_Cart B ON T.ProID=B.ProID AND T.Pronum=B.Pronum WHERE B.UserID=" + mu.UserID + " GROUP BY B.ProID";
                delSql = string.Format(delSql, selMax);
                SqlHelper.ExecuteSql(delSql);
            }
        }
        public static string GenerateCartID()
        {
            return "CT" + function.GetRandomString(15).ToLower();
        }
        public static string GetCartID(HttpContext ctx)
        {
            string name = "Cart_OrderNo";
            string cartID = CookieHelper.Get(ctx,name);
            try
            {
                if (string.IsNullOrEmpty(cartID))
                {
                    cartID = GenerateCartID();
                    CookieHelper.Set(ctx, name, cartID);
                }
            }
            catch (Exception ex) { ZLLog.L("GetCartID,购物车ID生成出错,原因:" + ex.Message); }
            return cartID;
        }
        public M_Cart NewCart(M_Product proMod)
        {
            M_Cart cartMod = new M_Cart();
            cartMod.Addtime = DateTime.Now;
            cartMod.StoreID = proMod.UserShopID;
            cartMod.FarePrice = proMod.LinPrice.ToString();

            cartMod.Proname = proMod.Proname;
            cartMod.ProID = proMod.ID;
            return cartMod;
        }
    }
}
