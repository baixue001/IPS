using System;using System.Data.Common;
using System.Data.SqlClient;
using System.Data;

namespace ZoomLa.Model
{
    public class M_GradeCate : M_Base
    {
        public M_GradeCate()
        {
            this.CateID = 0;
            this.CateName = "";
            this.Remark = "";
            this.GradeAlias = "";
        }
        /// <summary>
        /// 分级选项类别ID
        /// </summary>
        public int CateID { get; set; }
        /// <summary>
        /// 分级选项分类名
        /// </summary>
        public string CateName { get; set; }
        /// <summary>
        /// 分级选项分类备注
        /// </summary>
        public string Remark { get; set; }
        /// <summary>
        /// 分级选项选项别名 例：省份|城市 用"|"隔开
        /// </summary>
        public string GradeAlias { get; set; }
        public override string PK { get { return "CateID"; } }
        public override string TbName { get { return "ZL_GradeCate"; } }
        public override string[,] FieldList()
        {
            string[,] Tablelist = {
                                  {"CateID","Int","4"},
                                  {"CateName","NVarChar","50"},
                                  {"Remark","NVarChar","255"},
                                  {"GradeField","NVarChar","500"}
                                 };
            return Tablelist;
        }
        public override SqlParameter[] GetParameters()
        {
            var model = this;
            SqlParameter[] sp = GetSP();
            sp[0].Value = model.CateID;
            sp[1].Value = model.CateName;
            sp[2].Value = model.Remark;
            sp[3].Value = model.GradeAlias;
            return sp;
        }

        public M_GradeCate GetModelFromReader(DbDataReader rdr)
        {
            M_GradeCate model = new M_GradeCate();
            model.CateID = ConvertToInt(rdr["CateID"]);
            model.CateName = ConverToStr(rdr["CateName"]);
            model.Remark = ConverToStr(rdr["Remark"]);
            model.GradeAlias = ConverToStr(rdr["GradeField"]);
            rdr.Close();
            return model;
        }
    }
}
