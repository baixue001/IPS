using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using ZoomLa.Model.Content;
using ZoomLa.SQLDAL;

namespace ZoomLa.BLL.Content
{
    public class B_Docs_Item
    {
        private M_Docs_Item initMod = new M_Docs_Item();
        public string TbName = "", PK = "";
        public B_Docs_Item()
        {
            TbName = initMod.TbName;
            PK = initMod.PK;
        }
        public DataTable Sel()
        {
            return DBCenter.Sel(TbName,"",PK+" DESC");
        }
        public M_Docs_Item SelReturnModel(int ID)
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
        public int Insert(M_Docs_Item model)
        {
            return DBCenter.Insert(model);
        }
        public bool UpdateByID(M_Docs_Item model)
        {
            return DBCenter.UpdateByID(model, model.ID);
        }
        public void Del(string ids)
        {
            if (string.IsNullOrEmpty(ids)) { return; }
            SafeSC.CheckIDSEx(ids);
            DBCenter.DelByIDS(TbName, PK, ids);
        }
    }
}
