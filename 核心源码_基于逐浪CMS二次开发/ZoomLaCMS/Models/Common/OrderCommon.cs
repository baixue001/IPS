using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Web;
using ZoomLa.BLL;
using ZoomLa.BLL.Helper;
using ZoomLa.BLL.Shop;
using ZoomLa.Common;
using ZoomLa.Components;
using ZoomLa.Model;
using ZoomLa.Model.Shop;
using ZoomLa.SQLDAL;
using ZoomLaCMS;

/// <summary>
///  订单公用类，放置用于控制显示，需重用,不应置入BLL中的方法
/// </summary>
public class OrderCommon
{
    B_OrderList orderBll = new B_OrderList();
    B_CartPro cartProBll = new B_CartPro();
    B_User buser = new B_User();
    /// <summary>
    /// 是否到期,用于IDC服务订单
    /// </summary>
    public string IsExpire(object e)
    {
        DateTime st = DateTime.Now, et = DateTime.Now;
        string result = "<span style='color:orange'>无法判断</span>";
        if (DateTime.TryParse(e.ToString(), out et))
        {
            if (st > et)
            {
                result = "<span style='color:red'>已到期</span>";
            }
            else
            {
                string day = et.Subtract(st).Days > 100000 ? "无限期" : et.Subtract(st).Days + "天";
                result = string.Format("<span style='color:green'>正常(剩余:{0})</span>", day);
            }
        }
        return result;
    }
    public string IsBindSite(object e) 
    {
        string result = "";
        if (string.IsNullOrEmpty(e.ToString()))
        {
            result = "<span style='color:red'>未绑定</span>";
        }
        else
        {
            result = "<span style='color:green'>"+e.ToString()+"</span>";
        }
        return result;
    }
    /// <summary>
    /// 付款成功后,发送管理员短信,邮件,顾客短信
    /// </summary>
    /// <cmd>详见OrderMsg_Chk等参数,payed,ordered</cmd>
    /// <returns>发送结果</returns>
    public string SendMessage(M_OrderList orderMod, M_Order_PayLog logMod, string cmd)
    {
        if (logMod == null) logMod = new M_Order_PayLog();
        DataTable orderDT = orderBll.GetOrderListByUid(orderMod.id);
        string msg = "", result = "", tlp = "";
        if (!SiteConfig.SiteOption.OrderMsg_Chk.Contains(cmd))
        {
            result = cmd + "顾客短信ERR:未开启短信通知";
        }
        else if (!RegexHelper.IsMobilPhone(orderMod.MobileNum))
        {
            result = cmd + "顾客短信ERR:(" + orderMod.MobileNum + ")手机号码不正确";
        }
        else
        {
            tlp = GetJsonVal(SiteConfig.SiteOption.OrderMsg_Tlp, cmd);
            if (string.IsNullOrEmpty(tlp))
            {
                result = cmd + "顾客短信ERR:模板为空";
            }
            else
            {
                msg = TlpDeal(tlp, orderDT);
                result = cmd + "顾客短信Info:(" + orderMod.MobileNum + ")" + SendWebSMS.SendMessage(orderMod.MobileNum, msg);
                //ZLLog.L(result+"|||"+msg);
            }
        }
        logMod.Remind += result + ",";
        /*---------------------------------------------------------*/
        if (!SiteConfig.SiteOption.OrderMasterMsg_Chk.Contains(cmd))
        {
            result = cmd + "管理员短信ERR:未开启短信通知";
        }
        else if (!RegexHelper.IsMobilPhone(SiteConfig.SiteInfo.MasterPhone))
        {
            result = cmd + "管理员短信ERR:" + SiteConfig.SiteInfo.MasterPhone + "手机号码不正确";
        }
        else
        {
            tlp = GetJsonVal(SiteConfig.SiteOption.OrderMasterMsg_Tlp, cmd);
            if (string.IsNullOrEmpty(tlp))
            {
                result = cmd + "管理员短信ERR:模板为空";
            }
            else
            {
                msg = TlpDeal(tlp, orderDT);
                result = cmd + "管理员短信Info:(" + SiteConfig.SiteInfo.MasterPhone + ")" + SendWebSMS.SendMessage(SiteConfig.SiteInfo.MasterPhone, msg);
            }
        }
        logMod.Remind += result + ",";
        /*---------------------------------------------------------*/
        if (!SiteConfig.SiteOption.OrderMasterEmail_Chk.Contains(cmd))
        {
            result = cmd + "管理员邮件ERR:未开启邮件通知";
        }
        else if (!RegexHelper.IsEmail(SiteConfig.SiteInfo.WebmasterEmail))
        {
            result = cmd + "管理员邮件ERR:" + SiteConfig.SiteInfo.WebmasterEmail + "邮件地址不正确";
        }
        else
        {
            tlp = GetJsonVal(SiteConfig.SiteOption.OrderMasterMsg_Tlp, cmd);
            if (string.IsNullOrEmpty(tlp))
            {
                result = cmd + "管理员邮件ERR:模板为空";
            }
            else
            {
                msg = TlpDeal(tlp, orderDT);
                MailAddress adMod = new MailAddress(SiteConfig.SiteInfo.WebmasterEmail);
                MailInfo mailInfo = new MailInfo() { ToAddress = adMod, IsBodyHtml = true };
                mailInfo.FromName = SiteConfig.SiteInfo.SiteName;
                mailInfo.Subject = SiteConfig.SiteInfo.SiteName + "订单付款提醒";
                mailInfo.MailBody = msg;
                result = "管理员邮件Info:(" + SiteConfig.SiteInfo.WebmasterEmail + ")" + SendMail.Send(mailInfo).ToString();
            }
        }
        logMod.Remind += result + ",";
        return logMod.Remind;
    }
    /// <summary>
    /// 处理模板内容,主用于订单,支持标签
    /// </summary>
    /// <param name="tlp">模板</param>
    /// <param name="dt">只有一行的数据dt</param>
    /// <returns>替换后的html</returns>
    public string TlpDeal(string tlp, DataTable dt)
    {
        if (dt == null || dt.Rows.Count < 1) { return tlp; }
        else { return TlpDeal(tlp, dt.Rows[0]); }
    }
    public string TlpDeal(string tlp, DataRow dr)
    {
        return ZoomLa.BLL.Sys.B_EMailHelper.TlpDeal(tlp,dr);
    }
    //------------------用于后台OrderList
    public string GetOrderNo(int id, int aside,string orderno)
    {
        string result = "";
        string url = Call.PathAdmin("Shop/Orderlistinfo");
        switch (aside) //0:正常,1:前端回收站,2:前端删除
        {
            case 0:
                result = "<a href='" + url + "?id=" + id + "'>" + orderno + "</font></a>";
                break;
            case 1:
                result = "<a href='" + url + "?id=" + id + "'><font color=#cccccc>" + orderno + "</font></a>";
                break;
            case 2:
                result = "<a href='" + url + "?id=" + id + "'><font color=#cccccc>" + orderno + "</font></a>";
                break;
            default:
                break;
        }
        return result;
    }
    //-------JSON
    public string GetJsonVal(string json, string name)
    {
        if (string.IsNullOrEmpty(json)) return "";
        JObject obj = (JObject)JsonConvert.DeserializeObject(json);
        return obj[name].ToString();
    }
    public string GetShopUrl(object storeid, object proid)
    {
        return OrderHelper.GetShopUrl(DataConvert.CLng(storeid), Convert.ToInt32(proid));
    }
    //附加金额
    public bool HasPrice(object obj)
    {
        string json = DataConvert.CStr(obj);
        if (string.IsNullOrEmpty(json)) return false;
        M_LinPrice priceMod = JsonConvert.DeserializeObject<M_LinPrice>(json);
        return (priceMod.Purse > 0 || priceMod.Sicon > 0 || priceMod.Point > 0);
    }
    //根据订单表,计算并返回总金额
    public string GetTotalJson(DataTable dt)//可传订单表或购物车表
    {
        M_LinPrice totalMod = new M_LinPrice();
        foreach (DataRow dr in dt.Rows)
        {
            if (HasPrice(dr["AllMoney_Json"]))
            {
                M_LinPrice priceMod = JsonConvert.DeserializeObject<M_LinPrice>(DataConvert.CStr(dr["AllMoney_Json"]));
                totalMod.Purse += priceMod.Purse;
                totalMod.Sicon += priceMod.Sicon;
                totalMod.Point += priceMod.Point;
            }
        }
        return JsonConvert.SerializeObject(totalMod);
    }
    //------------
    public DataTable SelStoreDT(DataTable cartDT)
    {
        DataTable storedt = new DataTable();
        storedt.Columns.Add(new DataColumn("ID", typeof(int)));
        storedt.Columns.Add(new DataColumn("StoreName", typeof(string)));
        DataRow dr2 = storedt.NewRow();
        dr2["ID"] = 0;
        dr2["StoreName"] ="自营商城";
        storedt.Rows.Add(dr2);
        string storeids = "";
        foreach (DataRow dr in cartDT.Rows)
        {
            string id = dr["StoreID"].ToString();
            if (string.IsNullOrEmpty(id) || id.Equals("0")) continue;
            storeids += id + ",";
        }
        storeids = storeids.TrimEnd(',');
        if (!string.IsNullOrEmpty(storeids))//店铺,临时
        {
            SafeSC.CheckIDSEx(storeids);
            DataTable storedt2 = SqlHelper.ExecuteTable(CommandType.Text, "Select GeneralID AS ID,Title AS StoreName From ZL_CommonModel Where Tablename like 'ZL_Store_%' And GeneralID IN(" + storeids + ")");
            storedt.Merge(storedt2);
        }
        return storedt;
    }
    //带http的地址链接(disuse,改为存mht)
    public string GetSnapShot(string url)
    {
        string result = "";
        try
        {
            WebClient wc = new WebClient();
            wc.Headers.Add(HttpRequestHeader.ContentType, "application/x-www-form-urlencoded");
            wc.Encoding = Encoding.UTF8;
            result = wc.UploadString(new Uri(url), "Post", "");
        }
        catch { result = ""; }
        return result;
    }
    public static void ProductCheck(M_Product proMod)
    {
        if (proMod == null || proMod.ID == 0)
        {
           throw new Exception("您访问的商品信息不存在!");
        }
        else if (proMod.Nodeid == 0 || proMod.ModelID == 0)
        {
            throw new Exception("商品信息有误,NodeID或ModelID指定错误");
        }
        else if (proMod.Recycler)
        {
            throw new Exception("对不起,商品已删除!");
        }
        else if (proMod.Istrue == 0)
        {
            throw new Exception("对不起,商品正在审核中!");
        }
        else if (proMod.Sales == 0)
        {
            throw new Exception("对不起,商品已停止销售!");
        }
        else if (proMod.LinPrice < 0)
        {
            throw new Exception("商品价格有误,停止销售!");
        }
    }
}