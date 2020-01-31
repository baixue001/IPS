using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Reflection;
using System.Text;
using System.Web;
using System.Xml;
using System.Xml.Serialization;
using ZoomLa.Common;
using ZoomLa.SQLDAL;
using System.Linq;

namespace ZoomLaCMS.Common
{
    public class DataTableHelper
    {
        //=================================================批量插入建议使用SqlBulk数据库
        /// <summary>
        /// 使用DT中的数据更新目标数据库,可多或少字段，有则更新，无则插入
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="targetDT"></param>
        /// <param name="pk">身份标识,如SiteID</param>
        public bool UpdateDataToDB(DataTable dt, DataTable targetDT, string pk)
        {
            //上传SQL字符限制为1500
            //插入的文本中也有可能带有,所以另选其他作为分割符,或直接将其获取之后赋值
            if (dt == null || dt.Rows.Count == 0)
            {
                return true;
            }
            else if (string.IsNullOrEmpty(dt.TableName)) //未指定表名
            { throw new Exception("请先设定表名TableName"); return false; }
            //------判断完后正式插入,以目标表中的字段为准，只取有的字段
            string tname = dt.TableName;
            string colNames = "", spName = "";
            for (int i = 1; i < targetDT.Columns.Count; i++)//0是主键,所以从1开始,直接获取的无法判断哪个是主键
            {
                if (dt.Columns[targetDT.Columns[i].ColumnName] != null)//内存表拥有目标表的字段，才会拷贝
                {
                    colNames += "" + targetDT.Columns[i].ColumnName + ",";
                    spName += "@" + targetDT.Columns[i].ColumnName + ",";
                }
            }
            colNames = colNames.TrimEnd(','); spName = spName.TrimEnd(',');
            string[] colArr = colNames.Split(',');//存储列名的数组
            string colValues;
            //--生成插入语句
            string insertSql = string.Format("Insert into {0}({1}) values({2});", tname, colNames, spName);//表名，列名,占位符名
                                                                                                           //--生成更新语句
            string updateSql = string.Format("Update {0} Set ", tname, colNames, spName);
            for (int i = 0; i < colNames.Split(',').Length; i++)
            {
                updateSql += colNames.Split(',')[i] + " = " + spName.Split(',')[i] + ",";
            }
            updateSql = updateSql.TrimEnd(',');//Update tname set colname1=@spName1,colname2=@spName2
                                               //--
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                colValues = "";
                SqlParameter[] sp = new SqlParameter[colArr.Length];
                for (int j = 0; j < colArr.Length; j++)
                {
                    string cName = colArr[j];
                    if (dt.Rows[i][cName].GetType() == typeof(DBNull))
                    {
                        colValues = "NULL";
                    }
                    else if (targetDT.Columns[cName].DataType == typeof(string))
                        colValues = dt.Rows[i][cName].ToString();
                    else if (targetDT.Columns[cName].DataType == typeof(int) || dt.Columns[cName].DataType == typeof(float) || dt.Columns[cName].DataType == typeof(double))
                    {
                        colValues = dt.Rows[i][cName].ToString();
                    }
                    else if (targetDT.Columns[cName].DataType == typeof(DateTime))
                    {
                        colValues = dt.Rows[i][cName].ToString();
                    }
                    else if (targetDT.Columns[cName].DataType == typeof(bool))
                    {
                        colValues = dt.Rows[i][cName].ToString();
                    }
                    else
                        colValues = dt.Rows[i][cName].ToString();

                    if (colValues != "NULL")//如果为空,则插入Null值,避免表中Template字段被赋值
                        sp[j] = new SqlParameter(colArr[j], colValues);
                    else
                        sp[j] = new SqlParameter(colArr[j], DBNull.Value);
                }//列处理完成

                //有则更新,无则插入
                if (SqlHelper.ExecuteTable(CommandType.Text, "Select * From " + dt.TableName + " Where " + pk + " ='" + dt.Rows[i][pk] + "'").Rows.Count > 0)
                {
                    SqlHelper.ExecuteScalar(CommandType.Text, updateSql + " Where " + pk + " = " + dt.Rows[i][pk], sp);
                }
                else
                {
                    SqlHelper.ExecuteScalar(CommandType.Text, insertSql, sp);
                }
            }//for end;
            return true;
        }
        //=================================================序列化
        /// <summary>
        /// 将DataTable序列化为字符串返回,必须有TableName
        /// </summary> 
        /// <param name="pDt">需要序列化的DataTable</param> 
        /// <returns>序列化的DataTable</returns> 
        public static string SerializeDT(DataTable dt)
        {
            StringBuilder sb = new StringBuilder();
            XmlWriter writer = XmlWriter.Create(sb);
            XmlSerializer serializer = new XmlSerializer(typeof(DataTable));
            serializer.Serialize(writer, dt);
            XmlDocument xmlDoc = new XmlDocument();
            writer.Close();
            return sb.ToString();
        }
        /// <summary>
        ///  DataTable序列化,XML持久化存在本地,必须有TableName
        /// (推荐使用Json)
        /// 1,DataTable修改后存XML体积会加倍,比JSON大10倍
        /// 2,普通状态下也比Json大5倍
        /// </summary>
        /// <param name="dt">需要序列化的DataTable</param>
        /// <param name="path">物理路径</param>
        public static string SerializeDT(DataTable dt, string path)
        {
            if (path.StartsWith("/")) { throw new Exception("请指定物理路径"); }
            StringBuilder sb = new StringBuilder();
            XmlWriter writer = XmlWriter.Create(sb);
            XmlSerializer serializer = new XmlSerializer(typeof(DataTable));
            serializer.Serialize(writer, dt);
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(sb.ToString());//如果需要持久化存储,可如此
            xmlDoc.Save(path);
            writer.Close();
            return sb.ToString();
        }
        /// <summary> 
        /// 反序列化DataTable 
        /// </summary> 
        /// <param name="xmlStr">Xml字符串</param> 
        /// <returns>DataTable</returns> 
        public static DataTable DeserializeFromXML(string xmlStr)
        {
            StringReader strReader = new StringReader(xmlStr);
            XmlReader xmlReader = XmlReader.Create(strReader);
            XmlSerializer serializer = new XmlSerializer(typeof(DataTable));
            DataTable dt = serializer.Deserialize(xmlReader) as DataTable;
            return dt;
        }
        /// <summary> 
        /// 从物理路径反序列化DataTable 
        /// </summary> 
        /// <param name="xmlPath">序列化的DataTable的物理路径</param> 
        /// <returns>DataTable</returns> 
        public static DataTable DeserializeFromPath(string xmlPPath)
        {
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(xmlPPath);
            return DeserializeFromXML(xmlDoc.InnerXml);
        }
        //=================================================Common
        /// <summary>
        /// 获取dt中的列名
        /// </summary>
        public string[] GetColumnsName(DataTable dt)
        {
            if (dt == null || dt.Columns.Count < 1) { return (new string[] { "" }); }
            string[] s = new string[dt.Columns.Count];
            for (int i = 0; i < dt.Columns.Count; i++)
            {
                s[i] = dt.Columns[i].ColumnName;
            }
            return s;
        }
        /// <summary>
        /// 过滤重复行
        /// </summary>
        /// <param name="FieldName">字段名,暂只支持一个</param>
        public DataTable DistinctByField(DataTable dt, string FieldName)
        {
            if (dt == null || dt.Rows.Count < 1) { return dt; }
            DataTable returnDt = new DataTable();
            returnDt = dt.Copy();//将原DataTable复制一个新的
            if (dt.Rows.Count < 2) { return dt; }
            DataRow[] drs = returnDt.Select("", FieldName);//将DataTable按指定的字段排序
            object LastValue = null;
            for (int i = 0; i < drs.Length; i++)
            {
                if ((LastValue == null) || (!(ColumnEqual(LastValue, drs[i][FieldName]))))
                {
                    LastValue = drs[i][FieldName];
                    continue;
                }
                drs[i].Delete();
            }
            returnDt.AcceptChanges();//否则数据仍是十条,循环会报错
            return returnDt;
        }
        //辅助SelectDistinctByField
        private bool ColumnEqual(object A, object B)
        {
            // Compares two values to see if they are equal. Also compares DBNULL.Value.
            // Note: If your DataTable contains object fields, then you must extend this
            // function to handle them in a meaningful way if you intend to group on them.

            if (A == DBNull.Value && B == DBNull.Value) //  both are DBNull.Value
                return true;
            if (A == DBNull.Value || B == DBNull.Value) //  only one is DBNull.Value
                return false;
            return (A.Equals(B));  // value type standard comparison
        }
        /// <summary>
        /// DataTable分页
        /// </summary>
        /// <param name="dt">需分页的DataTable</param>
        /// <returns>需要显示的数据</returns>
        public DataTable PageDT(DataTable dt, int pageSize, int cPage)
        {
            DataTable result = dt.Clone();
            if (pageSize < 1) pageSize = 10;
            if (cPage < 1) cPage = 1;
            int pageStart = pageSize * (cPage - 1);//0
            int pageEnd = pageSize * cPage;//10
            if (pageStart > dt.Rows.Count)
            {
                return result;
            }
            for (int i = pageStart; i < pageEnd && i < dt.Rows.Count; i++)
            {
                DataRow dr = result.NewRow();
                foreach (DataColumn dc in dt.Columns)
                {
                    dr[dc.ColumnName] = dt.Rows[i][dc.ColumnName];
                }
                result.Rows.Add(dr);
            }
            return result;
        }
        /// <summary>
        /// 创建数据表
        /// </summary>
        public static DataTable CreateTable(string cols)
        {
            DataTable dt = new DataTable();
            foreach (string col in cols.Split(','))
            {
                dt.Columns.Add(new DataColumn(col, typeof(string)));
            }
            return dt;
        }
    }
    /// <summary>
    /// DataTable转实体模型类,测试性能OK
    /// </summary>
    public class DTConver<T> where T : new()
    {
        /// <summary>
        /// 将DataTable转换为实体列表
        /// </summary>
        /// <param name="dt">待转换的DataTable</param>
        /// <returns></returns>
        public List<T> ConvertToList(DataTable dt)
        {
            // 定义集合  
            var list = new List<T>();

            if (0 == dt.Rows.Count)
            {
                return list;
            }

            // 获得此模型的可写公共属性  
            IEnumerable<PropertyInfo> propertys = new T().GetType().GetProperties().Where(u => u.CanWrite);
            list = ConvertToEntity(dt, propertys);


            return list;
        }
        /// <summary>
        /// 将DataTable的首行转换为实体
        /// </summary>
        /// <param name="dt">待转换的DataTable</param>
        /// <returns></returns>
        public T ConvertToEntity(DataTable dt)
        {
            DataTable dtTable = dt.Clone();
            dtTable.Rows.Add(dt.Rows[0].ItemArray);
            return ConvertToList(dtTable)[0];
        }
        private List<T> ConvertToEntity(DataTable dt, IEnumerable<PropertyInfo> propertys)
        {

            var list = new List<T>();
            //遍历DataTable中所有的数据行  
            foreach (DataRow dr in dt.Rows)
            {
                var entity = new T();

                //遍历该对象的所有属性  
                foreach (PropertyInfo p in propertys)
                {
                    //将属性名称赋值给临时变量
                    string tmpName = p.Name;

                    //检查DataTable是否包含此列（列名==对象的属性名）    
                    if (!dt.Columns.Contains(tmpName)) continue;
                    //取值  
                    object value = dr[tmpName];
                    //如果非空，则赋给对象的属性  
                    if (value != DBNull.Value)
                    {
                        p.SetValue(entity, value, null);
                    }
                }
                //对象添加到泛型集合中  
                list.Add(entity);
            }
            return list;
        }
    }
}

