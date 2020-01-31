using System;using System.Data.Common;
using System.Collections.Generic;

using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace ZoomLa.Model.Content
{
    public class M_Docs_Item:M_Base
    {
        public int ID { get; set; }
        public string FileName { get; set; }
        /// <summary>
        /// Office文件
        /// </summary>
        public byte[] FileContent { get; set; }
        /// <summary>
        /// 即时转换为PDF,用于预览与签章
        /// </summary>
        public byte[] PdfContent { get; set; }
        /// <summary>
        /// 文件类型，预留
        /// </summary>
        public string FileType { get; set; }
        public int UserID { get; set; }
        public string UserName { get; set; }
        public int ZStatus { get; set; }
        public string Remind { get; set; }
        public DateTime CDate { get; set; }

        public M_Docs_Item()
        {
            ZStatus = 0;
            CDate = DateTime.Now;
        }
        public override string TbName { get { return "ZL_Docs_Item"; } }
        public override string[,] FieldList()
        {
            string[,] Tablelist = {
                        {"FileName","NVarChar","300"},
                        {"FileContent","Image",int.MaxValue+""},
                        {"PdfContent","Image",int.MaxValue+""},
                        {"FileType","NVarChar","50"},
                        {"UserID","Int","4"},
                        {"UserName","NVarChar","50"},
                        {"ZStatus","Int","4"},
                        {"Remind","NVarChar","300"},
                        {"CDate","DateTime","8"}

        };
            return Tablelist;
        }
        public override SqlParameter[] GetParameters()
        {
            M_Docs_Item model = this;
            SqlParameter[] sp = GetSP();
            sp[0].Value = model.FileName;
            sp[1].Value = model.FileContent;
            sp[2].Value = model.PdfContent;
            sp[3].Value = model.FileType;
            sp[4].Value = model.UserID;
            sp[5].Value = model.UserName;
            sp[6].Value = model.ZStatus;
            sp[7].Value = model.Remind;
            sp[8].Value = model.CDate;
            return sp;
        }
        public M_Docs_Item GetModelFromReader(DbDataReader rdr)
        {
            M_Docs_Item model = new M_Docs_Item();
            model.ID = Convert.ToInt32(rdr["ID"]);
            model.FileName = ConverToStr(rdr["FileName"]);
            model.FileContent = ConverToBytes(rdr["FileContent"]);
            model.PdfContent = ConverToBytes(rdr["PdfContent"]);
            model.FileType = ConverToStr(rdr["FileType"]);
            model.UserID = ConvertToInt(rdr["UserID"]);
            model.UserName = ConverToStr(rdr["UserName"]);
            model.ZStatus = ConvertToInt(rdr["ZStatus"]);
            model.Remind = ConverToStr(rdr["Remind"]);
            model.CDate = ConvertToDate(rdr["CDate"]);
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
