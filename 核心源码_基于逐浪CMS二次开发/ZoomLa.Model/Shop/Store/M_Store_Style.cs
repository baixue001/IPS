using System;using System.Data.Common;
using System.Collections.Generic;

using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace ZoomLa.Model.Shop
{
    public class M_Store_Style : M_Base
    {

        public int ID { get; set; }
        public string StyleName { get; set; }
        /// <summary>
        /// 缩略图
        /// </summary>
        public string Thumbnail { get; set; }
        /// <summary>
        /// 首页模板
        /// </summary>
        public string Template_Index { get; set; }
        /// <summary>
        /// 内容模板
        /// </summary>
        public string Template_Content { get; set; }
        /// <summary>
        /// 列表页模板
        /// </summary>
        public string Template_List { get; set; }
        /// <summary>
        /// 状态[预留]
        /// </summary>
        public int ZStatus { get; set; }
        public DateTime CDate { get; set; }
        public string Remind { get; set; }

        public override string TbName { get { return "ZL_Store_Style"; } }
        public override string PK { get { return "ID"; } }

        public override string[,] FieldList()
        {
            string[,] Tablelist = {
                                {"ID","Int","4"},
                                {"StyleName","NVarChar","100"},
                                {"Template_Index","NVarChar","500"},
                                {"Template_Content","NVarChar","500"},
                                {"Template_List","NVarChar","500"},
                                {"ZStatus","Int","4"},
                                {"CDate","DateTime","8"},
                                {"Remind","NVarChar","500"},
                                {"Thumbnail","NVarChar","500"}
        };
            return Tablelist;
        }

        public override SqlParameter[] GetParameters()
        {
            M_Store_Style model = this;
            if (model.CDate <= DateTime.MinValue) { model.CDate = DateTime.Now; }
            SqlParameter[] sp = GetSP();
            sp[0].Value = model.ID;
            sp[1].Value = model.StyleName;
            sp[2].Value = model.Template_Index;
            sp[3].Value = model.Template_Content;
            sp[4].Value = model.Template_List;
            sp[5].Value = model.ZStatus;
            sp[6].Value = model.CDate;
            sp[7].Value = model.Remind;
            sp[8].Value = model.Thumbnail;
            return sp;
        }
        public M_Store_Style GetModelFromReader(DbDataReader rdr)
        {
            M_Store_Style model = new M_Store_Style();
            model.ID = ConvertToInt(rdr["ID"]);
            model.StyleName = ConverToStr(rdr["StyleName"]);
            model.Template_Index = ConverToStr(rdr["Template_Index"]);
            model.Template_Content = ConverToStr(rdr["Template_Content"]);
            model.Template_List = ConverToStr(rdr["Template_List"]);
            model.ZStatus = ConvertToInt(rdr["ZStatus"]);
            model.CDate = ConvertToDate(rdr["CDate"]);
            model.Remind = ConverToStr(rdr["Remind"]);
            model.Thumbnail = ConverToStr(rdr["Thumbnail"]);
            rdr.Close();
            return model;
        }
    }
}
