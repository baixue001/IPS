using System;using System.Data.Common;
using System.Collections.Generic;
using System.Data;
using System.Xml;
using ZoomLa.Model.CreateJS;
namespace ZoomLa.Model
{
    public class M_Label : M_Base, ICloneable
    {
        public override string PK { get { return "LabelID"; } }
        public override string TbName { get { return ""; } }
        public int LabelID = 0;
        /// <summary>
        /// 站群用
        /// </summary>
        public int LabelAddUser = 0;
        /// <summary>
        /// 标签名称
        /// </summary>
        public string LabelName { get { return _labelName.Replace(" ", ""); } set { _labelName = value; } }
        private string _labelName = "", _labelCate = "";
        /// <summary>
        /// 标签分类
        /// </summary>
        public string LabelCate { get { return _labelCate.Replace(" ", ""); } set { _labelCate = value; } }
        /// <summary>
        /// 标签类型 1:静态,2:动态,4:分页动态
        /// </summary>
        public int LabelType = 2;
        /// <summary>
        /// 标签说明
        /// </summary>
        public string Desc = "";
        /// <summary>
        /// 标签参数(LabelParam)
        /// </summary>
        public string Param = "";
        /// <summary>
        /// 查询的表
        /// </summary>
        public string LabelTable = "";
        /// <summary>
        /// 查询字段
        /// </summary>
        public string LabelField = "*";
        /// <summary>
        /// 查询条件
        /// </summary>
        public string LabelWhere = "";
        /// <summary>
        /// 查询排序
        /// </summary>
        public string LabelOrder = "";
        /// <summary>
        /// 标签内容(LabelContent)
        /// </summary>
        public string Content = "";
        /// <summary>
        /// 查询数量
        /// </summary>
        public string LabelCount = "10";
        /// <summary>
        /// 站群用
        /// </summary>
        public int LabelNodeID = 0;
        /// <summary>
        /// 判断模式
        /// </summary>
        public string Modeltypeinfo = "参数判断";
        /// <summary>
        /// 计算方式(循环、累加)
        /// </summary>
        public string addroot = "循环";
        /// <summary>
        /// 判断逻辑
        /// </summary>
        public string setroot = "等于";
        /// <summary>
        /// 判断值
        /// </summary>
        public string Modelvalue = "";
        /// <summary>
        /// 参数标签
        /// </summary>
        public string Valueroot = "";
        /// <summary>
        /// 是否开启判断模式(1-开启 0-关闭)
        /// </summary>
        public int IsOpen = 0;
        /// <summary>
        /// 不满足条件的内容
        /// </summary>
        public string FalseContent = "";
        /// <summary>
        /// 数据为空的内容,为空则忽略
        /// </summary>
        public string EmptyContent = "";
        /// <summary>
        /// 数据源信息:主数据源|主表,次数据源|次表
        /// {"ds_m":"main","ds_s":"main","tb_m":"ZL_CommonModel","tb_s":"ZL_C_Article"}
        /// </summary>
        public string DataSourceType = "{\"ds_m\":\"main\",\"ds_s\":\"main\",\"tb_m\":\"\",\"tb_s\":\"\"}";
        /// <summary>
        /// (Disuse),暂用于二附院存结构名,以后取消
        /// </summary>
        public string ConnectString = "";
        /// <summary>
        /// 存储过程名
        /// </summary>
        public string ProceName = "";
        /// <summary>
        /// 存储过程参数 参数名:值,参数名:值,改为存储标签详情
        /// </summary>
        public string ProceParam = "";
        /// <summary>
        /// 是否存储过程 True:是,False:不是
        /// </summary>
        public bool IsProce
        {
            get
            {
                return !(string.IsNullOrEmpty(ProceName) || string.IsNullOrEmpty(ProceName.Trim()));
            }
        }
        public bool IsNull { get; private set; }
        /// <summary>
        /// 仅用于B_CreateHtml中提示
        /// </summary>
        public string ErrorMsg { get; set; }
        public override string[,] FieldList()
        {
            return null;
        }
        public static M_Label GetInfoFromDataTable(XmlNode node, DataTable dt)
        {
            M_Label info = new M_Label();
            if (dt == null || dt.Rows.Count < 1) { return info; }
            DataRow dr = dt.Rows[0];
            info.LabelID = info.ConvertToInt(node["LabelID"].InnerText);
            info.LabelName = node["LabelName"].InnerText;
            info.LabelType = info.ConvertToInt(node["LabelType"].InnerText);
            info.LabelCate = node["LabelCate"].InnerText;
            info.Desc = dr["LabelDesc"].ToString();
            info.Param = dr["LabelParam"].ToString();
            info.LabelTable = dr["LabelTable"].ToString();
            info.LabelField = dr["LabelField"].ToString();
            info.LabelWhere = dr["LabelWhere"].ToString();
            info.LabelOrder = dr["LabelOrder"].ToString();
            info.Content = dr["LabelContent"].ToString();
            info.LabelCount = dr["LabelCount"].ToString();
            info.LabelAddUser = info.ConvertToInt(GetFromDR(dt, "LabelAddUser", "0"));
            info.LabelNodeID = info.ConvertToInt(GetFromDR(dt, "LabelNodeID", "0"));
            info.Modeltypeinfo = GetFromDR(dt, "Modeltypeinfo");
            info.addroot = GetFromDR(dt, "addroot");
            info.setroot = GetFromDR(dt, "setroot");
            info.Modelvalue = GetFromDR(dt, "Modelvalue");
            info.Valueroot = GetFromDR(dt, "Valueroot");
            info.IsOpen = info.ConvertToInt(GetFromDR(dt, "IsOpen"));
            info.FalseContent = GetFromDR(dt, "FalseContent");
            info.EmptyContent = GetFromDR(dt, "EmptyContent");
            info.DataSourceType = GetFromDR(dt, "DataSourceType");
            info.ConnectString = GetFromDR(dt, "ConnectString");
            info.ProceName = GetFromDR(dt, "ProceName");
            info.ProceParam = GetFromDR(dt, "ProceParam");
            return info;
        }
        public static string GetFromDR(DataTable dt, string field, string def = "")
        {
            return dt.Columns.IndexOf(field) == -1 ? def : dt.Rows[0][field].ToString();
        }
        //---------------------------------------------对信息再解析
        private List<M_API_Param> _paramList = new List<M_API_Param>();
        /// <summary>
        /// 解析Param并返回模型列表
        /// </summary>
        public List<M_API_Param> ParamList
        {
            get
            {
                if (_paramList.Count > 0) { return _paramList; }
                if (!string.IsNullOrEmpty(Param))
                {
                    string[] paramArr = Param.Split('|');
                    foreach (string param in paramArr)
                    {
                        string[] valArr = param.Split(',');
                        M_API_Param model = new M_API_Param();
                        model.name = valArr[0];
                        model.defval = valArr[1];
                        model.type = valArr[2];
                        model.desc = valArr[3];
                        _paramList.Add(model);
                    }
                }
                return _paramList;
            }
        }
        public M_Label()
        {
            this.LabelName = "";
            this.LabelNodeID = 0;
        }
        public M_Label(bool flag) : this()
        {
            this.IsNull = flag;
        }
        public object Clone()
        {
            return new M_Label()
            {
                LabelType = this.LabelType,
                Content = this.Content,
                EmptyContent = this.EmptyContent,
                FalseContent = this.FalseContent,
                DataSourceType = this.DataSourceType,
                LabelCount = this.LabelCount,
                LabelField = this.LabelField,
                LabelTable = this.LabelTable,
                LabelWhere = this.LabelWhere,
                LabelOrder = this.LabelOrder,
                Param = this.Param
            };
        }
    }
    public class M_SubLabel
    {
        /// <summary>
        /// 只包含表名
        /// </summary>
        public string PureT1 { get { if (string.IsNullOrEmpty(T1)) { return ""; } else return T1.Split('.')[2]; } }
        public string PureT2 { get { if (string.IsNullOrEmpty(T2)) { return ""; } else return T2.Split('.')[2]; } }
        /// <summary>
        /// 主表名,ZoomlaCMS.dbo.ZL_CommonModel
        /// </summary>
        public string T1 { get; set; }
        /// <summary>
        /// 次表名
        /// </summary>
        public string T2 { get; set; }
        public string JoinType { get; set; }
        public string OnField1 { get; set; }
        public string OnField2 { get; set; }
    }
}