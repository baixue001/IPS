using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using ZoomLa.SQLDAL;

namespace ZoomLaCMS.Control { 
    public class C_BootTreeView
    {
        public string textTlp = "";//@Name
        public string icon = "";
        //允许选定
        public bool selectable = true;
        //已选定的值,暂只支持一个,传入ID
        public string value = "";
        public string CreateTreeView(DataTable dt)
        {
            C_TreeItem item = new C_TreeItem();
            CreateTreeView(dt, item);
            string result = JsonConvert.SerializeObject(item.nodes,
                   new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore });
            return result;
        }

        public void CreateTreeView(DataTable dt, C_TreeItem item)
        {
            DataRow[] drs = dt.Select("ParentID=" + item.id);
            if (drs.Length < 1) {  }
            foreach (DataRow dr in drs)
            {
                C_TreeItem node = new C_TreeItem();
                node.id = Convert.ToInt32(dr["id"]);
                node.icon = icon;
                node.text = TlpDeal(textTlp, dr);//dr["Name"].ToString();
                node.href = node.id.ToString();
                node.pid = item.id;
                node.selectable = selectable;
                if (node.id.ToString() == value) { node.state = new C_TreeItem_State() { selected = true }; }
                item.AddNode(node);
                CreateTreeView(dt, node);
            }
        }
        //替换模板中的值
        private string TlpDeal(string tlp, DataRow dr)
        {
            foreach (DataColumn dc in dr.Table.Columns)
            {
                tlp = tlp.Replace("@" + dc.ColumnName, DataConvert.CStr(dr[dc.ColumnName]));
            }
            return tlp;
        }
        public class C_TreeItem
        {
            public string text = "";
            public string href = "";
            public string icon = null;
            public List<C_TreeItem> nodes = null;
            public bool selectable = false;
            public C_TreeItem_State state = null;
            [JsonIgnore]
            public int id = 0;
            [JsonIgnore]
            public int pid = 0;
            public void AddNode(C_TreeItem node)
            {
                if (nodes == null) { nodes = new List<C_TreeItem>(); }
                nodes.Add(node);
            }
        }
        public class C_TreeItem_State
        {
            public bool @checked = false;
            public bool disabled = false;
            public bool selected = false;
            public bool expanded = false;
        }
    }
}
