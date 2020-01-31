using Newtonsoft.Json;
using System;
using System.Data;
using System.Web;
using Microsoft.AspNetCore.Mvc;
using ZoomLa.BLL;
using ZoomLa.BLL.Content;
using ZoomLa.BLL.Helper;
using ZoomLa.BLL.User;
using ZoomLa.Common;
using ZoomLa.Components;
using ZoomLaCMS.Control;
using ZoomLa.Model;
using ZoomLa.SQLDAL;
using ZoomLa.SQLDAL.SQL;
using ZoomLaCMS.Ctrl;
using Microsoft.AspNetCore.Authorization;
using ZoomLa.BLL.Shop;
using ZoomLa.Model.Shop;
using ZoomLa.Model.Content;
using ZoomLaCMS.Models.Product;
using ZoomLa.Safe;
using System.Text;
using System.IO;

namespace ZoomLaCMS.Areas.User.Controllers
{
    [Area("User")]
    [Authorize(Policy = "User")]
    public class ContentController : Ctrl_User
    {
        B_Favorite favBll = new B_Favorite();
        B_Content conBll = new B_Content();
        B_Node nodeBll = new B_Node();
        B_Model modBll = new B_Model();
        B_ModelField fieldBll = new B_ModelField();
        B_UserPromotions upBll = new B_UserPromotions();
        B_Comment cmtBll = new B_Comment();
        B_Product proBll = new B_Product();
        B_Stock stockBll = new B_Stock();
        B_User_BindPro bindBll = new B_User_BindPro();
        B_Group gpBll = new B_Group();
        B_KeyWord keyBll = new B_KeyWord();
        public int NodeID
        {
            get
            {
                if (ViewBag.NodeID == null) { ViewBag.NodeID = DataConvert.CLng(RequestEx["NodeID"]); }
                return DataConvert.CLng(ViewBag.NodeID);
            }
            set { ViewBag.NodeID = value; }
        }
        public int ModelID
        {
            get
            {
                if (ViewBag.ModelID == null) { ViewBag.ModelID = DataConvert.CLng(RequestEx["ModelID"]); }
                return DataConvert.CLng(ViewBag.ModelID);
            }
            set { ViewBag.ModelID = value; }
        }
        public void Index() { Response.Redirect("MyContent"); }
        public void Default() { Response.Redirect("MyContent"); }
        public IActionResult MyContent()
        {
            string Status = RequestEx["Status"] ?? "";
            DataTable nodeDT = nodeBll.SelByPid(0, true);
            string nodeids = upBll.GetNodeIDS(mu.GroupID);
            if (!string.IsNullOrEmpty(nodeids))
            {
                nodeDT.DefaultView.RowFilter = "NodeID in(" + nodeids + ")";
            }
            else
            {
                nodeDT.DefaultView.RowFilter = "1>2";//无权限，则去除所有
            }
            nodeDT = nodeDT.DefaultView.ToTable();
            C_TreeView treeMod = new C_TreeView()
            {
                NodeID = "NodeID",
                NodeName = "NodeName",
                NodePid = "ParentID",
                DataSource = nodeDT,
                liAllTlp = "<a href='MyContent'>全部内容</a>",
                LiContentTlp = "<a href='MyContent?NodeID=@NodeID'>@NodeName</a>",
                SelectedNode = NodeID.ToString()
            };
            if (NodeID != 0)
            {
                M_Node nod = nodeBll.GetNodeXML(NodeID);
                if (nod.NodeListType == 2)
                {
                    return RedirectToAction("ProductList", new { NodeID = NodeID });//跳转到商城
                }
                string ModeIDList = nod.ContentModel;
                string[] ModelID = ModeIDList.Split(",".ToCharArray(),StringSplitOptions.RemoveEmptyEntries);
                string AddContentlink = "";
                for (int i = 0; i < ModelID.Length; i++)
                {
                    M_ModelInfo infoMod = modBll.SelReturnModel(DataConverter.CLng(ModelID[i]));
                    if (infoMod == null) continue;
                    if (infoMod.ModelType != 5)
                    {
                        AddContentlink += "<a href='AddContent?NodeID=" + NodeID + "&ModelID=" + infoMod.ModelID + "' class='btn btn-info' style='margin-right:5px;'><i class='zi zi_plus'></i> 添加" + infoMod.ItemName + "</a>";
                    }
                }
                ViewBag.addhtml = AddContentlink;
            }
            PageSetting setting = conBll.SelContent(CPage, PSize, NodeID, Status, mu.UserName, RequestEx["skey"]);
            ViewBag.Status = Status;
            ViewBag.treeMod = treeMod;
            return View(setting);
        }
        public IActionResult ShowContent()
        {
            M_CommonData conMod = conBll.SelReturnModel(Mid);
            if (conMod == null) { return WriteErr("内容不存在"); return Content(""); }
            if (!mu.UserName.Equals(conMod.Inputer)) { return WriteErr("你无权查看该内容"); return Content(""); }
            M_Node nodeMod = nodeBll.SelReturnModel(conMod.NodeID);
            DataTable contentDT = conBll.GetContentByItems(conMod.TableName, conMod.GeneralID);
            ViewBag.nodeMod = nodeMod;
            //ViewBag.modelhtml = fieldBll.InputallHtml(conMod.ModelID, conMod.NodeID, new ModelConfig()
            //{
            //    ValueDT = contentDT,
            //    Mode = ModelConfig.SMode.PreView
            //});
            return View(conMod);
        }
        public IActionResult AddContent()
        {
            M_CommonData Cdata = new M_CommonData();
            if (Mid > 0)
            {
                Cdata = conBll.GetCommonData(Mid);
            }
            else { Cdata.NodeID = DataConvert.CLng(RequestEx["NodeID"]); Cdata.ModelID = DataConvert.CLng(RequestEx["ModelID"]); }

            if (Cdata.ModelID < 1 && Cdata.NodeID < 1) { return WriteErr("参数错误");  }
            M_ModelInfo model = modBll.SelReturnModel(Cdata.ModelID);
            M_Node nodeMod = nodeBll.SelReturnModel(Cdata.NodeID);
            if (Mid > 0)
            {
                if (mu.UserName != Cdata.Inputer) { return WriteErr("不能编辑不属于自己输入的内容!"); return Content(""); }
                //DataTable dtContent = conBll.GetContent(Mid);
                //ViewBag.modelhtml = fieldBll.InputallHtml(Cdata.ModelID, Cdata.NodeID, new ModelConfig()
                //{
                //    Source = ModelConfig.SType.UserContent,
                //    ValueDT = dtContent
                //});
            }
            //else
            //{
            //    ViewBag.modelhtml = fieldBll.InputallHtml(ModelID, NodeID, new ModelConfig()
            //    {
            //        Source = ModelConfig.SType.UserContent
            //    });
            //}
            ViewBag.ds = fieldBll.SelByModelID(Cdata.ModelID, true);
            ViewBag.op = (Mid < 1 ? "添加" : "修改") + model.ItemName;
            ViewBag.nodeMod = nodeMod;
            ViewBag.tip = "向 <a href='MyContent?NodeId=" + nodeMod.NodeID + "'>[" + nodeMod.NodeName + "]</a> 节点" + ViewBag.op;
            return View(Cdata);
        }
        #region product

