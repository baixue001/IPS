using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ZoomLa.BLL;
using ZoomLa.BLL.Content;
using ZoomLa.BLL.Shop;
using ZoomLa.Common;
using ZoomLa.Model;
using ZoomLa.Model.Content;
using ZoomLa.Model.Shop;
using ZoomLa.SQLDAL;
using ZoomLa.SQLDAL.SQL;
using ZoomLaCMS.Ctrl;

namespace ZoomLaCMS.Areas.Admin.Controllers.Shop
{
    [Area("Admin")]
    [Authorize(Policy = "Admin")]
    [Route("[area]/Shop/Store/[action]")]
    public class StoreController : Ctrl_Admin
    {
        B_Product proBll = new B_Product();
        B_Content conBll = new B_Content();
        B_Store_Info storeBll = new B_Store_Info();
        B_Store_Style styleBll = new B_Store_Style();
        B_Model modBll = new B_Model();
        B_ModelField mfBll = new B_ModelField();
        public IActionResult StoreManage()
        {
            //DataTable list = conBll.Store_Sel("", false); //mll.SelectTableName("ZL_CommonModel", "Tablename like 'ZL_Store_%'  order by GeneralID desc");
            PageSetting setting = storeBll.SelPage(CPage, PSize, new F_StoreInfo()
            {
                status = DataConvert.CLng(GetParam("status"), 99)

            });
            //switch (Request.QueryString["type"])
            //{
            //    case "0":
            //        list.DefaultView.RowFilter = "Status=99";
            //        break;
            //    case "1":
            //        list.DefaultView.RowFilter = "Status=0";
            //        break;
            //    case "2":
            //        list.DefaultView.RowFilter = "Status=99 AND EliteLevel=1";
            //        break;
            //    default:
            //        break;
            //}
            if (Request.IsAjax())
            {
                return PartialView("StoreManage_List", setting);
            }
            return View(setting);
        }
        public ContentResult Store_API()
        {
            string action = GetParam("action");
            string ids = Request.Form["idchk"];
            if (string.IsNullOrEmpty(ids)) { return Content(Failed.ToString()); }
            string[] idArr = ids.Split(",".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            foreach (string idStr in idArr)
            {
                int id = Convert.ToInt32(idStr);
                switch (action)
                {
                    case "audit"://批量审核
                        conBll.UpdateStatus(id, (int)ZLEnum.ConStatus.Audited);
                        break;
                    case "unaudit"://取消审核
                        conBll.UpdateStatus(id, (int)ZLEnum.ConStatus.Reject);
                        break;
                    case "elite"://批量推荐
                        conBll.UpdateStatus(id, (int)ZLEnum.ConStatus.Audited);
                        conBll.UpdateElite(idStr, 1);
                        break;
                    case "unelite"://取消推荐
                        conBll.UpdateElite(idStr, 0);
                        break;
                    case "del"://批量删除
                        conBll.DelContent(id);
                        break;
                }
            }
            return Content(Success.ToString());
        }
        public IActionResult StoreStyle()
        {
            PageSetting setting = styleBll.SelPage(CPage, PSize);
            if (Request.IsAjax())
            {
                return PartialView("StoreStyle_List", setting);
            }
            return View(setting);
        }
        public ContentResult Style_API()
        {
            string action = GetParam("action");
            string ids = GetParam("ids");
            switch (action)
            {
                case "del":
                    int id = DataConverter.CLng(ids);
                    styleBll.Del(id);
                    break;
            }
            return Content(Success.ToString());
        }
        public IActionResult StoreStyleAdd()
        {
            M_Store_Style styleMod = new M_Store_Style();
            if (Mid > 0)
            {
                styleMod = styleBll.SelReturnModel(Mid);
                if (styleMod == null) {return WriteErr("该店铺样式不存在"); }
            }
            return View(styleMod);
        }
        public IActionResult StoreStyleAdd_Submit()
        {
            M_Store_Style styleMod = new M_Store_Style();
            if (Mid > 0) { styleMod = styleBll.SelReturnModel(Mid); }
            styleMod.StyleName = RequestEx["StyleName_T"];
            styleMod.Template_Index = RequestEx["Template_Index_hid"];
            styleMod.Template_Content = RequestEx["Template_Content_hid"];
            styleMod.Template_List = RequestEx["Template_List_hid"];
            styleMod.Remind = RequestEx["Remind_T"];
            styleMod.Thumbnail = RequestEx["Thumbnail_t"];
            if (styleMod.ID > 0)
            {
                styleBll.UpdateByID(styleMod);
            }
            else
            {
                styleBll.Insert(styleMod);
            }
            return WriteOK("操作成功","StoreStyle");
        }
        public IActionResult StoreUpdate()
        {
            M_CommonData storeMod = conBll.SelReturnModel(Mid);
            if (storeMod == null) { storeMod = new M_CommonData(); }
            return View(storeMod);
        }
        public IActionResult StoreUpdate_Submit()
        {
            M_CommonData storeMod = new M_CommonData();
            M_UserInfo mu = buser.SelReturnModel(DataConverter.CLng(RequestEx["UserName_H"]));
            if (Mid > 0) { storeMod = conBll.SelReturnModel(Mid); }
            else
            {
                storeMod.ModelID = Convert.ToInt32(modBll.SelByType("6").Rows[0]["ModelID"]);
                M_ModelInfo modMod = modBll.SelReturnModel(storeMod.ModelID);
                storeMod.TableName = modMod.TableName;
            }
            storeMod.SuccessfulUserID = mu.UserID;
            storeMod.Inputer = mu.UserName;
            storeMod.Title = RequestEx["UserShopName_T"];
            storeMod.DefaultSkins = DataConverter.CLng(RequestEx["TlpView_Tlp"]);
            DataTable dt = mfBll.GetModelFieldList(storeMod.ModelID);
            DataTable table = Call.GetDTFromMVC(dt, Request);
            table = storeBll.FillDT(storeMod, table);
            if (storeMod.GeneralID > 0)
            {
                conBll.UpdateContent(table, storeMod);
            }
            else
            {
                conBll.AddContent(table, storeMod);
            }
            return WriteOK("操作成功", "StoreUpdate");
        }
        public IActionResult Product()
        {
            if (!B_ARoleAuth.AuthCheckEx(ZLEnum.Auth.shop, "product")) { return WriteErr("无权访问"); }
            Filter_Product filter = new Filter_Product()
            {
                NodeID = DataConvert.CLng(RequestEx["NodeID"]),
                storeid = DataConvert.CLng(RequestEx["StoreID"],-1),
                pid = 0,
                stype = GetParam("stype_dp"),
                proname = GetParam("proname"),
                adduser = GetParam("adduser"),
                istrue = DataConvert.CLng(GetParam("istrue_dp"), -100),
                issale = DataConvert.CLng(GetParam("issale_dp"), -100),
                hasRecycle = 0,
                proclass = "",
                orderBy = GetParam("orderBy")
            };
            filter.special = DataConverter.CLng(GetParam("special"));
            //filter.orderSort = filter.SetSort(txtbyfilde.SelectedValue, txtbyOrder.SelectedValue);
            PageSetting setting = proBll.SelPage(CPage, PSize, filter);
            if (Request.IsAjaxRequest())
            {
                return PartialView("/Areas/Admin/Views/Product/Product_List.cshtml", setting);
            }
            else
            {
                return View(setting);
            }
        }
        //public IActionResult StoreConfig()
        //{
        //    return View();
        //}
        //public IActionResult StoreConfig_Submit()
        //{
        //    return WriteOK("操作成功","StoreConfig");
        //}
        //public IActionResult DeliveryMan()
        //{
        //    return View();
        //}
    }
}