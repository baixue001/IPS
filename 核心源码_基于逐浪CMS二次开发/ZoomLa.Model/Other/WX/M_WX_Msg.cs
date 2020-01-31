using System;using System.Data.Common;
using System.Collections.Generic;

using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace ZoomLa.Model.Other
{
    //微信消息模板类
    public class M_WX_MsgTlp : M_Base
    {

        public int ID { get; set; }
        public string Alias { get; set; }
        /// <summary>
        /// 信息类型 text|image|multi(多条内容消息数据)
        /// </summary>
        public string MsgType { get; set; }
        /// <summary>
        /// 信息内容Json数据
        /// </summary>
        public string MsgContent { get; set; }
        /// <summary>
        /// 是否需要解析标签
        /// </summary>
        public int AllowLabel { get; set; }
        public int AdminID { get; set; }
        public DateTime CDate { get; set; }
        public M_WX_MsgTlp() { CDate = DateTime.Now; AllowLabel = 0; }
        public override string TbName { get { return "ZL_WX_MsgTlp"; } }
        public override string PK { get { return "ID"; } }

        public override string[,] FieldList()
        {
            string[,] Tablelist = {
                                {"ID","Int","4"},
                                {"Alias","NVarChar","100"},
                                {"MsgType","NVarChar","50"},
                                {"MsgContent","NText","50000"},
                                {"AllowLabel","Int","4"},
                                {"AdminID","Int","4"},
                                {"CDate","DateTime","8"}
        };
            return Tablelist;
        }

        public override SqlParameter[] GetParameters()
        {
            M_WX_MsgTlp model = this;
            if (model.CDate <= DateTime.Now) { model.CDate = DateTime.Now; }
            SqlParameter[] sp = GetSP();
            sp[0].Value = model.ID;
            sp[1].Value = model.Alias;
            sp[2].Value = model.MsgType;
            sp[3].Value = model.MsgContent;
            sp[4].Value = model.AllowLabel;
            sp[5].Value = model.AdminID;
            sp[6].Value = model.CDate;
            return sp;
        }
        public M_WX_MsgTlp GetModelFromReader(DbDataReader rdr)
        {
            M_WX_MsgTlp model = new M_WX_MsgTlp();
            model.ID = ConvertToInt(rdr["ID"]);
            model.Alias = ConverToStr(rdr["Alias"]);
            model.MsgType = ConverToStr(rdr["MsgType"]);
            model.MsgContent = ConverToStr(rdr["MsgContent"]);
            model.AllowLabel = ConvertToInt(rdr["AllowLabel"]);
            model.AdminID = ConvertToInt(rdr["AdminID"]);
            model.CDate = ConvertToDate(rdr["CDate"]);
            rdr.Close();
            return model;
        }
    }
}
