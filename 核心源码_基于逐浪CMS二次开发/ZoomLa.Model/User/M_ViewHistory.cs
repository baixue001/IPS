using System;using System.Data.Common;
using System.Data.SqlClient;
using System.Data;


namespace ZoomLa.Model
{
    public class M_ViewHistory : M_Base
    {
        public M_ViewHistory() { ZStatus = 1;ViewCount = 1; }
        public int ID { get; set; }
        /// <summary>
        /// 内容ID
        /// </summary>
        public int InfoId { get; set; }
        /// <summary>
        /// 购买时的收费类型
        /// </summary>
        public string type { get; set; }
        public int UserID { get; set; }
        public DateTime addtime { get; set; }
        /// <summary>
        /// 浏览次数,仅在节点设置为根据次数时生效
        /// </summary>
        public int ViewCount { get; set; }
        /// <summary>
        /// 过期或次数用完后,状态码为99,生效为1
        /// </summary>
        public int ZStatus { get; set; }
        public override string TbName { get { return "ZL_ViewHistory"; } }
        public override string[,] FieldList()
        {
            string[,] Tablelist = {
                                  {"Id","Int","4"},
                                  {"InfoId","Int","4"},
                                  {"type","NVarChar","1000"},
                                  {"UserID","Int","4"},
                                  {"addtime","DateTime","8"},
                                  {"ViewCount","Int","4"},
                                  {"ZStatus","Int","4"}
                                 };
            return Tablelist;
        }
        public override SqlParameter[] GetParameters()
        {
            M_ViewHistory model = this;
            if (model.addtime <= DateTime.MinValue) { model.addtime = DateTime.Now; }
            SqlParameter[] sp = GetSP();
            sp[0].Value = model.ID;
            sp[1].Value = model.InfoId;
            sp[2].Value = model.type;
            sp[3].Value = model.UserID;
            sp[4].Value = model.addtime;
            sp[5].Value = model.ViewCount;
            sp[6].Value = model.ZStatus;
            return sp;
        }
        public M_ViewHistory GetModelFromReader(DbDataReader rdr)
        {
            M_ViewHistory model = new M_ViewHistory();
            model.ID = Convert.ToInt32(rdr["Id"]);
            model.InfoId = ConvertToInt(rdr["InfoId"]);
            model.type = ConverToStr(rdr["type"]);
            model.UserID = Convert.ToInt32(rdr["UserID"]);
            model.addtime = ConvertToDate(rdr["addtime"]);
            model.ViewCount = ConvertToInt(rdr["ViewCount"]);
            model.ZStatus = ConvertToInt(rdr["ZStatus"]);
            rdr.Close();
            return model;
        }
    }
}