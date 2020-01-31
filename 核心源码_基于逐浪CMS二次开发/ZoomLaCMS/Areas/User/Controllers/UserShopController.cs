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
using ZoomLa.BLL.Helper;
using ZoomLa.BLL.Shop;
using ZoomLa.Common;
using ZoomLaCMS.Control;
using ZoomLa.Model;
using ZoomLa.Model.Shop;
using ZoomLa.SQLDAL;
using ZoomLa.SQLDAL.SQL;
using ZoomLaCMS.Areas.User.Models;
using ZoomLaCMS.Models.Product;
using Microsoft.AspNetCore.Authorization;
using ZoomLaCMS.Ctrl;
using ZoomLa.Model.Content;
using ZoomLa.BLL.Content;

namespace ZoomLaCMS.Areas.User.Controllers
{
    [Area("User")]
    [Authorize(Policy = "User")]
    public class UserShopController : Ctrl_User
    {
        // GET: /User/UserShop/
        B_Store_Info storeBll = new B_Store_Info();
        OrderCommon orderCom = new OrderCommon();
        B_Content conBll = new B_Content();
        B_Model modBll = new B_Model();
        B_ModelField fieldBll = new B_ModelField();
        B_CartPro cartProBll = new B_CartPro();
        B_Product proBll = new B_Product();
        B_Node nodeBll = new B_Node();
        B_Stock stockBll = new B_Stock();
        B_OrderList orderBll = new B_OrderList();
        B_Shop_FareTlp fareBll = new B_Shop_FareTlp();
        B_Shop_RegionPrice regionBll = new B_Shop_RegionPrice();
        B_Payment payBll = new B_Payment();
        B_KeyWord keyBll = new B_KeyWord();
        B_Shop_DeliveryMan manBll = new B_Shop_DeliveryMan();
        B_Store_Style styleBll = new B_Store_Style();
        B_Order_Share shareBll = new B_Order_Share();
        B_Arrive avBll = new B_Arrive();
        B_Order_Exp expBll = new B_Order_Exp();
        public string Action { get { return DataConvert.CStr(RequestEx["Action"]).ToLower(); } }
        private int NodeID { get { return DataConverter.CLng(RequestEx["NodeID"]); } }
        private int ModelID { get { return DataConverter.CLng(RequestEx["ModelID"]); } }
        #region 店铺申请|信息修改
        public void Default() { Response.Redirect("Index");  }
        public IActionResult Index()
        {
            M_CommonData storeMod = conBll.SelMyStore(mu.UserName);
            if (storeMod == null)
            {
                return RedirectToAction("StoreApply");//申请店铺
            }
            else if (storeMod.Status != 99)//等待审核
            {
                return RedirectToAction("StoreEdit");
            }
            else
            {
                DataTable cmdinfo = conBll.GetContent(storeMod.GeneralID);
                if (cmdinfo.Rows.Count < 1) { return WriteErr("店铺信息不完整");  }
                DataRow dr = cmdinfo.Rows[0];
                //DataTable sstDT = sstbll.GetStyleByModel(Convert.ToInt32(dr["StoreModelID"]), 1);
                //M_StoreStyleTable sst = sstbll.GetStyleByID(Convert.ToInt32(dr["StoreStyleID"]));
                //ViewBag.sstdp = MVCHelper.ToSelectList(sstDT, "StyleName", "ID", dr["StoreStyleID"].ToString());
                ViewBag.dr = dr;
                //ViewBag.sstimg = sst == null ? "" : function.GetImgUrl(sst.StylePic);
             
            }
            DataTable styleDt = styleBll.Sel();
            styleDt.Columns["ID"].ColumnName = "TemplateID";
            styleDt.Columns["Template_Index"].ColumnName = "TemplateUrl";
            styleDt.Columns["Thumbnail"].ColumnName = "TemplatePic";
            styleDt.Columns["StyleName"].ColumnName = "rname";
            ViewBag.styleDt = styleDt;
            return View(storeMod);
        }
        
        //审核完成后,用户可自由修改店铺信息
        public IActionResult UserShop_Edit()
        {
            M_CommonData storeMod = conBll.SelMyStore(mu.UserName);
            DataTable cmdinfo = conBll.GetContent(storeMod.GeneralID);
            //----------
            DataTable dt = fieldBll.GetModelFieldList(Convert.ToInt32(cmdinfo.Rows[0]["StoreModelID"]));
            Call commonCall = new Call();
            DataTable table;
            try
            {
                table = Call.GetDTFromMVC(dt, Request);
            }
            catch (Exception e)
            {
                return WriteErr(e.Message); 
            }
            storeMod.Title = RequestEx["StoreName_T"];
            storeMod.IP =IPScaner.GetUserIP(HttpContext);
            storeMod.DefaultSkins = DataConverter.CLng(RequestEx["TempleID_Hid"]);
            conBll.UpdateContent(table, storeMod);
            return WriteOK("提交成功", "Index"); 
        }
        public void StoreEdit() { Response.Redirect("StoreAuditing"); }
        public IActionResult StoreAuditing() { return View(); }
        
