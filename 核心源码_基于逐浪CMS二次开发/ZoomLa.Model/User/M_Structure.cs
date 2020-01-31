using System;using System.Data.Common;
using System.Collections.Generic;
using System.Data;

using System.Data.SqlClient;

namespace ZoomLa.Model
{
    public class M_Structure : M_Base
    {
        public int ID { get; set; }
        public int Group { get; set; }
        /// <summary>
        /// 组：0管理员组,1会员组，2邮件联系组,暂未使用
        /// </summary>
        public enum GroupType { Admin = 0, User = 1, Email = 2 }
        public int ParentID { get; set; }
        /// <summary>
        /// 父节点树,用于方便查询
        /// </summary>
        public string ParentTree { get; set; }
        public string Name { get; set; }
        public DateTime AddTime { get; set; }
        public int Opens { get; set; }
        public int Status { get; set; }
        /// <summary>
        /// 创建者ID(管理员)
        /// </summary>
        public int UserID { get; set; }
        public string Remind { get; set; }
        /// <summary>
        /// 部门成员IDS
        /// </summary>
        public string UserIDS { get; set; }
        /// <summary>
        /// 部门管理员IDS
        /// </summary>
        public string ManagerIDS { get; set; }
        /// <summary>
        /// 部门成员所拥有的权限
        /// </summary>
        public string Auth = "";
        public override string TbName { get { return "ZL_Structure"; } }
        public M_Structure()
        {
            this.Opens = 1;
            this.Status = 1;
            this.Group = (int)M_Structure.GroupType.User;//扩展的话，根据传入的GroupType,分不同的组
            this.AddTime = DateTime.Now;
            this.ParentTree = "";
        }
        public override string[,] FieldList()
        {
            string[,] Tablelist = {
                                  {"ID","Int","4"},
                                  {"Group","Int","4"},
                                  {"ParentID","Int","4"},
                                  {"Name","NVarChar","255"}, 
                                  {"AddTime","DateTime","8"},
                                  {"Opens","Int","4"},
                                  {"Status","Int","4"},
                                  {"UserID","Int","4"},
                                  {"Remind","NVarChar","255"},
                                  {"UserIDS","VarChar","8000"},
                                  {"ManagerIDS","VarChar","8000"},
                                  {"ParentTree","VarChar","8000"},
                                  {"Auth","VarChar","8000"}
                                 };
            return Tablelist;
        }
        public override SqlParameter[] GetParameters()
        {
            M_Structure model = this;
            if (model.AddTime <= DateTime.MinValue) { model.AddTime = DateTime.Now; }
            SqlParameter[] sp = GetSP();
            sp[0].Value = model.ID;
            sp[1].Value = model.Group;
            sp[2].Value = model.ParentID;
            sp[3].Value = model.Name;
            sp[4].Value = model.AddTime;
            sp[5].Value = model.Opens;
            sp[6].Value = model.Status;
            sp[7].Value = model.UserID;
            sp[8].Value = model.Remind;
            sp[9].Value = model.UserIDS;
            sp[10].Value = model.ManagerIDS;
            sp[11].Value = model.ParentTree;
            sp[12].Value = model.Auth;
            return sp;
        }
        public M_Structure GetModelFromReader(DbDataReader rdr)
        {
            M_Structure model = new M_Structure();
            model.ID = Convert.ToInt32(rdr["ID"]);
            model.Group = ConvertToInt(rdr["Group"]);
            model.ParentID = ConvertToInt(rdr["ParentID"]);
            model.Name = ConverToStr(rdr["Name"]);
            model.AddTime = ConvertToDate(rdr["AddTime"]);
            model.Opens = ConvertToInt(rdr["Opens"]);
            model.Status = ConvertToInt(rdr["Status"]);
            model.UserID = ConvertToInt(rdr["UserID"]);
            model.Remind = ConverToStr(rdr["Remind"]);
            model.UserIDS = ConverToStr(rdr["UserIDS"]);
            model.ManagerIDS = ConverToStr(rdr["ManagerIDS"]);
            model.ParentTree = ConverToStr(rdr["ParentTree"]);
            model.Auth = ConverToStr(rdr["Auth"]);
            rdr.Close();
            return model;
        }
    }
}
