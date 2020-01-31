using System;using System.Data.Common;
using System.Collections.Generic;

using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace ZoomLa.Model.Device
{
    public class M_DeviceM_Info : M_Base
    {

        public int ID { get; set; }
        /// <summary>
        /// 监控别名
        /// </summary>
        public string Alias { get; set; }
        /// <summary>
        /// 监控域名或IP
        /// </summary>
        public string Domain { get; set; }
        /// <summary>
        /// 暂不使用
        /// </summary>
        public string IP { get; set; }
        /// <summary>
        /// 监控用户名
        /// </summary>
        public string M_User { get; set; }
        /// <summary>
        /// 监控密码
        /// </summary>
        public string M_Pwd { get; set; }
        /// <summary>
        /// 监控通道号,如果有多个摄像头时,则需要设置,否则应该为1
        /// </summary>
        public int M_Channel { get; set; }
        /// <summary>
        /// 监控端口
        /// </summary>
        public int M_Port { get; set; }
        public int AdminID { get; set; }
        public int ZStatus { get; set; }
        public DateTime CDate { get; set; }
        public string Remind { get; set; }
        public M_DeviceM_Info()
        {
            M_Channel = 1;
            M_Port = 80;
            CDate = DateTime.Now;
            ZStatus = 1;
        }
        public override string TbName { get { return "ZL_DeviceM_Info"; } }
        public override string PK { get { return "ID"; } }

        public override string[,] FieldList()
        {
            string[,] Tablelist = {
                                {"ID","Int","4"},
                                {"Alias","NVarChar","100"},
                                {"Domain","NVarChar","100"},
                                {"IP","NVarChar","100"},
                                {"M_User","NVarChar","100"},
                                {"M_Pwd","NVarChar","100"},
                                {"M_Channel","Int","4"},
                                {"AdminID","Int","4"},
                                {"ZStatus","Int","4"},
                                {"CDate","DateTime","8"},
                                {"Remind","NVarChar","500"},
                                {"M_Port","Int","4"}
        };
            return Tablelist;
        }

        public override SqlParameter[] GetParameters()
        {
            M_DeviceM_Info model = this;
            SqlParameter[] sp = GetSP();
            sp[0].Value = model.ID;
            sp[1].Value = model.Alias;
            sp[2].Value = model.Domain;
            sp[3].Value = model.IP;
            sp[4].Value = model.M_User;
            sp[5].Value = model.M_Pwd;
            sp[6].Value = model.M_Channel;
            sp[7].Value = model.AdminID;
            sp[8].Value = model.ZStatus;
            sp[9].Value = model.CDate;
            sp[10].Value = model.Remind;
            sp[11].Value = model.M_Port;
            return sp;
        }
        public M_DeviceM_Info GetModelFromReader(DbDataReader rdr)
        {
            M_DeviceM_Info model = new M_DeviceM_Info();
            model.ID = Convert.ToInt32(rdr["ID"]);
            model.Alias = ConverToStr(rdr["Alias"]);
            model.Domain = ConverToStr(rdr["Domain"]);
            model.IP = ConverToStr(rdr["IP"]);
            model.M_User = ConverToStr(rdr["M_User"]);
            model.M_Pwd = ConverToStr(rdr["M_Pwd"]);
            model.M_Channel = ConvertToInt(rdr["M_Channel"]);
            model.AdminID = ConvertToInt(rdr["AdminID"]);
            model.ZStatus = ConvertToInt(rdr["ZStatus"]);
            model.CDate = ConvertToDate(rdr["CDate"]);
            model.Remind = ConverToStr(rdr["Remind"]);
            model.M_Port = ConvertToInt(rdr["M_Port"]);
            rdr.Close();
            return model;
        }
    }
}