        public IActionResult StoreApply()
        {
            DataTable moddt = modBll.GetListStore();
            M_CommonData storeMod = conBll.SelMyStore(mu.UserName);
            if (moddt.Rows.Count < 1) { return WriteErr("管理员未指定店铺申请模型");  }
            ViewBag.moddp = MVCHelper.ToSelectList(moddt, "ModelName", "ModelID");
            switch (Action)
            {
                case "edit":
                    {
                        if (storeMod == null) { return WriteErr("店铺申请不存在");  }
                        //DataTable dtContent = conBll.GetContent(storeMod.GeneralID);
                        //ViewBag.modhtml = fieldBll.InputallHtml(storeMod.ModelID, 0, new ModelConfig()
                        //{
                        //    ValueDT = dtContent
                        //});
                    }
                    break;
                default:
                    {
                        //ViewBag.modhtml = fieldBll.InputallHtml(DataConvert.CLng(moddt.Rows[0]["ModelID"]), 0, new ModelConfig()
                        //{
                        //    Source = ModelConfig.SType.Admin
                        //});
                        //如果未开通则填写,已申请未审核则显示提示,已开通跳回index页
                        if (storeMod != null)
                        {
                            if (storeMod.Status != (int)ZLEnum.ConStatus.Audited) { return View("StoreAuditing"); }
                            else { RedirectToAction("Store_Edit"); }
                        }
                        else { storeMod = new M_CommonData() { ModelID = DataConvert.CLng(moddt.Rows[0]["ModelID"]) }; }
                    }
                    break;
            }
            DataTable styleDt = styleBll.Sel();
            styleDt.Columns["ID"].ColumnName = "TemplateID";
            styleDt.Columns["Template_Index"].ColumnName = "TemplateUrl";
            styleDt.Columns["Thumbnail"].ColumnName = "TemplatePic";
            styleDt.Columns["StyleName"].ColumnName = "rname";
            ViewBag.styleDt = styleDt;
            return View(storeMod);
        }
        
        public IActionResult Apply_Add()
        {
            B_Store_Info stBll = new B_Store_Info();
            int modelid = DataConvert.CLng(RequestEx["model_dp"]);
            string store = RequestEx["store_t"];
            M_CommonData CData = conBll.SelMyStore(mu.UserName);
            if (CData == null) { CData = new M_CommonData(); }
            //----------------------------------------
            if (string.IsNullOrEmpty(store)) { return WriteErr("店铺名称不能为空");  }
            //M_StoreStyleTable sst = sstbll.GetNewStyle(modelid);
            //if (sst.ID == 0) { return WriteErr("后台没有为该模型绑定可用的模板!");  }
            CData.Title = HttpUtility.HtmlEncode(RequestEx["store_t"]);
            CData.DefaultSkins = DataConverter.CLng(RequestEx["TempleID_Hid"]);
            CData.ModelID = modelid;
            CData.TableName = modBll.SelReturnModel(CData.ModelID).TableName;
            CData.Inputer = mu.UserName;
            CData.SuccessfulUserID = mu.UserID;
            CData.Inputer = mu.UserName;
            CData.IP =IPScaner.GetUserIP(HttpContext);
            DataTable dt = fieldBll.GetModelFieldList(modelid);
            DataTable table =Call.GetDTFromMVC(dt, Request);
            table = stBll.FillDT(CData, table);
            if (CData.GeneralID > 0) { conBll.UpdateContent(table, CData); }
            else { conBll.AddContent(table, CData); }
            return View("StoreAuditing");
        }
        
        public void ViewMyStore()
        {
            M_Store_Info storeMod = storeBll.SelModelByUser(mu.UserID);
            Response.Redirect("/Store/StoreIndex?id=" + storeMod.ID); 
        }
        #endregion
        #region 库存管理
        
        public IActionResult StockList()
        {
            M_Store_Info storeMod = storeBll.SelModelByUser(mu.UserID);
            F_Stock filter = new F_Stock()
            {
                NodeID = DataConvert.CLng(RequestEx["NodeID"]),
                ProID = DataConvert.CLng(RequestEx["ProID"]),
                StockType = DataConvert.CLng(RequestEx["StockType"]),
                StoreID = storeMod.ID,
                ProName = RequestEx["ProName"],
                AddUser = RequestEx["AddUser"],
                SDate = RequestEx["SDate"],
                EDate = RequestEx["EDate"]
            };
            PageSetting setting = stockBll.SelPage(CPage, PSize,filter);
            if (Request.IsAjaxRequest())
            {
                return PartialView("StockList_List", setting);
            }
            return View(setting);
        }
        
        public IActionResult StockAdd()
        {
            int ProID = DataConvert.CLng(RequestEx["ProID"]);
            string action = DataConverter.CStr(RequestEx["action"]);
            M_Product proMod = Stock_GetProByID(mu, ProID);
            return View(proMod);
        }
        
