using System;using System.Data.Common;
using System.Data.SqlClient;
using System.Data;


namespace ZoomLa.Model
{
    [Serializable]
    public class M_Templata : M_Base
    {
        public M_Templata()
        {
            Addtime = DateTime.Now;
            IsTrue = 1;
            ContentFileEx = "html";
            NodeFileEx = "html";
            OpenType = "_blank";
            TemplateType = 2;
            TemplateUrl = "";
        }

        #region 属性定义

        ///<summary>
        ///黄页栏目ID
        ///</summary>
        public int TemplateID { get; set; }

        ///<summary>
        ///黄页栏目名称
        ///</summary>
        public string TemplateName { get; set; }

        ///<summary>
        ///黄页栏目地址
        ///</summary>
        public string TemplateUrl { get; set; }

        ///<summary>
        ///栏目类型(1-单页型栏目，2-栏目型，3-Url转发型栏目)
        ///</summary>
        public int TemplateType { get; set; }

        ///<summary>
        ///默认打开方式
        ///</summary>
        public string OpenType { get; set; }

        ///<summary>
        ///栏目样式组ID
        ///</summary>
        public string UserGroup { get; set; }

        ///<summary>
        ///添加时间
        ///</summary>
        public DateTime Addtime { get; set; }

        ///<summary>
        ///启用状态(0-停用,1-启用)
        ///</summary>
        public int IsTrue { get; set; }

        ///<summary>
        ///所属用户ID
        ///</summary>
        public int UserID { get; set; }

        ///<summary>
        ///排列顺序
        ///</summary>
        public int OrderID { get; set; }

        ///<summary>
        ///上级节点ID [0-根级]
        ///</summary>
        public int ParentID { get; set; }

        ///<summary>
        ///模型信息
        ///</summary>
        public string Modelinfo { get; set; }
        ///<summary>
        ///标识符
        ///</summary>
        public string Identifiers { get; set; }

        ///<summary>
        ///栏目扩展名
        ///</summary>
        public string NodeFileEx { get; set; }

        ///<summary>
        ///内容页扩展名
        ///</summary>
        public string ContentFileEx { get; set; }
        ///<summary>
        ///栏目图片地址
        ///</summary>
        public string Nodeimgurl { get; set; }

        ///<summary>
        ///栏目提示
        ///</summary>
        public string Nodeimgtext { get; set; }

        ///<summary>
        ///说明 用于在单页页详细介绍单页信息，支持HTML
        ///</summary>
        public string Pagecontent { get; set; }

        ///<summary>
        ///META关键词
        ///</summary>
        public string PageMetakeyword { get; set; }

        ///<summary>
        ///META网页描述
        ///</summary>
        public string PageMetakeyinfo { get; set; }
        ///<summary>
        ///外部链接地址
        ///</summary>
        public string Linkurl { get; set; }

        ///<summary>
        ///外部链接图片地址
        ///</summary>
        public string Linkimg { get; set; }

        ///<summary>
        ///外部链接提示
        ///</summary>
        public string Linktxt { get; set; }
        public string Username { get; set; }
        /// <summary>
        /// 绑定系统节点
        /// </summary>
        public int ParentNode { get; set; }
        /// <summary>
        /// 是否需要审核0:不需要,1:需要(默认)
        /// </summary>
        public int NeedAudit { get; set; }
        #endregion

