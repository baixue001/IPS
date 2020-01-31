using System;using System.Data.Common;
using System.Collections.Generic;

using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace ZoomLa.Model.Font
{
    public class M_Font_WriteWord : M_Base
    {
        public M_Font_WriteWord()
        {
            CDate = DateTime.Now;
            Svg = "";
            FontSvg = "";
            ZStatus = 0;
        }
        public int ID { get; set; }
        /// <summary>
        /// 所属手写字体ID
        /// </summary>
        public int WriteID { get; set; }
        /// <summary>
        /// 文字Unicode码(10进制)
        /// </summary>
        public string Unicode { get; set; }
        /// <summary>
        /// 字体SVG
        /// </summary>
        public string Svg { get; set; }
        /// <summary>
        /// 生成后的字体svg
        /// </summary>
        public string FontSvg { get; set; }
        public DateTime CDate { get; set; }
        /// <summary>
        /// 该文字是否写完
        /// </summary>
        public int ZStatus { get; set; }
        /// <summary>
        /// 书写数据,用于重放
        /// </summary>
        public string vCanvasData { get; set; }
        /// <summary>
        /// 最后一次的base64数据,用于避免数据过大
        /// </summary>
        public string vCanvasList { get; set; }
        /// <summary>
        /// 书写次数,用于配合重放
        /// </summary>
        public int vCount { get; set; }

        public override string TbName { get { return "ZL_Font_WriteWord"; } }
        public override string PK { get { return "ID"; } }
        public override string[,] FieldList()
        {
            string[,] Tablelist = {
                                {"ID","Int","4"},
                                {"WriteID","Int","4"},
                                {"Unicode","NVarChar","50"},
                                {"Svg","Text","100000"},
                                {"FontSvg","Text","100000"},
                                {"CDate","DateTime","8"},
                                {"ZStatus","Int","4"},
                                {"vCanvasData","Text","1000000"},
                                {"vCanvasList","Text","1000000"},
                                {"vCount","Int","4"}
        };
            return Tablelist;
        }

        public override SqlParameter[] GetParameters()
        {
            M_Font_WriteWord model = this;
            SqlParameter[] sp = GetSP();
            sp[0].Value = model.ID;
            sp[1].Value = model.WriteID;
            sp[2].Value = model.Unicode;
            sp[3].Value = model.Svg;
            sp[4].Value = model.FontSvg;
            sp[5].Value = model.CDate;
            sp[6].Value = model.ZStatus;
            sp[7].Value = model.vCanvasData;
            sp[8].Value = model.vCanvasList;
            sp[9].Value = model.vCount;
            return sp;
        }
        public M_Font_WriteWord GetModelFromReader(DbDataReader rdr)
        {
            M_Font_WriteWord model = new M_Font_WriteWord();
            model.ID = Convert.ToInt32(rdr["ID"]);
            model.WriteID = ConvertToInt(rdr["WriteID"]);
            model.Unicode = ConverToStr(rdr["Unicode"]);
            model.Svg = ConverToStr(rdr["Svg"]);
            model.FontSvg = ConverToStr(rdr["FontSvg"]);
            model.CDate = ConvertToDate(rdr["CDate"]);
            model.ZStatus = ConvertToInt(rdr["ZStatus"]);
            model.vCanvasData = ConverToStr(rdr["vCanvasData"]);
            model.vCanvasList = ConverToStr(rdr["vCanvasList"]);
            model.vCount = ConvertToInt(rdr["vCount"]);
            rdr.Close();
            return model;
        }
    }
}