        public IActionResult Stock_Add()
        {
            M_Store_Info storeMod = storeBll.SelModelByUser(mu.UserID);
            int ProID = DataConvert.CLng(RequestEx["ProID"]);
            string action = DataConverter.CStr(RequestEx["action"]);
            M_Product proMod = Stock_GetProByID(mu, ProID);
            M_Stock stockMod = new M_Stock();
            stockMod.proid = ProID;
            stockMod.proname = proMod.Proname;
            stockMod.Pronum = DataConverter.CLng(RequestEx["Pronum"]);
            stockMod.stocktype = DataConverter.CLng(RequestEx["stocktype_rad"]);
            string code = storeMod.ID + DateTime.Now.ToString("yyyyMMddHHmmss");
            stockMod.danju = (stockMod.stocktype == 0 ? "RK" : "CK") + code;
            stockMod.UserID = mu.UserID;
            stockMod.adduser = mu.UserName;
            stockMod.StoreID = storeMod.ID;
            if (stockMod.Pronum < 1) { return WriteErr("出入库数量不能小于1");  }
            switch (stockMod.stocktype)
            {
                case 0:
                    proMod.Stock += stockMod.Pronum;
                    break;
                case 1:
                    proMod.Stock -= stockMod.Pronum;
                    if (proMod.Stock < 0) { return WriteErr("出库数量不能大于库存!");  }
                    break;
                default:
                    throw new Exception("出入库操作错误");
            }
            stockBll.AddStock(stockMod);
            proBll.UpdateByID(proMod);
            if (action.Equals("addpro"))
            {
                int num = stockMod.stocktype == 0 ? stockMod.Pronum : -stockMod.Pronum;
            }
            return WriteOK("库存操作成功", "StockList");
        }
        private M_Product Stock_GetProByID(M_UserInfo mu, int ProID)
        {
            M_Product proMod = proBll.GetproductByid(ProID);
            if (proMod == null) { throw new Exception("商品不存在"); }
            if (proMod.UserID != mu.UserID) { throw new Exception("你无权操作该商品库存"); }
            return proMod;
        }
        #endregion
        #region 订单管理
        public IActionResult OrderList()
        {
            M_CommonData storeMod = conBll.SelMyStore_Ex(HttpContext, ref err);
            if (!string.IsNullOrEmpty(err)) { return WriteErr(err); }
            Filter_Order filter = new Filter_Order();
            filter.cpage = CPage;
            filter.psize = PSize;
            filter.storeType = storeMod.GeneralID.ToString();
            OrderViewModel viewMod = new OrderViewModel(filter, Request);
            viewMod.usefor = "store";
            if (Request.IsAjax())
            {
                return PartialView("OrderList_List", viewMod);
            }
            else
            {
                return View(viewMod);
            }
        }
        #endregion
        #region 送货管理
        public IActionResult DeliveryMan()
        {
            M_CommonData storeMod = conBll.SelMyStore_Ex(HttpContext, ref err);
            if (!string.IsNullOrEmpty(err)) { return WriteErr(err); }
            PageSetting setting = manBll.SelPage(CPage, PSize, storeMod.GeneralID);
            if (Request.IsAjaxRequest()) { return PartialView("DeliveryMan_List", setting); }
            return View(setting);
        }
        public IActionResult AddDeliverMan()
        {
            M_Shop_DeliveryMan delyMod = manBll.SelReturnModel(Mid);
            if (delyMod == null) { delyMod = new M_Shop_DeliveryMan(); }
            return View(delyMod);
        }
        public IActionResult DeliveryMan_Add()
        {
            M_Shop_DeliveryMan manMod = manBll.SelReturnModel(Mid);
            if (manMod == null) { manMod = new M_Shop_DeliveryMan(); }
            M_CommonData storeMod = conBll.SelMyStore_Ex(HttpContext, ref err);
            if (!string.IsNullOrEmpty(err)) { return WriteErr(err);  }
            manMod.Remind = RequestEx["Remind"];
            manMod.Auth = RequestEx["auth"];
            manMod.Bonus = DataConvert.CDouble(RequestEx["Bonus"]);
            //用户相关
            string input_user = RequestEx["user"];
            if (string.IsNullOrWhiteSpace(input_user)) { return WriteErr("用户信息不能为空");  }
            M_UserInfo user = buser.SelReturnModel(DataConverter.CLng(input_user));
            if (user.IsNull) { user = buser.GetUserByName(input_user); }
            if (user.IsNull) { return WriteErr("用户不存在");  }
            manMod.UserID = user.UserID;
            if (manMod.ID > 0)
            {
                if (manMod.StoreID != storeMod.GeneralID) { return WriteErr("你无权修改该送货员");  }
                manBll.UpdateByID(manMod);
            }
            else
            {
                DataTable dt = manBll.Sel(manMod.UserID);
                if (dt != null && dt.Rows.Count > 0)
                {
                    if (DataConverter.CLng(dt.Rows[0]["StoreID"]) == storeMod.GeneralID) { return WriteErr("该送货员已存在,不可重复添加");  }
                    else { return WriteErr("该用户已经是送货员");  }
                }
                manMod.StoreID = storeMod.GeneralID;
                manBll.Insert(manMod);
            }
            return WriteOK("操作成功", "DeliveryMan");
        }
        public int DeliveryMan_Del(string ids)
        {
            M_CommonData storeMod = conBll.SelMyStore_Ex(HttpContext, ref err);
            if (!string.IsNullOrEmpty(err)) { return Failed; }
            manBll.DelByIDS(ids, storeMod.GeneralID);
            return Success;
        }
        //给送货员看的结清订单
        public IActionResult DeliveryManOrder()
        {
            //DataTable dt = manBll.Sel(mu.UserID, "settle");
            //string storeIds = "";
            //foreach (DataRow dr in dt.Rows)
            //{
            //    storeIds += dr["StoreID"] + ",";
            //}
            int expStatus = DataConvert.CLng(RequestEx["exp"], -100);
            PageSetting setting = SPageByStore(CPage, PSize, mu.UserID, expStatus);
            if (Request.IsAjaxRequest()) { return PartialView("DeliveryManOrder_List", setting); }
            return View(setting);
        }
        public PageSetting SPageByStore(int cpage, int psize, int expUid, int expStatus = -100)
        {
            //if (string.IsNullOrEmpty(storeids)) { return new PageSetting() { dt = new DataTable() }; }
            //SafeSC.CheckIDSEx(storeids);
            //所有已付款,指定了送货员的订单
            string mTbName = "(SELECT A.*,B.PayType,B.PrePayInfo,C.ExpNo FROM ZL_OrderInfo A LEFT JOIN ZL_Payment B ON A.PayMentNo = B.PayNO LEFT JOIN ZL_Order_Exp C ON A.ExpressNum = C.ID WHERE A.Paymentstatus = 1 AND C.ExpNo='" + expUid + "')";
            //string where = "A.StoreID IN (" + storeids + ")  AND A.Settle=0 AND A.Paymentstatus=1";
            string where = "1=1 ";
            if (expStatus == -100)
            { }
            else if (expStatus == 99)
            {
                //已完结==预付单支付了尾款,普通订单完成了签收
                where += " AND ((A.PayType=1 AND A.PrePayInfo LIKE '%\"status\":99%') OR (A.PayType!=1 AND A.StateLogistics=99))";
            }
            else { where += " AND A.StateLogistics=" + expStatus; }
            PageSetting setting = PageSetting.Double(cpage, psize, mTbName, "ZL_CommonModel", "A.ID", "A.StoreID=B.GeneralID", where, "", null, "A.*,B.Title StoreName");
            DBCenter.SelPage(setting);
            return setting;
        }
        //运费模板
        public IActionResult DeliverType()
        {
            PageSetting setting = fareBll.SelPage(CPage, PSize,new Com_Filter() {uids=mu.UserID.ToString() });
            if (Request.IsAjaxRequest()) { return PartialView("DeliverType_List", setting); }
            else { return View("DeliverType", setting); }
        }
        public IActionResult AddDeliverType()
        {
            M_Shop_FareTlp fareMod = new M_Shop_FareTlp();
            if (Mid > 0)
            {
                fareMod = fareBll.SelReturnModel(Mid);
                if (mu.UserID != fareMod.UserID) { return WriteErr("你无权限修改该模板!");  }
            }
            return View(fareMod);
        }
        public IActionResult Deliver_Add()
        {
            M_Shop_FareTlp fareMod = new M_Shop_FareTlp();
            if (Mid > 0) { fareMod = fareBll.SelReturnModel(Mid); }
            fareMod.TlpName = RequestEx["TlpName_T"];
            fareMod.PriceMode = Convert.ToInt32(RequestEx["pricemod_rad"]);
            fareMod.Express = RequestEx["Fare_Hid"];
            fareMod.UserID = mu.UserID;
            //fareMod.Mail = "";
            fareMod.Remind = RequestEx["Remind_T"];
            fareMod.Remind2 = RequestEx["Remind2_T"];
            if (Mid > 0) { fareBll.UpdateByID(fareMod); }
            else { fareBll.Insert(fareMod); }
            return WriteOK("操作成功", "DeliverType"); 
        }
        public int Deliver_Del(string ids)
        {
            fareBll.DelByIDS(ids, mu.UserID);
            return Success;
        }
        #endregion
        #region 商品相关
        
