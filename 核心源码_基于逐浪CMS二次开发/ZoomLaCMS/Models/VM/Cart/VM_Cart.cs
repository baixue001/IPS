using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;

using ZoomLa.BLL;
using ZoomLa.BLL.Shop;
using ZoomLa.Common;
using ZoomLa.Model;
using ZoomLa.Model.Shop;

namespace ZoomLaCMS.Models.Cart
{
    public class VM_Cart
    {
        B_Product proBll = new B_Product();
        public M_UserInfo mu = null;
        public HttpContext ctx = null;
        public DataTable CartDT = new DataTable();
        public DataTable StoreDT = new DataTable();
        //当前循环的datarow
        public DataRow dr = null;
        //----------------------------------------------
        public VM_Cart(HttpContext ctx, M_UserInfo mu)
        {
            this.mu = mu;
            this.ctx = ctx;
        }
        public string GetShopUrl(DataRow dr)
        {
            return OrderHelper.GetShopUrl(DataConverter.CLng(dr["StoreID"]), Convert.ToInt32(dr["ProID"]));
        }
        public string GetImgUrl(object o)
        {
            return function.GetImgUrl(o);
        }
        public IHtmlContent GetStockStatus(DataRow dr)
        {
            if (DataConverter.CLng(dr["Allowed"]) == 0)
            {
                int pronum = Convert.ToInt32(dr["Pronum"]);
                int stock = Convert.ToInt32(dr["Stock"]);
                if (stock < pronum)
                {
                    return MvcHtmlString.Create("<span class='r_red_x'>缺货</span>");
                }
            }
            return MvcHtmlString.Create("<span class='r_green_x'>有货</span>");
        }
        //筛选店铺下的商品信息
        public DataTable GetCartDTByStore(int storeId)
        {
            CartDT.DefaultView.RowFilter = "StoreID=" + storeId;
            DataTable dt = CartDT.DefaultView.ToTable();
            return dt;
        }
        #region 价格计算
        //将计算出的单价缓存,用于避免重复计算
        private double TempPrice = 0;
        public double TotalPrice = 0;
        //单项商品小计
        public string GetPrice(DataRow dr)
        {
            int pronum = Convert.ToInt32(dr["Pronum"]);
            double total = TempPrice * pronum;
            TotalPrice += total;
            return total.ToString("0.00");
        }
        //单价
        public IHtmlContent GetMyPrice(DataRow dr)
        {
            int proID = Convert.ToInt32(dr["ProID"]);
            double linPrice = TempPrice = Convert.ToDouble(dr["LinPrice"]);
            M_Product proMod = proBll.GetproductByid(proID);
            //多区域价格
            //if (string.IsNullOrEmpty(Region))
            //{
            //    Region = buser.GetRegion(mu.UserID);
            //}
            //TempPrice = regionBll.GetRegionPrice(proID, linPrice, Region, mu.GroupID);
            //如果多区域价格未匹配,则匹配会员价
            if (TempPrice == linPrice || TempPrice <= 0) { TempPrice = proBll.P_GetByUserType(proMod, mu); }
            string html = "<i class='zi zi_yensign'></i><span id='price_" +dr["ID"] + "'>" + TempPrice.ToString("f2") + "</span>";
            //if (orderCom.HasPrice(Eval("LinPrice_Json")))
            //{
            //    string json = DataConvert.CStr(Eval("LinPrice_Json"));
            //    M_LinPrice priceMod = JsonConvert.DeserializeObject<M_LinPrice>(json);
            //    if (priceMod.Purse > 0)
            //    {
            //        html += "余额:<span id='price_purse_" + Eval("ID") + "'>" + priceMod.Purse.ToString("f2") + "</span>";
            //    }
            //    if (priceMod.Sicon > 0)
            //    {
            //        html += "|银币:<span id='price_sicon_" + Eval("ID") + "'>" + priceMod.Sicon.ToString("f0") + "</span>";
            //    }
            //    if (priceMod.Point > 0)
            //    {
            //        html += "|积分:<span id='price_point_" + Eval("ID") + "'>" + priceMod.Point.ToString("f0") + "</span>";
            //    }
            //}
            return MvcHtmlString.Create(html);
        }
        #endregion
    }
}