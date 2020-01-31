using System;using System.Data.Common;
using System.Collections.Generic;

using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace ZoomLa.Model.Content
{
    public class M_Content_MarkDown : M_Base
    {
        public M_Content_MarkDown()
        {
            ZStatus = 99;
            ZType = 0;
        }
        public int ID { get; set; }
        public string Title { get; set; }
        public string SubTitle { get; set; }
        public string Thumbnail { get; set; }
        /// <summary>
        /// MarkDown语法内容
        /// </summary>
        public string Content_MD { get; set; }
        /// <summary>
        /// 解析出的HTML内容
        /// </summary>
        public string Content { get; set; }
        public int AdminID { get; set; }
        public int UserID { get; set; }
        public string UserName { get; set; }
        public int ZStatus { get; set; }
        public int ZType { get; set; }
        public DateTime CDate { get; set; }
        public string Remark { get; set; }

        public override string TbName { get { return "ZL_Content_MarkDown"; } }
        public override string PK { get { return "ID"; } }

        public override string[,] FieldList()
        {
            string[,] Tablelist = {
                                {"ID","Int","4"},
                                {"Title","NVarChar","200"},
                                {"SubTitle","NVarChar","500"},
                                {"Thumbnail","NVarChar","500"},
                                {"Content_MD","NText","20000"},
                                {"Content","NText","20000"},
                                {"AdminID","Int","4"},
                                {"UserID","Int","4"},
                                {"UserName","NVarChar","100"},
                                {"ZStatus","Int","4"},
                                {"ZType","Int","4"},
                                {"CDate","DateTime","8"},
                                {"Remark","NVarChar","1000"}
        };
            return Tablelist;
        }

        public override SqlParameter[] GetParameters()
        {
            M_Content_MarkDown model = this;
            if (model.CDate <= DateTime.MinValue) { model.CDate = DateTime.Now; }
            SqlParameter[] sp = GetSP();
            sp[0].Value = model.ID;
            sp[1].Value = model.Title;
            sp[2].Value = model.SubTitle;
            sp[3].Value = model.Thumbnail;
            sp[4].Value = model.Content_MD;
            sp[5].Value = model.Content;
            sp[6].Value = model.AdminID;
            sp[7].Value = model.UserID;
            sp[8].Value = model.UserName;
            sp[9].Value = model.ZStatus;
            sp[10].Value = model.ZType;
            sp[11].Value = model.CDate;
            sp[12].Value = model.Remark;
            return sp;
        }
        public M_Content_MarkDown GetModelFromReader(DbDataReader rdr)
        {
            M_Content_MarkDown model = new M_Content_MarkDown();
            model.ID = Convert.ToInt32(rdr["ID"]);
            model.Title = ConverToStr(rdr["Title"]);
            model.SubTitle = ConverToStr(rdr["SubTitle"]);
            model.Thumbnail = ConverToStr(rdr["Thumbnail"]);
            model.Content_MD = ConverToStr(rdr["Content_MD"]);
            model.Content = ConverToStr(rdr["Content"]);
            model.AdminID = ConvertToInt(rdr["AdminID"]);
            model.UserID = ConvertToInt(rdr["UserID"]);
            model.UserName = ConverToStr(rdr["UserName"]);
            model.ZStatus = ConvertToInt(rdr["ZStatus"]);
            model.ZType = ConvertToInt(rdr["ZType"]);
            model.CDate = ConvertToDate(rdr["CDate"]);
            model.Remark = ConverToStr(rdr["Remark"]);
            rdr.Close();
            return model;
        }
    }
}
