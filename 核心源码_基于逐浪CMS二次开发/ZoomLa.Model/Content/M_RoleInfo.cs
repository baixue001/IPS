using System;using System.Data.Common;
using System.Data.SqlClient;
using System.Data;


namespace ZoomLa.Model
{
    public class M_RoleInfo:M_Base
    {
        #region 字段定义
        public int RoleID { get; set; }
        public string RoleName = "";
        public string Description = "";
        /// <summary>
        /// ??
        /// </summary>
        public int NodeID { get; set; }
        /// <summary>
        /// 权限
        /// </summary>
        public string Auth = "";
        /// <summary>
        /// 添加人ID
        /// </summary>
        public int CAdminId = 0;
        public DateTime CDate { get; set; }
        #endregion
        #region 构造函数
        public M_RoleInfo()
        {
            this.CDate = DateTime.Now;
        }
        #endregion

        public override string PK { get { return "RoleID"; } }
        public override string TbName { get { return "ZL_Role"; } }
        public override string[,] FieldList()
        {
            string[,] Tablelist = {
                                  {"RoleID","Int","4"},
                                  {"RoleName","NVarChar","50"},
                                  {"Description","NVarChar","255"},
                                  {"NodeID","Int","4"},
                                  {"Auth","VarChar","8000"},
                                  {"CAdminId","Int","4"},
                                  {"CDate","DateTime","8"},
                                 };
            return Tablelist;
        }
        public override SqlParameter[] GetParameters()
        {
            M_RoleInfo model = this;
            SqlParameter[] sp = GetSP();
            sp[0].Value = model.RoleID;
            sp[1].Value = model.RoleName;
            sp[2].Value = model.Description;
            sp[3].Value = model.NodeID;
            sp[4].Value = model.Auth;
            sp[5].Value = model.CAdminId;
            sp[6].Value = model.CDate;
            return sp;
        }
        public  M_RoleInfo GetModelFromReader(DbDataReader rdr)
        {
            M_RoleInfo model = new M_RoleInfo();
            model.RoleID = Convert.ToInt32(rdr["RoleID"]);
            model.RoleName = ConverToStr(rdr["RoleName"]);
            model.Description = ConverToStr(rdr["Description"]);
            model.NodeID = ConvertToInt(rdr["NodeID"]);
            model.Auth = ConverToStr(rdr["Auth"]);
            model.CAdminId = ConvertToInt(rdr["CAdminId"]);
            model.CDate = ConvertToDate(rdr["CDate"]);
            rdr.Dispose();
            return model;
        }
    }
}