        public IActionResult ProductList()
        {
            int NodeID = DataConverter.CLng(RequestEx["NodeID"]);
            int Recycler = DataConverter.CLng(RequestEx["Recycler"]);
            PageSetting setting = proBll.U_SPage(CPage, PSize, mu.UserID, NodeID, Recycler);
            if (Request.IsAjaxRequest()) { return PartialView("ProductList_List", setting); }
            DataTable nodeDT = nodeBll.SelByPid(0, true);
            nodeDT = nodeDT.DefaultView.ToTable();
            C_TreeView treeMod = new C_TreeView()
            {
                NodeID = "NodeID",
                NodeName = "NodeName",
                NodePid = "ParentID",
                DataSource = nodeDT,
                liAllTlp = "<a href='MyContent'>全部内容</a>",
                LiContentTlp = "<a href='MyContent?NodeID=@NodeID'>@NodeName</a>",
                SelectedNode = NodeID.ToString()
            };
            ViewBag.treeMod = treeMod;
            string AddContentlink = "";
            if (NodeID > 0)
            {
                M_Node nodeMod = nodeBll.GetNodeXML(NodeID);
                string[] ModelID = nodeMod.ContentModel.Split(',');
                for (int i = 0; i < ModelID.Length; i++)
                {

                    AddContentlink = AddContentlink + "<input name=\"btn" + i.ToString() + "\" class=\"btn btn-primary\" type=\"button\" value=\"添加" + modBll.GetModelById(DataConverter.CLng(ModelID[i])).ItemName + "\" onclick=\"javascript:window.location.href='AddProduct?ModelID=" + ModelID[i] + "&NodeID=" + this.NodeID + "';\" />&nbsp;&nbsp;";
                    if (modBll.GetModelById(DataConverter.CLng(ModelID[i])).Islotsize)
                    {
                        AddContentlink = AddContentlink + "<input name=\"btn" + i.ToString() + "\" class=\"btn btn-primary\"  type=\"button\" value=\"批量添加" + modBll.GetModelById(DataConverter.CLng(ModelID[i])).ItemName + "\" onclick=\"javascript:window.location.href='Release?ModelID=" + ModelID[i] + "&NodeID=" + this.NodeID + "';\" />&nbsp;&nbsp;";
                    }
                }
            }
            ViewBag.addlink = AddContentlink;
            ViewBag.Recycler = Recycler;
            return View(setting);
        }
        public IActionResult AddProduct()
        {
            VM_Product vm = new VM_Product();
            if (Mid < 1)
            {
                if (ModelID < 1) { return WriteErr("没有指定要添加内容的模型ID!");  }
                if (NodeID < 1) { return WriteErr("没有指定要添加内容的栏目节点ID!");  }
                vm.proMod = new M_Product() { Stock = 10, Rateset = 1, Dengji = 3 };
                vm.NodeID = NodeID;
                vm.ModelID = ModelID;
                vm.proMod.ProCode = B_Product.GetProCode();
            }
            else
            {
                vm.proMod = proBll.GetproductByid(Mid);
                vm.NodeID = vm.proMod.Nodeid;
                vm.ModelID = vm.proMod.ModelID;
                vm.ValueDT = proBll.GetContent(vm.proMod.TableName, vm.proMod.ItemID);
                if (!string.IsNullOrEmpty(vm.proMod.BindIDS))//捆绑商品
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
            DataTable table = new DataTable();
            M_Product proMod = FillProductModel(ref table, proBll.GetproductByid(Mid));
            if (Mid < 1)
            {
                proMod.ID = proBll.Add(table, proMod);
                IsAddStock(proMod, DataConvert.CLng(RequestEx["Stock"]));
                //多区域价格
                //SqlParameter[] sp = new SqlParameter[] { new SqlParameter("guid", RequestEx["ProGuid"]) };
                //SqlHelper.ExecuteSql("UPDATE ZL_Shop_RegionPrice SET [ProID]=" + proMod.ID + " WHERE [Guid]=@guid", sp);
            }
            else
            {
                proBll.Update(table, proMod);
            }
            return WriteOK("操作成功", "ProductList?NodeID=" + proMod.Nodeid); 
        }
        
        public IActionResult Product_AddToNew()
        {
            DataTable table = new DataTable();
            M_Product proMod = FillProductModel(ref table);
            //------------------
            proMod.AddUser = mu.UserName;
            proMod.Stock = 0;
            proMod.AddTime = DateTime.Now;
            proMod.UpdateTime = DateTime.Now;
            proMod.ID = proBll.Add(table, proMod);
            //------------------
            IsAddStock(proMod, DataConvert.CLng(RequestEx["Stock"]));
            return WriteOK("操作成功", "ProductList?NodeID=" + proMod.Nodeid); 
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
            proMod.PointVal = DataConverter.CLng(RequestEx["PointVal_T"]);
            proMod.Proinfo = RequestEx["Proinfo"];
            proMod.Procontent = RequestEx["Procontent"];
            proMod.Clearimg = RequestEx["txt_Clearimg"];
            proMod.Thumbnails = RequestEx["txt_Thumbnails"];
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
        public int Product_Del(string ids)
        {
            if (!String.IsNullOrEmpty(ids))
            {
                proBll.setproduct("recycle", ids);
            }
            return Success;
        }
        public int Product_Recover(string ids)
        {
            if (!String.IsNullOrEmpty(ids))
            {
                proBll.setproduct("recover", ids);
            }
            return Success;
        }
        public int Product_RealDel(string ids)
        {
            if (!String.IsNullOrEmpty(ids))
            {
                proBll.RealDelByIDS(ids);
            }
            return Success;
        }
        #endregion
        public IActionResult EditContent()
        {
            int gid = DataConvert.CLng(RequestEx["GeneralID"]);
            if (gid < 1) { gid = DataConvert.CLng(RequestEx["ID"]); }
            if (gid < 1) { return WriteErr("未指定需要修改的内容");  }
            return RedirectToAction("AddContent",new {ID=gid });
        }
        //其不是根据名称,而是根据顺序来取值
        public PartialViewResult Content_Data()
        {
            string Status = RequestEx["Status"] ?? "";
            PageSetting setting = conBll.SelContent(CPage, PSize, NodeID, Status, mu.UserName, RequestEx["skey"]);
            return PartialView("MyContent_List", setting);
        }
        [HttpPost]
        
        public IActionResult Content_Add()
        {
            M_CommonData CData = new M_CommonData();
            if (Mid > 0)
            {
                CData = conBll.GetCommonData(Mid);
                if (!CData.Inputer.Equals(mu.UserName)) { return WriteErr("你无权修改该内容");  }
            }
            else
            {
                CData.NodeID = NodeID;
                CData.ModelID = ModelID;
                CData.TableName = modBll.SelReturnModel(CData.ModelID).TableName;
            }
            M_Node nodeMod = nodeBll.SelReturnModel(CData.NodeID);
            DataTable dt = fieldBll.SelByModelID(CData.ModelID, false);
            DataTable table;
            table = Call.GetDTFromMVC(dt, Request);
            CData.Title = RequestEx["title"];
            CData.Subtitle = RequestEx["Subtitle"];
            CData.PYtitle = RequestEx["PYtitle"];
            CData.TagKey = RequestEx["tabinput"];
            CData.Status = nodeMod.SiteContentAudit;
            string parentTree = "";
            CData.TitleStyle = RequestEx["TitleStyle"];
            CData.TopImg = RequestEx["topimg_hid"];//首页图片
            if (Mid > 0)//修改内容
            {
                conBll.UpdateContent(table, CData);
            }
            else
            {
                CData.FirstNodeID = nodeBll.SelFirstNodeID(CData.NodeID, ref parentTree);
                CData.ParentTree = parentTree;
                CData.Inputer = mu.UserName;
                CData.SuccessfulUserID = mu.UserID;
                CData.GeneralID = conBll.AddContent(table, CData);//插入信息给两个表，主表和从表:CData-主表的模型，table-从表
            }
            if (Mid < 1 && SiteConfig.UserConfig.InfoRule > 0)//添加时增加积分
            {
                buser.AddMoney(mu.UserID, SiteConfig.UserConfig.InfoRule, M_UserExpHis.SType.Point, "添加内容:" + CData.Title + "增加积分");
            }
            return WriteOK("操作成功!", "MyContent?NodeID=" + CData.NodeID); 
        }
        public int Content_Del(string ids)
        {
            conBll.UpdateStatus(ids, (int)ZLEnum.ConStatus.Recycle, mu.UserName);
            return Success;
        }
        public int Content_RealDel(string ids)
        {
            conBll.DelContent(ids, mu.UserName);
            return Success;
        }
        public int Content_Recover(string ids)
        {
            if (!string.IsNullOrEmpty(ids))
            {
                conBll.UpdateStatus(ids, 0);
            }
            return Success;
        }
        public IActionResult FileBuyManage()
        {
            B_Content_FileBuy buyBll = new B_Content_FileBuy();
            PageSetting setting = buyBll.SelPage(CPage, PSize, mu.UserID);
            if (Request.IsAjaxRequest())
            {
                return PartialView("FileBuyManage_List", setting);
            }
            else { return View(setting); }
        }
        #region 收藏
        public IActionResult AddToFav()
        {
            M_Favorite favMod = new M_Favorite();
            favMod.InfoID = DataConvert.CLng(RequestEx["infoID"]);
            favMod.Owner = mu.UserID;
            favMod.AddDate = DateTime.Now;
            favMod.FavoriType = DataConvert.CLng(RequestEx["type"], 1);
            //---------------------------
            favMod.FavItemID = "";
            favMod.Title = HttpUtility.HtmlEncode(RequestEx["title"]);
            favMod.FavUrl = RequestEx["url"];
            switch (favMod.FavoriType)
            {
                case 1:
                case 3:
                    {
                        M_CommonData conMod = conBll.SelReturnModel(favMod.InfoID);
                        if (conMod == null) { err = "内容ID[" + favMod.InfoID + "]不存在";break; }
                        if (string.IsNullOrEmpty(favMod.Title))
                        {
                            favMod.Title = conMod.Title;
                        }
                    }
                    break;
                case 2:
                    {
                        M_Product proMod = proBll.GetproductByid(favMod.InfoID);
                        if (proMod == null) { err = "商品不存在";break; }
                        if (string.IsNullOrEmpty(favMod.Title))
                        {
                            favMod.Title = proMod.Proname;
                        }
                    }
                    break;
                case 4:
                    {
                        B_Ask askBll = new B_Ask();
                        M_Ask askMod = askBll.SelReturnModel(favMod.InfoID);
                        if (askMod == null) { err = "问题不存在";break; }
                        if (string.IsNullOrEmpty(favMod.Title))
                        {
                            favMod.Title = StringHelper.SubStr(askMod.Qcontent);
                        }
                    }
                    break;
                case 5:
                    {
                        B_Baike bkBll = new B_Baike();
                        M_Baike bkMod = bkBll.SelReturnModel(favMod.InfoID);
                        if (bkMod == null) { err = "百科不存在";break; }
                        if (string.IsNullOrEmpty(favMod.Title))
                        {
                            favMod.Title = bkMod.Tittle;
                        }
                    }
                    break;
            }
            if (string.IsNullOrEmpty(favMod.Title)) { favMod.Title = "无标题"; }
            if (favMod.InfoID < 1) { err = "未指定内容ID"; }
            else if (favMod.Owner < 1) { err = "用户未登录"; }
            else { favBll.insert(favMod); }
            ViewBag.err = err;
            return View();
        }
        public IActionResult MyFavori()
        {
            PageSetting setting = favBll.SelPage(CPage, PSize, mu.UserID, DataConvert.CLng(RequestEx["type"], -100));
            return View(setting);
        }
        public PartialViewResult Favour_Data()
        {
            PageSetting setting = favBll.SelPage(CPage, PSize, mu.UserID, DataConvert.CLng(RequestEx["type"], -100));
            return PartialView("MyFavori_List", setting);
        }
        public int Favour_Del(string ids)
        {
            favBll.DelByIDS(ids, mu.UserID);
            return Success;
        }
        #endregion
        #region 评论
        public IActionResult MyComment()
        {
            PageSetting setting = cmtBll.SelPage(CPage, PSize, NodeID, 0, mu.UserID);
            return View(setting);
        }
        public int Comment_Del(string ids)
        {
            cmtBll.U_Del(mu.UserID, ids);
            return Success;
        }
        #endregion
        #region MarkDown
        B_Content_MarkDown mdBll = new B_Content_MarkDown();
        [Route("/Plugins/Markdown")]
        public IActionResult MDEditor()
        {
            M_Content_MarkDown mdMod = new M_Content_MarkDown();
            if (Mid > 0)
            {
                mdMod = mdBll.SelReturnModel(Mid);
                if (mdMod == null) {  return WriteErr("文档不存在"); }
                if (mdMod.UserID != mu.UserID && mdMod.UserID != 0) {  return WriteErr("你无权访问该文档"); }
                //Title_T.Text = mdMod.Title;
            }
            else
            {
                mdMod.Content_MD = ZoomLa.BLL.SafeSC.ReadFileStr("/Plugins/Markdown/example.txt");
            }
            //content_md.Value = mdMod.Content_MD;
            return View(mdMod);
        }
        public IActionResult MDEditor_Submit()
        {
            M_Content_MarkDown mdMod = new M_Content_MarkDown();
            if (Mid > 0) { mdMod = mdBll.SelReturnModel(Mid); }
            mdMod.Content_MD = GetParam("content_md");
            mdMod.Content = Request.Form["editormd-html-code"];
            mdMod.UserID = mu.UserID;
            mdMod.UserName = mu.UserName;
            mdMod.Title = GetParam("Title_T");
            mdMod.Thumbnail = "";
            if (mdMod.ID > 0) { mdBll.UpdateByID(mdMod); }
            else { mdMod.ID = mdBll.Insert(mdMod); }
            //View.aspx?ID="+mdMod.ID
            return WriteOK("操作成功", "/User/Content/Markdown");
        }
        public IActionResult MDEditor_API()
        {
            string action = GetParam("action");
            string result = "";
            switch (action)
            {
                case "down":
                    {
                        string content = RequestEx["content"];
                        Stream sm = IOHelper.BytesToStream(Encoding.UTF8.GetBytes(content));
                        //var memi = provider.Mappings[fileExt];
                        return File(sm, "text/plain", "content.md");
                    }
                case "upload"://上传图片
                    {
                        var file = Request.Form.Files["editormd-image-file"];
                        string vpath = ZLHelper.GetUploadDir_Anony("content", "md");
                        string fname = function.GetRandomString(10) + "." + GetImgExt(file.FileName);
                        string url = SafeC.SaveFile(vpath, fname, file.OpenReadStream(), (int)file.Length);
                        result = JsonHelper.GetJson(
                            new string[] { "success", "message", "url" },
                            new string[] { "1", "上传成功", url });
                    }
                    break;
            }
            return Content(result);
        }
        public IActionResult MarkDown()
        {
            PageSetting setting = new PageSetting();
            setting = mdBll.SelPage(CPage, PSize, new Com_Filter() { uids = mu.UserID.ToString() });
            if (Request.IsAjax()) { return PartialView("Markdown_List", setting); }
            return View(setting);
        }
        //预览文档内容
        public IActionResult MDView()
        {
            M_Content_MarkDown mdMod = mdBll.SelReturnModel(Mid);
            if (mdMod == null) { mdMod = new M_Content_MarkDown(); mdMod.Content = "内容不存在"; }
            //Response.Write(mdMod.Content);
            //Response.Flush();
            //Response.End();
            return Content(mdMod.Content);
        }
        public string MarkDown_Del(string ids)
        {
            mdBll.Del(ids);
            return Success.ToString();
        }
        private string GetImgExt(string fname, string def = "jpg")
        {
            string[] allows = "jpg,png,gif".Split('|');
            if (string.IsNullOrEmpty(fname) || fname.IndexOf(".") < 1) { return def; }
            string ext = fname.Split('.')[1].ToLower();
            foreach (string allow in allows)
            {
                if (ext.Equals(allow)) { return allow; }
            }
            return def;
        }
        #endregion
    }
}
