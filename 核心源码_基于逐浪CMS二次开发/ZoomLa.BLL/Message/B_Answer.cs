using System;
using System.Data;
using ZoomLa.Model;
using ZoomLa.Common;
using ZoomLa.SQLDAL;
using System.Data.SqlClient;
using System.Collections.Generic;
using System.Collections;
using ZoomLa.SQLDAL.SQL;
using System.Data.Common;

namespace ZoomLa.BLL
{
    public class B_Answer
    {
        private string strTableName,PK;
        private M_Answer initMod=new M_Answer();
        public B_Answer()
        {
            strTableName = initMod.TbName;
            PK = initMod.PK;

        }
        public M_Answer SelReturnModel(int ID)
        {
            using (DbDataReader reader = DBCenter.SelReturnReader(strTableName, PK, ID))
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
        public DataTable Sel()
        {
            return DBCenter.Sel(strTableName);
        }
        public PageSetting SelPage(int cpage, int psize)
        {
            PageSetting setting = PageSetting.Single(cpage, psize, strTableName, PK, "");
            DBCenter.SelPage(setting);
            return setting;
        }
        public bool UpdateByID(M_Answer model)
        {
            return DBCenter.UpdateByID(model,model.AnswerID);
        }
        public bool Del(int ID)
        {
            return DBCenter.Del(strTableName,PK,ID);
        }
        public int insert(M_Answer model)
        {
            return DBCenter.Insert(model);
        }
        /// <summary>
        /// 通过问题的ID找答案
        /// </summary>
        public DataTable GetAnswersByQid(int Qid)
        {
            return DBCenter.Sel(strTableName,"QID="+Qid);
        }
        public bool DelByUid(int uid,int sid)
        {
            return DBCenter.DelByWhere(strTableName,"UserID="+ uid+ " AND Surveyid="+sid);
        }
    }
}
