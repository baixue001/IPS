using System;
using System.Data;
using ZoomLa.Model;
using ZoomLa.Common;
using ZoomLa.SQLDAL;
using System.Data.SqlClient;
using ZoomLa.SQLDAL.SQL;
using System.Web;
using Newtonsoft.Json;
using ZoomLa.Model.Page;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;

namespace ZoomLa.BLL
{

    public class B_Page
    {
        public DataTable modeldt = new DataTable();
        /// <summary>
        /// 黄页内容模型列表
        /// </summary>
        public DataTable moddt = new DataTable();
        public B_Page()
        {
            modeldt.Columns.Add(new DataColumn("modelid", typeof(int)));
            modeldt.Columns.Add(new DataColumn("template", typeof(string)));
        }
        public B_Page(string json)
        {
            modeldt.Columns.Add(new DataColumn("modelid", typeof(int)));
            modeldt.Columns.Add(new DataColumn("template", typeof(string)));
            if (string.IsNullOrEmpty(json)) return;
            if (json.Contains("|")) { return; }//旧格式也不予处理
            try
            {
                modeldt = JsonConvert.DeserializeObject<DataTable>(json);
            }
            catch { }
        }
        /// <summary>
        /// 将提交的值转化为DataTable
        /// </summary>
        public string GetSubmitModelChk(HttpRequest Request)
        {
            string ChkModel = Request.Form["ChkModel"];
            foreach (string model in ChkModel.Split(','))
            {
                if (DataConvert.CLng(model) < 1) {continue; }
                DataRow dr = modeldt.NewRow();
                dr["modelid"] = Convert.ToInt32(model);
                dr["template"] = Request.Form["TxtModelTemplate_" + model];
                modeldt.Rows.Add(dr);
            }
            if (modeldt.Rows.Count < 1) { return ""; }
            return JsonConvert.SerializeObject(modeldt);
        }
        public bool IsModelChk(int modelid)
        {
            return modeldt.Select("modelid=" + modelid + "").Length > 0;
        }
        public DataRow GetByModelID(int modelid)
        {
            DataRow[] drs = modeldt.Select("modelid=" + modelid + "");
            if (drs.Length > 0) { return drs[0]; }
            else { return null; }
        }
        public string GetTemplate(int modelid)
        {
            DataRow dr = GetByModelID(modelid);
            if (dr == null) { return ""; }
            else { return DataConvert.CStr(dr["template"]); }
        }
    }
}
