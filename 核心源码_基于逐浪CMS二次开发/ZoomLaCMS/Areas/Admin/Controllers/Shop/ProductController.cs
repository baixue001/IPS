using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using Microsoft.AspNetCore.Mvc;
using ZoomLa.BLL;
using ZoomLa.BLL.API;
using ZoomLa.BLL.Content;
using ZoomLa.BLL.CreateJS;
using ZoomLa.BLL.Helper;
using ZoomLa.BLL.Shop;
using ZoomLa.BLL.User;
using ZoomLa.Common;
using ZoomLaCMS.Control;
using ZoomLa.Model;
using ZoomLa.Model.Content;
using ZoomLa.Model.Shop;
using ZoomLa.SQLDAL;
using ZoomLa.SQLDAL.SQL;
using ZoomLaCMS.Models.Product;
using ZoomLaCMS.Ctrl;
using Microsoft.AspNetCore.Authorization;

namespace ZoomLaCMS.Areas.AdminMVC.Controllers
{
    [Area("Admin")]
    [Authorize(Policy="Admin")]
    public class ProductController : Ctrl_Admin
    {
        B_User_BindPro bindBll = new B_User_BindPro();
        B_Model modBll = new B_Model();
        B_ModelField fieldBll = new B_ModelField();
        B_Node nodeBll = new B_Node();
        B_Product proBll = new B_Product();
        B_Group gpBll = new B_Group();
        B_Stock stockBll = new B_Stock();
        B_Shop_FareTlp fareBll = new B_Shop_FareTlp();
        B_Shop_RegionPrice regionBll = new B_Shop_RegionPrice();
        B_KeyWord keyBll = new B_KeyWord();
        B_Shop_GroupPro sgpBll = new B_Shop_GroupPro();
        B_Content_VerBak verBll = new B_Content_VerBak();
        private int NodeID { get { return DataConverter.CLng(RequestEx["NodeID"]); } }
        private int ModelID { get { return DataConverter.CLng(RequestEx["ModelID"]); } }
        private int StoreID
        {
            get
            {
                return DataConverter.CLng(GetParam("StoreID"), -100);
            }
        }
        public IActionResult ProductManage()
        {
            if (!B_ARoleAuth.AuthCheckEx(ZLEnum.Auth.shop, "product")) { return WriteErr("无权访问"); }
            Filter_Product filter = new Filter_Product()
            {
                NodeID = NodeID,
                storeid = StoreID,
                pid = 0,
                stype = GetParam("stype_dp"),
                proname = GetParam("proname"),
                adduser = GetParam("adduser"),
                istrue = DataConvert.CLng(GetParam("istrue_dp"), -100),
                issale = DataConvert.CLng(GetParam("issale_dp"), -100),
                hasRecycle = 0,
                proclass = "",
                orderBy=GetParam("orderBy")
            };
            filter.special = DataConverter.CLng(GetParam("special"));
            //filter.orderSort = filter.SetSort(txtbyfilde.SelectedValue, txtbyOrder.SelectedValue);
            PageSetting setting = proBll.SelPage(CPage, PSize, filter);
            if (Request.IsAjaxRequest())
            {
                return PartialView("Product_List", setting);
            }
            else
            {
                return View(setting);
            }
        }
        public IActionResult ProductRecycle()
        {
            if (!B_ARoleAuth.AuthCheckEx(ZLEnum.Auth.shop, "product")) { return WriteErr("无权访问"); }
            Filter_Product filter = new Filter_Product()
            {
                NodeID = NodeID,
                //storeid = StoreID,
                pid = 0,
                stype = GetParam("stype_dp"),
                proname = GetParam("proname"),
                adduser = GetParam("adduser"),
                istrue = DataConvert.CLng(GetParam("istrue_dp"), -100),
                issale = DataConvert.CLng(GetParam("issale_dp"), -100),
                hasRecycle = 1,
                proclass = ""
            };
            filter.special = DataConverter.CLng(GetParam("special"));
            //filter.orderSort = filter.SetSort(txtbyfilde.SelectedValue, txtbyOrder.SelectedValue);
            PageSetting setting = proBll.SelPage(CPage, PSize, filter);
            if (Request.IsAjaxRequest())
            {
                return PartialView("Product_List", setting);
            }
            else
            {
                return View(setting);
            }
        }
        public ContentResult Product_API()
        {
            string action = GetParam("action");
            string ids = GetParam("ids");
            switch (action)
            {
                //case "upmove":
                //    {
                //        M_Product product = proBll.GetproductByid(Mid);
                //        M_Product productPre = proBll.GetNearID(NodeID, product.OrderID, 1);           //int NodeID, int CurrentID, int UporDown
                //        if (productPre.OrderID != 0)
                //        {
                //            int CurrOrder = product.OrderID;
                //            product.OrderID = productPre.OrderID;
                //            productPre.OrderID = CurrOrder;
                //            proBll.UpdateOrder(product);
                //            proBll.UpdateOrder(productPre);
                //        }
                //    }
                //    break;
                //case "downmove":
                //    {
                //        M_Product product = proBll.GetproductByid(Mid);
                //        M_Product productPre = proBll.GetNearID(NodeID, product.OrderID, 0);
                //        if (productPre.ID != 0)
                //        {
                //            int CurrOrder = product.OrderID;
                //            product.OrderID = productPre.OrderID;
                //            productPre.OrderID = CurrOrder;
                //            proBll.UpdateOrder(product);
                //            proBll.UpdateOrder(productPre);
                //        }
                //    }
                //    break;
                case "refresh"://刷新/重发布，将商品ctreatime和updatatime改为当前值
                    SafeSC.CheckIDSEx(ids);
                    string date = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");
                    DBCenter.UpdateSQL(proBll.TbName, "AddTime='" + date + "',UpdateTime='" + date + "'", "ID IN (" + ids + ")");
                    break;
                case "del":
                    SafeSC.CheckIDSEx(ids);
                    proBll.RealDelByIDS(ids);
                    break;
                case "clear":
                    proBll.ClearRecycle();
                    break;
                default:
                    proBll.setproduct(action, ids);
                    break;
            }
            return Content(Success.ToString());
        }
        //public IActionResult ProductImportTlp()
        //{
        //    //后期替换为通用导入模板
        //    if (ModelID < 1) { return WriteErr("未指定模型ID"); }
        //    M_ModelInfo modInfo = modBll.SelReturnModel(ModelID);
        //    if (modInfo == null) { return WriteErr("模型不存在"); }
        //    DataTable dt = fieldBll.GetModelFieldList(ModelID);
        //    if (dt == null || dt.Rows.Count < 1) { return WriteErr("指定的模型不存在"); }
        //    //----------------------------
        //    string str = "商品编号,条形码,商品名称,关键字,商品单位,商品重量,销售状态(1),属性设置(1),点击数,创建时间,更新时间,商品简介,详细介绍,商品清晰图,商品缩略图,生产商,品牌/商标,缺货时允许购买(0),限购数量,最低购买数量,市场参考价,当前零售价,";
        //    int i = 0;
        //    foreach (DataRow dr in dt.Rows)
        //    {
        //        str += dr["FieldAlias"].ToString();
        //        i = i + 1;
        //        if (i < dt.Rows.Count)
        //        {
        //            str += ",";
        //        }
        //    }
        //    DataGrid dg = new DataGrid();
        //    dg.DataSource = dt.DefaultView;
        //    dg.DataBind();
        //    //str += "\\n1\\n2\\n3";

