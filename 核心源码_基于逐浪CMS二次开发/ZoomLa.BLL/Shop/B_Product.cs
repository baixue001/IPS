using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Text;
using ZoomLa.BLL.Content;
using ZoomLa.BLL.Helper;
using ZoomLa.Common;
using ZoomLa.Components;
using ZoomLa.Model;
using ZoomLa.Model.Content;
using ZoomLa.Model.Shop;

using ZoomLa.Safe;
using ZoomLa.SQLDAL;
using ZoomLa.SQLDAL.SQL;

namespace ZoomLa.BLL.Shop
{
    public class B_Product
    {
        public string TbName, PK;
        private M_Product initMod = new M_Product();
        public B_Product()
        {
            TbName = initMod.TbName;
            PK = initMod.PK;
        }
        public M_Product GetproductByid(int ID)
        {
            if (ID < 1) { return null; }
            using (DbDataReader reader = DBCenter.SelReturnReader(TbName, PK, ID))
            {
                if (reader.Read())
                {
                    return initMod.GetModelFromReader(reader);
                }
                else
                {
                    return null;
                }
            }
        }
        public int Add(DataTable ContentDT, M_Product model)
        {
            //m_Product.ItemID = ItemID;
            int itemid = 0;
            if (!string.IsNullOrEmpty(model.TableName) && ContentDT.Rows.Count > 0)
            {
                itemid = DBCenter.Insert(model.TableName, BLLCommon.GetFields(ContentDT), BLLCommon.GetParas(ContentDT), BLLCommon.GetParameters(ContentDT).ToArray());
            }
            model.ItemID = itemid;
            return Insert(model);
        }
        public int Insert(M_Product model)
        {
            SYSProCheck(model);
            return DBCenter.Insert(model);
        }

        //-----------------------------------------------SELECT
        public DataTable Sel(int nodeid = 0, int pid = -100)
        {
            string where = " 1=1 ";
            if (nodeid > 0) { where += " AND NodeID=" + nodeid; }
            if (pid != -100) { where += " AND ParentID=" + pid; }
            return DBCenter.Sel(TbName, where, "ID DESC");
        }
        public DataTable GetProductAll(Filter_Product filter)
        {
            return SelPage(1, int.MaxValue, filter).dt;
        }

