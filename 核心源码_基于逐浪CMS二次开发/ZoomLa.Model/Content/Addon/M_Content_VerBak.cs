using System;using System.Data.Common;
using System.Collections.Generic;

using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace ZoomLa.Model.Content
{
    public class M_Content_VerBak : M_Base
    {
        public int ID { get; set; }
        /// <summary>
        /// 所属内容ID
        /// </summary>
        public int GeneralID { get; set; }
        /// <summary>
        /// 标题
        /// </summary>
        public string Title { get; set; }
        public string Inputer { get; set; }
        /// <summary>
        /// 备份流水号
        /// </summary>
        public string Flow { get; set; }
        /// <summary>
        /// 内容模型备份(json)
        /// </summary>
        public string ContentBak { get; set; }
        /// <summary>
        /// 符加表数据备份(xml)
        /// </summary>
        public string TableBak { get; set; }
        /// <summary>
        /// 附加数据备份
        /// </summary>
        public string ExtendBak { get; set; }
        /// <summary>
        /// 备份类型  content|product
        /// </summary>
        public string ZType { get; set; }
        public DateTime CDate { get; set; }
        public override string TbName { get { return "ZL_Content_VerBak"; } }
        public override string PK { get { return "ID"; } }
        public override string[,] FieldList()
        {
            string[,] Tablelist = {
                                {"ID","Int","4"},
                                {"GeneralID","Int","4"},
                                {"Title","NVarChar","100"},
                                {"Flow","NVarChar","200"},
                                {"ContentBak","NText","20000"},
                                {"TableBak","NText","20000"},
                                {"CDate","DateTime","8"},
                                {"Inputer","NVarChar","100"},
                                {"ExtendBak","NText","20000"},
                                {"ZType","NVarChar","50"}
        };
            return Tablelist;
        }
        public override SqlParameter[] GetParameters()
        {
            M_Content_VerBak model = this;
            if (model.CDate <= DateTime.Now) { model.CDate = DateTime.Now; }
            SqlParameter[] sp = GetSP();
            sp[0].Value = model.ID;
            sp[1].Value = model.GeneralID;
            sp[2].Value = model.Title;
            sp[3].Value = model.Flow;
            sp[4].Value = model.ContentBak;
            sp[5].Value = model.TableBak;
            sp[6].Value = model.CDate;
            sp[7].Value = model.Inputer;
            sp[8].Value = model.ExtendBak;
            sp[9].Value = model.ZType;
            return sp;
        }
        public M_Content_VerBak GetModelFromReader(DbDataReader rdr)
        {
            M_Content_VerBak model = new M_Content_VerBak();
            model.ID = ConvertToInt(rdr["ID"]);
            model.GeneralID = ConvertToInt(rdr["GeneralID"]);
            model.Title = ConverToStr(rdr["Title"]);
            model.Flow = ConverToStr(rdr["Flow"]);
            model.ContentBak = ConverToStr(rdr["ContentBak"]);
            model.TableBak = ConverToStr(rdr["TableBak"]);
            model.CDate = ConvertToDate(rdr["CDate"]);
            model.Inputer = ConverToStr(rdr["Inputer"]);
            model.ExtendBak = ConverToStr(rdr["ExtendBak"]);
            model.ZType = ConverToStr(rdr["ZType"]);
            rdr.Close();
            return model;
        }
    }
}
