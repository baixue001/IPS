using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using ZoomLa.BLL;

namespace ZoomLaCMS.JS.Plugs.OrgChart
{
    public partial class StructTree : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            B_Structure strBll = new B_Structure();
            DataTable dt = strBll.Sel();
            //name title className children[]
            M_Struct_Tree pmodel = new M_Struct_Tree();
            try
            {
                DTToJson(pmodel, dt, 0);
                ds_hid.Value = JsonConvert.SerializeObject(pmodel.children[0]);
            }
            catch { ds_hid.Value = "[]"; }
        }
        //先加到底,再返回顶部
        public void DTToJson(M_Struct_Tree pmodel, DataTable dt, int pid)
        {
            //第一级一般只有一个
            DataRow[] drs = dt.Select("ParentID=" + pid);
            if (drs.Length < 1) { return; }
            foreach (DataRow dr in drs)
            {
                M_Struct_Tree model = new M_Struct_Tree();
                model.name = dr["name"].ToString();
                model.title = dr["name"].ToString();
                model.layer = pmodel.layer + 1;
                model.className = "layer" + model.layer;
                DTToJson(model, dt, Convert.ToInt32(dr["ID"]));
                //json.Add("className");
                if (pmodel.children == null) { pmodel.children = new List<M_Struct_Tree>(); }
                pmodel.children.Add(model);
            }
        }
    }
    public class M_Struct_Tree
    {
        public int layer = 0;
        public string name = "";
        public string title = "";
        public string className = "";
        public List<M_Struct_Tree> children = null;
    }
}