        /// <summary>
        /// [main]
        /// </summary>
        public PageSetting SelPage(int cpage, int psize, Filter_Product filter)
        {
            string where = "1=1 ";
            List<SqlParameter> sp = new List<SqlParameter>();
            if (filter.NodeID > 0)
            {
                string nids = new B_Node().GetChilds(filter.NodeID);
                //where += " AND (A.NodeID=" + filter.NodeID + " OR A.FirstNodeID=" + filter.NodeID + ")";
                where += " AND A.NodeID IN (" + nids + ")";

            }
            if (!string.IsNullOrEmpty(filter.NodeIDS))
            { SafeSC.CheckIDSEx(filter.NodeIDS); where += " AND A.NodeID IN (" + filter.NodeIDS + ")"; }
            //组合|单品
            if (filter.pclass != -100) { where += "AND [Class]=" + filter.pclass; }
            //商品分类
            if (!string.IsNullOrEmpty(filter.proclass)) { SafeSC.CheckIDSEx(filter.proclass); where += " AND A.ProClass IN (" + filter.proclass + ")"; }
            if (filter.hasRecycle != -100) { where += " AND Recycler=" + filter.hasRecycle; }
            if (filter.istrue != -100) { where += " AND IsTrue=" + filter.istrue; }
            if (filter.issale != -100) { where += " AND Sales=" + filter.issale; }
            //搜索,支持指定条件
            //if (!string.IsNullOrEmpty(filter.skey))
            //{
            //    if (string.IsNullOrEmpty(filter.stype)) { filter.stype = "proname"; }
            //    sp.Add(new SqlParameter("skey", "%" + filter.skey + "%"));
            //    int sid = DataConverter.CLng(filter.skey);
            //    switch (filter.stype.ToLower())
            //    {
            //        case "id": where += " AND A.ID=" + sid; break;
            //        case "proname": where += " AND A.Proname LIKE @skey"; break;
            //        case "id_proname": where += " AND (A.ID=" + sid + " OR A.ProName LIKE @skey)"; break;
            //        case "adduser": where += " AND A.AddUser LIKE @skey"; break;
            //        case "storeid": filter.storeid = sid; break;
            //    }
            //}
            if (!string.IsNullOrEmpty(filter.proname))
            {
                sp.Add(new SqlParameter("proname", "%" + filter.proname + "%"));
                where += " AND A.Proname LIKE @proname";
            }
            if (!string.IsNullOrEmpty(filter.adduser))
            {
                sp.Add(new SqlParameter("adduser", "%" + filter.adduser + "%"));
                where += " AND A.AddUser LIKE @adduser";
            }
            if (filter.special > 0)
            {
                where += " AND SpecialID LIKE '%," + filter.special + ",%'";
            }
            //快速筛选
            where += GetWhereByType(filter.type);
            if (filter.storeid == 0) { where += " AND (A.UserShopID=0 OR A.UserShopID IS NULL)"; } //仅商城
            else if (filter.storeid > 0) { where += " AND A.UserShopID=" + filter.storeid; }//指定店铺
            else if (filter.storeid == -1) { where += " AND A.UserShopID>0"; }//仅店铺
            else if (filter.storeid == -100) { }//全部
            string orderBy = StrHelper.SQL_OrderBy(filter.orderBy,"id,linprice", "A.OrderId DESC,A.ID DESC");
            PageSetting setting = new PageSetting()
            {
                cpage = cpage,
                psize = psize,
                fields = "A.*,B.NodeName,(SELECT Title FROM ZL_CommonModel WHERE GeneralID=A.UserShopID) StoreName",
                t1 = TbName,
                t2 = "ZL_Node",
                on = "A.NodeID=B.NodeID",
                where = where,
                order = orderBy,
                spList = sp
            };
            DBCenter.SelPage(setting);
            return setting;
        }
        public DataTable GetContent(string tablename, int Itemid)
        {
            if (string.IsNullOrEmpty(tablename) || Itemid < 1) { return null; }
            SafeSC.CheckDataEx(tablename);
            return DBCenter.Sel(tablename, "ID=" + Itemid);
        }
        //用于旅游,机票页
        public DataTable SelByIDS(string ids, string field = "*")
        {
            ids = StrHelper.PureIDSForDB(ids);
            if (string.IsNullOrEmpty(ids)) { return null; }
            SafeSC.CheckIDSEx(ids);
            return DBCenter.SelWithField(TbName, field, "ID IN(" + ids + ")");
        }
        public int GetOrder(int NodeID, int size)
        {
            string strSql = "";
            if (size == 0)
                strSql = "select Min(OrderID) from ZL_Commodities where ItemID!=0";
            else
                strSql = "select Max(OrderID) from ZL_Commodities where ItemID!=0";
            strSql += " and NodeID = " + NodeID;
            return Convert.ToInt32(DBCenter.DB.ExecuteScalar(new SqlModel(strSql, null)));
        }
        /// <summary>
        ///获取前一或后一商品数据,用于前端和后端商品排序,
        ///0:下移(前一商品),1:上移(后一商品)
        /// </summary>
        public M_Product GetNearID(int NodeID, int CurrentID, int UporDown, int uid = 0)
        {
            string where = " 1=1 ";
            string order = "";
            if (NodeID > 0) { where += " AND NodeID=" + NodeID; }
            if (uid > 0) { where += " AND UserID=" + uid; }
            if (UporDown == 0)
            {
                where += " AND OrderId<" + CurrentID;
                order = "OrderId DESC";
            }
            else// if (UporDown == 1)
            {
                where += " AND OrderId>" + CurrentID;
                order = "OrderId ASC";
            }
            int id = DataConvert.CLng(DBCenter.ExecuteScala(TbName, "ID", where, order));
            return GetproductByid(id);
        }
        public bool UpdateOrder(M_Product Product)
        {
            return DBCenter.UpdateSQL(TbName, "OrderId=" + Product.OrderID, "ID=" + Product.ID);
        }
        public bool Update(DataTable ContentDT, M_Product model)
        {
            int ItemID = model.ItemID;
            if (ContentDT != null && ContentDT.Rows.Count > 0)
            {
                if (string.IsNullOrEmpty(model.TableName)) { throw new Exception("商品附表为空"); }
                List<SqlParameter> splist = new List<SqlParameter>();
                splist.AddRange(BLLCommon.GetParameters(ContentDT));
                if (DBCenter.IsExist(model.TableName, "ID=" + ItemID))
                {
                    DBCenter.UpdateSQL(model.TableName, BLLCommon.GetFieldAndPara(ContentDT), "ID=" + ItemID, splist);
                }
                else
                {
                    DBCenter.Insert(model.TableName, BLLCommon.GetFields(ContentDT), BLLCommon.GetParas(ContentDT), splist.ToArray());
                }
            }
            UpdateByID(model);
            return true;
        }
        public void UpdateByID(M_Product model)
        {
            SYSProCheck(model);
            DBCenter.UpdateByID(model, model.ID);
        }
        public bool setproduct(string type, string ids)
        {
            SafeSC.CheckIDSEx(ids);
            string sqlStr = string.Empty;
            switch (type)
            {
                case "sales":
                    sqlStr = "update ZL_Commodities set Sales=1 where (ID in (" + ids + "))";
                    break;
                case "hot":
                    sqlStr = "update ZL_Commodities set ishot=1 where (ID in (" + ids + "))";
                    break;
                case "best":
                    sqlStr = "update ZL_Commodities set isbest=1 where (ID in (" + ids + "))";
                    break;
                case "new":
                    sqlStr = "update ZL_Commodities set isnew=1 where (ID in (" + ids + "))";
                    break;
                case "unsales"://Sales
                    sqlStr = "update ZL_Commodities set Sales=0 where (ID in (" + ids + "))";
                    break;
                case "unhot"://ishot
                    sqlStr = "update ZL_Commodities set ishot=0 where (ID in (" + ids + "))";
                    break;
                case "unbest"://isbest
                    sqlStr = "update ZL_Commodities set isbest=0 where (ID in (" + ids + "))";
                    break;
                case "unnew"://isnew
                    sqlStr = "update ZL_Commodities set isnew=0 where (ID in (" + ids + "))";
                    break;
                case "audit"://批量审核
                    sqlStr = "update ZL_Commodities set istrue=1 where (ID in (" + ids + "))";
                    break;
                case "unaudit"://取消审核
                    sqlStr = "update ZL_Commodities set istrue=0 where (ID in (" + ids + "))";
                    break;
                case "recover"://还原
                    sqlStr = "update ZL_Commodities set Recycler=0 where (ID in (" + ids + "))";
                    break;
                case "recycle"://进回收站
                    sqlStr = "update ZL_Commodities set Recycler=1 where (ID in (" + ids + "))";
                    break;
                case "recover_all"://全部还原
                    sqlStr = "update ZL_Commodities set Recycler=0 where Recycler=1";
                    break;
                default:
                    break;
            }
            DBCenter.DB.ExecuteNonQuery(new SqlModel(sqlStr, null));
            return true;
        }
        /// <summary>
        /// 还原商品
        /// </summary>
        public bool UpDeleteByID(int ProductId)
        {
            return DBCenter.UpdateSQL(TbName, "Recycler=0", "ID=" + ProductId);
        }
        public void DelToRecycle(string ids)
        {
            SafeSC.CheckIDSEx(ids);
            DBCenter.UpdateSQL(TbName, "Recycler=1", "ID IN (" + ids + ")");
        }
        public void ClearRecycle()
        {
            DataTable dt = DBCenter.SelWithField(TbName, "ID,ItemID", "Recycler=1");
            string ids = StrHelper.GetIDSFromDT(dt, "ID");
            string items = StrHelper.GetIDSFromDT(dt, "ItemID");
            RealDelByIDS(ids, items);
        }
        /// <summary>
        /// 从数据库中移动商品相关信息
        /// </summary>
        public bool RealDelByIDS(string ids)
        {
            SafeSC.CheckIDSEx(ids);
            DataTable dt = DBCenter.Sel(TbName, "ID IN (" + ids + ")");
            string items = StrHelper.GetIDSFromDT(dt, "ItemID");
            return RealDelByIDS(ids, items);
        }
        public bool RealDelByIDS(string ids, string items)
        {
            SafeSC.CheckDataEx(items);
            SafeSC.CheckIDSEx(ids);
            DBCenter.DelByIDS(TbName, PK, ids);
            DBCenter.DelByIDS("ZL_P_Shop", "ID", items);
            return true;
        }
        //-------------Tools
        //需处理,多人同时添加问题,是否改为从内存中取
        public static string GetProCode()
        {
            string regular = SiteConfig.ShopConfig.ItemRegular;
            string stime = DateTime.Now.ToString("yyyy/MM/dd 00:00");
            string etime = DateTime.Now.ToString("yyyy/MM/dd 23:59:59");
            string itemCode = DateTime.Now.ToString(regular);
            //"SELECT Count(ID) From ZL_Commodities WHERE AddTime BETWEEN '" + stime + "' AND '" + etime + "'"
            int count = Convert.ToInt32(DBCenter.ExecuteScala("ZL_Commodities", "Count(ID)", "AddTime BETWEEN '" + stime + "' AND '" + etime + "'"));
            count++;
            if (count < 10) { itemCode += "0000" + count; }
            else if (count >= 10) { itemCode += "000" + count; }
            else if (count >= 100) { itemCode += "00" + count; }
            else if (count >= 1000) { itemCode += "0" + count; }
            else if (count >= 10000) { itemCode += count; }
            else { itemCode += count; }
            return itemCode;
        }
        /// <summary>
        /// 判断库中是否已有相同编号商品
        /// </summary>
        /// <returns></returns>
        public bool IsExistProCode(string code)
        {
            List<SqlParameter> sp = new List<SqlParameter>();
            sp.Add(new SqlParameter("code", code));
            DataTable dt = DBCenter.Sel(TbName, "ProCode = @code", "", sp);
            return dt.Rows.Count > 0;
        }
        private string GetWhereByType(int type)
        {
            string wherestr = "";
            switch (type)
            {
                case 1://所有商品
                    wherestr = "";
                    break;
                case 2://正常销售
                    wherestr = "And Sales=1";
                    break;
                case 3://未销售
                    wherestr = "And Sales=0";
                    break;
                case 4://正常销售商品
                    wherestr = "and ProClass=1";
                    break;
                case 5://特价处理
                    wherestr = "and istrue=1 and ProClass=2 and Sales=1";
                    break;
                case 6://所有热销
                    wherestr = "and ishot=1";
                    break;
                case 7://所有新品
                    wherestr = "and isnew=1";
                    break;
                case 8://所有精品
                    wherestr = "and isbest=1";
                    break;
                case 9://有促销活动
                    wherestr = "and istrue=1 and ID NOT in (select ID from ZL_Commodities where  istrue=1 and Preset = '1|0|0|{}') and Sales=1";
                    break;
                case 10://实际库存报警的商品
                    wherestr = "and istrue=1 and Stock<=StockDown and StockDown<>-1 and Sales=1 and JisuanFs=0";
                    break;
                case 11://预定库存报警的商品
                    wherestr = "and istrue=1 and Stock<=StockDown and StockDown<>-1 and Sales=1 and JisuanFs=1";
                    break;
                case 12://已售完的商品
                    wherestr = "and istrue=1 and Stock=0 and Sales=1 and Sold>0";
                    break;
                case 13:
                    wherestr = "and istrue=1 and Wholesales=1 and Sales=1";
                    break;
                case 14://所有捆绑销售的商品
                    wherestr = "and istrue=1 and Priority=1 and id<>0";
                    break;
                case 15://所有礼品
                    wherestr = "and istrue=1 and ItemID!=0 and Largess=1";
                    break;
                case 16://已审核商品
                    wherestr = "and istrue=1";
                    break;
                case 17://待审核商品
                    wherestr = "and istrue=0";
                    break;
                case 18://用户商品
                    wherestr = "and UserID>0";
                    break;
                case 19:
                    wherestr = "and ShiPrice>0";
                    break;
                case 20://组合商品
                    wherestr = " AND A.ParentID>0";
                    break;
            }
            return " " + wherestr + " ";
        }
        private void SYSProCheck(M_Product model)
        {
            if (string.IsNullOrEmpty(model.ShortProName))
            {
                model.ShortProName = model.Proname.Substring(0, model.Proname.Length > 5 ? 5 : model.Proname.Length);
            }
            if (model.OrderID == 0) { model.OrderID = (DataConvert.CLng(DBCenter.ExecuteScala(TbName, "MAX(OrderID)", "1=1")) + 1); }
            if (model.Recommend < 1) { model.Recommend = 0; }
            if (string.IsNullOrEmpty(model.ProCode)) { model.ProCode = GetProCode(); }
            if (model.LinPrice < 0) { throw new Exception("商品的价格不能为负数"); }
        }
        //数据导入
        public bool ImportProducts(DataTable dt, int ModelID, int NodeID)
        {
            B_ModelField b_ModelField = new B_ModelField();
            DataTable dtModelField = b_ModelField.GetModelFieldList(ModelID);
            //建立从表存放当前信息
            DataTable table = new DataTable();
            table.Columns.Add(new DataColumn("FieldName", typeof(string)));
            table.Columns.Add(new DataColumn("FieldType", typeof(string)));
            table.Columns.Add(new DataColumn("FieldValue", typeof(string)));
            table.Columns.Add(new DataColumn("ShopmodelID", typeof(string)));
            table.Columns.Add(new DataColumn("FieldAlias", typeof(string)));

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                if (dtModelField != null && dtModelField.Rows.Count > 0)
                {
                    foreach (DataRow dr in dtModelField.Rows)
                    {
                        DataRow row = table.NewRow();
                        row[0] = dr["FieldName"].ToString();
                        row[1] = dr["FieldType"].ToString();
                        row[4] = dr["FieldAlias"].ToString();
                        try
                        {
                            row[2] = dt.Rows[i][row[4].ToString()];
                        }
                        catch
                        {
                            return false;
                        }
                        table.Rows.Add(row);
                    }
                }
                //基础表内容

                B_Model modBll = new B_Model();
                M_ModelInfo modMod = modBll.SelReturnModel(ModelID);
                //主表内容
                M_Product m_Product = new M_Product();
                m_Product.ModelID = ModelID;
                m_Product.Nodeid = NodeID;
                m_Product.TableName = modMod.TableName;
                m_Product.ProCode = dt.Rows[i]["商品编号"].ToString();
                m_Product.BarCode = dt.Rows[i]["条形码"].ToString();
                m_Product.Proname = dt.Rows[i]["商品名称"].ToString();
                m_Product.Kayword = dt.Rows[i]["关键字"].ToString();
                m_Product.ProUnit = dt.Rows[i]["商品单位"].ToString();
                try
                {
                    m_Product.Weight = Convert.ToInt32(dt.Rows[i]["商品重量"].ToString() == "" ? 0 : dt.Rows[i]["商品重量"]);
                }
                catch
                {
                    throw new Exception("第" + (i + 1) + "行，字段[商品重量]格式不正确");
                }
                m_Product.ProClass = 1;
                m_Product.Sales = Convert.ToInt32(dt.Rows[i]["销售状态(1)"].ToString() == "1" ? 1 : 0);
                m_Product.Istrue = Convert.ToInt32(dt.Rows[i]["属性设置(1)"].ToString() == "1" ? 1 : 0);
                try
                {
                    m_Product.AllClickNum = Convert.ToInt32(dt.Rows[i]["点击数"].ToString() == "" ? 0 : dt.Rows[i]["点击数"]);
                }
                catch
                {
                    throw new Exception("第" + (i + 1) + "行，字段[点击数]格式不正确");
                }
                try
                {
                    m_Product.UpdateTime = Convert.ToDateTime(dt.Rows[i]["更新时间"]);
                }
                catch
                {
                    throw new Exception("第" + (i + 1) + "行，字段[更新时间]格式不正确");
                }
                m_Product.ModeTemplate = "";

                ///22222222222222222222
                m_Product.Proinfo = dt.Rows[i]["商品简介"].ToString();
                m_Product.Procontent = dt.Rows[i]["详细介绍"].ToString();
                m_Product.Clearimg = dt.Rows[i]["商品清晰图"].ToString();
                m_Product.Thumbnails = dt.Rows[i]["商品缩略图"].ToString();

                ///33333333333333333333
                m_Product.Producer = dt.Rows[i]["生产商"].ToString();
                m_Product.Brand = dt.Rows[i]["品牌/商标"].ToString();
                m_Product.Allowed = Convert.ToInt32(dt.Rows[i]["缺货时允许购买(0)"].ToString() == "0" ? 0 : 1);
                try
                {
                    m_Product.Quota = Convert.ToInt32(dt.Rows[i]["限购数量"].ToString() == "" ? 0 : dt.Rows[i]["限购数量"]);
                }
                catch
                {
                    throw new Exception("第" + (i + 1) + "行，字段[限购数量]格式不正确");
                }
                try
                {
                    m_Product.DownQuota = Convert.ToInt32(dt.Rows[i]["最低购买数量"].ToString() == "" ? 0 : dt.Rows[i]["最低购买数量"]);
                }
                catch
                {
                    throw new Exception("第" + (i + 1) + "行，字段[最低购买数量]格式不正确");
                }
                m_Product.Stock = 0;//库存数量

                try
                {
                    m_Product.ShiPrice = Convert.ToDouble(dt.Rows[i]["市场参考价"].ToString() == "" ? 0 : dt.Rows[i]["市场参考价"]);
                }
                catch
                {
                    throw new Exception("第" + (i + 1) + "行，字段[市场参考价]格式不正确");
                }
                try
                {
                    m_Product.LinPrice = Convert.ToDouble(dt.Rows[i]["当前零售价"].ToString() == "" ? 0 : dt.Rows[i]["当前零售价"]);
                }
                catch
                {
                    throw new Exception("第" + (i + 1) + "行，字段[当前零售价]格式不正确");
                }
                try
                {
                    m_Product.AddTime = DateTime.Parse(dt.Rows[i]["创建时间"].ToString());
                }
                catch (Exception)
                {
                    throw new Exception("第" + (i + 1) + "行，字段[创建时间]格式不正确");
                }

                try
                {
                    m_Product.StockDown = 0;
                }
                catch
                {
                    throw new Exception("第" + (i + 1) + "行，字段[库存报警下限]格式不正确");
                }
                try
                {
                    m_Product.Rateset = 1;
                }
                catch
                {
                    throw new Exception("第" + (i + 1) + "行，字段[税率设置]格式不正确");
                }
                try
                {
                    m_Product.Rate = 0;
                }
                catch
                {
                    throw new Exception("第" + (i + 1) + "行，字段[商品税率]格式不正确");
                }
                try
                {
                    m_Product.Dengji = 3;
                }
                catch
                {
                    throw new Exception("第" + (i + 1) + "行，字段[商品推荐等级]格式不正确");
                }

                try
                {
                    m_Product.ShiPrice = Convert.ToDouble(dt.Rows[i]["市场参考价"].ToString() == "" ? 0 : dt.Rows[i]["市场参考价"]);
                }
                catch
                {
                    throw new Exception("第" + (i + 1) + "行，字段[市场参考价]格式不正确");
                }
                try
                {
                    m_Product.LinPrice = Convert.ToDouble(dt.Rows[i]["当前零售价"].ToString() == "" ? 0 : dt.Rows[i]["当前零售价"]);
                }
                catch
                {
                    throw new Exception("第" + (i + 1) + "行，字段[当前零售价]格式不正确");
                }
                //m_Product.UserPrice = dt.Rows[i]["会员组价格"].ToString();
                m_Product.UserPrice = "0";
                try
                {
                    m_Product.Recommend = 0;
                }
                catch
                {
                    throw new Exception("第" + (i + 1) + "行，字段[打折优惠率]格式不正确");
                }
                try
                {
                    m_Product.PointVal = 0;
                }
                catch
                {
                    throw new Exception("第" + (i + 1) + "行，字段[积分价格]格式不正确");
                }
                m_Product.Wholesales = 0;//允许批发
                try
                {
                    m_Product.Wholesaleone = 1;
                }
                catch
                {
                    throw new Exception("第" + (i + 1) + "行，字段[允许单独销售]格式不正确");
                }
                try
                {
                    m_Product.Largess = 0;
                }
                catch
                {
                    throw new Exception("第" + (i + 1) + "行，字段[设置为礼品]格式不正确");
                }

                ///55555555555555555555
                try
                {
                    m_Product.IntegralNum = 0;
                }
                catch
                {
                    throw new Exception("第" + (i + 1) + "行，字段[购物积分]格式不正确");
                }

                Add(table, m_Product);
                table.Rows.Clear();//清除子表中的数据
            }
            return true;
        }
        //-------------------------------会员价逻辑
        /// <summary>
        /// 根据配置,使用预定价-->(会员价||会员组价)-->零售价,用于购物车与订单
        /// </summary>
        public double P_GetByUserType(M_Product model, M_UserInfo mu)
        {
            if (model == null) { throw new Exception("商品不存在"); }
            if (mu.IsNull) { return model.LinPrice; }//购物车界面,用户尚未登录
            double price = 0;
            //if (model.BookPrice > 0) { price = model.BookPrice; }
            if (!string.IsNullOrEmpty(model.UserPrice) && model.UserType > 0)
            {
                switch (model.UserType)
                {
                    case 1:
                        price = DataConvert.CDouble(model.UserPrice);
                        break;
                    case 2://会员组价格,如未匹配,或填写不正确,则仍按零售价
                        {
                            DataTable updt = JsonConvert.DeserializeObject<DataTable>(model.UserPrice);
                            //兼容以前的存储格式
                            if (updt.Columns.Contains("price")) { updt.Columns["price"].ColumnName = "value"; }
                            DataRow[] drs = updt.Select("gid='" + mu.GroupID + "'");
                            if (drs.Length > 0) { price = Convert.ToDouble(drs[0]["value"]); }
                        }
                        break;
                }
            }
            if (price <= 0) { price = model.LinPrice; }
            return price;
        }
        /// <summary>
        /// 根据多价格编号,返回价格信息
        /// </summary>
        /// <param name="codeobj">价格编号</param>
        /// <param name="json">价格Json</param>
        /// <param name="price">价格</param>
        public DataRow GetPriceByCode(object codeobj, string json, ref double price)
        {
            if (codeobj == null || codeobj == DBNull.Value || string.IsNullOrEmpty(codeobj.ToString())) { return null; }
            string code = codeobj.ToString();
            DataTable dt = JsonConvert.DeserializeObject<DataTable>(json);
            if (dt.Rows.Count < 1 || dt.Select("code='" + code + "'").Length < 1) { return null; }
            DataRow dr = dt.Select("code='" + code + "'")[0];
            price = Convert.ToDouble(dr["LinPrice"]);
            return dr;
        }
        /// <summary>
        /// 用于购物车页与订单页面,返回套装促销商品
        /// </summary>
        public DataTable Suit_GetProduct(string json, int num)
        {
            DataTable prodt = JsonConvert.DeserializeObject<DataTable>(json);
            string ids = "";
            foreach (DataRow pro in prodt.Rows)
            {
                ids += pro["ID"] + ",";
            }
            DataTable pdt = DBCenter.SelWithField("ZL_Commodities", "A.*,suitnum=1,Pronum=1", "ID IN (" + ids.Trim(',') + ")");
            for (int i = 0; i < pdt.Rows.Count; i++)
            {
                DataRow pdr = pdt.Rows[i];
                DataRow[] drs = prodt.Select("ID='" + pdr["id"] + "'");
                if (drs.Length > 0)
                {
                    pdr["suitnum"] = drs[0]["suitnum"];
                    pdr["LinPrice"] = drs[0]["Price"];
                    pdr["Pronum"] = DataConvert.CLng(num) * DataConvert.CLng(pdr["suitnum"]);
                }
            }
            pdt.Columns["UserShopID"].ColumnName = "StoreID";
            pdt.Columns["ID"].ColumnName = "ProID";
            return pdt;
        }
        #region 用户中心使用
        public DataTable U_GetProductAll(int uid, int nodeid)
        {
            string where = "UserID=" + uid;
            if (nodeid > 0) { where += " AND NodeID=" + nodeid; }
            return DBCenter.Sel(TbName, where, PK + " DESC");
        }
        public PageSetting U_SPage(int cpage, int psize, int uid, int nodeid = 0, int Recycler = -100)
        {
            string where = "UserID=" + uid;
            if (nodeid > 0) { where += " AND (NodeID=" + nodeid + " OR FirstNodeID=" + nodeid + ")"; }
            if (Recycler != -100) { where += " AND Recycler=" + Recycler; }
            PageSetting setting = PageSetting.Single(cpage, psize, TbName, PK, where, PK + " DESC");
            DBCenter.SelPage(setting);
            return setting;
        }
        #endregion
        #region Show
        public string ShowProAttr(object item)
        {
            DataRow dr = (item as DataRowView).Row;
            string html = "";
            html += DataConverter.CLng(dr["isbest"]) == 1 ? "<span style='color:blue;margin-left:5px;'>精</span>" : "";
            html += DataConverter.CLng(dr["ishot"]) == 1 ? "<span style='color:red;margin-left:5px;'>热</span>" : "";
            html += DataConverter.CLng(dr["isnew"]) == 1 ? "<span style='color:green;margin-left:5px;'>新</span>" : "";
            html += DataConverter.CLng(dr["Largess"]) == 1 ? "<span style='color:#99CC00;margin-left:5px;'>特</span>" : "";
            return html;
        }
        public string ShowStatus(object item)
        {
            DataRow dr = (item as DataRowView).Row;
            int status = DataConvert.CLng(dr["istrue"]);
            return status == 1 ? "已审核" : "未审核";
        }
        #endregion
    }
    public class Filter_Product
    {
        public int NodeID = 0;
        public int storeid = -100;
        public int type = 1;
        public int pid = -100;
        public string skey = "";
        public string stype = "";
        public int pclass = -100;
        public string proclass = "";
        public string NodeIDS = "";
        public int special = -100;
        //回收站,已审核,可销售
        public int hasRecycle = 0;
        public int istrue = -100;
        public int issale = -100;
        //-----------
        public string proname = "";
        public string adduser = "";
        //-----------排序
        public string orderBy = "";
        //public string orderSort = "A.OrderId DESC,A.ID DESC";
        //public string SetSort(string field, string order)
        //{
        //    string result = "A.OrderId DESC,A.ID DESC";
        //    if (string.IsNullOrEmpty(field) || string.IsNullOrEmpty(order)) return result;
        //    switch (field.ToLower())
        //    {
        //        case "id": field = "A.ID"; break;
        //        case "addtime": field = "A.AddTime"; break;
        //        case "updatetime": field = "A.UpdateTime"; break;
        //        case "allclicknum": field = "A.AllClickNum"; break;
        //        case "dengji": field = "A.Dengji"; break;
        //        case "stock": field = "A.Stock"; break;
        //        default: field = ""; break;
        //    }
        //    if (order.ToLower().Equals("asc")) { order = " ASC"; }
        //    else { order = " DESC"; }
        //    if (!string.IsNullOrEmpty(field)) { result = field + order; }
        //    return result;
        //}
    }
}
