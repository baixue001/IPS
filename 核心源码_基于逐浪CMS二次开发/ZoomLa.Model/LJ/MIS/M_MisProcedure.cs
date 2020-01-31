using System;using System.Data.Common;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using Newtonsoft.Json;

namespace ZoomLa.Model
{
    public class M_MisProcedure:M_Base
    {
        public M_MisProcedure()
        {
            FirstStep = new M_MisProLevel();
            AllowFlow = 1;
            AllowAttach=1;
            NodeID = 0;//绑定节点,暂无用
        }
        string _sponsor,_manager;
        public int ID { get; set; }
        public string ProcedureName { get; set; }
        /// <summary>
        /// 流程类型,对应公文等ProType,1:自由流程,2:公文流程,3:限定流程
        /// </summary>
        public int TypeID { get; set; }
        public int ClassID { get; set; }
        public string ModelID { get; set; }
        public int CommType { get; set; }
        /// <summary>
        /// 是否归档 1:是,0否(现不给配置,默认均归档)
        /// </summary>
        public int AllowAttach { get; set; }
        public int AllowFlow { get; set; }
        public string Remind { get; set; }
        public int Status { get; set; }
        /// <summary>
        /// 现用于用户角色,为空则所有人均可使用
        /// </summary>
        public string Sponsor 
        {
            get { _sponsor = string.IsNullOrEmpty(_sponsor) ? "" : "," + (_sponsor.Trim(',')) + ","; return _sponsor; }
            set { _sponsor = value; } 
        }
        /// <summary>
        /// 流程管理员IDS
        /// </summary>
        public string Manager 
        {
            get { _manager = string.IsNullOrEmpty(_manager) ? "" : "," + (_manager.Trim(',')) + ","; return _manager; }
            set { _manager = value; } 
        }
        /// <summary>
        /// Disuse,已用于项目名称
        /// </summary>
        public string SponsorGroup
        {
            get;
            set;
        }
        /// <summary>
        /// 绑定节点ID
        /// </summary>
        public int NodeID { get; set; }
        /// <summary>
        /// 表单类型1:模型表单,2:Word表单,3:Excel表单
        /// </summary>
        public int FormType { get; set; }
        /// <summary>
        /// 根据类型的不同存表单ID,内容,或路径信息
        /// </summary>
        public string FormInfo { get; set; }
        /// <summary>
        /// 流程自定义模板名
        /// </summary>
        public string FlowTlp { get; set; }
        /// <summary>
        /// 打印自定义模板名
        /// </summary>
        public string PrintTlp { get; set; }
        //-------------------------起草的权限信息等
        //流程权限
        public string DocAuth { get; set; }
        /// <summary>
        /// 起草步骤的权限[disuse],起草步骤的权限,应该是流程中第一步的权限
        /// </summary>
        public M_MisProLevel FirstStep { get; set; }

        public override string TbName { get { return "ZL_MisProcedure"; } }
        public override string[,] FieldList()
        {
            string[,] Tablelist = {
                        {"ID","Int","4"},
        	            {"ProcedureName","NVarChar","50"},            
                        {"TypeID","Int","4"},            
                        {"ClassID","Int","4"},            
                        {"ModelID","VarChar","50"},            
                        {"CommType","Int","4"},            
                        {"AllowAttach","Int","4"},            
                        {"AllowFlow","Int","4"},            
                        {"Remind","NVarChar","200"},            
                        {"Status","Int","4"},
                        {"Sponsor","NVarChar","300"},
                        {"NodeID","Int","4"},
                        {"SponsorGroup","NVarChar","300"},
                        {"Manager","Text","20000"},
                        {"FormType","Int","4"},
                        {"FormInfo","NText","10000"},
                        {"DocAuth","VarChar","500"},
                        {"FlowTlp","NVarChar","200"},
                        {"PrintTlp","NVarChar","200"},
                        {"FirstStep","NText","20000"}
        };
            return Tablelist;
        }
        public override SqlParameter[] GetParameters()
        {
            M_MisProcedure model = this;
            SqlParameter[] sp = GetSP();
            sp[0].Value = model.ID;
            sp[1].Value = model.ProcedureName;
            sp[2].Value = model.TypeID;
            sp[3].Value = model.ClassID;
            sp[4].Value = model.ModelID;
            sp[5].Value = model.CommType;
            sp[6].Value = model.AllowAttach;
            sp[7].Value = model.AllowFlow;
            sp[8].Value = model.Remind;
            sp[9].Value = model.Status;
            sp[10].Value = model.Sponsor;
            sp[11].Value = model.NodeID;
            sp[12].Value = model.SponsorGroup;
            sp[13].Value = model.Manager;
            sp[14].Value = model.FormType;
            sp[15].Value = model.FormInfo;
            sp[16].Value = model.DocAuth;
            sp[17].Value = model.FlowTlp;
            sp[18].Value = model.PrintTlp;
            sp[19].Value = JsonConvert.SerializeObject(model.FirstStep);
            return sp;
        }
        public M_MisProcedure GetModelFromReader(DbDataReader rdr)
        {
            M_MisProcedure model = new M_MisProcedure();
            model.ID = Convert.ToInt32(rdr["ID"]);
            model.ProcedureName = ConverToStr(rdr["ProcedureName"]);
            model.TypeID = ConvertToInt(rdr["TypeID"]);
            model.ClassID = ConvertToInt(rdr["ClassID"]);
            model.ModelID = ConverToStr(rdr["ModelID"]);
            model.CommType = ConvertToInt(rdr["CommType"]);
            model.AllowAttach = ConvertToInt(rdr["AllowAttach"]);
            model.AllowFlow = ConvertToInt(rdr["AllowFlow"]);
            model.Remind = ConverToStr(rdr["Remind"]);
            model.Status = ConvertToInt(rdr["Status"]);
            model.Sponsor = ConverToStr(rdr["Sponsor"]);
            model.NodeID = ConvertToInt(rdr["NodeID"]);
            model.SponsorGroup = ConverToStr(rdr["SponsorGroup"]);
            model.Manager = ConverToStr(rdr["Manager"]);
            model.FormType = ConvertToInt(rdr["FormType"]);
            model.FormInfo = ConverToStr(rdr["FormInfo"]);
            model.DocAuth = ConverToStr(rdr["DocAuth"]);
            model.FlowTlp = ConverToStr(rdr["FlowTlp"]);
            model.PrintTlp = ConverToStr(rdr["PrintTlp"]);
            try
            {
                model.FirstStep = JsonConvert.DeserializeObject<M_MisProLevel>(ConverToStr(rdr["FirstStep"]));
            }
            catch { }
            if (model.FirstStep == null) { model.FirstStep = new M_MisProLevel(); }
            rdr.Dispose();
            return model;
        }
        //--------------Tools
        public string DirPath = "~/Mis/OA/Tlp/";
        public string GetTypeStr(int typeid = -1)
        {
            if (typeid == -1) typeid = TypeID;
            switch (typeid)
            {
                case 1:
                    return "自由流程";
                case 2:
                    return "公文流程";
                case 3:
                    return "限定流程";
                default:
                    return "未知类型";
            }
        }
        public ProTypes MyProType { get { return (ProTypes)TypeID; } }
        /// <summary>
        ///1:自由流程(请假等),2:管理员定义的自由流程(收发文),3:管理员后台定义(财务等)
        /// </summary>
        public enum ProTypes
        {
            Free = 1, AdminFree = 2, Admin = 3
        };
    }
}
