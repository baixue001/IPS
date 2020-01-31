using System;using System.Data.Common;
using System.Collections.Generic;

using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace ZoomLa.Model.Common
{
    //文件入库,用于一些对安全性要求较高的文件
    public class M_Com_File : M_Base
    {
        public M_Com_File()
        {
            FileGuid = System.Guid.NewGuid().ToString();
        }
        //FileContent不在列表中返回,根据需要再从数据库中获取
        public int ID { get; set; }
        /// <summary>
        /// 文件Gu
        /// </summary>
        public string FileGuid { get; set; }
        /// <summary>
        /// 文件展示名称
        /// </summary>
        public string FileName { get; set; }
        /// <summary>
        /// 文件类型,有需要则备注
        /// </summary>
        public string FileType { get; set; }
        /// <summary>
        /// 所属模块 oa|cloud
        /// </summary>
        public string Source { get; set; }
        public int UserID { get; set; }
        public int AdminID { get; set; }
        public DateTime CDate { get; set; }
        /// <summary>
        /// 文件修改时间
        /// </summary>
        public DateTime UpdateTime { get; set; }
        public string Remind { get; set; }
        public byte[] FileContent { get; set; }

        public override string TbName { get { return "ZL_Com_File"; } }
        public override string PK { get { return "ID"; } }

        public override string[,] FieldList()
        {
            string[,] Tablelist = {
                                {"ID","Int","4"},
                                {"FileGuid","NVarChar","200"},
                                {"FileName","NVarChar","500"},
                                {"FileType","NVarChar","500"},
                                {"Source","NVarChar","50"},
                                {"UserID","Int","4"},
                                {"AdminID","Int","4"},
                                {"CDate","DateTime","8"},
                                {"UpdateTime","DateTime","8"},
                                {"Remind","NVarChar","500"},
                                {"FileContent","Image","5000000"}
        };
            return Tablelist;
        }

        public override SqlParameter[] GetParameters()
        {
            M_Com_File model = this;
            if (model.CDate <= DateTime.MinValue) { model.CDate = DateTime.Now; }
            if (model.UpdateTime <= DateTime.MinValue) { model.UpdateTime = DateTime.Now; }
            if (string.IsNullOrEmpty(model.FileGuid)) { model.FileGuid = Guid.NewGuid().ToString(); }
            SqlParameter[] sp = GetSP();
            sp[0].Value = model.ID;
            sp[1].Value = model.FileGuid;
            sp[2].Value = model.FileName;
            sp[3].Value = model.FileType;
            sp[4].Value = model.Source;
            sp[5].Value = model.UserID;
            sp[6].Value = model.AdminID;
            sp[7].Value = model.CDate;
            sp[8].Value = model.UpdateTime;
            sp[9].Value = model.Remind;
            sp[10].Value = model.FileContent;
            return sp;
        }
        public M_Com_File GetModelFromReader(DbDataReader rdr)
        {
            M_Com_File model = new M_Com_File();
            model.ID = Convert.ToInt32(rdr["ID"]);
            model.FileGuid = ConverToStr(rdr["FileGuid"]);
            model.FileName = ConverToStr(rdr["FileName"]);
            model.FileType = ConverToStr(rdr["FileType"]);
            model.Source = ConverToStr(rdr["Source"]);
            model.UserID = ConvertToInt(rdr["UserID"]);
            model.AdminID = ConvertToInt(rdr["AdminID"]);
            model.CDate = ConvertToDate(rdr["CDate"]);
            model.UpdateTime = ConvertToDate(rdr["UpdateTime"]);
            model.Remind = ConverToStr(rdr["Remind"]);
            model.FileContent = ConverToBytes(rdr["FileContent"]);
            rdr.Close();
            return model;
        }
        private byte[] ConverToBytes(object data)
        {
            if (data == null || data == DBNull.Value) { return null; }
            else { return (byte[])data; }
        }
    }
}
