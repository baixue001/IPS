using System;
using System.Collections.Generic;
using System.Text;
using ZoomLa.Model;
using System.Data;
using System.Data.SqlClient;
using ZoomLa.SQLDAL;
using ZoomLa.Common;
using ZoomLa.SQLDAL.SQL;
using ZoomLa.BLL.Helper;
using System.Data.Common;

namespace ZoomLa.BLL
{
    public class B_Arrive
    {
        private string TbName, PK;
        private M_Arrive initMod = new M_Arrive();
        public B_Arrive()
        {
            TbName = initMod.TbName;
            PK = initMod.PK;
        }
        public M_Arrive SelReturnModel(int ID)
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
        private M_Arrive SelReturnModel(string strWhere, SqlParameter[] sp)
        {
            using (DbDataReader reader = DBCenter.SelReturnReader(TbName, strWhere, sp))
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
        public M_Arrive SelReturnModel(string arriveNo, string arrivePwd)
        {
            SqlParameter[] sp = new SqlParameter[] { new SqlParameter("arriveNo", arriveNo), new SqlParameter("arrivePwd", arrivePwd) };
            using (DbDataReader reader = DBCenter.SelReturnReader(TbName, " Where ArriveNo=@arriveNo And arrivePwd=@arrivePwd", sp))
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
        /// 根据流水号找到该用户未被使用的优惠券
        /// </summary>
        public M_Arrive SelModelByFlow(string flow, int uid)
        {
            if (string.IsNullOrEmpty(flow) || uid < 1) { return null; }
            SqlParameter[] sp = new SqlParameter[] { new SqlParameter("flow", flow) };
            using (DbDataReader reader = DBCenter.SelReturnReader(TbName, " Where flow=@flow AND UserID=" + uid + " AND State=1", sp))
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
        public DataTable Sel(int type, int state, string flow, string name, string stime, string etime, string username = "", string addon = "")
        {
            string where = " 1=1 ";
            List<SqlParameter> sp = new List<SqlParameter>();
            if (type != -100) { where += " AND A.type=" + type; }
            if (state != -100) { where += " AND A.state=" + state; }
            if (!string.IsNullOrEmpty(flow)) { where += " AND A.flow=@flow"; sp.Add(new SqlParameter("flow", flow)); }
            if (!string.IsNullOrEmpty(name)) { where += " AND A.ArriveName LIKE @name"; sp.Add(new SqlParameter("name", "%" + name + "%")); }
            if (!string.IsNullOrEmpty(stime)) { where += " AND A.AgainTime>=@stime"; sp.Add(new SqlParameter("stime", stime)); }
            if (!string.IsNullOrEmpty(etime)) { where += " AND A.EndTime<=@etime"; sp.Add(new SqlParameter("etime", etime)); }
            if (!string.IsNullOrEmpty(username)) { where += " AND B.UserName LIKE @username"; sp.Add(new SqlParameter("username", "%" + username + "%")); }
            switch (addon.ToLower())
            {
                case "isbind":
                    where += " AND A.UserID>0";
                    break;
                case "nobind":
                    where += " AND A.UserID=0";
                    break;
                case "expired":
                    where += " AND A.EndTime < GETDATE()";
                    break;
            }
            return DBCenter.JoinQuery("A.*,B.UserName", TbName, "ZL_User", "A.UserID=B.UserID", where, PK + " DESC", sp.ToArray());
        }
        public bool GetUpdate(M_Arrive model)
        {
            return DBCenter.UpdateByID(model,model.ID);
        }
        public bool DelByIDS(string ids, int storeID = -100)
        {
            if (string.IsNullOrEmpty(ids)) { return false; }
            SafeSC.CheckIDSEx(ids);
            string where = PK + " IN (" + ids + ")";
            if (storeID != -100) { where += " AND StoreID=" + storeID; }
            return DBCenter.DelByWhere(TbName, where);
        }
        public int GetInsert(M_Arrive model)
        {
            return DBCenter.Insert(model);
        }
        /// <summary>
        /// 通过优惠券IDS来更改用户ID，已使用的优惠券不更改
        /// </summary>
        /// <returns></returns>
        public bool GetUpdateUserIdByIDS(string ids, int userId, int storeID = -100)
        {
            if (string.IsNullOrEmpty(ids)) { return false; }
            SafeSC.CheckIDSEx(ids);
            string where = "State!=10 AND ID IN (" + ids + ")";
            if (storeID != -100) { where += " AND StoreID=" + storeID; }
            return DBCenter.UpdateSQL(TbName, "UserID=" + userId, where);
        }
        /// <summary>
        /// 批量激活尚未激活的优惠券
        /// </summary>
        public void ActiveByIDS(string ids)
        {
            SafeSC.CheckIDSEx(ids);
            DBCenter.UpdateSQL(TbName, "State=1", "State=0 AND ID IN (" + ids + ")");
        }
        //-----新购物流程
        public DataTable U_Sel(int uid, int type, int state)
        {
            if (uid < 1) { return null; }
            string where = "1=1";
            where += " AND UserID=" + uid;
            if (type != -100) { where += " AND type=" + type; }
            if (state != -100) { where += " AND state=" + state; }
            return DBCenter.Sel(TbName, where, PK + " DESC");
        }
        /// <summary>
        /// 筛选出,用户尚未领取过的优惠券
        /// </summary>
        public PageSetting U_SelForGet(int cpage,int psize,int uid,int sid)
        {
            string where = "";
            //1,用户未领取过的券并且在有效期内
            where = "SELECT Flow,(SELECT COUNT(ID) FROM ZL_Arrive WHERE Flow=A.Flow AND UserID=0)LCount FROM ZL_Arrive A WHERE EndTime>'" + DateTime.Now + "' AND Flow NOT IN (SELECT Flow FROM ZL_Arrive WHERE UserID=" + uid + " GROUP BY Flow)GROUP BY Flow";
            //2,券还有剩余
            where = "SELECT Flow FROM (" + where + ")A WHERE LCount>0";
            where = "SELECT MIN(ID) FROM ZL_Arrive WHERE Flow IN (" + where + ") GROUP BY Flow";
            where = "ID IN (" + where + ")";
            if (sid != -100) { where += " AND StoreID=" + sid; }
            PageSetting setting = PageSetting.Single(cpage, psize, TbName, PK, where, PK + " DESC");
            DBCenter.SelPage(setting);
            return setting;
        }
        /// <summary>
        /// 根据flow领取对应的优惠券
        /// (暂不限定用户领取数量,但领取页面会筛除掉)
        /// </summary>
        public int U_GetArrive(int uid, string flow)
        {
            List<SqlParameter> sp = new List<SqlParameter>();
            sp.Add(new SqlParameter("@flow", flow));
            int arrid = DataConvert.CLng(DBCenter.ExecuteScala(TbName, "ID", "Flow=@flow AND UserID=0", "", sp));
            DBCenter.UpdateSQL(TbName, "UserID=" + uid, "ID=" + arrid);
            return arrid;
        }
        /// <summary>
        /// 修改优惠卷的状态(已使用过的不可变更)
        /// </summary>
        public void UpdateState(string ids, int state, int storeID = -100)
        {
            if (string.IsNullOrEmpty(ids)) { return; }
            SafeSC.CheckIDSEx(ids);
            string where = "ID IN (" + ids + ") AND State!=10 ";
            if (storeID != -100) { where += " AND StoreID =" + storeID; }
            DBCenter.UpdateSQL(TbName, "State=" + state, where);
        }
        //-----Logical
        public string GetMoneyRegion(double min, double max)
        {
            //double min = DataConvert.CDouble(Eval("MinAmount"));
            //double max = DataConvert.CDouble(Eval("MaxAmount"));
            if (min == 0 && max == 0) { return "无使用门槛"; }
            if (max == 0 && min > 0) { return "满" + min.ToString("f0") + "元使用"; }
            if (max > 0 && min == 0) { return max + "元以下使用"; }
            if (max > 0 && min > 0) { return min + "-" + max + "元使用"; }
            return "<span style='color:red;'>使用条件错误</span>";
        }
        public string GetTypeStr(DataRow dr)
        {
            string result = GetTypeStr(DataConvert.CLng(dr["Type"]));
            if (DataConvert.CLng(dr["StoreID"]) > 0)
            {
                //string title =DataConvert.CStr(DBCenter.ExecuteScala("ZL_CommonModel", "Title", "GeneralID=" + dr["StoreID"]));
                //result = title + result;
                return "店铺优惠卷";
            }
            return result;
        }
        public string GetTypeStr(int type)
        {
            string result = "";
            switch (type)
            {
                case 0:
                    result = "优惠卷";
                    break;
                case 1:
                    result = "现金卡";
                    break;
                default:
                    result = type.ToString();
                    break;
            }
            return result;
        }
        public string GetStateStr(int state)
        {
            string result = "";
            switch (state)
            {
                case 0:
                    result = "<span>未激活</span>";
                    break;
                case 1:
                    result = "<span style='color:#61b0e9;'>已激活</span>";
                    break;
                case 10:
                    result = "<span style='color:red;'>已使用</span>";
                    break;
                default:
                    break;
            }
            return result;
        }
        /// <summary>
        /// 检测优惠券是否可用,用于订单生成页
        /// </summary>
        /// <param name="flow"></param>
        /// <param name="cartdt">购买的商品列表</param>
        /// <param name="money">商品累计金额(cartdt)</param>
        /// <returns>优惠后的金额,不能使用的原因,是否可使用</returns>
        public M_Arrive_Result U_CheckArrive(M_Arrive model, int uid, DataTable cartdt, double money)
        {
            M_Arrive_Result retMod = new M_Arrive_Result();
            retMod.flow = model.Flow;
            retMod.money = money;
            if (model == null) { retMod.err = "指定的优惠券不存在"; }
            else if (model.UserID != uid) { retMod.err = "优惠券与用户不匹配"; }
            else if (uid < 1) { retMod.err = "用户信息不正确"; }
            else if (model.State == 10) { retMod.err = "优惠券已被使用"; }
            else if (model.State == 0) { retMod.err = "优惠券尚未激活"; }
            else if (model.Amount < 1) { retMod.err = "优惠券金额异常[" + model.Amount + "]"; }
            else if (model.EndTime < DateTime.Now) { retMod.err = "优惠券已过期"; }
            else if (model.AgainTime > DateTime.Now) { retMod.err = "优惠券尚未到可使用时段"; }
            if (!string.IsNullOrEmpty(retMod.err)) { return retMod; }
            model.MaxAmount = model.MaxAmount == 0 ? 5000000 : model.MaxAmount;
            if (model.MinAmount > money) { retMod.err = "未达到最小金额使用限制";return retMod; }
            else if (model.MaxAmount < money) { retMod.err = "超过最大金额使用限制"; return retMod; }
            //检测是否限定了店铺(商品必须全部属于同一店铺)
            if (model.StoreID != -100)
            {
                if (cartdt.Select("StoreID NOT IN ('" + model.StoreID + "')").Length > 0)
                {
                    retMod.err = "优惠卷不可用于该店铺";
                    return retMod;
                }
            }
            //------------------------------------------------------------------------
            #region 如指定了商品,则需要进行商品检测,优惠金额也只限于指定的商品,不可超过上限,直接返回
            if (!string.IsNullOrEmpty(model.ProIDS))
            {
                cartdt.DefaultView.RowFilter = "ProID IN (" + StrHelper.PureIDSForDB(model.ProIDS) + ")";
                DataTable prodt = cartdt.DefaultView.ToTable();
                if (prodt.Rows.Count < 1) { retMod.err = "没有符合该优惠卷的商品"; return retMod; }
                double promoney = DataConvert.CDouble(prodt.Compute("SUM(AllMoney)", ""));
                if (model.MinAmount > promoney) { retMod.err = "指定商品未达到最小金额使用限制"; }
                else if (model.MaxAmount < promoney) { retMod.err = "指定商品超过最大金额使用限制"; }
                else
                {
                    //优惠的金额不可超过指定商品的上限,即以小的为准
                    double amount = model.Amount <= promoney ? model.Amount : promoney;
                    money = money - amount;
                    money = money < 0 ? 0 : money;
                    retMod.money = money;
                    retMod.amount = amount;
                    retMod.enabled = true;
                    return retMod;
                }
            }
            #endregion
            else //正常使用优惠卷
            {
                money = money - model.Amount;
                money = money < 0 ? 0 : money;
                retMod.money = money;
                retMod.amount = model.Amount;
                retMod.enabled = true;
                return retMod;
            }
            return retMod;
        }
        /// <summary>
        /// 使用目标优惠券,并写入日志
        /// </summary>
        /// <param name="model">优惠券模型</param>
        /// <param name="uid">需要使用该优惠券的用户ID</param>
        /// <param name="money">订单的金额,优惠完成后该值会被修改</param>
        /// <param name="err">优惠券错误原因</param>
        /// <param name="remind">优惠券使用备注</param>
        /// <returns>true使用成功,false则查看err</returns>
        public M_Arrive_Result U_UseArrive(M_Arrive model, int uid, DataTable cartdt, double money, string remind)
        {
            M_Arrive_Result retMod = U_CheckArrive(model, uid, cartdt, money);
            if (retMod.enabled)
            {
                List<SqlParameter> sp = new List<SqlParameter>() { new SqlParameter("remind", remind), new SqlParameter("usetime", DateTime.Now) };
                DBCenter.UpdateSQL("ZL_Arrive", "State=10,UseRemind=@remind,UseTime=@usetime", "ID=" + model.ID, sp);
            }
            return retMod;
        }
        /// <summary>
        /// 制作优惠卷
        /// </summary>
        /// <param name="noType">优惠卷编码类型</param>
        /// <param name="uids">默认分配给哪些用户</param>
        public CommonReturn CreateArrive(M_Arrive avMod, int num, string noType="1", string uids = "")
        {
            if (num < 1) { return CommonReturn.Failed("未指定生成数量"); }
            if (avMod.EndTime <= DateTime.Now) { return CommonReturn.Failed("到期时间不能早于当前时间"); }
            if (avMod.AgainTime >= avMod.EndTime) { return CommonReturn.Failed("到期时间不能晚于发布时间"); }
            if (avMod.MaxAmount < 0 || avMod.MinAmount < 0) { return CommonReturn.Failed("使用范围数值不正确"); }
            if (avMod.MaxAmount != 0 && avMod.MinAmount > avMod.MaxAmount) { return CommonReturn.Failed("使用范围不正确,最小值不能大于最大值"); }
            if (avMod.Amount < 1) { return CommonReturn.Failed("优惠金额不正确,最小值为1"); }
            //----------------------------------------
            if (string.IsNullOrEmpty(avMod.Flow)) {avMod.Flow = Guid.NewGuid().ToString(); }
            int[] uidArr = H_GetUserArr(uids);
            for (int i = 0; i < num; i++)
            {
                avMod.ArriveNO = H_GetArriveNo(noType);
                avMod.ArrivePwd = "ZL" + function.GetRandomString(9);
                avMod.UserID = uidArr.Length > i ? uidArr[i] : 0;
                GetInsert(avMod);
            }
            return CommonReturn.Success();
        }
        private string H_GetArriveNo(string type)
        {
            string arriveNO = "";
            switch (type)
            {
                case "0"://纯数字
                    arriveNO = function.GetRandomString(9, 2);
                    break;
                case "1"://字母
                    arriveNO = "ZL" + function.GetRandomString(9, 3).ToLower();
                    break;
                case "2"://混淆
                default:
                    arriveNO = "ZL" + function.GetRandomString(9, 3).ToLower();
                    break;
            }
            return arriveNO;
        }
        private int[] H_GetUserArr(string uids)
        {
            List<int> uidList = new List<int>();
            string[] uidArr = uids.Split(',');
            foreach (string uid in uidArr)
            {
                if (DataConverter.CLng(uid) > 0) { uidList.Add(Convert.ToInt32(uid)); }
            }
            return uidList.ToArray();
        }
        ////-----
        public PageSetting SelPage(int cpage, int psize, int uid, int type, int state, string addon = "")
        {
            return SelPage(cpage, psize, new Filter_Arrive()
            {
                uid = uid,
                type = type,
                state = state,
                addon = addon
            });
        }
        public PageSetting SelPage(int cpage, int psize, Filter_Arrive filter)
        {
            string where = "1=1";
            if (filter.uid > 0) { where += " AND UserID=" + filter.uid; }
            if (filter.type != -100) { where += " AND type=" + filter.type; }
            if (filter.state != -100) { where += " AND state=" + filter.state; }
            if (filter.storeID != -100) { where += " AND StoreID="+filter.storeID; }
            if (!string.IsNullOrEmpty(filter.StoreIDS))
            {
                SafeSC.CheckIDSEx(filter.StoreIDS);
                where += " AND StoreID IN (" + filter.StoreIDS + ")";
            }
            switch (filter.addon.ToLower())
            {
                case "expired":
                    where += " AND EndTime <= GETDATE()";
                    break;
                case "noexp":
                    where += " AND EndTime > GETDATE()";
                    break;
                default:
                    break;
            }
            //string mtbname = "(SELECT A.*,B.Title AS StoreName FROM ZL_Arrive A LEFT JOIN ZL_CommonModel B ON A.StoreID=B.GeneralID)";
            string fields = "*,(SELECT UserName FROM ZL_User WHERE UserID=A.UserID) AS UserName";
            fields += ",(SELECT Title FROM ZL_CommonModel WHERE GeneralID=A.StoreID) AS StoreName";
            PageSetting setting = PageSetting.Single(cpage, psize, TbName, PK, where, "ID DESC", null, fields);
            DBCenter.SelPage(setting);
            return setting;
        }
    }
    public class Filter_Arrive
    {
        /// <summary>
        /// 优惠卷所属店铺
        /// </summary>
        public int storeID = -100;
        public int uid = -100;
        public int type = -100;
        public int state = -100;
        public string addon = "";
        public string StoreIDS = "";
        public string skey = "";
    }
}