        //    Encoding gb = System.Text.Encoding.GetEncoding("GB2312");
        //    StringWriter sw = new StringWriter();
        //    sw.WriteLine(str);
        //    //Response.AddHeader("Content-Disposition", "attachment; filename=" + Server.UrlEncode(filesname) + ".csv");
        //    //Response.ContentEncoding = Encoding.GetEncoding("GB2312");
        //    //Response.Write(sw.ToString());
        //    //Response.End();
        //    //  text/plain
        //    return File(Encoding.Default.GetBytes(sw.ToString()), "application/octet-stream", modInfo.ItemName + "模型导入模板.csv");
        //}
        //public IActionResult ProductImport()
        //{
        //    //int FileLen = FileUpload1.PostedFile.ContentLength;
        //    //byte[] input = new byte[FileLen];
        //    //System.IO.Stream UpLoadStream = FileUpload1.PostedFile.InputStream;
        //    var file = Request.Files["tlp_up"];
        //    Stream UpLoadStream = file.InputStream;
        //    byte[] input = new byte[file.ContentLength];
        //    UpLoadStream.Read(input, 0, file.ContentLength);
        //    UpLoadStream.Position = 0;
        //    System.IO.StreamReader sr = new System.IO.StreamReader(UpLoadStream, System.Text.Encoding.Default);
        //    //文件内容
        //    string filecontent = sr.ReadToEnd().ToString();
        //    sr.Close();
        //    filecontent = Server.HtmlEncode(filecontent);
        //    ArrayList myAL = new ArrayList();
        //    DataTable table1 = new DataTable();
        //    table1.Clear();
        //    string[] filearr = filecontent.Split(new string[] { "\n" }, StringSplitOptions.RemoveEmptyEntries);
        //    for (int i = 0; i < filearr.Length; i++)
        //    {
        //        string[] danarr = (filearr[i]).Split(new string[] { "," }, StringSplitOptions.None);

