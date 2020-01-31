using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using ZoomLa.BLL.Content;
using ZoomLa.Common;
using ZoomLa.Model;
using ZoomLa.Model.Content;
using ZoomLa.SQLDAL;
using ZoomLa.SQLDAL.SQL;

namespace ZoomLaCMS.Areas.Admin.Models
{
    public class VM_ContentManage 
    {
        B_Model modBll = new B_Model();
        public PageSetting setting = new PageSetting();
        public M_Node nodeMod = new M_Node();
        /// <summary>
        /// 当前筛选条件
        /// </summary>
        public Filter_Content filter = new Filter_Content();
        //节点可绑定多模型
        private DataTable _modelDT = null;
        public DataTable ModelDT { get { if (_modelDT == null) { _modelDT = modBll.Sel(); } return _modelDT; } }
        //文章与点击数
        public int Count_DJS = 0;
        public int Count_WZS = 0;
        public int NodeID { get { return nodeMod == null ? 0 : nodeMod.NodeID; } }
        /// <summary>
        /// 获取添加按钮Html,由前端调用(数据已取完)
        /// </summary>
        public string GetAddHtml()
        {
            string html = "";
            if (nodeMod.NodeID < 1 || string.IsNullOrEmpty(nodeMod.ContentModel)) { return html; }
            string[] modelIDArr = nodeMod.ContentModel.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            string temp = "<div class=\"btn-group\" style=\"margin-right:3px;\"><button type=\"button\" class=\"btn btn-default dropdown-toggle\" data-toggle=\"dropdown\">{0}管理<span class=\"caret\"></span></button><ul class=\"dropdown-menu\" role=\"menu\"><li><a href=\"AddContent?ModelID={1}&NodeID={2}\">添加{0}</a></li><li><a href=\"javascript:;\" onclick=\"showimp('{0}',{1});\" >导入{0}</a></li><li><a href=\"ContentManage?ModelID={1}&NodeID={2}\">{0}列表</a></li></ul></div>";
            for (int i = 0; i < modelIDArr.Length; i++)
            {
                DataRow modelDR = SelFromModelDT(DataConverter.CLng(modelIDArr[i]));
                if (modelDR == null) { continue; }
                if (i == 0)
                {
                    //ItemName_L.Text = modelDR["ItemName"].ToString();
                    //RPT.ItemUnit = modelDR["ItemUnit"].ToString();
                    //RPT.ItemName = modelDR["ItemName"].ToString();
                    setting.unit = modelDR["ItemUnit"].ToString() + modelDR["ItemName"].ToString();
                }
                html += String.Format(temp, DataConverter.CStr(modelDR["ItemName"]).Replace(" ", ""), modelDR["ModelID"], nodeMod.NodeID);
            }
            return html;
        }
        //从模型缓存中加载指定数据
        public DataRow SelFromModelDT(int modelid)
        {
            if (ModelDT == null || ModelDT.Rows.Count < 1 || modelid < 1) { return null; }
            DataRow[] drs = ModelDT.Select("ModelID=" + modelid);
            if (drs.Length < 1) { return null; }
            else
            {
                drs[0]["ItemName"] = DataConverter.CStr(drs[0]["ItemName"]);
                return drs[0];
            }
        }
    }
}
