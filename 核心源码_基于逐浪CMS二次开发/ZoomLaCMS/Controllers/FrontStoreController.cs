using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ZoomLa.BLL;
using ZoomLa.BLL.Content;
using ZoomLa.BLL.Shop;
using ZoomLa.Common;
using ZoomLa.Components;
using ZoomLa.Model;
using ZoomLa.Model.Content;
using ZoomLa.Model.Shop;
using ZoomLaCMS.Ctrl;

namespace ZoomLaCMS.Controllers
{
    public class FrontStoreController : Ctrl_Front
    {
        B_Product proBll = new B_Product();

        B_Content conBll = new B_Content();
        B_Node nodeBll = new B_Node();
        B_ModelField mfbll = new B_ModelField();
        B_Store_Style styleBll = new B_Store_Style();
        B_Model modBll = new B_Model();

        public int Cpage { get { return DataConverter.CLng(GetParam("page")); } }
        protected int listnum = -1;
        public void ProductShow() { Response.Redirect("/Shop/" + ItemID + ""); return; }
        public IActionResult Shopindex()
        {
            string IndexDir = SiteConfig.SiteOption.ShopTemplate;
            IndexDir = function.VToP(SiteConfig.SiteOption.TemplateDir + "/" + IndexDir);
            if (!FileSystemObject.IsExist(IndexDir, FsoMethod.File)) { return WriteErr("[产生错误的可能原因：内容信息不存在或未开放!]"); }
            else
            {
                string IndexHtml = FileSystemObject.ReadFile(IndexDir);
                IndexHtml = bll.CreateHtml(IndexHtml, 0, 0, "0");
                return HtmlToClient(IndexHtml);
            }
        }
        public IActionResult Shoplist()
        {
            if (NodeID < 1) { return WriteErr("[产生错误的可能原因：没有指定栏目ID]"); }
            M_Node nodeinfo = nodeBll.GetNodeXML(NodeID);
            if (nodeinfo.IsNull) { return WriteErr("产生错误的可能原因：您访问的内容信息不存在"); }
            string TemplateDir = "";
            if (string.IsNullOrEmpty(nodeinfo.IndexTemplate))
                TemplateDir = nodeinfo.ListTemplateFile;
            else
                TemplateDir = nodeinfo.IndexTemplate;

            if (string.IsNullOrEmpty(TemplateDir)) { return WriteErr("产生错误的可能原因：节点不存在或未绑定模型模板");  }
            else
            {
                TemplateDir =function.VToP(SiteConfig.SiteOption.TemplateDir + "/" + TemplateDir);
                string ContentHtml = FileSystemObject.ReadFile(TemplateDir);
                ContentHtml = this.bll.CreateHtml(ContentHtml, Cpage, ItemID, "1");
                return HtmlToClient(ContentHtml);
            }
        }
        public void ShopSearch()
        {
            Response.Redirect("/Search/SearchList?keyword=&Node=" + NodeID); return;
        }
        public IActionResult StoreContent()
        {
            if (ItemID < 1) { return WriteErr("[产生错误的可能原因：您访问的商品信息不存在！");  }
            M_Product pinfo = proBll.GetproductByid(ItemID);
            if (pinfo == null) { return WriteErr("[产生错误的可能原因：您访问的商品信息不存在！]"); }
            if (pinfo.UserShopID < 1) { Response.Redirect("/Shop/" + ItemID + ""); }
            M_CommonData storeMod = conBll.Store_SelModel(pinfo.UserShopID);
            if (!StoreCheck(storeMod, ref err)) {  return WriteErr(err); }
            M_Store_Style styleMod = styleBll.SelReturnModel(storeMod.DefaultSkins);
            if (styleMod == null || string.IsNullOrEmpty(styleMod.Template_Content)) { return WriteErr("尚未定义模板路径");  }
            string tlppath = styleMod.Template_Content;
            string ContentHtml = SafeSC.ReadFileStr(SiteConfig.SiteOption.TemplateDir + "/" + tlppath);
            ContentHtml = bll.CreateHtml(ContentHtml, 0, ItemID, 0);
            return HtmlToClient(ContentHtml);
        }
        public IActionResult StoreIndex()
        {
            //if (ItemID < 1) {  return WriteErr("店铺ID错误,StoreIndex?id=店铺ID");return; }
            M_CommonData storeMod = conBll.Store_SelModel(ItemID);
            if (!StoreCheck(storeMod, ref err)) { return WriteErr(err); }
            string tlppath = "";
            if (!string.IsNullOrEmpty(storeMod.Template)) { tlppath = storeMod.Template; }
            else
            {
                M_Store_Style styleMod = styleBll.SelReturnModel(storeMod.DefaultSkins);
                if (styleMod != null) { tlppath = styleMod.Template_Index; }

            }
            if (string.IsNullOrEmpty(tlppath)) { return WriteErr("尚未定义模板路径");}
            string ContentHtml = SafeSC.ReadFileStr(SiteConfig.SiteOption.TemplateDir + "/" + tlppath);
            ContentHtml = bll.CreateHtml(ContentHtml, 0, ItemID, 0);
            return HtmlToClient(ContentHtml);
        }
        public IActionResult Storelist()
        {
            //if (ItemID < 1) {  return WriteErr("店铺ID错误,StoreIndex?id=店铺ID"); return; }
            M_CommonData storeMod = conBll.Store_SelModel(ItemID);
            if (!StoreCheck(storeMod, ref err)) { return WriteErr(err);  }
            string tlppath = "";
            M_Store_Style styleMod = styleBll.SelReturnModel(storeMod.DefaultSkins);
            if (styleMod == null || string.IsNullOrEmpty(styleMod.Template_List)) { return WriteErr("尚未定义模板路径"); }
            tlppath = styleMod.Template_List;
            string ContentHtml = SafeSC.ReadFileStr(SiteConfig.SiteOption.TemplateDir + "/" + tlppath);
            ContentHtml = bll.CreateHtml(ContentHtml, 0, ItemID, 0);
            return HtmlToClient(ContentHtml);
        }
        private bool StoreCheck(M_CommonData storeMod, ref string err)
        {
            if (storeMod == null || storeMod.IsNull) { err = "店铺信息不存在"; return false; }
            if (!storeMod.IsStore) { err = "错误,指定的ID并非店铺"; return false; }
            if (storeMod.Status == 0)
            {
                err = "该店铺被关闭了"; return false;
            }
            if (storeMod.Status != 99)
            {
                err = "该店铺还在审核中"; return false;
            }
            return true;
        }
    }
}