        //        if (i == 0)
        //        {
        //            for (int j = 0; j < danarr.Length; j++)
        //            {
        //                table1.Columns.Add(danarr[j].Trim());
        //            }
        //        }
        //        else
        //        {
        //            DataRow dr = table1.NewRow();
        //            for (int d = 0; d < danarr.Length; d++)
        //            {
        //                dr[d] = danarr[d].ToString();
        //            }
        //            table1.Rows.Add(dr);
        //        }
        //    }
        //    if (proBll.ImportProducts(table1, ModelID, NodeID))
        //    {
        //        return WriteOK("导入成功");
        //    }
        //    else
        //    {
        //        return WriteErr("导入失败");
        //    }
        //}


        public IActionResult AddProduct()
        {
            VM_Product vm = new VM_Product();
            if (!B_ARoleAuth.AuthCheckEx(ZLEnum.Auth.shop, "product")) {  }
            if (Mid < 1)
            {
                if (ModelID < 1) { return WriteErr("没有指定要添加内容的模型ID!");  }
                if (NodeID < 1) { return WriteErr("没有指定要添加内容的栏目节点ID!");  }
                vm.proMod = new M_Product() { Stock = 10, Rateset = 1, Dengji = 3 };
                vm.NodeID = NodeID;
                vm.ModelID = ModelID;
                vm.proMod.ProCode = B_Product.GetProCode();
                vm.ProGuid = Guid.NewGuid().ToString();
            }
            else
            {
                int VerID = DataConvert.CLng(RequestEx["Ver"]);
                if (VerID > 0)
                {
                    M_Content_VerBak verMod = verBll.SelReturnModel(VerID);
                    vm.proMod = JsonConvert.DeserializeObject<M_Product>(verMod.ContentBak);
                    if (vm.proMod.ID != Mid) { return WriteErr("加载的版本与商品不匹配");  }
                    vm.ValueDT = JsonConvert.DeserializeObject<DataTable>(verMod.TableBak);
                }
                else
                {
                    vm.proMod = proBll.GetproductByid(Mid);
                    vm.ValueDT = proBll.GetContent(vm.proMod.TableName, vm.proMod.ItemID);
                }
                vm.ProGuid = vm.proMod.ID.ToString();
                if (vm.proMod.Class == 2) { Response.Redirect(Call.PathAdmin("Shop/Arrive/SuitProAdd?ID=" + vm.proMod.ID)); }
                vm.NodeID = vm.proMod.Nodeid;
                vm.ModelID = vm.proMod.ModelID;
                if (!string.IsNullOrEmpty(vm.proMod.BindIDS))//捆绑商品
                {
                    DataTable dt = proBll.SelByIDS(vm.proMod.BindIDS, "id,Thumbnails,Proname,LinPrice");
                    vm.bindList = JsonConvert.SerializeObject(dt);
                }
                //多区域价格
                vm.regionMod = regionBll.SelModelByGuid(vm.ProGuid);
                if (vm.regionMod == null) { vm.regionMod = new M_Shop_RegionPrice(); }
                #region 特选商品
                {
                    string where = string.Format("(ProIDS LIKE '%,{0},%' OR ProIDS LIKE '{0},%' OR ProIDS LIKE '%,{0}')", vm.proMod.ID.ToString());
                    DataTable dt = DBCenter.SelWithField("ZL_User_BindPro", "UserID", where);
                    string uids = StrHelper.GetIDSFromDT(dt, "UserID");
                    ViewBag.prouids = uids;
                }
                #endregion
            }
            //------------------------------------------------------------------------------------------------
            vm.nodeMod = nodeBll.SelReturnModel(vm.NodeID);
            if (vm.nodeMod.IsNull) { return WriteErr("节点[" + vm.NodeID + "]不存在");  }
            return View(vm);
        }
        public IActionResult ProductShow()
        {
            M_Product proMod = proBll.GetproductByid(Mid);
            if (proMod == null || proMod.ID < 1) { return WriteErr("商品不存在"); }
            return View(proMod);
        }
        
