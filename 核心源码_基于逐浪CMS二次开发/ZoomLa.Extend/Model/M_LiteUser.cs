using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using ZoomLa.Components;
using ZoomLa.Model;

namespace ZoomLa.Extend
{

    /*
     * 1.注册字段中添加新字段
     * 2.model层添加新变量
     * 3.view层中更新语句
     * 4.前端UI使用
     */ 
    public class M_LiteUser:M_Base
    {
        public int userId = 0;
        /// <summary>
        /// 为学员分配的教练
        /// </summary>
        public int siteId = 0;
        public int groupId = 0;
        public string userName = "";
        public string userFace = "";
        public string trueName = "";
        public string honeyName = "";
        //微信openId
        public string openid = "";
        /// <summary>
        /// 推荐人信息
        /// </summary>
        public int puid = 0;
        public string puname = "";
        /// <summary>
        /// 接车地址
        /// </summary>
        public string address = "";
        public string mobile = "";
        /// <summary>
        /// 判断后给予
        /// </summary>
        public string sex = "";
        /// <summary>
        /// 证件类型(中文)
        /// </summary>
        public string cardType = "";
        /// <summary>
        /// 证件号
        /// </summary>
        public string idcard = "";
        public int age = 0;

        public M_LiteUser() { }
        public M_LiteUser GetModelFromReader(DataRow rdr)
        {
            M_LiteUser model = new M_LiteUser();
            model.userId = Convert.ToInt32(rdr["UserID"]);
            model.groupId = ConvertToInt(rdr["groupId"]);
            model.userFace = ConverToStr(rdr["userFace"]);
            model.userName = ConverToStr(rdr["UserName"]);
            model.honeyName = ConverToStr(rdr["HoneyName"]);
            model.trueName = ConverToStr(rdr["trueName"]);
            model.openid = ConverToStr(rdr["openid"]);
            model.puid = ConvertToInt(rdr["puid"]);
            model.puname = ConverToStr(rdr["puname"]);
            model.address = ConverToStr(rdr["address"]);
            model.mobile = ConverToStr(rdr["mobile"]);
            model.sex = ConvertToInt(rdr["userSex"]) == 1 ? "男" : "女";
            model.cardType = ConverToStr(rdr["cardType"]);
            model.idcard = ConverToStr(rdr["idCard"]);
            model.age = ConvertToInt(rdr["age"]);
            model.siteId = ConvertToInt(rdr["siteId"]);
            //------------
            //if (string.IsNullOrEmpty(model.userFace))
            //{
            //    model.userFace = SiteConfig.SiteInfo.SiteUrl + "/Images/UserFace/noface.png";
            //}
            //else if (!model.userFace.Contains("://"))
            //{
            //    model.userFace = SiteConfig.SiteInfo.SiteUrl + model.userFace;
            //}
            return model;
        }
        public string ToJson()
        {
            return JsonConvert.SerializeObject(this, Formatting.None,
                new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore });
        }
        public override string TbName { get { return "ZL_EX_UserView"; } }
        public override string PK { get { return "userId"; } }
        public override SqlParameter[] GetParameters()
        {
            M_LiteUser model = this;
            SqlParameter[] sp = GetSP();
            return sp;
        }

        public override string[,] FieldList()
        {
            throw new NotImplementedException();
        }
    }
}
