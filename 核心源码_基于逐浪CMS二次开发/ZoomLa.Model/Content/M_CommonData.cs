using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Text;

namespace ZoomLa.Model.Content
{
    public class M_CommonData : M_Base
    {
        public M_CommonData()
        {
            CreateTime = DateTime.Now;
            UpDateTime = DateTime.Now;
            EliteLevel = 0;
            UpDateType = 2;
            IsComm = 1;
            PdfLink = "";
            TaskInfo = "";
            Hits = 0;
            //----------------------important
            InfoID = 0;
            SuccessfulUserID = 0;
            Inputer = "";
            TopImg = "";
            ItemID = 0; Status = 0; OrderID = 0;
            IsCreate = 0; NodeID = 0; ModelID = 0; FirstNodeID = 0;
        }
        public bool IsNull { get { return GeneralID < 1; } }
        /// <summary>
        /// 是否为店铺
        /// </summary>
        public bool IsStore
        {
            get { return TableName.ToLower().StartsWith("zl_store_"); }
        }
        //--------------------------------------------
        public int GeneralID { get; set; }
        public int OrderID { get; set; }
        public int NodeID { get; set; }
        public int ModelID { get; set; }
        public int ItemID { get; set; }
        public string TableName { get; set; }
        public string Title { get; set; }
        /// <summary>
        /// 作者
        /// </summary>
        public string Inputer { get; set; }
        /// <summary>
        /// 点击数
        /// </summary>
        public int Hits { get; set; }
        public DateTime CreateTime { get; set; }
        /// <summary>
        /// 信息状态 -2为删除，-1为退稿，0为待审核，99为终审通过，其它为自定义
        /// </summary>
        public int Status { get; set; }
        /// <summary>
        /// 是否推荐 1=已推荐
        /// </summary>
        public int EliteLevel { get; set; }
        /// <summary>
        /// 添加该内容的管理员ID,用户添加则为0
        /// </summary>
        public int InfoID { get; set; }
        /// <summary>
        /// 所属专题ID数组
        /// </summary>
        public string SpecialID { get; set; }
        /// <summary>
        /// 是否已生成0=未生成 1=已生成
        /// </summary>
        public int IsCreate { get; set; }
        public string HtmlLink { get; set; }
        /// <summary>
        /// 标题颜色
        /// </summary>
        public string Titlecolor { get; set; }
        /// <summary>
        /// 自定义模板地址(xxx.html)
        /// </summary>
        public string Template { get; set; }
        /// <summary>
        /// 关键字
        /// </summary>
        public string TagKey { get; set; }
        public DateTime UpDateTime { get; set; }
        /// <summary>
        /// 生成的PDF链接地址
        /// </summary>
        public string PdfLink { get; set; }
        /// <summary>
        /// 用作为存储生成的Word链接
        /// </summary>
        public string Rtype { get; set; }
        /// <summary>
        ///  发文用户ID(原为中标用户ID)
        /// </summary>
        public int SuccessfulUserID { get; set; }
        public string Subtitle { get; set; }
        public string PYtitle { get; set; }
        /// <summary>
        /// 店铺的风格ID
        /// </summary>
        public int DefaultSkins { get; set; }
        public int FirstNodeID { get; set; }
        /// <summary>
        /// 标题字体
        /// </summary>
        public string TitleStyle { get; set; }
        /// <summary>
        /// 父级树
        /// </summary>
        public string ParentTree { get; set; }
        public string TopImg { get; set; }
        /// <summary>
        /// 关联内容IDS
        /// </summary>
        public string RelatedIDS { get; set; }
        /// <summary>
        /// 是否允许评论 1:是
        /// </summary>
        public int IsComm { get; set; }
        /// <summary>
        /// 管理员审核时间
        /// </summary>
        public string AuditTime { get; set; }
        /// <summary>
        /// 会员组的访问权限(空则不限制)
        /// </summary>
        public string UGroupAuth { get; set; }
        /// <summary>
        /// 是否置顶
        /// </summary>
        public int IsTop { get; set; }
        /// <summary>
        /// 置顶到什么时间,为空则不限制
        /// </summary>
        public string IsTopTime { get; set; }
        public string IP { get; set; }
        /// <summary>
        /// 自动任务信息(json)(自动审核|过期)
        /// 可自由扩展,信息存量足够
        /// audit,expire
        /// </summary>
        public string TaskInfo { get; set; }
        #region disuse
        public int UpDateType { get; set; }
        // 原为站群捕捉
        public int IsCatch { get; set; }
        //原为退稿原因
        public string because_back { get; set; }
        #endregion
        public override string TbName { get { return "ZL_CommonModel"; } }
        public override string PK { get { return "GeneralID"; } }

