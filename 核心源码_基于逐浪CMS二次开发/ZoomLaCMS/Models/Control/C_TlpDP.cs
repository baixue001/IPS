using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ZoomLa.Common;
using ZoomLa.Components;

namespace ZoomLaCMS.Control
{
    public class C_TlpDP

    {
        //控件id与名称
        public string name = "";
        //模板预览URL,无则不显示预览图标
        public string preurl = "";
        //选中值
        public string seled = "";
        private DataTable _dt = null;
        public DataTable dt
        {
            get
            {
                if (_dt == null)
                {

                    _dt = GetDTForTemplate();
                }
                return _dt;
            }
            set { _dt = value; }
        }
        public C_TlpDP(string name, string seled, string preulr = "", DataTable dt = null)
        {
            this.name = name;
            this.preurl = preulr;
            this.seled = seled;
            _dt = dt;
        }
        public DataTable GetDTForTemplate()
        {
            DataTable tables = FileSystemObject.GetDirectoryAllInfos(function.VToP(SiteConfig.SiteOption.TemplateDir), FsoMethod.All);
            tables.DefaultView.RowFilter = "type=1 OR name LIKE '%.html'";
            DataTable newtables =FileSystemObject.FiterHasFile(tables.DefaultView.ToTable());

            //追加根目录文件
            foreach (DataRow dr in tables.Rows)
            {
                //如果是html文件，如放置在根目录
                if (dr["type"].Equals("2") && Path.GetExtension(dr["name"].ToString()).Equals(".html"))
                {
                    DataRow[] temprows = newtables.Select("name='" + dr["name"] + "'");
                    if (temprows.Length == 0)
                    {
                        DataRow child = newtables.NewRow();
                        child["type"] = 3;
                        child["path"] = dr["path"];
                        child["rname"] = dr["rname"].ToString();
                        child["name"] = dr["name"];
                        child["pid"] = 0;
                        newtables.Rows.Add(child);
                    }
                }
            }
            string templateDir = SiteConfig.SiteOption.TemplateDir;
            //忽略部分目录，并替换路径
            for (int i = 0; i < newtables.Rows.Count; i++)
            {
                DataRow dr = newtables.Rows[i];
                if (dr["rname"].ToString().Contains(@"\views")) { dr["rname"] = "ignore"; }
                else
                {
                    dr["rname"] = function.PToV(dr["rname"].ToString()).Replace(templateDir,"").TrimStart('/');
                }
            }

            newtables.DefaultView.RowFilter = "rname not in ('ignore')";
            newtables = newtables.DefaultView.ToTable();
            //加一条空模板数据
            DataRow empdr = newtables.NewRow();
            empdr["type"] = 4;
            empdr["path"] = "";
            empdr["rname"] = "";
            empdr["name"] = "不指定模板!";
            empdr["pid"] = 0;
            newtables.Rows.InsertAt(empdr, 0);
            return newtables;
        }
    }
}
