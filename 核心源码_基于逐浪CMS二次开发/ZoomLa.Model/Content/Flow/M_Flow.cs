using System;using System.Data.Common;
using System.Collections.Generic;
using System.Text;
using System.Data.SqlClient;
using System.Data;

namespace ZoomLa.Model
{
    [Serializable]
    public class M_Flow : M_Base
    {
        #region 构造方法
        public M_Flow()
        {

        }
        #endregion
        #region 属性、字段
        /// <summary>
        /// 流程编号
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// 流程名称
        /// </summary>
        public string FlowName { get; set; }
        /// <summary>
        /// 流程描述
        /// </summary>
        public string FlowDepict { get; set; }
        #endregion
        public override string TbName { get { return "ZL_Flow"; } }
        public override string[,] FieldList()
        {
            string[,] Tablelist = {
                                  {"id","Int","4"},
                                  {"flowName","NVarChar","50"},
                                  {"flowDepict","NVarChar","50"}
                                 };
            return Tablelist;
        }
        public override SqlParameter[] GetParameters()
        {
            var model = this;
            SqlParameter[] sp = GetSP();
            sp[0].Value = model.Id;
            sp[1].Value = model.FlowName;
            sp[2].Value = model.FlowDepict;
            return sp;
        }
        public M_Flow GetModelFromReader(DbDataReader rdr)
        {
            M_Flow model = new M_Flow();
            model.Id = Convert.ToInt32(rdr["ID"]);
            model.FlowName = ConverToStr(rdr["flowName"]);
            model.FlowDepict = ConverToStr(rdr["flowDepict"]);
            rdr.Close();
            return model;
        }
    }
}