        public override string[,] FieldList()
        {
            string[,] Tablelist = {
                                {"GeneralID","Int","4"},
                                {"OrderID","Int","4"},
                                {"NodeID","Int","4"},
                                {"ModelID","Int","4"},
                                {"ItemID","Int","4"},
                                {"TableName","NVarChar","50"},
                                {"Title","NVarChar","255"},
                                {"Inputer","NVarChar","255"},
                                {"Hits","Int","4"},
                                {"CreateTime","DateTime","8"},
                                {"Status","Int","4"},
                                {"EliteLevel","Int","4"},
                                {"InfoID","Int","4"},
                                {"SpecialID","NVarChar","255"},
                                {"IsCreate","Int","4"},
                                {"HtmlLink","NVarChar","500"},
                                {"Titlecolor","NVarChar","255"},
                                {"Template","NVarChar","255"},
                                {"TagKey","NVarChar","1000"},
                                {"UpDateTime","DateTime","8"},
                                {"UpDateType","Int","4"},
                                {"because_back","NVarChar","200"},
                                {"PdfLink","NVarChar","500"},
                                {"Rtype","NVarChar","255"},
                                {"SuccessfulUserID","Int","4"},
                                {"Subtitle","NVarChar","2000"},
                                {"PYtitle","NVarChar","50"},
                                {"DefaultSkins","Int","4"},
                                {"FirstNodeID","Int","4"},
                                {"TitleStyle","NVarChar","255"},
                                {"ParentTree","NVarChar","255"},
                                {"TopImg","NVarChar","100"},
                                {"IsCatch","Int","4"},
                                {"RelatedIDS","VarChar","3000"},
                                {"IsComm","Int","4"},
                                {"AuditTime","NVarChar","100"},
                                {"UGroupAuth","NVarChar","4000"},
                                {"IsTop","Int","4"},
                                {"IsTopTime","NVarChar","100"},
                                {"IP","NVarChar","100"},
                                {"TaskInfo","NVarChar","4000"}
        };
            return Tablelist;
        }
        public override SqlParameter[] GetParameters()
        {
            M_CommonData model = this;
            SqlParameter[] sp = GetSP();
            sp[0].Value = model.GeneralID;
            sp[1].Value = model.OrderID;
            sp[2].Value = model.NodeID;
            sp[3].Value = model.ModelID;
            sp[4].Value = model.ItemID;
            sp[5].Value = model.TableName;
            sp[6].Value = model.Title;
            sp[7].Value = model.Inputer;
            sp[8].Value = model.Hits;
            sp[9].Value = model.CreateTime;
            sp[10].Value = model.Status;
            sp[11].Value = model.EliteLevel;
            sp[12].Value = model.InfoID;
            sp[13].Value = model.SpecialID;
            sp[14].Value = model.IsCreate;
            sp[15].Value = model.HtmlLink;
            sp[16].Value = model.Titlecolor;
            sp[17].Value = model.Template;
            sp[18].Value = model.TagKey;
            sp[19].Value = model.UpDateTime;
            sp[20].Value = model.UpDateType;
            sp[21].Value = model.because_back;
            sp[22].Value = model.PdfLink;
            sp[23].Value = model.Rtype;
            sp[24].Value = model.SuccessfulUserID;
            sp[25].Value = model.Subtitle;
            sp[26].Value = model.PYtitle;
            sp[27].Value = model.DefaultSkins;
            sp[28].Value = model.FirstNodeID;
            sp[29].Value = model.TitleStyle;
            sp[30].Value = model.ParentTree;
            sp[31].Value = model.TopImg;
            sp[32].Value = model.IsCatch;
            sp[33].Value = model.RelatedIDS;
            sp[34].Value = model.IsComm;
            sp[35].Value = model.AuditTime;
            sp[36].Value = model.UGroupAuth;
            sp[37].Value = model.IsTop;
            sp[38].Value = model.IsTopTime;
            sp[39].Value = model.IP;
            sp[40].Value = model.TaskInfo;
            return sp;
        }
        public M_CommonData GetModelFromReader(DbDataReader rdr)
        {
            M_CommonData model = new M_CommonData();
            model.GeneralID = ConvertToInt(rdr["GeneralID"]);
            model.OrderID = ConvertToInt(rdr["OrderID"]);
            model.NodeID = ConvertToInt(rdr["NodeID"]);
            model.ModelID = ConvertToInt(rdr["ModelID"]);
            model.ItemID = ConvertToInt(rdr["ItemID"]);
            model.TableName = ConverToStr(rdr["TableName"]);
            model.Title = ConverToStr(rdr["Title"]);
            model.Inputer = ConverToStr(rdr["Inputer"]);
            model.Hits = ConvertToInt(rdr["Hits"]);
            model.CreateTime = ConvertToDate(rdr["CreateTime"]);
            model.Status = ConvertToInt(rdr["Status"]);
            model.EliteLevel = ConvertToInt(rdr["EliteLevel"]);
            model.InfoID = ConvertToInt(rdr["InfoID"]);
            model.SpecialID = ConverToStr(rdr["SpecialID"]);
            model.IsCreate = ConvertToInt(rdr["IsCreate"]);
            model.HtmlLink = ConverToStr(rdr["HtmlLink"]);
            model.Titlecolor = ConverToStr(rdr["Titlecolor"]);
            model.Template = ConverToStr(rdr["Template"]);
            model.TagKey = ConverToStr(rdr["TagKey"]);
            model.UpDateTime = ConvertToDate(rdr["UpDateTime"]);
            model.UpDateType = ConvertToInt(rdr["UpDateType"]);
            model.because_back = ConverToStr(rdr["because_back"]);
            model.PdfLink = ConverToStr(rdr["PdfLink"]);
            model.Rtype = ConverToStr(rdr["Rtype"]);
            model.SuccessfulUserID = ConvertToInt(rdr["SuccessfulUserID"]);
            model.Subtitle = ConverToStr(rdr["Subtitle"]);
            model.PYtitle = ConverToStr(rdr["PYtitle"]);
            model.DefaultSkins = ConvertToInt(rdr["DefaultSkins"]);
            model.FirstNodeID = ConvertToInt(rdr["FirstNodeID"]);
            model.TitleStyle = ConverToStr(rdr["TitleStyle"]);
            model.ParentTree = ConverToStr(rdr["ParentTree"]);
            model.TopImg = ConverToStr(rdr["TopImg"]);
            model.IsCatch = ConvertToInt(rdr["IsCatch"]);
            model.RelatedIDS = ConverToStr(rdr["RelatedIDS"]);
            model.IsComm = ConvertToInt(rdr["IsComm"]);
            model.AuditTime = ConverToStr(rdr["AuditTime"]);
            model.UGroupAuth = ConverToStr(rdr["UGroupAuth"]);
            model.IsTop = ConvertToInt(rdr["IsTop"]);
            model.IsTopTime = ConverToStr(rdr["IsTopTime"]);
            model.IP = ConverToStr(rdr["IP"]);
            model.TaskInfo = ConverToStr(rdr["TaskInfo"]);
            rdr.Close();
            return model;
        }
        public M_CommonData GetModelFromReader(DataRow rdr)
        {
            M_CommonData model = new M_CommonData();
            model.GeneralID = ConvertToInt(rdr["GeneralID"]);
            model.OrderID = ConvertToInt(rdr["OrderID"]);
            model.NodeID = ConvertToInt(rdr["NodeID"]);
            model.ModelID = ConvertToInt(rdr["ModelID"]);
            model.ItemID = ConvertToInt(rdr["ItemID"]);
            model.TableName = ConverToStr(rdr["TableName"]);
            model.Title = ConverToStr(rdr["Title"]);
            model.Inputer = ConverToStr(rdr["Inputer"]);
            model.Hits = ConvertToInt(rdr["Hits"]);
            model.CreateTime = ConvertToDate(rdr["CreateTime"]);
            model.Status = ConvertToInt(rdr["Status"]);
            model.EliteLevel = ConvertToInt(rdr["EliteLevel"]);
            model.InfoID = ConvertToInt(rdr["InfoID"]);
            model.SpecialID = ConverToStr(rdr["SpecialID"]);
            model.IsCreate = ConvertToInt(rdr["IsCreate"]);
            model.HtmlLink = ConverToStr(rdr["HtmlLink"]);
            model.Titlecolor = ConverToStr(rdr["Titlecolor"]);
            model.Template = ConverToStr(rdr["Template"]);
            model.TagKey = ConverToStr(rdr["TagKey"]);
            model.UpDateTime = ConvertToDate(rdr["UpDateTime"]);
            model.UpDateType = ConvertToInt(rdr["UpDateType"]);
            model.because_back = ConverToStr(rdr["because_back"]);
            model.PdfLink = ConverToStr(rdr["PdfLink"]);
            model.Rtype = ConverToStr(rdr["Rtype"]);
            model.SuccessfulUserID = ConvertToInt(rdr["SuccessfulUserID"]);
            model.Subtitle = ConverToStr(rdr["Subtitle"]);
            model.PYtitle = ConverToStr(rdr["PYtitle"]);
            model.DefaultSkins = ConvertToInt(rdr["DefaultSkins"]);
            model.FirstNodeID = ConvertToInt(rdr["FirstNodeID"]);
            model.TitleStyle = ConverToStr(rdr["TitleStyle"]);
            model.ParentTree = ConverToStr(rdr["ParentTree"]);
            model.TopImg = ConverToStr(rdr["TopImg"]);
            model.IsCatch = ConvertToInt(rdr["IsCatch"]);
            model.RelatedIDS = ConverToStr(rdr["RelatedIDS"]);
            model.IsComm = ConvertToInt(rdr["IsComm"]);
            model.AuditTime = ConverToStr(rdr["AuditTime"]);
            model.UGroupAuth = ConverToStr(rdr["UGroupAuth"]);
            model.IsTop = ConvertToInt(rdr["IsTop"]);
            model.IsTopTime = ConverToStr(rdr["IsTopTime"]);
            model.IP = ConverToStr(rdr["IP"]);
            model.TaskInfo = ConverToStr(rdr["TaskInfo"]);
            return model;
        }
    }
}
