using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using ZoomLa.BLL;
using ZoomLa.BLL.Content;
using ZoomLa.Components;
using ZoomLa.Model;
using ZoomLa.Model.Content;
using ZoomLa.SQLDAL;

namespace ZoomLaCMS.Models.Field
{
    public class VM_FieldModel : VM_Base
    {
        private B_ModelField fieldBll = new B_ModelField();
        private B_Content conBll = new B_Content();
        //------------------------------------
        //解析配置,未配置则默认用后端方式
        public ModelConfig config = null;
        //public ModelConfig modcfg = null;
        //指定模型的列表字段
        public DataTable fieldDT = null;
        //-----------------------------------用于解析
        //字段模型,foreach中使用
        public M_ModelField fieldMod = null;
        //如果是修改,该项不能为空
        public DataRow valueDR = null;
        //-------------
        public string UploadDir = (SiteConfig.SiteOption.UploadDir).TrimEnd('/') + "/";
        public string ManageDir = SiteConfig.SiteOption.ManageDir;
        //用于标识哪些资源被加载过了,避免重复加载
        public Dictionary<string, int> ResDic = new Dictionary<string, int>();
        public int ModelID, NodeID, GeneralID;
        //用于内容预览页
        public VM_FieldModel() { }
        //
        public VM_FieldModel(DataTable fieldDT, DataTable valueDT)
        {
            this.fieldDT = fieldDT;
            if (valueDT != null && valueDT.Rows.Count > 0)
            {
                valueDR = valueDT.Rows[0];
            }
            this.config = new ModelConfig();
        }
        //用于内容与商品
        public VM_FieldModel(int modelid, int nodeid, ModelConfig modcfg, int gid , DataRow valueDr = null)
        {
            this.config = modcfg;
            this.ModelID = modelid;
            this.NodeID = nodeid;
            //暂只用于内容管理处
            this.fieldDT = fieldBll.SelByModelID(ModelID, false, false);
            if (gid > 0)//如果指定了gid
            {
                this.GeneralID = gid;
                DataTable valueDT = conBll.GetContent(gid);
                if (valueDT != null && valueDT.Rows.Count > 0)
                {
                    valueDR = valueDT.Rows[0];
                }
            }
            if (valueDr != null) { this.valueDR = valueDr; }
        }
        /// <summary>
        /// 仅用于注册页面,订单|购物车等无模型字段
        /// </summary>
        public VM_FieldModel(DataTable fielddt, ModelConfig modcfg)
        {
            fieldDT = fielddt;
            this.config = modcfg;
            if (modcfg.ValueDR != null) { this.valueDR = modcfg.ValueDR; }
        }
        //------------------------------Tools
        /// <summary>
        /// 根据字段名,获取字段的值
        /// </summary>
        public string GetValue(string fname = "")
        {
            if (string.IsNullOrEmpty(fname)) { fname = fieldMod.FieldName; }
            if (valueDR == null || !valueDR.Table.Columns.Contains(fname)) { return ""; }
            return DataConvert.CStr(valueDR[fname]);
        }
        /// <summary>
        /// 为字段设置默认值
        /// </summary>
        public void SetValue(string value, string fname = "")
        {
            if (string.IsNullOrEmpty(fname)) { fname = fieldMod.FieldName; }
            if (valueDR == null)
            {
                DataTable valueDT = new DataTable();
                valueDT.Rows.Add(valueDT.NewRow());
                valueDR = valueDT.Rows[0];
            }
            if (!valueDR.Table.Columns.Contains(fname))
            {
                valueDR.Table.Columns.Add(new DataColumn(fname, typeof(string)));
            }
            valueDR[fname] = value;
        }
    }
}