        public override string TbName { get { return "ZL_PageTemplate"; } }
        public override string PK { get { return "TemplateID"; } }
        /// <summary>
        /// 返回实体列表数组
        /// </summary>
        /// <returns>String[]</returns>
        public override string[,] FieldList()
        {
            string[,] Tablelist = {
                                  {"TemplateID","Int","4"},
                                  {"TemplateName","NVarChar","50"},
                                  {"TemplateUrl","NVarChar","50"},
                                  {"TemplateType","Int","4"}, 
                                  {"OpenType","NVarChar","50"},
                                  {"UserGroup","NVarChar","50"},
                                  {"Addtime","DateTime","8"},
                                  {"IsTrue","Int","4"},
                                  {"Userid","Int","4"}, 
                                  {"UserName","NVarChar","4000"},
                                  {"OrderID","Int","4"},
                                  {"ParentID","Int","4"},
                                  {"Modelinfo","Text","400"},
                                  {"identifiers","NVarChar","50"}, 
                                  {"NodeFileEx","NVarChar","255"},
                                  {"ContentFileEx","NVarChar","255"},
                                  {"Nodeimgurl","NVarChar","50"},
                                  {"Nodeimgtext","NVarChar","50"},
                                  {"Pagecontent","Text","400"}, 
                                  {"PageMetakeyword","NVarChar","255"},
                                  {"PageMetakeyinfo","NVarChar","255"},
                                  {"linkurl","NVarChar","255"},
                                  {"linkimg","NVarChar","255"},
                                  {"linktxt","NVarChar","255"}, 
                                  {"ParentNode","Int","4"},
                                  {"NeedAudit","Int","4"}
                                 };
            return Tablelist;
        }
        public override SqlParameter[] GetParameters()
        {
            M_Templata model=this;
            SqlParameter[] sp = GetSP();
            sp[0].Value = model.TemplateID;
            sp[1].Value = model.TemplateName;
            sp[2].Value = model.TemplateUrl;
            sp[3].Value = model.TemplateType;
            sp[4].Value = model.OpenType;
            sp[5].Value = model.UserGroup;
            sp[6].Value = model.Addtime;
            sp[7].Value = model.IsTrue;
            sp[8].Value = model.UserID;
            sp[9].Value = model.Username;
            sp[10].Value = model.OrderID;
            sp[11].Value = model.ParentID;
            sp[12].Value = model.Modelinfo;
            sp[13].Value = model.Identifiers;
            sp[14].Value = model.NodeFileEx;
            sp[15].Value = model.ContentFileEx;
            sp[16].Value = model.Nodeimgurl;
            sp[17].Value = model.Nodeimgtext;
            sp[18].Value = model.Pagecontent;
            sp[19].Value = model.PageMetakeyword;
            sp[20].Value = model.PageMetakeyinfo;
            sp[21].Value = model.Linkurl;
            sp[22].Value = model.Linkimg;
            sp[23].Value = model.Linktxt;
            sp[24].Value = model.ParentNode;
            sp[25].Value = model.NeedAudit;
            return sp;
        }

        public M_Templata GetModelFromReader(DbDataReader rdr)
        {
            M_Templata model = new M_Templata();
            model.TemplateID = Convert.ToInt32(rdr["TemplateID"]);
            model.TemplateName = ConverToStr(rdr["TemplateName"]);
            model.TemplateUrl = ConverToStr(rdr["TemplateUrl"]);
            model.TemplateType = ConvertToInt(rdr["TemplateType"]);
            model.OpenType = ConverToStr(rdr["OpenType"]);
            model.UserGroup = ConverToStr(rdr["UserGroup"]);
            model.Addtime = ConvertToDate(rdr["Addtime"]);
            model.IsTrue = ConvertToInt(rdr["IsTrue"]);
            model.UserID = ConvertToInt(rdr["Userid"]);
            model.Username = ConverToStr(rdr["UserName"]);
            model.OrderID = ConvertToInt(rdr["OrderID"]);
            model.ParentID = ConvertToInt(rdr["ParentID"]);
            model.Modelinfo = ConverToStr(rdr["Modelinfo"]);
            model.Identifiers = ConverToStr(rdr["identifiers"]);
            model.NodeFileEx = ConverToStr(rdr["NodeFileEx"]);
            model.ContentFileEx = ConverToStr(rdr["ContentFileEx"]);
            model.Nodeimgurl = ConverToStr(rdr["Nodeimgurl"]);
            model.Nodeimgtext = ConverToStr(rdr["Nodeimgtext"]);
            model.Pagecontent = ConverToStr(rdr["Pagecontent"]);
            model.PageMetakeyword = ConverToStr(rdr["PageMetakeyword"]);
            model.PageMetakeyinfo = ConverToStr(rdr["PageMetakeyinfo"]);
            model.Linkurl = ConverToStr(rdr["linkurl"]);
            model.Linkimg = ConverToStr(rdr["linkimg"]);
            model.Linktxt =ConverToStr(rdr["linktxt"]);
            model.ParentNode = ConvertToInt(rdr["ParentNode"]);
            model.NeedAudit =ConvertToInt(rdr["NeedAudit"]);
            rdr.Close();
            return model;
        }
    }
}