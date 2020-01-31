using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
//用于管理XMLDB下的文件,并提供支持方法
//仅用于信息读取,写入需要手动维护
namespace ZoomLa.Components
{
    public class XMLDB
    {
        private static DataTable _zlfont = null;
        public static DataTable ZLFont
        {
            get
            {
                if (_zlfont == null)
                {
                    string xmlPath = AppDomain.CurrentDomain.BaseDirectory + @"Config\XMLDB\ZLFont.xml";
                    if (!File.Exists(xmlPath)) { return new DataTable(); }
                    else
                    {
                        DataSet ds = new DataSet();
                        ds.ReadXml(xmlPath);
                        _zlfont = ds.Tables[0];
                    }
                }
                return _zlfont;
            }
            set { _zlfont = value; }//将其设为null则重载
        }
        public static DataRow SelReturnModel(DataTable dt, string field, string value)
        {
            DataRow[] drs = dt.Select(field + "='" + value + "'");
            if (drs.Length > 0) { return drs[0]; }
            else { return null; }
        }
        /// <summary>
        /// 加载指定的XML
        /// </summary>
        /// <param name="name">虚拟路径,或ZL_Font.xml</param>
        /// <returns>如读取失败,返回空表,非null值</returns>
        public static DataTable SelDT(string name)
        {
            string xmlPath = "";
            if (name.StartsWith("/"))
            {
                xmlPath = AppDomain.CurrentDomain.BaseDirectory + name.Replace("/", "\\");
            }
            else { xmlPath = AppDomain.CurrentDomain.BaseDirectory + @"Config\XMLDB\" + name; }
            if (!File.Exists(xmlPath)) { return new DataTable(); }
            DataSet ds = new DataSet();
            ds.ReadXml(xmlPath);
            return ds.Tables[0];
        }
        /// <summary>
        /// 更新XML配置文件
        /// </summary>
        public static void Update(string name, DataTable dt)
        {
            string xmlPath = "";
            if (name.StartsWith("/"))
            {
                xmlPath = AppDomain.CurrentDomain.BaseDirectory + name.Replace("/", "\\");
            }
            else { xmlPath = AppDomain.CurrentDomain.BaseDirectory + @"Config\XMLDB\" + name; }
            string xml = DTToXml(dt);
            if (!Directory.Exists(Path.GetDirectoryName(xmlPath))) { Directory.CreateDirectory(Path.GetDirectoryName(xmlPath)); }
            File.WriteAllText(xmlPath, xml);
        }
        /// <summary>
        /// 将DataTable序列化为可存储的XML字符串
        /// </summary>
        public static string DTToXml(DataTable dt)
        {
            if (string.IsNullOrEmpty(dt.TableName))
            {
                dt.TableName = "Item";
            }
            string xml_content = "";
            foreach (DataRow dr in dt.Rows)
            {
                string xml_item = "";
                foreach (DataColumn dc in dt.Columns)
                {
                    xml_item += string.Format("<{0}>{1}</{0}>\n", dc.ColumnName, dr[dc.ColumnName]);
                }
                xml_item = "<" + dt.TableName + ">\n" + xml_item + "</" + dt.TableName + ">\n";
                xml_content += xml_item;
            }
            xml_content = "<?xml version=\"1.0\" standalone=\"yes\"?>\n<XMLDB>\n" + xml_content + "\n</XMLDB>";
            return xml_content;
        }
        public static void DelByID(DataTable dt,string value, string field = "id")
        {
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                if ((dt.Rows[i][field] + "") == value) { dt.Rows.Remove(dt.Rows[i]); }
            }
        }
    }
}
