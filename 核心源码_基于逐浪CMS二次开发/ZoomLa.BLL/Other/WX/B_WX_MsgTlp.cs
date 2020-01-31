using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using ZoomLa.Model.Other;
using ZoomLa.SQLDAL;

namespace ZoomLa.BLL.Other
{
    public class B_WX_MsgTlp
    {
        private M_WX_MsgTlp initMod = new M_WX_MsgTlp();
        public string TbName = "", PK = "";
        public B_WX_MsgTlp()
        {
            TbName = initMod.TbName;
            PK = initMod.PK;
        }
        public DataTable Sel()
        {
            return DBCenter.Sel(TbName, "", PK + " DESC");
        }
        public M_WX_MsgTlp SelReturnModel(int ID)
        {
            using (DbDataReader reader = DBCenter.SelReturnReader(TbName, PK, ID))
            {
                if (reader.Read())
                {
                    return initMod.GetModelFromReader(reader);
                }
                else
                {
                    return null;
                }
            }
        }
        public int Insert(M_WX_MsgTlp model)
        {
            return DBCenter.Insert(model);
        }
        public bool UpdateByID(M_WX_MsgTlp model)
        {
            return DBCenter.UpdateByID(model, model.ID);
        }
        public bool Del(int ID)
        {
            return DBCenter.Del(TbName, PK, ID);
        }
        public string GetMsgType(string msgType)
        {
            switch (msgType)
            {
                case "text":
                    return "文本";
                case "image":
                    return "图文";
                case "multi":
                    return "多图文";
                default:
                    return msgType;
            }
        }
    }
}