        public void Product_Add()
        {
            if (!B_ARoleAuth.AuthCheckEx(ZLEnum.Auth.shop, "product")) {  }
            DataTable table = new DataTable();
            M_Product proMod = FillProductModel(ref table, proBll.GetproductByid(Mid));
            if (Mid < 1)
            {
                proMod.ID = proBll.Add(table, proMod);
                IsAddStock(proMod, DataConvert.CLng(Request.Form["Stock"]));
                //多区域价格
                //SqlParameter[] sp = new SqlParameter[] { new SqlParameter("guid", Request.Form["ProGuid"]) };
                //SqlHelper.ExecuteSql("UPDATE ZL_Shop_RegionPrice SET [ProID]=" + proMod.ID + " WHERE [Guid]=@guid", sp);
            }
            else
            {
                proBll.Update(table, proMod);
            }
            IsUserProduct(proMod, Request.Form["uprouids_old_hid"], Request.Form["uprouids_hid"]);
            IsNeedVerBak(proMod);
            IsHaveMaterial(proMod);
            IsHavePresent(proMod);
            Response.Redirect(Call.PathAdmin("Product/ProductShow?ID=" + proMod.ID));
        }
        
        public void Product_AddToNew()
        {
            if (!B_ARoleAuth.AuthCheckEx(ZLEnum.Auth.shop, "product")) { return ; }
            DataTable table = new DataTable();
            M_Product proMod = proBll.GetproductByid(Mid);
            proMod = FillProductModel(ref table, proMod);
            //------------------
            proMod.ProCode = B_Product.GetProCode();
            proMod.AddUser = adminMod.AdminName;
            proMod.Stock = 0;
            proMod.AddTime = DateTime.Now;
            proMod.UpdateTime = DateTime.Now;
            proMod.ID = 0;
            proMod.ID = proBll.Add(table, proMod);
            //------------------
            IsNeedVerBak(proMod);
            IsHaveMaterial(proMod);
            IsAddStock(proMod, DataConvert.CLng(Request.Form["Stock"]));
            IsUserProduct(proMod, Request.Form["uprouids_old_hid"], Request.Form["uprouids_hid"]);
            IsHavePresent(proMod);
            Response.Redirect(Call.PathAdmin("Product/ProductShow?ID=" + proMod.ID));
        }
        //新建商品,添加库存
        private void IsAddStock(M_Product proMod, int proStock)
        {
            if (proStock == 0) {  }
            string danju = proMod.UserShopID + DateTime.Now.ToString("yyyyMMddHHmmss");
            M_Stock stockMod = new M_Stock()
            {
                proid = proMod.ID,
                proname = proMod.Proname,
                adduser = proMod.AddUser,
                StoreID = proMod.UserShopID,
            };
            //int proStock = DataConverter.CLng(Stock.Text);
            stockMod.proid = proMod.ID;
            stockMod.stocktype = 0;
            stockMod.Pronum = proStock;
            stockMod.danju = "RK" + danju;
            stockMod.content = "添加商品:" + proMod.Proname + "入库";
            stockBll.AddStock(stockMod);
        }
        private void IsNeedVerBak(M_Product proMod)
        {
            if (!string.IsNullOrEmpty(Request.Form["verbak_chk"]))
            {
                M_Content_VerBak verMod = new M_Content_VerBak();
                DataTable valueDT = proBll.GetContent(proMod.TableName,proMod.ItemID);
                verMod.GeneralID = proMod.ID;
                verMod.ContentBak = JsonConvert.SerializeObject(proMod);
                verMod.TableBak = JsonConvert.SerializeObject(valueDT);
                verMod.Title = proMod.Proname;
                verMod.Inputer = adminMod.AdminName;
                verMod.ZType = "product";
                verBll.Insert(verMod);
            }
        }
        //更新商品所用原料信息
        private void IsHaveMaterial(M_Product proMod)
        {
            B_Shop_Material matBll = new B_Shop_Material();
            B_CodeModel codeBll = new B_CodeModel("ZL_Shop_ProMaterial");
            try
            {
                DataTable dt = DBCenter.SelWithField(matBll.TbName, "ID");
                foreach (DataRow dr in dt.Rows)
                {
                    int id = Convert.ToInt32(dr["ID"]);
                    int matnum = DataConvert.CLng(Request.Form["mat_" + id + "_num"]);
                    if (matnum < 0) { matnum = 0; }
                    DataTable codeMod = codeBll.SelByWhere("ProID=" + proMod.ID + " AND MatID=" + id, "ID DESC");
                    if (codeMod.Rows.Count < 1)
                    {
                        codeMod.Rows.Add(codeMod.NewRow());
                    }
                    codeMod.Rows[0]["ProID"] = proMod.ID;
                    codeMod.Rows[0]["MatID"] = id;
                    codeMod.Rows[0]["MatNum"] = matnum;
                    codeMod.Rows[0]["Remind"] = "";
                    if (DataConvert.CLng(codeMod.Rows[0]["ID"]) > 0) { codeBll.UpdateByID(codeMod.Rows[0]); }
                    else { codeBll.Insert(codeMod.Rows[0]); }
                }
            }
            catch { }
        }
        //会员特选商品逻辑
        private void IsUserProduct(M_Product proMod, string olduids, string newuids)
        {
            #region 特选商品
            //有更改则执行特选商品
            string uids = StrHelper.IDS_GetChange(olduids, newuids);//uprouids_old_hid.Value, uprouids_hid.Value
            if (!string.IsNullOrEmpty(uids))
            {
                //1,目标表中可能无数据
                //2,只对变更部分操作
                //目的：会员可知道自己有哪些商品,商品处也可知道可有哪些特选
                string[] add = uids.Split('|')[0].Split(',');
                string[] remove = uids.Split('|')[1].Split(',');
                foreach (string id in add)
                {
                    int uid = DataConvert.CLng(id);
                    if (uid < 1) { continue; }
                    M_User_BindPro bindMod = bindBll.SelModelByUid(uid);
                    if (bindMod == null) { bindMod = new M_User_BindPro() { UserID = uid }; }
                    bindMod.ProIDS = StrHelper.AddToIDS(bindMod.ProIDS, proMod.ID.ToString());
                    if (bindMod.ID > 0) { bindBll.UpdateByID(bindMod); }
                    else { bindBll.Insert(bindMod); }
                }
                foreach (string id in remove)
                {
                    int uid = DataConvert.CLng(id);
                    if (uid < 1) { continue; }
                    M_User_BindPro bindMod = bindBll.SelModelByUid(uid);
                    if (bindMod == null) { bindMod = new M_User_BindPro() { UserID = uid }; }
                    bindMod.ProIDS = StrHelper.RemoveToIDS(bindMod.ProIDS, proMod.ID.ToString());
                    if (bindMod.ID > 0) { bindBll.UpdateByID(bindMod); }
                    else { bindBll.Insert(bindMod); }
                }
            }
            #endregion
        }
        //是否有赠品逻辑
        private void IsHavePresent(M_Product proMod)
        {
            B_Shop_Present ptBll = new B_Shop_Present();
            ptBll.BatInsert(proMod.ID, Request.Form["present_hid"]);
        }
        private M_Product FillProductModel(ref DataTable table, M_Product proMod = null)
        {
            if (proMod == null) { proMod = new M_Product(); }
            string adminname = adminMod.AdminName;
            //DataTable gpdt = gpBll.GetGroupList();
            /*--------------proMod------------*/
            if (proMod.ID < 1)
            {
                proMod.Nodeid = NodeID;
                proMod.FirstNodeID = nodeBll.SelFirstNodeID(proMod.Nodeid);
                proMod.ModelID = ModelID;
                proMod.TableName = modBll.SelReturnModel(ModelID).TableName;
                proMod.AddUser = adminname;
            }
            DataTable dt = fieldBll.GetModelFieldList(proMod.ModelID);
            table =Call.GetDTFromMVC(dt, Request);
            //-------------------------------------------
            proMod.ProCode = Request.Form["ProCode"];
            proMod.BarCode = Request.Form["BarCode"];
            proMod.Proname = Request.Form["Proname"];
            proMod.ShortProName = Request.Form["ShortProName"];
            proMod.Kayword = Request.Form["tabinput"];
            keyBll.AddKeyWord(proMod.Kayword, 1);
            proMod.ProUnit = Request.Form["ProUnit"];
            proMod.AllClickNum = DataConverter.CLng(Request.Form["AllClickNum"]);
            proMod.Weight = DataConvert.CDouble(Request.Form["Weight_T"]);
            proMod.ProClass = DataConverter.CLng(Request.Form["proclass_rad"]);
            proMod.IDCPrice = Request.Form["IDC_Hid"];
            proMod.PointVal = DataConverter.CLng(Request.Form["PointVal"]);
            proMod.Proinfo = Request.Form["Proinfo"];
            proMod.Procontent = Request.Form["Procontent"];
            proMod.Clearimg = Request.Form["Clearimg_t"];
            proMod.Thumbnails = Request.Form["Thumbnails_t"];
            proMod.Producer = Request.Form["Producer"];
            proMod.Brand = Request.Form["Brand"];
            //proMod.Quota = DataConverter.CLng(Quota.Text);
            //proMod.DownQuota = DataConverter.CLng(DownQuota.Text);
            proMod.StockDown = DataConverter.CLng(Request.Form["StockDown"]);
            proMod.Rate = DataConverter.CDouble(Request.Form["Rate"]);
            proMod.Rateset = DataConverter.CLng(Request.Form["Rateset"]);
            proMod.Dengji = DataConverter.CLng(Request.Form["Dengji"]);
            proMod.ShiPrice = DataConverter.CDouble(Request.Form["ShiPrice"]);
            proMod.LinPrice = DataConverter.CDouble(Request.Form["LinPrice"]);
            //proMod.Preset = (OtherProject.SelectedValue == null) ? "" : OtherProject.SelectedValue;  //促销
            //proMod.Integral = DataConverter.CLng(Integral.Text);
            //proMod.Propeid = DataConverter.CLng(Propeid.Text);
            proMod.Recommend = DataConverter.CLng(Request.Form["Recommend"]);
            //proMod.Largesspirx = DataConverter.CLng(Largesspirx.Text);
            proMod.Largess = DataConvert.CLng(Request.Form["Largess"]);
            //更新时间，若没有指定则为当前时间
            proMod.UpdateTime = DataConverter.CDate(Request.Form["UpdateTime"]);
            proMod.AddTime = DataConverter.CDate(Request.Form["AddTime"]);
            proMod.ModeTemplate = Request.Form["ModeTemplate_hid"];
            //proMod.bookDay = DataConverter.CLng(BookDay_T.Text);
            proMod.BookPrice = DataConverter.CDouble(Request.Form["BookPrice"]);
            proMod.FarePrice = Request.Form["FareTlp_Rad"];
            proMod.UserShopID = DataConvert.CLng(Request.Form["UserShopID"]);//店铺页面应该禁止此项
            proMod.UserType = DataConverter.CLng(Request.Form["UserPrice_Rad"]);
            proMod.Quota = DataConvert.CLng(Request.Form["Quota_Rad"]);
            proMod.DownQuota = DataConvert.CLng(Request.Form["DownQuota_Rad"]);
            switch (proMod.UserType)
            {
                case 1:
                    proMod.UserPrice = Request.Form["UserPrice"];
                    break;
                case 2:
                    proMod.UserPrice = Request.Form["Price_Group_Hid"];
                    break;
            }
            switch (proMod.Quota)
            {
                case 0:
                    break;
                case 2:
                    proMod.Quota_Json = Request.Form["Quota_Group_Hid"];
                    break;
            }
            switch (proMod.DownQuota)
            {
                case 0:
                    break;
                case 2:
                    proMod.DownQuota_Json = Request.Form["DownQuota_Group_Hid"];
                    break;
            }
            proMod.Sales = DataConverter.CLng(Request.Form["Sales_Chk"]);
            proMod.Istrue = DataConverter.CLng(Request.Form["istrue_chk"]);
            proMod.Ishot = DataConverter.CLng(Request.Form["ishot_chk"]);
            proMod.Isnew = DataConverter.CLng(Request.Form["isnew_chk"]);
            proMod.Isbest = DataConverter.CLng(Request.Form["isbest_chk"]);
            proMod.Allowed = DataConverter.CLng(Request.Form["Allowed"]);
            proMod.GuessXML = Request.Form["GuessXML"];
            proMod.Wholesalesinfo = Request.Form["ChildPro_Hid"];
            proMod.DownCar = DataConvert.CLng(Request.Form["DownCar"]);
            proMod.SpecialID = Request.Form["Spec_Hid"];
            ////捆绑商品);
            if (!string.IsNullOrEmpty(Request.Form["Bind_Hid"]))
            {
                //获取绑定商品
                DataTable binddt = JsonHelper.JsonToDT(Request.Form["Bind_Hid"]);
                proMod.BindIDS = StrHelper.GetIDSFromDT(binddt, "ID");
            }
            else
            {
                proMod.BindIDS = "";
            }
            return proMod;
        }
        #region 库存管理
        public IActionResult StockList()
        {
            if (!B_ARoleAuth.AuthCheckEx(ZLEnum.Auth.shop, "stock")) {  }
            F_Stock filter = new F_Stock()
            {
                NodeID = DataConvert.CLng(RequestEx["NodeID"]),
                ProID = DataConvert.CLng(RequestEx["ProID"]),
                StockType = DataConvert.CLng(RequestEx["StockType"]),
                ProName = RequestEx["ProName"],
                AddUser = RequestEx["AddUser"],
                SDate = RequestEx["SDate"],
                EDate = RequestEx["EDate"]
            };
            PageSetting setting = stockBll.SelPage(CPage, PSize, filter);
            if (Request.IsAjaxRequest())
            {
                return PartialView("~/Areas/User/Views/UserShop/StockList_List.cshtml", setting);
            }
            return View(setting);
        }
        public IActionResult StockAdd()
        {
            if (!B_ARoleAuth.AuthCheckEx(ZLEnum.Auth.shop, "stock")) {  }
            int ProID = DataConvert.CLng(RequestEx["ProID"]);
            string action = DataConverter.CStr(RequestEx["action"]);
            M_Product proMod = proBll.GetproductByid(ProID);
            return View(proMod);
        }
        public IActionResult Stock_Add()
        {
            int ProID = DataConvert.CLng(RequestEx["ProID"]);
            string action = DataConverter.CStr(RequestEx["action"]);
            M_Product proMod = proBll.GetproductByid(ProID);
            M_Stock stockMod = new M_Stock();
            stockMod.proid = ProID;
            stockMod.proname = proMod.Proname;
            stockMod.Pronum = DataConverter.CLng(Request.Form["Pronum"]);
            stockMod.stocktype = DataConverter.CLng(Request.Form["stocktype_rad"]);
            string code = 0 + DateTime.Now.ToString("yyyyMMddHHmmss");
            stockMod.danju = (stockMod.stocktype == 0 ? "RK" : "CK") + code;
            stockMod.UserID = adminMod.AdminId;
            stockMod.adduser = adminMod.AdminName;
            if (stockMod.Pronum < 1) { return WriteErr("出入库数量不能小于1");  }
            switch (stockMod.stocktype)
            {
                case 0:
                    proMod.Stock += stockMod.Pronum;
                    break;
                case 1:
                    proMod.Stock -= stockMod.Pronum;
                    if (proMod.Stock < 0) { return WriteErr("出库数量不能大于库存!"); }
                    break;
                default:
                    throw new Exception("出入库操作错误");
            }
            stockBll.AddStock(stockMod);
            proBll.UpdateByID(proMod);
            if (action.Equals("addpro"))
            {
                int num = stockMod.stocktype == 0 ? stockMod.Pronum : -stockMod.Pronum;
                return Content("<script>parent.addStock(" + num + ");</script>");
            }
            return WriteOK("库存操作成功", "StockList");
        }
        #endregion
        #region 版本备份
        public IActionResult Addon_VerBak()
        {
            PageSetting setting = verBll.SelPage(CPage, PSize, DataConvert.CLng(RequestEx["GeneralID"]),"product");
            return View("Addon/VerBak", setting);
        }
        public int Addon_VerBak_Del(string ids)
        {
            verBll.Del(ids);
            return M_APIResult.Success;
        }
        #endregion
    }
}