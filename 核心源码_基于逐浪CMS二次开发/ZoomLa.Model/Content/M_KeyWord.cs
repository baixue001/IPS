﻿namespace ZoomLa.Model
{
    using System;using System.Data.Common;
    using System.Data.SqlClient;
    using System.Data;

    public class M_KeyWord : M_Base
    {
        public M_KeyWord()
        {
            Hits = 0;
            QuoteTimes = 0;
            LastUseTime = DateTime.Now;
            Priority = 3;
            KeywordType = 1;
        }
        public int KeyWordID { get; set; }
        /// <summary>
        /// 关键词
        /// </summary>
        public string KeywordText { get; set; }
        /// <summary>
        /// 关键字类别:1:搜索关键词,2为试题关键字 3为试卷关键字
        /// </summary>
        public int KeywordType { get; set; }
        /// <summary>
        /// 优先级
        /// </summary>
        public int Priority { get; set; }
        /// <summary>
        /// 点击次
        /// </summary>
        public int Hits { get; set; }
        /// <summary>
        /// 最近使用时间
        /// </summary>
        public DateTime LastUseTime { get; set; }
        /// <summary>
        /// 引用文章Gids
        /// </summary>
        public string ArrGeneralID { get; set; }
        /// <summary>
        /// 引用次数
        /// </summary>
        public int QuoteTimes { get; set; }
        public override string PK { get { return "KeyWordID"; } }
        public override string TbName { get { return "ZL_Keywords"; } }
        public override string[,] FieldList()
        {
            string[,] Tablelist = {
                                  {"KeyWordID","Int","4"},
                                  {"KeywordText","NVarChar","1000"},
                                  {"KeywordType","Int","4"},
                                  {"Priority","Int","4"},
                                  {"Hits","Int","4"},
                                  {"LastUseTime","DateTime","8"},
                                  {"ArrGeneralId","NVarChar","4000"},
                                  {"QuoteTimes","Int","4"}
                                 };
            return Tablelist;
        }
        public override SqlParameter[] GetParameters()
        {
            M_KeyWord model = this;
            SqlParameter[] sp = GetSP();
            if (model.LastUseTime <= DateTime.MinValue) { model.LastUseTime = DateTime.Now; }
            sp[0].Value = model.KeyWordID;
            sp[1].Value = model.KeywordText;
            sp[2].Value = model.KeywordType;
            sp[3].Value = model.Priority;
            sp[4].Value = model.Hits;
            sp[5].Value = model.LastUseTime;
            sp[6].Value = model.ArrGeneralID;
            sp[7].Value = model.QuoteTimes;
            return sp;
        }
        public M_KeyWord GetModelFromReader(DbDataReader rdr)
        {
            M_KeyWord model = new M_KeyWord();
            model.KeyWordID = Convert.ToInt32(rdr["KeyWordID"]);
            model.KeywordText = ConverToStr(rdr["KeywordText"]);
            model.KeywordType = ConvertToInt(rdr["KeywordType"]);
            model.Priority = ConvertToInt(rdr["Priority"]);
            model.Hits = ConvertToInt(rdr["Hits"]);
            model.LastUseTime = ConvertToDate(rdr["LastUseTime"]);
            model.ArrGeneralID = ConverToStr(rdr["ArrGeneralID"]);
            model.QuoteTimes = ConvertToInt(rdr["QuoteTimes"]);
            rdr.Dispose();
            return model;
        }
    }
}