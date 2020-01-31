using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using ZoomLa.BLL;
using ZoomLa.Model;
using ZoomLa.SQLDAL;
using ZoomLa.BLL.Shop;
using Newtonsoft.Json;
using ZoomLa.Common;

using ZoomLa.Model.Shop;
using ZoomLa.BLL.Content;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc;

namespace ZoomLaCMS.Models.Product
{
    //用于添加|修改商品页
    public class VM_Product
    {
        private B_Group gpBll = new B_Group();
        private B_Node nodeBll = new B_Node();
        private B_Product proBll=new B_Product();
        private B_Shop_FareTlp fareBll=new B_Shop_FareTlp();
        private B_ModelField fieldBll=new B_ModelField();
        public M_Product proMod = null;
        public M_Node nodeMod = null;
        public M_Shop_RegionPrice regionMod = new M_Shop_RegionPrice();
        public int NodeID = 0, ModelID = 0;
        //新建时为Guid,而后则为商品的ID
        public string ProGuid = "";
        public string bindList = "";
        public string groupList = "";//会员组信息,给予前端JS调用
        //public string modelHtml = "";
        public DataTable ValueDT = new DataTable();
        public DataRow ValueDR { get { if (ValueDT == null || ValueDT.Rows.Count < 1) { return null; } else { return ValueDT.Rows[0]; } } }
        private DataTable gpdt = null;
        //-------厂商|商标
        private M_Trademark _brand = null;
        private M_Manufacturers _Producer = null;
        public M_Trademark Brand
        {
            get
            {
                if (_brand == null)
                {
                    _brand = new B_Trademark().SelReturnModel(DataConvert.CLng(proMod.Brand));
                    if (_brand == null) { _brand = new M_Trademark(); }
                }
                return _brand;
            }
        }
        public M_Manufacturers Producer
        {
            get
            {
                if (_Producer == null)
                {
                    _Producer = new B_Manufacturers().SelReturnModel(DataConvert.CLng(proMod.Producer));
                    if (_Producer == null) { _Producer = new M_Manufacturers(); }
                }
                return _Producer;
            }
        }
        // 会员价格列表
        public DataTable gpriceDT
        {
            get
            {
                if (gpdt == null)
                {
                    gpdt = gpBll.GetGroupList();
                    //附加会员价,限购数,最低购买数等限制
                    gpdt.Columns.Add(new DataColumn("price", typeof(string)));
                    gpdt.Columns.Add(new DataColumn("quota", typeof(string)));
                    gpdt.Columns.Add(new DataColumn("downquota", typeof(string)));
                    if (proMod != null && proMod.ID > 0)
                    {
                        if (proMod.UserType == 2 && proMod.UserPrice.Contains("["))
                        {
                            DataTable dt = JsonConvert.DeserializeObject<DataTable>(proMod.UserPrice);
                            if (dt.Columns.Contains("price")) { dt.Columns["price"].ColumnName = "value"; }
                            foreach (DataRow dr in dt.Rows)
                            {
                                DataRow[] gps = gpdt.Select("GroupID='" + dr["gid"] + "'");
                                if (gps.Length > 0) { gps[0]["price"] = DataConvert.CDouble(dr["value"]).ToString("F2"); }
                            }
                        }
                        if (proMod.Quota_Json.Contains("["))
                        {
                            DataTable dt = JsonConvert.DeserializeObject<DataTable>(proMod.Quota_Json);
                            foreach (DataRow dr in dt.Rows)
                            {
                                DataRow[] gps = gpdt.Select("GroupID='" + dr["gid"] + "'");
                                if (gps.Length > 0) { gps[0]["quota"] = DataConvert.CLng(dr["value"]); }
                            }
                        }
                        if (proMod.DownQuota_Json.Contains("["))
                        {
                            DataTable dt = JsonConvert.DeserializeObject<DataTable>(proMod.DownQuota_Json);
                            foreach (DataRow dr in dt.Rows)
                            {
                                DataRow[] gps = gpdt.Select("GroupID='" + dr["gid"] + "'");
                                if (gps.Length > 0) { gps[0]["downquota"] = DataConvert.CLng(dr["value"]); }
                            }
                        }
                    }
                }//if end;
                return gpdt;
            }
        }
        //运费
        private DataTable _faredt = null;
        public DataTable fareDT
        {
            get
            {
                if (_faredt == null) { _faredt = fareBll.Sel(); }
                return _faredt;
            }
            set { _faredt = value; }
        }
        public VM_Product(M_Product proMod, HttpRequest Request)
        {
            groupList = JsonConvert.SerializeObject(DBCenter.SelWithField("ZL_Group", "GroupID,GroupName"));
            fareDT = fareBll.Sel();
            this.proMod = proMod;
            if (proMod.ID > 0)
            {
                this.NodeID = proMod.Nodeid;
                this.ModelID = proMod.ModelID;
                this.ProGuid = proMod.ID.ToString();
                //捆绑商品
                if (!string.IsNullOrEmpty(proMod.BindIDS))
                {
                    DataTable dt = proBll.SelByIDS(proMod.BindIDS, "id,Thumbnails,Proname,LinPrice");
                    bindList = JsonConvert.SerializeObject(dt);
                }
                //if (!string.IsNullOrEmpty(proMod.TableName))
                //{
                //    DataTable valueDT = proBll.Getmodetable(proMod.TableName.ToString(), proMod.ItemID);
                //    if (valueDT != null && valueDT.Rows.Count > 0)
                //    {
                //        modelHtml = fieldBll.InputallHtml(ModelID, NodeID, new ModelConfig() { ValueDT = valueDT });
                //    }
                //}
            }
            else
            {
                this.NodeID = DataConvert.CLng(Request.GetParam("NodeID"));
                this.ModelID = DataConvert.CLng(Request.GetParam("ModelID"));
                this.ProGuid = System.Guid.NewGuid().ToString();
                this.proMod.ProCode = B_Product.GetProCode();
                //modelHtml = fieldBll.InputallHtml(ModelID, NodeID, new ModelConfig() { Source = ModelConfig.SType.Admin });
            }
            nodeMod = nodeBll.SelReturnModel(NodeID);
        }
        public VM_Product() 
        {
            groupList = JsonConvert.SerializeObject(DBCenter.SelWithField("ZL_Group", "GroupID,GroupName"));
        }
        /// <summary>
        /// 返回店铺下拉列表
        /// </summary>
        public SelectList GetStoreList(string seled)
        {
            B_Content conBll = new B_Content();
            DataTable dt = conBll.Store_Sel();
            return MVCHelper.ToSelectList(dt, "Title", "GeneralID", seled);
        }
    }
}