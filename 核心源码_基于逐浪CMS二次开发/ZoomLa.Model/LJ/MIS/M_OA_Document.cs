﻿using System;using System.Data.Common;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace ZoomLa.Model
{
    [Serializable]
    //ZL_OA_Document
    public class M_OA_Document:M_Base
    {
        public int ID { get; set; }
        /// <summary>
        /// 密级,紧急程序,重要度
        /// </summary>
        public string Secret { get; set; }
        /// <summary>
        /// 紧急程度
        /// </summary>
        public string Urgency { get; set; }
        /// <summary>
        /// 重要程度
        /// </summary>
        public string Importance { get; set; }
        /// <summary>
        /// 原DocType,指向附表数据
        /// </summary>
        public int ItemID { get; set; }
        /// <summary>
        /// 用于模版ID
        /// </summary>
        public int Type { get; set;  }
        /// <summary>
        /// 内容ID(disuse)
        /// </summary>
        public string Content { get; set; }
        public string Title { get; set; }
        /// <summary>
        /// 关键词disuse
        /// </summary>
        public string Keywords { get; set; }
        /// <summary>
        /// 用户所属组织结构,现只取第一个(用户可属于多个)
        /// </summary>
        public string Branch { get; set; }
        public int UserID { get; set; }
        /// <summary>
        /// 创建时间(用户不可修改)
        /// </summary>
        public DateTime CreateTime { get; set; }
        /// <summary>
        /// -99:回收站,-80:草稿,-1拒绝,98:Free|AdminFree流程,等待指定一下步骤或归档,99:完成,2:回退至发起人(普通的回退不会成为该状态)
        /// </summary>
        public int Status
        {
            get;
            set;
        }
        public int ProID { get; set; }
        /// <summary>
        /// 正在执行的步骤,文刚发出为0:起草(可被撤回)(如被任意一主办人处理过则为1),其余为当前步骤号
        /// AdminFree中的步骤号考虑到分支,不一定与源步骤号对应
        /// </summary>
        public int CurStepNum { get; set; }
        /// <summary>
        /// 公共附件地址,多个用,隔开(所有审批人与CC人员可看)
        /// </summary>
        public string PublicAttach { get; set; }
        /// <summary>
        /// 改用于存Word文件名,为安全考虑仅存文件名
        /// </summary>
        public string PrivateAttach { get; set; }
        /// <summary>
        /// 签章 格式为: signID:x|y  (只有发起人的存这)
        /// </summary>
        public string SignID { get; set; }
        /// <summary>
        /// 该文档所绑定的流程类型
        /// </summary>
        public int ProType { get; set; }
        /// <summary>
        /// 文档编号
        /// </summary>
        public string No { get; set; }
        /// <summary>
        /// 发文时间(用户可修改)
        /// </summary>
        public DateTime SendDate { get; set; }
        /// <summary>
        /// 用户真实名称,如无则为用户名
        /// </summary>
        public string UserName { get; set; }
        /// <summary>
        /// 拟办意见
        /// </summary>
        public string Remind { get; set; }
        /// <summary>
        /// 系统备注,预留
        /// </summary>
        public string SysRemind { get; set; }

        /// <summary>
        /// 用于存表名
        /// </summary>
        public string TableName { get; set; }
        //------------------------------------------
        /// <summary>
        /// 如果被拒绝或完成，返回true
        /// </summary>
        public bool IsComplete
        {
            get
            {
                return (Status == 99 || Status == -1 || Status == 105);
            }
        }
        /// <summary>
        /// 是否自由流程,True为是
        /// </summary>
        public bool IsFreePro
        {
            get
            {
                switch (ProType)
                {
                    case (int)M_MisProcedure.ProTypes.Free:
                    case (int)M_MisProcedure.ProTypes.AdminFree:
                        return true;
                    default:
                        return false;
                }
            }
        }
        //------------------------------------------
        public override string TbName { get { return "ZL_OA_Document"; } }
        public override string[,] FieldList()
        {
            string[,] Tablelist = {
                                    {"ID","Int","4"}, 
                                    {"Secret","NVarChar","200"},            
                                    {"Urgency","NVarChar","200"},            
                                    {"Importance","NVarChar","200"},            
                                    {"Type","Int","4"},            
                                    {"Cont","NText","20000"},            
                                    {"Title","NVarChar","200"},            
                                    {"Keywords","NVarChar","50"},            
                                    {"Branch","NVarChar","50"},            
                                    {"UserID","Int","4"},            
                                    {"CreateTime","DateTime","8"},            
                                    {"Status","Int","4"},
                                    {"ProID","Int","4"},
                                    {"CurStepNum","Int","4"},
                                    {"PublicAttach","NVarChar","300"},
                                    {"PrivateAttach","NVarChar","300"},
                                    {"SignID","NVarChar","50"},
                                    {"DocType","Int","4"},
                                    {"ProType","Int","4"},
                                    {"No","NVarChar","200"},
                                    {"SendDate","DateTime","8"},
                                    {"UserName","NVarChar","100"},
                                    {"TableName","NVarChar","100"},
                                    {"Remind","NVarChar","4000"},
                                    {"SysRemind","NVarChar","4000"}

        };
            return Tablelist;
        }
        public override SqlParameter[] GetParameters()
        {
            var model = this;
            if (CreateTime <= DateTime.MinValue) CreateTime = DateTime.Now;
            if (SendDate <= DateTime.MinValue) SendDate = DateTime.Now;
            SqlParameter[] sp = GetSP();
            sp[0].Value = model.ID;
            sp[1].Value = model.Secret;
            sp[2].Value = model.Urgency;
            sp[3].Value = model.Importance;
            sp[4].Value = model.Type;
            sp[5].Value = model.Content;
            sp[6].Value = model.Title;
            sp[7].Value = model.Keywords;
            sp[8].Value = model.Branch;
            sp[9].Value = model.UserID;
            sp[10].Value = model.CreateTime;
            sp[11].Value = model.Status;
            sp[12].Value = model.ProID;
            sp[13].Value = model.CurStepNum;
            sp[14].Value = model.PublicAttach;
            sp[15].Value = model.PrivateAttach;
            sp[16].Value = model.SignID;
            sp[17].Value = model.ItemID;
            sp[18].Value = model.ProType;
            sp[19].Value = model.No;
            sp[20].Value = model.SendDate;
            sp[21].Value = model.UserName;
            sp[22].Value = model.TableName;
            sp[23].Value = model.Remind;
            sp[24].Value = model.SysRemind;
            return sp;
        }
        public M_OA_Document GetModelFromReader(DbDataReader rdr)
        {
            M_OA_Document model = new M_OA_Document();
            model.ID = Convert.ToInt32(rdr["ID"]);
            model.Secret = ConverToStr(rdr["Secret"]);
            model.Urgency = ConverToStr(rdr["Urgency"]);
            model.Importance = ConverToStr(rdr["Importance"]);
            model.Type = ConvertToInt(rdr["Type"]);
            model.Content = ConverToStr(rdr["Cont"]);
            model.Title = ConverToStr(rdr["Title"]);
            model.Keywords = ConverToStr(rdr["Keywords"]);
            model.Branch = ConverToStr(rdr["Branch"]);
            model.UserID = ConvertToInt(rdr["UserID"]);
            model.CreateTime = ConvertToDate(rdr["CreateTime"]);
            model.Status = Convert.ToInt32(rdr["Status"]);
            model.ProID = Convert.ToInt32(rdr["ProID"]);
            model.CurStepNum = ConvertToInt(rdr["CurStepNum"]);
            model.PublicAttach = ConverToStr(rdr["PublicAttach"]);
            model.PrivateAttach = ConverToStr(rdr["PrivateAttach"]);
            model.SignID = ConverToStr(rdr["SignID"]);
            model.ItemID = ConvertToInt(rdr["DocType"]);
            model.ProType = ConvertToInt(rdr["ProType"]);
            model.No = ConverToStr(rdr["No"]);
            model.SendDate = ConvertToDate(rdr["SendDate"]);
            model.UserName = ConverToStr(rdr["UserName"]);
            model.TableName = ConverToStr(rdr["TableName"]);
            model.Remind = ConverToStr(rdr["Remind"]);
            model.SysRemind = ConverToStr(rdr["SysRemind"]);
            rdr.Close();
            return model;
        }
    }
}