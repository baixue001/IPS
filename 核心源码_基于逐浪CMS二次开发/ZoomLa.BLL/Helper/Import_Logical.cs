
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using ZoomLa.SQLDAL;

namespace ZoomLa.BLL.Helper
{
    public class Import_Logical
    {
        //1,DataSet加载XML时,报没有 Unicode 字节顺序标记。不能切换到 Unicode
        //答:不使用DataSet,直接使用XML反序列化
        /// <summary>
        /// 从数据库中获取结构,生成XML导入配置文件
        /// </summary>
        /// <param name="dt">需要生成的数据表(必须有表名)</param>
        /// <param name="xmlPath">存储的XML物理路径</param>
        public static void CreateXMLByDT(DataTable dt, string xmlPath)
        {
            if (!Directory.Exists(Path.GetDirectoryName(xmlPath))) { Directory.CreateDirectory(Path.GetDirectoryName(xmlPath)); }
            if (File.Exists(xmlPath)) { File.Delete(xmlPath); }
            List<Import_Field> list = new List<Import_Field>();
            //-----------------------------
            foreach (DataColumn dc in dt.Columns)
            {
                if (dc.ColumnName.ToUpper().Equals("ID")) { continue; }
                Import_Field model = new Import_Field();
                model.TableName = dt.TableName;
                model.FieldName = dc.ColumnName;
                model.FieldAlias = "";//生成文件后手动填入
                model.FieldType = dc.DataType.ToString().Split('.')[1];
                if (model.FieldType.Equals("DateTime"))
                {
                    model.DefaultValue = "{date}";
                }
                list.Add(model);
            }
            using (StringWriter stringWriter = new StringWriter(new StringBuilder()))
            {
                //XmlSerializerNamespaces namespaces = new XmlSerializerNamespaces();
                //namespaces.Add("", "");
                XmlSerializer xmlSerializer = new XmlSerializer(typeof(List<Import_Field>));
                xmlSerializer.Serialize(stringWriter, list);
                FileStream fs = new FileStream(xmlPath, FileMode.OpenOrCreate);
                StreamWriter sw = new StreamWriter(fs, Encoding.UTF8);
                sw.Write(stringWriter.ToString());
                sw.Close();
                fs.Close();
            }
        }
        /// <summary>
        /// 通过配置文件生成给用户导入的CSV文件
        /// </summary>
        /// <param name="xmlPath">配置XML位置</param>
        /// <returns>SafeC.DownStr(csv, "test.xls",Encoding.Default);</returns>
        public static string CreateCSVByXML(string xmlPath)
        {
            List<Import_Field> list = GetListByXML(xmlPath);
            StringBuilder sb = new StringBuilder();
            foreach (Import_Field model in list)
            {
                if (!model.IsShow) { continue; }
                string alias = !string.IsNullOrEmpty(model.FieldAlias) ? model.FieldAlias : model.FieldName;
                if (string.IsNullOrEmpty(model.FieldName)) { throw new Exception("[" + xmlPath + "]中有字段名称为空"); }
                sb.Append(alias + "\t");
            }
            sb.Append("\n");
            return sb.ToString();
        }
        /// <summary>
        /// 根据XML生成Excel并自动下载
        /// </summary>
        public static void Excel_CreateByXML(string xmlPath,string name)
        {
            //List<Import_Field> list = GetListByXML(xmlPath);
            //MemoryStream stream = new MemoryStream();
            //Workbook workbook = new Workbook(stream);
            //Worksheet sheet = workbook.Worksheets[0];
            //Cells cells = sheet.Cells;
            //int cellIndex = 0;
            //foreach (Import_Field model in list)
            //{
            //    if (!model.IsShow) { continue; }
            //    string alias = !string.IsNullOrEmpty(model.FieldAlias) ? model.FieldAlias : model.FieldName;
            //    if (string.IsNullOrEmpty(model.FieldName)) { throw new Exception("[" + xmlPath + "]中有字段名称为空"); }
            //    cells[0, cellIndex].PutValue(alias);
            //    cellIndex++;
            //}
            //System.Web.HttpContext.Current.Response.Clear();
            //workbook.Save(System.Web.HttpContext.Current.Response, name + ".xlsx", ContentDisposition.Attachment, new XlsSaveOptions(SaveFormat.Xlsx));
        }
        /// <summary>
        /// 将Excel转为DataTable
        /// 原生的有Bug,最后一行无法导出
        /// </summary>
        /// <param name="xmlPath">XML配置文件路径</param>
        /// <param name="stream">上传文件的流</param>
        /// <returns></returns>
        public static DataTable Excel_ToDT(string xmlPath, Stream stream)
        {
            //将数据填充模型,写入数据库
            //Workbook workbook = new Workbook(stream);
            //Cells cells = workbook.Worksheets[0].Cells;
            //DataTable dt = new DataTable();
            //for (int i = 0; i <= cells.MaxColumn; i++)
            //{
            //    dt.Columns.Add(new DataColumn(cells[0, i].Value.ToString()));
            //}
            ////将数据填充成DataTable
            //for (int i = 1; i < (cells.MaxRow + 1); i++)
            //{
            //    DataRow dr = dt.NewRow();
            //    for (int j = 0; j < dt.Columns.Count; j++)
            //    {
            //        dr[j] = cells[i, j].Value;
            //    }
            //    dt.Rows.Add(dr);
            //}
            ////将字段别名还原为字段名,填充默认值,增加上不予前端显示的字段
            //List<Import_Field> list = Import_Logical.GetListByXML(xmlPath);
            //foreach (Import_Field field in list)
            //{
            //    try
            //    {
            //        //将别名改回字段名
            //        if (!string.IsNullOrEmpty(field.FieldAlias) && dt.Columns.Contains(field.FieldAlias))
            //        {
            //            dt.Columns[field.FieldAlias].ColumnName = field.FieldName;
            //        }
            //        //检测是否设了前端不显示
            //        if (field.IsShow == false)
            //        {
            //            //如果提交的CSV中有该列,则直接移除后再加上
            //            if (dt.Columns[field.FieldName] != null) { dt.Columns.Remove(field.FieldName); }
            //            dt.Columns.Add(field.FieldName, System.Type.GetType("System." + field.FieldType));
            //        }
            //        //如果有设定了默认值,则对其中空值进行处理
            //        if (!string.IsNullOrEmpty(field.DefaultValue))
            //        {
            //            if (field.DefaultValue == "{date}") { field.DefaultValue = DateTime.Now.ToString(); }
            //            //检测dt中是否有空值存在,有则替换
            //            DataRow[] drs = dt.Select(field.FieldName + "='' OR " + field.FieldName + " IS null");
            //            foreach (DataRow dr in drs)
            //            {
            //                dr[field.FieldName] = field.DefaultValue;
            //            }
            //        }
            //        else
            //        {
            //            //未设定默认值,根据数据类型,将值处理
            //            DataRow[] drs = dt.Select(field.FieldName + "='' OR " + field.FieldName + " IS null");
            //            foreach (DataRow dr in drs)
            //            {
            //                dr[field.FieldName] = GetDefValueByDataType(field.FieldType);
            //            }
            //        }
            //    }
            //    catch (Exception ex) { throw new Exception(field.FieldName+":"+ex.Message); }
            //}
            //dt.TableName = list[0].TableName;
            ////检测是否缺少字段,将其补齐
            //if (!string.IsNullOrEmpty(dt.TableName))
            //{
            //    DataTable dbDT = DBCenter.Sel(dt.TableName, "1=2");
            //    foreach (DataColumn dc in dbDT.Columns)
            //    {
            //        if (dt.Columns.Contains(dc.ColumnName)) { continue; }
            //        dt.Columns.Add(new DataColumn(dc.ColumnName, dc.DataType));
            //    }
            //}
            //return dt;
            return null;
        }
        /// <summary>
        /// 根据XML还原出List
        /// </summary>
        public static List<Import_Field> GetListByXML(string xmlPath)
        {
            if (!File.Exists(xmlPath)) { throw new Exception("文件["+xmlPath+"]不存在"); }
            List<Import_Field> list = null;
            using (TextReader reader = new StreamReader(xmlPath))
            {
                XmlSerializer xmlSerializer = new XmlSerializer(typeof(List<Import_Field>));
                list = xmlSerializer.Deserialize(reader) as List<Import_Field>;
            }
            return list;
        }
        private static object GetDefValueByDataType(string fieldType)
        {
            object result = "";
            switch (fieldType)
            {
                case "Boolean":
                    result = false;
                    break;
                case "String":
                    break;
                case "Int32":
                    result = 0;
                    break;
                case "DateTime":
                    result = DateTime.Now.ToString();
                    break;
                default:
                    result = "";
                    break;
            }
            return result;
        }
    }
    public class Import_Field
    {
        /// <summary>
        /// 字符所属表(支持内容(主表与副表插入))
        /// </summary>
        public string TableName = "";
        /// <summary>
        /// 数据库字段名
        /// </summary>
        public string FieldName = "";
        /// <summary>
        /// 字段别名(显示的Excel头)
        /// </summary>
        public string FieldAlias = "";
        /// <summary>
        /// 数据库字段类型(可直接使用DataTable中的类型)
        /// </summary>
        public string FieldType = "";
        /// <summary>
        /// 是否有默认值(可支持特殊值,如{date},{uname})
        /// </summary>
        public string DefaultValue = "";
        /// <summary>
        /// 是否需要显示在Excel中,允许用户填值导入
        /// </summary>
        public bool IsShow = true;
    }
    /*
        string xmlPath = function.VToP("/Config/Import/CRM_Client.xml");
        DataTable dt = DBCenter.Sel("ZL_CRMS_Client");
        dt.TableName = "ZL_CRMS_Client";
        Import_Logical.CreateXMLByDT(dt, xmlPath);
        string csv = Import_Logical.CreateCSVByXML(xmlPath);
        SafeC.DownStr(csv, "test.xls",Encoding.Default);
     */
}
