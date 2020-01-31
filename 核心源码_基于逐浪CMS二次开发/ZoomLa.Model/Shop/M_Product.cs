using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Text;

namespace ZoomLa.Model.Shop
{
    public class M_Product : M_Base
    {
        public int JisuanFs
        {
            get;
            set;
        }
        //--------------------------------------
        public int ExpRemind { get; set; }
        /// <summary>
        /// 商品所属促销分组(对应表ID)
        /// </summary>
        public int ParentID { get; set; }
        #region 属性定义
        public int ID
        {
            get;
            set;
        }
        public int OrderID
        {
            get;
            set;
        }
        /// <summary>
        /// 商品类别
        /// 0:普通,2:促销组合(列表中不抽出)
        /// </summary>
        public int Class
        {
            get;
            set;
        }
        public int Nodeid
        {
            get;
            set;
        }
        public int ModelID
        {
            get;
            set;
        }
        /// <summary>
        /// 商品编号
        /// </summary>
        public string ProCode
        {
            get;
            set;
        }
        /// <summary>
        /// 条形码
        /// </summary>
        public string BarCode
        {
            get;
            set;
        }
        /// <summary>
        /// 商品名称
        /// </summary>
        public string Proname
        {
            get;
            set;
        }
        /// <summary>
        /// 关键字
        /// </summary>
        public string Kayword
        {
            get;
            set;
        }
        /// <summary>
        /// 单位
        /// </summary>
        public string ProUnit
        {
            get;
            set;
        }
        /// <summary>
        /// 重量
        /// </summary>
        public double Weight
        {
            get;
            set;
        }
        /// <summary>
        ///商品类型[1:正常|店铺,2:特价,3:积分,4:团购,5:云购,6:IDC,7:旅游,8:酒店,9:机票]
        /// </summary>
        public int ProClass
        {
            get;
            set;
        }
        /// <summary>
        /// ZC:正常,TJ:特价,JF:积分,TG:团购,YG:云购,FW:服务
        /// </summary>
        public enum ClassType
        {
            ZC = 1, TJ = 2, JF = 3, TG = 4, YG = 5, IDC = 6, LY = 7, JD = 8, JP = 9
        }
        /// <summary>
        /// 商品属性
        /// </summary>
        public int Properties
        {
            get;
            set;
        }
        /// <summary>
        /// 销售操作[1-允许销售,2-不允许]
        /// </summary>
        public int Sales
        {
            get;
            set;
        }
        /// <summary>
        /// 商品简介
        /// </summary>
        public string Proinfo
        {
            get;
            set;
        }
        /// <summary>
        /// 商品详细介绍,促销组合的商品信息
        /// </summary>
        public string Procontent
        {
            get;
            set;
        }
        /// <summary>
        /// 商品清晰图
        /// </summary>
        public string Clearimg
        {
            get;
            set;
        }
        /// <summary>
        /// 商品缩略图
        /// </summary>
        public string Thumbnails
        {
            get;
            set;
        }
        private string m_thumbnails = "";
        /// <summary>
        /// 缩略图路径,对m_thumbnails封装
        /// </summary>
        public string ThumbPath
        {
            get
            {
                if (!string.IsNullOrEmpty(m_thumbnails))
                    m_thumbnails = "/" + m_thumbnails;
                else { m_thumbnails = "/UploadFiles/nopic.svg"; }
                return m_thumbnails;
            }
        }
        /// <summary>
        /// 厂商
        /// </summary>
        public string Producer
        {
            get;
            set;
        }
        /// <summary>
        /// 商标
        /// </summary>
        public string Brand
        {
            get;
            set;
        }
        /// <summary>
        /// 1-缺货允许购买，0-不允许购买
        /// </summary>
        public int Allowed
        {
            get;
            set;
        }
        /// <summary>
        /// 限制购买数量[购买上限]-1不限制
        /// </summary>
        public int Quota
        {
            get;
            set;
        }
        /// <summary>
        /// 最低购买数量[购买下限]-1不限制
        /// </summary>
        public int DownQuota
        {
            get;
            set;
        }
        /// <summary>
        /// 库存数量
        /// </summary>
        public int Stock
        {
            get;
            set;
        }
        /// <summary>
        /// 最低库存数量[报警]
        /// </summary>
        public int StockDown
        {
            get;
            set;
        }
        /// <summary>
        /// 税率设置,百分数%
        /// </summary>
        public double Rate
        {
            get;
            set;
        }
        /// <summary>
        /// 税率设置
        /// </summary>
        public int Rateset
        {
            get;
            set;
        }
        /// <summary>
        /// 推荐等级
        /// </summary>
        public int Dengji
        {
            get;
            set;
        }
        /// <summary>
        /// 市场价
        /// </summary>
        public double ShiPrice
        {
            get;
            set;
        }
        /// <summary>
        /// 零售价
        /// </summary>
        public double LinPrice
        {
            get;
            set;
        }
        /// <summary>
        /// 会员价
        /// </summary>
        public string MemberPrice
        {
            get;
            set;
        }
        /// <summary>
        /// [disuse]
        /// </summary>
        public string ActPrice
        {
            get;
            set;
        }
        /// <summary>
        /// 是否允许批发 1:是
        /// </summary>
        public int Wholesales
        {
            get;
            set;
        }
        /// <summary>
        /// 零售
        /// </summary>
        public int Wholesaleone
        {
            get;
            set;
        }
        /// <summary>
        /// 多价格信息
        /// </summary>
        public string Wholesalesinfo
        {
            get;
            set;
        }
        /// <summary>
        /// 促销方案
        /// </summary>
        public string Preset
        {
            get;
            set;
        }
        /// <summary>
        /// 购物积分
        /// </summary>
        public int Integral
        {
            get;
            set;
        }
        /// <summary>
        /// 优惠率%数,不允许负数,不允许超过100,优惠5%,则减价5%
        /// </summary>
        public int Recommend
        {
            get;
            set;
        }
        /// <summary>
        /// 更新时间
        /// </summary>
        public DateTime UpdateTime
        {
            get;
            set;
        }
        /// <summary>
        /// 模型模板
        /// </summary>
        public string ModeTemplate
        {
            get;
            set;
        }
        /// <summary>
        /// 添加人
        /// </summary>
        public string AddUser
        {
            get;
            set;
        }
        private int _downcar = 1;
        /// <summary>
        /// 购买倍数(商品必须是该值的倍数)
        /// </summary>
        public int DownCar
        {
            get { return _downcar < 1 ? 1 : _downcar; }
            set { _downcar = value; }
        }
        /// <summary>
        /// 添加时间
        /// </summary>
        public DateTime AddTime
        {
            get;
            set;
        }
        /// <summary>
        /// 模型表名
        /// </summary>
        public string TableName
        {
            get;
            set;
        }
        /// <summary>
        /// 1:已审核,0:未审核
        /// </summary>
        public int Istrue
        {
            get;
            set;
        }
        /// <summary>
        /// [未用][备用字段]
        /// </summary>
        public int Isgood
        {
            get;
            set;
        }
        /// <summary>
        /// 新品
        /// </summary>
        public int Isnew
        {
            get;
            set;
        }
        /// <summary>
        /// 热销
        /// </summary>
        public int Ishot
        {
            get;
            set;
        }
        /// <summary>
        /// 精品
        /// </summary>
        public int Isbest
        {
            get;
            set;
        }
        /// <summary>
        /// 从表ID
        /// </summary>
        public int ItemID
        {
            get;
            set;
        }
        /// <summary>
        /// 改为店铺ShopOrder,只用于店铺中商品排序
        /// </summary>
        public int ComModelID
        {
            get; set;
        }
        /// <summary>
        /// 点击数
        /// </summary>
        public int AllClickNum
        {
            get;
            set;
        }
        /// <summary>
        /// 会员特选商品
        /// </summary>
        public int Largess
        {
            get;
            set;
        }
        /// <summary>
        /// 购物数量送积分
        /// </summary>
        public int IntegralNum
        {
            get;
            set;
        }
        /// <summary>
        /// 商品属性,存储是否返修,竞猜,秒杀等字段等,repair,return
        /// </summary>
        public string GuessXML
        {
            get;
            set;
        }
        /// <summary>
        /// 第一级节点
        /// </summary>
        public int FirstNodeID
        {
            get;
            set;
        }
        /// <summary>
        /// 赠送积分
        /// </summary>
        public double PointVal
        {
            get;
            set;
        }
        /// <summary>
        /// 是否设为标准:0为否,1为是
        /// </summary>
        public int isStand
        {
            get;
            set;
        }
        /// <summary>
        /// 店铺ID,StoreID
        /// </summary>
        public int UserShopID
        {
            get;
            set;
        }
        /// <summary>
        /// 预订价
        /// </summary>
        public double BookPrice
        {
            get;
            set;
        }
        /// <summary>
        /// 计价类型：0:不使用,1:为会员价,2:按会员组定价
        /// 后期扩展支持批发价等
        /// </summary>
        public int UserType
        {
            get;
            set;
        }
        /// <summary>
        /// 如果按会员组打折,格式为:会员组ID|价格
        /// </summary>
        public string UserPrice
        {
            get;
            set;
        }
        /// <summary>
        /// 是否进入回收站
        /// </summary>
        public bool Recycler
        {
            get;
            set;
        }
        public int UserID { get; set; }
        ///<summary>
        ///运费模板ID
        /// </summary>
        public string FarePrice
        { get; set; }
        /// <summary>
        /// 捆绑商品的IDS
        /// </summary>
        public string BindIDS { get; set; }
        /// <summary>
        /// IDC商品价格
        /// </summary>
        public string IDCPrice { get; set; }
        public string Quota_Json { get; set; }
        public string DownQuota_Json { get; set; }
        public string ShortProName { get; set; }
        #endregion
        public string SpecialID = "";
        public string Addon = "";
        //移除
        public string LinPrice_Json { get; set; }
        public M_Product()
        {
            this.AddTime = DateTime.Now;
            this.UpdateTime = DateTime.Now;
            this.Recycler = false;
            this.ProUnit = "件";
            this.Dengji = 3;//默认三星推荐
            this.Sales = 1;
            this.Isnew = 1;
            this.ParentID = 0;
            this.Class = 0;
            this.ProClass = 1;
            this.Properties = 0;
            this.Isgood = 0;
            this.UserShopID = 0;
            this.Allowed = 1;
            this.Istrue = 1;
        }
        public override string TbName { get { return "ZL_Commodities"; } }
        public override string[,] FieldList()
        {
            string[,] Tablelist = {
                                {"ID","Int","4"},
                                {"Class","Int","4"},
                                {"Nodeid","Int","4"},
                                {"ModelID","Int","4"},
                                {"ProCode","NVarChar","50"},
                                {"BarCode","NVarChar","50"},
                                {"Proname","NVarChar","500"},
                                {"Kayword","NVarChar","255"},
                                {"ProUnit","NVarChar","50"},
                                {"Weight","Money","8"},
                                {"ProClass","Int","4"},
                                {"Properties","Int","4"},
                                {"Sales","Int","4"},
                                {"Proinfo","NVarChar","4000"},
                                {"Procontent","Text","10000"},
                                {"Clearimg","NVarChar","4000"},
                                {"Thumbnails","NVarChar","4000"},
                                {"Producer","NVarChar","4000"},
                                {"Brand","NVarChar","4000"},
                                {"Allowed","Int","4"},
                                {"Quota","VarChar","8000"},
                                {"DownQuota","VarChar","8000"},
                                {"Stock","Int","4"},
                                {"StockDown","Int","4"},
                                {"Rate","Money","32"},
                                {"Rateset","Int","4"},
                                {"Dengji","Int","4"},
                                {"ShiPrice","Money","32"},
                                {"LinPrice","Money","32"},
                                {"Wholesales","Int","4"},
                                {"Wholesaleone","Int","4"},
                                {"Preset","NVarChar","4000"},
                                {"Integral","Int","4"},
                                {"Recommend","Int","4"},
                                {"AllClickNum","Int","4"},
                                {"UpdateTime","DateTime","8"},
                                {"ModeTemplate","NVarChar","255"},
                                {"AddUser","NVarChar","255"},
                                {"DownCar","Int","4"},
                                {"AddTime","DateTime","8"},
                                {"TableName","NVarChar","50"},
                                {"Istrue","Int","4"},
                                {"Isgood","Int","4"},
                                {"isnew","Int","4"},
                                {"ishot","Int","4"},
                                {"isbest","Int","4"},
                                {"Wholesalesinfo","NVarChar","4000"},
                                {"ItemID","Int","4"},
                                {"ComModelID","Int","4"},
                                {"Largess","Int","4"},
                                {"IntegralNum","Int","4"},
                                {"GuessXML","NVarChar","1000"},
                                {"FirstNodeID","Int","4"},
                                {"PointVal","Money","32"},
                                {"BookPrice","Money","32"},
                                {"UserType","Int","4"},
                                {"UserPrice","NVarChar","1000"},
                                {"FarePrice","NVarChar","400"},
                                {"ExpRemind","Int","4"},
                                {"ParentID","Int","4"},
                                {"UserID","Int","4"},
                                {"UserShopID","Int","4"},
                                {"BindIDS","VarChar","4000"},
                                {"OrderID","Int","4"},
                                {"IDCPrice","NText","20000"},
                                {"Recycler","Int","4"},
                                {"Quota_Json","VarChar","8000"},
                                {"DownQuota_Json","VarChar","8000"},
                                {"ShortProName","NVarChar","100"},
                                {"SpecialID","VarChar","500"},
                                {"Addon","NVarChar","4000"}
                                };
            return Tablelist;
        }
        public M_Product GetModelFromReader(DbDataReader rdr)
        {
            M_Product model = new M_Product();
            model.ID = Convert.ToInt32(rdr["ID"]);
            model.Class = ConvertToInt(rdr["Class"]);
            model.Nodeid = ConvertToInt(rdr["Nodeid"]);
            model.ModelID = ConvertToInt(rdr["ModelID"]);
            model.AllClickNum = ConvertToInt(rdr["AllClickNum"]);
            model.ProCode = ConverToStr(rdr["ProCode"]);
            model.BarCode = ConverToStr(rdr["BarCode"]);
            model.Proname = ConverToStr(rdr["Proname"]);
            model.Kayword = ConverToStr(rdr["Kayword"]);
            model.ProUnit = ConverToStr(rdr["ProUnit"]);
            model.Weight = ConverToDouble(rdr["Weight"]);
            model.ProClass = ConvertToInt(rdr["ProClass"]);
            model.Properties = ConvertToInt(rdr["Properties"]);
            model.Sales = ConvertToInt(rdr["Sales"]);
            model.Proinfo = ConverToStr(rdr["Proinfo"]);
            model.Procontent = ConverToStr(rdr["Procontent"]);
            model.Clearimg = ConverToStr(rdr["Clearimg"]);
            model.Thumbnails = ConverToStr(rdr["Thumbnails"]);
            model.Producer = ConverToStr(rdr["Producer"]);
            model.Brand = ConverToStr(rdr["Brand"]);
            model.Allowed = ConvertToInt(rdr["Allowed"]);
            model.Quota = ConvertToInt(rdr["Quota"]);
            model.DownQuota = ConvertToInt(rdr["DownQuota"]);
            model.Stock = ConvertToInt(rdr["Stock"]);
            model.StockDown = ConvertToInt(rdr["StockDown"]);
            model.Rate = ConverToDouble(rdr["Rate"]);
            model.Rateset = ConvertToInt(rdr["Rateset"]);
            model.Dengji = ConvertToInt(rdr["Dengji"]);
            model.ShiPrice = ConverToDouble(rdr["ShiPrice"]);
            model.LinPrice = ConverToDouble(rdr["LinPrice"]);
            model.Wholesales = ConvertToInt(rdr["Wholesales"]);
            model.Wholesaleone = ConvertToInt(rdr["Wholesaleone"]);
            model.Preset = ConverToStr(rdr["Preset"]);
            model.Integral = ConvertToInt(rdr["Integral"]);
            model.Recommend = ConvertToInt(rdr["Recommend"]);
            model.AllClickNum = ConvertToInt(rdr["AllClickNum"]);
            model.UpdateTime = ConvertToDate(rdr["UpdateTime"]);
            model.ModeTemplate = ConverToStr(rdr["ModeTemplate"]);
            model.AddUser = ConverToStr(rdr["AddUser"]);
            model.DownCar = ConvertToInt(rdr["DownCar"]);
            model.AddTime = ConvertToDate(rdr["AddTime"]);
            model.TableName = ConverToStr(rdr["TableName"]);
            model.Istrue = ConvertToInt(rdr["Istrue"]);
            model.Isgood = ConvertToInt(rdr["Isgood"]);
            model.Isnew = ConvertToInt(rdr["isnew"]);
            model.Ishot = ConvertToInt(rdr["ishot"]);
            model.Isbest = ConvertToInt(rdr["isbest"]);
            model.Wholesalesinfo = ConverToStr(rdr["Wholesalesinfo"]);
            model.ItemID = ConvertToInt(rdr["ItemID"]);
            model.ComModelID = ConvertToInt(rdr["ComModelID"]);
            model.Largess = ConvertToInt(rdr["Largess"]);
            model.IntegralNum = ConvertToInt(rdr["IntegralNum"]);
            model.GuessXML = ConverToStr(rdr["GuessXML"]);
            model.OrderID = ConvertToInt(rdr["OrderID"]);
            model.FirstNodeID = ConvertToInt(rdr["FirstNodeID"]);
            model.PointVal = ConverToDouble(rdr["PointVal"]);
            model.isStand = ConvertToInt(rdr["isStand"]);
            model.UserShopID = ConvertToInt(rdr["UserShopID"]);
            model.BookPrice = ConverToDouble(rdr["BookPrice"]);
            model.UserType = ConvertToInt(rdr["UserType"]);
            model.UserPrice = ConverToStr(rdr["UserPrice"]);
            model.Recycler = ConverToBool(rdr["Recycler"]);
            model.FarePrice = ConverToStr(rdr["FarePrice"]);
            model.ExpRemind = ConvertToInt(rdr["ExpRemind"]);
            model.ParentID = ConvertToInt(rdr["ParentID"]);
            model.UserID = ConvertToInt(rdr["UserID"]);
            model.BindIDS = ConverToStr(rdr["BindIDS"]);
            model.IDCPrice = ConverToStr(rdr["IDCPrice"]);
            model.Quota_Json = ConverToStr(rdr["Quota_Json"]);
            model.DownQuota_Json = ConverToStr(rdr["DownQuota_Json"]);
            model.ShortProName = ConverToStr(rdr["ShortProName"]);
            model.SpecialID = ConverToStr(rdr["SpecialID"]);
            model.Addon = ConverToStr(rdr["Addon"]);
            rdr.Close();
            return model;
        }
        public M_Product GetModelFromReader(DataRow rdr)
        {
            M_Product model = new M_Product();
            model.ID = Convert.ToInt32(rdr["ID"]);
            model.Class = ConvertToInt(rdr["Class"]);
            model.Nodeid = ConvertToInt(rdr["Nodeid"]);
            model.ModelID = ConvertToInt(rdr["ModelID"]);
            model.AllClickNum = ConvertToInt(rdr["AllClickNum"]);
            model.ProCode = ConverToStr(rdr["ProCode"]);
            model.BarCode = ConverToStr(rdr["BarCode"]);
            model.Proname = ConverToStr(rdr["Proname"]);
            model.Kayword = ConverToStr(rdr["Kayword"]);
            model.ProUnit = ConverToStr(rdr["ProUnit"]);
            model.Weight = ConverToDouble(rdr["Weight"]);
            model.ProClass = ConvertToInt(rdr["ProClass"]);
            model.Properties = ConvertToInt(rdr["Properties"]);
            model.Sales = ConvertToInt(rdr["Sales"]);
            model.Proinfo = ConverToStr(rdr["Proinfo"]);
            model.Procontent = ConverToStr(rdr["Procontent"]);
            model.Clearimg = ConverToStr(rdr["Clearimg"]);
            model.Thumbnails = ConverToStr(rdr["Thumbnails"]);
            model.Producer = ConverToStr(rdr["Producer"]);
            model.Brand = ConverToStr(rdr["Brand"]);
            model.Allowed = ConvertToInt(rdr["Allowed"]);
            model.Quota = ConvertToInt(rdr["Quota"]);
            model.DownQuota = ConvertToInt(rdr["DownQuota"]);
            model.Stock = ConvertToInt(rdr["Stock"]);
            model.StockDown = ConvertToInt(rdr["StockDown"]);
            model.Rate = ConverToDouble(rdr["Rate"]);
            model.Rateset = ConvertToInt(rdr["Rateset"]);
            model.Dengji = ConvertToInt(rdr["Dengji"]);
            model.ShiPrice = ConverToDouble(rdr["ShiPrice"]);
            model.LinPrice = ConverToDouble(rdr["LinPrice"]);
            model.Wholesales = ConvertToInt(rdr["Wholesales"]);
            model.Wholesaleone = ConvertToInt(rdr["Wholesaleone"]);
            model.Preset = ConverToStr(rdr["Preset"]);
            model.Integral = ConvertToInt(rdr["Integral"]);
            model.Recommend = ConvertToInt(rdr["Recommend"]);
            model.AllClickNum = ConvertToInt(rdr["AllClickNum"]);
            model.UpdateTime = ConvertToDate(rdr["UpdateTime"]);
            model.ModeTemplate = ConverToStr(rdr["ModeTemplate"]);
            model.AddUser = ConverToStr(rdr["AddUser"]);
            model.DownCar = ConvertToInt(rdr["DownCar"]);
            model.AddTime = ConvertToDate(rdr["AddTime"]);
            model.TableName = ConverToStr(rdr["TableName"]);
            model.Istrue = ConvertToInt(rdr["Istrue"]);
            model.Isgood = ConvertToInt(rdr["Isgood"]);
            model.Isnew = ConvertToInt(rdr["isnew"]);
            model.Ishot = ConvertToInt(rdr["ishot"]);
            model.Isbest = ConvertToInt(rdr["isbest"]);
            model.Wholesalesinfo = ConverToStr(rdr["Wholesalesinfo"]);
            model.ItemID = ConvertToInt(rdr["ItemID"]);
            model.ComModelID = ConvertToInt(rdr["ComModelID"]);
            model.Largess = ConvertToInt(rdr["Largess"]);
            model.IntegralNum = ConvertToInt(rdr["IntegralNum"]);
            model.GuessXML = ConverToStr(rdr["GuessXML"]);
            model.OrderID = ConvertToInt(rdr["OrderID"]);
            model.FirstNodeID = ConvertToInt(rdr["FirstNodeID"]);
            model.PointVal = ConverToDouble(rdr["PointVal"]);
            model.isStand = ConvertToInt(rdr["isStand"]);
            model.UserShopID = ConvertToInt(rdr["UserShopID"]);
            model.BookPrice = ConverToDouble(rdr["BookPrice"]);
            model.UserType = ConvertToInt(rdr["UserType"]);
            model.UserPrice = ConverToStr(rdr["UserPrice"]);
            model.Recycler = ConverToBool(rdr["Recycler"]);
            model.FarePrice = ConverToStr(rdr["FarePrice"]);
            model.ExpRemind = ConvertToInt(rdr["ExpRemind"]);
            model.ParentID = ConvertToInt(rdr["ParentID"]);
            model.UserID = ConvertToInt(rdr["UserID"]);
            model.BindIDS = ConverToStr(rdr["BindIDS"]);
            model.IDCPrice = ConverToStr(rdr["IDCPrice"]);
            model.Quota_Json = ConverToStr(rdr["Quota_Json"]);
            model.DownQuota_Json = ConverToStr(rdr["DownQuota_Json"]);
            model.ShortProName = ConverToStr(rdr["ShortProName"]);
            model.SpecialID = ConverToStr(rdr["SpecialID"]);
            model.Addon = ConverToStr(rdr["Addon"]);
            return model;
        }
        public override SqlParameter[] GetParameters()
        {
            M_Product model = this;
            if (model.AddTime <= DateTime.MinValue) { model.AddTime = DateTime.Now; }
            model.UpdateTime = DateTime.Now;
            SqlParameter[] sp = GetSP();
            sp[0].Value = model.ID;
            sp[1].Value = model.Class;
            sp[2].Value = model.Nodeid;
            sp[3].Value = model.ModelID;
            sp[4].Value = model.ProCode;
            sp[5].Value = model.BarCode;
            sp[6].Value = model.Proname;
            sp[7].Value = model.Kayword;
            sp[8].Value = model.ProUnit;
            sp[9].Value = model.Weight;
            sp[10].Value = model.ProClass;
            sp[11].Value = model.Properties;
            sp[12].Value = model.Sales;
            sp[13].Value = model.Proinfo;
            sp[14].Value = model.Procontent;
            sp[15].Value = model.Clearimg;
            sp[16].Value = model.Thumbnails;
            sp[17].Value = model.Producer;
            sp[18].Value = model.Brand;
            sp[19].Value = model.Allowed;
            sp[20].Value = model.Quota;
            sp[21].Value = model.DownQuota;
            sp[22].Value = model.Stock;
            sp[23].Value = model.StockDown;
            sp[24].Value = model.Rate;
            sp[25].Value = model.Rateset;
            sp[26].Value = model.Dengji;
            sp[27].Value = model.ShiPrice;
            sp[28].Value = model.LinPrice;
            sp[29].Value = model.Wholesales;
            sp[30].Value = model.Wholesaleone;
            sp[31].Value = model.Preset;
            sp[32].Value = model.Integral;
            sp[33].Value = model.Recommend;
            sp[34].Value = model.AllClickNum;
            sp[35].Value = model.UpdateTime;
            sp[36].Value = model.ModeTemplate;
            sp[37].Value = model.AddUser;
            sp[38].Value = model.DownCar;
            sp[39].Value = model.AddTime;
            sp[40].Value = model.TableName;
            sp[41].Value = model.Istrue;
            sp[42].Value = model.Isgood;
            sp[43].Value = model.Isnew;
            sp[44].Value = model.Ishot;
            sp[45].Value = model.Isbest;
            sp[46].Value = model.Wholesalesinfo;
            sp[47].Value = model.ItemID;
            sp[48].Value = model.ComModelID;
            sp[49].Value = model.Largess;
            sp[50].Value = model.IntegralNum;
            sp[51].Value = model.GuessXML;
            sp[52].Value = model.FirstNodeID;
            sp[53].Value = model.PointVal;
            sp[54].Value = model.BookPrice;
            sp[55].Value = model.UserType;
            sp[56].Value = model.UserPrice;
            sp[57].Value = model.FarePrice;
            sp[58].Value = model.ExpRemind;
            sp[59].Value = model.ParentID;
            sp[60].Value = model.UserID;
            sp[61].Value = model.UserShopID;
            sp[62].Value = model.BindIDS;
            sp[63].Value = model.OrderID;
            sp[64].Value = model.IDCPrice;
            sp[65].Value = model.Recycler;
            sp[66].Value = model.Quota_Json;
            sp[67].Value = model.DownQuota_Json;
            sp[68].Value = model.ShortProName;
            sp[69].Value = model.SpecialID;
            sp[70].Value = model.Addon;
            return sp;
        }
    }
    [Serializable]
    public class M_LinPrice//附加金额售价
    {
        public double Purse { get; set; }
        public double Sicon { get; set; }
        public double Point { get; set; }
    }
}
