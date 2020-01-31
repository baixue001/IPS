using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using ZoomLa.Model;
using ZoomLa.Model.Shop;
using ZoomLa.SQLDAL;

namespace ZoomLa.BLL.Shop
{
    public class B_Shop_Present
    {
        public string TbName = "ZL_Shop_Present", PK = "ID";
        public void BatInsert(int proID, string json)
        {
            //先清除再重新写入
            DBCenter.DelByWhere(TbName, "ProID=" + proID);
            if (string.IsNullOrEmpty(json) || json.Equals("[]")) { return; }
            DataTable dt = JsonConvert.DeserializeObject<DataTable>(json);
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                M_Shop_Present ptMod = new M_Shop_Present().GetModelFromReader(dt.Rows[i]);
                ptMod.ProID = proID;
                Insert(ptMod);
            }
        }
        //StartNum:赠送条件,P_Price:大于零则为折扣赠送
        public DataTable Sel(int proID)
        {
            DataTable dt = DBCenter.Sel(TbName, "ProID=" + proID);
            return dt;
        }
        public DataTable Sel() { return DBCenter.Sel(TbName); }
        public int Insert(M_Shop_Present model)
        {
            return DBCenter.Insert(model);
        }
        public bool UpdateByID(M_Shop_Present model)
        {
            return DBCenter.UpdateByID(model, model.ID);
        }
        public void Del(string ids)
        {
            if (string.IsNullOrEmpty(ids)) { return; }
            SafeSC.CheckIDSEx(ids);
            DBCenter.DelByIDS(TbName, PK, ids);
        }
        public DataTable WhereLogical(W_Filter filter)
        {
            DataTable dt = Sel(filter.cartMod.ProID);
            dt.Columns.Add("ok", typeof(int));
            //dt.Columns.Add("remind",typeof(string));
            dt.Columns.Add("R_Num",typeof(int));
            dt.Columns.Add("R_Price",typeof(double));
            if (dt.Rows.Count > 0)
            {
                // 检测是否符合条件,并得出最终的赠送方案
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    DataRow dr = dt.Rows[i];
                    dr["ok"] = 0;
                    M_Shop_Present ptMod = new M_Shop_Present().GetModelFromReader(dr);
                    int pronum = filter.cartMod.Pronum;
                    //-------------------------------------
                    switch (ptMod.W_Type)
                    {
                        case 0://买赠
                            {
                                #region 买赠
                                //未满足条件
                                if (ptMod.W_StartNum >pronum || ptMod.P_Num < 1) { }
                                else
                                {
                                    //倍数
                                    int multip = Convert.ToInt32(pronum / ptMod.W_StartNum);
                                    dr["R_Num"] = ptMod.P_Num * multip;
                                    dr["ok"] = 1;
                                }
                                #endregion
                            }
                            break;
                        case 1://折扣
                            {
                                if (ptMod.W_StartNum > pronum) { }
                                else
                                {
                                    //金额统计在主方法中完成
                                    int multip = Convert.ToInt32(pronum / ptMod.W_StartNum);//有几件商品可享受优惠
                                    //计算出折扣金额
                                    double price = DataConvert.CDouble(filter.cartMod.FarePrice);
                                    if (price <= 0) { throw new Exception("折扣金额不正确"); }
                                    double money = (price * multip * ptMod.W_StartNum) - ((price * multip * ptMod.W_StartNum) * (ptMod.P_Price / 100));
                                    dr["ok"] = 1;
                                    dr["R_Price"] = money.ToString("F2");
                                    filter.DiscountMoney += money;
                                }
                            }
                            break;
                        case 2://第二件优惠(后期移除,功能与上方重叠)
                            {
                                ptMod.W_StartNum = 2;
                                if (ptMod.W_StartNum > pronum) { }
                                else
                                {
                                    //金额统计在主方法中完成
                                    int multip = Convert.ToInt32(pronum / ptMod.W_StartNum);//有几件商品可享受优惠
                                    //计算出折扣金额
                                    double price = Convert.ToDouble(filter.cartMod.FarePrice);
                                    double money_orgin = (price * multip * ptMod.W_StartNum);//未优惠过的原价
                                    double money_after = ((price * multip) * (ptMod.P_Price / 100)) + (price * multip);//优惠后的价格

                                    double money = money_orgin - money_after;
                                    dr["ok"] = 1;
                                    dr["R_Price"] = money.ToString("F2");
                                    filter.DiscountMoney += money;
                                }
                            }
                            break;
                    }
                }
                dt.DefaultView.RowFilter = "ok='1'";
                dt = dt.DefaultView.ToTable();
            }
            return dt;
        }
        /// <summary>
        /// 用户支付时,促销操作需要执行的功能
        /// </summary>
        public void WhereLogical_Payed() { }
        /// <summary>
        /// 订单生成时,促销操作需要执行的功能
        /// </summary>
        public void WhereLogical_Ordered() { }
        /// <summary>
        /// 用户退货时,促销操作要执行的功能
        /// </summary>
        public void WhereLogical_Back() { }
        #region 日志记录
        public void Log_BatInsert(int cartid, DataTable dt)
        {

        }
        #endregion
    }
    //用于计算促销条件的逻辑
    public class W_Filter
    {
        public W_Filter() { }
        //购物车初始化
        public W_Filter(DataRow dr)
        {
            cartMod = new M_Cart().GetModelFromReader(dr);
        }
        public M_Cart cartMod = null;
        /// <summary>
        /// 验算过后优惠的金额
        /// </summary>
        public double DiscountMoney = 0;
        /// <summary>
        /// 对促销活动筛选, money:只取与金额有关的活动
        /// </summary>
        public string TypeFilter = "";
    }
}
