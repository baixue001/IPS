using System;using System.Data.Common;
using System.Data.SqlClient;
using System.Data;
namespace ZoomLa.Model
{
    [Serializable]
    public class M_MailIdiograph:M_Base
    {
        #region 定义字段
        public int ID { get; set; }
        /// <summary>
        /// 签名名称
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 添加时间
        /// </summary>
        public DateTime CDate { get; set; }
        /// <summary>
        /// 状态
        /// </summary>
        public int State { get; set; }
        /// <summary>
        /// 签名内容
        /// </summary>
        public string Context { get; set; }
        public int ZType { get; set; }
        public int UserID { get; set; }
        public int AdminID { get; set; }

        #endregion


        public override string TbName { get { return "ZL_Mail_Idiograph"; } }
        public override string[,] FieldList()
        {
            string[,] Tablelist = {
                                  {"ID","Int","4"},
                                  {"Name","NVarChar","50"},
                                  {"CDate","DateTime","8"},
                                  {"State","Int","4"},
                                  {"Context","NVarChar","4000"},
                                  {"ZType","Int","4"},
                                  {"UserID","Int","4"},
                                  {"AdminID","Int","4"}
                                 };
            return Tablelist;
        }
        public M_MailIdiograph()
        {
            State = 1;
            ZType = 0;
        }

        public override SqlParameter[] GetParameters()
        {
            M_MailIdiograph model = this;
            if (model.CDate <= DateTime.MinValue) { model.CDate = DateTime.Now; }
            SqlParameter[] sp = GetSP();
            sp[0].Value = model.ID;
            sp[1].Value = model.Name;
            sp[2].Value = model.CDate;
            sp[3].Value = model.State;
            sp[4].Value = model.Context;
            sp[5].Value = model.ZType;
            sp[6].Value = model.UserID;
            sp[7].Value = model.AdminID;
            return sp;
        }
        public M_MailIdiograph GetModelFromReader(DbDataReader rdr)
        {
            M_MailIdiograph model = new M_MailIdiograph();
            model.ID = Convert.ToInt32(rdr["ID"]);
            model.Name = ConverToStr(rdr["Name"]);
            model.CDate = ConvertToDate(rdr["CDate"]);
            model.State = ConvertToInt(rdr["State"]);
            model.Context = ConverToStr(rdr["Context"]);
            model.ZType = ConvertToInt(rdr["ZType"]);
            model.UserID = ConvertToInt(rdr["UserID"]);
            model.AdminID = ConvertToInt(rdr["AdminID"]);
            rdr.Close();
            rdr.Dispose();
            return model;
        }
    }
}