        public IActionResult AddProduct()
        {
            M_Store_Info storeMod = storeBll.SelModelByUser(mu.UserID);
            VM_Product vm = new VM_Product();
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
                vm.proMod = proBll.GetproductByid(Mid);
                if (vm.proMod.UserShopID != storeMod.ID) { return WriteErr("你无权修改该商品");  }
                vm.NodeID = vm.proMod.Nodeid;
                vm.ModelID = vm.proMod.ModelID;
                vm.ValueDT = proBll.GetContent(vm.proMod.TableName, vm.proMod.ItemID);
                //多区域价格
                vm.ProGuid = vm.proMod.ID.ToString();
                vm.regionMod = regionBll.SelModelByGuid(vm.ProGuid);
                if (vm.regionMod == null) { vm.regionMod = new M_Shop_RegionPrice(); }
                //捆绑商品
                if (!string.IsNullOrEmpty(vm.proMod.BindIDS))
                {
                    DataTable dt = proBll.SelByIDS(vm.proMod.BindIDS, "id,Thumbnails,Proname,LinPrice");
                    vm.bindList = JsonConvert.SerializeObject(dt);
                }
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
            if (vm.nodeMod.IsNull) { return WriteErr("节点[" + NodeID + "]不存在");  }
            return View(vm);
        }
        
        
        public IActionResult Product_Add()
        {
            //不支持版本备份,不支持会员特选
            DataTable table = new DataTable();
            M_Product proMod = FillProductModel(ref table, proBll.GetproductByid(Mid));
            if (Mid < 1)
            {
                proMod.ID = proBll.Add(table, proMod);
                IsAddStock(proMod, DataConvert.CLng(RequestEx["Stock"]));
                //多区域价格
                SqlParameter[] sp = new SqlParameter[] { new SqlParameter("guid", RequestEx["ProGuid"]) };
                SqlHelper.ExecuteSql("UPDATE ZL_Shop_RegionPrice SET [ProID]=" + proMod.ID + " WHERE [Guid]=@guid", sp);
            }
            else
            {
                proBll.Update(table, proMod);
            }
            IsHavePresent(proMod);
            return WriteOK("操作成功", "ProductList");
        }
        
        
        public IActionResult Product_AddToNew()
        {
            DataTable table = new DataTable();
            M_Product srcMod = proBll.GetproductByid(Mid);
            srcMod = FillProductModel(ref table, srcMod);
            //------------------
            srcMod.ProCode = B_Product.GetProCode();
            srcMod.AddUser = mu.UserName;
            srcMod.Stock = 0;
            srcMod.AddTime = DateTime.Now;
            srcMod.UpdateTime = DateTime.Now;
            srcMod.ID = 0;
            srcMod.ID = proBll.Add(table, srcMod);
            //------------------
            IsAddStock(srcMod, DataConvert.CLng(RequestEx["Stock"]));
            IsHavePresent(srcMod);
            return WriteOK("操作成功", "ProductList"); 
        }
        private M_Product FillProductModel(ref DataTable table, M_Product proMod = null)
        {
            if (proMod == null) { proMod = new M_Product(); }
            //DataTable gpdt = gpBll.GetGroupList();
            /*--------------proMod------------*/
            if (proMod.ID < 1)
            {
                proMod.Nodeid = NodeID;
                proMod.FirstNodeID = nodeBll.SelFirstNodeID(proMod.Nodeid);
                proMod.ModelID = ModelID;
                proMod.TableName = modBll.SelReturnModel(ModelID).TableName;
                proMod.AddUser = mu.UserName;

                //店铺专用
                M_CommonData storeMod = conBll.SelMyStore_Ex(HttpContext,ref err);
                if (!string.IsNullOrEmpty(err)) { throw new Exception(err); }
                proMod.UserShopID = storeMod.GeneralID;
                proMod.UserID = mu.UserID;
            }
            DataTable dt = fieldBll.GetModelFieldList(proMod.ModelID);
            table = Call.GetDTFromMVC(dt, Request);
            //-------------------------------------------
            proMod.ProCode = RequestEx["ProCode"];
            proMod.BarCode = RequestEx["BarCode"];
            proMod.Proname = RequestEx["Proname"];
            proMod.ShortProName = RequestEx["ShortProName"];
            proMod.Kayword = RequestEx["tabinput"];
            keyBll.AddKeyWord(proMod.Kayword, 1);
            proMod.ProUnit = RequestEx["ProUnit"];
            proMod.AllClickNum = DataConverter.CLng(RequestEx["AllClickNum"]);
            proMod.Weight = DataConvert.CDouble(RequestEx["Weight_T"]);
            proMod.ProClass = DataConverter.CLng(RequestEx["proclass_rad"]);
            proMod.IDCPrice = RequestEx["IDC_Hid"];
            //前端不允许定义赠送值
            //proMod.PointVal = DataConverter.CLng(RequestEx["PointVal"]);
            proMod.Proinfo = RequestEx["Proinfo"];
            proMod.Procontent = RequestEx["Procontent"];
            proMod.Clearimg = RequestEx["Clearimg_t"];
            proMod.Thumbnails = RequestEx["Thumbnails_t"];
            //proMod.Producer = RequestEx["Producer"];
            //proMod.Brand = RequestEx["Brand"];
            //proMod.Quota = DataConverter.CLng(Quota.Text);
            //proMod.DownQuota = DataConverter.CLng(DownQuota.Text);
            proMod.StockDown = DataConverter.CLng(RequestEx["StockDown"]);
            proMod.Rate = DataConverter.CDouble(RequestEx["Rate"]);
            proMod.Rateset = DataConverter.CLng(RequestEx["Rateset"]);
            proMod.Dengji = DataConverter.CLng(RequestEx["Dengji"]);
            proMod.ShiPrice = DataConverter.CDouble(RequestEx["ShiPrice"]);
            proMod.LinPrice = DataConverter.CDouble(RequestEx["LinPrice"]);
            //proMod.Preset = (OtherProject.SelectedValue == null) ? "" : OtherProject.SelectedValue;  //促销
            //proMod.Integral = DataConverter.CLng(Integral.Text);
            //proMod.Propeid = DataConverter.CLng(Propeid.Text);
            proMod.Recommend = DataConverter.CLng(RequestEx["Recommend"]);
            //proMod.Largesspirx = DataConverter.CLng(Largesspirx.Text);
            proMod.Largess = DataConvert.CLng(RequestEx["Largess"]);
            //更新时间，若没有指定则为当前时间
            proMod.UpdateTime = DataConverter.CDate(RequestEx["UpdateTime"]);
            proMod.AddTime = DataConverter.CDate(RequestEx["AddTime"]);
            proMod.ModeTemplate = RequestEx["ModeTemplate_hid"];
            //proMod.bookDay = DataConverter.CLng(BookDay_T.Text);
            proMod.BookPrice = DataConverter.CDouble(RequestEx["BookPrice"]);
            proMod.FarePrice = RequestEx["FareTlp_Rad"];
            //proMod.UserShopID = DataConvert.CLng(RequestEx["UserShopID"]);//店铺页面应该禁止此项
            proMod.UserType = DataConverter.CLng(RequestEx["UserPrice_Rad"]);
            proMod.Quota = DataConvert.CLng(RequestEx["Quota_Rad"]);
            proMod.DownQuota = DataConvert.CLng(RequestEx["DownQuota_Rad"]);
            switch (proMod.UserType)
            {
                case 1:
                    proMod.UserPrice = RequestEx["UserPrice"];
                    break;
                case 2:
                    proMod.UserPrice = RequestEx["Price_Group_Hid"];
                    break;
            }
            switch (proMod.Quota)
            {
                case 0:
                    break;
                case 2:
                    proMod.Quota_Json = RequestEx["Quota_Group_Hid"];
                    break;
            }
            switch (proMod.DownQuota)
            {
                case 0:
                    break;
                case 2:
                    proMod.DownQuota_Json = RequestEx["DownQuota_Group_Hid"];
                    break;
            }
            proMod.Sales = DataConverter.CLng(RequestEx["Sales_Chk"]);
            proMod.Istrue = DataConverter.CLng(RequestEx["istrue_chk"]);
            proMod.Ishot = DataConverter.CLng(RequestEx["ishot_chk"]);
            proMod.Isnew = DataConverter.CLng(RequestEx["isnew_chk"]);
            proMod.Isbest = DataConverter.CLng(RequestEx["isbest_chk"]);
            proMod.Allowed = DataConverter.CLng(RequestEx["Allowed"]);
            proMod.GuessXML = RequestEx["GuessXML"];
            proMod.Wholesalesinfo = RequestEx["ChildPro_Hid"];
            proMod.DownCar = DataConvert.CLng(RequestEx["DownCar"]);
            ////捆绑商品
            if (!string.IsNullOrEmpty(RequestEx["Bind_Hid"]))
            {
                //获取绑定商品
                DataTable binddt = JsonHelper.JsonToDT(RequestEx["Bind_Hid"]);
                proMod.BindIDS = StrHelper.GetIDSFromDT(binddt, "ID");
            }
            else
            {
                proMod.BindIDS = "";
            }
            return proMod;
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
        //是否有赠品逻辑
        private void IsHavePresent(M_Product proMod)
        {
            B_Shop_Present ptBll = new B_Shop_Present();
            ptBll.BatInsert(proMod.ID, RequestEx["present_hid"]);
        }
        
        public IActionResult ProductList()
        {
            int NodeID = DataConvert.CLng(RequestEx["NodeID"]);
            int filter = DataConvert.CLng(RequestEx["quicksouch"]);
            M_Store_Info storeMod = storeBll.SelModelByUser(mu.UserID);
            PageSetting setting = proBll.SelPage(CPage, PSize, new Filter_Product()
            {
                storeid = storeMod.ID,
                NodeID = NodeID,
                proname = GetParam("keyword"),
                adduser = mu.UserName
            });
            if (Request.IsAjaxRequest())
            {
                return PartialView("ProductList_List", setting);
            }
            else
            {
                string str = "";
                string ModeIDList = "";
                string[] ModelID = null;
                if (NodeID > 0)
                {
                    M_Node nod = nodeBll.GetNodeXML(NodeID);
                    ModeIDList = nod.ContentModel;
                    ModelID = ModeIDList.Split(',');
                    string tlp = " <div class='btn-group'><button type='button' class='btn btn-default dropdown-toggle' data-toggle='dropdown'>{0}管理<span class='caret'></span></button><ul class='dropdown-menu' role='menu'><li><a href='AddProduct?ModelID={1}&NodeID={2}'>添加{0}</a></li><li><a href='javascript:;' onclick='ShowImport();'>导入{0}</a></li></ul></div>";
                    if (ModelID != null)
                    {
                        for (int i = 0; i < ModelID.Length; i++)
                        {
                            M_ModelInfo model = modBll.SelReturnModel(DataConverter.CLng(ModelID[i]));
                            if (!string.IsNullOrEmpty(model.ItemName))
                            {
                                str += string.Format(tlp, model.ItemName, ModelID[i], NodeID);
                            }
                        }
                    }
                }
                ViewBag.addhtml = str;
                return View(setting);
            }
        }
        
        public IActionResult SelShopNode()
        {
            int nid = DataConvert.CLng(RequestEx["NodeID"]);
            if (nid > 0)
            {
                M_Node nodeMod = nodeBll.SelReturnModel(nid);
                int mid = DataConvert.CLng(nodeMod.ContentModel.Split(',')[0]);
                //return RedirectToAction("AddProduct?NodeID=" + nid + "&ModelID=" + mid, new { nodeid = nid, modelid = mid });
                Response.Redirect("AddProduct?NodeID=" + nid + "&ModelID=" + mid); 
            }
            return View();
        }
        private string GetTemplate(M_CommonData storeMod,string type)
        {
            M_Store_Style styleMod = styleBll.SelReturnModel(storeMod.DefaultSkins);
            switch (type)
            {
                case "index":

                    break;
                case "content":

                    break;
                case "list":

                    break;
            }
            return "";
        }
        
        [HttpPost]
        public string ShopNode_API()
        {
            M_APIResult retMod = new M_APIResult(M_APIResult.Success);
            int nid = DataConvert.CLng(RequestEx["nid"]);
            //DataTable dt = DBCenter.SelWithField("ZL_Node", "NodeID,NodeName", "ParentID=" + nid);
            DataTable dt = nodeBll.GetNodeListUserShop(nid);
            dt = dt.DefaultView.ToTable(false, "NodeID", "NodeName");
            retMod.result = JsonConvert.SerializeObject(dt);
            return retMod.ToString();
        }

        public int Product_OP(string ids)
        {
            string action = RequestEx["a"];
            if (string.IsNullOrEmpty(ids)) { return Failed; }
            switch (action)
            {
                case "order"://排序本店商品
                    {
                        M_Store_Info storeMod = storeBll.SelModelByUser(mu.UserID);
                        DataTable dt = JsonConvert.DeserializeObject<DataTable>(Request.Form["list"]);
                        foreach (DataRow dr in dt.Rows)
                        {
                            int id = DataConvert.CLng(dr["ID"]);
                            int orderID = DataConvert.CLng(dr["order"]);
                            if (orderID < 0) { orderID = 0; }
                            DBCenter.UpdateSQL("ZL_Commodities", "ComModelID=" + orderID, "ID=" + id + " AND UserShopID=" + storeMod.ID);
                        }
                    }
                    break;
                default:
                    proBll.setproduct(action, ids);
                    break;
            }
            return Success;
        }

        public int Product_Del(string ids)
        {
            if (!String.IsNullOrEmpty(ids))
            {
                proBll.setproduct("recycle", ids);
            }
            return Success;
        }
        #endregion
        #region 优惠券
        
        public IActionResult ArriveManage()
        {
            M_Store_Info storeMod = storeBll.SelModelByUser(mu.UserID);
            PageSetting setting = avBll.SelPage(CPage, PSize, new Filter_Arrive()
            {
                storeID = storeMod.ID
            });
            if (Request.IsAjax()) { return PartialView("ArriveManage_List", setting); }
            else { return View(setting); }
        }
        
        public IActionResult ArriveAdd()
        {
            M_Arrive avMod = avBll.SelReturnModel(Mid);
            if (avMod == null) { avMod = new M_Arrive(); }
            if (avMod.ID > 0) { return View("ArriveEdit", avMod); }
            else { return View("ArriveAdd", avMod); }
        }
        
        public IActionResult Arrive_Add(M_Arrive avMod)
        {
            M_Store_Info storeMod = storeBll.SelModelByUser(mu.UserID);
            if (Mid < 1)//添加优惠券
            {
                avMod.CUser = mu.UserID;
                avMod.StoreID = storeMod.ID;
                avMod.State = DataConvert.CLng(RequestEx["state_chk"]);
                int num = DataConverter.CLng(RequestEx["txtCreateNum"]);
                CommonReturn retMod = avBll.CreateArrive(avMod, num, "1", RequestEx["UserID_Hid"]);
                if (retMod.isok) { return WriteOK("批量添加成功!", "ArriveManage"); }
                else { return WriteErr(retMod.err); }
            }
            else
            {
                avMod = avBll.SelReturnModel(Mid);
                avMod.ArriveName = RequestEx["ArriveName"];
                avMod.Amount = DataConvert.CLng(RequestEx["Amount"]);
                avMod.MinAmount = Convert.ToDouble(RequestEx["MinAmount"]);
                avMod.MaxAmount = Convert.ToDouble(RequestEx["MaxAmount"]);
                avMod.AgainTime = DataConvert.CDate(RequestEx["AgainTime"]);
                avMod.EndTime = DataConvert.CDate(RequestEx["EndTime"]);
                avBll.GetUpdate(avMod);
                return WriteOK("修改成功!", "ArriveManage");
            }
        }
        
        public string Arrive_API()
        {
            M_Store_Info storeMod = storeBll.SelModelByUser(mu.UserID);
            string action = RequestEx["action"];
            string ids = RequestEx["ids"];

            switch (action)
            {
                case "del":
                    //只可删除未被领取的优惠券
                    {
                        if (string.IsNullOrEmpty(ids)) { return Failed.ToString(); }
                        SafeSC.CheckIDSEx(ids);
                        string where = "ID IN (" + ids + ") AND StoreID=" + storeMod.ID + " AND UserID=0";
                        DBCenter.DelByWhere("ZL_Arrive", where);
                    }
                    break;
                case "change":
                    if (string.IsNullOrEmpty(ids)) { return Failed.ToString(); }
                    SafeSC.CheckIDSEx(ids);
                    int state = Convert.ToInt32(RequestEx["state"]);
                    if (state == 0)
                    {
                        //取消激活,只适用于已激活但未被用户领取的优惠券。
                        string where = "ID IN (" + ids + ") AND StoreID=" + storeMod.ID + " AND UserID=0";
                        DBCenter.UpdateSQL("ZL_Arrive", "State=0", where);
                    }
                    else { avBll.UpdateState(ids, state, storeMod.ID); }
                    break;
                case "bind":
                    int uid = Convert.ToInt32(RequestEx["uid"]);
                    avBll.GetUpdateUserIdByIDS(ids, uid, storeMod.ID);
                    break;
                case "unbind":
                    {
                        //\\win10\D\Web\办酒网BAK
                        if (string.IsNullOrEmpty(ids)) { return Failed.ToString(); }
                        SafeSC.CheckIDSEx(ids);
                        avBll.GetUpdateUserIdByIDS(ids, 0, storeMod.ID);
                    }
                    break;
                default:
                    return "[" + action + "]未命中";
            }
            return M_APIResult.Success.ToString();
        }
        #endregion
        //处理用户对商品的评价
        public IActionResult StoreShare()
        {
            M_Store_Info storeMod = storeBll.SelModelByUser(mu.UserID);
            //筛选属于我们店,但未回复的share
            F_Order_Share filter = new F_Order_Share()
            {
                storeID = storeMod.ID,
                pid = DataConvert.CLng(RequestEx["pid"])
            };
            PageSetting setting = shareBll.SelPage(CPage, PSize, filter);
            if (Request.IsAjax())
            {
                if (filter.pid == 0) { return PartialView("StoreShare_List", setting); }
                else
                {
                    //[special],仅显示店主的回复
                    filter.r_uid = mu.UserID;
                    setting = shareBll.SelPage(CPage, PSize, filter);
                    return PartialView("StoreShare_Reply", setting);
                }
            }
            else { return View(setting); }
        }
        //[办酒网临时使用]
        public string Delivery_API()
        {
            string action = RequestEx["action"];
            switch (action)
            {
                case "o_sign"://订单签收
                    {
                        int oid = DataConvert.CLng(RequestEx["oid"]);
                        M_OrderList orderMod = orderBll.SelReturnModel(oid);
                        if (orderMod == null || string.IsNullOrEmpty(orderMod.ExpressNum)) { return "订单不存在,或未指定配送员"; }
                        M_Order_Exp expMod = expBll.SelReturnModel(DataConvert.CLng(orderMod.ExpressNum));
                        if (mu.UserID != DataConvert.CLng(expMod.ExpNo))
                        {
                            return "你无权操作该订单";
                        }
                        if (orderMod.StateLogistics != (int)M_OrderList.ExpEnum.HasSend)
                        {
                            return "订单的物流状态并非已发货";
                        }
                        orderMod.StateLogistics = (int)M_OrderList.ExpEnum.HasReceived;
                        expMod.SignDate = DateTime.Now.ToString();
                        expMod.AdminRemind = "送货员确认";
                        orderBll.UpdateByID(orderMod);
                        expBll.UpdateByID(expMod);
                    }
                    break;
            }
            return Success.ToString();
        }
    }
}