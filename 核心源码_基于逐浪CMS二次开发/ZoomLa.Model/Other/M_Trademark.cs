
namespace ZoomLa.Model
{
    using System;using System.Data.Common;
    using System.Data;
    using System.Data.SqlClient;

    public class M_Trademark:M_Base
    {
        public M_Trademark()
        {
            Istop = 0;
            Isuse = 1;
        }
        public int id { get; set; }
        public DateTime Addtime { get; set; }
        /// <summary>
        /// 商标名
        /// </summary>
        public string Trname { get; set; }
        /// <summary>
        /// 所属厂商
        /// </summary>
        public int Producer { get; set; }
        /// <summary>
        /// 商标类别
        /// </summary>
        public string TrClass { get; set; }
        /// <summary>
        /// 是否使用
        /// </summary>
        public int Isuse { get; set; }
        /// <summary>
        /// 是否品牌
        /// </summary>
        public int Istop { get; set; }
        /// <summary>
        /// 是否推荐
        /// </summary>
        public int Isbest { get; set; }
        /// <summary>
        /// 品牌照片
        /// </summary>
        public string TrPhoto { get; set; }
        /// <summary>
        /// 品牌简介
        /// </summary>
        public string TrContent { get; set; }
        public override string TbName { get { return "ZL_Trademark"; } }
        public override string[,] FieldList()
        {
            string[,] Tablelist = {
                                  {"id","Int","4"},
                                  {"Trname","NVarChar","255"},
                                  {"Producer","Int","4"},
                                  {"TrClass","NVarChar","50"}, 
                                  {"Isuse","Int","4"},
                                  {"Istop","Int","4"},
                                  {"Isbest","Int","4"},
                                  {"TrPhoto","NVarChar","500"},
                                  {"TrContent","NText","5000"}, 
                                  {"Addtime","DateTime","8"}
                                 };
            return Tablelist;
        }
        public override SqlParameter[] GetParameters()
        {
            var model = this;
            SqlParameter[] sp = GetSP();
            if (model.Addtime <= DateTime.MinValue) { model.Addtime = DateTime.Now; }
            sp[0].Value = model.id;
            sp[1].Value = model.Trname;
            sp[2].Value = model.Producer;
            sp[3].Value = model.TrClass;
            sp[4].Value = model.Isuse;
            sp[5].Value = model.Istop;
            sp[6].Value = model.Isbest;
            sp[7].Value = model.TrPhoto;
            sp[8].Value = model.TrContent;
            sp[9].Value = model.Addtime;
            return sp;
        }

        public M_Trademark GetModelFromReader(DbDataReader rdr)
        {
            M_Trademark model = new M_Trademark();
            model.id = Convert.ToInt32(rdr["id"]);
            model.Trname = ConverToStr(rdr["Trname"]);
            model.Producer = ConvertToInt(rdr["Producer"]);
            model.TrClass = ConverToStr(rdr["TrClass"]);
            model.Isuse = ConvertToInt(rdr["Isuse"]);
            model.Istop = ConvertToInt(rdr["Istop"]);
            model.Isbest = ConvertToInt(rdr["Isbest"]);
            model.TrPhoto = ConverToStr(rdr["TrPhoto"]);
            model.TrContent = ConverToStr(rdr["TrContent"]);
            model.Addtime = ConvertToDate(rdr["Addtime"]);
            rdr.Close();
            return model;
        }
    }
}