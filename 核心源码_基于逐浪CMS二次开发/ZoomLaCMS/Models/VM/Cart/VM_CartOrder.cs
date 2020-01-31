using Microsoft.AspNetCore.Html;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;

using ZoomLa.BLL.Shop;
using ZoomLa.Common;
using ZoomLa.Components;
using ZoomLa.Model;
using ZoomLa.Model.Shop;
using ZoomLa.SQLDAL;

namespace ZoomLaCMS.Models.Cart
{
    public class VM_CartOrder
    {
        private B_Shop_FareTlp fareBll = new B_Shop_FareTlp();
        public bool ShowAddress = true;
        public bool ShowPassenger = true;
        public int ProClass = 0;
        public int usepoint = 0;
        public M_UserInfo mu = null;
        public DataTable StoreDT = null;
        public DataTable CartDT = null;
        public List<M_Cart_Contract> Guest = new List<M_Cart_Contract>();
        //总计金额,用于优惠券计算
        public double allmoney = 0;
        public DataRow dr = null;
        //-------------------------------------------
        public string GetShopUrl(DataRow dr)
        {
            return OrderHelper.GetShopUrl(DataConverter.CLng(dr["StoreID"]), Convert.ToInt32(dr["ProID"]));
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
        //已折扣的金额
        public string GetDisCount(DataRow dr)
        {
            //return "- " + (Convert.ToDouble(Eval("AllIntegral")) - Convert.ToDouble(Eval("AllMoney"))).ToString("f2");
            return "";
        }
        //用于酒店订单等
        public string GetAddition(DataRow dr)
        {
            string additional = DataConverter.CStr(dr["Additional"]), result = "", contract = "";
            if (string.IsNullOrEmpty(additional)) return "";
            switch (ProClass)
            {
                case 7://旅游,酒店,机票
                    {
                        string tlp = "入住时间:{0}<br/>联系人:{1}";
                        M_Cart_Travel model = JsonConvert.DeserializeObject<M_Cart_Travel>(additional);
                        foreach (M_Cart_Contract m in model.Contract)
                        {
                            contract += m.Name + "," + m.Mobile + "|";
                        }
                        contract = contract.TrimEnd('|');
                        string another = string.IsNullOrEmpty(model.ProList[0].Remind) ? "" : DataConverter.CDate(model.ProList[0].Remind).ToString("MM-dd HH:mm");
                        result = string.Format(tlp, model.ProList[0].GoDate.ToString("MM-dd HH:mm --") + another, contract);
                    }
                    break;
                case 8:
                    {
                        string tlp = "订单信息:{0},{1}人,时间:{2}--{3}<br/>入住人:{4}<br/>联系人:{5}";
                        M_Cart_Hotel model = JsonConvert.DeserializeObject<M_Cart_Hotel>(additional);
                        foreach (M_Cart_Contract m in model.Contract)
                        {
                            contract += m.Name + "," + m.Mobile + "|";
                        }
                        contract = contract.TrimEnd('|');
                        result = string.Format(tlp, model.HotelName, model.PeopleNum, model.ProList[0].GoDate, model.ProList[0].OutDate, model.Guest[0].Name, contract);
                    }
                    break;
                case 9:
                    break;
                default:
                    break;
            }
            return result;
        }
        public void IsShowAddress(int ProClass)
        {
            switch (ProClass)
            {
                case (int)M_Product.ClassType.YG://不需要地址
                    ShowAddress = false;
                    ShowPassenger = false;
                    break;
                case (int)M_Product.ClassType.LY:
                case (int)M_Product.ClassType.JD:
                    ShowAddress = false;
                    //ReUrl_A2.Visible = true;
                    //ReUrl_A2.HRef = "/Cart/Form/Passengers?IDS=" + ids;
                    break;
                default:
                    ShowPassenger = false;
                    break;
            }
        }
    }
}