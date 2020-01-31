using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using ZoomLa.Common;
using ZoomLa.Model;
using ZoomLa.SQLDAL;

namespace ZoomLa.BLL
{
    /*
     * BLL层的静态公用方法类
     */
    public class BLLCommon
    {
        public static readonly string Version = "V8|190131";
        public static readonly string Version_JS = "0ff34ce20190131";//用于JS与css版本控制
        public static readonly string Version_WorkTable = "当前版本：CMS V8 Beta版，数据引擎：SQL Server 2005及更高版本";//WorkTable
        public static readonly string Version_CMS = "逐浪CMS V8 Beta版";//SystemFinger
        //----------------------------
        public static StringBuilder ueditorMin = new StringBuilder();//简洁版编辑器(不允许上传图片,附件)
        public static StringBuilder ueditorMinEx = new StringBuilder();//简洁版编辑器(带上传附件)
        public static StringBuilder ueditorMid = new StringBuilder();//标准版
        public static StringBuilder ueditorBar = new StringBuilder();//贴吧等使用,带表情,视图,多图片和代码
        public static StringBuilder ueditorNom = new StringBuilder();//聊天等地使用,带表情(无全屏)
        public static StringBuilder ueditorDesign = new StringBuilder();//用于design,可修改html
        //public static StringBuilder ueFormula = new StringBuilder();//用于教师
        static BLLCommon()
        {
            ueditorMin.AppendLine(" toolbars : [['FullScreen','Undo', 'Redo','Bold', 'Italic', 'NumberedList', 'BulletedList',   'Smiley', 'ShowBlocks', 'Maximize', 'underline', 'fontborder', 'strikethrough', '|', 'justifyleft', 'justifycenter', 'justifyright', 'justifyjustify']]");
            ueditorMinEx.AppendLine(" toolbars : [['FullScreen','Undo', 'Redo','Bold', 'Italic', 'NumberedList', 'BulletedList',   'Smiley', 'ShowBlocks', 'Maximize', 'underline', 'fontborder', 'strikethrough', '|', 'justifyleft', 'justifycenter', 'justifyright', 'justifyjustify','simpleupload','insertimage','attachment']]");
            ueditorMid.AppendLine("toolbars : [['fullscreen','|', 'undo', 'redo', '|',");
            ueditorMid.AppendLine("'bold', 'italic', 'underline', 'fontborder', 'strikethrough', 'superscript', 'subscript', 'removeformat', 'formatmatch', 'autotypeset', 'blockquote', 'pasteplain', '|', 'forecolor', 'backcolor', 'insertorderedlist', 'insertunorderedlist', 'selectall', 'cleardoc', '|',");
            ueditorMid.AppendLine("'rowspacingtop', 'rowspacingbottom', 'lineheight', '|',");
            ueditorMid.AppendLine(" 'customstyle', 'paragraph', 'fontfamily', 'fontsize', '|',");
            ueditorMid.AppendLine(" 'directionalityltr', 'directionalityrtl', 'indent', '|','simpleupload','insertimage','emotion','attachment','map',");
            ueditorMid.AppendLine(" 'justifyleft', 'justifycenter', 'justifyright', 'justifyjustify', '|', 'touppercase', 'tolowercase', '|',");
            ueditorMid.AppendLine(" 'link', 'unlink', 'anchor', '|', 'pagebreak','|','template', 'horizontal', 'date', 'time', '|', 'spechars', 'inserttable', 'deletetable', 'insertparagraphbeforetable', 'insertrow', 'deleterow', 'insertcol', 'deletecol', 'mergecells', 'mergeright', 'mergedown', 'splittocells', 'splittorows', 'splittocols', 'kityformula','|', 'print', 'preview', 'searchreplace']]");
            ueditorBar.AppendLine(" toolbars : [['FullScreen','Undo', 'Redo','Bold', 'Italic', 'NumberedList', 'BulletedList',   'Smiley', 'ShowBlocks', 'Maximize', 'underline', 'fontborder', 'strikethrough', '|', 'justifyleft', 'justifycenter', 'justifyright', 'justifyjustify','insertcode','simpleupload','insertimage','insertvideo','emotion','attachment','map']]");
            ueditorNom.AppendLine(" toolbars : [['Undo', 'Redo','Bold', 'Italic', 'NumberedList', 'BulletedList',   'Smiley', 'ShowBlocks', 'Maximize', 'underline', 'fontborder', 'strikethrough', '|', 'justifyleft', 'justifycenter', 'justifyright', 'justifyjustify','emotion']]");
            ueditorDesign.AppendLine(" toolbars : [['source', '|','Undo', 'Redo','Bold', 'Italic', 'NumberedList', 'BulletedList',   'Smiley', 'ShowBlocks', 'Maximize', 'underline', 'fontborder', 'strikethrough', '|', 'justifyleft', 'justifycenter', 'justifyright', 'justifyjustify','insertcode','simpleupload','insertimage','insertvideo','emotion','attachment','map']]");
            //-----
        }
        #region M_Base
        /// <summary>
        /// 获取字段窜
        /// </summary>
        public static string GetFields(M_Base model)
        {
            string str = string.Empty, PK = model.PK.ToLower();
            string[,] strArr = model.FieldList();
            for (int i = 0; i < strArr.GetLength(0); i++)
            {
                if (strArr[i, 0].ToLower() != PK)
                {
                    str += "[" + strArr[i, 0] + "],";
                }
            }
            return str.Substring(0, str.Length - 1);
        }
        /// <summary>
        /// 获取参数串
        /// </summary>
        public static string GetParas(M_Base model)
        {
            string str = string.Empty, PK = model.PK.ToLower();
            string[,] strArr = model.FieldList();
            for (int i = 0; i < strArr.GetLength(0); i++)
            {
                if (strArr[i, 0].ToLower() != PK)
                {
                    str += "@" + strArr[i, 0] + ",";
                }
            }
            return str.Substring(0, str.Length - 1);
        }
        /// <summary>
        /// 获取字段=参数(Update)
        /// </summary>
        public static string GetFieldAndPara(M_Base model)
        {
            string str = string.Empty, PK = model.PK.ToLower();
            string[,] strArr = model.FieldList();
            for (int i = 0; i < strArr.GetLength(0); i++)
            {
                if (strArr[i, 0].ToLower() != PK)
                {
                    str += "[" + strArr[i, 0] + "]=@" + strArr[i, 0] + ",";
                }
            }
            return str.Substring(0, str.Length - 1);
        }
        #endregion
        #region DataRow
        public static string GetParas(DataRow dr, string pk = "id")
        {
            DataTable dt = dr.Table;
            string str = string.Empty;
            foreach (DataColumn col in dt.Columns)
            {
                if (col.ColumnName.Equals(pk, StringComparison.CurrentCultureIgnoreCase)) continue;
                str += "@" + col.ColumnName + ",";
            }
            return str.TrimEnd(',');
        }
        public static string GetFields(DataRow dr, string pk = "id")
        {
            DataTable dt = dr.Table;
            string str = string.Empty;
            foreach (DataColumn col in dt.Columns)
            {
                if (col.ColumnName.Equals(pk, StringComparison.CurrentCultureIgnoreCase)) continue;
                str += "[" + col.ColumnName + "],";
            }
            return str.TrimEnd(',');
        }
        public static string GetFieldAndPara(DataRow dr, string pk = "id")
        {
            DataTable dt = dr.Table;
            string str = string.Empty;
            foreach (DataColumn col in dt.Columns)
            {
                if (col.ColumnName.Equals(pk, StringComparison.CurrentCultureIgnoreCase)) continue;
                str += "[" + col.ColumnName + "]=@" + col.ColumnName + ",";
            }
            return str.TrimEnd(',');
        }
        public static SqlParameter[] GetParameters(DataRow dr, string pk = "id")
        {
            DataTable dt = dr.Table;
            List<SqlParameter> listSP = new List<SqlParameter>();
            foreach (DataColumn col in dt.Columns)
            {
                if (col.ColumnName.Equals(pk, StringComparison.CurrentCultureIgnoreCase)) continue;
                listSP.Add(new SqlParameter(col.ColumnName, dr[col.ColumnName]));
            }
            return listSP.ToArray();
        }
        #endregion
        #region 用于模型
        public static string GetFields(DataTable dt, string pk = "id")
        {
            string fields = "";
            foreach (DataRow dr in dt.Rows)
            {
                string field = DataConvert.CStr(dr["FieldName"]);
                switch (DBCenter.DB.DBType)
                {
                    case "oracle":
                        fields += WrapField(field) + ",";
                        break;
                    case "mysql":
                        fields += "`" + field + "`,";
                        break;
                    default:
                        fields += "[" + field + "],";
                        break;
                }
            }
            fields = fields.TrimEnd(',');
            return fields;
        }
        public static string GetParas(DataTable dt, string pk = "id")
        {
            string fields = "";
            foreach (DataRow dr in dt.Rows)
            {
                switch (DBCenter.DB.DBType)
                {
                    case "oracle":
                        fields += ":" + dr["FieldName"].ToString() + ",";
                        break;
                    default:
                        fields += "@" + dr["FieldName"] + ",";
                        break;
                }
            }
            fields = fields.TrimEnd(',');
            return fields;
        }
        public static string GetFieldAndPara(DataTable dt, string pk = "id")
        {
            string sets = "";
            foreach (DataRow dr in dt.Rows)
            {
                string field = DataConvert.CStr(dr["FieldName"]);
                switch (DBCenter.DB.DBType)
                {
                    case "oracle":
                        sets += WrapField(field) + "=:" + field + ",";
                        break;
                    case "mysql":
                        sets += "`" + field + "`" + "=@" + field + ",";
                        break;
                    default:
                        sets += "[" + field + "]" + "=@" + field + ",";
                        break;
                }
            }
            sets = sets.TrimEnd(',');
            return sets;
        }
        public static List<SqlParameter> GetParameters(DataTable dt, string pk = "id")
        {
            List<SqlParameter> splist = new List<SqlParameter>();
            foreach (DataRow dr in dt.Rows)
            {
                splist.Add(new SqlParameter(dr["FieldName"].ToString(), dr["FieldValue"]));
            }
            return splist;
        }
        private static string WrapField(string field) { if (DBHelper.IsKeyWord(field)) { field = "\"" + field + "\""; } return field; }
        #endregion
        //-----
        public static string GetXmlByNode(string node)
        {
            string result = "", ppath = function.VToP("/Config/Souce.config");
            XmlDocument xmldoc = new XmlDocument();
            if (File.Exists(ppath))
            {
                xmldoc.Load(ppath);
                result = xmldoc.SelectSingleNode("//Bll/" + node).InnerText;
            }
            return result;
        }
    }
    //默认为成功,失败则需要写原因
    public class CommonReturn
    {
        public static CommonReturn Success()
        {
            CommonReturn model = new CommonReturn();
            return model;
        }
        public static CommonReturn Success(object instance)
        {
            CommonReturn model = new CommonReturn();
            model.model = instance;
            return model;
        }
        public static CommonReturn Failed(string msg)
        {
            CommonReturn model = new CommonReturn();
            model.isok = false;
            model.err = msg;
            return model;
        }
        public bool isok = true;
        public string err = "";
        public string addon = "";
        public object model = null;
    }
    public class Com_Filter
    {
        public string ids = "";
        public string uids = "";
        public string nids = "";
        public string status = "";
        public string type = "-100";
        public string skey = "";
        public string uname = "";

        public int storeId = -100;
        //因为父級是从0开始
        public int pid = -100;
        /// <summary>
        /// 是否包含回收站数据
        /// </summary>
        public bool isRecycle = false;
        public string addon = "";
        public string mode = "";
        //排序
        public string orderBy = "";
    }
}
